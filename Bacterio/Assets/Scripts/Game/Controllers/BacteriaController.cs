using UnityEngine;
using Mirror;

using Bacterio.Common;
using Bacterio.MapObjects;
using Bacterio.NetworkEvents.BacteriaEvents;

namespace Bacterio.Game
{
    public sealed partial class MapController : System.IDisposable
    {
        private sealed partial class BacteriaController : System.IDisposable
        {
            private readonly MapController _mapCtrl = null;
            private readonly ObjectPool<Bacteria> _bacteriaPool = null;
            private readonly GameObject _bacteriaObj = null;
            public readonly Network.NetworkArray<Bacteria> _bacteriaArray = null;


            public BacteriaController(MapController mapController)
            {
                _mapCtrl = mapController;
                _bacteriaObj = GlobalContext.assetBundleProvider.LoadObjectAsset("Bacteria");

                //Create the structures to hold the bacteria
                _bacteriaPool = new ObjectPool<Bacteria>(_bacteriaObj.GetComponent<Bacteria>(), Constants.BACTERIA_POOL_INITIAL_SIZE, Constants.BACTERIA_POOL_GROWTH_AMOUNT, Constants.OUT_OF_RANGE_POSITION, Quaternion.identity, null, 
                                (bacteria) => bacteria.gameObject.SetActive(false), (bacteria) => bacteria.gameObject.SetActive(true));
                _bacteriaArray = new Network.NetworkArray<Bacteria>(Constants.BACTERIA_INITIAL_MAX_AMOUNT, Constants.BACTERIA_GROWTH_AMOUNT);

                //Register to bacteria spawned events. Only on clients. Host doesn't need it since it already does all the logic before it does the spawn / unspawn commands
                if (!Network.NetworkInfo.IsHost)
                {
                    Bacteria.BacteriaSpawned += OnBacteriaSpawnedClient;
                    Bacteria.BacteriaDespawned += OnBacteriaDespawnedClient;

                }
                else //Events that only the host needs
                {
                    NetworkServer.RegisterHandler<RequestBacteriaDamageEvent>(OnRequestBacteriaDamageEvent);
                    NetworkServer.RegisterHandler<RequestBacteriaSyncEvent>(OnRequestBacteriaSyncServer);
                }
            }

            public void Update()
            {
                //fill the sync data
                ref var bacterias = ref _bacteriaArray.GetAll();
                for (int i = 0; i <= _bacteriaArray.LastIndex; i++)
                {
                    if (bacterias[i] == null)
                        continue;

                    RunAi(bacterias[i]);
                }
            }

            public void RunReconnectSync()
            {
                WDebug.Assert(!Network.NetworkInfo.IsHost, "Requesting bacteria structures with host");

                NetworkClient.RegisterHandler<ReplyBacteriaSyncEvent>(OnReplyBacteriaSyncClient);
                NetworkEvents.EmptyEvent.Send<RequestBacteriaSyncEvent>();
            }

            public void Dispose()
            {
                _bacteriaPool.Dispose();
                _bacteriaArray.Dispose();

                NetworkClient.UnregisterHandler<ReplyBacteriaSyncEvent>();
                //Only clients registered
                if (!Network.NetworkInfo.IsHost)
                {
                    Bacteria.BacteriaSpawned -= OnBacteriaSpawnedClient;
                    Bacteria.BacteriaDespawned -= OnBacteriaDespawnedClient;
                }
                else
                {
                    NetworkServer.UnregisterHandler<RequestBacteriaDamageEvent>();
                }
            }

      
            //******************************************************************* Bacteria commands that can be called from any controller
            public void SpawnBacteriaServer(Vector2 position)
            {
                WDebug.Assert(Network.NetworkInfo.IsHost, "Attempted to spawn a bacteria in a client");

                var bacteria = _bacteriaPool.Pop();
                bacteria.transform.localPosition = position;
                bacteria.Configure();

                //Add to network array and spawn it
                _bacteriaArray.Add(bacteria);
                NetworkServer.Spawn(bacteria.gameObject);

                //For testing
                _mapCtrl._trapCtrl.SpawnTrapBacteriaServer(bacteria, bacteria.transform.position, Databases.TrapDbId.ExplodingTrap);
            }

            public Bacteria GetBacteria(Network.NetworkArrayObject.UniqueId uniqueId)
            {
                return _bacteriaArray.GetFromId(uniqueId);
            }

            /// <summary>Runs the logic when a bullet hits a bacteria</summary>
            /// <returns>If the hit counted or not</returns>
            public bool OnBulletHitBacteria(Bullet bullet, Bacteria bacteria)
            {
                //Bacterias should ignore bullets from each other. Don't process dead bacteria either
                if (bullet._ownerBlockType == BlockType.Bacteria || !bacteria.IsAlive)
                    return false;

                //Check if enough time has passed for this bullet to hit the bacteria again
                if (bullet._bacteriaHitsMs.ContainsKey(bacteria._uniqueId))
                {
                    //If not enough time has passed, hit shouldn't count
                    if (GlobalContext.localTimeMs < bullet._bacteriaHitsMs[bacteria._uniqueId])
                        return false;
                    bullet._bacteriaHitsMs[bacteria._uniqueId] = GlobalContext.localTimeMs + bullet._hitDelayMs;
                }
                else
                    bullet._bacteriaHitsMs.Add(bacteria._uniqueId, GlobalContext.localTimeMs + bullet._hitDelayMs);

                //Ignore dmg if these conditions are true but still say the hit triggered
                if (bacteria._isInvincible)
                    return true;

                //TODO: update bacteria animator here when hit

                //Only the owner of the bullet actually goes to the phase of calculating damage and spreading it in the network. But other clients still process the animation hits
                //This should keep it graphically acceptable and still let us keep bullets away from the network
                if (!bullet._clientIsOwnerOfBullet)
                    return true;

                //If we get here we know we are the owner of the bullet. Now if we're the host, we apply damage and broadcast. If we're not the host, we only request damage
                var damage = CalcDamage(bullet._attackPower, bacteria); 
                if (Network.NetworkInfo.IsHost)
                    ApplyDamageToBacteria(bacteria, damage); //HP is synchronized by Mirror
                else //Request host to apply damage to the target bacteria                
                    RequestBacteriaDamageEvent.Send(bacteria, damage);                

                return true;
            }

            public void KnockBacteria(Bacteria bacteria, Vector2 direction, float distance)
            {
                if (bacteria._isKnockImmune)
                    return;

                //TODO: knock back should be a lerp over time. The delay that appears when running clients is because in server it teleports but in client it lerps automatically

                //If there's a structure in the way, don't move
                var origin = bacteria.transform.position;
                var hit = Physics2D.Raycast(origin, direction, distance, Constants.STRUCTURES_MASK);
                if (hit)
                    bacteria.transform.position = hit.point; //TODO: should apply an offset so it won't overlap with a structure
                else
                    bacteria.transform.Translate(direction * distance, Space.World);
            } 

            public void DamageBacteria(Bacteria bacteria, int attackPower)
            {
                //It's possible to receive this damage twice in 1 loop if bacteria triggers something like 2 aoes
                if(bacteria.IsAlive)
                    ApplyDamageToBacteria(bacteria, CalcDamage(attackPower, bacteria));
            }

            //******************************************************************* Internal Utility methods
            private void RunAi(Bacteria bacteria)
            {
                if (GlobalContext.localTimeMs >= bacteria._movement.nextWanderTime)
                    CalculateWanderPath(bacteria);

                if (bacteria._isMoving)
                {
                    //Calculate next position
                    var nextPos = Vector3.MoveTowards(bacteria.transform.position, bacteria._movement.Target, GlobalContext.localDeltaTimeSec * bacteria._baseMoveSpeed);
                    var direction = nextPos - bacteria.transform.position;

                    //Only move if collisions don't cancel the movement
                    CheckBacteriaMovementCollision(bacteria, direction);
                    if(bacteria._isMoving)
                        bacteria.transform.position = nextPos;

                    //Check if we reached target position to update movement
                    if (nextPos == bacteria._movement.Target)
                    {
                        if (bacteria._movement.IsAtEnd)
                        {
                            bacteria._isMoving = false;
                            bacteria._movement.nextWanderTime = GlobalContext.localTimeMs + (int)(Random.value * Constants.MAX_WANDER_TIME_INTERVAL);
                        }
                        else
                            bacteria._movement.currentIndex++;
                    }
                }
            }

            private void CheckBacteriaMovementCollision(Bacteria bacteria, Vector2 direction)
            {
                //TODO: Should do multiple raycasts according to bacteria size and minimum object size. This is to ensure we never miss an object when we move

                var radius = bacteria.GetComponent<CircleCollider2D>().radius; //TODO: this should be a variable instead of get component
                var hits = Physics2D.RaycastAll(bacteria.transform.position, direction, direction.magnitude + radius, Constants.BACTERIA_MOVE_COLLISION_MASK);

                //No hits
                if (hits.Length <= 0)
                    return;

                //Else we have collisions
                for (int i = 0; i < hits.Length; i++)
                {
                    var layer = hits[i].collider.gameObject.layer;

                    //check what we collided with
                    if (layer == Constants.CELLS_LAYER)
                    {
                        WDebug.Log("Bacteria collided with Cell");

                        var cell = hits[i].collider.GetComponent<Cell>();

                        //Knock bacteria
                        var cellToBacteriaDir = (bacteria.transform.position - cell.transform.position).normalized;
                        KnockBacteria(bacteria, cellToBacteriaDir, Constants.DEFAULT_KNOCKBACK_DISTANCE);

                        //Always cancel movement if we knocked with a cell. Also set position so we don't move further than the collision point
                        CancelMovement(bacteria);
                        bacteria.transform.position = hits[i].point;

                        //Process hit on cell if cell has authority, otherwise notify client that he has been hit
                        if (!cell.hasAuthority)
                        {
                            NetworkEvents.CellEvents.RequestCellDamageEvent.Send(cell.connectionToClient, bacteria.GetAttackPower(), -cellToBacteriaDir * Constants.DEFAULT_KNOCKBACK_DISTANCE);
                            continue;
                        }

                        //If we get here, then we have authority. Apply damage and knockback cell
                        _mapCtrl._cellCtrl.DamageCell(cell, bacteria.GetAttackPower());
                        _mapCtrl._cellCtrl.KnockCell(cell, -cellToBacteriaDir, Constants.DEFAULT_KNOCKBACK_DISTANCE);
                    }
                    else if (layer == Constants.PILLS_LAYER)
                    {
                        //WDebug.LogVerb("Bacteria collided with pill - Not implemented");
                    }
                    else if (layer == Constants.TRAPS_LAYER)
                    {
                        //WDebug.LogVerb("Bacteria collided with trap - Not implemented");
                    }
                    else
                        WDebug.LogWarn("Bacteria collided with unknown layer: " + hits[i].collider.gameObject.layer);
                }
            }

            private Structure GetStructureInPath(Vector2 origin, Vector2 destination)
            {
                var hit = Physics2D.Raycast(origin, (destination - origin).normalized, Vector2.Distance(origin, destination), Constants.PATH_COLLISION_MASK);

                if (hit.collider == null)
                    return null;

                WDebug.Assert(hit.collider.GetComponent<Structure>() != null, "Detected something in the raycast but it is not a structure");
                return hit.collider.GetComponent<Structure>();
            }
            
            private int CalcDamage(int attackPower, Bacteria targetBacteria)
            {
                WDebug.Log("Dealing " + Mathf.Max(attackPower - targetBacteria.GetDefense(), 0) + " damage to bacteria");
                return Mathf.Max(attackPower - targetBacteria.GetDefense(), 0);
            }

            private void CancelMovement(Bacteria bacteria)
            {
                bacteria._isMoving = false;
                bacteria._movement.nextWanderTime = GlobalContext.localTimeMs + (int)(Random.value * Constants.MAX_WANDER_TIME_INTERVAL);
            }

            /// <summary>Kills a bacteria. Can be called on both the host and clients.</summary>
            private void KillBacteria(Bacteria bacteria)
            {
                //In case health is negative or we were called from an instant kill method
                bacteria._currentHp = 0;

                //Only handle respawns and notify death if we're the host
                if (!Network.NetworkInfo.IsHost)
                    return;

                _bacteriaArray.Remove(bacteria);
                _bacteriaPool.Push(bacteria);
                NetworkServer.UnSpawn(bacteria.gameObject);

                NetworkEvents.CellEvents.NotifyAddUpgradePoints.Send(Constants.DEFAULT_UPGRADE_POINTS_PER_KILL);
            }

            private void ApplyDamageToBacteria(Bacteria bacteria, int damage)
            {
                //apply dmg, and if health becomes <= 0, kill bacteria
                bacteria._currentHp -= damage;

                if (!bacteria.IsAlive)
                    KillBacteria(bacteria);
            }

            private void CalculateWanderPath(Bacteria bacteria)
            {
                bacteria._movement.path.Clear();
                bacteria._movement.currentIndex = 0;

                Vector2 targetPos;
                Structure structure;
                int count = -1;

                //Try to get a valid destination
                do
                {
                    targetPos = new Vector2(Random.Range(-Constants.WANDER_DISTANCE, Constants.WANDER_DISTANCE),
                                                Random.Range(-Constants.WANDER_DISTANCE, Constants.WANDER_DISTANCE));
                    targetPos.x += bacteria.transform.position.x;
                    targetPos.y += bacteria.transform.position.y;

                    structure = GetStructureInPath(bacteria.transform.position, targetPos);

                    if (structure == null)
                    {
                        bacteria._movement.path.Add(targetPos);
                        bacteria._isMoving = true;
                        bacteria._movement.nextWanderTime = int.MaxValue;
                        return;
                    }

                } while (structure.ContainsPoint(targetPos) && count++ < Constants.MAXIMUM_WANDER_RETRIES);

                if (count == Constants.MAXIMUM_WANDER_RETRIES)
                    return;

                ref var nodes = ref GlobalContext.structureDb.GetStructureData(structure._dbId)._pathNodes;
                var path = _mapCtrl._pathfinder.GetPath(bacteria.transform.position, targetPos, ref nodes, structure.transform.position);

                if (path == null)
                {
                    WDebug.Log("Couldn't find path from: " + bacteria.transform.position + " to: " + targetPos);
                    return;
                }

                bacteria._movement.path = path;
                bacteria._isMoving = true;
                bacteria._movement.nextWanderTime = int.MaxValue;
            }

       
            //******************************************************************* Network event handlers
            private void OnBacteriaSpawnedClient(Bacteria bacteria)
            {
                WDebug.Assert(!Network.NetworkInfo.IsHost, "Bacteria spawned callback called in host");
                WDebug.Log("Bacteria spawned. Tag: " + bacteria._uniqueId.tag);
                bacteria.Configure(); //This should not be needed if stats are synched through mirror
                _bacteriaArray.Insert(bacteria);
            }

            private void OnBacteriaDespawnedClient(Bacteria bacteria)
            {
                WDebug.Assert(!Network.NetworkInfo.IsHost, "Bacteria despawned callback called in host");
                WDebug.Log("Bacteria despawned. Tag: " + bacteria._uniqueId.tag);

                _bacteriaArray.Remove(bacteria);
            }

            private void OnRequestBacteriaDamageEvent(NetworkConnection connection, RequestBacteriaDamageEvent dmgEvent)
            {
                WDebug.Assert(Network.NetworkInfo.IsHost, "Request bacteria damage called on non host");

                //If bacteria is already gone
                if (!_bacteriaArray.Contains(dmgEvent.bacteriaId))
                    return;

                var bacteria = _bacteriaArray.GetFromId(dmgEvent.bacteriaId);
                WDebug.Assert(bacteria.IsAlive, "Got request bacteriaDamage event but bacteria was already dead. Shouldn't happen due to the 'contains' above");

                //Otherwise apply damage and have it sync through mirror
                ApplyDamageToBacteria(bacteria, dmgEvent.damage);
                if (dmgEvent.knockForce != Vector2.zero)
                    KnockBacteria(bacteria, dmgEvent.knockForce, dmgEvent.knockForce.magnitude);
            }

            
            //******************************************************************* Syncing methods
            private void OnRequestBacteriaSyncServer(NetworkConnection connection, RequestBacteriaSyncEvent syncEvent)
            {
                WDebug.Log("Received request to sync bacterias with a client");

                //Construct the list of cells that need to be synced
                ReplyBacteriaSyncEvent reply;
                reply._bacteriaSyncDatas = new BacteriaSyncData[_bacteriaArray.Count];
                int count = 0;

                //fill the sync data
                ref var bacterias = ref _bacteriaArray.GetAll();
                for (int i = 0; i <= _bacteriaArray.LastIndex; i++)
                {
                    if (bacterias[i] == null)
                        continue;

                    reply._bacteriaSyncDatas[count]._auras = _mapCtrl._auraCtrl.GetBacteriaAuraSyncData();
                    reply._bacteriaSyncDatas[count]._traps = _mapCtrl._trapCtrl.GetBacteriaTrapSyncData();
                    reply._bacteriaSyncDatas[count]._bullets = _mapCtrl._bulletCtrl.GetBacteriaBulletSyncData();
                    count++;
                }

                ReplyBacteriaSyncEvent.Send(connection, ref reply);
            }

            private void OnReplyBacteriaSyncClient(ReplyBacteriaSyncEvent reply)
            {
                WDebug.Log("Received reply to sync bacterias with host");

                NetworkClient.UnregisterHandler<ReplyBacteriaSyncEvent>();

                for (int i = 0; i < reply._bacteriaSyncDatas.Length; i++)
                {
                    var data = reply._bacteriaSyncDatas[i];

                    _mapCtrl._auraCtrl.SetBacteriaAurasFromSyncData(ref data._auras);
                    _mapCtrl._trapCtrl.SetBacteriaTrapsFromSyncData(ref data._traps);
                    _mapCtrl._bulletCtrl.SetBacteriaBulletsFromSyncData(ref data._bullets);
                }
            }
        }
    }
}

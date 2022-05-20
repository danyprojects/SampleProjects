using System.Collections.Generic;
using UnityEngine;

using Bacterio.MapObjects;
using Bacterio.Databases;
using Bacterio.Common;
using Bacterio.NetworkEvents.BulletEvents;

namespace Bacterio.Game
{
    public sealed partial class MapController : System.IDisposable
    {
        private sealed partial class BulletController : System.IDisposable
        {   
            private readonly MapController _mapCtrl = null;
            private readonly ObjectPool<Bullet> _bulletPool = null;

            //Each cell will have it's own array of bullets, where the owner of the cell controls the IDs of the bullets.
            //Every client will process all the arrays, and is able to destroy the bullet locally even if it is not the owner
            //For effects that trigger when a bullet hits, the owner of the bullet will broadcast the effect as well as the bullet ID. That way the effect will show even if the bullet has already been removed
            //For enemy bullets, they are always spawned by the host, but every player can say that they were hit by an enemy bullet. This will be forwarded to the server, who will do the actual process of applying damage.
            //  This is so that if same single target bullet hits 2 different players in 2 different screens, only the first one gets damage. And in the case an explosion happens, the explosion gets broadcasted to the spot the first one was hit
            //  This guarantees that while it is possible the player won't actually get damaged when he gets hit, he will also not get damaged unless he sees himself get hit.
            //  It is possible that he can get hit by an explosion that triggered next to him rather than on him, if the collision of 2 bullets above happens. But in this case the result is the same so it doesn't matter
            private readonly BlockArray<Bullet>[] _cellBullets = null;
            private readonly BlockArray<Bullet> _bacteriaBullets = null;


            public BulletController(MapController mapController)
            {
                _mapCtrl = mapController;

                var bulletObj = GlobalContext.assetBundleProvider.LoadObjectAsset("Bullet");
                WDebug.Assert(bulletObj != null, "No bullet object from bundle");

                _bulletPool = new ObjectPool<Bullet>(bulletObj.GetComponent<Bullet>(), Constants.BULLET_POOL_INITIAL_SIZE, Constants.BULLET_POOL_GROWTH_AMOUNT, Constants.OUT_OF_RANGE_POSITION, Quaternion.identity, null, OnBulletPush, OnBulletPop);
                _bacteriaBullets = new BlockArray<Bullet>(Constants.BULLET_BACTERIA_INITIAL_AMOUNT, Constants.BULLET_BACTERIA_GROWTH_AMOUNT);
                _cellBullets = new BlockArray<Bullet>[Constants.MAX_PLAYERS];

                for (int i = 0; i < Constants.MAX_PLAYERS; i++)
                    _cellBullets[i] = new BlockArray<Bullet>(Constants.BULLET_CELL_INITIAL_AMOUNT, Constants.BULLET_CELL_GROWTH_AMOUNT);
            }

            public void Update()
            {
                //Run update bullets for all arrays
                UpdateBulletArray(_bacteriaBullets);
                for (int i = 0; i < _cellBullets.Length; i++)
                    UpdateBulletArray(_cellBullets[i]);
            }

            public void UpdateOnReconnectLocalPlayerReady()
            {

            }

            public void CleanupRemotePlayer(Network.NetworkPlayerObject player)
            {
                WDebug.Assert(_cellBullets.Length > player._uniqueId._index && _cellBullets[player._uniqueId._index] != null, "Bullets: Got cleanup for unexistent player");
                _cellBullets[player._uniqueId._index].Dispose();
            }

            public void Dispose()
            {
                //Dispose of the cluster bullets first
                ref var bullets = ref _bacteriaBullets.GetAll();
                for (int i = 0; i <= _bacteriaBullets.LastIndex; i++)                
                    if (bullets[i] != null && bullets[i]._clusterBullets != null && bullets[i]._clusterBullets.Length > 0)
                        RemoveBullet(bullets[i], _bacteriaBullets);

                for (int k = 0; k < _cellBullets.Length; k++)
                {
                    bullets = ref _cellBullets[k].GetAll();
                    for (int i = 0; i <= _cellBullets[k].LastIndex; i++)
                        if (bullets[i] != null && bullets[i]._clusterBullets != null && bullets[i]._clusterBullets.Length > 0)
                            RemoveBullet(bullets[i], _cellBullets[k]);
                }

                //Destroy all bullets
                _bulletPool.Dispose();

                _bacteriaBullets.Dispose();
                for (int i = 0; i < _cellBullets.Length; i++)
                    _cellBullets[i].Dispose();
            }


            //******************************************************************* Public utility methods
            public Bullet ShootBulletOwner(Cell localCell, Vector2 direction)
            {
                WDebug.Assert(localCell.hasAuthority, "Shoot bullet (add) overload called without client authority");

                //Get a new bullet and configure it
                var bullet = _bulletPool.Pop();
                bullet.Configure(localCell, direction, GlobalContext.localTimeMs, 15 * Constants.ONE_SECOND_MS);

                //Add it to the array
                _cellBullets[localCell._uniqueId._index].Add(bullet);

                //Process the shooting attributes before updating the shooting time and ammunition, since the attributes might affect them
                ProcessBulletShootAttribute(localCell, bullet);

                //Update cell variables
                localCell._nextShootTimeMs += localCell.GetShootDelay();
                localCell.Ammunition -= localCell._bulletData._ammunitionCost;

                return bullet;
            }

            public Bullet ShootBulletClient(Cell owner, Vector2 direction, Block.UniqueId bulletUniqueId)
            {
                //Get a new bullet and configure it
                var bullet = _bulletPool.Pop();
                bullet.Configure(owner, direction, GlobalContext.localTimeMs, 15 * Constants.ONE_SECOND_MS);

                //Add it to the array
                _cellBullets[owner._uniqueId._index].Insert(bullet, bulletUniqueId);

                return bullet;
            }

            
            //******************************************************************* Internal utility methods
            private void UpdateBulletArray(BlockArray<Bullet> bulletArray)
            {
                ref var bullets = ref bulletArray.GetAll();
                for (int i = 0; i <= bulletArray.LastIndex; i++)
                {
                    if (bullets[i] == null)
                        continue;

                    //Raycast should run before moving, so we check for collisions after the bullet is visually at the position of last update.
                    //If we run the raycast after moving as well, visually we will not see the last movement of the bullet

                    //If it's a cluster bullet, run the collision check per bullet in the cluster instead of the own bullet
                    if (bullets[i].IsClusterParent)                    
                        ProcessBulletCluster(ref bullets[i]._clusterBullets);                    
                    else
                        CheckBulletCollision(bullets[i]);

                    //Check if bullet needs to be removed 
                    if (bullets[i]._removeTimeMs < GlobalContext.localTimeMs)
                    {
                        RemoveBullet(bullets[i], bulletArray);
                        continue;
                    }
                    else if (!bullets[i].enabled) //Otherwise check if it's disabled. Nothing to do other than wait for remove time 
                    {
                        continue;
                    }
                    else if (bullets[i]._lifeTimeMs < GlobalContext.localTimeMs) //Otherwise check if it has passed regular lifetime to disable it
                    {
                        DeactivateBullet(bullets[i]);
                        continue;
                    }

                    //move bullet (moves the whole cluster if it's a cluster)
                    ProcessBulletMovementAttribute(bullets[i]);
                }
            }

            private void CheckBulletCollision(Bullet bullet)
            {
                //Run a raycast in bullet direction to check for targets
                //TODO - BAC-30: Collision detection needs optimizing.

                //could happen if bullet has disabled the damage and only has movement
                if (bullet.HasNoDamageHitAttribute)
                    return;

                var origin = bullet.transform.position;
                var distance = bullet.GetComponent<SpriteRenderer>().size.x / 4;
                var hits = Physics2D.RaycastAll(origin, bullet.transform.up, distance, Constants.BULLET_MOVE_COLLISION_MASK);
                WDebug.LogCWarn(Constants.BULLET_DIRECTION_AXIS != Vector2.up, "WARNING: Bullet direction constant doesn't match the axis used in the raycast");

                //No collisions
                if (hits.Length <= 0)
                    return;

                //Check if any of the collisions matters. And if it does run the process hit attribute right away. This way AOE effects can trigger per enemy hit if it's a pierce bullet
                for (int i = 0; i < hits.Length && bullet.enabled; i++)
                {
                    if(hits[i].transform.gameObject.layer == Constants.CELLS_LAYER)
                    {
                        var cell = hits[i].transform.GetComponent<Cell>();
                        if (_mapCtrl._cellCtrl.OnBulletHitCell(bullet, cell))
                            ProcessBulletHitAttribute(bullet, hits[i].normal);
                    }
                    else if (hits[i].transform.gameObject.layer == Constants.BACTERIA_LAYER)
                    {
                        var bacteria = hits[i].transform.GetComponent<Bacteria>();
                        if (_mapCtrl._bacteriaCtrl.OnBulletHitBacteria(bullet, bacteria))
                            ProcessBulletHitAttribute(bullet, hits[i].normal);
                    }
                    else if (hits[i].transform.gameObject.layer == Constants.AURAS_LAYER)
                    {
                       // WDebug.Log("Bullet hit Aura");
                    }
                    else if (hits[i].transform.gameObject.layer == Constants.TRAPS_LAYER)
                    {
                      //  WDebug.Log("Bullet hit trap");
                    }
                    else if(hits[i].transform.gameObject.layer == Constants.STRUCTURES_LAYER)
                    {
                     //   WDebug.Log("Bullet hit structure");
                    }
                    else
                        WDebug.LogWarn("Bullet collided with unknown layer: " + hits[i].collider.gameObject.layer);
                }
            }

            private void ProcessBulletCluster(ref Bullet[] bullets)
            {
                for (int i = 0; i < bullets.Length; i++)
                {
                    var clusterBullet = bullets[i];
                    if (clusterBullet == null) //Some could be null due to already removed
                        continue;

                    CheckBulletCollision(clusterBullet);

                    //If bullet has detached from the cluster, move it seperatly from the cluster
                    if (clusterBullet.IsDetachedFromCluster)
                        ProcessBulletMovementAttribute(clusterBullet);
                }
            }
            
            private void ProcessBulletMovementAttribute(Bullet bullet)
            {
                if(bullet.IsRotative)
                {
                    WDebug.Assert(bullet._clusterBulletCount > 0, "Non cluster parent bullet with rotative movement attribute");
                    bullet._clusterTransform.Rotate(Vector3.forward, bullet._rotationSpeed * GlobalContext.localDeltaTimeSec, Space.World);
                }

                //Process the movement type without rotative. These are all exclusive. This is what will move the "center" in case bullet is rotative
                switch (bullet.MoveAttributeNoRotative)
                {
                    case MovementAttribute.Straight: bullet.transform.Translate(Constants.BULLET_DIRECTION_AXIS * GlobalContext.localDeltaTimeSec * bullet._moveSpeed, Space.Self); break;
                    case MovementAttribute.Wave:
                    {
                        float lerpFactor = 0.5f * (1 + Mathf.Sin(Mathf.PI / 2 +  Mathf.PI * (GlobalContext.localTimeSec - bullet._spawnTimeMs / (float)Constants.ONE_SECOND_MS) * bullet._waveSpeed));
                        bullet.transform.localEulerAngles = new Vector3(0, 0, bullet._initialAngle + Mathf.LerpAngle(-bullet._maxAngle, bullet._maxAngle, lerpFactor));
                        bullet.transform.Translate(Constants.BULLET_DIRECTION_AXIS * GlobalContext.localDeltaTimeSec * bullet._moveSpeed, Space.Self);
                    } break;
                    case MovementAttribute.Immovable:
                    {
                        float lerpFactor = 0.5f * (1 + Mathf.Sin(Mathf.PI * GlobalContext.localTimeSec * bullet._rotationSpeed));
                        bullet.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(0, 0, -bullet._maxAngle), Quaternion.Euler(0, 0, bullet._maxAngle), lerpFactor);
                    } break;
                    case MovementAttribute.Boomerang:
                    {
                        
                    } break;
                    case MovementAttribute.Homing:
                    {
                    } break;

                    default: WDebug.LogError("Invalid movement attribute in bullet data :" + bullet._movementAttribute); break;
                }

                //If it's a cluster, update the position of the cluster
                if (bullet.IsClusterParent)
                    bullet._clusterTransform.position = bullet.transform.position;
            }

            private void ProcessBulletShootAttribute(Cell cell, Bullet bullet)
            {
                //Process burst first, as it's the only thing that affects the reload speed
                if (cell.HasBurstAttribute)
                {
                    ref var bulletData = ref GlobalContext.bulletDb.GetBulletData(cell._bulletData._dbId);
                    cell._bulletData._bulletBurstCounter = (byte)((cell._bulletData._bulletBurstCounter + 1) % cell._bulletData._bulletBurstMax);
                    cell.BaseReloadSpeed = cell._bulletData._bulletBurstCounter == 0 ? bulletData.reloadTime : bulletData.burstConfig.burstCooldown; //If it's 0 then the counter has done a full lap due to line above
                }

                //Then check if it's cluster
                if (cell.HasClusterAttribute)
                {
                    bullet._renderer.enabled = false;
                    bullet._clusterBullets = new Bullet[bullet._clusterBulletCount];

                    //Get an empty transform that we'll use to rotate the cluster. This is an independent transform that will follow the main bullet
                    //It has to be independent so we can apply paths like wave and still have the correct rotation in Rotative
                    bullet._clusterTransform = GlobalContext.emptyTransformsPool.Pop();
                    bullet._clusterTransform.SetPositionAndRotation(bullet.transform.position, bullet.transform.rotation);

                    GenerateClusterShape(bullet);

                    cell._bulletData._ammunitionCost = bullet._clusterBulletCount;
                }
                else
                    cell._bulletData._ammunitionCost = 1;

                //If it's not cluster then it would be default, which we don't need to do anything.
                WDebug.Assert(cell.HasClusterAttribute || cell.HasSingleAttribute, "Invalid cell shoot attribute: " + cell._bulletData._shootAttribute);
            }

            private void ProcessBulletHitAttribute(Bullet bullet, Vector2 normal)
            {
                //Run bullet specific code
                //Bullets should always be deactivated first instead of removed. This is because we do not notify when a bullet ends, to save some graphical glitches. So
                //  it is possible that a bullet will not hit something in one of the clients.
                //  and only hit something near the end of its lifetime. In this case the bullet must still exist everywhere so that it can be accessed if necessary.
                //  Bullets will only be removed when their lifetime hits 0

                //Process area Hit first, as these always trigger even since they're not exclusive with the rest and they don't consume the bullet
                switch(bullet.HitAttributeArea)
                {
                    case HitAttribute.AreaCircle: break;
                    case HitAttribute.AreaCone: break;

                    default: break; //Will happen if there's no area hit attribute configured. Ignore it
                }

                //Then process hit without "Area", these are exclusive. This will decide if bullet is consumed / should change direction
                switch (bullet.HitAttributeNoArea)
                {
                    case HitAttribute.Once: DeactivateBullet(bullet);break;
                    case HitAttribute.Pierce:
                    {
                        bullet._hitCount++;
                        if (bullet._hitCount >= bullet._maxHits)
                            DeactivateBullet(bullet);
                    }break;
                    case HitAttribute.Ricochet:
                    {
                        //Detatch from cluster if it was part of one
                        if (bullet.IsClusterChild)
                            DetachBulletFromCluster(bullet);
                        
                        bullet.transform.right = Vector2.Reflect(bullet.transform.up, normal);
                        bullet._hitCount++;
                        bullet._initialAngle = bullet.transform.localEulerAngles.z;

                        if (bullet._hitCount >= bullet._maxHits)
                            DeactivateBullet(bullet);
                    }break;

                    default: WDebug.LogError("Invalid bullet hit attribute: " + bullet._hitAttribute); break;
                }
            }

            private void DetachBulletFromCluster(Bullet bullet)
            {
                //The parent should still keep the bullet inside the cluster array. This way the detached bullet will still be processed by cluster itself, it just won't follow it.
                //This will let us keep the bullet without needing a unique ID, so no changes in the network are needed. It will be destroyed when the cluster is also destroyed

                var parent = bullet._clusterParent;

                //Copy owner
                bullet._ownerBlockType = parent._ownerBlockType;
                bullet._owner = parent._owner;
                bullet._clientIsOwnerOfBullet = parent._clientIsOwnerOfBullet;

                //Remove parent but keep the position. Keep the reference to the cluster still
                bullet.transform.SetParent(null, true);
            }

            private void RemoveBullet(Bullet bullet, BlockArray<Bullet> bullets)
            {
                //If it's a cluster bullet, remove all child bullets as well
                if (bullet._clusterBulletCount > 0)
                {
                    for (int i = 0; i < bullet._clusterBullets.Length; i++)
                    {
                        if (bullet._clusterBullets[i] == null) //Skip any cluster bullets that were already removed
                            continue;

                        bullet._clusterBullets[i].transform.SetParent(null, false);
                        _bulletPool.Push(bullet._clusterBullets[i]);
                    }

                    //Free the empty transform used for rotations
                    GlobalContext.emptyTransformsPool.Push(bullet._clusterTransform);
                }

                //Remove the parent for immovable bullets
                if ((bullet._movementAttribute & MovementAttribute.Immovable) > 0)
                    bullet.transform.SetParent(null, false);

                _bulletPool.Push(bullet);

                //Then remove from bullet array
                bullets.Remove(bullet);
            }

            private void DeactivateBullet(Bullet bullet)
            {
                //If bullet is boomerang we should just disable the damage but keep the movement going, because we need to see the bullet reach the owner
                if(bullet.IsBoomerang)
                {
                    bullet._hitAttribute = HitAttribute.NoDamage;
                    return;
                }

                //Bullet is part of a cluster, remove it instead of disabling
                if (bullet.IsClusterChild)
                {
                    var parent = bullet._clusterParent;

                    //Remove parent (might still have it) and put it in the pool for reuse
                    bullet.transform.SetParent(null, false);
                    _bulletPool.Push(bullet);

                    //If there are no more bullets in the cluster. Disable the cluster parent
                    parent._clusterBulletCount--;
                    if (parent._clusterBulletCount <= 0)
                    {
                        parent._clusterBullets = System.Array.Empty<Bullet>();
                        parent.enabled = false;
                    }

                    //Look for the bullet index to remove it from the parent first. Will do nothing if parent was disabled above
                    for (int i = 0; i < parent._clusterBullets.Length; i++) 
                        if(parent._clusterBullets[i] == bullet)
                        {
                            parent._clusterBullets[i] = null;
                            break;
                        }
                }
                else
                {
                    bullet.enabled = false;
                    bullet.transform.position = Constants.OUT_OF_RANGE_POSITION;
                }
            }

            private void GenerateClusterShape(Bullet parentBullet)
            {
                //TODO: this should follow the shapes from the DB
                int startX = -parentBullet._clusterBulletCount / 2;
                //Get all the bullets for the cluster and parent them to the original bullet
                for (int i = 0; i < parentBullet._clusterBulletCount; i++)
                {
                    var childBullet = _bulletPool.Pop();
                    childBullet.Configure(parentBullet);

                    //There should be different cluster configurations that handle the position later on
                    childBullet.transform.localPosition = new Vector2(startX + i, 0);
                    childBullet.transform.SetParent(parentBullet._clusterTransform, false);
                    parentBullet._clusterBullets[i] = childBullet;
                }

            }

            //******************************************************************* Network event handlers


            //******************************************************************* Syncing methods
            public CellBulletSyncData[] GetCellBulletSyncData(Cell cell)
            {
                var array = _cellBullets[cell._uniqueId._index];
                CellBulletSyncData[] data = new CellBulletSyncData[array.Count];
                int count = 0;

                //fill the sync data
                ref var bullets = ref array.GetAll();
                for (int i = 0; i <= array.LastIndex; i++)
                {
                    if (bullets[i] == null)
                        continue;

                    data[count].bulletId = bullets[i]._uniqueId;
                    data[count].position = bullets[i].transform.localPosition;
                    data[count].direction = bullets[i].transform.right;
                    data[count].remainingLifetime = bullets[i]._lifeTimeMs - GlobalContext.localTimeMs;
                    count++;
                }

                return data;
            }

            public void SetCellBulletsFromSyncData(Cell cell, ref CellBulletSyncData[] syncData)
            {
                //There is a possibility that we already received some bullets before the sync arrived, so we need to check for that. The oposite isn't possible
                for (int i = 0; i < syncData.Length; i++)
                {
                    //Only attach if bullet doesnt exist yet
                    if (!_cellBullets[cell._uniqueId._index].Contains(syncData[i].bulletId))
                    {
                        var bullet = ShootBulletClient(cell, syncData[i].direction, syncData[i].bulletId);
                        //Set the remaining vars that are not part of the config
                        bullet.transform.position = syncData[i].position;
                        bullet._lifeTimeMs = GlobalContext.localTimeMs + syncData[i].remainingLifetime;
                    }
                }
            }

            public BacteriaBulletSyncData[] GetBacteriaBulletSyncData()
            {
                BacteriaBulletSyncData[] data = new BacteriaBulletSyncData[_bacteriaBullets.Count];
                int count = 0;

                //fill the sync data
                ref var bullets = ref _bacteriaBullets.GetAll();
                for (int i = 0; i <= _bacteriaBullets.LastIndex; i++)
                {
                    if (bullets[i] == null)
                        continue;

                    data[count]._bacteriaId = ((Bacteria)bullets[i]._owner)._uniqueId;
                    data[count].bulletId = bullets[i]._uniqueId;
                    data[count].direction = bullets[i].transform.right;
                    data[count].position = bullets[i].transform.position;
                    data[count].remainingLifetime = bullets[i]._lifeTimeMs - GlobalContext.localTimeMs;
                    count++;
                }

                return data;
            }

            public void SetBacteriaBulletsFromSyncData(ref BacteriaBulletSyncData[] syncData)
            {
                if (syncData == null)
                    return;

                //There is a possibility that we already received some bullets before the sync arrived, so we need to check for that. The oposite isn't possible
                for (int i = 0; i < syncData.Length; i++)
                {
                    WDebug.Assert(_mapCtrl._bacteriaCtrl.GetBacteria(syncData[i]._bacteriaId), "Got sync data bullet for inexistent bacteria");

                    //Only attach if bullet doesnt exist yet
                    if (!_bacteriaBullets.Contains(syncData[i].bulletId))
                    {
                        //TODO: Attach to bacteria
                    }
                }
            }

            //******************************************************************* Bullet pool methods
            private void OnBulletPop(Bullet bullet)
            {
                bullet._renderer.enabled = true;
            }

            private void OnBulletPush(Bullet bullet)
            {
                bullet.transform.position = Constants.OUT_OF_RANGE_POSITION;
                bullet._clusterBullets = null;
                bullet._clusterParent = null;
                bullet._clusterBulletCount = 0;
                bullet._bacteriaHitsMs.Clear();
                for (int i = 0; i < bullet._cellHitMs.Length; i++)
                    bullet._cellHitMs[i] = 0;
            }
        }
    }
}

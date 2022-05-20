using UnityEngine;

using Mirror;
using Bacterio.NetworkEvents.CellEvents;
using Bacterio.MapObjects;

namespace Bacterio.Game
{
    public sealed partial class MapController : System.IDisposable
    {
        private sealed partial class CellController : System.IDisposable
        {
            private readonly MapController _mapCtrl = null;

            public Cell LocalCell { get; private set; } = null;
            public int CellCount { get; private set; } = 0;
            private readonly Cell[] _cells = null;
            private bool _isSynced = true;
            private bool _hasAuthority = true;

            public CellController(MapController mapController, bool isReconnect)
            {
                _mapCtrl = mapController;
                _isSynced = !isReconnect;
                _hasAuthority = !isReconnect;
                _cells = new Cell[Constants.MAX_PLAYERS];

                //register input events for local player
                GameContext.inputHandler.RegisterOnMovementEvent(OnLocalPlayerMovementEvent, OnLocalPlayerMovementReleaseEvent);
                GameContext.inputHandler.RegisterOnShootEvent(OnLocalPlayerShootEvent, OnLocalPlayerShootReleaseEvent);

                //If it's not a reconnect, normal behaviour
                if (!isReconnect)
                {
                    var players = _mapCtrl._networkPlayerController.GetAll(); ;
                    for (int i = 0; i <= _mapCtrl._networkPlayerController.LastIndex; i++)
                    {
                        if (players[i] == null)
                            continue;

                        _cells[i] = (Cell)players[i];
                        _cells[i].Configure(Databases.CellDbId.Default);
                        CellCount++;
                        if (_cells[i].hasAuthority)
                            LocalCell = _cells[i];
                    }

                    WDebug.Assert(LocalCell != null, "Didn't find a local cell even though we came from room screen");
                    LocalCell.transform.position = _mapCtrl._structureCtrl.GetSpawnPointNearHeart();
                    OnLocalCellReady();
                }
                else //Register to events from networkPlayerController that are only relevant for clients who are reconnecting
                {
                    //Assign the bundle cell as our local purely to avoid having to do null checks. It'll be overwritten once our real cell arrives
                    LocalCell = GlobalContext.assetBundleProvider.LoadObjectAsset("Cell").GetComponent<Cell>();
                    //Invalidate the variables from the object in the bundle just that the ifs always fail but prevent null checks
                    LocalCell._uniqueId._index = -1;
                    LocalCell._uniqueId._clientToken = Constants.INVALID_CLIENT_TOKEN;

                    _mapCtrl._networkPlayerController.PlayerGotAuthority += OnCellGotAuthority;
                }

                //Register to events from networkPlayerController that are always relevant
                _mapCtrl._networkPlayerController.PlayerSpawned += OnCellSpawned;
                _mapCtrl._networkPlayerController.PlayerDespawned += OnCellDespawned;

                //Register network events according to being host or client. Host has to register in client events too due to the way mirror handles the messages
                NetworkClient.RegisterHandler<NotifyShootEvent>(OnOtherPlayerShootEventClient);
                NetworkClient.RegisterHandler<RequestCellDamageEvent>(OnRequestCellDamageEventClient);
                NetworkClient.RegisterHandler<NotifyAddUpgradePoints>(OnNotifyAddUpgradePointsClient);
                
                if (Network.NetworkInfo.IsHost) //Also register events for host
                {
                    NetworkServer.RegisterHandler<NotifyShootEvent>(OnOtherPlayerShootEventServer);
                    NetworkServer.RegisterHandler<RequestCellSyncEvent>(OnRequestCellSyncServer);
                }
            }

            public void Update()
            {
                if (LocalCell.IsAlive)
                {
                    if (GlobalContext.localTimeMs >= LocalCell._nextBulletRegenMs)
                    {
                        LocalCell.Ammunition += 1;
                        LocalCell._nextBulletRegenMs += LocalCell.GetBulletRegenTime();
                    }

                    _mapCtrl._auraCtrl.UpdateLocalCellAuras();
                }
            }

            public void RunReconnectSync()
            {
                WDebug.Assert(!Network.NetworkInfo.IsHost, "Requesting sync cells with host");

                NetworkClient.RegisterHandler<ReplyCellSyncEvent>(OnReplyCellSyncClient);
                NetworkEvents.EmptyEvent.Send<RequestCellSyncEvent>();
            }

            public void CleanupRemotePlayer(Network.NetworkPlayerObject player)
            {
                WDebug.Assert(_cells[player._uniqueId._index] != null, "Got cleanup for unexistent player");

                //_cells[player._uniqueId._index] = null;
            }

            public void Dispose()
            {
                _mapCtrl._networkPlayerController.PlayerSpawned -= OnCellSpawned;
                _mapCtrl._networkPlayerController.PlayerDespawned -= OnCellDespawned;
                _mapCtrl._networkPlayerController.PlayerGotAuthority -= OnCellGotAuthority;

                //Unregister network events
                NetworkClient.UnregisterHandler<NotifyShootEvent>();
                NetworkClient.UnregisterHandler<ReplyCellSyncEvent>();
                NetworkClient.UnregisterHandler<RequestCellDamageEvent>();
                NetworkClient.UnregisterHandler<NotifyAddUpgradePoints>();

                if (Network.NetworkInfo.IsHost)
                {
                    NetworkServer.UnregisterHandler<NotifyShootEvent>();
                    NetworkServer.UnregisterHandler<RequestCellSyncEvent>();
                }

                for (int i = 0; i < _cells.Length; i++)
                    Object.Destroy(_cells[i]?.gameObject);
            }


            //******************************************************************* Public utility methods that can be called from any controller
            public void KillCell(Cell cell)
            {
                //Do generic cell killing stuff

                //Reset cell stats / flags
                cell.CurrentHp = 0;
                cell._isShooting = false;
                cell._isMoving = false;

                //If cell was local player, run specific code for local player only
                if (cell == LocalCell)
                    KillLocalPlayer();
            }

            /// <summary>Runs the logic when a bullet hits a cell</summary>
            /// <returns>If the hit counted or not</returns>
            public bool OnBulletHitCell(Bullet bullet, Cell cell)
            {
                //Only process hits by own cell. No friendly fire
                if (!cell.hasAuthority || bullet._ownerBlockType == BlockType.Cell)
                    return false;

                //Check if enough time has passed for this bullet to hit the cell again. If not, hit shouldn't count
                if (GlobalContext.localTimeMs < bullet._cellHitMs[cell._uniqueId._index])
                    return false;
                bullet._cellHitMs[cell._uniqueId._index] = GlobalContext.localTimeMs + bullet._hitDelayMs;

                //Ignore dmg if these conditions are true but still say the hit counted
                if (cell._isInvincible)
                    return true;

                DamageCell(cell, bullet._attackPower);

                return true;
            }

            public void KnockCell(Cell cell, Vector2 direction, float distance)
            {
                if (cell._isKnockImmune)
                    return;

                //If there's a structure in the way, don't move
                var origin = cell.transform.position;
                var hit = Physics2D.Raycast(origin, direction, distance, Constants.STRUCTURES_MASK);
                if (hit)
                    cell.transform.position = hit.point; //TODO: should apply an offset so it won't overlap with a structure
                else
                    cell.transform.Translate(direction * distance, Space.World);

                CheckCellIsInTerritory();
            }

            public void DamageCell(Cell cell, int attackPower)
            {
                WDebug.Assert(cell.hasAuthority, "Called damage cell but we don't have authority");

                //This could happen if we get pushed and trigger multiple things in same loop
                if (!cell.IsAlive)
                    return;

                //Apply damage to cell first
                cell.CurrentHp -= CalcDamage(attackPower, cell);

                if (cell.CurrentHp <= 0)
                    KillCell(cell);
            }

            public Cell GetCell(Network.NetworkPlayerObject.UniqueId uniqueId)
            {
                WDebug.Assert(uniqueId._index < _cells.Length, "Attempted to get a cell at an index out of bounds");
                return _cells[uniqueId._index];
            }

            public Cell GetCell(int index)
            {
                WDebug.Assert(index < _cells.Length, "Attempted to get a cell at an index out of bounds");
                return _cells[index];
            }

            public bool CheckAnyCellRemainingLives()
            {
                for (int i = 0; i < _cells.Length; i++)
                    if (_cells[i] != null && _cells[i].Lives >= 0)
                        return true;

                return false;
            }


            //******************************************************************* Local input events
            private void OnLocalPlayerMovementEvent(Vector2 direction)
            {
                //Only update angle if the player is not shooting.
                //TODO BAC-34: Rotate angle smoothely
                if (!LocalCell._isShooting)
                    LocalCell.transform.localEulerAngles = new Vector3(0, 0, Vector3.SignedAngle(Constants.DEFAULT_UNIT_DIRECTION, direction, Vector3.forward));

                //If there's a structure in the way, don't move
                var origin = LocalCell.transform.position;
                var distance = LocalCell.GetMovementSpeed() * GlobalContext.localDeltaTimeSec + LocalCell.GetComponent<SpriteRenderer>().size.x / 4;
                if (Physics2D.Raycast(origin, direction, distance, Constants.PATH_COLLISION_MASK))
                    return;

                //Else we want to move, but first we need to check collisions
                LocalCell._isMoving = true;
                direction = direction * LocalCell.GetMovementSpeed() * GlobalContext.localDeltaTimeSec;
                CheckLocalCellMovementCollision(direction);

                //If we are still moving, do the translate
                if (LocalCell._isMoving)
                {
                    LocalCell.transform.Translate(direction, Space.World);
                    CheckCellIsInTerritory();
                }
            }

            private void OnLocalPlayerMovementReleaseEvent()
            {
                LocalCell._isMoving = false;
            }

            private void OnLocalPlayerShootEvent(Vector2 direction)
            {
                //Local player should update direction here. Other players shouldn't get updated by bullet shooting. NetworkTransform will handle it for us
                LocalCell.transform.localEulerAngles = new Vector3(0, 0, Vector3.SignedAngle(Constants.DEFAULT_UNIT_DIRECTION, direction, Vector3.forward));

                //Prevent shooting if no ammunition
                if (LocalCell.Ammunition <= 0)
                    return;

                //First thing is checking if we weren't shooting, to update the shoot time so we can deal with frame skip
                if (!LocalCell._isShooting && GameContext.serverTimeMs >= LocalCell._nextShootTimeMs)
                    LocalCell._nextShootTimeMs = GameContext.serverTimeMs; //This guarantees it will shoot on the next if

                //Next we update the shooting flag
                LocalCell._isShooting = true;

                //Then we do the actual bullet shooting if the time is ok
                if (GameContext.serverTimeMs >= LocalCell._nextShootTimeMs)
                {
                    //Shoot and get delay afterwards. Since the shooting might affter the delay
                    var bullet = _mapCtrl._bulletCtrl.ShootBulletOwner(LocalCell, direction);

                    //notify network when we shoot a bullet            
                    NotifyShootEvent.SendShotBullet((byte)LocalCell._uniqueId._index, direction, bullet._uniqueId);
                }
            }

            private void OnLocalPlayerShootReleaseEvent()
            {
                LocalCell._isShooting = false;

                //Notify network that we've stopped shooting
                NotifyShootEvent.SendShootStop((byte)LocalCell._uniqueId._index);
            }


            //******************************************************************* Internal utility
            private void CheckLocalCellMovementCollision(Vector2 direction)
            {
                //TODO: Should do multiple raycasts according to cell size and minimum object size. This is to ensure we never miss an object when we move
                var radius = LocalCell.GetComponent<CircleCollider2D>().radius; //TODO: this should be a variable instead of get component
                var hits = Physics2D.RaycastAll(LocalCell.transform.position, direction, direction.magnitude + radius, Constants.CELL_MOVE_COLLISION_MASK);

                //No hits
                if (hits.Length <= 0)
                    return;

                //Else we have collisions
                for (int i = 0; i < hits.Length; i++)
                {
                    //check what we collided with
                    if (hits[i].collider.gameObject.layer == Constants.BACTERIA_LAYER)
                    {
                        WDebug.Log("Cell collided with bacteria");

                        var bacteria = hits[i].collider.GetComponent<Bacteria>();

                        //Always cancel movement if we knocked with a bacteria. Also set position so we don't move further than the collision point
                        LocalCell._isMoving = false;
                        LocalCell.transform.position = hits[i].point;

                        //Apply damage to cell first
                        DamageCell(LocalCell, bacteria.GetAttackPower());

                        //Always knock bacteria. If we're the host, knock bacteria immediatly, otherwise request host to knock it
                        var bacteriaToCellDir = (LocalCell.transform.position - bacteria.transform.position).normalized;
                        if (Network.NetworkInfo.IsHost)
                            _mapCtrl._bacteriaCtrl.KnockBacteria(bacteria, -bacteriaToCellDir, Constants.DEFAULT_KNOCKBACK_DISTANCE);
                        else
                            NetworkEvents.BacteriaEvents.RequestBacteriaDamageEvent.Send(bacteria, 0, -bacteriaToCellDir * Constants.DEFAULT_KNOCKBACK_DISTANCE);

                        //Then knock back cell if cell is still alive
                        if (LocalCell.IsAlive)                        
                            KnockCell(LocalCell, bacteriaToCellDir, Constants.DEFAULT_KNOCKBACK_DISTANCE);     

                    }
                    else if (hits[i].collider.gameObject.layer == Constants.PILLS_LAYER)
                    {
                        WDebug.Log("Cell collided with pill");

                        var pill = hits[i].collider.GetComponent<Pill>();

                        //If we're host, consume the pill right away. Otherwise request host to make us consume it.
                        if (Network.NetworkInfo.IsHost)
                        {
                            //trigger pill effect and despawn it
                            _mapCtrl._pillCtrl.TriggerPill(pill, LocalCell);
                            _mapCtrl._pillCtrl.DespawnPillServer(pill);
                        }
                        else
                        {
                            NetworkEvents.PillEvents.RequestCellConsumePill.Send(pill);
                            _mapCtrl._pillCtrl.DeactivatePillClient(pill);
                        }
                    }
                    else if (hits[i].collider.gameObject.layer == Constants.TRAPS_LAYER)
                    {
                        WDebug.Log("Cell collided with trap");

                        var trap = hits[i].collider.GetComponent<Trap>();

                        //Skip offensive traps that were layed down by other cells
                        if (trap._ownerCellIndex != Constants.INVALID_CELL_INDEX && GlobalContext.trapDb.GetTrapData(trap._dbId).trapType == Databases.TrapDb.TrapData.TrapType.Offensive)
                            continue;

                        //Host should trigger trap right away. Others should request
                        if (Network.NetworkInfo.IsHost)
                        {
                            _mapCtrl._trapCtrl.TriggerTrap(trap, BlockType.Cell, LocalCell);
                            _mapCtrl._trapCtrl.DespawnTrapServer(trap);
                        }
                        else
                        {
                            NetworkEvents.TrapEvents.RequestCellStepOnTrapEvent.Send(trap);
                            _mapCtrl._trapCtrl.DeactivateTrapClient(trap);
                        }
                    }
                    else
                    {
                        WDebug.LogWarn("Cell collided with unknown layer: " + hits[i].collider.gameObject.layer);
                    }
                }
            }

            private int CalcDamage(int attackPower, Cell cell)
            {
                WDebug.Log("Dealing " + Mathf.Max(attackPower - cell.GetDefense(), 0) + " damage to cell");
                return Mathf.Max(attackPower - cell.GetDefense(), 0);
            }

            private void CheckCellIsInTerritory()
            {
                if (!Physics.Raycast(LocalCell.transform.position, Vector3.forward, out RaycastHit hit, Constants.TERRITORY_Z_POS, Constants.TERRITORY_MASK))
                    return;

                WDebug.LogVerb("Cell is in territory");
            }

            private void KillLocalPlayer()
            {
                WDebug.Assert(LocalCell != null, "Got kill local player but we didn't have a local player yet");

                //Disable input while player is dead
                GameContext.inputHandler.IsPaused = true;
                                
                //Decrement lives
                LocalCell.Lives--;

                //Clients always show the death screen. Host only shows if any cell has lives remaining. 
                //If it's the host and nobody has lives left, the storyteller will end the game
                if (!Network.NetworkInfo.IsHost || CheckAnyCellRemainingLives())
                {
                    var respawnTime = LocalCell.Lives >= 0 ? Constants.DEFAULT_CELL_RESPAWN_TIME : Constants.INVALID_CELL_RESPAWN_TIME;
                    GameContext.uiController.ShowPlayerDeathScreen(respawnTime, RespawnLocalPlayer);
                }
            }
            
            private void RespawnLocalPlayer()
            {
                LocalCell.CurrentHp = LocalCell.GetMaxHp();
                LocalCell.transform.position = _mapCtrl._structureCtrl.GetSpawnPointNearHeart();
                LocalCell._nextBulletRegenMs = GlobalContext.localTimeMs + LocalCell.GetBulletRegenTime();
                GameContext.inputHandler.IsPaused = false;
            }

            private void OnLocalCellReady()
            {
                GlobalContext.cameraController.AssignTarget(LocalCell.transform);

                //If it's a reconnect, we could start dead
                if (!LocalCell.IsAlive)
                {
                    WDebug.Assert(!Network.NetworkInfo.IsHost, "Host started dead. Should never happen");
                    GameContext.inputHandler.IsPaused = true;

                    //If we still have to wait, start the timer to have us respawn. 
                    if (LocalCell._respawnTimeMs > 0)
                        GameContext.uiController.ShowPlayerDeathScreen((int)LocalCell._respawnTimeMs, RespawnLocalPlayer);
                    else //Otherwise just respawn
                        RespawnLocalPlayer();
                }
                else //Otherwise we start alive
                {
                    LocalCell._nextBulletRegenMs = GlobalContext.localTimeMs + LocalCell.GetBulletRegenTime();
                    GameContext.inputHandler.IsPaused = false;
                    GameContext.uiController.ConfigureUpgradeShopPanel(LocalCell, _mapCtrl._auraCtrl.AttachAuraToCellOwner, _mapCtrl._buffCtrl.ApplyBuff);
                }

                LocalCell.SendAllNotifications();

                //TODO: cancel loading screen when we have one
            }
            

            //******************************************************************* Network event handlers          
            private void OnOtherPlayerShootEventServer(NetworkConnection connection, NotifyShootEvent shootEvent) 
            {
                //Forward the event to clients, including host
                NetworkServer.SendToReady(shootEvent);
            }

            private void OnOtherPlayerShootEventClient(NotifyShootEvent shootEvent) 
            {
                //Local client already processed it. This will be called due to being broadcasted to all clients
                if (shootEvent.cellIndex == LocalCell._uniqueId._index)
                    return;

                //If we received a packet saying it is shooting. Spawn a bullet accordingly
                if (shootEvent.isShooting)
                    _mapCtrl._bulletCtrl.ShootBulletClient(_cells[shootEvent.cellIndex], shootEvent.direction, shootEvent.bulletUniqueId);
                else
                    _cells[shootEvent.cellIndex]._isShooting = false;
            }

            private void OnRequestCellDamageEventClient(RequestCellDamageEvent damageEvent)
            {
                //If we get here, then we have authority. Apply damage and knockback cell
                _mapCtrl._cellCtrl.DamageCell(LocalCell, damageEvent.damage);
                _mapCtrl._cellCtrl.KnockCell(LocalCell, damageEvent.knockForce, damageEvent.knockForce.magnitude);
            }

            private void OnNotifyAddUpgradePointsClient(NotifyAddUpgradePoints pointsEvent)
            {
                LocalCell.UpgradePoints += pointsEvent.additionalPoints;
            }

            //******************************************************************* Network player controller events
            private void OnCellSpawned(Network.NetworkPlayerObject obj)
            {
                WDebug.Assert(obj as Cell != null, "Can't cast spawned obj to cell");

                _cells[obj._uniqueId._index] = (Cell)obj;
                CellCount++;

                //Check if we already have authority over it
                if (obj.hasAuthority && LocalCell != null)
                    OnCellGotAuthority(obj);
            }

            private void OnCellDespawned(Network.NetworkPlayerObject obj)
            {
                WDebug.Assert(obj as Cell != null, "Can't cast despawned obj to cell");

                _cells[obj._uniqueId._index] = null;
                CellCount--;
            }

            private void OnCellGotAuthority(Network.NetworkPlayerObject obj)
            {
                WDebug.Assert(obj as Cell != null, "Can't cast authorityChanged obj to cell");

                WDebug.Log("Client: Got the player object with authority");
                LocalCell = (Cell)obj;
                GlobalContext.cameraController.AssignTarget(LocalCell.transform);
                _hasAuthority = true;

                //Only call local cell ready if we're already done with synching all the cells
                if (_isSynced)
                {
                    WDebug.Log("Client: Authority received after cells are synced. Starting gameplay");
                    _mapCtrl._bulletCtrl.UpdateOnReconnectLocalPlayerReady();
                    _mapCtrl._trapCtrl.UpdateOnReconnectLocalPlayerReady();
                    _mapCtrl._auraCtrl.UpdateOnReconnectLocalPlayerReady();
                    OnLocalCellReady();
                }
            }

            //******************************************************************* Syncing events
            private void OnRequestCellSyncServer(NetworkConnection connection, RequestCellSyncEvent syncEvent)
            {
                WDebug.Log("Received request to sync cells with a client");

                //Construct the list of cells that need to be synced
                ReplyCellSyncEvent reply;
                reply._cellSyncDatas = new CellSyncData[CellCount];
                int count = 0;

                //fill the sync data
                for (int i = 0; i < _cells.Length; i++)
                {
                    if (_cells[i] == null)
                        continue;

                    reply._cellSyncDatas[count]._cellId = _cells[i]._uniqueId;

                    //fill the status
                    reply._cellSyncDatas[count]._status = _cells[i].GetStatus();

                    //Fill others
                    //If respawn time is invalid, don't calculate
                    reply._cellSyncDatas[count]._remainingRespawnTimeMs = _cells[i]._respawnTimeMs - (_cells[i]._respawnTimeMs == Constants.INVALID_CELL_RESPAWN_TIME ? 0 : GameContext.serverTimeMs);

                    //fill the auras / traps / bullets that cell owns
                    reply._cellSyncDatas[count]._auras = _mapCtrl._auraCtrl.GetCellAuraSyncData(_cells[i]);
                    reply._cellSyncDatas[count]._traps = _mapCtrl._trapCtrl.GetCellTrapSyncData(_cells[i]);
                    reply._cellSyncDatas[count]._bullets = _mapCtrl._bulletCtrl.GetCellBulletSyncData(_cells[i]);
                    count++;
                }

                ReplyCellSyncEvent.Send(connection, ref reply);
            }

            private void OnReplyCellSyncClient(ReplyCellSyncEvent reply)
            {
                WDebug.Log("Received reply to sync cells with host");

                NetworkClient.UnregisterHandler<ReplyCellSyncEvent>();

                for (int i = 0; i < reply._cellSyncDatas.Length; i++)
                {
                    var data = reply._cellSyncDatas[i];
                    var cell = _cells[data._cellId._index];
                    WDebug.Assert(cell != null, "Received cell sync data but cell hasn't spawned yet: " + data._cellId._clientToken);

                    //Fill the status
                    cell.Configure(ref data._status);
                    cell._respawnTimeMs = data._remainingRespawnTimeMs;

                    //Create the auras / traps / bullets for this cell
                    _mapCtrl._auraCtrl.SetCellAurasFromSyncData(cell, ref data._auras);
                    _mapCtrl._trapCtrl.SetCellTrapsFromSyncData(cell, ref data._traps);
                    _mapCtrl._bulletCtrl.SetCellBulletsFromSyncData(cell, ref data._bullets);
                }

                _isSynced = true;

                //If we already received authority while synching, then start gameplay
                if (_hasAuthority)
                {
                    WDebug.Log("Client: sync finished after authority was received. Starting gameplay");
                    _mapCtrl._bulletCtrl.UpdateOnReconnectLocalPlayerReady();
                    _mapCtrl._trapCtrl.UpdateOnReconnectLocalPlayerReady();
                    _mapCtrl._auraCtrl.UpdateOnReconnectLocalPlayerReady();
                    OnLocalCellReady();
                }
            }

        }
    }
}

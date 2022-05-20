using System.Collections.Generic;
using UnityEngine;
using Mirror;

using Bacterio.MapObjects;
using Bacterio.Common;
using Bacterio.NetworkEvents.TrapEvents;

namespace Bacterio.Game
{
    public sealed partial class MapController : System.IDisposable
    {
        private sealed partial class TrapController : System.IDisposable
        {
            private sealed partial class TrapTrigger { }

            private readonly MapController _mapCtrl = null;
            private readonly ObjectPool<Trap> _trapPool = null;

            //Trap logic is the same as Auras
            private readonly BlockArray<Trap>[] _cellTraps = null;
            private readonly BlockArray<Trap> _bacteriaTraps = null;

            private readonly TrapTrigger _trapTriggers = null;

            public TrapController(MapController mapController)
            {
                _mapCtrl = mapController;

                var trapObj = GlobalContext.assetBundleProvider.LoadObjectAsset("Trap");
                WDebug.Assert(trapObj != null, "No Trap object in bundle");

                _trapPool = new ObjectPool<Trap>(trapObj.GetComponent<Trap>(), Constants.TRAP_POOL_INITIAL_SIZE, Constants.TRAP_POOL_GROWTH_AMOUNT, Constants.OUT_OF_RANGE_POSITION, Quaternion.identity, null);
                _bacteriaTraps = new BlockArray<Trap>(Constants.TRAP_BACTERIA_INITIAL_AMOUNT, Constants.TRAP_BACTERIA_GROWTH_AMOUNT);
                _cellTraps = new BlockArray<Trap>[Constants.MAX_PLAYERS];
                _trapTriggers = new TrapTrigger(this);

                for (int i = 0; i < Constants.MAX_PLAYERS; i++)
                    _cellTraps[i] = new BlockArray<Trap>(Constants.TRAP_CELL_INITIAL_AMOUNT, Constants.TRAP_CELL_GROWTH_AMOUNT);

                //Register handlers
                if (Network.NetworkInfo.IsHost)
                {
                    NetworkServer.RegisterHandler<TrapSpawnedEvent>(OnCellTrapSpawnedServer);
                    NetworkServer.RegisterHandler<RequestCellStepOnTrapEvent>(OnCellStepOnTrapServer);
                }
                NetworkClient.RegisterHandler<TrapSpawnedEvent>(OnTrapSpawnedClient);
                NetworkClient.RegisterHandler<TrapDespawnedEvent>(OnTrapDespawnedClient);
                NetworkClient.RegisterHandler<NotifyTrapTriggerEvent>(OnNotifyTrapTriggerClient);
            }

            public void CleanupRemotePlayer(Network.NetworkPlayerObject player)
            {
                WDebug.Assert(_cellTraps.Length > player._uniqueId._index && _cellTraps[player._uniqueId._index] != null, "Got cleanup for unexistent player");
                _cellTraps[player._uniqueId._index].Dispose();
            }

            public void UpdateOnReconnectLocalPlayerReady()
            {

            }

            public void Dispose()
            {
                //Unregister handlers
                if (Network.NetworkInfo.IsHost)
                {
                    NetworkServer.UnregisterHandler<TrapSpawnedEvent>();
                    NetworkServer.UnregisterHandler<RequestCellStepOnTrapEvent>();
                }
                NetworkClient.UnregisterHandler<TrapSpawnedEvent>();
                NetworkClient.UnregisterHandler<TrapDespawnedEvent>();
                NetworkClient.UnregisterHandler<NotifyTrapTriggerEvent>();

                //Destroy all bullets
                _trapPool.Dispose();

                _bacteriaTraps.Dispose();
                for (int i = 0; i < _cellTraps.Length; i++)
                    _cellTraps[i].Dispose();
            }

            //******************************************************************* Public utility methods that can be called from any controller
            public void SpawnTrapCellOwner(Cell cell, Vector2 position, Databases.TrapDbId dbId)
            {
                WDebug.Assert(cell.hasAuthority, "Called spawnTrap from a cell with no authority");

                ref var trapData = ref GlobalContext.trapDb.GetTrapData(dbId);

                var trap = _trapPool.Pop();
                trap.transform.localPosition = position + Vector2.one * 2;
                trap._dbId = dbId;
                trap._ownerCellIndex = cell._uniqueId._index;
                trap._ownerBacteriaId = Network.NetworkArrayObject.UniqueId.invalid;
                trap._attackPower = cell.GetAttackPower() * (trapData.multiplier / 100);
                trap.gameObject.SetActive(true);
                _cellTraps[cell._uniqueId._index].Add(trap);

                //Broadcast in network
                TrapSpawnedEvent.Send(cell, trap);

                //For now do nothing with the owner
            }

            public void SpawnTrapBacteriaServer(Bacteria bacteria, Vector2 position, Databases.TrapDbId dbId)
            {
                WDebug.Assert(Network.NetworkInfo.IsHost, "Called spawnTrap bacteria from a client");

                ref var trapData = ref GlobalContext.trapDb.GetTrapData(dbId);

                var trap = _trapPool.Pop();
                trap.transform.localPosition = position;
                trap._dbId = dbId;
                trap._ownerCellIndex = Constants.INVALID_CELL_INDEX;
                trap._ownerBacteriaId = bacteria._uniqueId;
                trap._attackPower = bacteria.GetAttackPower() * (trapData.multiplier / 100);
                trap.gameObject.SetActive(true);
                _bacteriaTraps.Add(trap);

                //Broadcast in network
                TrapSpawnedEvent.Send(bacteria, trap);

                //For now do nothing with the owner
            }

            public void DespawnTrapServer(Trap trap)
            {
                //Notify clients to despawn trap
                TrapDespawnedEvent.Send(trap);

                //Despawn it locally
                WDebug.Log("Removing trap index: " + trap._uniqueId.arrayIndex + " position: " + trap.transform.position);
                trap.transform.position = Constants.OUT_OF_RANGE_POSITION;
                trap.gameObject.SetActive(false);

                if (trap._ownerCellIndex != Constants.INVALID_CELL_INDEX)
                    _cellTraps[trap._ownerCellIndex].Remove(trap);
                else
                    _bacteriaTraps.Remove(trap);

                trap._ownerCellIndex = Constants.INVALID_CELL_INDEX;
                trap._ownerBacteriaId = Network.NetworkArrayObject.UniqueId.invalid;

                _trapPool.Push(trap);
            }

            public void DeactivateTrapClient(Trap trap)
            {
                trap.gameObject.SetActive(false);
            }

            public void TriggerTrap(Trap trap, BlockType targetType, MonoBehaviour target)
            {
                WDebug.Assert(target != null, "TriggerTrap called with a null target");

                //If host, notify trigger in network
                if (Network.NetworkInfo.IsHost)
                {
                    if(targetType == BlockType.Cell)
                        NotifyTrapTriggerEvent.Send(trap, (Cell)target);
                    else
                        NotifyTrapTriggerEvent.Send(trap, (Bacteria)target);
                }

                switch (trap._dbId)
                {
                    case Databases.TrapDbId.ExplodingTrap: _trapTriggers.TriggerExplodingTrap(trap, targetType, target); break;
                }

                WDebug.Log("Triggered trap: " + trap._uniqueId.arrayIndex + " at target type: " + targetType + " id: " + (targetType == BlockType.Cell ? ((Cell)target)._uniqueId._clientToken : ((Bacteria)target)._uniqueId.index));
            }


            //******************************************************************* Internal Utility methods
            private void SpawnCellTrapClient(Cell cell, Vector2 position, Block.UniqueId trapId, Databases.TrapDbId dbId, int attackPower)
            {
                var trap = _trapPool.Pop();
                trap.transform.localPosition = position;
                trap._dbId = dbId;
                trap._ownerCellIndex = cell._uniqueId._index;
                trap._ownerBacteriaId = Network.NetworkArrayObject.UniqueId.invalid;
                trap._attackPower = attackPower;
                trap.gameObject.SetActive(true);
                _cellTraps[cell._uniqueId._index].Insert(trap, trapId);

                //For now do nothing with the owner
            }

            private void SpawnBacteriaTrapClient(Bacteria bacteria, Vector2 position, Block.UniqueId trapId, Databases.TrapDbId dbId, int attackPower)
            {
                var trap = _trapPool.Pop();
                trap.transform.localPosition = position;
                trap._dbId = dbId;
                trap._ownerCellIndex = Constants.INVALID_CELL_INDEX;
                trap._ownerBacteriaId = bacteria._uniqueId;
                trap._attackPower = attackPower;
                _bacteriaTraps.Insert(trap, trapId);

                //For now do nothing with the owner
            }

            private void DespawnedTrapClient(Trap trap)
            {
                //Despawn it locally
                WDebug.Log("Removing trap index: " + trap._uniqueId.arrayIndex + " position: " + trap.transform.position);
                trap.transform.position = Constants.OUT_OF_RANGE_POSITION;
                trap.gameObject.SetActive(false);

                if (trap._ownerCellIndex != Constants.INVALID_CELL_INDEX)
                    _cellTraps[trap._ownerCellIndex].Remove(trap);
                else
                    _bacteriaTraps.Remove(trap);

                trap._ownerCellIndex = Constants.INVALID_CELL_INDEX;
                trap._ownerBacteriaId = Network.NetworkArrayObject.UniqueId.invalid;

                _trapPool.Push(trap);
            }

            //******************************************************************* Network event handlers
            private void OnCellTrapSpawnedServer(NetworkConnection connection, TrapSpawnedEvent trapSpawnedEvent)
            {
                //Although this event arrives from a TrapSpawnedEvent, it must have been called by a cell since it came from a client
                WDebug.Assert(trapSpawnedEvent.cellOwnerIndex != Constants.INVALID_CELL_INDEX, "Invalid cell index in TrapSpawnedEvent even though it came from client");
                WDebug.Assert(Network.NetworkInfo.IsHost, "OnTrapSpawnedServer was called on a client");

                //Forward event to all clients, including the host
                NetworkServer.SendToReady(trapSpawnedEvent);
            }

            private void OnTrapSpawnedClient(TrapSpawnedEvent trapSpawnedEvent)
            {
                //spawned by a cell
                if(trapSpawnedEvent.cellOwnerIndex != Constants.INVALID_CELL_INDEX)
                {
                    var cell = _mapCtrl._cellCtrl.GetCell(trapSpawnedEvent.cellOwnerIndex);                
                    
                    // Null could happen if player disconnects. Also if cell has authority, means we got this event from a broadcast started by us, so we can ignore it
                    if (cell == null || cell.hasAuthority)
                        return;

                    SpawnCellTrapClient(cell, trapSpawnedEvent.position, trapSpawnedEvent.trapId, trapSpawnedEvent.dbId, trapSpawnedEvent.attackPower);
                }
                else //Spawned by a bacteria
                {
                    //Host is the one that spawns bacteria traps. Ignore
                    if (Network.NetworkInfo.IsHost)
                        return;

                    var bacteria = _mapCtrl._bacteriaCtrl.GetBacteria(trapSpawnedEvent.bacteriaOwnerId);
                    WDebug.Assert(bacteria != null, "Server sent us a bacteria spawned trap event, but bacteria does not exist");

                    SpawnBacteriaTrapClient(bacteria, trapSpawnedEvent.position, trapSpawnedEvent.trapId, trapSpawnedEvent.dbId, trapSpawnedEvent.attackPower);
                }
            }

            private void OnTrapDespawnedClient(TrapDespawnedEvent trapEvent)
            {
                //Host is the one that despawns it. Ignore
                if (Network.NetworkInfo.IsHost)
                    return;

                WDebug.Assert(trapEvent.cellOwnerIndex == Constants.INVALID_CELL_INDEX || _mapCtrl._cellCtrl.GetCell(trapEvent.cellOwnerIndex) != null, "Got a trap despawned, owned by an unexistent cell");
                var trap = trapEvent.cellOwnerIndex != Constants.INVALID_CELL_INDEX ? _cellTraps[trapEvent.cellOwnerIndex].GetFromId(trapEvent.trapId) : _bacteriaTraps.GetFromId(trapEvent.trapId);

                DespawnedTrapClient(trap);
            }

            private void OnCellStepOnTrapServer(NetworkConnection connection, RequestCellStepOnTrapEvent stepOnEvent)
            {
                WDebug.Assert(Network.NetworkInfo.IsHost, "Requested stepOn on client");

                var cellTarget = (Cell)_mapCtrl._networkPlayerController.GetFromConnection(connection);
                WDebug.Assert(cellTarget != null, "Received invalid cell step on trap");

                var trapArray = stepOnEvent.cellOwnerIndex != Constants.INVALID_CELL_INDEX ? _cellTraps[stepOnEvent.cellOwnerIndex] : _bacteriaTraps;

                //Could happen if trap expired or another thing stepped on it
                if (!trapArray.Contains(stepOnEvent.trapId))
                    return;

                //Get trap from cell traps or bacteria traps accordingly
                var trap = trapArray.GetFromId(stepOnEvent.trapId);

                TriggerTrap(trap, BlockType.Cell, cellTarget);
                DespawnTrapServer(trap);
            }

            private void OnNotifyTrapTriggerClient(NotifyTrapTriggerEvent trapEvent)
            {
                //Host is the one that notifies it. Ignore
                if (Network.NetworkInfo.IsHost)
                    return;

                WDebug.Assert(trapEvent.cellOwnerIndex == Constants.INVALID_CELL_INDEX || _mapCtrl._cellCtrl.GetCell(trapEvent.cellOwnerIndex) != null, "Got a trap trigger notify, owned by an unexistent cell");

                Trap trap;
                //If trap is owned by a cell
                if (trapEvent.cellOwnerIndex != Constants.INVALID_CELL_INDEX)                
                    trap = _cellTraps[trapEvent.cellOwnerIndex].GetFromId(trapEvent.trapId);                
                else //Else is owned by bacteria                
                    trap = _bacteriaTraps.GetFromId(trapEvent.trapId);
                WDebug.Assert(trap != null, "Got trigger trap of non existent trap");

                //If the trigger is by a cell
                if (trapEvent.triggerCellIndex != Constants.INVALID_CELL_INDEX)
                {
                    var cellTarget = _mapCtrl._cellCtrl.GetCell(trapEvent.triggerCellIndex);
                    WDebug.Assert(cellTarget != null, "Got trigger trap with non existent cell target");
                    TriggerTrap(trap, BlockType.Cell, cellTarget);
                }
                else //Else is a trigger by bacteria
                {
                    var bacteriaTarget = _mapCtrl._bacteriaCtrl.GetBacteria(trapEvent.triggerBacteriaId);
                    WDebug.Assert(bacteriaTarget != null, "Got trigger trap with non existent bacteria");
                    TriggerTrap(trap, BlockType.Bacteria, bacteriaTarget);
                }                
            }


            //******************************************************************* Syncing events
            public CellTrapSyncData[] GetCellTrapSyncData(Cell cell)
            {
                var array = _cellTraps[cell._uniqueId._index];
                CellTrapSyncData[] data = new CellTrapSyncData[array.Count];
                int count = 0;

                //fill the sync data
                ref var traps = ref array.GetAll();
                for (int i = 0; i <= array.LastIndex; i++)
                {
                    if (traps[i] == null)
                        continue;

                    data[count].trapId = traps[i]._uniqueId;
                    data[count].dbId = traps[i]._dbId;
                    data[count].position = traps[i].transform.position;
                    data[count].attackPower = traps[i]._attackPower;
                    count++;
                }

                return data;
            }

            public void SetCellTrapsFromSyncData(Cell cell, ref CellTrapSyncData[] syncData)
            {
                //There is a possibility that we already received some traps before the sync arrived, so we need to check for that. The oposite isn't possible
                for (int i = 0; i < syncData.Length; i++)
                {
                    //Only spawn trap if it doesn't exist yet
                    if (!_cellTraps[cell._uniqueId._index].Contains(syncData[i].trapId))
                        SpawnCellTrapClient(cell, syncData[i].position, syncData[i].trapId, syncData[i].dbId, syncData[i].attackPower);
                }
            }

            public BacteriaTrapSyncData[] GetBacteriaTrapSyncData()
            {
                BacteriaTrapSyncData[] data = new BacteriaTrapSyncData[_bacteriaTraps.Count];
                int count = 0;

                //fill the sync data
                ref var traps = ref _bacteriaTraps.GetAll();
                for (int i = 0; i <= _bacteriaTraps.LastIndex; i++)
                {
                    if (traps[i] == null)
                        continue;

                    data[count].trapId = traps[i]._uniqueId;
                    data[count].dbId = traps[i]._dbId;
                    data[count].ownerBacteriaId = traps[i]._ownerBacteriaId;
                    data[count].position = traps[i].transform.position;
                    data[count].attackPower = traps[i]._attackPower;
                    count++;
                }

                return data;
            }

            public void SetBacteriaTrapsFromSyncData(ref BacteriaTrapSyncData[] syncData)
            {
                if (syncData == null)
                    return;

                //There is a possibility that we already received some traps before the sync arrived, so we need to check for that. The oposite isn't possible
                for (int i = 0; i < syncData.Length; i++)
                {
                    WDebug.Assert(_mapCtrl._bacteriaCtrl.GetBacteria(syncData[i].ownerBacteriaId), "Got sync data trap for inexistent bacteria");
                    //Only attach if trap doesnt exist yet
                    if (!_bacteriaTraps.Contains(syncData[i].trapId))
                    {
                        var bacteria = _mapCtrl._bacteriaCtrl.GetBacteria(syncData[i].ownerBacteriaId);
                        WDebug.Assert(bacteria != null, "got sync data trap for a non existent bacteria");

                        //Only spawn trap if it doesn't exist yet
                        if (!_bacteriaTraps.Contains(syncData[i].trapId))
                            SpawnBacteriaTrapClient(bacteria, syncData[i].position, syncData[i].trapId, syncData[i].dbId, syncData[i].attackPower);
                    }
                }
            }
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

using Bacterio.MapObjects;
using Bacterio.Common;
using Bacterio.NetworkEvents.AuraEvents;

namespace Bacterio.Game
{
    public sealed partial class MapController : System.IDisposable
    {
        private sealed partial class AuraController : System.IDisposable
        {
            private sealed partial class AuraTriggers { }

            private readonly MapController _mapCtrl = null;
            private readonly ObjectPool<Aura> _auraPool = null;
                        
            //The logic between controlling auras is the same as bullets. But it should be significantly easier to understand since auras dont move
            //Unlike bullets, Aura disappearence is broadcasted by the owner, so we do not have to worry about used IDs as long as packet ordering is guaranteed
            private readonly BlockArray<Aura>[] _cellAuras = null;
            private readonly BlockArray<Aura> _bacteriaAuras = null;
            private readonly AuraTriggers _auraTriggers = null;
            private float _localCellLargestRadius = 0;

            public AuraController(MapController mapController)
            {
                _mapCtrl = mapController;


                var auraObj = GlobalContext.assetBundleProvider.LoadObjectAsset("Aura");
                WDebug.Assert(auraObj != null, "No aura object from bundle");

                _auraPool = new ObjectPool<Aura>(auraObj.GetComponent<Aura>(), Constants.AURA_POOL_INITIAL_SIZE, Constants.AURA_POOL_GROWTH_AMOUNT, Constants.OUT_OF_RANGE_POSITION, Quaternion.identity, null);
                _bacteriaAuras = new BlockArray<Aura>(Constants.AURA_BACTERIA_INITIAL_AMOUNT, Constants.AURA_BACTERIA_GROWTH_AMOUNT);
                _cellAuras = new BlockArray<Aura>[Constants.MAX_PLAYERS];
                _auraTriggers = new AuraTriggers(this);

                for (int i = 0; i < Constants.MAX_PLAYERS; i++)
                    _cellAuras[i] = new BlockArray<Aura>(Constants.AURA_CELL_INITIAL_AMOUNT, Constants.AURA_CELL_GROWTH_AMOUNT);

                //Register handlers
                if (Network.NetworkInfo.IsHost)
                {
                    Mirror.NetworkServer.RegisterHandler<CellAuraAttachedEvent>(OnCellAuraAttachedHost);
                }
                Mirror.NetworkClient.RegisterHandler<CellAuraAttachedEvent>(OnCellAuraAttachedClient);
            }

            public void UpdateBacteriaAuras()
            {
                //Process bacteria auras
                ref var auras = ref _bacteriaAuras.GetAll();
                for (int i = 0; i <= _bacteriaAuras.LastIndex; i++)
                {
                    if (auras[i] == null)
                        continue;

                    var bacteria = auras[i].transform.parent.GetComponent<Bacteria>();
                    CheckBacteriaAuraCollision(bacteria, auras[i]);
                }
            }

            public void UpdateLocalCellAuras()
            {
                var localCell = _mapCtrl._cellCtrl.LocalCell;
                WDebug.Assert(_localCellLargestRadius != 0, "Cells must always have at least 1 aura (To check wound range)");

                //Get all the colliders in range
                var colliders = Physics2D.OverlapCircleAll(localCell.transform.position, _localCellLargestRadius, Constants.AURA_COLLISION_MASK);

                //If we cought nothing
                if (colliders.Length <= 0)
                    return;

                //Precalculate calculate collider distances only once. This way multiple auras only check against this array
                var distances = new float[colliders.Length];
                for (int i = 0; i < colliders.Length; i++)
                    distances[i] = Vector2.Distance(localCell.transform.position, colliders[i].transform.position);

                //Go through all auras and process the colliders
                var index = localCell._uniqueId._index;
                ref var auras = ref _cellAuras[index].GetAll();
                for (int i = 0; i <= _cellAuras[index].LastIndex; i++)
                {
                    if (auras[i] == null)
                        continue;

                    CheckLocalCellAuraCollision(localCell, auras[i], ref colliders, ref distances);
                }
            }

            public void UpdateOnReconnectLocalPlayerReady()
            {
                //Calculate the largest radius
                var index = _mapCtrl._cellCtrl.LocalCell._uniqueId._index;
                ref var auras = ref _cellAuras[index].GetAll();
                for (int i = 0; i <= _cellAuras[index].LastIndex; i++)
                {
                    if (auras[i]._radius > _localCellLargestRadius)
                        _localCellLargestRadius = auras[i]._radius;
                }
            }

            public void CleanupRemotePlayer(Network.NetworkPlayerObject player)
            {
                WDebug.Assert(_cellAuras.Length > player._uniqueId._index && _cellAuras[player._uniqueId._index] != null, "Got cleanup for unexistent player");
                _cellAuras[player._uniqueId._index].Dispose();
            }

            public void Dispose()
            {
                //Unregister handlers
                if (Network.NetworkInfo.IsHost)
                {
                    Mirror.NetworkServer.UnregisterHandler<CellAuraAttachedEvent>();
                }
                Mirror.NetworkClient.UnregisterHandler<CellAuraAttachedEvent>();

                //Destroy all bullets
                _auraPool.Dispose();

                _bacteriaAuras.Dispose();
                for (int i = 0; i < _cellAuras.Length; i++)
                    _cellAuras[i].Dispose();
            }

         
            //******************************************************************* Public utility methods
            public void AttachAuraToCellOwner(Cell cell, Databases.AuraDbId dbId)
            {
                WDebug.Assert(cell.hasAuthority, "Tried to attach aura to other cell without authority");

                ref var auraData = ref GlobalContext.auraDb.GetAuraData(dbId);

                var aura = _auraPool.Pop();
                aura._dbId = dbId;
                aura._radius = auraData.radius; //default for now
                aura._ownerBlockType = BlockType.Cell;
                aura._owner = cell;
                aura.gameObject.SetActive(auraData.IsVisible); //An invisible aura is just for logic that will run on it's own update. It does not need to show the renderer or trigger bullets
                aura.GetComponent<CircleCollider2D>().enabled = auraData.IsInteractable;

                //Update local largest
                if (aura._radius > _localCellLargestRadius)
                    _localCellLargestRadius = aura._radius;

                //set the position
                aura.transform.SetParent(cell.transform, false);
                aura.transform.localPosition = Vector3.zero;
                aura.transform.localScale = new Vector3(4, 4, 1);
                _cellAuras[cell._uniqueId._index].Add(aura);

                //For now notify even invisible auras
                CellAuraAttachedEvent.Send(cell, aura);
            }

            /// <returns>Returns weather to stop processing aura or not</returns>
            public bool TriggerAura(Aura aura, Cell owner, Collider2D collider)
            {
                switch(aura._dbId)
                {
                    case Databases.AuraDbId.CellWoundDetection: _auraTriggers.TriggerCellWoundDetection(aura, owner, collider); break;

                    //If aura doesn't have a trigger, fallthrough to return true and stop processing aura
                    case Databases.AuraDbId.Test:
                    default: return true;
                }

                //If we get here then we executed a trigger that didn't have a return.
                //if the trigger has no return value, then it doesn't care if it needs to stop processing, we can assume it doesnt need to
                return false;
            }

            public bool TriggerAura(Aura aura, Bacteria owner, Collider2D collider)
            {
                switch (aura._dbId)
                {

                    //If aura doesn't have a trigger, fallthrough to return true and stop processing aura
                    case Databases.AuraDbId.CellWoundDetection:
                    case Databases.AuraDbId.Test:
                    default: return true;
                }

                //If we get here then we executed a trigger that didn't have a return.
                //if the trigger has no return value, then it doesn't care if it needs to stop processing, we can assume it doesnt need to
                return false;
            }

            public bool TriggerAura(Aura aura, Bullet bullet)
            {
                switch(aura._dbId)
                {
                    //If aura doesn't have a trigger, fallthrough to return true and stop processing aura
                    case Databases.AuraDbId.CellWoundDetection:
                    case Databases.AuraDbId.Test:
                    default: return true;
                }

                //If we get here then we executed a trigger that didn't have a return.
                //if the trigger has no return value, then it doesn't care if it needs to stop processing, we can assume it doesnt need to
                return false;
            }


            //******************************************************************* Internal Utility methods
            private void CheckBacteriaAuraCollision(Bacteria owner, Aura aura)
            {
                //TODO: radius should be a obtained from somewhere else instead of doing get component all the time
                float radius = aura.GetComponent<CircleCollider2D>().radius * aura.transform.localScale.x;

                //Bacteria auras should be mostly interactable / visible so we can still afford to always pay the raycast. We can optimize it out later if needed
                var colliders = Physics2D.OverlapCircleAll(aura.transform.position, radius, Constants.AURA_COLLISION_MASK);
                WDebug.Assert(colliders.Length >= 1, "Didn't even catch ourselves in the aura radius");

                //If we only caught ourselves in it.
                if (colliders.Length == 1)
                    return;

                //Else we have collisions
                for (int i = 0; i < colliders.Length; i++)                
                    if (TriggerAura(aura, owner, colliders[i]))
                        return;                
            }

            private void CheckLocalCellAuraCollision(Cell localCell, Aura aura, ref Collider2D[] colliders, ref float[] distances)
            {
                ref var auraData = ref GlobalContext.auraDb.GetAuraData(aura._dbId);
                for (int i = 0; i < colliders.Length; i++)
                {
                    //Skip objects that are not relevant to the aura or not within the range of the aura
                    var affected = auraData.mask & (1 << colliders[i].gameObject.layer);
                    if (affected == 0 || distances[i] > aura._radius)
                        continue;

                    //If trigger tells us to stop processing, then we stop and move on to next aura
                    if (TriggerAura(aura, localCell, colliders[i]))
                        return;
                }
            }

        
            //****************************************************************** Internal client-only Methods
            private void AttachAuraToCellClient(Cell cell, Block.UniqueId uniqueId, Databases.AuraDbId dbId)
            {
                WDebug.Assert(!cell.hasAuthority, "Tried to attach aura to other cell with authority");

                ref var auraData = ref GlobalContext.auraDb.GetAuraData(dbId);

                var aura = _auraPool.Pop();
                aura._dbId = dbId;
                aura._radius = auraData.radius;
                aura._ownerBlockType = BlockType.Cell;
                aura._owner = cell;
                aura.gameObject.SetActive(auraData.IsVisible); //An invisible aura is just for logic that will run on it's own update. It does not need to show the renderer or trigger bullets
                aura.GetComponent<CircleCollider2D>().enabled = auraData.IsInteractable;

                aura.transform.SetParent(cell.transform, false);
                aura.transform.localPosition = Vector3.zero;
                aura.transform.localScale = new Vector3(4, 4, 1);
                _cellAuras[cell._uniqueId._index].Insert(aura, uniqueId);
            }

        
            //******************************************************************* Network Event handlers
            private void OnCellAuraAttachedHost(Mirror.NetworkConnection connection, CellAuraAttachedEvent auraEvent)
            {
                WDebug.Assert(Network.NetworkInfo.IsHost, "OnCellAuraAttachedHost on a client");

                //Forward event to all clients, including the host
                Mirror.NetworkServer.SendToReady(auraEvent);
            }

            private void OnCellAuraAttachedClient(CellAuraAttachedEvent auraEvent)
            {
                var cell = _mapCtrl._cellCtrl.GetCell(auraEvent.cellIndex);

                // Null could happen if player disconnects. Also if cell has authority, means we got this event from a broadcast started by us, so we can ignore it
                if (cell == null || cell.hasAuthority)
                    return;

                AttachAuraToCellClient(cell, auraEvent.auraId, auraEvent.dbId);
            }

            
            //******************************************************************* Syncing methods
            public CellAuraSyncData[] GetCellAuraSyncData(Cell cell)
            {
                var array = _cellAuras[cell._uniqueId._index];
                CellAuraSyncData[] data = new CellAuraSyncData[array.Count];
                int count = 0;

                //fill the sync data
                ref var auras = ref array.GetAll();
                for (int i = 0; i <= array.LastIndex; i++)
                {
                    if (auras[i] == null)
                        continue;

                    data[count].auraId = auras[i]._uniqueId;
                    data[count].dbId = auras[i]._dbId;
                    count++;
                }

                return data;
            }

            public void SetCellAurasFromSyncData(Cell cell, ref CellAuraSyncData[] syncData)
            {
                //There is a possibility that we already received some auras before the sync arrived, so we need to check for that. The oposite isn't possible
                for (int i = 0; i < syncData.Length; i++)
                {
                    //Only attach if aura doesnt exist yet
                    if (!_cellAuras[cell._uniqueId._index].Contains(syncData[i].auraId))
                    {
                        AttachAuraToCellClient(cell, syncData[i].auraId, syncData[i].dbId);
                    }
                }
            }

            public BacteriaAuraSyncData[] GetBacteriaAuraSyncData()
            {
                BacteriaAuraSyncData[] data = new BacteriaAuraSyncData[_bacteriaAuras.Count];
                int count = 0;

                //fill the sync data
                ref var auras = ref _bacteriaAuras.GetAll();
                for (int i = 0; i <= _bacteriaAuras.LastIndex; i++)
                {
                    if (auras[i] == null)
                        continue;

                    //TODO: set owner
                    data[count].auraId = auras[i]._uniqueId;
                    data[count].dbId = auras[i]._dbId;
                    count++;
                }

                return data;
            }

            public void SetBacteriaAurasFromSyncData(ref BacteriaAuraSyncData[] syncData)
            {
                if (syncData == null)
                    return;
                //There is a possibility that we already received some auras before the sync arrived, so we need to check for that. The oposite isn't possible
                for (int i = 0; i < syncData.Length; i++)
                {
                    WDebug.Assert(_mapCtrl._bacteriaCtrl.GetBacteria(syncData[i]._bacteriaId), "Got sync data aura for inexistent bacteria");
                    //Only attach if aura doesnt exist yet
                    if (!_bacteriaAuras.Contains(syncData[i].auraId))
                    {
                        //TODO: Attach to bacteria
                    }
                }
            }
        }
    }
}
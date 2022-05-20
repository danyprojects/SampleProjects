using UnityEngine;
using Mirror;

using Bacterio.MapObjects;
using Bacterio.Common;
using Bacterio.NetworkEvents.PillEvents;

namespace Bacterio.Game
{
    public sealed partial class MapController : System.IDisposable
    {
        private sealed partial class PillController : System.IDisposable
        {
            private sealed partial class PillTriggers { }

            private readonly MapController _mapCtrl = null;
            private readonly ObjectPool<Pill> _pillPool = null;
            private readonly BlockArray<Pill> _pillArray = null;
            private readonly PillTriggers _pillTriggers = null;

            public PillController(MapController mapController)
            {
                _mapCtrl = mapController;


                var pillObj = GlobalContext.assetBundleProvider.LoadObjectAsset("Pill");
                WDebug.Assert(pillObj != null, "No pill object in bundle");

                _pillPool = new ObjectPool<Pill>(pillObj.GetComponent<Pill>(), Constants.PILL_POOL_INITIAL_SIZE, Constants.PILL_POOL_GROWTH_AMOUNT, Constants.OUT_OF_RANGE_POSITION, Quaternion.identity, null);
                _pillArray = new BlockArray<Pill>(Constants.PILL_INITIAL_AMOUNT, Constants.PILL_GROWTH_AMOUNT);
                _pillTriggers = new PillTriggers(this);

                //Register event handlers
                NetworkClient.RegisterHandler<PillSpawnedEvent>(OnPillSpawnedClient);
                NetworkClient.RegisterHandler<PillDespawnedEvent>(OnPillDespawnedClient);
                NetworkClient.RegisterHandler<NotifyCellTriggeredPill>(OnNotifyCellTriggeredPillClient);
                if (Network.NetworkInfo.IsHost)
                {
                    NetworkServer.RegisterHandler<RequestPillFullSyncEvent>(OnRequestPillFullSyncServer);
                    NetworkServer.RegisterHandler<RequestCellConsumePill>(OnRequestCellConsumePillServer);
                }
            }

            public void RunReconnectSync()
            {
                WDebug.Assert(!Network.NetworkInfo.IsHost, "Requesting sync pills with host");

                NetworkClient.RegisterHandler<ReplyPillFullSyncEvent>(OnReplyPillFullSyncEventClient);
                NetworkEvents.EmptyEvent.Send<RequestPillFullSyncEvent>();
            }

            public void Dispose()
            {
                //Unregister handlers
                NetworkClient.UnregisterHandler<PillSpawnedEvent>();
                NetworkClient.UnregisterHandler<PillDespawnedEvent>();
                NetworkClient.UnregisterHandler<ReplyPillFullSyncEvent>(); //only registered on reconnect
                NetworkClient.UnregisterHandler<NotifyCellTriggeredPill>();

                if (Network.NetworkInfo.IsHost)
                {
                    NetworkServer.UnregisterHandler<RequestPillFullSyncEvent>();
                    NetworkServer.UnregisterHandler<RequestCellConsumePill>();
                }

                _pillPool.Dispose();
                _pillArray.Dispose();
            }

            //******************************************************************* Public utility methods that can be called from any controller
            public void SpawnPillServer(Vector2 position, Databases.PillDbId dbId)
            {
                WDebug.Assert(Network.NetworkInfo.IsHost, "Spawned a pill locally in client");

                var pill = _pillPool.Pop();
                pill._dbId = dbId;
                pill.transform.localPosition = position;
                pill.gameObject.SetActive(true);
                _pillArray.Add(pill);

                PillSpawnedEvent.Send(pill, position);
            }

            public void DespawnPillServer(Pill pill)
            {
                //Notify clients to despawn pill
                PillDespawnedEvent.Send(pill);

                //Despawn it locally
                WDebug.Log("Removing pill index: " + pill._uniqueId.arrayIndex + " position: " + pill.transform.position);
                pill.transform.position = Constants.OUT_OF_RANGE_POSITION;
                pill.gameObject.SetActive(false);
                _pillArray.Remove(pill);
                _pillPool.Push(pill);
            }

            public void DeactivatePillClient(Pill pill)
            {
                pill.gameObject.SetActive(false);
            }

            public void TriggerPill(Pill pill, Cell cell)
            {
                //Notify trigger in network if host
                if (Network.NetworkInfo.IsHost)
                    NotifyCellTriggeredPill.Send(cell, pill);

                switch(pill._dbId)
                {
                    case Databases.PillDbId.MovementSpeed: _pillTriggers.TriggerMovementSpeedPill(cell, pill); break;
                }

                WDebug.Log("Triggered pill: " + pill._uniqueId.arrayIndex + " in cell: " + cell._uniqueId._clientToken);
            }

            //******************************************************************* Internal Utility methods
            private void SpawnPillClient(Vector2 position, Block.UniqueId pillId, Databases.PillDbId dbId)
            {
                WDebug.Assert(!Network.NetworkInfo.IsHost, "Spawned a pill from network in host");

                var pill = _pillPool.Pop();
                pill.transform.localPosition = position;
                pill._dbId = dbId;
                _pillArray.Insert(pill, pillId);
            }

            private void DespawnPillClient(Pill pill)
            {
                //Despawn it locally
                pill.transform.position = Constants.OUT_OF_RANGE_POSITION;
                pill.gameObject.SetActive(false);
                _pillArray.Remove(pill);
                _pillPool.Push(pill);
            }


            //******************************************************************* Network Event handlers
            private void OnPillSpawnedClient(PillSpawnedEvent pillEvent)
            {
                //Host is the one that spawns the pills, ignore this event
                if (Network.NetworkInfo.IsHost)
                    return;

                SpawnPillClient(pillEvent.position, pillEvent.pillId, pillEvent.dbId);
            }

            private void OnPillDespawnedClient(PillDespawnedEvent pillEvent)
            {
                //Host is the one that despawns the pills, ignore this event
                if (Network.NetworkInfo.IsHost)
                    return;

                var pill = _pillArray.GetFromId(pillEvent.pillId);
                DespawnPillClient(pill);
            }

            private void OnRequestCellConsumePillServer(NetworkConnection connection, RequestCellConsumePill pillEvent)
            {
                WDebug.Assert(_mapCtrl._networkPlayerController.GetFromConnection(connection) != null, "Got a request from a connection but didn't find the cell for it");
                var cell = (Cell)_mapCtrl._networkPlayerController.GetFromConnection(connection);

                //Could happen if client sends this before processing the server despawn
                if (!_pillArray.Contains(pillEvent.pillId))
                    return;

                var pill = _pillArray.GetFromId(pillEvent.pillId);

                TriggerPill(pill, cell);
                DespawnPillServer(pill);
            }

            private void OnNotifyCellTriggeredPillClient(NotifyCellTriggeredPill pillEvent)
            {
                //Host is the one that notifies, skip
                if (Network.NetworkInfo.IsHost)
                    return;

                WDebug.Assert(_pillArray.GetFromId(pillEvent.pillId) != null, "Host sent a pill consume but pill didnt exit");
                var pill = _pillArray.GetFromId(pillEvent.pillId);

                var cell = _mapCtrl._cellCtrl.GetCell(pillEvent.cellIndex);

                TriggerPill(pill, cell);
            }

            //******************************************************************* Syncing events
            private void OnRequestPillFullSyncServer(NetworkConnection connection, RequestPillFullSyncEvent syncEvent)
            {
                //ready the event
                ReplyPillFullSyncEvent replyEvent;
                replyEvent.pillIds = new Block.UniqueId[_pillArray.Count];
                replyEvent.dbIds = new Databases.PillDbId[_pillArray.Count];
                replyEvent.positions = new Vector2[_pillArray.Count];
                int replyPillCount = 0;

                //fill the event
                ref var pills = ref _pillArray.GetAll();
                for (int i = 0; i <= _pillArray.LastIndex; i++)
                {
                    if (pills[i] == null)
                        continue;

                    replyEvent.pillIds[replyPillCount] = pills[i]._uniqueId;
                    replyEvent.dbIds[replyPillCount] = pills[i]._dbId;
                    replyEvent.positions[replyPillCount] = pills[i].transform.localPosition;
                    replyPillCount++;
                }

                connection.Send(replyEvent);
            }

            private void OnReplyPillFullSyncEventClient(ReplyPillFullSyncEvent reply)
            {
                //Spawn all the pills
                for (int i = 0; i < reply.pillIds.Length; i++)
                    SpawnPillClient(reply.positions[i], reply.pillIds[i], reply.dbIds[i]);
                
                //Unregister, shouldn't need this again
                NetworkClient.UnregisterHandler<ReplyPillFullSyncEvent>();
            }
        }
    }
}
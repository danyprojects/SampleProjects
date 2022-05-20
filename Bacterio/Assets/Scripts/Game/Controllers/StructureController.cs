using System.Collections.Generic;
using UnityEngine;

using Bacterio.MapObjects;
using Bacterio.Common;
using Bacterio.NetworkEvents.StructureEvents;

namespace Bacterio.Game
{
    public sealed partial class MapController : System.IDisposable
    {
        private sealed partial class StructureController : System.IDisposable
        {
            private readonly MapController _mapCtrl = null;
            private readonly ObjectPool<Structure> _structurePool = null;
            private readonly ObjectPool<Territory> _territoryPool = null;
            private readonly BlockArray<Structure> _structureArray = null;

            public StructureController(MapController mapController)
            {
                _mapCtrl = mapController;

                var structureObj = GlobalContext.assetBundleProvider.LoadObjectAsset("Structure");
                var territoryObj = GlobalContext.assetBundleProvider.LoadObjectAsset("Territory");
                WDebug.Assert(structureObj != null, "No Structure object in bundle");
                WDebug.Assert(territoryObj != null, "No Territory object in bundle");

                _structurePool = new ObjectPool<Structure>(structureObj.GetComponent<Structure>(), Constants.STRUCTURE_POOL_INITIAL_SIZE, Constants.STRUCTURE_POOL_GROWTH_AMOUNT, Constants.OUT_OF_RANGE_POSITION, Quaternion.identity, null);
                _territoryPool = new ObjectPool<Territory>(territoryObj.GetComponent<Territory>(), Constants.STRUCTURE_POOL_INITIAL_SIZE, Constants.STRUCTURE_POOL_GROWTH_AMOUNT, Constants.OUT_OF_RANGE_POSITION, Quaternion.identity, null);
                _structureArray = new BlockArray<Structure>(Constants.STRUCTURE_INITIAL_AMOUNT, Constants.STRUCTURE_GROWTH_AMOUNT);

                Mirror.NetworkClient.RegisterHandler<StructureSpawnedEvent>(OnStructureSpawnedClient);
                Mirror.NetworkClient.RegisterHandler<StructureDespawnedEvent>(OnStructureDespawnedClient);
                Mirror.NetworkClient.RegisterHandler<StructureWithTerritorySpawnedEvent>(OnStructureWithTerritorySpawnedClient);
                Mirror.NetworkClient.RegisterHandler<TerritoryDeformed>(OnTerritoryDeformedClient);
                if (Network.NetworkInfo.IsHost)
                {
                    Mirror.NetworkServer.RegisterHandler<RequestStructureFullSyncEvent>(OnRequestStructureFullSyncServer);
                    Mirror.NetworkServer.RegisterHandler<RequestWoundHealEvent>(OnRequestWoundHealEventServer);
                }
            }

            public void Update()
            {
                WDebug.Assert(Network.NetworkInfo.IsHost, "Only host should update structures");

                ref var structures = ref _structureArray.GetAll();
                for (int i = 0; i <= _structureArray.LastIndex; i++)
                {
                    if (structures[i] == null)
                        continue;

                    if(structures[i]._territory != null)
                        UpdateTerritory(structures[i]._territory);

                    if (structures[i]._isSpawner)
                        UpdateSpawnerStructure(structures[i]);
                }
            }

            public void RunReconnectSync()
            {
                WDebug.Assert(!Network.NetworkInfo.IsHost, "Requesting sync structures with host");

                Mirror.NetworkClient.RegisterHandler<ReplyStructureFullSyncEvent>(OnReplyStructureFullSyncEvent);
                NetworkEvents.EmptyEvent.Send<RequestStructureFullSyncEvent>();
            }

            public void Dispose()
            {
                Mirror.NetworkClient.UnregisterHandler<StructureSpawnedEvent>();
                Mirror.NetworkClient.UnregisterHandler<ReplyStructureFullSyncEvent>();
                Mirror.NetworkClient.UnregisterHandler<StructureDespawnedEvent>();
                Mirror.NetworkClient.UnregisterHandler<TerritoryDeformed>();

                if (Network.NetworkInfo.IsHost)
                {
                    Mirror.NetworkServer.UnregisterHandler<RequestStructureFullSyncEvent>();
                    Mirror.NetworkServer.UnregisterHandler<RequestWoundHealEvent>();
                }

                _structurePool.Dispose();
                _structureArray.Dispose();
                _territoryPool.Dispose();
            }

       
            //******************************************************************* Public utility methods
            /// <summary> Spawns a structure and the territory if it should have one </summary>
            public void SpawnStructureServer(Vector2 position, Databases.StructureDbId dbId)
            {
                WDebug.Assert(Network.NetworkInfo.IsHost, "Spawned a structure locally in client");

                ref var structureData = ref GlobalContext.structureDb.GetStructureData(dbId);

                //Spawn structure
                var structure = _structurePool.Pop();
                structure.Configure(dbId, position);
                _structureArray.Add(structure);

                //TODO: temporary. Change when we have the real way of setting structure image
                structure.GetComponent<SpriteRenderer>().sprite = GlobalContext.assetBundleProvider.LoadUIImageAsset<Sprite>(structureData._spriteName);

                //Spawn territory and notify structure spawn with territory
                if (structureData._hasTerritory)
                {
                    structure._territory = SpawnStructureTerritory(structure);
                    StructureWithTerritorySpawnedEvent.Send(structure, structure._territory);
                }
                else //Notify structure spawn without territory
                    StructureSpawnedEvent.Send(structure);

                if (dbId == Databases.StructureDbId.Wound)
                    GameContext.gameStatus.ActiveWoundCount++;
            }
           
            public void DespawnStructureServer(Structure structure)
            {
                //Notify clients to despawn structure
                StructureDespawnedEvent.Send(structure);

                //Then despawn it locally. Starting with territory
                if (structure._territory != null)
                    DespawnStructureTerritory(structure);

                structure.transform.position = Constants.OUT_OF_RANGE_POSITION;
                _structureArray.Remove(structure);
                _structurePool.Push(structure);

                if (structure._dbId == Databases.StructureDbId.Wound)
                    GameContext.gameStatus.ActiveWoundCount--;
            }

            public void HealWound(Structure wound, int healPower)
            {
                WDebug.Assert(wound._dbId == Databases.StructureDbId.Wound, "Called heal wound for a structure that is not a wound");
                WDebug.Assert(Network.NetworkInfo.IsHost, "Called heal wound but we're not host");

                wound._woundData.hp -= healPower;
                WDebug.Log("Healed wound id: " + wound._uniqueId.arrayIndex + ", new HP: " + wound._woundData.hp);

                //Wound is fully healed, destroy it
                if (wound._woundData.hp <= 0)
                    DespawnStructureServer(wound);
            }
         
            public Vector2 GetSpawnPointNearHeart()
            {
                //TODO: this should be a spawn point near a respawn point, and not the heart
                var direction = Random.insideUnitCircle;

                return direction + direction.normalized * Constants.MIN_HEART_RADIUS;
            }

      
            //******************************************************************* Internal utility methods
            private void UpdateTerritory(Territory territory)
            {
                WDebug.Assert(Network.NetworkInfo.IsHost, "Updated territory from client");

                if (GlobalContext.localTimeMs >= territory._nextDeformMs)
                {
                    territory.DeformOutwards();
                    territory._nextDeformMs += Constants.DEFAULT_TERRITORY_DEFORM_INTERVAL;

                    //notify network about the deform
                    TerritoryDeformed.Send(territory._structure, true);
                }
            }

            private Territory SpawnStructureTerritory(Structure structure, int seed = Constants.INVALID_SEED)
            {
                var territory = _territoryPool.Pop();
                territory._structure = structure;
                territory._nextDeformMs = GlobalContext.localTimeMs + Constants.DEFAULT_TERRITORY_DEFORM_INTERVAL;

                if (seed == Constants.INVALID_SEED)
                    territory._random = new MTRandom();
                else
                    territory._random = new MTRandom(seed);

                //Because the seed is shared, as long as this is the first thing we do, every client will end up with the same initial mesh
                var name = GlobalContext.structureDb.GetTerritoryMeshName(territory._random);
                territory.Configure(GlobalContext.assetBundleProvider.LoadMiscAsset<Mesh>(name));

                territory.transform.position = Vector3.zero;
                territory.transform.SetParent(structure.transform, false);
                territory.gameObject.SetActive(true);
                return territory;
            }

            private void DespawnStructureTerritory(Structure structure)
            {
                var territory = structure._territory;
                structure._territory = null;
                territory._structure = null;

                territory.gameObject.SetActive(false);
                territory.transform.position = Constants.OUT_OF_RANGE_POSITION;
                territory.transform.SetParent(null, false);

                _territoryPool.Push(territory);
            }

            private void UpdateSpawnerStructure(Structure structure)
            {
                if (GlobalContext.localTimeMs < structure._spawnerData.nextBacteriaSpawnMs)
                    return;

                //Spawn a bacteria

                var direction = _mapCtrl._random.PointInACircle();
                var offset = direction.normalized * Constants.MIN_BACTERIA_SPAWN_RADIUS;
                offset.x += structure.transform.position.x;
                offset.y += structure.transform.position.y;

                _mapCtrl._bacteriaCtrl.SpawnBacteriaServer(direction + offset);
                structure._spawnerData.nextBacteriaSpawnMs = GlobalContext.localTimeMs + Constants.DEFAULT_BACTERIA_SPAWN_INTERVAL;
                
            }

            //****************************************************************** Internal client-only Methods
            private void SpawnStructureClient(Databases.StructureDbId dbId, Vector2 position, Block.UniqueId structureId)
            {
                WDebug.Assert(!Network.NetworkInfo.IsHost, "Spawned a structure from network in host");
                var structure = _structurePool.Pop();
                structure.Configure(dbId, position);
                _structureArray.Insert(structure, structureId);

                ref var structureData = ref GlobalContext.structureDb.GetStructureData(dbId);
                //TODO: temporary. Change when we have the real way of setting structure image
                structure.GetComponent<SpriteRenderer>().sprite = GlobalContext.assetBundleProvider.LoadUIImageAsset<Sprite>(structureData._spriteName);

                if (dbId == Databases.StructureDbId.Wound)
                    GameContext.gameStatus.ActiveWoundCount++;
            }

            private void SpawnStructureWithTerritoryClient(Databases.StructureDbId dbId, Vector2 position, Block.UniqueId structureId, int seed, int deformCount)
            {
                WDebug.Assert(!Network.NetworkInfo.IsHost, "Spawned a structure from network in host");
                var structure = _structurePool.Pop();
                structure.Configure(dbId, position);
                _structureArray.Insert(structure, structureId);

                ref var structureData = ref GlobalContext.structureDb.GetStructureData(dbId);
                //TODO: temporary. Change when we have the real way of setting structure image
                structure.GetComponent<SpriteRenderer>().sprite = GlobalContext.assetBundleProvider.LoadUIImageAsset<Sprite>(structureData._spriteName);

                //spawn territory with given seed
                structure._territory = SpawnStructureTerritory(structure, seed);

                //Apply the deforms to match it as the server does
                for (int i = 0; i < deformCount; i++)
                    structure._territory.DeformOutwards();

                if (dbId == Databases.StructureDbId.Wound)
                    GameContext.gameStatus.ActiveWoundCount++;
            }
           
            private void DespawnStructureClient(Structure structure)
            {
                //Ddelete it locally. Starting with territory
                if (structure._territory != null)
                    DespawnStructureTerritory(structure);

                structure.transform.position = Constants.OUT_OF_RANGE_POSITION;
                _structureArray.Remove(structure);
                _structurePool.Push(structure);

                if (structure._dbId == Databases.StructureDbId.Wound)
                    GameContext.gameStatus.ActiveWoundCount--;
            }


            //******************************************************************* Network Event handlers
            private void OnStructureSpawnedClient(StructureSpawnedEvent structureEvent)
            {
                //Host is the one that spawns the structures, ignore this event
                if (Network.NetworkInfo.IsHost)
                    return;

                SpawnStructureClient(structureEvent.dbId, structureEvent.position, structureEvent.structureId);
            }

            private void OnStructureDespawnedClient(StructureDespawnedEvent structureEvent)
            {
                //Host is the one that despawns the structures, ignore this event
                if (Network.NetworkInfo.IsHost)
                    return;

                var structure = _structureArray.GetFromId(structureEvent.structureId);

                if (structure != null)
                    DespawnStructureClient(structure);
            }

            private void OnStructureWithTerritorySpawnedClient(StructureWithTerritorySpawnedEvent structureEvent)
            {
                //Host is the one that spawns the structures, ignore this event
                if (Network.NetworkInfo.IsHost)
                    return;

                SpawnStructureWithTerritoryClient(structureEvent.dbId, structureEvent.position, structureEvent.structureId, structureEvent.seed, 0);
            }

            private void OnTerritoryDeformedClient(TerritoryDeformed territoryEvent)
            {
                //Host is the one that sent this, ignore this event
                if (Network.NetworkInfo.IsHost)
                    return;

                var structure = _structureArray.GetFromId(territoryEvent.structureId);

                WDebug.Assert(structure != null, "Received territoryDeform event for non existing structure");
                WDebug.Assert(structure._territory != null, "Received territoryDeform event for structure without territory");

                if (territoryEvent.isOutwards)
                    structure._territory.DeformOutwards();
                else
                    structure._territory.DeformInwards();
            }

            private void OnRequestWoundHealEventServer(Mirror.NetworkConnection connection, RequestWoundHealEvent woundEvent)
            {
                var wound = _structureArray.GetFromId(woundEvent.woundId);

                //maybe wound is gone already
                if (wound == null)
                    return;

                HealWound(wound, woundEvent.healPower);
            }


            //******************************************************************* Syncing methods
            private void OnRequestStructureFullSyncServer(Mirror.NetworkConnection connection, RequestStructureFullSyncEvent syncEvent)
            {
                //ready the event
                ReplyStructureFullSyncEvent replyEvent;
                replyEvent.structureIds = new Block.UniqueId[_structureArray.Count];
                replyEvent.positions = new Vector2[_structureArray.Count];
                replyEvent.dbIds = new Databases.StructureDbId[_structureArray.Count];
                replyEvent.seeds = new int[_structureArray.Count];
                replyEvent.deforms = new short[_structureArray.Count];

                int replyStructureCount = 0;

                //fill the event
                ref var structures = ref _structureArray.GetAll();
                for (int i = 0; i <= _structureArray.LastIndex; i++)
                {
                    if (structures[i] == null)
                        continue;

                    replyEvent.structureIds[replyStructureCount] = structures[i]._uniqueId;
                    replyEvent.positions[replyStructureCount] = structures[i].transform.localPosition;
                    replyEvent.dbIds[replyStructureCount] = structures[i]._dbId;
                    replyEvent.seeds[replyStructureCount] = structures[i]._territory != null ? structures[i]._territory._random.Seed : Constants.INVALID_SEED;
                    replyEvent.deforms[replyStructureCount] = (short)(structures[i]._territory != null ? structures[i]._territory._deformCount : 0);
                    replyStructureCount++;
                }

                connection.Send(replyEvent);
            }

            private void OnReplyStructureFullSyncEvent(ReplyStructureFullSyncEvent reply)
            {
                //Spawn all the structures
                for (int i = 0; i < reply.structureIds.Length; i++)
                {
                    //spawn structure with or without territory, using the seed as the decider
                    if (reply.seeds[i] == Constants.INVALID_SEED)
                        SpawnStructureClient(reply.dbIds[i], reply.positions[i], reply.structureIds[i]);
                    else
                        SpawnStructureWithTerritoryClient(reply.dbIds[i], reply.positions[i], reply.structureIds[i], reply.seeds[i], reply.deforms[i]);
                }

                //Unregister, shouldn't need this again
                Mirror.NetworkClient.UnregisterHandler<ReplyStructureFullSyncEvent>();
            }
        }
    }
}
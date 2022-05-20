using UnityEngine;
using Bacterio.Common;
using Bacterio.Input;
using Bacterio.UI;
using Bacterio.Lobby;
using Bacterio.Network;

namespace Bacterio.Game
{
    public sealed partial class MapController : System.IDisposable
    {
        /// <summary>
        /// The storyteller is responsible for controller when wounds and pills spawn, as well as the current rate and difficulty of the game, which will affect already spawned wounds and bacteria
        /// </summary>
        private sealed partial class StoryTeller
        {
            private MapController _mapCtrl;

            private MTRandom _random = null;

            //Variables set once at the start
            private readonly int _startWoundCount = 2;
            private readonly int _startPillCount = 2;
            private readonly int _totalWounds = 5;
            private readonly int _totalPills = 5;

            //State variables that change with flow of the game
            private int _remainingWoundCount;
            private int _woundSpawnRadius;

            //timestamps
            private int _nextPillSpawnMs = 0;
            private int _nextWoundSpawnMs = 0;

            public StoryTeller(MapController mapController)
            {
                _mapCtrl = mapController;
                _random = new MTRandom();

                //Do nothing if it's a client
                if (!NetworkInfo.IsHost)
                    return;

                //TODO: load starting config

                //Spawn the heart
                _mapCtrl._structureCtrl.SpawnStructureServer(Vector2.zero, Databases.StructureDbId.Heart);

                //Spawn the starting pills
                for (int i = 0; i < _startPillCount; i++)                
                    SpawnPillRandomPosition(Databases.PillDbId.MovementSpeed);

                //Spawn the starting wounds
                for (int i = 0; i < _startWoundCount; i++)
                    SpawnWoundRandomPosition();

                //Set the timestamps to spawn next pills / wounds
                _nextWoundSpawnMs = GlobalContext.localTimeMs + Constants.DEFAULT_WOUND_SPAWN_INTERVAL;
                _nextPillSpawnMs = GlobalContext.localTimeMs + Constants.DEFAULT_PILL_SPAWN_INTERVAL;

                //Set other variables
                _remainingWoundCount = _totalWounds - _startWoundCount;

                //Register to game status events
                GameStatus.ActiveWoundCountChanged += OnWoundCountChanged;

                //Register to other events
                MapObjects.Cell.LivesChanged += OnLivesChanged;
            }

            public void Update()
            {
                WDebug.Assert(NetworkInfo.IsHost, "Only host should run update of storyteller");

                if(_remainingWoundCount > 0 && GlobalContext.localTimeMs >= _nextWoundSpawnMs)
                {
                    SpawnWoundRandomPosition();
                    _remainingWoundCount--;
                    _nextWoundSpawnMs = GlobalContext.localTimeMs + Constants.DEFAULT_WOUND_SPAWN_INTERVAL;
                }

                if (GlobalContext.localTimeMs >= _nextPillSpawnMs)
                {
                    SpawnPillRandomPosition(Databases.PillDbId.MovementSpeed);
                    _nextPillSpawnMs = GlobalContext.localTimeMs + Constants.DEFAULT_PILL_SPAWN_INTERVAL;
                }
            }

            public void Dispose()
            {
                GameStatus.ActiveWoundCountChanged -= OnWoundCountChanged;
                MapObjects.Cell.LivesChanged -= OnLivesChanged;
            }

            //******************************************************************* Internal Utility methods
            private void SpawnWoundRandomPosition()
            {
                var direction = _random.PointInACircle();
                var offset = direction.normalized * (Constants.MAX_HEART_RADIUS + Constants.DEFAULT_TERRITORY_RADIUS); //So we don't spawn wounds within range of heart + the territory radius
                _mapCtrl._structureCtrl.SpawnStructureServer(offset + direction * _woundSpawnRadius, Databases.StructureDbId.Wound);
            }

            private void SpawnPillRandomPosition(Databases.PillDbId pillDbId)
            {
                var direction = _random.PointInACircle();
                var offset = direction.normalized * Constants.MIN_PILL_SPAWN_RADIUS;
                _mapCtrl._pillCtrl.SpawnPillServer(offset + direction * Constants.DEFAULT_PILL_SPAWN_RADIUS, pillDbId);
            }

            //******************************************************************* Callbacks for general stuff
            private void OnWoundCountChanged(int newWoundCount)
            {
                if (newWoundCount == 0 && _remainingWoundCount == 0)
                    GameContext.gameStatus.EndResult = GameEndResult.Win;
            }

            private void OnLivesChanged(MapObjects.Cell cell, int newLives)
            {
                //If we have negative lives, someone ran out of lives. So we should check if everyone is out to end the game. If nobody has lives, end the game
                if (newLives < 0 && !_mapCtrl._cellCtrl.CheckAnyCellRemainingLives())
                    GameContext.gameStatus.EndResult = GameEndResult.Lose;
            }
        }
    }
}

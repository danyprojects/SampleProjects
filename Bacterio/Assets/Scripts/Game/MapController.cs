using System;
using UnityEngine;
using Bacterio.Common;
using Bacterio.Input;
using Bacterio.UI;
using Bacterio.Lobby;
using Bacterio.Network;

namespace Bacterio.Game
{
    public sealed partial class MapController : IDisposable
    {
        //Nested controllers
        private sealed partial class CellController { };
        private sealed partial class BacteriaController { };
        private sealed partial class BulletController { };
        private sealed partial class PillController { };
        private sealed partial class TrapController { };
        private sealed partial class AuraController { };
        private sealed partial class StructureController { };
        private sealed partial class BuffController { };
        private sealed partial class StoryTeller { };

        private readonly GameSetupData _gameSetupData = null;
        private readonly NetworkPlayerController _networkPlayerController = null;
        private readonly Pathfinder _pathfinder = null;
        private readonly MapGrid _mapGrid = null;
        private readonly MTRandom _random = null;

        //Nested controllers
        private readonly CellController _cellCtrl = null;
        private readonly BacteriaController _bacteriaCtrl = null;
        private readonly BulletController _bulletCtrl = null;
        private readonly PillController _pillCtrl = null;
        private readonly TrapController _trapCtrl = null;
        private readonly AuraController _auraCtrl = null;
        private readonly StructureController _structureCtrl = null;
        private readonly BuffController _buffCtrl = null;
        private readonly StoryTeller _storyteller = null;

        public MapController( GameSetupData gameSetupData, NetworkPlayerController networkPlayerController)
        {
            _gameSetupData = gameSetupData;
            _networkPlayerController = networkPlayerController;
            _pathfinder = new Pathfinder();
            _mapGrid = new MapGrid();
            _random = new MTRandom();

            bool isReconnect = _networkPlayerController.Count == 0; // in a reconnect we never have any players yet
            WDebug.Assert(!isReconnect || !NetworkInfo.IsHost, "Reconnect triggered in host!");

            //instantiate other controllers. Cell should be last
            _buffCtrl = new BuffController(this);
            _bulletCtrl = new BulletController(this);
            _bacteriaCtrl = new BacteriaController(this);
            _structureCtrl = new StructureController(this);
            _pillCtrl = new PillController(this);
            _trapCtrl = new TrapController(this);
            _auraCtrl = new AuraController(this);
            _cellCtrl = new CellController(this, isReconnect);

            //Storyteller should be the last thing
            _storyteller = new StoryTeller(this);

            //TEST ********** Should only be true for connects from Room. Run the tests
            if (_networkPlayerController.Count > 0)
            {
                _auraCtrl.AttachAuraToCellOwner(_cellCtrl.LocalCell, Databases.AuraDbId.CellWoundDetection);
                _auraCtrl.AttachAuraToCellOwner(_cellCtrl.LocalCell, Databases.AuraDbId.Test);
            }
        }

        public void SynchronizeOnReconnect()
        {
            _pillCtrl.RunReconnectSync();
            _structureCtrl.RunReconnectSync();
            _cellCtrl.RunReconnectSync();
            _bacteriaCtrl.RunReconnectSync();
            //Bullets, auras and traps don't need to run sync, they will be synced by the bacteria and cells
        }

        public void UpdateHost()
        {
            //Do the same as update client + the auras, structures and bacteria
            UpdateClient();

            _auraCtrl.UpdateBacteriaAuras(); //Process bacteria auras
            _structureCtrl.Update(); //To update territories
            _bacteriaCtrl.Update(); //To run AI

            _storyteller.Update();
        }

        public void UpdateClient()
        {
            _cellCtrl.Update(); //Process LocalCell, will also process owned auras
            _bulletCtrl.Update(); //Bullets are only client sided. Loop can be the same
        }

        public void CleanupRemotePlayer(NetworkPlayerObject player)
        {
            //forward to controllers that keep data related to the player
            _bulletCtrl.CleanupRemotePlayer(player);
            _trapCtrl.CleanupRemotePlayer(player);
            _auraCtrl.CleanupRemotePlayer(player);
            _cellCtrl.CleanupRemotePlayer(player);
        }

        public void Dispose()
        {
            //Call dispose on non MonoBehavior objects
            _auraCtrl.Dispose();
            _structureCtrl.Dispose();
            _trapCtrl.Dispose();
            _pillCtrl.Dispose();
            _bulletCtrl.Dispose();
            _bacteriaCtrl.Dispose();
            _storyteller.Dispose();
        }
    }
}

using System;
using System.Collections.Generic;
using Bacterio.MapObjects;
using Bacterio.UI.Screens;

namespace Bacterio.Lobby
{
    //Contains data for all modes, even if not all modes use the same
    public sealed class GameSetupData
    {
        public event Action DataUpdated;

        private struct DataChangeMessage : Mirror.NetworkMessage
        {
            public GameMode selectedGameMode;
            public int testValue;
        }

        public GameMode SelectedGameMode { get; private set; } = GameMode.None;

        public int TestValue { get; private set; } = 0;


        public GameSetupData(GameMode initialMode)
        {
            SelectedGameMode = initialMode;

            Mirror.NetworkClient.RegisterHandler<DataChangeMessage>(OnNetworkReceiveDataChange);
        }

        //***************************************** Mehtods to change settings
        public void ChangeTestValue(int newValue)
        {
            WDebug.Assert(Network.NetworkInfo.IsHost, "Only host can change setup data!");

            TestValue = newValue;

            //Update in network
            UpdateDataOnNetwork();

            //Update UI
            DataUpdated?.Invoke();
        }
        
        //***************************************** Network setupData events
        private void UpdateDataOnNetwork() //TODO: This can be an enum instead of full data if it starts getting too much data
        {
            //Update in the network. This will also send to ourselves.
            var dataChange = new DataChangeMessage()
            {
                selectedGameMode = SelectedGameMode,
                testValue = TestValue
            };
            Mirror.NetworkServer.SendToAll(dataChange);
        }

        private void OnNetworkReceiveDataChange(DataChangeMessage dataChange)
        {
            //Host already has the updated info
            if (Network.NetworkInfo.IsHost)
                return;

            SelectedGameMode = dataChange.selectedGameMode;
            TestValue = dataChange.testValue;

            DataUpdated?.Invoke();
        }

        public void Destroy()
        {
            Mirror.NetworkClient.UnregisterHandler<DataChangeMessage>();
        }
    }
}

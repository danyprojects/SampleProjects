using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Bacterio.UI.Entries;

namespace Bacterio.UI.Screens
{
    public sealed class RoomFinderScreen : MonoBehaviour
    {
        [SerializeField] private RoomFinderEntry _roomEntry = null;
        [SerializeField] private Button _createRoomButton = null;
        [SerializeField] private Button _refreshButton = null;
        [SerializeField] private Button _joinRoomButton = null;

        private Common.ObjectPool<RoomFinderEntry> _roomEntryPool = null;
        private List<RoomFinderEntry> _roomEntries = null;
        private Vector3 _firstEntryPos = Vector3.zero;
        private Vector3 _entryOffset = Vector3.zero;

        public Lobby.RoomFinder RoomFinder { get; set; }

        public void SetActive(bool isActive)
        {
            GetComponent<Canvas>().enabled = isActive;
            this.enabled = isActive;

            WDebug.Assert(RoomFinder != null, "No room finder in roomfinderscreen");
            if (isActive)
            {
                RoomFinder.SetDiscoveryStatus(isActive);
                RoomFinder.RefreshRooms();
            }
            else
            {
                for (int i = 0; i < _roomEntries.Count; i++)
                    GameObject.Destroy(_roomEntries[i].gameObject);
                _roomEntries.Clear();
                _roomEntryPool.Dispose();
                RoomFinder.ClearRooms();
            }
        }

        private void Awake()
        {
            RoomFinder.RoomAdded += OnRoomAdded;
            _entryOffset = new Vector3(0, _roomEntry.GetComponent<RectTransform>().rect.height + 10, 0);

            //_joinRoomButton.onClick.AddListener(() => { OnJoinRoomSubmit(); });
            _createRoomButton.onClick.AddListener(() => 
                {
                    ScreenController.ShowInputPopup("Room Name", "Submit", OnCreateRoomSubmit, "Room");
                });

            _roomEntries = new List<RoomFinderEntry>(); 
            _roomEntryPool = new Common.ObjectPool<RoomFinderEntry>(_roomEntry, 0, 1, _roomEntry.transform.position, Quaternion.identity, _roomEntry.transform.parent, OnEntryPush, null);
            _firstEntryPos = _roomEntry.transform.position;
        }

        private void OnCreateRoomSubmit(string roomName)
        {
            RoomFinder.CreateLANRoom(roomName);
            ScreenController.HideInputPopup();
        }

        private void OnRoomEntryClick(Lobby.Room room)
        {
            RoomFinder.JoinRoom(room);
        }

        private void OnRoomAdded(Lobby.Room room)
        {
            //Get an entry and configure it
            var entry = _roomEntryPool.Pop();
            entry.Configure(OnRoomEntryClick, room);

            //Set the position before adding to the list so the calculations are correct
            entry.transform.localPosition += _entryOffset * _roomEntries.Count;
            _roomEntries.Add(entry);

            //Now enable it after configuring everything
            entry.gameObject.SetActive(true);
        }

        private void OnEntryPush(RoomFinderEntry entry)
        {
            entry.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            RoomFinder.RoomAdded -= OnRoomAdded;
        }
    }
}

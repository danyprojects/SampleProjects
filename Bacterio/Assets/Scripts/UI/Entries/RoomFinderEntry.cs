using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Bacterio.UI.Entries
{
    public sealed class RoomFinderEntry : MonoBehaviour
    {
        [SerializeField] private Button _entryButton;
        [SerializeField] private TextMeshProUGUI _roomNameText;
        [SerializeField] private Image _roomLocked;

        private void Awake()
        {
        }

        public void Configure(Action<Lobby.Room> clickCb, Lobby.Room room)
        {
            _roomNameText.text = room.RoomName;
            _entryButton.onClick.RemoveAllListeners();
            _entryButton.onClick.AddListener(() => clickCb(room));
        }
    }
}

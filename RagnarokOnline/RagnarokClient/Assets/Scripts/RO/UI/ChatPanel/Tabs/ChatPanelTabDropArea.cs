using UnityEngine;

namespace RO.UI
{
    public partial class ChatPanelController : UIController.Panel
    {
        // We use this for leftover tab area to detect subchat drop events
        public class TabDropArea : MonoBehaviour,
            IDropHandler
        {
            private ChatPanelController _controller;

            private void Awake()
            {
                _controller = GetComponentInParent<ChatPanelController>();
            }

            public void OnDrop(PointerEventData eventData)
            {
                if (CanvasFilter.HasFlag(CanvasFilter.DragSubChat))
                {
                    var dropIndex = _controller._droppedChatIndex;

                    if (_controller.OnAttachChat(dropIndex, _controller._attachedActiveChatsCount))
                        _controller._droppedChatIndex = -1;
                }
            }
        }
    }

    public class ChatPanelTabDropArea : ChatPanelController.TabDropArea { }
}
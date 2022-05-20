namespace RO.UI
{
    public partial class ChatPanelController : UIController.Panel
    {
        public partial class DetachedChatPanelController : UIController.Panel
        {
            public class DragArea : UI.DragArea,
                IBeginDragHandler
            {
                private DetachedChatPanelController _controller;

                private new void Awake()
                {
                    base.Awake();
                    _controller = GetComponentInParent<DetachedChatPanelController>();
                }

                public new void OnBeginDrag(PointerEventData eventData)
                {
                    if (eventData.button != PointerEventData.InputButton.Left)
                        return;

                    var inputField = _controller._chatPanel._tabData._inputField;
                    if (inputField.transform.parent == transform) // close it
                        inputField.gameObject.SetActive(false);

                    _controller._chatPanel._droppedChatIndex = _controller._chatIndex;

                    base.OnBeginDrag(eventData);
                }
            }
        }
    }
}
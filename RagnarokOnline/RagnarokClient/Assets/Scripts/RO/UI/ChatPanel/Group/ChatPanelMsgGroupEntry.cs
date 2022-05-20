using UnityEngine;

namespace RO.UI
{
    public partial class ChatPanelController : UIController.Panel
    {
        public partial class MsgGroupController : MonoBehaviour
        {
            public class MsgGroupEntry : MonoBehaviour,
                IPointerEnterHandler, IPointerClickHandler
            {
                [SerializeField]
                private MsgGroup _group = default;

                [SerializeField]
                private ChatPanelMsgGroupController _controller = default;

                public void OnPointerClick(PointerEventData eventData)
                {
                    _controller._chatPanel._msgGroup = _group;
                    EventSystem.SetSelectedGameObject(null);
                }

                public void OnPointerEnter(PointerEventData eventData)
                {
                    _controller._hightlight.anchoredPosition = ((RectTransform)transform).anchoredPosition;
                }
            }
        }
    }

    public class ChatPanelMsgGroupEntry : ChatPanelController.MsgGroupController.MsgGroupEntry { }
}
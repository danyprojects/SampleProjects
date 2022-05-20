using UnityEngine;

namespace RO.UI
{
    public partial class ChatPanelController : UIController.Panel
    {
        public partial class MsgGroupController : MonoBehaviour,
            ISelectHandler, IDeselectHandler
        {
            [SerializeField]
            private Vector2 _hightlightAnchorPos = default;
            [SerializeField]
            private Button _button = default;
            [SerializeField]
            private RectTransform _hightlight = default; // Shared with whisper controller

            private ChatPanelController _chatPanel;

            private void Awake()
            {
                _chatPanel = GetComponentInParent<ChatPanelController>();

                _button.OnClick = () => gameObject.SetActive(true);
                gameObject.SetActive(false);
            }

            private void OnEnable()
            {
                EventSystem.SetSelectedGameObject(gameObject);
            }

            public void OnDeselect()
            {
                gameObject.SetActive(false);

                _hightlightAnchorPos = _hightlight.anchoredPosition;
                _hightlight.gameObject.SetActive(false);
            }

            public void OnSelect()
            {
                _hightlight.SetParent(transform);
                _hightlight.SetAsFirstSibling();

                _hightlight.pivot = new Vector2(0, 0.5f);
                _hightlight.anchorMin = Vector2.one;
                _hightlight.anchorMax = Vector2.one;

                _hightlight.anchoredPosition = _hightlightAnchorPos;

                _hightlight.gameObject.SetActive(true);
            }
        }
    }

    public class ChatPanelMsgGroupController : ChatPanelController.MsgGroupController { }
}
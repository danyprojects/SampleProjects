using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public partial class ChatPanelController : UIController.Panel
    {
        public partial class WhisperController : MonoBehaviour,
            ISelectHandler, IDeselectHandler
        {
            public class WhisperEntry : MonoBehaviour,
                ListScrollView.ISlot, IPointerEnterHandler, IPointerClickHandler, IPointerDownHandler
            {
                [SerializeField]
                private WhisperController _controller = default;

                private Text _text;
                private int _index;

                public RectTransform RectTransform => _text.rectTransform;

                public void Init()
                {
                    _text = GetComponentInChildren<Text>();
                }

                public void Fill(int index)
                {
                    // fetch it in reverse order
                    _index = _controller._list.Count - 1 - index;
                    _text.text = _controller._list[_index];
                }

                public void OnPointerEnter(PointerEventData eventData)
                {
                    _controller._hightlight.anchoredPosition =
                        ((RectTransform)transform).anchoredPosition;
                }

                public void OnPointerDown(PointerEventData eventData)
                {
                    // We just care about click but event hits ListScrollView so we need this
                }

                public void OnPointerClick(PointerEventData eventData)
                {
                    _controller.OnWhisperSelected(_index);
                    _controller.gameObject.SetActive(false);
                }
            }
        }
    }

    public class ChatPanelWhisperEntry : ChatPanelController.WhisperController.WhisperEntry { }
}
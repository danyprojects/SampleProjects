using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public partial class ChatPanelController : UIController.Panel
    {
        public class Tab : MonoBehaviour,
            IPointerDownHandler, IPointerUpHandler, IPointerClickHandler,
            IBeginDragHandler, IDragHandler, IDropHandler,
            IPointerEnterHandler, IPointerExitHandler
        {
            [SerializeField]
            private Toggle _toggle = default; // This is actually manually controlled here it has raycast turned off

            [SerializeField]
            private Text _text = default;
            [SerializeField]
            private Image _image = default; // We need to set hightlight sprite manually
            [SerializeField]
            private int _chatIndex;

            private ChatPanelController _controller;

            public const int NewTabPreferredWidth = 72;
            public const int TabDefaultHeight = 17;
            public const int TabInternalPadding = 6;
            public const int TabLeftPadding = 3;

            [field: SerializeField]
            public int Order { get; set; } = default;

            [field: SerializeField]
            public int PreferredWidth { get; private set; } = default;

            public string Text
            {
                get { return _text.text; }
                set
                {
                    Vector2 extents = _text.rectTransform.rect.size;

                    var settings = _text.GetGenerationSettings(extents);
                    settings.generateOutOfBounds = true;

                    var gen = _text.cachedTextGenerator;
                    PreferredWidth = (int)gen.GetPreferredWidth(value, settings) + TabInternalPadding;

                    _text.text = value;
                }
            }

            // We are always going to be active tab if enabled
            public void Enable(int order)
            {
                Order = order;
                _toggle.SetValue(true);
                gameObject.SetActive(true);
                _image.overrideSprite = null; // in case a mouse exit got lost
            }

            // We are always active tab when getting disabled
            public void Disable()
            {
                gameObject.SetActive(false);

                var inputField = _controller._tabData._inputField;

                if (inputField.transform.parent == transform)
                    DeactivateInputField(inputField);

                var index = _controller._lastForegroundChatIndex;
                _controller._tabData._tabs[index]._toggle.SetValue(true);

                if (_controller._attachedActiveChatsCount > 2) // pick lowest order tab
                {
                    var nextLastIndex = _controller._tabData._tabs
                        .Where(
                            x => x != null && x.gameObject.activeSelf &&
                            x._chatIndex != _chatIndex && x._chatIndex != index)
                        .OrderBy(x => x.Order)
                        .First()._chatIndex;

                    _controller._lastForegroundChatIndex = nextLastIndex;
                }
                else // only 1 tab left
                    _controller._lastForegroundChatIndex = _controller._foregroundChatIndex;
            }

            public void Resize(int width)
            {
                var height = _image.rectTransform.sizeDelta.y;

                _image.rectTransform.sizeDelta = new Vector2(width, height);

                // they overllap by 1 pixel so decrement it by Order
                _image.rectTransform.anchoredPosition = new Vector2(TabLeftPadding + Order * width - Order,
                    _image.rectTransform.anchoredPosition.y);
            }

            // Called by controller since we might be a recycled tab
            public void Init(int chatIndex)
            {
                _chatIndex = chatIndex;
                Text = $"New Tab_{chatIndex}";
            }

            public ChatPanelTab Instantiate()
            {
                var inputField = _controller._tabData._inputField;

                if (inputField.transform.parent == transform)
                {
                    DeactivateInputField(inputField);
                    inputField.transform.SetParent(_controller.transform);
                    inputField.transform.SetAsFirstSibling();
                }

                var tab = Instantiate(gameObject, _controller.transform).GetComponent<ChatPanelTab>();

                var dragAreaController = _controller.GetComponent<DragAreaController>();
                var graphics = tab.GetComponentsInChildren<Graphic>(true);

                foreach (var graphic in graphics)
                    dragAreaController.AddGraphic(graphic);

                return tab;
            }

            private void Awake()
            {
                _controller = GetComponentInParent<ChatPanelController>();
                _toggle.OnValueChanged = OnTabToggle;
            }

            // We need it or pointer click won't trigger
            public void OnPointerDown(PointerEventData eventData) { }

            public void OnPointerUp(PointerEventData eventData)
            {
                // Prevent bubble up
            }

            public void OnPointerClick(PointerEventData eventData)
            {
                var inputField = _controller._tabData._inputField;

                // Also ignore if input field opened on us
                if (eventData.button != PointerEventData.InputButton.Left ||
                    inputField.transform.parent == transform && inputField.gameObject.activeSelf)
                    return;

                _controller.BringToFront();

                if (eventData.clickCount >= 2)
                {
                    var inputRect = (RectTransform)inputField.transform;
                    var tabRect = (RectTransform)transform;

                    inputField.transform.SetParent(transform);
                    inputRect.sizeDelta = new Vector2(tabRect.rect.width - 4, tabRect.rect.height - 2);
                    inputRect.anchoredPosition = new Vector2(2, 0);

                    inputField.Text = _text.text;
                    inputField.OnSubmit = OnSubmit;
                    inputField.OverridePanel(_controller);
                    inputField.gameObject.SetActive(true);

                    EventSystem.CurrentKeyboardHandler = inputField;
                    inputField.SelectAll();
                }
                else
                    _toggle.OnPointerClick(eventData);
            }

            public void OnDrop(PointerEventData eventData)
            {
                if (CanvasFilter.HasFlag(CanvasFilter.DragSubChat))
                {
                    var dropIndex = _controller._droppedChatIndex;
                    if (_controller.OnAttachChat(dropIndex, Order))
                        _controller._droppedChatIndex = -1;

                    _image.overrideSprite = null;
                }
            }

            // We forward this event to the detached chat
            public void OnBeginDrag(PointerEventData eventData)
            {
                if (eventData.button != PointerEventData.InputButton.Left)
                    return;

                var inputField = _controller._tabData._inputField;

                // Don't drag with input field opened
                if ((inputField.transform.parent != transform || !inputField.gameObject.activeSelf) &&
                    _controller.OnDetachChat(_chatIndex, false))
                {
                    var chatTrans = (RectTransform)_controller._detachedChats[_chatIndex].transform;

                    _image.rectTransform.pivot = Vector2.up;
                    chatTrans.position = _image.rectTransform.position;
                    _image.rectTransform.pivot = Vector2.zero;

                    _controller._droppedChatIndex = _chatIndex;
                    _controller._detachedChats[_chatIndex].BeginDrag(eventData);
                }
            }

            // Need this to be elegible for drag events
            public void OnDrag(PointerEventData eventData) { }

            public void OnPointerEnter(PointerEventData eventData)
            {
                if (CanvasFilter.HasFlag(CanvasFilter.DragSubChat))
                {
                    _image.overrideSprite = _controller._tabData._dragHightlightSprite;
                }
            }

            public void OnPointerExit(PointerEventData eventData)
            {
                _image.overrideSprite = null;
            }

            private static void DeactivateInputField(InputField field)
            {
                if (ReferenceEquals(EventSystem.CurrentKeyboardHandler, field))
                    EventSystem.CurrentKeyboardHandler = null;
                field.gameObject.SetActive(false);
            }

            private void OnSubmit(string text)
            {
                var inputField = _controller._tabData._inputField;

                if (text.Length == 0)
                {
                    _controller.AddMessage("Empty tab name not allowed", MsgType.SystemError);
                    DeactivateInputField(inputField);
                    return;
                }

                // Check if tab size that fits
                var totalSpace = _controller.TotalTabPreferredWidth(
                    inputField.PrefferedWidth + TabInternalPadding, _chatIndex) + MinRightPadWidth;

                if (totalSpace > _controller.MaxMainChatWidth)
                {
                    _controller.AddMessage("Can't rename tab, name is too long", MsgType.SystemError);
                    DeactivateInputField(inputField);
                    return;
                }

                DeactivateInputField(inputField);
                Text = inputField.Text;

                _controller.RedrawTabs();
            }

            private void OnTabToggle(bool isOn)
            {
                if (isOn)
                {
                    _controller.OnTabSwap(_chatIndex);

                    // for correct tab overlap by setting it to the end
                    _image.rectTransform.SetSiblingIndex(transform.parent.childCount - 2);
                }
                else
                    _controller._lastForegroundChatIndex = _chatIndex;

                _image.rectTransform.sizeDelta = new Vector2(_image.rectTransform.sizeDelta.x,
                    isOn ? TabDefaultHeight : TabDefaultHeight - 1);
            }
        }
    }

    public class ChatPanelTab : ChatPanelController.Tab { }
}
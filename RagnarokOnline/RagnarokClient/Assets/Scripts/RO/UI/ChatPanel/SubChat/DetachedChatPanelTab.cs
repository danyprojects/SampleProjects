using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public partial class ChatPanelController : UIController.Panel
    {
        public partial class DetachedChatPanelController : UIController.Panel
        {
            public class Tab : UI.DragArea,
                IBeginDragHandler, IDragHandler, IEndDragHandler,
                IPointerDownHandler, IPointerClickHandler
            {
                [SerializeField]
                private Text _text = default;

                private DetachedChatPanelController _controller;

                public int PrefferedWidth => (int)_text.preferredWidth;

                public string Text
                {
                    get { return _text.text; }
                    set { _text.text = value; }
                }

                public static void DeactivateInputField(InputField field)
                {
                    if (ReferenceEquals(EventSystem.CurrentKeyboardHandler, field))
                        EventSystem.CurrentKeyboardHandler = null;
                    field.gameObject.SetActive(false);
                }

                public new void OnBeginDrag(PointerEventData eventData)
                {
                    var inputField = _controller._chatPanel._tabData._inputField;

                    // Don't drag with input field opened
                    if (inputField.transform.parent != transform || !inputField.gameObject.activeSelf)
                    {
                        _controller._chatPanel._droppedChatIndex = _controller._chatIndex;
                        base.OnBeginDrag(eventData);
                    }
                }

                private new void Awake()
                {
                    base.Awake();

                    _controller = GetComponentInParent<DetachedChatPanelController>();
                }

                // We need it or pointer click won't trigger
                public void OnPointerDown(PointerEventData eventData)
                {
                    _controller.BringToFront();
                }

                public void OnPointerClick(PointerEventData eventData)
                {
                    var inputField = _controller._chatPanel._tabData._inputField;

                    // Also ignore if input field opened on us
                    if (eventData.button != PointerEventData.InputButton.Left ||
                        inputField.transform.parent == transform && inputField.gameObject.activeSelf)
                        return;

                    // Double click always processed
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
                }

                public new void OnDrag(PointerEventData eventData)
                {
                    if (CanvasFilter.HasFlag(CanvasFilter.DragSubChat))
                        base.OnDrag(eventData);
                }

                public new void OnEndDrag(PointerEventData eventData)
                {
                    if (CanvasFilter.HasFlag(CanvasFilter.DragSubChat))
                    {
                        base.OnEndDrag(eventData);

                        // check if the chat was droped on a tab
                        if (_controller._chatPanel._droppedChatIndex == -1)
                            _controller.gameObject.SetActive(false);
                    }
                }

                private void OnSubmit(string text)
                {
                    var inputField = _controller._chatPanel._tabData._inputField;

                    if (text.Length == 0)
                    {
                        var index = _controller._chatPanel.AddMessageInternal(_controller._chatIndex,
                            new MessageData { text = "Empty tab name not allowed", type = MsgType.SystemError });

                        _controller.AddEntry(index);
                        DeactivateInputField(inputField);
                        return;
                    }

                    int newWidth = inputField.PrefferedWidth + ChatPanelController.Tab.TabInternalPadding;

                    // Check if tab size that fits
                    if (newWidth > PrefferedWidth &&
                        _controller._rightPad.rect.width - MinRightPadWidth - newWidth + PrefferedWidth < 0)
                    {
                        var index = _controller._chatPanel.AddMessageInternal(_controller._chatIndex,
                            new MessageData { text = "Can't rename tab, name is too long", type = MsgType.SystemError });

                        _controller.AddEntry(index);
                        DeactivateInputField(inputField);
                        return;
                    }

                    DeactivateInputField(inputField);
                    _text.text = inputField.Text;

                    _controller.ResizeTabArea(newWidth);
                }
            }
        }
    }

    public class DetachedChatPanelTab : ChatPanelController.DetachedChatPanelController.Tab { }
}
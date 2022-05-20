using System.Collections.Generic;
using UnityEngine;

namespace RO.UI
{
    public partial class ChatPanelController : UIController.Panel
    {
        public partial class WhisperController : MonoBehaviour,
            ISelectHandler, IDeselectHandler, IScrollHandler
        {
            [SerializeField]
            private ListScrollView _scrollView = default;
            [SerializeField]
            private Button _button = default;
            [SerializeField]
            private InputField _inputField = default;
            [SerializeField]
            private RectTransform _hightlight = default; // Shared with group controller

            private int _index = 0;
            private List<string> _list = new List<string>();
            private HashSet<string> _set = new HashSet<string>();

            private ChatPanelController _chatPanelController;


            public void Init()
            {
                _chatPanelController = GetComponentInParent<ChatPanelController>();

                _button.OnClick = OnClickButton;
                _list.Add("");
                _scrollView.Init();
                _scrollView.AddEntry(0);

                _inputField.OnTab = () => EventSystem.CurrentKeyboardHandler = _chatPanelController._chatInputField;
                _chatPanelController._chatInputField.OnTab = () => EventSystem.CurrentKeyboardHandler = _inputField;

                _inputField.OnDownArrow = OnDownArrow;
                _inputField.OnUpArrow = OnUpArrow;
            }

            public void Clear()
            {
                _list.Clear();
                _set.Clear();
                _inputField.Clear();
                _scrollView.Clear();
                _index = 0;
            }

            // Process current whisper in input field and return correct MsgType
            public MsgType Process()
            {
                if (_inputField.Text.Length == 0)
                    return MsgType.Normal;

                if (_set.Add(_inputField.Text))
                {
                    // override the empty character, and put it back at end
                    _list[_list.Count - 1] = _inputField.Text;
                    _list.Add("");
                    _index = _list.Count - 2;
                    _scrollView.AddEntry(_index);

                    if (_list.Count <= 5)
                    {
                        ((RectTransform)gameObject.transform).sizeDelta += new Vector2(0, _scrollView.RowHeight);
                        _scrollView.VisibleRows++;
                    }
                }

                return MsgType.Whisper;
            }

            public void OnDeselect()
            {
                gameObject.SetActive(false);
                _hightlight.gameObject.SetActive(false);
            }

            public void OnSelect()
            {
                // reverse it since they are drawn in reverse order
                var index = _list.Count - 1 - _index;

                _scrollView.RedrawAt(index);

                _hightlight.SetParent(transform);
                _hightlight.SetAsFirstSibling();

                _hightlight.pivot = Vector2.zero;
                _hightlight.anchorMin = Vector2.zero;
                _hightlight.anchorMax = Vector2.zero;
                _hightlight.anchoredPosition = _scrollView.GetIndexAnchorPosition(index);

                _hightlight.gameObject.SetActive(true);
            }

            public void OnScroll(PointerEventData eventData)
            {
                _scrollView.OnScroll(eventData);
            }

            private void OnEnable()
            {
                EventSystem.SetSelectedGameObject(gameObject);
            }

            private void OnClickButton()
            {
                if (_list.Count > 1)
                    _scrollView.gameObject.SetActive(true);
                else
                    _chatPanelController.AddMessage("No Whisper List.", MsgType.SystemError);
            }

            private void OnDownArrow()
            {
                if (_index > 0)
                    OnWhisperSelected(--_index);
            }

            private void OnUpArrow()
            {
                if (_index < _list.Count - 1)
                    OnWhisperSelected(++_index);
            }

            private void OnWhisperSelected(int index)
            {
                _index = index;
                _inputField.Text = _list[index];
                _inputField.SelectAll();
            }
        }
    }

    public class ChatPanelWhisperController : ChatPanelController.WhisperController { }
}
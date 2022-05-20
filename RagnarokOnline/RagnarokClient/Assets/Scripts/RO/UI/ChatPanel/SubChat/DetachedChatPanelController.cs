using System.Collections;
using UnityEngine;

namespace RO.UI
{
    public partial class ChatPanelController : UIController.Panel
    {
        public partial class DetachedChatPanelController : UIController.Panel
        {
            [SerializeField]
            private ChatListScrollView _scrollView = default;
            [SerializeField]
            private Button _deleteButton = default;
            [SerializeField]
            private Button _filterOptionsButton = default;
            [SerializeField]
            private Button _attachButton = default;
            [SerializeField]
            private Tab _tab = default;
            [SerializeField]
            private RectTransform _rightPad = default;

            private const int MinRightPadWidth = 40;
            private static PointerEventData _cachedDragEventData = new PointerEventData();
            private static readonly Vector2 DefaultPosition = new Vector2(100, -100);
            private static readonly Vector2 DefaultOffset = new Vector2(10, -20);
            private static readonly Vector2 DefaultSize = new Vector2(300, 85);

            private ChatPanelController _chatPanel;
            private int _chatIndex;

            public string TabText => _tab.Text;
            public int PrefferedWidth => _tab.PrefferedWidth;

            public void SetDefaultPosition()
            {
                var detachedChatsCount = 1 + MaxChats - _chatPanel._attachedActiveChatsCount - _chatPanel._freeChatIndexes.Count;
                var rectTrans = (RectTransform)transform;

                rectTrans.anchoredPosition = DefaultPosition + DefaultOffset * detachedChatsCount;
            }

            // We need to forward from main chat tab when detaching
            // So create a couroutine and delay it so we finihs initializations
            public void BeginDrag(PointerEventData eventData)
            {
                eventData.pointerDrag = _tab.gameObject; // Swap drag target

                _cachedDragEventData.button = eventData.button;
                _cachedDragEventData.pressPosition = eventData.pressPosition;

                StartCoroutine(DelayedBeginDragCouroutine());
            }

            public void AddEntry(int index)
            {
            }

            private void Awake()
            {
                GetComponentInChildren<ResizePanel>().OnResize = OnResize;

                _filterOptionsButton.OnClick = () => _chatPanel.OnClickChatFilterOptions(_chatIndex);
                _attachButton.OnClick = OnClickAttach;
                _deleteButton.OnClick = () => _chatPanel.OnDeleteDetachedChat(_chatIndex);

                //_scrollView.Init()
            }

            private void OnClickAttach()
            {
                if (_chatPanel.OnAttachChat(_chatIndex, _chatPanel._attachedActiveChatsCount))
                    gameObject.SetActive(false);
            }

            private new void OnEnable()
            {
                base.OnEnable();
                BringToFront();

                ((RectTransform)transform).sizeDelta = DefaultSize;

                // Refresh Tab
                var mainTab = _chatPanel._tabData._tabs[_chatIndex];

                if (!ReferenceEquals(mainTab.Text, _tab.Text)) // Do nothing if text is the same
                {
                    _tab.Text = mainTab.Text;

                    var width = ((RectTransform)mainTab.gameObject.transform).rect.width;
                    ResizeTabArea(width);
                }

                // Refresh and recalculate Text box

                _scrollView.Clear();
            }

            private new void OnDisable()
            {
                base.OnDisable();

                // Deactivate input field if it's on us
                if (_chatPanel._tabData._inputField.transform.parent == _tab.transform)
                    Tab.DeactivateInputField(_chatPanel._tabData._inputField);
            }

            private IEnumerator DelayedBeginDragCouroutine()
            {
                yield return 0;
                _tab.OnBeginDrag(_cachedDragEventData);
            }

            private void ResizeTabArea(float newTabSize)
            {
                var tabTrans = (RectTransform)_tab.gameObject.transform;
                var sizeDiff = newTabSize - tabTrans.sizeDelta.x;

                tabTrans.sizeDelta += new Vector2(sizeDiff, 0);
                _rightPad.sizeDelta -= new Vector2(sizeDiff, 0);
            }

            private void OnResize(int x, int y)
            {

            }

            public static DetachedChatPanelController Instantiate(int chatIndex, ChatPanelController chatController)
            {
                var controller = Instantiate<DetachedChatPanelController>(chatController.UiController,
                    chatController.transform.parent, "DetachedChatPanel");

                controller._chatIndex = chatIndex;
                controller._chatPanel = chatController;

                return controller;
            }
        }
    }

    public class DetachedChatPanelController : ChatPanelController.DetachedChatPanelController { }
}
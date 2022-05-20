using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RO.UI
{
    public partial class ChatPanelController : UIController.Panel,
        IScrollHandler, ICanvasRaycastFilter
    {
#pragma warning disable 0649
        [Serializable]
        private struct TabData
        {
            [SerializeField]
            public InputField _inputField;

            [SerializeField]
            public Sprite _dragHightlightSprite;

            [SerializeField]
            public ChatPanelTab[] _tabs;

            [SerializeField]
            public RectTransform _rightPad;
        }
#pragma warning restore 0649

        [SerializeField]
        private ChatListScrollView _scrollView = default;

        [SerializeField]
        private ChatInputField _chatInputField = default;

        [SerializeField]
        private Button _chatFilterOptionsButton = default;

        [SerializeField]
        private Button _detachChatButton = default;

        [SerializeField]
        private Button _deleteChatButton = default;

        [SerializeField]
        private Button _addChatButton = default;

        [SerializeField]
        private Toggle _lockChatToggle = default;

        [SerializeField]
        private UI.ChatMessage _baseMessage = default;

        [SerializeField]
        private int _maxVisibleMsgs = default;

        [SerializeField]
        private int _defaultVisibleMsgs = default;

        [SerializeField]
        private Button _expandChatButton = default;

        [SerializeField]
        private WhisperController _whisperController = default;

        [SerializeField]
        private ResizePanel _resizePanel = default;

        [SerializeField]
        private TabData _tabData = default;

        private int _panelDefaultWidth;

        public enum MsgType
        {
            Normal,
            Whisper,
            SystemError,
        }

        public enum MsgGroup
        {
            All,
            Party,
            Guild
        }

        struct MessageData
        {
            public string text;
            public MsgType type;
        }

        enum ChatState
        {
            Free,
            Detached,
            Background,
            Foreground
        }

        private const int MinRightPadWidth = 82;

        private const int MaxChats = 10;
        private const int MaxMsgsPerChat = 100;

        private int MaxMainChatWidth;

        private MessageData[][] _messageData = new MessageData[MaxChats][];
        private int[] _messageIndex = new int[MaxChats];
        private MsgGroup _msgGroup;

        private DetachedChatPanelController[] _detachedChats = new DetachedChatPanelController[MaxChats];
        private Stack<int> _freeChatIndexes = new Stack<int>(new[] { 9, 8, 7, 6, 5, 4, 3, 2 });
        private ChatState[] _chatState = new ChatState[MaxChats];
        private int _droppedChatIndex = -1;
        private int _foregroundChatIndex = 0;
        private int _lastForegroundChatIndex = 0;
        private int _attachedActiveChatsCount = 2;

        private int _maxSizeStep;
        private int _currentSizeStep = 0;

        public void ClearWhisperList()
        {
            _whisperController.Clear();
        }

        public void AddMessage(string text, MsgType msgType)
        {
            var msgData = new MessageData
            {
                text = text,
                type = msgType
            };

            // TODO:: analise msg for link data here, share same link instance
            // with the different chats

            for (int i = 0; i < _chatState.Length; i++)
            {
                switch (_chatState[i])
                {
                    case ChatState.Background:
                        if (IsMessageAllowed(i, msgType))
                            AddMessageInternal(i, msgData);
                        break;
                    case ChatState.Detached:
                        if (IsMessageAllowed(i, msgType))
                            _detachedChats[i].AddEntry(AddMessageInternal(i, msgData));
                        break;
                }
            }

            if (IsMessageAllowed(_foregroundChatIndex, msgType))
            {
                var index = AddMessageInternal(_foregroundChatIndex, msgData);
                _scrollView.AddEntry(index);
            }
        }

        private int AddMessageInternal(int chatIndex, MessageData msg)
        {
            var index = _messageIndex[chatIndex]++;

            _messageIndex[chatIndex] %= MaxMsgsPerChat;
            _messageData[chatIndex][index] = msg;

            return index;
        }

        private bool IsMessageAllowed(int chatIndex, MsgType msgType)
        {
            return true;
        }

        private void Awake()
        {
            _chatState[0] = ChatState.Foreground;
            _chatState[1] = ChatState.Background;

            var msgs = new UI.ChatMessage[_maxVisibleMsgs];
            msgs[0] = _baseMessage;

            for (int i = 1; i < msgs.Length; i++)
                msgs[i] = _baseMessage.Clone(0);

            _messageData[0] = new MessageData[MaxMsgsPerChat];
            _messageData[1] = new MessageData[MaxMsgsPerChat];

            _scrollView.Init(msgs, _defaultVisibleMsgs);
            _chatInputField.OnSubmit = OnSubmitChatMsg;
            _chatInputField.Init();

            _whisperController.Init();
            _tabData._inputField.InitAndDeactivate();

            _chatFilterOptionsButton.OnClick = () => OnClickChatFilterOptions(_foregroundChatIndex);
            _detachChatButton.OnClick = () => OnDetachChat(_foregroundChatIndex);
            _deleteChatButton.OnClick = () => OnDeleteChat();
            _addChatButton.OnClick = OnAddChat;
            _expandChatButton.OnClick = OnClickExpandChat;
            _resizePanel.OnResize = OnResizeChat;

            _panelDefaultWidth = (int)((RectTransform)transform).rect.width;
            MaxMainChatWidth = (int)((RectTransform)_scrollView.transform).rect.width;

            _maxSizeStep = (_resizePanel.MaxSize.y - _resizePanel.MinSize.y) / -_resizePanel.Step.y;
        }

        private new void OnEnable()
        {
            base.OnEnable();

            EventSystem.DefaultKeyboardHandler = _chatInputField;
            EventSystem.CurrentKeyboardHandler = _chatInputField;
        }

        private new void OnDisable()
        {
            base.OnDisable();

            EventSystem.DefaultKeyboardHandler = null;
            EventSystem.CurrentKeyboardHandler = null;

            // Make sure this is never on while we are disabled
            Common.Globals.UI.IsOverChatBox = false;
        }

        private void OnClickChatFilterOptions(int chatIndex)
        {

        }

        private void OnResizeChat(int xStep, int yStep)
        {
            if (yStep != _currentSizeStep)
            {
                _currentSizeStep = yStep;
                _scrollView.ResizeVertical(_defaultVisibleMsgs + _currentSizeStep);
            }
        }

        private void OnClickExpandChat()
        {
            if (_currentSizeStep == _maxSizeStep)
                _currentSizeStep = 0;
            else
                _currentSizeStep = Mathf.Clamp(_currentSizeStep + 3, 0, _maxSizeStep);

            ((RectTransform)transform).sizeDelta =
                _resizePanel.MinSize - _resizePanel.Step * _currentSizeStep;

            _scrollView.ResizeVertical(_defaultVisibleMsgs + _currentSizeStep);
        }

        private bool OnDetachChat(int chatIndex, bool reposition = true)
        {
            if (_lockChatToggle.IsOn || _attachedActiveChatsCount < 2)
                return false;

            var chat = _detachedChats[chatIndex];
            if (chat == null) // lazy initialize
                _detachedChats[chatIndex] = chat = DetachedChatPanelController.Instantiate(chatIndex, this);

            if (reposition)
                chat.SetDefaultPosition();

            _chatState[chatIndex] = ChatState.Detached;
            var tab = _tabData._tabs[chatIndex];

            tab.Disable();
            chat.gameObject.SetActive(true);

            DecrementTabsOrder(tab.Order);

            _attachedActiveChatsCount--;
            RedrawTabs();
            RedrawActiveChat();

            return true;
        }

        private bool OnAttachChat(int chatIndex, int order)
        {
            if (_lockChatToggle.IsOn)
                return false;

            var chat = _detachedChats[chatIndex];

            // Check it text fits it chat if not don't allow atach
            var totalSpace = TotalTabPreferredWidth(chat.PrefferedWidth, chatIndex) + MinRightPadWidth;
            if (totalSpace > MaxMainChatWidth)
            {
                AddMessage("Can't attach chat, tab name is too long", MsgType.SystemError);
                return false;
            }

            IncrementTabsOrder(order);

            _tabData._tabs[chatIndex].Text = chat.TabText;
            _tabData._tabs[chatIndex].Enable(order);

            _attachedActiveChatsCount++;
            RedrawTabs();
            RedrawActiveChat();

            return true;
        }

        private void OnDeleteChat()
        {
            if (_attachedActiveChatsCount < 2)
                return;

            var index = _foregroundChatIndex;
            _chatState[index] = ChatState.Free;
            var tab = _tabData._tabs[index];
            tab.Disable();

            _freeChatIndexes.Push(index);
            DecrementTabsOrder(tab.Order);

            _attachedActiveChatsCount--;
            RedrawTabs();
            RedrawActiveChat();
        }

        private void OnDeleteDetachedChat(int chatIndex)
        {
            _chatState[chatIndex] = ChatState.Free;
            _detachedChats[chatIndex].gameObject.SetActive(false);

            _freeChatIndexes.Push(chatIndex);
        }

        private void OnAddChat()
        {
            if (_freeChatIndexes.Count == 0)
            {
                AddMessage("Reached maximum allowed number of chats", MsgType.SystemError);
                return;
            }

            var index = _freeChatIndexes.Pop();

            var totalSpace = TotalTabPreferredWidth(Tab.NewTabPreferredWidth + Tab.TabInternalPadding, index)
                            + MinRightPadWidth;

            if (totalSpace > MaxMainChatWidth)
            {
                _freeChatIndexes.Push(index);
                AddMessage("No tab space for more chats, rename existing tabs or detach them", MsgType.SystemError);
                return;
            }

            if (_messageData[index] == null) // lazy initialize
            {
                _messageData[index] = new MessageData[MaxMsgsPerChat];
                _tabData._tabs[index] = _tabData._tabs[0].Instantiate();
            }

            _tabData._tabs[index].Init(index);
            _tabData._tabs[index].Enable(_attachedActiveChatsCount);

            _attachedActiveChatsCount++;
            RedrawTabs();
            RedrawActiveChat();
        }

        private void RedrawTabs()
        {
            var tabs = _tabData._tabs.Where(x => x != null && x.gameObject.activeSelf).OrderBy(x => x.Order);
            var preferredWidth = 0;
            foreach (var tab in tabs)
            {
                if (tab.PreferredWidth > preferredWidth)
                    preferredWidth = tab.PreferredWidth;
            }

            foreach (var tab in tabs)
                tab.Resize(preferredWidth);

            // Tabs overlap by one pixel so take that into account
            _tabData._rightPad.sizeDelta = new Vector2(_panelDefaultWidth + _tabData._rightPad.anchoredPosition.x
                - Tab.TabLeftPadding - preferredWidth * _attachedActiveChatsCount + _attachedActiveChatsCount
                , Tab.TabDefaultHeight);
        }

        private void DecrementTabsOrder(int pivot)
        {
            for (int i = 0; i < _tabData._tabs.Length; i++)
            {
                var tab = _tabData._tabs[i];
                if (tab != null && tab.gameObject.activeSelf && tab.Order > pivot)
                    tab.Order--;
            }
        }

        private void IncrementTabsOrder(int pivot)
        {
            for (int i = 0; i < _tabData._tabs.Length; i++)
            {
                var tab = _tabData._tabs[i];
                if (tab != null && tab.gameObject.activeSelf && tab.Order >= pivot)
                    tab.Order++;
            }
        }

        private void RedrawActiveChat()
        {
            _scrollView.Clear();
            int i = 0;
            foreach (var msgData in _messageData[_foregroundChatIndex])
            {
                if (msgData.text == null) // break when there are no more msgs
                    break;

                // index will be correctly translated in ChatMessage here we just need to add total of existing msgs
                _scrollView.AddEntry(i++);
            }
        }

        private void OnTabSwap(int activeChatIndex)
        {
            _chatState[_foregroundChatIndex] = ChatState.Background;
            _chatState[activeChatIndex] = ChatState.Foreground;

            _foregroundChatIndex = activeChatIndex;
            RedrawActiveChat();
        }

        private int TotalTabPreferredWidth(int prefferedWidth, int newTabIndex)
        {
            int max = prefferedWidth;
            int i = 0;
            int count = 1;

            foreach (var tab in _tabData._tabs)
            {
                if (i++ == newTabIndex)
                    continue;

                if (tab != null && tab.gameObject.activeSelf)
                {
                    count++;

                    if (tab.PreferredWidth > max)
                        max = tab.PreferredWidth;
                }
            }

            return max * count;
        }

        private MessageData GetMessageData(int chatIndex, int msgIndex)
        {
            return _messageData[chatIndex][msgIndex];
        }

        private void OnSubmitChatMsg(string text)
        {
            if (text.Length == 0)
            {
                _chatInputField.ToogleChatOff();
                return;
            }

            _chatInputField.Clear();

            MsgType msgType = _whisperController.Process();

            AddMessage(text, msgType);
        }

        public new bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            const CanvasFilter mask = ~(CanvasFilter.NpcDialog | CanvasFilter.DragSubChat);

            return (mask & CanvasFilter) == 0;
        }

        public static ChatPanelController Instantiate(UIController uiController, Transform parent)
        {
            var controller = Instantiate<ChatPanelController>(uiController, parent, "ChatPanel");

            return controller;
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (CanvasFilter.HasFlag(CanvasFilter.DragSubChat))
                return;

            _scrollView.OnScroll(eventData);
        }
    }
}
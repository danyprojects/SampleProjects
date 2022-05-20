using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public class ChatListScrollView : MonoBehaviour
        , ICanvasRaycastFilter, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private Scrollbar _scrollbar = default;

        [SerializeField]
        private Vector2 _padding = default;

        private float _lineHeight;
        private int _maxVisibleLines;
        private ChatMessage[] _msgs;
        private bool _ignoreScrollBarEvent = false;

        private int _currentScrollBarStep = 1;
        private int _totalLines = 0;
        private int _visibleLines = 0;
        private int _msgCount = 0;
        private int _firstVisibleMsgIndex = 0;
        private int _visibleMsgCount = 0;

        private int _upperMsgOverflow = 0;
        private int _lowerMsgOverflow = 0;

        private void Awake()
        {
            _scrollbar.onValueChanged.AddListener(OnScrollBarValueChanged);
        }

        public void Init(ChatMessage[] msgs, int maxVisibleLines)
        {
            _msgs = msgs;
            _maxVisibleLines = maxVisibleLines;
            _lineHeight = ((RectTransform)msgs.First().gameObject.transform).rect.height;
        }

        // Resizes assuming only height has changed
        public void ResizeVertical(int maxVisibleLines)
        {
            // force text to be aligned to bottom, this only triggers if we resize while in middle of chat
            if (_currentScrollBarStep < _scrollbar.numberOfSteps && _currentScrollBarStep > 1)
            {
                while (_currentScrollBarStep > 1)
                {
                    _currentScrollBarStep--;
                    ShiftDown();
                }
            }

            // hide all of the entries above the threshold
            if (maxVisibleLines < _maxVisibleLines)
            {
                _lowerMsgOverflow = 0;
                _visibleLines = Math.Min(maxVisibleLines, _visibleLines);

                for (int i = 0, count = _visibleMsgCount; i < count; i++)
                {
                    var entryY = _msgs[i].rectTransform.anchoredPosition.y - _padding.y;
                    var entryLines = _msgs[i].LineCount();

                    if (entryY - entryLines * _lineHeight >= maxVisibleLines * _lineHeight) // off view
                    {
                        _visibleMsgCount--;
                        _msgs[i].rectTransform.gameObject.SetActive(false);
                    }
                    else
                    {
                        var overflow = (entryY - maxVisibleLines * _lineHeight) / _lineHeight;
                        _lowerMsgOverflow += Math.Max((int)overflow, 0);
                    }
                }
            }
            else
            {
                var extraLines = maxVisibleLines - _maxVisibleLines - _lowerMsgOverflow;
                if (extraLines > 0) // we need to spawn more msgs
                {
                    _lowerMsgOverflow = 0;
                    while (extraLines > 0 && _firstVisibleMsgIndex + _visibleMsgCount < _msgCount)
                    {
                        var basePos = _msgs[_visibleMsgCount - 1].rectTransform.anchoredPosition;
                        var newMsg = _msgs[_visibleMsgCount++];

                        newMsg.Fill(_msgCount - _firstVisibleMsgIndex - _visibleMsgCount); // asking on reverse
                        var lines = newMsg.LineCount();
                        newMsg.rectTransform.anchoredPosition = basePos + new Vector2(0, lines * _lineHeight);

                        _lowerMsgOverflow = Math.Max(lines - extraLines, 0);
                        extraLines -= lines;
                        _visibleLines += lines - _lowerMsgOverflow;
                        newMsg.gameObject.SetActive(true);
                    }
                }
                else
                {
                    _visibleLines += maxVisibleLines - _maxVisibleLines;
                    _lowerMsgOverflow -= maxVisibleLines - _maxVisibleLines;
                }
            }

            // Don't update anything else if all messages fit on screen
            // Or if the chat is fully scrolled down
            if (maxVisibleLines > _totalLines || (_firstVisibleMsgIndex == 0 && _upperMsgOverflow == 0))
            {
                _maxVisibleLines = maxVisibleLines;
                UpdateScrollBar();
                return;
            }

            var lineDiff = maxVisibleLines - _maxVisibleLines;

            _currentScrollBarStep -= lineDiff;
            if (lineDiff > 0)
                while (lineDiff-- > 0 && (_upperMsgOverflow > 0 || _firstVisibleMsgIndex > 0))
                    ShiftDown();
            else if (_msgs[_visibleMsgCount - 1].rectTransform.anchoredPosition.y == _maxVisibleLines * _lineHeight)
                while (lineDiff++ < 0)
                    ShiftUp();

            _maxVisibleLines = maxVisibleLines;
            UpdateScrollBar();
        }

        // Resizes taking into account both dimentions
        public void Resize(int maxVisibleLines)
        {
            var msgCount = _msgCount;
            Clear();
            _maxVisibleLines = maxVisibleLines;

            var totalLines = 0;
            int i = msgCount - 1;
            while (totalLines < maxVisibleLines) // calculate visible lines
                totalLines += (int)(_msgs[0].PreCalculateHeight(i--) / _lineHeight);

            for (int k = 0; k <= i; k++) // calculate invisible lines
                totalLines += (int)(_msgs[0].PreCalculateHeight(k) / _lineHeight);

            for (; i < msgCount; i++) // redraw from bottom
                AddEntry(i);

            _totalLines = totalLines;
            _msgCount = msgCount;
        }

        public void AddEntry(int index)
        {
            _msgCount++;

            // we are not fully scrolled down
            if (_firstVisibleMsgIndex != 0)
            {
                _firstVisibleMsgIndex++;
                var steps = (int)(_msgs[0].PreCalculateHeight(index) / _lineHeight);
                _currentScrollBarStep += steps;
                _totalLines += steps;

                UpdateScrollBar();
                return;
            }

            var oldVisibleMsgCount = _visibleMsgCount;
            var tmp = _msgs[_visibleMsgCount];
            tmp.Fill(index);
            var lines = tmp.LineCount();

            _totalLines += lines;
            _lowerMsgOverflow = 0; // overflow gets recalculated

            // push everything up
            for (int i = 0; i < oldVisibleMsgCount; i++)
            {
                _msgs[i].rectTransform.anchoredPosition += new Vector2(0, lines * _lineHeight);
                var entryY = _msgs[i].rectTransform.anchoredPosition.y - _padding.y;
                var entryLines = _msgs[i].LineCount();

                if (entryY - entryLines * _lineHeight >= _maxVisibleLines * _lineHeight) // off view
                {
                    _visibleMsgCount--;
                    _msgs[i].rectTransform.gameObject.SetActive(false);
                }
                else
                {
                    var overflow = (entryY - _maxVisibleLines * _lineHeight) / _lineHeight;
                    _lowerMsgOverflow += Math.Max((int)overflow, 0);
                }

                var hold = _msgs[i];
                _msgs[i] = tmp;
                tmp = hold;
            }

            // we pull it out of loop so push last entry back in
            {
                var hold = _msgs[oldVisibleMsgCount];
                _msgs[oldVisibleMsgCount] = tmp;
                tmp = hold;
            }

            _visibleMsgCount++;
            _lowerMsgOverflow += Math.Max(lines - _maxVisibleLines, 0); // in case of single big message
            _visibleLines = Math.Min(_visibleLines + lines, _maxVisibleLines);

            tmp.rectTransform.anchoredPosition = new Vector2(_padding.x, _padding.y + lines * _lineHeight);
            tmp.rectTransform.gameObject.SetActive(true);

            UpdateScrollBar();
        }

        private void ShiftUp()
        {
            var tmpMsg = _msgs[0];

            // in case we have a single entry that overflows to both top/bottom
            if (_visibleMsgCount == 1)
            {
                _upperMsgOverflow++;
                _lowerMsgOverflow--;
                tmpMsg.rectTransform.anchoredPosition -= new Vector2(0, _lineHeight);
                return;
            }

            void AddNew(int index, ChatMessage msg)
            {
                msg.rectTransform.anchoredPosition = _msgs[_visibleMsgCount - 2].rectTransform.anchoredPosition;
                msg.Fill(index);
                var lines = msg.LineCount();
                msg.rectTransform.anchoredPosition += new Vector2(0, lines * _lineHeight);

                _lowerMsgOverflow = lines - 1;
            }

            // first element will be removed 
            if (tmpMsg.LineCount() - _upperMsgOverflow - 1 == 0)
            {
                // push old elements to begin on array
                for (int i = 0; i < _visibleMsgCount - 1; i++)
                {
                    _msgs[i] = _msgs[i + 1];
                    _msgs[i].rectTransform.anchoredPosition -= new Vector2(0, _lineHeight);
                }

                _upperMsgOverflow = 0;
                _msgs[_visibleMsgCount - 1] = tmpMsg;

                if (_lowerMsgOverflow > 0) // we won't need to put another element in
                {
                    _lowerMsgOverflow--;
                    _visibleMsgCount--;
                    _firstVisibleMsgIndex++;
                    tmpMsg.rectTransform.gameObject.SetActive(false);
                }
                else // if there is no overflow then there must be another element or op should not have been allowed
                {
                    AddNew(_msgCount - ++_firstVisibleMsgIndex - _visibleMsgCount, tmpMsg); // asking on reverse
                }
            }
            else
            {
                _upperMsgOverflow++;

                // push old elements to begin on array
                for (int i = 0; i < _visibleMsgCount; i++)
                    _msgs[i].rectTransform.anchoredPosition -= new Vector2(0, _lineHeight);

                if (_lowerMsgOverflow > 0) // we won't need to put another element in any case
                {
                    _lowerMsgOverflow--;
                }
                else // if there is no overflow then there must be another element or op should not have been allowed
                {
                    tmpMsg = _msgs[_visibleMsgCount++];
                    AddNew(_msgCount - _firstVisibleMsgIndex - _visibleMsgCount, tmpMsg); // asking on reverse
                    tmpMsg.rectTransform.gameObject.SetActive(true);
                }
            }
        }

        private void ShiftDown()
        {
            var tmpMsg = _msgs[_visibleMsgCount - 1];

            // in case we have a single entry that overflows to both top/bottom
            if (_visibleMsgCount == 1)
            {
                _lowerMsgOverflow++;
                _upperMsgOverflow--;
                tmpMsg.rectTransform.anchoredPosition += new Vector2(0, _lineHeight);

                return;
            }

            // last element will be removed 
            bool removeLast = tmpMsg.LineCount() - _lowerMsgOverflow - 1 == 0;

            if (_upperMsgOverflow > 0) // we won't need to put another element in
            {
                if (removeLast)
                {
                    _lowerMsgOverflow = 0;
                    _visibleMsgCount--;
                }
                else
                {
                    var y = tmpMsg.rectTransform.anchoredPosition.y + _lineHeight;
                    if (y - tmpMsg.LineCount() * _lineHeight < _maxVisibleLines * _lineHeight &&
                        y > _maxVisibleLines * _lineHeight)
                        _lowerMsgOverflow++;
                }

                // push old elements to end on array
                for (int i = 0; i < _visibleMsgCount; i++)
                    _msgs[i].rectTransform.anchoredPosition += new Vector2(0, _lineHeight);

                _upperMsgOverflow--;
                tmpMsg.rectTransform.gameObject.SetActive(!removeLast);
            }
            else
            {
                if (removeLast)
                    _lowerMsgOverflow = 0;
                else
                {
                    var y = tmpMsg.rectTransform.anchoredPosition.y + _lineHeight;
                    if (y - tmpMsg.LineCount() * _lineHeight < _maxVisibleLines * _lineHeight &&
                        y > _maxVisibleLines * _lineHeight)
                        _lowerMsgOverflow++;

                    tmpMsg = _msgs[_visibleMsgCount++]; //save ref since we are about to override it
                }

                // push old elements to end on array
                for (int i = _visibleMsgCount - 1; i > 0; i--)
                {
                    _msgs[i] = _msgs[i - 1];
                    _msgs[i].rectTransform.anchoredPosition += new Vector2(0, _lineHeight);
                }
                _msgs[0] = tmpMsg;

                _firstVisibleMsgIndex--;
                var index = _msgCount - _firstVisibleMsgIndex - 1;  // asking on reverse

                tmpMsg.Fill(index);
                var lines = tmpMsg.LineCount();

                tmpMsg.rectTransform.anchoredPosition = new Vector2(_padding.x, _padding.y + _lineHeight);
                _upperMsgOverflow = lines - 1;

                tmpMsg.rectTransform.gameObject.SetActive(true);
            }
        }

        private void UpdateScrollBar()
        {
            _ignoreScrollBarEvent = true;

            int steps = _totalLines - _maxVisibleLines;
            if (steps > 0)
            {
                _scrollbar.numberOfSteps = steps + 1;

                _scrollbar.size = _maxVisibleLines / (float)(_totalLines);
                if (!_scrollbar.gameObject.activeSelf)
                    _scrollbar.gameObject.SetActive(true);

                _scrollbar.SetValueWithoutNotify((_currentScrollBarStep - 1) / ((float)steps));
            }
            else
            {
                _scrollbar.numberOfSteps = 1;
                _scrollbar.gameObject.SetActive(false);
            }

            _ignoreScrollBarEvent = false;
        }

        public void Clear()
        {
            for (int i = 0; i < _visibleMsgCount; i++)
                _msgs[i].rectTransform.gameObject.SetActive(false);

            _msgCount = _visibleMsgCount = 0;
            _firstVisibleMsgIndex = 0;

            _upperMsgOverflow = _lowerMsgOverflow = 0;
            _totalLines = _visibleLines = 0;

            _currentScrollBarStep = 1;

            _ignoreScrollBarEvent = true;
            _scrollbar.numberOfSteps = 0;
            _scrollbar.SetValueWithoutNotify(0);
            _ignoreScrollBarEvent = false;
        }

        public void OnScroll(PointerEventData data)
        {
            if (_totalLines - _visibleLines <= 0)
                return;

            Vector2 delta = data.scrollDelta;
            // Down is positive for scroll events, while in UI system up is positive.
            delta.y *= -1;
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                delta.y = delta.x;

            if (delta.y >= 1)
            {
                _scrollbar.value = Mathf.Clamp01(_scrollbar.value - (1f / (_scrollbar.numberOfSteps - 1)));
            }
            else if (delta.y <= -1)
            {
                _scrollbar.value = Mathf.Clamp01(_scrollbar.value + (1f / (_scrollbar.numberOfSteps - 1)));
            }
        }

        private void OnScrollBarValueChanged(float value)
        {
            if (_ignoreScrollBarEvent) //bar shots event on steps update...
                return;

            var nextStep = (int)Mathf.Lerp(1, _scrollbar.numberOfSteps, value);
            if (nextStep == _currentScrollBarStep)
                return;

            if (nextStep > _currentScrollBarStep) //we moved down
            {
                int toHideCount = nextStep - _currentScrollBarStep;
                while (toHideCount-- > 0)
                    ShiftUp();
            }
            else //we moved up
            {
                int toHideCount = _currentScrollBarStep - nextStep;
                while (toHideCount-- > 0)
                    ShiftDown();
            }

            _currentScrollBarStep = nextStep;
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            const CanvasFilter mask = ~(CanvasFilter.NpcDialog);

            return (mask & UIController.Panel.CanvasFilter) == 0;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Common.Globals.UI.IsOverChatBox = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Common.Globals.UI.IsOverChatBox = false;
        }
    }
}
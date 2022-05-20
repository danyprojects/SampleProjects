using System;
using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public class ListScrollView : MonoBehaviour
        , IPointerDownHandler, IPointerUpHandler
    {
        public interface ISlot
        {
            RectTransform RectTransform { get; }
            void Fill(int index);

            void Init(); // called once during scrollview construction 
        }

        public enum Direction
        {
            BottomToTop = 2,
            TopToBottom = 3
        }

        [SerializeField]
        private Direction _direction = Direction.TopToBottom;

        [SerializeField]
        private Vector2Int _padding = Vector2Int.zero;

        [SerializeField]
        private int _visibleRows = default;
        public int VisibleRows
        {
            get { return _visibleRows; }
            set { SetVisibleRows(value); }
        }

        [SerializeField]
        private int _maxRows = default;
        public int MaxRows => _maxRows;

        public int RowHeight => Math.Abs(_entryHeight);

        [SerializeField]
        private RectTransform _slot = default; // MUST be an ISlot

        [SerializeField]
        private Scrollbar _scrollbar = default;

        private ISlot[] _slots;
        private int _entryCount = 0;
        private int _firstVisibleEntryIndex = 0;

        private int _currentBarStep = 1;
        private int _entryHeight;
        private bool _ignoreScrollbarEvent = false;

        // this allows it to act as a view over an arbitrary array chunk and not needing extra index translation tables
        private int _customListOffset = 0;
        private UIController.Panel _panel;
        private float _scrollDir;

        private void Start()
        {
            var dragController = GetComponentInParent<DragAreaController>();

            foreach (var slot in _slots)
            {
                var graphics = _slot.GetComponentsInChildren<Graphic>(true);
                foreach (var graphic in graphics)
                    dragController.AddGraphic(graphic);
            }
        }

        public void Init() // To get around initialization order issues we don't use awake
        {
            _scrollbar.onValueChanged.AddListener(OnScrollBarValueChanged);
            _scrollbar.direction = (Scrollbar.Direction)_direction;
            _panel = GetComponentInParent<UIController.Panel>();

            _slot.gameObject.SetActive(false);

            if (_direction == Direction.TopToBottom)
            {
                _slot.pivot = Vector2.up;
                _slot.anchorMin = Vector2.up;
                _slot.anchorMax = Vector2.up;
                _entryHeight = (int)_slot.rect.height;
                _scrollDir = 1;
            }
            else
            {
                _slot.pivot = Vector2.zero;
                _slot.anchorMin = Vector2.zero;
                _slot.anchorMax = Vector2.zero;
                _entryHeight = (int)-_slot.rect.height;
                _scrollDir = -1;
            }


            _slot.anchoredPosition = _padding;
            _slots = new ISlot[_maxRows];
            _slots[0] = _slot.GetComponent<ISlot>();
            _slots[0].Init();

            Debug.Assert(_slots[0] != null);

            for (int i = 1; i < _maxRows; i++)
            {
                _slots[i] = Instantiate(_slot.gameObject, gameObject.transform).GetComponent<ISlot>();
                _slots[i].Init();
                _slots[i].RectTransform.anchoredPosition -= new Vector2(0, _entryHeight * i);
            }
            _scrollbar.transform.SetAsLastSibling();
        }

        // Always returns first slot if requested index is not visible
        public Vector2 GetIndexAnchorPosition(int index)
        {
            index -= _customListOffset;

            if (index < _firstVisibleEntryIndex || index >= _firstVisibleEntryIndex + VisibleRows)
                return _slots[0].RectTransform.anchoredPosition;

            return _slots[index - _firstVisibleEntryIndex].RectTransform.anchoredPosition;
        }

        public void AddEntry(int index)
        {
            index -= _customListOffset;

            _entryCount++;
            UpdateScrollBar();

            //we always add at end 
            if (index >= _firstVisibleEntryIndex + VisibleRows)
                return;

            _slots[index - _firstVisibleEntryIndex].Fill(index + _customListOffset);
            _slots[index - _firstVisibleEntryIndex].RectTransform.gameObject.SetActive(true);
        }

        public void RemoveEntry(int index)
        {
            index -= _customListOffset;

            _entryCount--;
            UpdateScrollBar();

            if (index < _firstVisibleEntryIndex || index >= _firstVisibleEntryIndex + VisibleRows)
                return;

            var tmp = _slots[_firstVisibleEntryIndex - index];
            tmp.RectTransform.gameObject.SetActive(false);
            tmp.RectTransform.anchoredPosition = _slots[_slots.Length - 1].RectTransform.anchoredPosition;

            for (int i = _firstVisibleEntryIndex - index; i < _slots.Length - 1; ++i)
            {
                _slots[i] = _slots[i + 1];
                _slots[i].RectTransform.anchoredPosition += new Vector2(0, _entryHeight);
            }

            _slots[_slots.Length - 1] = tmp;
        }

        private void SetVisibleRows(int value)
        {
            if (value == _visibleRows)
                return;

            var old = _visibleRows;

            _visibleRows = value;
            UpdateScrollBar();

            if (old > value) //we need to disable some entries
            {
                for (int i = 0; i < old - value; i++)
                    _slots[old - 1 - i].RectTransform.gameObject.SetActive(false);
            }
            else if (old < value) //we need to enable some entries
            {
                for (int i = 0; i < value - old; i++)
                {
                    if (old + i < _entryCount)
                    {
                        _slots[old + i].Fill(_firstVisibleEntryIndex + old + i);
                        _slots[old + i].RectTransform.gameObject.SetActive(true);
                    }
                }
            }
        }

        // assuming the entries have already been swaped in original array, before calling this
        // fillEntry may be triggered so index1 and index2 should be the indexes as they were BEFORE the original array swap
        public void Swap(int index1, int index2)
        {
            index1 -= _customListOffset;
            index2 -= _customListOffset;

            // index 1 not visible
            if (index1 < _firstVisibleEntryIndex || index1 >= _firstVisibleEntryIndex + _visibleRows)
            {
                if (index2 < _firstVisibleEntryIndex || index2 >= _firstVisibleEntryIndex + _visibleRows)
                    return; //both not visible

                _slots[_firstVisibleEntryIndex - index2].Fill(index2 + _customListOffset);
            }
            // index 2 not visible
            else if (index2 < _firstVisibleEntryIndex || index2 >= _firstVisibleEntryIndex + _visibleRows)
            {
                if (index1 < _firstVisibleEntryIndex || index1 >= _firstVisibleEntryIndex + _visibleRows)
                    return; //both not visible

                _slots[_firstVisibleEntryIndex - index1].Fill(index1 + _customListOffset);
            }
            // both are visible
            else
            {
                var tmp1 = _slots[_firstVisibleEntryIndex - index1];
                var tmp2 = _slots[_firstVisibleEntryIndex - index2];
                var tmpPos = tmp1.RectTransform.anchoredPosition;

                tmp1.RectTransform.anchoredPosition = tmp2.RectTransform.anchoredPosition;
                tmp2.RectTransform.anchoredPosition = tmpPos;
            }
        }

        // customListOffset value will be deducted from any index related calls, and added to the fillentry Cb
        public void Clear(int customOffset = 0)
        {
            for (int i = 0; i < _visibleRows; i++)
                _slots[i].RectTransform.gameObject.SetActive(false);

            _entryCount = 0;
            _firstVisibleEntryIndex = 0;

            _currentBarStep = 1;
            _ignoreScrollbarEvent = true;
            _scrollbar.numberOfSteps = 0;
            _scrollbar.SetValueWithoutNotify(0);
            _ignoreScrollbarEvent = false;
            _customListOffset = customOffset;
        }

        // ask scrollview to redraw visible slots
        public void Redraw()
        {
            for (int i = 0; i < _visibleRows && i < _entryCount; i++)
            {
                _slots[i].RectTransform.gameObject.SetActive(false);
                _slots[i].Fill(_firstVisibleEntryIndex + i + _customListOffset);
                _slots[i].RectTransform.gameObject.SetActive(true);
            }
        }

        // redraws a valid view containing the requested index
        public void RedrawAt(int index)
        {
            index -= _customListOffset;

            var tmpIndex = _entryCount - _visibleRows;
            if (tmpIndex > 0) // we have more entries that visible rows
            {
                _firstVisibleEntryIndex = Math.Min(tmpIndex, index);
                _currentBarStep = _firstVisibleEntryIndex + 1;
            }
            else
            {
                _firstVisibleEntryIndex = 0;
                _currentBarStep = 1;
            }

            UpdateScrollBar();

            Redraw();
        }

        public void OnScroll(PointerEventData data)
        {
            if (_entryCount - _visibleRows <= 0)
                return;

            Vector2 delta = data.scrollDelta;
            // Down is positive for scroll events, while in UI system up is positive.
            delta.y *= -1;
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                delta.y = delta.x;

            if (delta.y >= 1)
            {
                _scrollbar.value = Mathf.Clamp01(_scrollbar.value + (_scrollDir / (_scrollbar.numberOfSteps - 1)));
            }
            else if (delta.y <= -1)
            {
                _scrollbar.value = Mathf.Clamp01(_scrollbar.value - (_scrollDir / (_scrollbar.numberOfSteps - 1)));
            }
        }

        private void UpdateScrollBar()
        {
            _ignoreScrollbarEvent = true;

            int steps = _entryCount - _visibleRows;
            if (steps > 0)
            {
                _scrollbar.numberOfSteps = steps + 1;

                _scrollbar.size = _visibleRows / (float)(_entryCount);
                if (!_scrollbar.gameObject.activeSelf)
                {
                    _scrollbar.gameObject.SetActive(true);
                    _scrollbar.SetValueWithoutNotify(0f);
                }
                else
                {
                    _scrollbar.SetValueWithoutNotify((_currentBarStep - 1) / ((float)steps));
                }
            }
            else
                _scrollbar.gameObject.SetActive(false);

            _ignoreScrollbarEvent = false;
        }

        private void StepDown()
        {
            ISlot tmp = _slots[0];
            tmp.RectTransform.gameObject.SetActive(false);
            tmp.RectTransform.anchoredPosition = _slots[_visibleRows - 1].RectTransform.anchoredPosition;

            //push old elements to begin on array
            for (int i = 0; i < _visibleRows - 1; i++)
            {
                _slots[i] = _slots[i + 1];
                _slots[i].RectTransform.anchoredPosition += new Vector2(0, _entryHeight);
            }

            _slots[_visibleRows - 1] = tmp;
            tmp.Fill(_firstVisibleEntryIndex + _visibleRows + _customListOffset);
            tmp.RectTransform.gameObject.SetActive(true);

            _firstVisibleEntryIndex += 1;
        }

        private void StepUp()
        {
            ISlot tmp = _slots[_visibleRows - 1];
            tmp.RectTransform.gameObject.SetActive(false);
            tmp.RectTransform.anchoredPosition = _slots[0].RectTransform.anchoredPosition;

            //push old elements to end on array
            for (int i = _visibleRows - 1; i > 0; i--)
            {
                _slots[i] = _slots[i - 1];
                _slots[i].RectTransform.anchoredPosition -= new Vector2(0, _entryHeight);
            }

            _slots[0] = tmp;
            _firstVisibleEntryIndex -= 1;
            tmp.Fill(_firstVisibleEntryIndex + _customListOffset);
            tmp.RectTransform.gameObject.SetActive(true);
        }

        private void OnScrollBarValueChanged(float value)
        {
            if (_ignoreScrollbarEvent) //bar shots event on steps update...
                return;

            var nextStep = (int)Mathf.Lerp(1, _scrollbar.numberOfSteps, value);
            if (nextStep == _currentBarStep)
                return;

            if (nextStep > _currentBarStep) //we moved down
            {
                int toHideCount = nextStep - _currentBarStep;
                while (toHideCount-- > 0)
                    StepDown();
            }
            else //we moved up
            {
                int toHideCount = _currentBarStep - nextStep;
                while (toHideCount-- > 0)
                    StepUp();
            }

            _currentBarStep = nextStep;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _panel.BringToFront();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // don't let event through
        }
    }
}
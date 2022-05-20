using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public class MultiColumnListScrollView : MonoBehaviour
        , IPointerDownHandler, IPointerUpHandler
    {
        public interface ISlot
        {
            RectTransform rectTransform { get; }
            void fill(int index);
        }

        [SerializeField]
        private Scrollbar scrollbar = default;

        [SerializeField]
        private float entryHeight = default;

        [SerializeField]
        private float horizontalSpace = default;

        [SerializeField]
        private float verticalSpace = default;

        [field: SerializeField]
        public int VisibleRows { get; private set; }

        [field: SerializeField]
        public int VisibleColumns { get; private set; }

        private ISlot[] buffer;
        private int entryCount = 0;
        private int firstVisibleEntryIndex = 0; //the pivot index for the original array

        private ISlot[] tmpBuffer;
        private Vector2[] tmpPositions;

        private int currentBarStep = 1;

        private bool ignoreScrollbarEvent = false;

        // this allows it to act as a view over an arbitrary array chunk and not needing extra index translation tables
        private int customListOffset = 0;

        private UIController.Panel _panel;
        public void Awake()
        {
            scrollbar.onValueChanged.AddListener(onScrollBarValueChanged);
            _panel = GetComponentInParent<UIController.Panel>();
        }

        public void Init(ISlot[] slots)
        {
            buffer = slots;
            tmpBuffer = new ISlot[buffer.Length];
            tmpPositions = new Vector2[buffer.Length];
        }

        public void AddEntry(int index)
        {
            index -= customListOffset;

            entryCount++;
            updateScrollBar();

            //we always add at end, if entry can be shown early exit
            if (index >= firstVisibleEntryIndex + VisibleRows * VisibleColumns)
                return;

            buffer[index - firstVisibleEntryIndex].fill(index + customListOffset);
            buffer[index - firstVisibleEntryIndex].rectTransform.gameObject.SetActive(true);
        }

        public void RemoveEntry(int index)
        {
            index -= customListOffset;

            //if entry removed is not displayed, early exit
            if (index < firstVisibleEntryIndex || index >= firstVisibleEntryIndex + VisibleRows * VisibleColumns)
            {
                entryCount--;
                updateScrollBar();
            }
            else
            {
                int last = calculateLastVisibleEntryIndex();

                entryCount--;
                updateScrollBar();

                // save data for entry to be removed
                var tmp = buffer[index];
                var tmpPosition = buffer[last].rectTransform.anchoredPosition;

                tmp.rectTransform.gameObject.SetActive(false);

                // pull everything back
                for (int i = last; i > index; --i)
                    buffer[i].rectTransform.anchoredPosition = buffer[i - 1].rectTransform.anchoredPosition;

                for (int i = index; i < last; ++i)
                    buffer[i] = buffer[i + 1];

                tmp.rectTransform.anchoredPosition = tmpPosition;
                buffer[last] = tmp;

                //restore last entry, if needed
                if (firstVisibleEntryIndex + last < entryCount)
                {
                    buffer[last].fill(firstVisibleEntryIndex + last + customListOffset);
                    buffer[last].rectTransform.gameObject.SetActive(true);
                }
            }
        }

        // assuming the entries have already been swaped in original array, before calling this
        // fillEntry may be triggered so index1 and index2 should be the indexes as they were BEFORE the original array swap
        public void Swap(int index1, int index2)
        {
            index1 -= customListOffset;
            index2 -= customListOffset;

            // index 1 not visible
            if (index1 < firstVisibleEntryIndex || index1 >= firstVisibleEntryIndex + VisibleRows * VisibleColumns)
            {
                if (index2 < firstVisibleEntryIndex || index2 >= firstVisibleEntryIndex + VisibleRows * VisibleColumns)
                    return; //both not visible

                buffer[firstVisibleEntryIndex - index2].fill(index2 + customListOffset);
            }
            // index 2 not visible
            else if (index2 < firstVisibleEntryIndex || index2 >= firstVisibleEntryIndex + VisibleRows * VisibleColumns)
            {
                if (index1 < firstVisibleEntryIndex || index1 >= firstVisibleEntryIndex + VisibleRows * VisibleColumns)
                    return; //both not visible

                buffer[firstVisibleEntryIndex - index1].fill(index1 + customListOffset);
            }
            // both are visible
            else
            {
                var tmp1 = buffer[firstVisibleEntryIndex - index1];
                var tmp2 = buffer[firstVisibleEntryIndex - index2];
                var tmpPos = tmp1.rectTransform.anchoredPosition;

                tmp1.rectTransform.anchoredPosition = tmp2.rectTransform.anchoredPosition;
                tmp2.rectTransform.anchoredPosition = tmpPos;
            }
        }

        public void SetDimentions(int rows, int columns)
        {
            var oldRows = VisibleRows;
            var oldColumns = VisibleColumns;

            if (rows == oldRows && oldColumns == columns)
                return;

            VisibleRows = rows;
            VisibleColumns = columns;
            updateScrollBar();

            var oldCount = oldRows * oldColumns;
            var newCount = VisibleRows * VisibleColumns;

            if (oldCount > newCount) //we need to disable some entries
            {
                for (int i = 0; i < oldCount - newCount; i++)
                    buffer[oldCount - 1 - i].rectTransform.gameObject.SetActive(false);
            }
            else if (oldCount < newCount) //we need to enable some entries
            {
                for (int i = 0; i < newCount - oldCount; i++)
                {
                    if (oldCount + i < entryCount)
                    {
                        buffer[oldCount + i].fill(firstVisibleEntryIndex + oldCount + i + customListOffset);
                        buffer[oldCount + i].rectTransform.gameObject.SetActive(true);
                    }
                }
            }

            Vector2 refPos = buffer[0].rectTransform.anchoredPosition;

            //just update every entry position for simplicity
            for (int i = 0; i < VisibleRows; i++)
            {
                for (int j = 0; j < VisibleColumns; j++)
                {
                    buffer[i * columns + j].rectTransform.anchoredPosition = refPos + new Vector2((entryHeight + verticalSpace) * j, -(entryHeight + horizontalSpace) * i);
                }
            }
        }

        // customListOffset value will be deducted from any index related calls, and added to the fillentry Cb
        public void Clear(int customOffset = 0)
        {
            int last = calculateLastVisibleEntryIndex();
            for (int i = 0; i <= last; i++)
                buffer[i].rectTransform.gameObject.SetActive(false);

            entryCount = 0;
            firstVisibleEntryIndex = 0;

            currentBarStep = 1;
            ignoreScrollbarEvent = true;
            scrollbar.numberOfSteps = 0;
            scrollbar.SetValueWithoutNotify(0);
            ignoreScrollbarEvent = false;
            customListOffset = customOffset;
        }

        // ask scrollview to redraw visible slots
        public void Redraw()
        {
            for (int i = 0; i < VisibleRows; i++)
            {
                for (int j = 0; j < VisibleColumns; j++)
                {
                    var index = i * VisibleColumns + j;
                    buffer[index].rectTransform.gameObject.SetActive(false);
                    buffer[index].fill(firstVisibleEntryIndex + index + customListOffset);
                    buffer[index].rectTransform.gameObject.SetActive(true);
                }
            }
        }

        public void OnScroll(PointerEventData data)
        {
            if (entryCount - VisibleRows * VisibleColumns <= 0)
                return;

            Vector2 delta = data.scrollDelta;
            // Down is positive for scroll events, while in UI system up is positive.
            delta.y *= -1;
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                delta.y = delta.x;

            if (delta.y >= 1)
            {
                scrollbar.value = Mathf.Clamp01(scrollbar.value + (1f / (scrollbar.numberOfSteps - 1)));
            }
            else if (delta.y <= -1)
            {
                scrollbar.value = Mathf.Clamp01(scrollbar.value - (1f / (scrollbar.numberOfSteps - 1)));
            }
        }

        private int calculateLastVisibleEntryIndex()
        {
            // calculate index of last element
            int last = entryCount - firstVisibleEntryIndex - 1;
            if (last > VisibleRows * VisibleColumns - 1)
                last = VisibleRows * VisibleColumns - 1;

            return last;
        }

        private void updateScrollBar()
        {
            ignoreScrollbarEvent = true;

            int steps = (entryCount - VisibleRows * VisibleColumns);
            if (steps > 0)
            {
                // only consider each new row a step
                steps = steps % VisibleColumns == 0 ? steps / VisibleColumns : steps / VisibleColumns + 1;

                scrollbar.numberOfSteps = steps + 1;

                scrollbar.size = VisibleRows / Mathf.Ceil((float)(entryCount) / VisibleColumns);
                if (!scrollbar.gameObject.activeSelf)
                {
                    scrollbar.gameObject.SetActive(true);
                    scrollbar.SetValueWithoutNotify(0f);
                }
                else
                {
                    scrollbar.SetValueWithoutNotify((currentBarStep - 1) / ((float)steps));
                }
            }
            else
                scrollbar.gameObject.SetActive(false);

            ignoreScrollbarEvent = false;
        }

        private void stepDown()
        {
            // calculate index of last element
            int last = calculateLastVisibleEntryIndex();

            // save first row
            for (int i = 0; i < VisibleColumns; i++)
            {
                tmpBuffer[i] = buffer[i];
                tmpBuffer[i].rectTransform.gameObject.SetActive(false);
                tmpPositions[i] = buffer[last - i].rectTransform.anchoredPosition;
            }

            // shift position of all elements past first row upwards
            for (int i = last; i >= VisibleColumns; i--)
                buffer[i].rectTransform.anchoredPosition = buffer[i - VisibleColumns].rectTransform.anchoredPosition;

            firstVisibleEntryIndex += VisibleColumns;

            // push all elements past first row upwards in buffer
            for (int i = 0; i <= last - VisibleColumns; i++)
                buffer[i] = buffer[i + VisibleColumns];

            // push first row down
            for (int i = 0; i < VisibleColumns; i++)
            {
                tmpBuffer[i].rectTransform.anchoredPosition = tmpPositions[i];
                buffer[last - i] = tmpBuffer[i];

                var entryIndex = firstVisibleEntryIndex + last - i;
                if (entryIndex < entryCount)
                {
                    buffer[last - i].fill(entryIndex + customListOffset);
                    buffer[last - i].rectTransform.gameObject.SetActive(true);
                }
            }
        }

        private void stepUp()
        {
            // calculate index of last element
            int last = calculateLastVisibleEntryIndex();

            // save existing last row entries
            for (int i = 0; i < VisibleColumns; i++)
            {
                tmpBuffer[i] = buffer[last - i];
                tmpBuffer[i].rectTransform.gameObject.SetActive(false);
                tmpPositions[i] = buffer[i].rectTransform.anchoredPosition;
            }

            // shift position of all elements until last row downwards
            for (int i = 0; i <= last - VisibleColumns; i++)
                buffer[i].rectTransform.anchoredPosition = buffer[i + VisibleColumns].rectTransform.anchoredPosition;

            // push all elements before last row downwards in buffer
            for (int i = last; i >= VisibleColumns; i--)
                buffer[i] = buffer[i - VisibleColumns];

            firstVisibleEntryIndex -= VisibleColumns;

            // push last row down and init new entries
            for (int i = 0; i < VisibleColumns; i++)
            {
                tmpBuffer[i].rectTransform.anchoredPosition = tmpPositions[i];
                buffer[i] = tmpBuffer[i];

                tmpBuffer[i].fill(firstVisibleEntryIndex + i + customListOffset);
                tmpBuffer[i].rectTransform.gameObject.SetActive(true);
            }

            // check if we can reconstruct some wiped ou entries
            for (int i = last; i < VisibleColumns * VisibleRows; i++)
            {
                var entryIndex = firstVisibleEntryIndex + i;
                if (entryIndex < entryCount)
                {
                    buffer[i].fill(entryIndex + customListOffset);
                    buffer[i].rectTransform.gameObject.SetActive(true);
                }
            }
        }

        private void onScrollBarValueChanged(float value)
        {
            if (ignoreScrollbarEvent) //bar shots event on steps update...
                return;

            var nextStep = (int)Mathf.Lerp(1, scrollbar.numberOfSteps, value);
            if (nextStep == currentBarStep)
                return;

            if (nextStep > currentBarStep) //we moved down
            {
                int toHideCount = nextStep - currentBarStep;
                while (toHideCount-- > 0)
                    stepDown();
            }
            else //we moved up
            {
                int toHideCount = currentBarStep - nextStep;
                while (toHideCount-- > 0)
                    stepUp();
            }

            currentBarStep = nextStep;
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
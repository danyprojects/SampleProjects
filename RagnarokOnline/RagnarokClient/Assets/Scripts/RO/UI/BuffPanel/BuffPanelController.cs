using RO.Common;
using RO.Databases;
using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public sealed partial class BuffPanelController : UIController.Panel
    {
        private const int MAX_BUFFS = 20;

        private struct PriorityIndex
        {
            public byte start;
            public byte end;
        }

        //Variables to configure the sizing of the panel
        private readonly Vector2 _startPosition = new Vector2(0, -Screen.height * 0.2f); //20% in height
        private const float GAP_PER_BUFF = 5, BUFF_DIMENSION = 32, SPACE_PER_BUFF = GAP_PER_BUFF + BUFF_DIMENSION;

        //Array with all buff sprite icons
        [SerializeField] private Sprite[] _buffIcons = default;

        //Arrays for each buff game object 
        [SerializeField] private GameObject _originalUiBuff = default;
        private Image[] _buffUiImages = new Image[MAX_BUFFS];
        private BuffSlotController[] _buffSlotControllers = new BuffSlotController[MAX_BUFFS];

        //Data for insertions and lookups
        private BuffIDs[] _buffSlotIds = new BuffIDs[MAX_BUFFS];
        private PriorityIndex[] _prioIndices = new PriorityIndex[(int)BuffDb.Priority.Last];

        //Variables to be used for shader
        private Color[] _highlightColors = new Color[(int)BuffDb.Priority.Last];
        private float[] _startTimes = new float[MAX_BUFFS];
        private float[] _durations = new float[MAX_BUFFS];

        private void Awake()
        {
            //Init the colors that will be highlited on top of each buff priority if they have a cooldown
            _highlightColors[(int)BuffDb.Priority.Top] = Color.white;
            _highlightColors[(int)BuffDb.Priority.Red] = Color.white;
            _highlightColors[(int)BuffDb.Priority.Purple] = new Color(1, 100 / 255f, 60 / 255f);
            _highlightColors[(int)BuffDb.Priority.Yellow] = Color.white;
            _highlightColors[(int)BuffDb.Priority.DarkBlue] = Color.white;
            _highlightColors[(int)BuffDb.Priority.Blue] = new Color(1, 100 / 255f, 60 / 255f);
            _highlightColors[(int)BuffDb.Priority.Green] = Color.white;
            _highlightColors[(int)BuffDb.Priority.DarkYellow] = new Color(1, 100 / 255f, 60 / 255f);
            _highlightColors[(int)BuffDb.Priority.White] = Color.white;

            //Calculate the dimensions of the buff panel and how many buffs per column
            RectTransform trans = (RectTransform)transform;
            trans.anchoredPosition = _startPosition;
            float sizeY = Screen.height + _startPosition.y;
            int buffsPerColumn = (int)(sizeY / SPACE_PER_BUFF);
            float sizeX = (MAX_BUFFS / buffsPerColumn + 1) * SPACE_PER_BUFF + 10;
            trans.sizeDelta = new Vector2(sizeX, sizeY);

            //Init first image from the original
            _buffUiImages[0] = _originalUiBuff.GetComponent<Image>();
            _buffSlotControllers[0] = _originalUiBuff.GetComponent<BuffSlotController>();
            _buffSlotControllers[0].Index = 0;
            _startTimes[0] = 0;
            _durations[0] = float.MaxValue;

            //Then make MAX_BUFFS - 1 copies of it and init them too
            int column = 0;
            for (int i = 1; i < MAX_BUFFS; i++)
            {
                if (i % buffsPerColumn == 0)
                    column++;

                _buffUiImages[i] = Instantiate(_originalUiBuff, transform, false).GetComponent<Image>();
                _buffSlotControllers[i] = _buffUiImages[i].GetComponent<BuffSlotController>();
                _buffSlotControllers[i].Index = i;
                _buffUiImages[i].rectTransform.anchoredPosition = new Vector3(-10 - SPACE_PER_BUFF * column, -(i % buffsPerColumn) * SPACE_PER_BUFF, 0);
                _buffUiImages[i].color = new Color(0, 0, 0, i / 255f);

                _startTimes[i] = 0;
                _durations[i] = float.MaxValue;
            }

            //Send the starting arrays to the shader
            UpdateShaderArrays();
        }

        public override void BringToFront() { }

        public void AddPermanentBuff(BuffIDs buffId)
        {
            if (_buffIcons[(int)buffId] == null)
                return;

            int index = GetFreeIndex(buffId);

            //Update the shader info
            _startTimes[index] = float.MaxValue;
            _durations[index] = 1;
            UpdateShaderArrays();

            //Set the sprite and enable it
            _buffUiImages[index].sprite = _buffIcons[(int)BuffDb.Buffs[(int)buffId].iconId];
            _buffUiImages[index].enabled = true;

            _buffSlotIds[index] = buffId;
        }

        public void AddBuff(BuffIDs buffId, float duration)
        {
            if (_buffIcons[(int)buffId] == null)
                return;

            int index = GetFreeIndex(buffId);

            //Update the shader info
            _startTimes[index] = Globals.TimeSinceLevelLoad;
            _durations[index] = duration;
            UpdateShaderArrays();

            //Set the sprite and enable it
            int prioIndex = (int)BuffDb.Buffs[(int)buffId].priority;
            _buffUiImages[index].sprite = _buffIcons[(int)BuffDb.Buffs[(int)buffId].iconId];
            _buffUiImages[index].color = new Color(_highlightColors[prioIndex].r, _highlightColors[prioIndex].g,
                                        _highlightColors[prioIndex].b, _buffUiImages[index].color.a);
            _buffUiImages[index].enabled = true;
            _buffSlotIds[index] = buffId;
        }

        public void RemoveBuff(BuffIDs buffId)
        {
            //Find the buff in it's prio category
            int prioIndex = (int)BuffDb.Buffs[(int)buffId].priority;
            for (int i = _prioIndices[prioIndex].start; i < _prioIndices[prioIndex].end; i++)
                if (_buffSlotIds[i] == buffId)
                {
                    //Save the last index of last prio to clear it's data afterwards
                    int lastIndex = _prioIndices[(int)BuffDb.Priority.Last - 1].end - 1;

                    //Start shifting from left to right of prio, starting with own prio
                    //In own prio, we might have to start somewhere in the middle. Start copying 1 to the right of what we found
                    LeftShiftInBuffPrio(prioIndex, i - _prioIndices[prioIndex].start + 1);

                    for (int prio = prioIndex + 1; prio < (int)BuffDb.Priority.Last; prio++)
                        LeftShiftInBuffPrio(prio);

                    //Now clear the relevant data from old last index
                    _durations[lastIndex] = 1;
                    _startTimes[lastIndex] = float.MaxValue;
                    _buffSlotIds[lastIndex] = BuffIDs.Last;
                    _buffUiImages[lastIndex].enabled = false;

                    UpdateShaderArrays();

                    return;
                }

            //Do nothing if we didn't find the buff
            //Handling the exit point inside the for safes us an if
        }

        //Gets the index of to write the buff data into. Also shifts the arrays if needed
        private int GetFreeIndex(BuffIDs buffId)
        {
            //Look for buff in it's priority range. If it finds it shift only it's priority range as the sizes don't change
            int prioIndex = (int)BuffDb.Buffs[(int)buffId].priority;
            for (int i = _prioIndices[prioIndex].start; i < _prioIndices[prioIndex].end; i++)
                if (_buffSlotIds[i] == buffId)
                {
                    //Shift left starting at right of found
                    LeftShiftDuringAddInBuffPrio(prioIndex, i - _prioIndices[prioIndex].start + 1);

                    //Return last index of prio
                    return _prioIndices[prioIndex].end - 1;
                }

            //If we didn't find the buff, we need to shift remaining arrays by 1
            //NOTE: We always assume we do not hit the buff cap so we don't have to do bounds check

            //Start shifting from last priority until current so it's safe to  move
            for (int i = (int)BuffDb.Priority.Last - 1; i > prioIndex; i--)
                RightShiftInBuffPrio(i);

            //The end slot is now free to use for sure. We always insert at the end
            int index = _prioIndices[prioIndex].end;

            //After shifting, the last index of last prio might be disabled. If we did at least 1 shift, enabled it to make sure it shows
            if (index != _prioIndices[(int)BuffDb.Priority.Last - 1].end - 1)
                _buffUiImages[_prioIndices[(int)BuffDb.Priority.Last - 1].end - 1].enabled = true;

            //Update end slot counting on insertion
            _prioIndices[prioIndex].end++;

            return index;
        }

        private void RightShiftInBuffPrio(int prioIndex)
        {
            //start shifting from end to start. Does nothing if it's empty
            for (int i = _prioIndices[prioIndex].end - 1; i >= _prioIndices[prioIndex].start; i--)
            {
                _durations[i + 1] = _durations[i];
                _startTimes[i + 1] = _startTimes[i];
                _buffUiImages[i + 1].sprite = _buffUiImages[i].sprite;
                _buffUiImages[i + 1].color = new Color(_buffUiImages[i].color.r, _buffUiImages[i].color.g,
                                                       _buffUiImages[i].color.b, (i + 1) / 255f);
                _buffSlotIds[i + 1] = _buffSlotIds[i];
            }

            //Increment the start and end for next lookup / insertion
            _prioIndices[prioIndex].start++;
            _prioIndices[prioIndex].end++;
        }

        private void LeftShiftInBuffPrio(int prioIndex, int offset = 0)
        {
            LeftShiftDuringAddInBuffPrio(prioIndex, offset);

            //Decrement the start and end for next lookup / insertion
            //This should not be called if there's no buffs so we should always be able to decrement
            if (offset == 0) //If we shifted left from somewhere not in the start then don't change the start
                _prioIndices[prioIndex].start--;
            _prioIndices[prioIndex].end--;
        }

        private void LeftShiftDuringAddInBuffPrio(int prioIndex, int offset)
        {
            //start shifting from start to end. Does nothing if it's empty
            for (int i = _prioIndices[prioIndex].start + offset; i < _prioIndices[prioIndex].end; i++)
            {
                _durations[i - 1] = _durations[i];
                _startTimes[i - 1] = _startTimes[i];
                _buffUiImages[i - 1].sprite = _buffUiImages[i].sprite;
                _buffUiImages[i - 1].color = new Color(_buffUiImages[i].color.r, _buffUiImages[i].color.g,
                                                       _buffUiImages[i].color.b, (i - 1) / 255f);
                _buffSlotIds[i - 1] = _buffSlotIds[i];
            }

            //During add we dont decrement since we'll be adding again
        }

        private void UpdateShaderArrays()
        {
            Shader.SetGlobalFloatArray(Media.MediaConstants.SHADER_UI_BUFF_START_TIMES_ID, _startTimes);
            Shader.SetGlobalFloatArray(Media.MediaConstants.SHADER_UI_BUFF_DURATIONS_ID, _durations);
        }

        public static BuffPanelController Instantiate(UIController uiController, Transform parent)
        {
            var controller = Instantiate<BuffPanelController>(uiController, parent, "BuffPanel");
            controller.gameObject.SetActive(true);

            return controller;
        }
    }
}
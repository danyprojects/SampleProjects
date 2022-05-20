using RO.IO;
using UnityEngine;

namespace RO.UI
{
    public sealed partial class BattleModePanelController : UIController.Panel
        , ICanvasRaycastFilter, IPointerDownHandler
    {
        private const int MAX_SLOTS = 38;

        [SerializeField] private SlotController[] _slots = new SlotController[MAX_SLOTS];
        [SerializeField] private GameObject[] _rows = new GameObject[4];
        [SerializeField] private GameObject _quickPanel = default;
        [SerializeField] private Sprite _emptySlotSprite = default;
        [SerializeField] private Button _closeButton = default;

        private enum VisibilityState
        {
            Hidden = 0,
            UntilRow1,
            UntilRow2,
            UntilRow3,
            UntilRow4,
        }

        private class SlotInfo
        {
            public void Clear()
            {
                _id = -1;
                _isEmpty = true;
                _iconType = IconType.None;
                _count = 0;
                _context = null;
            }

            public short _id = -1;
            public bool _isEmpty = true;
            public IconType _iconType = IconType.None;
            public int _count = 0;     //has skill level /item cardinality
            public object _context = null; //has the skill/item on the slot
            public string _description; // what shows on label 
        }

        private readonly Color _hightlightColor = new Color(181 / 255.0f, 1, 181 / 255.0f, 1);
        private SlotInfo[] _slotInfo = new SlotInfo[MAX_SLOTS];
        private int _rowsVisibleCount = 1;

        BattleModePanelController()
        {
            for (int i = 0; i < _slotInfo.Length; i++)
                _slotInfo[i] = new SlotInfo();
        }

        private void Awake()
        {
            KeyBinder.RegisterAction(KeyBinder.Shortcut.SkillBar, ChangeVisibilityState);

            var index = transform.GetSiblingIndex();

            _quickPanel.GetComponent<QuickSlotsPanel>().ForceSetUiController(UiController);
            _quickPanel.SetActive(gameObject.activeSelf);
            _quickPanel.transform.SetParent(transform.parent); //detach quickslot from ou panel
            _quickPanel.transform.SetSiblingIndex(index + 1);

            GetComponentInChildren<ResizePanel>().OnResize = OnResize;

            _closeButton.OnClick = OnClickClose;
            _closeButton.gameObject.GetComponent<LabelArea>().OnEnter
                = () => LabelController.ShowLabel(_closeButton.transform.position,
                IO.KeyBinder.GetShortcutAsString(IO.KeyBinder.Shortcut.SkillBar), default);
        }

        public new bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            const CanvasFilter filter = ~(CanvasFilter.NpcDialog | CanvasFilter.ItemDrag | CanvasFilter.SkillDrag);

            return (filter & CanvasFilter) == 0;
        }

        private void OnClickClose()
        {
            gameObject.SetActive(false);
        }

        private void ChangeVisibilityState()
        {
            _rowsVisibleCount = (_rowsVisibleCount + 1) % 5;

            if (_rowsVisibleCount == 0)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            ((RectTransform)gameObject.transform).sizeDelta = new Vector2(280, 34 * _rowsVisibleCount);

            int i = 1;
            for (; i < _rowsVisibleCount; i++)
                _rows[i].SetActive(true);

            for (; i < _rows.Length; i++) //disable the rest
                _rows[i].SetActive(false);
        }

        private new void OnEnable()
        {
            base.OnEnable();

            _quickPanel.SetActive(true);
            if (_rowsVisibleCount == 0)
                ChangeVisibilityState();
        }

        private new void OnDisable()
        {
            base.OnDisable();

            _quickPanel.SetActive(false);
            _rowsVisibleCount = 0;
        }

        private void OnResize(int stepX, int stepY)
        {
            _rows[1].SetActive(stepY < 0);
            _rows[2].SetActive(stepY < -1);
            _rows[3].SetActive(stepY < -2);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            BringToFront();
        }

        public static BattleModePanelController Instantiate(UIController uiController, Transform parent)
        {
            var controller = Instantiate<BattleModePanelController>(uiController, parent, "BattleModePanel");

            return controller;
        }
    }
}
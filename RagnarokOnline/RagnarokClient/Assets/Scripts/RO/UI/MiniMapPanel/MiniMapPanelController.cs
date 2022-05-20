using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static RO.Databases.MapDb;

namespace RO.UI
{
    public class MiniMapPanelController : UIController.Panel,
        ICanvasRaycastFilter
    {
#pragma warning disable 0649
        [Serializable]
        public struct Info
        {
            public Sprite Small;
            public Sprite Big;
        }
#pragma warning restore 0649

        [SerializeField]
        private Image _map = default;

        [SerializeField]
        private Button _zoomInButton = default;
        [SerializeField]
        private Button _zoomOutButton = default;
        [SerializeField]
        private Button _infoButton = default;
        [SerializeField]
        private Button _worldMapButton = default;

        [SerializeField]
        private Info _weaponShop = default;
        [SerializeField]
        private Info _armorShop = default;
        [SerializeField]
        private Info _guide = default;
        [SerializeField]
        private Info _inn = default;
        [SerializeField]
        private Info _kafra = default;
        [SerializeField]
        private Info _blacksmith = default;
        [SerializeField]
        private Info _toolDealer = default;
        [SerializeField]
        private Info _stylist = default;
        /*[SerializeField]
        private Sprite _warp = default;*/
        [SerializeField]
        private Sprite _party = default;
        [SerializeField]
        private Sprite _guild = default;
        [SerializeField]
        private RectTransform _playerArrow = default;
        [SerializeField]
        private Material _partyMaterial = default;
        [SerializeField]
        private Color[] _partyColors = new Color[18];

        private int[] _partyColorUsageCount = new int[18];
        private Stack<Image> _mapObjectPool = new Stack<Image>();
        private List<Image> _mapInfoObjects = new List<Image>();
        private Dictionary<int, Image> _mapPartyObjects = new Dictionary<int, Image>();
        private Dictionary<int, Image> _mapGuildObjects = new Dictionary<int, Image>();

        private bool _showInfo = true;
        private int _scaleIndex = 0;
        private readonly float[] _scales = new float[] { 0.25f, 0.4375f, 0.625f, 0.8125f, 1f };
        private float _scale = 0.25f;

        private bool _hasVisibleMap = false;
        private VisibilityState _visibilityState = VisibilityState.Visible;
        private float _mapHeight;
        private float _mapWidth;
        private int _mapTextHeight;
        private int _mapTextWidth;

        private Vector2Int _playerPos = new Vector2Int(-1, -1);

        public enum InfoIcon
        {
            Armor_Shop,
            Guide,
            Inn,
            Kafra,
            Blacksmith,
            Tool_Dealer,
            Stylist,
            Weapon_Shop
        }

        private enum VisibilityState
        {
            Visible,
            Hidden,
            Transparent,
        }

        private void Awake()
        {
            // We need to reparent due to minimap mask2D
            _zoomInButton.gameObject.transform.SetParent(transform.parent);
            _zoomInButton.transform.SetSiblingIndex(transform.GetSiblingIndex());

            _zoomOutButton.gameObject.transform.SetParent(transform.parent);
            _zoomOutButton.transform.SetSiblingIndex(transform.GetSiblingIndex());

            _infoButton.gameObject.transform.SetParent(transform.parent);
            _infoButton.transform.SetSiblingIndex(transform.GetSiblingIndex());

            _worldMapButton.gameObject.transform.SetParent(transform.parent);
            _worldMapButton.transform.SetSiblingIndex(transform.GetSiblingIndex());

            _zoomInButton.OnClick = OnZoomIn;
            _zoomOutButton.OnClick = OnZoomOut;
            _infoButton.OnClick = OnInfo;
            _worldMapButton.OnClick = OnWorlMap;

            _playerArrow.gameObject.GetComponent<LabelArea>().OnEnter = () => { };

            for (int i = 0; i < 10; i++)
            {
                var obj = Instantiate(_playerArrow.gameObject, _map.transform, false);
                obj.SetActive(false);

                _mapObjectPool.Push(obj.GetComponent<Image>());
            }

            IO.KeyBinder.RegisterAction(IO.KeyBinder.Shortcut.MiniMap, OnChangeVisibility);
        }

        private new void OnEnable()
        {
            base.OnEnable();

            _zoomInButton.gameObject.SetActive(true);
            _zoomOutButton.gameObject.SetActive(true);
            _infoButton.gameObject.SetActive(true);
            _worldMapButton.gameObject.SetActive(true);
        }

        private new void OnDisable()
        {
            base.OnDisable();

            _zoomInButton.gameObject.SetActive(false);
            _zoomOutButton.gameObject.SetActive(false);
            _infoButton.gameObject.SetActive(false);
            _worldMapButton.gameObject.SetActive(false);
        }

        public override void BringToFront() { }

        // Also activates minimap unless player has it hidden
        public void Load(MapIds mapId, int mapWidth, int mapHeight)
        {
            _mapHeight = mapHeight;
            _mapWidth = mapWidth;

            var map = AssetBundleProvider.LoadMiniMapBundleAsset(MapNames[(int)mapId]);
            if (map == null)
            {
                gameObject.SetActive(false);
                _hasVisibleMap = false;
                return;
            }
            _hasVisibleMap = true;

            _map.sprite = map;
            _map.SetNativeSize();
            _mapTextHeight = map.texture.height;
            _mapTextWidth = map.texture.width;

            ClearObjects();
            if (_visibilityState != VisibilityState.Hidden)
                gameObject.SetActive(true);
        }

        public void UpdatePlayerDirection(int direction)
        {
            _playerArrow.eulerAngles = new Vector3(0, 0, -45 * direction);
        }

        public void UpdatePlayerPosition(Vector2Int position)
        {
            if (_playerPos == position)
                return;
            _playerPos = position;

            RepositionObject(_playerArrow, position);

            if (_scaleIndex == 0)
                return;

            float clampedX = Mathf.Clamp(_playerArrow.anchoredPosition.x, 64 * 1 / _scale, _mapTextHeight - 64 * 1 / _scale);
            float clampedY = Mathf.Clamp(_playerArrow.anchoredPosition.y, 64 * 1 / _scale, _mapTextWidth - 64 * 1 / _scale);

            _map.rectTransform.pivot = new Vector2(clampedX / _mapTextWidth, clampedY / _mapTextHeight);
        }

        public void AddParty(int id, Vector2Int position)
        {
            int lowestCount = _partyColorUsageCount[0];
            int index = 0;
            for (int i = 1; i < _partyColorUsageCount.Length; i++)
            {
                if (_partyColorUsageCount[i] < lowestCount)
                {
                    lowestCount = _partyColorUsageCount[i];
                    index = i;
                }
            }

            int rand = UnityEngine.Random.Range(0, 17);
            if (_partyColorUsageCount[rand] == lowestCount)
                index = rand;

            _partyColorUsageCount[index]++;

            var party = CreateObject(_party, position, false);
            party.material = _partyMaterial;
            party.color = _partyColors[index];
            party.transform.SetAsFirstSibling();

            _mapPartyObjects.Add(id, party);
        }

        public void RemoveParty(int id)
        {
            if (_mapPartyObjects.TryGetValue(id, out var party))
            {
                for (int i = 0; i < _partyColors.Length; i++)
                {
                    if (_partyColors[i] == party.color)
                    {
                        _partyColorUsageCount[i]--;
                        break;
                    }
                }

                party.gameObject.SetActive(false);
                _mapObjectPool.Push(party);
            }
        }

        public void UpdatePartyPosition(int id, Vector2Int position)
        {
            if (_mapPartyObjects.TryGetValue(id, out var party))
                RepositionObject(party.rectTransform, position);
        }

        public void AddGuild(int id, Vector2Int position)
        {
            var guild = CreateObject(_guild, position, false);

            guild.transform.SetAsFirstSibling();
            _mapGuildObjects.Add(id, guild);
        }

        public void RemoveGuild(int id)
        {
            if (_mapGuildObjects.TryGetValue(id, out var guild))
            {
                guild.gameObject.SetActive(false);
                _mapObjectPool.Push(guild);
            }
        }

        public void UpdateGuildPosition(int id, Vector2Int position)
        {
            if (_mapGuildObjects.TryGetValue(id, out var guild))
                RepositionObject(guild.rectTransform, position);
        }

        public void AddInfoIcon(InfoIcon infoIcon, Vector2Int position)
        {
            Info info = GetInfo(infoIcon);
            var obj = CreateObject(info.Small, position, true, info.Big);

            obj.GetComponent<LabelArea>().OnEnter = () =>
            {
                LabelController.ShowLabel(obj.transform.position, infoIcon.ToString().Replace('_', ' '),
                    new Vector2(-obj.rectTransform.rect.width / 2, obj.rectTransform.rect.height / 2));
            };

            obj.transform.SetSiblingIndex(_playerArrow.GetSiblingIndex());
            _mapInfoObjects.Add(obj);
        }

        private Info GetInfo(InfoIcon infoIcon)
        {
            switch (infoIcon)
            {
                case InfoIcon.Armor_Shop:
                    return _armorShop;
                case InfoIcon.Guide:
                    return _guide;
                case InfoIcon.Inn:
                    return _inn;
                case InfoIcon.Kafra:
                    return _kafra;
                case InfoIcon.Blacksmith:
                    return _blacksmith;
                case InfoIcon.Tool_Dealer:
                    return _toolDealer;
                case InfoIcon.Stylist:
                    return _stylist;
                case InfoIcon.Weapon_Shop:
                    return _weaponShop;
                default:
                    throw new InvalidOperationException();
            }
        }

        private Image CreateObject(Sprite sprite, Vector2Int position, bool raycastTarget, Sprite hoverSprite = null)
        {
            var obj = _mapObjectPool.Count > 0 ? _mapObjectPool.Pop()
                : Instantiate(_playerArrow.gameObject, _map.transform, false).GetComponent<Image>();

            obj.sprite = sprite;
            obj.SetNativeSize();
            obj.raycastTarget = raycastTarget;
            obj.transform.localScale = _playerArrow.localScale;
            obj.material = null;
            obj.color = Color.white;
            obj.gameObject.GetComponent<MiniMapObject>().OnHoverSprite = hoverSprite;
            obj.gameObject.GetComponent<LabelArea>().OnEnter = () => { };

            RepositionObject(obj.rectTransform, position);
            obj.gameObject.SetActive(_showInfo);

            return obj;
        }

        private void RepositionObject(RectTransform rect, Vector2Int position)
        {
            float posX = Mathf.Lerp(0, _mapTextWidth, position.x / _mapWidth);
            float posY = Mathf.Lerp(0, _mapTextHeight, position.y / _mapHeight);

            rect.anchoredPosition = new Vector2(posX, posY);
        }

        private void RescaleMap()
        {
            _map.rectTransform.localScale = new Vector3(_scale, _scale, _scale);
            float inverseScale = 1 / _scale;
            var inverseScaleVect = new Vector3(inverseScale, inverseScale, inverseScale);

            _playerArrow.localScale = inverseScaleVect;

            // Update scale of every object
            foreach (var party in _mapPartyObjects.Values)
                party.rectTransform.localScale = inverseScaleVect;

            foreach (var guild in _mapGuildObjects.Values)
                guild.rectTransform.localScale = inverseScaleVect;

            foreach (var info in _mapInfoObjects)
                info.rectTransform.localScale = inverseScaleVect;

            if (_scaleIndex == 0)
            {
                _map.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                _map.rectTransform.anchoredPosition = Vector2.zero;
            }
            else
            {
                // force next update to center map on player
                _playerPos = new Vector2Int(-1, -1);
            }
        }

        private void ClearObjects()
        {
            foreach (var party in _mapPartyObjects.Values)
            {
                party.gameObject.SetActive(false);
                _mapObjectPool.Push(party);
            }
            _mapPartyObjects.Clear();
            Array.Clear(_partyColorUsageCount, 0, _partyColorUsageCount.Length);

            foreach (var guild in _mapGuildObjects.Values)
            {
                guild.gameObject.SetActive(false);
                _mapObjectPool.Push(guild);
            }
            _mapGuildObjects.Clear();

            foreach (var info in _mapInfoObjects)
            {
                info.gameObject.SetActive(false);
                _mapObjectPool.Push(info);
            }
            _mapInfoObjects.Clear();
        }

        private void OnZoomIn()
        {
            if (_scaleIndex < 4)
            {
                _scale = _scales[++_scaleIndex];
                RescaleMap();
            }
        }

        private void OnZoomOut()
        {
            if (_scaleIndex > 0)
            {
                _scale = _scales[--_scaleIndex];
                RescaleMap();
            }
        }

        private void OnInfo()
        {
            _showInfo = !_showInfo;
            foreach (var info in _mapInfoObjects)
                info.gameObject.SetActive(_showInfo);
        }

        private void OnWorlMap()
        {

        }

        private void OnChangeVisibility()
        {
            if (!_hasVisibleMap)
                return;

            switch (_visibilityState)
            {
                case VisibilityState.Visible:
                    _visibilityState = VisibilityState.Transparent;
                    _map.color = new Color(_map.color.r, _map.color.g, _map.color.b, 0.5f);
                    break;
                case VisibilityState.Transparent:
                    _visibilityState = VisibilityState.Hidden;
                    gameObject.SetActive(false);
                    break;
                case VisibilityState.Hidden:
                    _visibilityState = VisibilityState.Visible;
                    _map.color = new Color(_map.color.r, _map.color.g, _map.color.b, 1f);
                    gameObject.SetActive(true);
                    break;
            }
        }

        public new bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            return base.IsRaycastLocationValid(sp, eventCamera) &&
                RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, sp, eventCamera);
        }

        public static MiniMapPanelController Instantiate(UIController uiController, Transform parent)
        {
            return Instantiate<MiniMapPanelController>(uiController, parent, "MiniMapPanel");
        }
    }
}
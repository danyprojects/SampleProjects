using RO.Media;
using System.Collections.Generic;
using UnityEngine;

namespace RO
{
    public class ObjectPoll : MonoBehaviour
    {
        [SerializeField] private bool _destroyOnLoad = false;

        private static GameObject _objectPollObj = null, _monsterSpritePrefab = null, _effectSpritePrefab = null, _effectQuadPrefab = null;
        private static GameObject _castCirclePrefab = null, _floatingTextAnimatorPrefab = null, _castCylinderPrefab = null, _fillBarPrefab = null;
        private static GameObject _castLockOnPrefab = null, _emptyGameObjectPrefab = null, _itemRendererPrefab = null;

        private static readonly Stack<GameObject> _emptyGameObjects = new Stack<GameObject>();
        private static readonly Stack<GameObject> _playerAnimatorControllers = new Stack<GameObject>();
        private static readonly Stack<GameObject> _equipmentAnimators = new Stack<GameObject>();
        private static readonly Stack<GameObject> _shieldAnimators = new Stack<GameObject>();
        private static readonly Stack<GameObject> _monsterAnimatorControllers = new Stack<GameObject>();
        private static readonly Stack<GameObject> _monsterSpritePrefabs = new Stack<GameObject>();
        private static readonly Stack<GameObject> _effectSpritePrefabs = new Stack<GameObject>();
        private static readonly Stack<GameObject> _effectQuadPrefabs = new Stack<GameObject>();
        private static readonly Stack<GameObject> _castCircles = new Stack<GameObject>();
        private static readonly Stack<GameObject> _castLockOns = new Stack<GameObject>();
        private static readonly Stack<GameObject> _floatingTexts = new Stack<GameObject>();
        private static readonly Stack<GameObject> _cylinders = new Stack<GameObject>();
        private static readonly Stack<GameObject> _fillBars = new Stack<GameObject>();
        private static readonly Stack<GameObject> _itemAnimators = new Stack<GameObject>();

        public static GameObject EmptyGameObjectsPoll
        {
            get
            {
                if (_emptyGameObjects.Count == 0)
                    return Instantiate(_emptyGameObjectPrefab);
                GameObject obj = _emptyGameObjects.Pop();
                obj.SetActive(true);
                return obj;
            }
            set
            {
                value.transform.SetParent(_objectPollObj.transform, false);
                value.SetActive(false);
                _emptyGameObjects.Push(value);
            }
        }
        public static GameObject PlayerAnimatorControllersPoll
        {
            get
            {
                //Init another player object if poll is empty
                if (_playerAnimatorControllers.Count == 0)
                    return new GameObject("Player").AddComponent<PlayerAnimatorController>().gameObject;
                GameObject obj = _playerAnimatorControllers.Pop();
                obj.SetActive(true);
                obj.GetComponent<PlayerAnimatorController>().enabled = true;
                return obj;
            }
            set
            {
                value.transform.SetParent(_objectPollObj.transform, false);
                value.GetComponent<PlayerAnimatorController>().enabled = false;
                value.SetActive(false);
                _playerAnimatorControllers.Push(value);
            }
        }
        public static GameObject EquipmentAnimatorsPoll
        {
            get
            {
                GameObject obj = null;
                if (_equipmentAnimators.Count != 0)
                {
                    obj = _equipmentAnimators.Pop();
                    obj.SetActive(true);
                }
                return obj;
            }
            set
            {
                value.transform.SetParent(_objectPollObj.transform, false);
                value.SetActive(false);
                _equipmentAnimators.Push(value);
            }
        }
        public static GameObject ShieldAnimatorsPoll
        {
            get
            {
                GameObject obj = null;
                if (_shieldAnimators.Count != 0)
                {
                    obj = _shieldAnimators.Pop();
                    obj.SetActive(true);
                }
                return obj;
            }
            set
            {
                value.SetActive(true); // In case the gameobject was not active when it was sent to the object poll
                value.transform.SetParent(_objectPollObj.transform, false);
                value.SetActive(false);
                _shieldAnimators.Push(value);
            }
        }
        public static GameObject MonsterAnimatorControllersPoll
        {
            get
            {
                //Init another player object if poll is empty
                if (_monsterAnimatorControllers.Count == 0)
                    return new GameObject("Monster").AddComponent<MonsterAnimatorController>().gameObject;
                GameObject obj = _monsterAnimatorControllers.Pop();
                obj.SetActive(true);
                obj.GetComponent<MonsterAnimatorController>().enabled = true;
                return obj;
            }
            set
            {
                value.transform.SetParent(_objectPollObj.transform, false);
                value.GetComponent<MonsterAnimatorController>().enabled = false;
                value.SetActive(false);
                _monsterAnimatorControllers.Push(value);
            }
        }
        public static GameObject MonsterRenderersPoll
        {
            get
            {
                //Init another sprite renderer object if poll is empty
                if (_monsterSpritePrefabs.Count == 0)
                    return Instantiate(_monsterSpritePrefab);
                GameObject obj = _monsterSpritePrefabs.Pop();
                obj.SetActive(true);
                obj.GetComponent<MeshRenderer>().enabled = true;
                return obj;
            }
            set
            {
                value.transform.SetParent(_objectPollObj.transform, false);
                value.GetComponent<MeshRenderer>().enabled = false;
                value.SetActive(false);
                _monsterSpritePrefabs.Push(value);
            }
        }
        public static GameObject EffectSpriteRenderersPoll
        {
            get
            {
                //Init another sprite renderer object if poll is empty
                if (_effectSpritePrefabs.Count == 0)
                    return Instantiate(_effectSpritePrefab);
                GameObject obj = _effectSpritePrefabs.Pop();
                obj.SetActive(true);
                obj.GetComponent<SpriteRenderer>().enabled = true;
                return obj;
            }
            set
            {
                value.transform.SetParent(_objectPollObj.transform, false);
                value.GetComponent<SpriteRenderer>().enabled = false;
                value.SetActive(false);
                _effectSpritePrefabs.Push(value);
            }
        }
        public static GameObject EffectSpriteQuadsPoll
        {
            get
            {
                //Init another sprite renderer object if poll is empty
                if (_effectQuadPrefabs.Count == 0)
                    return Instantiate(_effectQuadPrefab);
                GameObject obj = _effectQuadPrefabs.Pop();
                obj.SetActive(true);
                //obj.GetComponent<MeshRenderer>().enabled = true;  //Let the animator control renderer enabling
                return obj;
            }
            set
            {
                value.transform.SetParent(_objectPollObj.transform, false);
                //value.GetComponent<MeshRenderer>().enabled = false;
                value.SetActive(false);
                _effectQuadPrefabs.Push(value);
            }
        }
        public static GameObject CastCirclePoll
        {
            get
            {
                if (_castCircles.Count == 0)
                    return Instantiate(_castCirclePrefab);
                GameObject obj = _castCircles.Pop();
                obj.GetComponent<CastCircleAnimator>().enabled = true;
                return obj;
            }
            set
            {
                value.transform.SetParent(_objectPollObj.transform, false);
                value.GetComponent<CastCircleAnimator>().enabled = false;
                _castCircles.Push(value);
            }
        }
        public static GameObject CastLockOnPoll
        {
            get
            {
                if (_castLockOns.Count == 0)
                    return Instantiate(_castLockOnPrefab);
                GameObject obj = _castLockOns.Pop();
                obj.GetComponent<CastLockOnAnimator>().enabled = true;
                return obj;
            }
            set
            {
                value.transform.SetParent(_objectPollObj.transform, false);
                value.GetComponent<CastLockOnAnimator>().enabled = false;
                _castLockOns.Push(value);
            }
        }
        public static GameObject FloatingTextPoll
        {
            get
            {
                if (_floatingTexts.Count == 0)
                    return Instantiate(_floatingTextAnimatorPrefab);
                GameObject obj = _floatingTexts.Pop();
                obj.GetComponent<FloatingTextAnimator>().enabled = true;
                return obj;
            }
            set
            {
                value.transform.SetParent(_objectPollObj.transform, false);
                value.GetComponent<FloatingTextAnimator>().enabled = false;
                _floatingTexts.Push(value);
            }
        }
        public static GameObject CylinderPoll
        {
            get
            {
                if (_cylinders.Count == 0)
                    return Instantiate(_castCylinderPrefab);
                GameObject obj = _cylinders.Pop();
                obj.GetComponent<MeshRenderer>().enabled = true;
                return obj;
            }
            set
            {
                value.transform.SetParent(_objectPollObj.transform, false);
                value.GetComponent<MeshRenderer>().enabled = false;
                value.GetComponent<AudioSource>().Stop();
                _cylinders.Push(value);
            }
        }
        public static GameObject FillBarPool
        {
            get
            {
                if (_fillBars.Count == 0)
                    return Instantiate(_fillBarPrefab);
                GameObject obj = _fillBars.Pop();
                obj.GetComponent<MeshRenderer>().enabled = true;
                return obj;
            }
            set
            {
                value.transform.SetParent(_objectPollObj.transform, false);
                value.GetComponent<MeshRenderer>().enabled = false;
                _fillBars.Push(value);
            }
        }
        public static GameObject ItemAnimatorsPool
        {
            get
            {
                if (_itemAnimators.Count == 0)
                    return Instantiate(_itemRendererPrefab);
                GameObject obj = _itemAnimators.Pop();
                return obj;
            }
            set
            {
                value.transform.SetParent(_objectPollObj.transform, false);
                _itemAnimators.Push(value);
            }
        }


        private void Awake()
        {
            _objectPollObj = gameObject;
            if (!_destroyOnLoad)
                DontDestroyOnLoad(_objectPollObj);
            _monsterSpritePrefab = AssetBundleProvider.LoadMiscBundleAsset<GameObject>("MonsterRenderer");
            _effectSpritePrefab = AssetBundleProvider.LoadMiscBundleAsset<GameObject>("EffectRenderer");
            _itemRendererPrefab = AssetBundleProvider.LoadMiscBundleAsset<GameObject>("ItemRenderer");
            _effectQuadPrefab = AssetBundleProvider.LoadMiscBundleAsset<GameObject>("EffectQuad");
            _castCirclePrefab = AssetBundleProvider.LoadMiscBundleAsset<GameObject>("CastCircle");
            _floatingTextAnimatorPrefab = AssetBundleProvider.LoadMiscBundleAsset<GameObject>("FloatingTextAnimator");
            _castCylinderPrefab = AssetBundleProvider.LoadMiscBundleAsset<GameObject>("Cylinder");
            _castLockOnPrefab = AssetBundleProvider.LoadMiscBundleAsset<GameObject>("CastLockOn");
            _fillBarPrefab = AssetBundleProvider.LoadMiscBundleAsset<GameObject>("FillBar");
            _emptyGameObjectPrefab = AssetBundleProvider.LoadMiscBundleAsset<GameObject>("EmptyGameObject");            
        }
    }
}

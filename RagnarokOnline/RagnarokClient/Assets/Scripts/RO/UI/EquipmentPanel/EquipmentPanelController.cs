using RO.Common;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RO.UI
{
    public sealed class EquipmentPanelController : UIController.Panel
        , IPointerDownHandler
    {
#pragma warning disable 0649
        [Serializable]
        private struct Item
        {
            public Image _image;
            public Text _labelWhite;
            public Text _labelBlack;
        }

        [Serializable]
        private struct General
        {
            public Item _headTop;
            public Item _headMid;
            public Item _headLow;
            public Item _body;
            public Item _rightHand;
            public Item _leftHand;
            public Item _robe;
            public Item _shoes;
            public Item _leftAccessory;
            public Item _rightAccessory;
            public Item _arrow;
        }

        [Serializable]
        private struct Costume
        {
            public Item _headTop;
            public Item _headMid;
            public Item _headLow;
            public Item _body;
            public Item _floor;
        }

        [Serializable]
        private struct ButtonStat
        {
            public Text _base;
            public Text _bonus;
            public Text _cost;
            public UnityEngine.UI.Button _button;
        }

        [Serializable]
        private struct BonusStat
        {
            public Text _base;
            public Text _bonus;
        }

        [Serializable]
        private struct Status
        {
            public ButtonStat _str;
            public ButtonStat _agi;
            public ButtonStat _vit;
            public ButtonStat _int;
            public ButtonStat _dex;
            public ButtonStat _luck;

            public BonusStat _atk;
            public BonusStat _matk;
            public Text _hit;
            public Text _critical;
            public BonusStat _def;
            public BonusStat _mdef;
            public BonusStat _flee;
            public Text _aspd;
            public Text _statusPoint;
            public Text _guild;
        }
#pragma warning restore 0649

        [SerializeField]
        private General general = default;
        [SerializeField]
        private Costume costume = default;
        [SerializeField]
        private Status status = default;
        [SerializeField]
        private UnityEngine.UI.Toggle _showEquipToggle = default;
        [SerializeField]
        private UnityEngine.UI.Button _cartOffButton = default;
        [SerializeField]
        private UnityEngine.UI.Button _cartItemsButton = default;

        /*[SerializeField]
        private StaticSpriteAnimator _bodyAnimator = default;*/

        private RO.MapObjects.Character _character;

        private void Awake()
        {
            //IO.KeyBinder.RegisterAction(IO.KeyBinder.Shortcut.Equipment, 
            //  () => gameObject.SetActive(!gameObject.activeSelf));

            /* _closeButton.OnClick = onPressClose;
             _closeButton.gameObject.GetComponent<LabelArea>().OnEnter
                 = () => LabelController.ShowLabel(_closeButton.transform.position,
                 IO.KeyBinder.GetShortcutAsString(IO.KeyBinder.Shortcut.Equipment), default);*/

            _character = new RO.MapObjects.Character(0);
            _character.gameObject.transform.SetParent(transform, true);
            _character.gameObject.transform.localPosition = Vector3.zero;
            _character.gameObject.transform.localEulerAngles = Vector3.zero;

            //  setBody(Gender.Female, Jobs.Hunter);
        }

        void SetHeadTop(Sprite sprite, string name)
        {
            general._headTop._image.sprite = sprite;
            general._headTop._labelBlack.text = name;
            general._headTop._labelWhite.text = name;
        }

        void SetHeadMid(Sprite sprite, string name)
        {
            general._headMid._image.sprite = sprite;
            general._headMid._labelBlack.text = name;
            general._headMid._labelWhite.text = name;
        }

        void SetHeadLow(Sprite sprite, string name)
        {
            general._headLow._image.sprite = sprite;
            general._headLow._labelBlack.text = name;
            general._headLow._labelWhite.text = name;
        }

        void SetBody(Sprite sprite, string name)
        {
            general._body._image.sprite = sprite;
            general._body._labelBlack.text = name;
            general._body._labelWhite.text = name;
        }

        void SetLeftHand(Sprite sprite, string name)
        {
            general._leftHand._image.sprite = sprite;
            general._leftHand._labelBlack.text = name;
            general._leftHand._labelWhite.text = name;
        }

        void SetRightHand(Sprite sprite, string name)
        {
            general._rightHand._image.sprite = sprite;
            general._rightHand._labelBlack.text = name;
            general._rightHand._labelWhite.text = name;
        }

        void SetRobe(Sprite sprite, string name)
        {
            general._robe._image.sprite = sprite;
            general._robe._labelBlack.text = name;
            general._robe._labelWhite.text = name;
        }

        void SetShoes(Sprite sprite, string name)
        {
            general._shoes._image.sprite = sprite;
            general._shoes._labelBlack.text = name;
            general._shoes._labelWhite.text = name;
        }

        void SetLeftAccessory(Sprite sprite, string name)
        {
            general._leftAccessory._image.sprite = sprite;
            general._leftAccessory._labelBlack.text = name;
            general._leftAccessory._labelWhite.text = name;
        }

        void SetRightAccessory(Sprite sprite, string name)
        {
            general._rightAccessory._image.sprite = sprite;
            general._rightAccessory._labelBlack.text = name;
            general._rightAccessory._labelWhite.text = name;
        }

        void SetArrow(Sprite sprite, string name)
        {
            general._arrow._image.sprite = sprite;
            general._arrow._labelBlack.text = name;
            general._arrow._labelWhite.text = name;
        }



        void SetCostumeHeadTop(Sprite sprite, string name)
        {
            costume._headTop._image.sprite = sprite;
            costume._headTop._labelBlack.text = name;
            costume._headTop._labelWhite.text = name;
        }

        void SetCostumeHeadMid(Sprite sprite, string name)
        {
            costume._headMid._image.sprite = sprite;
            costume._headMid._labelBlack.text = name;
            costume._headMid._labelWhite.text = name;
        }

        void SetCostumeHeadLow(Sprite sprite, string name)
        {
            costume._headLow._image.sprite = sprite;
            costume._headLow._labelBlack.text = name;
            costume._headLow._labelWhite.text = name;
        }

        void SetCostumeBody(Sprite sprite, string name)
        {
            costume._body._image.sprite = sprite;
            costume._body._labelBlack.text = name;
            costume._body._labelWhite.text = name;
        }

        void SetCostumeFloor(Sprite sprite, string name)
        {
            costume._floor._image.sprite = sprite;
            costume._floor._labelBlack.text = name;
            costume._floor._labelWhite.text = name;
        }



        void SetBaseStr(int str)
        {
            status._str._base.text = str.ToString();
        }

        void SetBonusStr(int str)
        {
            status._str._bonus.text = str.ToString();
        }

        void SetStrCost(int str)
        {
            status._str._cost.text = str.ToString();
        }

        UnityEvent GetStrCostOnClick()
        {
            return status._str._button.onClick;
        }



        void SetBaseAgi(int agi)
        {
            status._agi._base.text = agi.ToString();
        }

        void SetBonusAgi(int agi)
        {
            status._agi._bonus.text = agi.ToString();
        }

        void SetAgiCost(int agi)
        {
            status._agi._cost.text = agi.ToString();
        }

        UnityEvent GetAgiCostOnClick()
        {
            return status._agi._button.onClick;
        }



        void SetBaseVit(int vit)
        {
            status._vit._base.text = vit.ToString();
        }

        void SetBonusVit(int vit)
        {
            status._vit._bonus.text = vit.ToString();
        }

        void SetVitCost(int vit)
        {
            status._vit._cost.text = vit.ToString();
        }

        UnityEvent GetVitCostOnClick()
        {
            return status._vit._button.onClick;
        }



        void SetBaseInt(int int_)
        {
            status._int._base.text = int_.ToString();
        }

        void SetBonusInt(int int_)
        {
            status._int._bonus.text = int_.ToString();
        }

        void SetIntCost(int int_)
        {
            status._int._cost.text = int_.ToString();
        }

        UnityEvent GetIntCostOnClick()
        {
            return status._int._button.onClick;
        }



        void SetBaseDex(int dex)
        {
            status._dex._base.text = dex.ToString();
        }

        void SetBonusDex(int dex)
        {
            status._dex._bonus.text = dex.ToString();
        }

        void SetDexCost(int dex)
        {
            status._dex._cost.text = dex.ToString();
        }

        UnityEvent GetDexCostOnClick()
        {
            return status._dex._button.onClick;
        }



        void SetBaseLuk(int luk)
        {
            status._luck._base.text = luk.ToString();
        }

        void SetBonusLuk(int luk)
        {
            status._luck._bonus.text = luk.ToString();
        }

        void SetLukCost(int luk)
        {
            status._luck._cost.text = luk.ToString();
        }

        UnityEvent GetLukCostOnClick()
        {
            return status._luck._button.onClick;
        }



        void SetBaseAtk(int atk)
        {
            status._atk._base.text = atk.ToString();
        }

        void SetBonusAtk(int atk)
        {
            status._atk._bonus.text = atk.ToString();
        }




        void SetBaseMatk(int matk)
        {
            status._matk._base.text = matk.ToString();
        }

        void SetBonusMatk(int matk)
        {
            status._matk._bonus.text = matk.ToString();
        }




        void SetBaseDef(int def)
        {
            status._def._base.text = def.ToString();
        }

        void SetBonusDef(int def)
        {
            status._def._bonus.text = def.ToString();
        }



        void SetBaseMdef(int mdef)
        {
            status._mdef._base.text = mdef.ToString();
        }

        void SetBonusMdef(int mdef)
        {
            status._mdef._bonus.text = mdef.ToString();
        }




        void SetBaseFlee(int flee)
        {
            status._flee._base.text = flee.ToString();
        }

        void SetBonusFlee(int flee)
        {
            status._flee._bonus.text = flee.ToString();
        }



        void SetHit(int hit)
        {
            status._hit.text = hit.ToString();
        }

        void SetCritical(int critical)
        {
            status._critical.text = critical.ToString();
        }

        void SetAspd(int aspd)
        {
            status._aspd.text = aspd.ToString();
        }

        void SetStatusPoint(int statusPoint)
        {
            status._statusPoint.text = statusPoint.ToString();
        }

        void SetGuild(string guild)
        {
            status._guild.text = guild;
        }



        void enableCart()
        {
            _cartOffButton.gameObject.SetActive(true);
            _cartItemsButton.gameObject.SetActive(true);
        }

        void disableCart()
        {
            _cartOffButton.gameObject.SetActive(false);
            _cartItemsButton.gameObject.SetActive(true);
        }

        UnityEvent GetCartOffOnClick()
        {
            return _cartOffButton.onClick;
        }


        UnityEvent<bool> GetShowEquipOnValueChanged()
        {
            return _showEquipToggle.onValueChanged;
        }


        void setBody(Gender gender, Jobs job)
        {

        }

        public void OnPointerDown(PointerEventData eventData)
        {
            BringToFront();
        }
    }
}

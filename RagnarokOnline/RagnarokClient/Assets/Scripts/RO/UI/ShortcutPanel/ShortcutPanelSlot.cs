using RO.IO;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public partial class ShortcutPanelController : UIController.Panel
    {
        public class Slot : MonoBehaviour
            , IPointerDownHandler, IPointerUpHandler
        {
#pragma warning disable 0649
            [Serializable]
            public struct Keys
            {
                public KeyBinder.CommandKeys commandKeys;
                public KeyCode keyCode;
            }

            [Serializable]
            public struct Data
            {
                public string description;
                public Keys keys;
            }
#pragma warning restore 0649

            [SerializeField]
            private Image background = default;
            [SerializeField]
            private Toggle quickCastToggle = default;
            [SerializeField]
            private Toggle holdToggle = default;
            [SerializeField]
            private Text descriptionText = default;
            [SerializeField]
            private Text keysText = default;
            [SerializeField]
            private Data skillBarData = default;
            [SerializeField]
            private Data[] interfaceData = new Data[2];
            [SerializeField]
            private Data macroData = default;


            private ShortcutPanelController _panel;
            private bool isDirty = false;
            private int slotIndex;

            private bool cachedQuickCast = false;
            private bool cachedHold = false;
            private Keys _cachedSkillBarKeys;
            private Keys[] _cachedInterfaceKeys = new Keys[2];
            private Keys _cachedMacroKeys;

            private string _cachedSkillBarDescription;
            private string[] _cachedInterfaceDescription = new string[2];
            private string _cachedMacroDescription;

            private void Awake()
            {
                _panel = GetComponentInParent<ShortcutPanelController>();
                quickCastToggle.OnValueChanged = v => cachedQuickCast = v;
                holdToggle.OnValueChanged = v => cachedHold = v;

                for (int i = 0; i < _panel._slots.Length; i++)
                {
                    if (ReferenceEquals(this, _panel._slots[i]))
                    {
                        slotIndex = i;
                        break;
                    }
                }

                LoadKeysFromKeyBinder();
                Refresh();
            }

            public void SetTogglesEnabled(bool enabled)
            {
                holdToggle.enabled = enabled;
                quickCastToggle.enabled = enabled;
            }

            public void OnPointerDown(PointerEventData eventData)
            {
                if (_panel._selectedSlot == this)
                    return;

                var labelPos = _panel._labelRectTransform.anchoredPosition;
                _panel._labelRectTransform.anchoredPosition =
                    new Vector2(labelPos.x, background.rectTransform.anchoredPosition.y + Label.DefaultHeight);

                if (_panel._selectedSlot != null)
                    _panel._selectedSlot.background.sprite = _panel._rowDefaultSprite;
                else
                    _panel.label.gameObject.SetActive(true);

                background.sprite = _panel.rowHighligthedSprite;
                _panel._selectedSlot = this;
            }

            public void OnPointerUp(PointerEventData eventData)
            {
                // just prevent bubble up
            }

            public void OnTabChanged()
            {
                gameObject.SetActive(false);
                holdToggle.gameObject.SetActive(_panel.activeTab == Tab.Skillbar);
                quickCastToggle.gameObject.SetActive(_panel.activeTab == Tab.Skillbar);

                var y = background.rectTransform.sizeDelta.y;
                var x = _panel.activeTab == Tab.Skillbar ?
                    _panel._skillBarRowWidth : _panel._defaultRowWidth;

                background.rectTransform.sizeDelta = new Vector2(x, y);

                gameObject.SetActive(Refresh());
            }

            public void OnReset()
            {
                isDirty = true;

                quickCastToggle.SetValue(cachedQuickCast = false);
                holdToggle.SetValue(cachedHold = false);

                var pageNumber = _panel.interfaceTabNumber;

                if (!_cachedSkillBarKeys.Equals(skillBarData.keys))
                    _cachedSkillBarDescription = stringFromKeys(skillBarData.keys);

                if (!_cachedInterfaceKeys[pageNumber].Equals(interfaceData[pageNumber].keys))
                    _cachedInterfaceDescription[pageNumber] = stringFromKeys(interfaceData[pageNumber].keys);

                if (!_cachedMacroKeys.Equals(macroData.keys))
                    _cachedMacroDescription = stringFromKeys(macroData.keys);

                _cachedSkillBarKeys = skillBarData.keys;
                _cachedMacroKeys = macroData.keys;

                for (int i = 0; i < interfaceData.Length; i++)
                    _cachedInterfaceKeys[i] = interfaceData[i].keys;

                Refresh();
            }

            public void OnCancel()
            {
                if (isDirty)
                {
                    isDirty = false;
                    LoadKeysFromKeyBinder();
                    Refresh();
                }
            }

            public void OnApply()
            {
                if (isDirty)
                {
                    isDirty = false;
                    UpdateKeyBinder();
                    Refresh();
                }
            }

            public void DeselectSlot()
            {
                Debug.Assert(_panel._selectedSlot == this);

                _panel.label.gameObject.SetActive(false);
                background.sprite = _panel._rowDefaultSprite;
                _panel._selectedSlot = null;
            }

            private void SaveKeysToCache(Keys keys, Tab tab, int pageNumber)
            {
                isDirty = true;

                switch (tab)
                {
                    case Tab.Skillbar:
                        _cachedSkillBarKeys = keys;
                        keysText.text = _cachedSkillBarDescription = stringFromKeys(keys);
                        break;
                    case Tab.Interface:
                        _cachedInterfaceKeys[pageNumber] = keys;
                        keysText.text = _cachedInterfaceDescription[pageNumber] = stringFromKeys(keys);
                        break;
                    case Tab.Macro:
                        _cachedMacroKeys = keys;
                        keysText.text = _cachedMacroDescription = stringFromKeys(keys);
                        break;
                }
            }

            private void ShowColisionDialog(Keys keys, string colisionDescription, Action unregisterOldKeys)
            {
                _panel.isCollisionDialogOpen = true;
                _panel.MessageDialogController.ShowDialog(
                    $"Duplicated with ['{colisionDescription}']. Do you still want to change?",
                    () =>
                    {
                        unregisterOldKeys();
                        SaveKeysToCache(keys, _panel.activeTab, _panel.interfaceTabNumber);
                        _panel.isCollisionDialogOpen = false;
                    },
                    () => { _panel.isCollisionDialogOpen = false; },
                    CanvasFilter.ModalMsgDialog);
            }

            public void ProcessInputKeys(Keys keys)
            {
                // Look for colisions first
                foreach (var slot in _panel._slots)
                {
                    if (slot._cachedSkillBarKeys.Equals(keys))
                    {
                        ShowColisionDialog(keys, slot.skillBarData.description,
                            () => slot.SaveKeysToCache(default, Tab.Skillbar, default));
                        return;
                    }

                    if (slot._cachedMacroKeys.Equals(keys))
                    {
                        ShowColisionDialog(keys, slot.macroData.description,
                            () => slot.SaveKeysToCache(default, Tab.Macro, default));
                        return;
                    }

                    for (int i = 0; i < _cachedInterfaceKeys.Length; i++)
                    {
                        if (slot._cachedInterfaceKeys[i].Equals(keys))
                        {
                            ShowColisionDialog(keys, slot.interfaceData[i].description,
                                () => slot.SaveKeysToCache(default, Tab.Interface, i));
                            return;
                        }
                    }
                }

                SaveKeysToCache(keys, _panel.activeTab, _panel.interfaceTabNumber);
            }

            private void UpdateKeyBinder()
            {
                KeyBinder.KeysPanel.RegisterKey((KeyBinder.Hotkey)slotIndex, _cachedSkillBarKeys.keyCode, cachedHold, _cachedSkillBarKeys.commandKeys, cachedQuickCast);

                if (!string.IsNullOrEmpty(macroData.description))
                    KeyBinder.KeysPanel.RegisterKey((KeyBinder.Macro)slotIndex, _cachedMacroKeys.keyCode, _cachedMacroKeys.commandKeys);

                for (int i = 0; i < _cachedInterfaceKeys.Length; i++)
                    if (!string.IsNullOrEmpty(interfaceData[i].description))
                        KeyBinder.KeysPanel.RegisterKey((KeyBinder.Shortcut)(slotIndex + _panel._slots.Length * i), _cachedInterfaceKeys[i].keyCode, _cachedInterfaceKeys[i].commandKeys);
            }

            private bool Refresh()
            {
                var pageNumber = _panel.interfaceTabNumber;

                switch (_panel.activeTab)
                {
                    case Tab.Skillbar:
                        descriptionText.text = skillBarData.description;
                        keysText.text = _cachedSkillBarDescription;
                        break;
                    case Tab.Interface:
                        if (string.IsNullOrEmpty(interfaceData[pageNumber].description))
                            return false;
                        descriptionText.text = interfaceData[pageNumber].description;
                        keysText.text = _cachedInterfaceDescription[pageNumber];
                        break;
                    case Tab.Macro:
                        if (string.IsNullOrEmpty(macroData.description))
                            return false;
                        descriptionText.text = macroData.description;
                        keysText.text = _cachedMacroDescription;
                        break;
                }

                return true;
            }

            private void LoadKeysFromKeyBinder()
            {
                var data = KeyBinder.KeysPanel.GetKeyData((KeyBinder.Hotkey)slotIndex);

                quickCastToggle.SetValue(cachedQuickCast = data.speedcast);
                holdToggle.SetValue(cachedHold = data.hold);

                _cachedSkillBarKeys.keyCode = data.key;
                _cachedSkillBarKeys.commandKeys = data.commandKeys;

                _cachedSkillBarDescription = KeyBinder.GetHotkeyAsString((KeyBinder.Hotkey)slotIndex);

                for (int i = 0; i < _cachedInterfaceKeys.Length; i++)
                {
                    if (string.IsNullOrEmpty(interfaceData[i].description))
                        continue;

                    data = KeyBinder.KeysPanel.GetKeyData((KeyBinder.Shortcut)(slotIndex + _panel._slots.Length * i));

                    _cachedInterfaceKeys[i].keyCode = data.key;
                    _cachedInterfaceKeys[i].commandKeys = data.commandKeys;

                    _cachedInterfaceDescription[i] = KeyBinder.GetShortcutAsString((KeyBinder.Shortcut)(slotIndex + _panel._slots.Length * i));
                }

                if (string.IsNullOrEmpty(macroData.description))
                    return;

                data = KeyBinder.KeysPanel.GetKeyData((KeyBinder.Macro)slotIndex);

                _cachedMacroKeys.keyCode = data.key;
                _cachedMacroKeys.commandKeys = data.commandKeys;

                _cachedMacroDescription = KeyBinder.GetMacroAsString((KeyBinder.Macro)slotIndex);
            }

            private string stringFromKeys(Keys keys)
            {
                return KeyBinder.KeysPanel.ConvertKeysToString(keys.keyCode, keys.commandKeys);
            }
        }
    }

    public class ShortcutPanelSlot : ShortcutPanelController.Slot { }
}
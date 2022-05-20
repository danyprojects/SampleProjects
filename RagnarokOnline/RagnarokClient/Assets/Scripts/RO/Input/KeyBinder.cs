using System;
using System.Collections.Generic;
using UnityEngine;

namespace RO.IO
{
    public sealed class KeyBinder
    {
        public enum Hotkey : int
        {
            _1_1 = 0,
            _1_2,
            _1_3,
            _1_4,
            _1_5,
            _1_6,
            _1_7,
            _1_8,
            _1_9,
            _2_1,
            _2_2,
            _2_3,
            _2_4,
            _2_5,
            _2_6,
            _2_7,
            _2_8,
            _2_9,
            _3_1,
            _3_2,
            _3_3,
            _3_4,
            _3_5,
            _3_6,
            _3_7,
            _3_8,
            _3_9,
            _4_1,
            _4_2,
            _4_3,
            _4_4,
            _4_5,
            _4_6,
            _4_7,
            _4_8,
            _4_9,

            Last
        }

        // When adding more, ShortcutsPanel needs to be updated in Editor
        public enum Macro : int
        {
            _1 = 0,
            _2,
            _3,
            _4,
            _5,
            _6,
            _7,
            _8,
            _9,
            _10,
            _11,
            _12,
            _13,
            _14,
            _15,
            _16,
            _17,
            _18,
            _19,
            _20,

            Last
        }

        // When adding more, ShortcutsPanel needs to be updated in Editor
        public enum Shortcut : int
        {
            BasicInfo = 0,
            Equipment,
            Skills,
            FriendList,
            PartyList,
            Chatroom,
            ViewCart,
            Inventory,
            Guild,
            SitStand,
            MiniMap,
            ShortcutsList,
            CommandsList,
            UIOnOff,
            SkillBar,
            HpSpBarOnOff,
            Screenshot,
            PetInfo,
            HomunculusInfo,
            MercenaryInfo,
            HomunCommand,
            MercCommand,
            GroundCursorOnOff,
            RecordingOnOff,
            StartPauseRecording,
            QuestLog,
            EmoticonList,
            ChatOnOff,
            Achievements,
            Mail,
            WorldMap,

            Last
        }

        public enum CommandKeys : int
        {
            Alt = 1 << 0,  //1
            Ctrl = 1 << 1, //2
            Shift = 1 << 2, //4
            Alt_Ctrl = Alt | Ctrl, //3
            Alt_Shift = Alt | Shift, //5
            Ctrl_Shift = Ctrl | Shift, //6
            Alt_Ctrl_Shift = Alt | Ctrl | Shift, //7

            None = 0
        }

        private struct KeyBindHotkey
        {
            public KeyCode key;
            public CommandKeys commandKeys;
            public Action<bool> action;
            public float lastSend; // for use in hold
            public bool hold;
            public bool quickCast;
        }

        private struct KeyBind
        {
            public KeyCode key;
            public CommandKeys commandKeys;
            public Action action;
        }

        private struct KeyCombination
        {
            private readonly int _hashCode;

            public KeyCombination(KeyCode key, CommandKeys commandKeys)
            {
                int value = ((int)commandKeys << 16) | (int)key;
                _hashCode = value.GetHashCode();
            }

            //So that hash key will be a combination of key + command key
            public override int GetHashCode()
            {
                return _hashCode;
            }
        }

        //holds the keys used during input
        private static readonly HashSet<KeyCombination> _inputFieldKeys;

        //these are static size and holds the data of bindings and actions per keys
        private static KeyBindHotkey[] _hotkeys = new KeyBindHotkey[(int)Hotkey.Last];
        private static KeyBind[] _macros = new KeyBind[(int)Macro.Last];
        private static KeyBind[] _shortcuts = new KeyBind[(int)Shortcut.Last];

        // These hold ONLY the indexes of the arrays on top that have BOTH action and key set
        private static List<int> _boundOnDownHotkeys = new List<int>((int)Hotkey.Last);
        private static List<int> _boundOnHoldHotkeys = new List<int>((int)Hotkey.Last);
        private static List<int> _boundMacros = new List<int>((int)Macro.Last);
        private static List<int> _boundShortcuts = new List<int>((int)Shortcut.Last);
        private static List<int> _boundOnDownInputHotkeys = new List<int>((int)Hotkey.Last);
        private static List<int> _boundOnHoldInputHotkeys = new List<int>((int)Hotkey.Last);
        private static List<int> _boundInputMacros = new List<int>((int)Macro.Last);
        private static List<int> _boundInputShortcuts = new List<int>((int)Shortcut.Last);

        // These are updated every time there are new keybindings
        private static string[] _hotkeysAsString = new string[(int)Hotkey.Last];
        private static string[] _macrosAsString = new string[(int)Macro.Last];
        private static string[] _shortcutsAsString = new string[(int)Shortcut.Last];

        // For cases were direct enum name is undesired
        private static readonly Dictionary<KeyCode, string> _keyCodesAsString;
        private static readonly string[] _commandKeysAsString = new string[]
        {
            null,
            "Alt + ",
            "Ctrl + ",
            "Ctrl + Alt + ",
            "Shift + ",
            "Shift + Alt + ",
            "Ctrl + Shift + ",
            "Ctrl + Shift + Alt + ",
        };

        public static bool IsBlockingAllKeys = false;
        public static bool IsBlockingInputKeys = false;

        static KeyBinder()
        {
            _inputFieldKeys = new HashSet<KeyCombination>()
            {
                //Do we need to add other command key combinations for these 2?
                new KeyCombination(KeyCode.CapsLock, CommandKeys.None), new KeyCombination(KeyCode.Numlock, CommandKeys.None),

                //Special shortcuts with letters + Ctrl  
                new KeyCombination(KeyCode.C, CommandKeys.Ctrl), new KeyCombination(KeyCode.V, CommandKeys.Ctrl),
                new KeyCombination(KeyCode.X, CommandKeys.Ctrl), new KeyCombination(KeyCode.A, CommandKeys.Ctrl),

                //misc keys
                //Block shift + space since it's likely user is pressing it while typing normally
                new KeyCombination(KeyCode.Space, CommandKeys.None), new KeyCombination(KeyCode.Space, CommandKeys.Shift),
                new KeyCombination(KeyCode.Backspace, CommandKeys.None), new KeyCombination(KeyCode.Delete, CommandKeys.None),
                new KeyCombination(KeyCode.Home, CommandKeys.None), new KeyCombination(KeyCode.End, CommandKeys.None),
            };

            //Add all letter keys with shift and no shift for caps and non caps
            //Add all misc keys that only trigger without command and with shift
            for (KeyCode key = KeyCode.A; key <= KeyCode.Tilde; key++)
            {
                _inputFieldKeys.Add(new KeyCombination(key, CommandKeys.None));
                _inputFieldKeys.Add(new KeyCombination(key, CommandKeys.Shift));
            }

            //Add all alpha keys (top row of keyboard) with shift and alt_ctrl
            //Add all sign keys with shift and alt_ctrl
            for (KeyCode key = KeyCode.Alpha0; key <= KeyCode.BackQuote; key++)
            {
                _inputFieldKeys.Add(new KeyCombination(key, CommandKeys.None));
                _inputFieldKeys.Add(new KeyCombination(key, CommandKeys.Shift));
                _inputFieldKeys.Add(new KeyCombination(key, CommandKeys.Alt_Ctrl));
            }

            //Add all numpad keys without combinations
            for (KeyCode key = KeyCode.Keypad0; key <= KeyCode.KeypadEquals; key++)
                _inputFieldKeys.Add(new KeyCombination(key, CommandKeys.None));

            // Add more as needed
            _keyCodesAsString = new Dictionary<KeyCode, string>
            {
                // Alphanumeric number keys
                { KeyCode.Alpha0, "0"}, { KeyCode.Alpha1, "1"}, { KeyCode.Alpha2, "2"}, { KeyCode.Alpha3, "3"},
                { KeyCode.Alpha4, "4"}, { KeyCode.Alpha5, "5"}, { KeyCode.Alpha6, "6"}, { KeyCode.Alpha7, "7"},
                { KeyCode.Alpha8, "8"}, { KeyCode.Alpha9, "9"},

                // Numpad keys
                { KeyCode.Keypad0, "Num_0" }, { KeyCode.Keypad1, "Num_1" }, { KeyCode.Keypad2, "Num_2" }, { KeyCode.Keypad3, "Num_3" },
                { KeyCode.Keypad4, "Num_4" }, { KeyCode.Keypad5, "Num_5" }, { KeyCode.Keypad6, "Num_6" }, { KeyCode.Keypad7, "Num_7" },
                { KeyCode.Keypad8, "Num_8" }, { KeyCode.Keypad9, "Num_9" },
                { KeyCode.KeypadPeriod, "Decimal" }, { KeyCode.KeypadDivide, "Divide" }, { KeyCode.KeypadMultiply, "Multiply" },
                { KeyCode.KeypadMinus, "Minus" }, { KeyCode.KeypadPlus, "Plus" }, { KeyCode.KeypadEquals, "Equals" }, { KeyCode.Numlock, "Num" }, 
   
                //Alphanumeric operator keys
                { KeyCode.Exclaim, "!" }, { KeyCode.DoubleQuote, "\"" }, { KeyCode.Hash, "#" }, { KeyCode.Dollar, "$" },
                { KeyCode.Percent, "%" }, { KeyCode.Ampersand, "&" }, { KeyCode.Quote, "'" }, { KeyCode.LeftParen, "(" },
                { KeyCode.RightParen, ")" }, { KeyCode.Asterisk, "*" }, { KeyCode.Plus, "+" }, { KeyCode.Comma, "," },
                { KeyCode.Minus, "-" }, { KeyCode.Period, "." }, { KeyCode.Slash, "/" }, { KeyCode.Backslash, "\\" },
                { KeyCode.Colon, ":" }, { KeyCode.Semicolon, ";" }, { KeyCode.Less, "<" }, { KeyCode.Greater, ">" },
                { KeyCode.Equals, "=" }, { KeyCode.Caret, "^" }, { KeyCode.Question, "?" }, { KeyCode.At, "@" },
                { KeyCode.LeftBracket, "[" }, { KeyCode.RightBracket, "]" }, { KeyCode.Underscore, "_" }, { KeyCode.BackQuote, "`" },
                { KeyCode.LeftCurlyBracket, "{" }, { KeyCode.RightCurlyBracket, "}" }, { KeyCode.Pipe, "|" }, { KeyCode.Tilde, "~" },

                //Misc keys                
                { KeyCode.Backspace, "Back" }, { KeyCode.PageDown, "Prior" }, { KeyCode.PageUp, "Next" }, { KeyCode.AltGr, "AltGr" },
                { KeyCode.CapsLock, "Caps" }, { KeyCode.ScrollLock, "Scroll" },

                { KeyCode.None, null}
            };

            //TODO: Load config from player prefs

            RegisterKey(Hotkey._1_1, KeyCode.Alpha1, false, CommandKeys.None, false);
            RegisterKey(Hotkey._1_2, KeyCode.Alpha2, false, CommandKeys.None, false);
            RegisterKey(Hotkey._1_3, KeyCode.Alpha3, false, CommandKeys.None, false);
            RegisterKey(Hotkey._1_4, KeyCode.Alpha4, false, CommandKeys.None, false);
            RegisterKey(Hotkey._1_5, KeyCode.Alpha5, false, CommandKeys.None, false);
            RegisterKey(Hotkey._1_6, KeyCode.Alpha6, false, CommandKeys.None, false);
            RegisterKey(Hotkey._1_7, KeyCode.Alpha7, false, CommandKeys.None, false);
            RegisterKey(Hotkey._1_8, KeyCode.Alpha8, false, CommandKeys.None, false);
            RegisterKey(Hotkey._1_9, KeyCode.Alpha9, false, CommandKeys.None, false);

            RegisterKey(Hotkey._2_1, KeyCode.Q, false, CommandKeys.None, false);
            RegisterKey(Hotkey._2_2, KeyCode.W, false, CommandKeys.None, false);
            RegisterKey(Hotkey._2_3, KeyCode.E, false, CommandKeys.None, false);
            RegisterKey(Hotkey._2_4, KeyCode.R, false, CommandKeys.None, false);
            RegisterKey(Hotkey._2_5, KeyCode.T, false, CommandKeys.None, false);
            RegisterKey(Hotkey._2_6, KeyCode.Y, false, CommandKeys.None, false);
            RegisterKey(Hotkey._2_7, KeyCode.U, false, CommandKeys.None, false);
            RegisterKey(Hotkey._2_8, KeyCode.I, false, CommandKeys.None, false);
            RegisterKey(Hotkey._2_9, KeyCode.O, false, CommandKeys.None, false);

            RegisterKey(Hotkey._3_1, KeyCode.A, false, CommandKeys.None, false);
            RegisterKey(Hotkey._3_2, KeyCode.S, false, CommandKeys.None, false);
            RegisterKey(Hotkey._3_3, KeyCode.D, false, CommandKeys.None, false);
            RegisterKey(Hotkey._3_4, KeyCode.F, false, CommandKeys.None, false);
            RegisterKey(Hotkey._3_5, KeyCode.G, false, CommandKeys.None, false);
            RegisterKey(Hotkey._3_6, KeyCode.H, false, CommandKeys.None, false);
            RegisterKey(Hotkey._3_7, KeyCode.J, false, CommandKeys.None, false);
            RegisterKey(Hotkey._3_8, KeyCode.K, false, CommandKeys.None, false);
            RegisterKey(Hotkey._3_9, KeyCode.L, false, CommandKeys.None, false);

            RegisterKey(Hotkey._4_1, KeyCode.Z, false, CommandKeys.None, false);
            RegisterKey(Hotkey._4_2, KeyCode.X, false, CommandKeys.None, false);
            RegisterKey(Hotkey._4_3, KeyCode.C, false, CommandKeys.None, false);
            RegisterKey(Hotkey._4_4, KeyCode.V, false, CommandKeys.None, false);
            RegisterKey(Hotkey._4_5, KeyCode.B, false, CommandKeys.None, false);
            RegisterKey(Hotkey._4_6, KeyCode.N, false, CommandKeys.None, false);
            RegisterKey(Hotkey._4_7, KeyCode.M, false, CommandKeys.None, false);
            RegisterKey(Hotkey._4_8, KeyCode.Comma, false, CommandKeys.None, false);
            RegisterKey(Hotkey._4_9, KeyCode.Period, false, CommandKeys.None, false);

            RegisterKey(Shortcut.SkillBar, KeyCode.F12, CommandKeys.None);

            RegisterKey(Macro._1, KeyCode.F1, CommandKeys.None);
            RegisterKey(Macro._2, KeyCode.F2, CommandKeys.None);
            RegisterKey(Macro._3, KeyCode.F3, CommandKeys.None);
            RegisterKey(Macro._4, KeyCode.F4, CommandKeys.None);
            RegisterKey(Macro._5, KeyCode.F5, CommandKeys.None);
            RegisterKey(Macro._6, KeyCode.F6, CommandKeys.None);
            RegisterKey(Macro._7, KeyCode.F7, CommandKeys.None);
            RegisterKey(Macro._8, KeyCode.F8, CommandKeys.None);
            RegisterKey(Macro._9, KeyCode.F9, CommandKeys.None);
            RegisterKey(Macro._10, KeyCode.F10, CommandKeys.None);
        }

        public sealed class KeysPanel
        {
            public struct KeyData
            {
                public KeyCode key;
                public CommandKeys commandKeys;
                public bool hold;
                public bool speedcast;
            }

            public static void RegisterKey(Hotkey hotkey, KeyCode key, bool hold, CommandKeys commandKeys, bool speedcast)
            {
                KeyBinder.RegisterKey(hotkey, key, hold, commandKeys, speedcast);
            }

            public static void RegisterKey(Macro macro, KeyCode key, CommandKeys commandKeys)
            {
                KeyBinder.RegisterKey(macro, key, commandKeys);
            }

            public static void RegisterKey(Shortcut shortcut, KeyCode key, CommandKeys commandKeys)
            {
                KeyBinder.RegisterKey(shortcut, key, commandKeys);
            }

            public static string ConvertKeysToString(KeyCode key, CommandKeys commandKeys)
            {
                return KeyBinder.ConvertKeysToString(key, commandKeys);
            }

            public static KeyData GetKeyData(Hotkey hotkey)
            {
                return new KeyData
                {
                    key = _hotkeys[(int)hotkey].key,
                    commandKeys = _hotkeys[(int)hotkey].commandKeys,
                    hold = _hotkeys[(int)hotkey].hold,
                    speedcast = _hotkeys[(int)hotkey].quickCast
                };
            }

            public static KeyData GetKeyData(Macro macro)
            {
                return new KeyData
                {
                    key = _macros[(int)macro].key,
                    commandKeys = _macros[(int)macro].commandKeys,
                };
            }

            public static KeyData GetKeyData(Shortcut shortcut)
            {
                return new KeyData
                {
                    key = _shortcuts[(int)shortcut].key,
                    commandKeys = _shortcuts[(int)shortcut].commandKeys,
                };
            }
        }

        public static string GetHotkeyAsString(Hotkey hotkey)
        {
            return _hotkeysAsString[(int)hotkey];
        }

        public static string GetMacroAsString(Macro macro)
        {
            return _macrosAsString[(int)macro];
        }

        public static string GetShortcutAsString(Shortcut shortcut)
        {
            return _shortcutsAsString[(int)shortcut];
        }

        // Check all action registered keys and call their actions
        public void ProcessBoundKeys(CommandKeys commandKeys)
        {
            if (IsBlockingAllKeys)
                return;

            ProcessHotkeysOnDown(commandKeys, _boundOnDownHotkeys);
            ProcessHotkeysOnHold(commandKeys, _boundOnHoldHotkeys);
            ProcessMacros(commandKeys, _boundMacros);
            ProcessShortcuts(commandKeys, _boundShortcuts);

            if (IsBlockingInputKeys)
                return;

            ProcessHotkeysOnDown(commandKeys, _boundOnDownInputHotkeys);
            ProcessHotkeysOnHold(commandKeys, _boundOnHoldInputHotkeys);
            ProcessMacros(commandKeys, _boundInputMacros);
            ProcessShortcuts(commandKeys, _boundInputShortcuts);
        }

        public static void RegisterAction(Hotkey hotkey, Action<bool> action)
        {
            _hotkeys[(int)hotkey].action = action;
            _hotkeys[(int)hotkey].lastSend = Common.Globals.Time;

            AddOrRemoveKeyFromBoundHotkeys((int)hotkey, _hotkeys[(int)hotkey].hold);
        }

        public static void RegisterAction(Macro macro, Action action)
        {
            _macros[(int)macro].action = action;

            AddOrRemoveKeyFromBound(ref _macros, ref _boundMacros, (int)macro);
        }

        public static void RegisterAction(Shortcut shortcut, Action action)
        {
            _shortcuts[(int)shortcut].action = action;

            AddOrRemoveKeyFromBound(ref _shortcuts, ref _boundShortcuts, (int)shortcut);
        }

        private static void RegisterKey(Hotkey hotkey, KeyCode key, bool hold, CommandKeys commandKeys, bool speedcast)
        {
            _hotkeys[(int)hotkey].key = key;
            _hotkeys[(int)hotkey].commandKeys = commandKeys;
            _hotkeys[(int)hotkey].hold = hold;
            _hotkeys[(int)hotkey].quickCast = speedcast;

            _hotkeysAsString[(int)hotkey] = ConvertKeysToString(key, commandKeys);

            AddOrRemoveKeyFromBoundHotkeys((int)hotkey, hold);
        }

        private static void RegisterKey(Macro macro, KeyCode key, CommandKeys commandKeys)
        {
            _macros[(int)macro].key = key;
            _macros[(int)macro].commandKeys = commandKeys;

            _macrosAsString[(int)macro] = ConvertKeysToString(key, commandKeys);

            if (_inputFieldKeys.Contains(new KeyCombination(key, commandKeys)))
                AddOrRemoveKeyFromBound(ref _macros, ref _boundInputMacros, (int)macro);
            else
                AddOrRemoveKeyFromBound(ref _macros, ref _boundMacros, (int)macro);
        }

        private static void RegisterKey(Shortcut shortcut, KeyCode key, CommandKeys commandKeys)
        {
            _shortcuts[(int)shortcut].key = key;
            _shortcuts[(int)shortcut].commandKeys = commandKeys;

            _shortcutsAsString[(int)shortcut] = ConvertKeysToString(key, commandKeys);

            if (_inputFieldKeys.Contains(new KeyCombination(key, commandKeys)))
                AddOrRemoveKeyFromBound(ref _shortcuts, ref _boundInputShortcuts, (int)shortcut);
            else
                AddOrRemoveKeyFromBound(ref _shortcuts, ref _boundShortcuts, (int)shortcut);
        }

        private static void AddOrRemoveKeyFromBoundHotkeys(int key, bool hold)
        {
            bool isInputKey = _inputFieldKeys.Contains(new KeyCombination(_hotkeys[key].key, _hotkeys[key].commandKeys));

            //Pick the correct lists depending on hold and if it's an input key
            List<int> _boundHotkeysToAdd = isInputKey ? (hold ? _boundOnHoldInputHotkeys : _boundOnDownInputHotkeys)
                                                      : (hold ? _boundOnHoldHotkeys : _boundOnDownHotkeys);
            List<int> _boundHotkeysToRemove = isInputKey ? (hold ? _boundOnDownInputHotkeys : _boundOnHoldInputHotkeys)
                                                         : (hold ? _boundOnDownHotkeys : _boundOnHoldHotkeys);

            //Check if it's contained in the other array in case we're switching hold modes
            if (_boundHotkeysToRemove.Contains(key))
                _boundHotkeysToRemove.Remove(key);

            // if action or key is not set, key should not be in the array
            if (_hotkeys[key].action == null || _hotkeys[key].key == KeyCode.None)
            {
                //if key was in the array, remove it
                if (_boundHotkeysToAdd.Contains(key))
                    _boundHotkeysToAdd.Remove(key);
                return;
            }

            //else add it to the array if it's not there yet
            if (!_boundHotkeysToAdd.Contains(key))
                _boundHotkeysToAdd.Add(key);
        }

        private static void AddOrRemoveKeyFromBound(ref KeyBind[] keyBinds, ref List<int> boundArray, int key)
        {
            // if action or key is not set, key should not be in the array
            if (keyBinds[key].action == null || keyBinds[key].key == KeyCode.None)
            {
                // if key was in the array, remove it
                if (boundArray.Contains(key))
                    boundArray.Remove(key);
                return;
            }

            // else add it to the array if it's not there yet
            if (!boundArray.Contains(key))
                boundArray.Add(key);
        }

        private void ProcessHotkeysOnDown(CommandKeys commandKeys, List<int> boundHotkeys)
        {
            for (int i = 0; i < boundHotkeys.Count; i++)
            {
                // Skip if we're not pressing hotkey or we're not pressing the exact combination of command keys
                if (!Input.GetKeyDown(_hotkeys[boundHotkeys[i]].key) || _hotkeys[boundHotkeys[i]].commandKeys != commandKeys)
                    continue;
                _hotkeys[boundHotkeys[i]].action(_hotkeys[boundHotkeys[i]].quickCast);
            }
        }

        private void ProcessHotkeysOnHold(CommandKeys commandKeys, List<int> boundHotkeys)
        {
            for (int i = 0; i < boundHotkeys.Count; i++)
            {
                // Skip if we're not pressing hotkey or we're not pressing the exact combination of command keys or if not enough time has elapsed
                if (!Input.GetKey(_hotkeys[boundHotkeys[i]].key) || _hotkeys[boundHotkeys[i]].commandKeys != commandKeys
                    || Common.Globals.Time < _hotkeys[boundHotkeys[i]].lastSend)
                    continue;
                _hotkeys[boundHotkeys[i]].action(_hotkeys[boundHotkeys[i]].quickCast);
                _hotkeys[boundHotkeys[i]].lastSend += +Common.Constants.SEND_INPUT_DELAY;
            }
        }

        private void ProcessMacros(CommandKeys commandKeys, List<int> boundMacros)
        {
            for (int i = 0; i < boundMacros.Count; i++)
            {
                // Skip if we're not pressing hotkey or we're not pressing the exact combination of command keys
                if (!Input.GetKeyDown(_macros[boundMacros[i]].key) || _macros[boundMacros[i]].commandKeys != commandKeys)
                    continue;
                _macros[boundMacros[i]].action();
            }
        }

        private void ProcessShortcuts(CommandKeys commandKeys, List<int> boundShortcuts)
        {
            for (int i = 0; i < boundShortcuts.Count; i++)
            {
                // Skip if we're not pressing hotkey
                if (!Input.GetKeyDown(_shortcuts[boundShortcuts[i]].key) || _shortcuts[boundShortcuts[i]].commandKeys != commandKeys)
                    continue;
                _shortcuts[boundShortcuts[i]].action();
            }
        }

        private static string ConvertKeysToString(KeyCode key, CommandKeys commandKeys)
        {
            if (!_keyCodesAsString.TryGetValue(key, out var keyCode))
                keyCode = Enum.GetName(typeof(KeyCode), key);

            string prefix = _commandKeysAsString[(int)commandKeys];
            return prefix != null ? prefix + keyCode : keyCode;
        }
    }
}

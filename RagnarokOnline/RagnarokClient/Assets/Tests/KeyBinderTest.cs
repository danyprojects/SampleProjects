using NUnit.Framework;
using RO.IO;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Tests
{
    class KeyBinderTest : MonoBehaviour
    {
        private struct KeyBindHotkey
        {
            public KeyCode key;
            public KeyBinder.CommandKeys commandKeys;
            public Action<bool> action;
            public float lastSend;
            public bool hold;
            public bool quickCast;
        }

        Type keyBindType = null;
        private dynamic _hotkeys = null;
        List<int> _boundHotkeysDown = null;
        List<int> _boundHotkeysHold = null;
        List<int> _boundInputHotkeysDown = null;
        List<int> _boundInputHotkeysHold = null;

        private void Init()
        {
            //Get private types
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (assembly.ManifestModule.Name != "RagnarokClient.dll")
                    continue;
                var types = assembly.GetTypes();
                keyBindType = assembly.GetType("RO.IO.KeyBinder+KeyBindHotkey");
            }

            Assert.IsNotNull(keyBindType);

            var flags = BindingFlags.NonPublic | BindingFlags.Static;
            //Get the array of hotkeys
            Type keyBinderType = typeof(KeyBinder);
            FieldInfo fieldInfo = keyBinderType.GetField("_hotkeys", flags);
            _hotkeys = Convert.ChangeType(fieldInfo.GetValue(null), keyBindType.MakeArrayType());

            //Get lists of bound hotkeys
            _boundHotkeysDown = (List<int>)keyBinderType.GetField("_boundOnDownHotkeys", flags).GetValue(null);
            _boundHotkeysHold = (List<int>)keyBinderType.GetField("_boundOnHoldHotkeys", flags).GetValue(null);
            _boundInputHotkeysDown = (List<int>)keyBinderType.GetField("_boundOnDownInputHotkeys", flags).GetValue(null);
            _boundInputHotkeysHold = (List<int>)keyBinderType.GetField("_boundOnHoldInputHotkeys", flags).GetValue(null);

            Assert.IsNotNull(_hotkeys);
            Assert.IsNotNull(_boundHotkeysDown);
            Assert.IsNotNull(_boundHotkeysHold);
            Assert.IsNotNull(_boundInputHotkeysDown);
            Assert.IsNotNull(_boundInputHotkeysHold);
        }

        private KeyBindHotkey GetHotkey(KeyBinder.Hotkey hotkey)
        {
            KeyBindHotkey bind;
            var flags = BindingFlags.Public | BindingFlags.Instance;

            bind.action = (Action<bool>)keyBindType.GetField("action", flags).GetValue(_hotkeys.GetValue((int)hotkey));
            bind.commandKeys = (KeyBinder.CommandKeys)keyBindType.GetField("commandKeys", flags).GetValue(_hotkeys.GetValue((int)hotkey));
            bind.key = (KeyCode)keyBindType.GetField("key", flags).GetValue(_hotkeys.GetValue((int)hotkey));
            bind.hold = (bool)keyBindType.GetField("hold", flags).GetValue(_hotkeys.GetValue((int)hotkey));
            bind.quickCast = (bool)keyBindType.GetField("quickCast", flags).GetValue(_hotkeys.GetValue((int)hotkey));
            bind.lastSend = (float)keyBindType.GetField("lastSend", flags).GetValue(_hotkeys.GetValue((int)hotkey));

            return bind;
        }

        [Test]
        public void HotkeysNoInputTest()
        {
            Init();

            bool cast = false;
            bool hold = false;
            KeyBinder.KeysPanel.RegisterKey(KeyBinder.Hotkey._1_1, KeyCode.F1, hold, KeyBinder.CommandKeys.None, cast);

            var hotkey = GetHotkey(KeyBinder.Hotkey._1_1);

            Assert.AreEqual(hotkey.key, KeyCode.F1);
            Assert.IsTrue(_boundHotkeysDown.Count + _boundHotkeysHold.Count == 0); //No bound keys yet

            KeyBinder.RegisterAction(KeyBinder.Hotkey._1_1, (bool quickCast) => Assert.AreEqual(cast, quickCast));
            //Check it enters the right lists
            Assert.IsTrue(_boundHotkeysDown.Count == 1);
            Assert.IsTrue(_boundHotkeysHold.Count == 0);

            //Check quick cast
            hotkey = GetHotkey(KeyBinder.Hotkey._1_1);
            hotkey.action(hotkey.quickCast);

            //Register again with hold and quick cast this time
            hold = true;
            cast = true;
            KeyBinder.KeysPanel.RegisterKey(KeyBinder.Hotkey._1_1, KeyCode.F1, hold, KeyBinder.CommandKeys.None, cast);

            //Check it swaps lists
            Assert.IsTrue(_boundHotkeysDown.Count == 0);
            Assert.IsTrue(_boundHotkeysHold.Count == 1);

            //Check quick cast
            hotkey = GetHotkey(KeyBinder.Hotkey._1_1);
            hotkey.action(hotkey.quickCast);

            //Cleanup
            KeyBinder.RegisterAction(KeyBinder.Hotkey._1_1, null);
            KeyBinder.KeysPanel.RegisterKey(KeyBinder.Hotkey._1_1, KeyCode.None, hold, KeyBinder.CommandKeys.None, false);
        }

        [Test]
        public void HotkeysInputTest()
        {
            Init();

            bool cast = false;
            bool hold = false;
            KeyBinder.KeysPanel.RegisterKey(KeyBinder.Hotkey._1_1, KeyCode.A, hold, KeyBinder.CommandKeys.None, cast);

            var hotkey = GetHotkey(KeyBinder.Hotkey._1_1);

            Assert.AreEqual(hotkey.key, KeyCode.A);
            Assert.IsTrue(_boundInputHotkeysDown.Count + _boundInputHotkeysHold.Count == 0); //No bound keys yet

            KeyBinder.RegisterAction(KeyBinder.Hotkey._1_1, (bool quickCast) => Assert.AreEqual(cast, quickCast));
            //Check it enters the right lists
            Assert.IsTrue(_boundInputHotkeysDown.Count == 1);
            Assert.IsTrue(_boundInputHotkeysHold.Count == 0);

            //Check quick cast
            hotkey = GetHotkey(KeyBinder.Hotkey._1_1);
            hotkey.action(hotkey.quickCast);

            //Register again with hold and quick cast this time
            hold = true;
            cast = true;
            KeyBinder.KeysPanel.RegisterKey(KeyBinder.Hotkey._1_1, KeyCode.A, hold, KeyBinder.CommandKeys.None, cast);

            //Check it swaps lists
            Assert.IsTrue(_boundInputHotkeysDown.Count == 0);
            Assert.IsTrue(_boundInputHotkeysHold.Count == 1);

            //Check quick cast
            hotkey = GetHotkey(KeyBinder.Hotkey._1_1);
            hotkey.action(hotkey.quickCast);

            //Cleanup
            KeyBinder.RegisterAction(KeyBinder.Hotkey._1_1, null);
            KeyBinder.KeysPanel.RegisterKey(KeyBinder.Hotkey._1_1, KeyCode.None, hold, KeyBinder.CommandKeys.None, false);
        }

        [Test]
        public void HotkeysInputCommandTest()
        {
            Init();

            bool cast = false;
            bool hold = false;
            KeyBinder.KeysPanel.RegisterKey(KeyBinder.Hotkey._1_1, KeyCode.A, hold, KeyBinder.CommandKeys.Shift, cast);

            var hotkey = GetHotkey(KeyBinder.Hotkey._1_1);

            Assert.AreEqual(hotkey.key, KeyCode.A);
            Assert.AreEqual(hotkey.commandKeys, KeyBinder.CommandKeys.Shift);
            Assert.IsTrue(_boundInputHotkeysDown.Count + _boundInputHotkeysHold.Count == 0); //No bound keys yet

            KeyBinder.RegisterAction(KeyBinder.Hotkey._1_1, (bool quickCast) => Assert.AreEqual(cast, quickCast));
            //Check it enters the right lists
            Assert.IsTrue(_boundInputHotkeysDown.Count == 1);
            Assert.IsTrue(_boundInputHotkeysHold.Count == 0);

            //Check quick cast
            hotkey = GetHotkey(KeyBinder.Hotkey._1_1);
            hotkey.action(hotkey.quickCast);

            //Register again with hold and quick cast this time
            hold = true;
            cast = true;
            KeyBinder.KeysPanel.RegisterKey(KeyBinder.Hotkey._1_1, KeyCode.A, hold, KeyBinder.CommandKeys.None, cast);

            //Check it swaps lists
            Assert.IsTrue(_boundInputHotkeysDown.Count == 0);
            Assert.IsTrue(_boundInputHotkeysHold.Count == 1);

            //Check quick cast
            hotkey = GetHotkey(KeyBinder.Hotkey._1_1);
            hotkey.action(hotkey.quickCast);

            //Cleanup
            KeyBinder.RegisterAction(KeyBinder.Hotkey._1_1, null);
            KeyBinder.KeysPanel.RegisterKey(KeyBinder.Hotkey._1_1, KeyCode.None, hold, KeyBinder.CommandKeys.None, false);
        }
    }
}

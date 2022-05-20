using System;
using System.Collections.Generic;
using UnityEngine;

namespace RO.UI
{
    public class MessageDialogController
    {
        private UIController _uiController;
        private Transform _rootParent;
        private Stack<MessageDialog> _dialogs;
        private List<MessageDialog> _dialogsActive;
        private const int _deleteThreshold = 2;

        public MessageDialogController(UIController uiController, Transform rootParent)
        {
            _uiController = uiController;
            _rootParent = rootParent;
            _dialogs = new Stack<MessageDialog>();
            _dialogsActive = new List<MessageDialog>();

            for (int i = 0; i < _deleteThreshold; i++)
                _dialogs.Push(MessageDialog.Instantiate(uiController, rootParent));
        }

        public void CancelAllDialogs()
        {
            foreach (var dialog in _dialogsActive)
                FreeDialog(dialog);

            _dialogsActive.Clear();
        }

        public MessageDialog ShowDialog(string message, Action ok, CanvasFilter filter = 0)
        {
            UIController.Panel.SetCanvasFilterFlags(filter);

            var dialog = _dialogs.Count > 0 ? _dialogs.Pop()
                : MessageDialog.Instantiate(_uiController, _rootParent);

            dialog.gameObject.SetActive(true); // so awake can trigger if needed 
            dialog.Fill(message, filter | CanvasFilter.NpcDialog,
                () => { FreeAndRemoveDialog(dialog); ok(); },
                null);

            _dialogsActive.Add(dialog);
            dialog.gameObject.transform.SetAsLastSibling();

            return dialog;
        }

        public MessageDialog ShowDialog(string message, Action ok, Action cancel, CanvasFilter filter = 0)
        {
            UIController.Panel.SetCanvasFilterFlags(filter);

            var dialog = _dialogs.Count > 0 ? _dialogs.Pop()
                : MessageDialog.Instantiate(_uiController, _rootParent);

            dialog.gameObject.SetActive(true); // so awake can trigger if needed 
            dialog.Fill(message, filter | CanvasFilter.NpcDialog,
                () => { FreeAndRemoveDialog(dialog); ok(); },
                () => { FreeAndRemoveDialog(dialog); cancel(); });

            _dialogsActive.Add(dialog);
            dialog.gameObject.transform.SetAsLastSibling();

            return dialog;
        }

        // Meant in case we want to cancel dialog programatically
        public void FreeAndRemoveDialog(MessageDialog dialog)
        {
            _dialogsActive.Remove(dialog);
            FreeDialog(dialog);
        }

        private void FreeDialog(MessageDialog dialog)
        {
            dialog.gameObject.SetActive(false);

            // it fine to clear both of them veven if they were off
            UIController.Panel.ClearCanvasFilterFlags(CanvasFilter.ModalMsgDialog | CanvasFilter.DisconnectDialog);

            if (_dialogs.Count <= _deleteThreshold)
                _dialogs.Push(dialog);
            else
                MonoBehaviour.Destroy(dialog.gameObject);
        }
    }
}
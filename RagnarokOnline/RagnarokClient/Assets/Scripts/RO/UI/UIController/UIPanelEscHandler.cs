using System;
using UnityEngine;

namespace RO.UI
{
    public partial class UIController : MonoBehaviour
    {
        public abstract partial class Panel : MonoBehaviour, ICanvasRaycastFilter
        {
            [Serializable]
            enum EscAction
            {
                DisableStack,
                Ignore,
                ClosePanel
            }

            public class EscHandler
            {
                private UIController _uiController;
                private Panel _head;
                private int _disableCount = 0;
                private MessageDialog _quitDialog;

                public EscHandler(UIController uiController)
                {
                    _uiController = uiController;
                }

                public void OnPanelEnabled(Panel panel)
                {
                    switch (panel.escAction)
                    {
                        case EscAction.ClosePanel:
                            if (_head != null)
                                _head._prevPanel = panel;
                            panel._nextPanel = _head;
                            _head = panel;
                            break;
                        case EscAction.DisableStack:
                            _disableCount++;
                            break;
                    }
                }

                public void OnPanelDisabled(Panel panel)
                {
                    switch (panel.escAction)
                    {
                        case EscAction.ClosePanel:
                            var prev = panel._prevPanel;
                            var next = panel._nextPanel;
                            panel._prevPanel = panel._nextPanel = null;

                            if (prev != null)
                                prev._nextPanel = next;
                            if (next != null)
                                next._prevPanel = prev;
                            if (_head == panel)
                                _head = next;
                            break;
                        case EscAction.DisableStack:
                            _disableCount--;
                            break;
                    }
                }

                public void Disable()
                {
                    _disableCount += 2;
                }

                public void Enable()
                {
                    _disableCount -= 2;
                }

                public void Update()
                {
                    if (Input.GetKeyDown(KeyCode.Escape))
                        OnEscPressed();
                }

                private void OnEscPressed()
                {
                    // allow it to pass while we have the quit dialog in lobby unless another dialog pops up
                    if (!(_disableCount == 0 || (_disableCount == 1 && _quitDialog != null)))
                        return;

                    if (_uiController.isActiveAndEnabled)
                    {
                        if (_quitDialog == null)
                        {
                            //_uiController._messageDialogController.
                            _quitDialog = _uiController._messageDialogController.ShowDialog(
                                "Are you sure you want to quit?", () => Application.Quit(),
                                () => { _quitDialog = null; }
                                , CanvasFilter.ModalMsgDialog);
                        }
                        else
                        {
                            _uiController._messageDialogController.FreeAndRemoveDialog(_quitDialog);
                            _quitDialog = null;
                        }
                    }
                    else
                    {
                        if (_head != null)
                            _head.gameObject.SetActive(false);
                        else
                            _uiController._gameOptionPanelController.gameObject.SetActive(true);
                    }
                }
            }
        }
    }
}
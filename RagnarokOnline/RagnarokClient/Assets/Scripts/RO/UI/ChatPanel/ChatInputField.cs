using UnityEngine;

namespace RO.UI
{
    public partial class ChatPanelController : UIController.Panel
    {
        public class ChatInputField : InputField
            , IKeyboardHandler
        {
            [SerializeField]
            private Toggle _chatOnToogle = default;

            public void Init()
            {
                _chatOnToogle.OnValueChanged = (x) => transform.parent.gameObject.SetActive(x);
            }

            public void ToogleChatOff()
            {
                _chatOnToogle.SetValue(false);
            }

            public new void OnKeyDown(Event evt)
            {
                if (transform.parent.gameObject.activeInHierarchy)
                    base.OnKeyDown(evt);
                else if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    _chatOnToogle.SetValue(true);
                    base.OnKeyboardFocus(true);
                }
            }

            public new void OnKeyboardFocus(bool hasFocus)
            {
                if (!hasFocus)
                {
                    base.OnKeyboardFocus(false);
                }
                // This will trigger 1st time before awake has run so check directly our parent instead
                else if (transform.parent.gameObject.activeInHierarchy)
                    base.OnKeyboardFocus(true);
            }
        }
    }

    public class ChatInputField : ChatPanelController.ChatInputField { }
}
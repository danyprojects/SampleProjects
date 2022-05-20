using RO.Network;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace RO.UI
{
    public sealed class LoginPanelController : UIController.Panel
    {
        [SerializeField]
        private InputField _username = default;

        [SerializeField]
        private InputField _password = default;

        [SerializeField]
        private Toggle _keepToogle = default;

        [SerializeField]
        private Button _loginButton = default;

        [SerializeField]
        private Button _registerButton = default;

        [SerializeField]
        private Button _exitButton = default;

        private SND_Login _sndLogin = new SND_Login();
        private SND_RegisterAccount _sndRegister = new SND_RegisterAccount();

        //TODO:: add regex validators, and pop-up messages on click
        private Regex nameRegex = new Regex(@"[a-zA-Z]([0-9a-zA-Z]){3,}", RegexOptions.Compiled);
        private Regex passwordRegex = new Regex(@".{8,}", RegexOptions.Compiled);

        public LoginPanelController()
        {
            PacketDistributer.RegisterCallback(PacketIds.RCV_ReplyRegisterAccount, OnRegisterReply);
            PacketDistributer.RegisterCallback(PacketIds.RCV_ReplyInvalidLogin, OnInvalidLogin);
        }

        private void Awake()
        {
            _loginButton.OnClick = RequestLogin;
            _registerButton.OnClick = RequestRegister;
            _exitButton.OnClick = OnExitClick;
            _keepToogle.OnValueChanged = OnKeepChanged;

            _username.OnSubmit = (u) => EventSystem.CurrentKeyboardHandler = _password;

            _username.OnTab = () => EventSystem.CurrentKeyboardHandler = _password;
            _password.OnTab = () => EventSystem.CurrentKeyboardHandler = _username;
        }

        private new void OnEnable()
        {
            base.OnEnable();
            EventSystem.CurrentKeyboardHandler = _username;

            _password.Clear();
        }

        private new void OnDisable()
        {
            base.OnDisable();
            EventSystem.CurrentKeyboardHandler = null;
            RO.Media.CursorAnimator.UnsetAnimation(RO.Media.CursorAnimator.Animations.Click);
        }

        private void OnExitClick()
        {
            Application.Quit();
        }

        private void OnKeepChanged(bool selected)
        {
            //TODO::
        }

        private void OnRegisterReply(RCV_Packet packet)
        {
            var replyPacket = (RCV_ReplyRegisterAccount)packet;

            switch (replyPacket.replyStatus)
            {
                case RCV_ReplyRegisterAccount.ReplyStatus.Error_DuplicateName:
                    ShowDialog("Username is already taken");
                    break;
                case RCV_ReplyRegisterAccount.ReplyStatus.Error_Generic:
                    ShowDialog("Internal server error");
                    break;
            }
        }

        private void OnInvalidLogin(RCV_Packet packet)
        {
            ShowDialog("Invalid username or password");
        }

        private void ShowDialog(string message)
        {
            gameObject.SetActive(false);
            MessageDialogController.ShowDialog(message, () => gameObject.SetActive(true));
        }

        private void RequestLogin()
        {
            if (!nameRegex.IsMatch(_username.Text) || !passwordRegex.IsMatch(_password.Text))
            {
                ShowDialog("Invalid username or password");
                return;
            }

            //Try to connect whenever login is pressed
            if (!NetworkController.IsConnected() && !NetworkController.StartTcp())
            {
                ShowDialog("Could not connect to server");
                return;
            }

            _sndLogin.Name = _username.Text;
            _sndLogin.Password = _password.Text;
            NetworkController.SendPacket(_sndLogin);
            _password.Clear();
        }

        private void RequestRegister()
        {
            if (_username.Text == null || _username.Text.Length < 2)
            {
                ShowDialog("Username must have at least 3 characters");
                return;
            }

            if (Char.IsDigit(_username.Text.First()))
            {
                ShowDialog("Username cannot start with a number");
                return;
            }

            if (!nameRegex.IsMatch(_username.Text))
            {
                ShowDialog("Invalid username, use only alphanumeric characters");
                return;
            }

            if (!passwordRegex.IsMatch(_password.Text))
            {
                ShowDialog("Password must be at least 8 characters");
                return;
            }

            if (!NetworkController.IsConnected() && !NetworkController.StartTcp())
            {
                ShowDialog("Could not connect to server");
                return;
            }

            _sndRegister.Name = _username.Text;
            _sndRegister.Password = _password.Text;
            NetworkController.SendPacket(_sndRegister);
            _password.Clear();
        }

        public static LoginPanelController Instantiate(UIController uiController, Transform parent)
        {
            return Instantiate<LoginPanelController>(uiController, parent, "LoginPanel");
        }
    }
}

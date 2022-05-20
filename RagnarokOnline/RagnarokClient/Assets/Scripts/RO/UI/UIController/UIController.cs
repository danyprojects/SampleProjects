using RO.Common;
using RO.Network;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    using static RO.Databases.MapDb;
    using Skill = LocalPlayer.SkillTree.Skill;

    public sealed partial class UIController : MonoBehaviour
    {
        public class CharacterInfo
        {
            public struct Gear
            {
                public int id;
                public int palleteId;
            }

            public string name;
            public string currentMap;
            public Jobs job;
            public Gender gender;
            public int hairstyle;
            public int baseLevel;
            public int jobLevel;
            public int maxHp;
            public int currentHp;
            public int maxSp;
            public int currentSp;
            public short str;
            public short agi;
            public short vit;
            public short int_;
            public short dex;
            public short luk;
            public short index;

            public int totalExp;
            public int currentBaseExp, currentJobExp;
            public int weight, maxWeight;
            public int zeny;

            public Gear[] gear;

            public bool isMounted;
        }

        private CharacterInfo[] _availableCharacters;
        private CharacterInfo _selectedCharacter;
        private RCV_Packet _packetIn;

        [SerializeField]
        private Canvas _rootCanvas = default;

        // UI materials need to be injected from editor
        [SerializeField]
        private Material[] _uiMaterials = new Material[8];
        [SerializeField]
        private Material[] _uiMaterialsTransparent = new Material[8];

        private DragIconController _dragIconController;
        private LoginPanelController _loginPanelController;
        private CharacterMakePanelController _characterMakePanelController;
        private CharacterSelectPanelController _characterSelectPanelController;
        private BasicInfoPanelController _basicInfoPanelController;
        private EquipmentPanelController _equipmentPanelController;
        private SkillPanelController _skillPanelController;
        private BattleModePanelController _battleModePanelController;
        private GameOptionPanelController _gameOptionPanelController;
        private BackgroundPanelController _backgroundPanelController;
        private BuffPanelController _buffPanelController;
        private SoundPanelController _soundPanelController;
        private ShortcutPanelController _shortcutPanelController;
        private MiniMapPanelController _miniMapPanelController;
        private ChatPanelController _chatPanelController;

        private bool _isLobbyTriggeredDisconnect = false;
        private Panel.EscHandler _escHandler;
        private LabelController _labelController;
        private MessageDialogController _messageDialogController;

        // Called by GameController every update to allow ui to run internal updates if needed
        public void Process()
        {
            _escHandler.Update();
        }

        public void Disconnect()
        {
            if (!_isLobbyTriggeredDisconnect)
            {
                _messageDialogController.ShowDialog("Disconnected from server",
                () =>
                {
                    EnterLobby();
                    LoginPanelSetActive(true);
                }, CanvasFilter.ModalMsgDialog | CanvasFilter.DisconnectDialog);
            }
            _isLobbyTriggeredDisconnect = false;
        }

        public CharacterInfo GetSelectedCharacter()
        {
            return _selectedCharacter;
        }

        public void EnterBlackScreen()
        {
            _backgroundPanelController.ShowBlackScreen();
        }

        // Fades screen into black
        public void FadeOutScreen()
        {
            _backgroundPanelController.ShowFadeOut();
        }

        // Fades black into screen
        public void FadeInScreen()
        {
            _backgroundPanelController.ShowFadeIn();
        }

        public void EnterLoadingScreen()
        {
            _backgroundPanelController.EnterLoadingScreen();
        }

        public void SetLoadProgress(float progress)
        {
            _backgroundPanelController.SetLoadProgress(progress);
        }

        public void ExitLoadingScreen()
        {
            _backgroundPanelController.ExitLoadingScreen();
        }

        public void EnterLobby()
        {
            UISetActive(false);

            _loginPanelController = LoginPanelController.Instantiate(this, _rootCanvas.transform);
            _characterSelectPanelController = CharacterSelectPanelController.Instantiate(this, _rootCanvas.transform);
            _characterMakePanelController = CharacterMakePanelController.Instantiate(this, _rootCanvas.transform);

            // deactivate these in case DC happened in lobby
            _characterSelectPanelController.gameObject.SetActive(false);
            _characterMakePanelController.gameObject.SetActive(false);

            _gameOptionPanelController.EnteredLobby();

            SoundController.PlayLoginBgm();
            _backgroundPanelController.ShowBackgroundImage();
        }

        public void ExitLobby()
        {
            _loginPanelController.gameObject.SetActive(false);
            _characterMakePanelController.gameObject.SetActive(false);
            _characterSelectPanelController.gameObject.SetActive(false);

            Destroy(_loginPanelController.gameObject);
            Destroy(_characterMakePanelController.gameObject);
            Destroy(_characterSelectPanelController.gameObject);

            _loginPanelController = null;
            _characterMakePanelController = null;
            _characterSelectPanelController = null;
        }

        public void LoadMiniMap(MapIds mapId, int mapWidth, int mapHeight)
        {
            _miniMapPanelController.Load(mapId, mapWidth, mapHeight);
        }

        public void UISetActive(bool active)
        {
            if (!active)
                _messageDialogController.CancelAllDialogs();

            _basicInfoPanelController.gameObject.SetActive(active);
            _skillPanelController.gameObject.SetActive(false);
            _battleModePanelController.gameObject.SetActive(active);
            _gameOptionPanelController.gameObject.SetActive(false);
            _buffPanelController.gameObject.SetActive(active);
            _soundPanelController.gameObject.SetActive(false);
            _shortcutPanelController.gameObject.SetActive(false);
            _miniMapPanelController.gameObject.SetActive(active);
            _chatPanelController.gameObject.SetActive(active);
        }

        public void SkillTreeReload(Skill[] skills, Jobs job)
        {
            _skillPanelController.Load(skills, job);
        }

        public void SkillTreeAddExtraSkill(Skill skill)
        {

        }

        public void SkillTreeRemoveExtraSkill(Skill skill)
        {

        }

        public void UpdatePlayerPosition(Vector2Int position)
        {
            _miniMapPanelController.UpdatePlayerPosition(position);
        }

        public void UpdatePlayerDirection(int direction)
        {
            _miniMapPanelController.UpdatePlayerDirection(direction);
        }

        public void UpdatePlayerSkillPoints(int skillPoints)
        {
            _skillPanelController.SetSkillPoints(skillPoints);
        }

        public void UpdatePlayerMaxHP(int hp)
        {
            _basicInfoPanelController.SetMaxHP(hp);
        }

        public void UpdatePlayerHP(int hp)
        {
            _basicInfoPanelController.SetHP(hp);
        }

        public void UpdatePlayerMaxSP(int sp)
        {
            _basicInfoPanelController.SetMaxSP(sp);
        }

        public void UpdatePlayerSP(int sp)
        {
            _basicInfoPanelController.SetSP(sp);
        }

        public void UpdatePlayerName(string name)
        {
            _basicInfoPanelController.SetName(name);
        }

        public void UpdatePlayerJob(string job)
        {
            _basicInfoPanelController.SetJob(job);
        }

        public void UpdatePlayerBaseLvl(int lvl)
        {
            _basicInfoPanelController.SetBaseLvl(lvl);
        }

        public void UpdatePlayerJobLvl(int lvl)
        {
            _basicInfoPanelController.SetJobLvl(lvl);
        }

        public void UpdatePlayerZeny(int zeny)
        {
            _basicInfoPanelController.SetZeny(zeny);
        }

        public void UpdatePlayerWeight(int weight)
        {
            _basicInfoPanelController.SetWeight(weight);
        }

        public void UpdatePlayerMaxWeight(int weight)
        {
            _basicInfoPanelController.SetMaxWeight(weight);
        }

        public void UpdatePlayerJobExp(int exp)
        {
            _basicInfoPanelController.SetJobExp(exp);
        }

        public void UpdatePlayerJobNextLvlExp(int exp)
        {
            _basicInfoPanelController.SetJobNextLvlExp(exp);
        }

        public void UpdatePlayerBaseExp(int exp)
        {
            _basicInfoPanelController.SetBaseExp(exp);
        }

        public void UpdatePlayerBaseNextLvlExp(int exp)
        {
            _basicInfoPanelController.SetBaseNextLvlExp(exp);
        }

        public void AddBuff(Databases.BuffIDs buffId, float duration)
        {
            _buffPanelController.AddBuff(buffId, duration);
        }

        public void AddPermanentBuff(Databases.BuffIDs buffId)
        {
            _buffPanelController.AddPermanentBuff(buffId);
        }

        public void RemoveBuff(Databases.BuffIDs buffId)
        {
            _buffPanelController.RemoveBuff(buffId);
        }

        void Awake()
        {
            _escHandler = new Panel.EscHandler(this);

            _labelController = new LabelController(_rootCanvas.transform);
            _messageDialogController = new MessageDialogController(this, _rootCanvas.transform);

            _buffPanelController = BuffPanelController.Instantiate(this, _rootCanvas.transform);
            _miniMapPanelController = MiniMapPanelController.Instantiate(this, _rootCanvas.transform);
            _chatPanelController = ChatPanelController.Instantiate(this, _rootCanvas.transform);
            _basicInfoPanelController = BasicInfoPanelController.Instantiate(this, _rootCanvas.transform);
            _skillPanelController = SkillPanelController.Instantiate(this, _rootCanvas.transform);
            _dragIconController = DragIconController.Instantiate(this, _rootCanvas.transform);
            _battleModePanelController = BattleModePanelController.Instantiate(this, _rootCanvas.transform);
            _gameOptionPanelController = GameOptionPanelController.Instantiate(this, _rootCanvas.transform);
            _backgroundPanelController = BackgroundPanelController.Instantiate(this, _rootCanvas.transform);
            _soundPanelController = SoundPanelController.Instantiate(this, _rootCanvas.transform);
            _shortcutPanelController = ShortcutPanelController.Instantiate(this, _rootCanvas.transform);

            _availableCharacters = new CharacterInfo[CharacterSelectPanelController.MAX_CHARACTER_SLOTS];

            for (int i = 0; i < _availableCharacters.Length; i++)
            {
                _availableCharacters[i] = new CharacterInfo();
                _availableCharacters[i].gear = new CharacterInfo.Gear[10];
                _availableCharacters[i].index = (short)i;
            }

            if (_uiMaterials[0] != null) throw new MissingFieldException("default material is autofilled in");
            if (!_uiMaterials[1].name.Equals("UI_sprite_palette")) throw new MissingFieldException("UI_sprite_palette");
            if (!_uiMaterials[2].name.Equals("UI_grayscale")) throw new MissingFieldException("UI_grayscale");
            if (!_uiMaterials[3].name.Equals("UI_text_outline")) throw new MissingFieldException("UI_text_outline");
            if (!_uiMaterials[4].name.Equals("UI_battle_mode_slot")) throw new MissingFieldException("UI_battle_mode_slot");
            if (!_uiMaterials[5].name.Equals("UI_text_label")) throw new MissingFieldException("UI_text_label");
            if (!_uiMaterials[6].name.Equals("UI_mask")) throw new MissingFieldException("UI_mask");
            if (!_uiMaterials[7].name.Equals("UI_masked_text_label")) throw new MissingFieldException("UI_masked_text_label");

            if (!_uiMaterialsTransparent[0].name.Equals("UI_transparent")) throw new MissingFieldException("UI_transparent");
            if (!_uiMaterialsTransparent[1].name.Equals("UI_sprite_palette_transparent")) throw new MissingFieldException("UI_sprite_palette_transparent");
            if (!_uiMaterialsTransparent[2].name.Equals("UI_grayscale_transparent")) throw new MissingFieldException("UI_grayscale_transparent");
            if (!_uiMaterialsTransparent[3].name.Equals("UI_text_outline_transparent")) throw new MissingFieldException("UI_text_outline_transparent");
            if (!_uiMaterialsTransparent[4].name.Equals("UI_battle_mode_slot_transparent")) throw new MissingFieldException("UI_battle_mode_slot_transparent");
            if (!_uiMaterialsTransparent[5].name.Equals("UI_text_label_transparent")) throw new MissingFieldException("UI_text_label_transparent");
            if (!_uiMaterialsTransparent[6].name.Equals("UI_mask_transparent")) throw new MissingFieldException("UI_mask_transparent");
            if (!_uiMaterialsTransparent[7].name.Equals("UI_masked_text_label_transparent")) throw new MissingFieldException("UI_masked_text_label_transparent");

            _uiMaterials[0] = Graphic.defaultGraphicMaterial;
        }

        void Start()
        {
            DontDestroyOnLoad(gameObject);

            SoundController.SetBgmVolume(SoundController.DEFAULT_BGM_VOLUME);
            SoundController.SetEffectsVolume(SoundController.DEFAULT_EFFECTS_VOLUME);

            EnterLobby();
            LoginPanelSetActive(true);
        }

        // runs only when GameController is disabled
        private void Update()
        {
            //Process MAX_PACKETS_PER_LOOP packets from queue
            for (int i = 0; i < Constants.MAX_READ_PACKETS_PER_LOOP; i++)
            {
                if (NetworkController.QueuePacketsIn.TryDequeue(out _packetIn))
                    PacketDistributer.Distribute(_packetIn);
                else //No packets to process, don't continue loop
                    break;

                if (!enabled)
                    return;
            }

            //Update the cursor state
            if (Input.GetMouseButtonDown(0))
                Media.CursorAnimator.OnCursorDown();
            if (Input.GetMouseButtonUp(0))
                Media.CursorAnimator.OnCursorUp();

            Process(); // These run every update
#if STANDALONE_CLIENT
            ProcessSentPackets();
#endif
        }

        private void TriggerLobbyDisconnect()
        {
            if (_isLobbyTriggeredDisconnect = TcpSocket.IsConnected)
                TcpSocket.Disconnect();
        }

        private void LoginPanelSetActive(bool active)
        {
            _loginPanelController.gameObject.SetActive(active);
        }

        private void CharacterSelectPanelSetActive(bool active)
        {
            _characterSelectPanelController.gameObject.SetActive(active);
        }

        private void CharacterMakePanelSetActive(bool active)
        {
            _characterMakePanelController.gameObject.SetActive(active);
        }

        private void SkillPanelSetActive(bool active)
        {
            Debug.Assert(_skillPanelController != null);
            _skillPanelController.gameObject.SetActive(active);
        }

        private void SoundPanelSetActive(bool active)
        {
            Debug.Assert(_soundPanelController != null);
            _soundPanelController.gameObject.SetActive(active);
        }

#if STANDALONE_CLIENT
        //Methods for standalone mode
        SND_Packet _packetOut;

        public void ProcessSentPackets()
        {
            for (int i = 0; i < Constants.MAX_READ_PACKETS_PER_LOOP; i++)
            {
                if (NetworkController.QueuePacketsOut.TryDequeue(out _packetOut))
                {
                    try
                    {
                        PacketMirrorer.PacketMirrors[_packetOut.PacketId](_packetOut);
                    }
                    catch
                    {
                        Debug.Log("No handler for packet ID: " + (PacketIds)_packetOut.PacketId);
                    }
                }
                else //No packets to process, don't continue loop
                    break;
            }
        }
#endif

    }
}
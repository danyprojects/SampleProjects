using Algorithms;
using RO.Common;
using RO.Databases;
using RO.IO;
using RO.LocalPlayer;
using RO.MapObjects;
using RO.Network;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static RO.Databases.MapDb;

namespace RO
{
    public partial class GameController : MonoBehaviour
    {
        private partial class BlockController { }
        private sealed partial class UnitController { }
        private sealed partial class SkillController { }
        private sealed partial class NpcController { }
        private sealed partial class BuffController { }
        private sealed partial class ItemController { }

        private Map _map = null;
        private LocalCharacter _localPlayer = null;
        private MapPortal[] _mapPortals = null;

        private int _mapId = 1;
        private bool _isComingFromLobby = true;
        private Vector2Int _spawnPosition = Vector2Int.zero;
        private Character.Direction _spawnDirection = new Character.Direction(0, 0);

        private RCV_Packet _packetIn;

        [SerializeField] private CameraFollow _cameraFollow = null;
        [SerializeField] private InputHandler _inputHandler = null;
        [SerializeField] private UI.UIController _uiController = null;

        private BlockController _blocks = null;
        private UnitController _unitController = null;
        private SkillController _skillController = null;
        private NpcController _npcController = null;
        private BuffController _buffController = null;
        private ItemController _itemController = null;

        //list of static class updaters
        private Media.FloatingTextController.Updater _floatingTextUpdater = null;
        private Media.EffectsAnimatorController.Updater _effectsUpdater = null;
        private TimerController.Dispatcher _timerDispatcher = null;
        private WaitForSecondsRealtime _waitForSecondsRealtime = new WaitForSecondsRealtime(1);

        public LocalCharacter LocalPlayer
        {
            get
            {
                return _localPlayer;
            }
            private set
            {
                _localPlayer = value;
                _localPlayer.gameObject.transform.parent = null;
                DontDestroyOnLoad(_localPlayer.gameObject);

                SoundController.MoveAudioListenerToObject(_localPlayer.transform); // To have sound appear 3 dimensional centered on player
            }
        }

        private void Update()
        {
            // Process MAX_PACKETS_PER_LOOP packets from queue
            for (int i = 0; i < Constants.MAX_READ_PACKETS_PER_LOOP; i++)
            {
                if (NetworkController.QueuePacketsIn.TryDequeue(out _packetIn))
                    PacketDistributer.Distribute(_packetIn);
                else // No packets to process, don't continue loop
                    break;

                // Don't keep processing loop if any of the packets disabled game controller
                if (!enabled)
                    return;
            }

            // Run timers
            _timerDispatcher.Dispatch();

            MapStateLoop();
            UIStateLoop();

#if STANDALONE_CLIENT
            ProcessSentPackets();
#endif
        }

        // Avoid putting things here as much as possible
        private void UIStateLoop()
        {
            _uiController.UpdatePlayerDirection(_localPlayer._direction.Body);
            _uiController.UpdatePlayerPosition(_localPlayer.position);
            _uiController.Process(); // run general ui loop
        }

        private void MapStateLoop()
        {
            //Always process input first since other modules are dependent on it
            _inputHandler.UpdateInput();
            //Update camera before updating units
            _cameraFollow.UpdateCamera();

            _unitController.UpdateCharacters();
            _unitController.UpdateMonsters();
            _itemController.UpdateItems();

            //Update map for water / object animations
            _map.UpdateMap();

            //Update static classes
            _effectsUpdater.UpdateEffects();
            _floatingTextUpdater.UpdateFloatingTexts();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.buildIndex == 0) // skip game scene
                return;

            // Force update time since level load
            Globals.TimeSinceLevelLoad = Time.timeSinceLevelLoad;

            _uiController.ExitLoadingScreen();
            _uiController.EnterBlackScreen();

            // Give map references to all that need to know the map
            _map = GameObject.Find(scene.name).GetComponent<Map>();
            _inputHandler.AssignMap(_map);
            Media.CastCircleController.Updater.AssignMap(_map);
            Pathfinder.SetMap(_map.MapData);
            SoundController.PlayMapBgm(_map.MapData.Id);

            // If we came from lobby we need to assign camera target and main player
            if (_isComingFromLobby)
            {
                _cameraFollow.AssignTarget(_localPlayer.transform, _localPlayer);
                _localPlayer.UpdateRotationAsCenter(Globals.Camera.transform);
                _localPlayer.UpdateDirection(_cameraFollow.CameraDirection);

                _isComingFromLobby = false;
            }

            // Set prepared data from enter map packet
            _localPlayer.InstantAppearAtCell(_spawnPosition.x, _spawnPosition.y);
            _localPlayer.UpdateDirection(_spawnDirection);
            _cameraFollow.SnapToTarget();

            _uiController.LoadMiniMap((MapIds)_mapId, _map.MapData.Width, _map.MapData.Height);
            _uiController.UISetActive(true);
            _uiController.FadeInScreen();

            _uiController.enabled = false;
            enabled = true;
        }

        private void RecycleObjects()
        {
            // Cleanup player. Move somewhere else
            _localPlayer.flags.IsMoving = false;
            _localPlayer.flags.IsFixingPosition = false;

            // Timers have to be the first thing to clear
            _timerDispatcher.ClearOnSceneChange();

            _blocks.ClearCharacters();
            _blocks.ClearMonsters();
            _blocks.ClearItems();
            _npcController.ClearWarpPortals();

            // These should probably stay here
            _effectsUpdater.ClearEffects();
            _floatingTextUpdater.ClearFloatingTexts();

            foreach (var portal in _mapPortals)
                if (portal != null)
                    portal.IsEnabled = false;
        }

        private void Awake()
        {
            _mapPortals = new MapPortal[Constants.MAX_WARP_PORTAL_COUNT];

            RegisterPacketHandlers();

            _blocks = new BlockController();
            _unitController = new UnitController(this);
            _skillController = new SkillController(this);
            _npcController = new NpcController(this);
            _buffController = new BuffController(this);
            _itemController = new ItemController(this);

            _floatingTextUpdater = new Media.FloatingTextController.Updater();
            _effectsUpdater = new Media.EffectsAnimatorController.Updater();
            _timerDispatcher = new TimerController.Dispatcher();

            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            //    QualitySettings.vSyncCount = 0;
            //    Application.targetFrameRate = 3;

#if STANDALONE_CLIENT
            PacketMirrorer.GameController = this;
#endif
        }

        // Start of packet handlers
        private void RegisterPacketHandlers()
        {
            PacketDistributer.RegisterCallback(PacketIds.ConnectionClosed, OnConnectionClosed);
            PacketDistributer.RegisterCallback(PacketIds.RCV_EnterMap, OnEnterMap);
            PacketDistributer.RegisterCallback(PacketIds.RCV_EnterLobby, OnEnterLobby);
            PacketDistributer.RegisterCallback(PacketIds.RCV_ExitLobby, OnExitLobby);
        }

        private void OnConnectionClosed(RCV_Packet packet)
        {
            Debug.Log("Game manager received connection closed");

            SoundController.MoveAudioListenerToObject(null); // Since player might disappear

            // In any case disconnect trigers we are going to go to lobby
            // Uicontroller will do transition for lobby automatically on a Disconnect call
            _uiController.Disconnect();
            _isComingFromLobby = true;

            _uiController.enabled = true;
            enabled = false;
        }

        private void OnEnterLobby(RCV_Packet packet)
        {
            Debug.Log("Received OnEnterLobby");

            SoundController.MoveAudioListenerToObject(null); // Since player might disappear
            _isComingFromLobby = true;
            _uiController.EnterLobby();

            _uiController.enabled = true;
            enabled = false;
        }

        private void OnExitLobby(RCV_Packet packet)
        {
            Debug.Log("Received OnExitLobby");

            _uiController.ExitLobby();

            LocalPlayer = LocalPlayer != null ? LocalPlayer : new LocalCharacter(_uiController, _inputHandler);
            LocalPlayer.load(_uiController.GetSelectedCharacter());
        }

        private void OnEnterMap(RCV_Packet packet)
        {
            RCV_EnterMap mapPacket = (RCV_EnterMap)packet;
            Debug.Log("Game controller received enter map id: " + mapPacket.MapId);

            // temporary for testing
            InjectTestDataOnEnterMap();

            _uiController.EnterLoadingScreen();

            RecycleObjects();

            // Prepare data for moving map
            _mapId = mapPacket.MapId;
            _spawnPosition.x = mapPacket.PosX;
            _spawnPosition.y = mapPacket.PosY;
            _spawnDirection = mapPacket.direction;

            StartCoroutine(LoadScene());

            // this way we won't process any Update but will still get OnSceneLoaded called
            _uiController.enabled = false;
            enabled = false;
        }

        private IEnumerator LoadScene()
        {
            yield return null;

            AsyncOperation op = SceneManager.LoadSceneAsync(_mapId + Constants.MAP_SCENE_START);

            while (!op.isDone)
            {
                _uiController.SetLoadProgress(op.progress / 0.9f);
                yield return null;
            }
            _uiController.SetLoadProgress(1);

            yield return _waitForSecondsRealtime;
        }

        private void InjectTestDataOnEnterMap()
        {
            KeyBinder.RegisterAction(KeyBinder.Macro._1, () =>
            {
                var jobLvlPacket = new SND_ElevatedAtCommand();
                jobLvlPacket.makeJobLvlCMD(250);
                NetworkController.SendPacket(jobLvlPacket);
            });
            KeyBinder.RegisterAction(KeyBinder.Macro._2, () =>
            {
                var allSkillsPacket = new SND_ElevatedAtCommand();
                allSkillsPacket.makeAllSkillsCMD();
                NetworkController.SendPacket(allSkillsPacket);
            });

            KeyBinder.RegisterAction(KeyBinder.Macro._3, () =>
            {
                _localPlayer.castInfo.castAuraToken = Media.EffectsAnimatorController.PlayEffect(Databases.CylinderEffectIDs.MagicPillarBlue, _localPlayer.transform, 5, null);
            });

            KeyBinder.RegisterAction(KeyBinder.Macro._4, () =>
            {
                NetworkController.QueuePacketsIn.Enqueue(new RCV_MonsterEnterRange
                {
                    dbId = (short)Databases.MonsterIDs.RaydricArcher,
                    instanceId = 1,
                    posX = (short)_localPlayer.position.x,
                    posY = (short)_localPlayer.position.y,
                    enterType = EnterRangeType.Default
                });
            });

            KeyBinder.RegisterAction(KeyBinder.Macro._5, () =>
            {
                _localPlayer.castInfo.CancelCastAnimations();
            });

            KeyBinder.RegisterAction(KeyBinder.Macro._6, () =>
            {
                NetworkController.QueuePacketsIn.Enqueue(new RCV_PlayerJobOrLevelChanged
                {
                    playerId = Constants.LOCAL_SESSION_ID,
                    baseLevel = (byte)((_localPlayer.BaseLvl + 1) % 255),
                    job = Jobs.HighWizard,
                    jobLevel = (byte)((_localPlayer.JobLvl + 1) % 255)
                });
            });

            KeyBinder.RegisterAction(KeyBinder.Macro._7, () =>
            {
                NetworkController.QueuePacketsIn.Enqueue(new RCV_OtherEnterRange
                {
                    blockId = 0,
                    blockType = BlockTypes.Item,
                    dbId = (int)ItemIDs.Honey,
                    enterType = EnterRangeType.Default,
                    posX = (short)_localPlayer.position.x,
                    posY = (short)_localPlayer.position.y
                });
            });

            KeyBinder.RegisterAction(KeyBinder.Macro._8, () =>
            {
                _localPlayer.PlayerAnimator.PlayStandbyAnimation();
            });

            KeyBinder.RegisterAction(KeyBinder.Macro._9, () => { _localPlayer.PlayerAnimator.IsMounted = !_localPlayer.PlayerAnimator.IsMounted; });

            var jobchangePacket = new SND_ElevatedAtCommand();
            jobchangePacket.makeJobChangeCMD(Jobs.HighWizard);
            NetworkController.SendPacket(jobchangePacket);
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
                    }catch
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
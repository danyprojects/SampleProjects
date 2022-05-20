
using UnityEngine;
using Bacterio.Databases;
using Bacterio.Common;

namespace Bacterio
{
    /// <summary>
    /// Hold useful static data accessible from anywhere. Behaves as a singleton. Data is only updated by the owner of the instance
    /// </summary>
    public class GlobalContext
    {
        public static float localDeltaTimeSec { get; private set; }
        public static float localTimeSec { get; private set; }
        public static int localDeltaTimeMs { get; private set; }
        public static int localTimeMs { get; private set; }

        //databases
        public static StructureDb structureDb { get; private set; }
        public static AuraDb auraDb { get; private set; }
        public static TrapDb trapDb { get; private set; }
        public static EffectDb effectDb { get; private set; }
        public static ShopDb shopDb { get; private set; }
        public static BulletDb bulletDb { get; private set; }
        public static CellDb cellDb { get; private set; }

        //Other useful objects
        public static AssetBundleProvider assetBundleProvider { get; private set; }
        public static CameraController cameraController { get; private set; }
        public static ObjectPool<Transform> emptyTransformsPool { get; private set; }

        public GlobalContext()
        {
            assetBundleProvider = new AssetBundleProvider();
            cameraController = Camera.main.GetComponent<CameraController>();
            emptyTransformsPool = new ObjectPool<Transform>(new GameObject("EmptyTransform").transform, 0, 1, Vector3.zero, Quaternion.identity, null, OnTransformPush);

            //Init dbs
            structureDb = new StructureDb(assetBundleProvider);
            auraDb = new AuraDb(assetBundleProvider);
            trapDb = new TrapDb(assetBundleProvider);
            effectDb = new EffectDb(assetBundleProvider);
            shopDb = new ShopDb(assetBundleProvider);
            bulletDb = new BulletDb(assetBundleProvider);
            cellDb = new CellDb(assetBundleProvider);

            //Run some checks
            WDebug.Assert(cameraController != null, "No CameraController in main camera");

            Update();
        }

        public void Update()
        {
            localDeltaTimeSec = Time.deltaTime;
            localTimeSec = Time.time;

            var newLocalTimeMs = (int)(localTimeSec * Constants.ONE_SECOND_MS);
            localDeltaTimeMs = newLocalTimeMs - localTimeMs;
            localTimeMs = newLocalTimeMs;
        }

        public void Dispose()
        {
            structureDb = null;
            auraDb = null;
            trapDb = null;
            effectDb = null;
            shopDb = null;

            assetBundleProvider.Dispose();
            assetBundleProvider = null;
            cameraController = null;
        }

        private void OnTransformPush(Transform transform)
        {
            WDebug.Assert(transform.childCount == 0, "Released an empty transform to pool but it still had children");
            transform.SetParent(null, false);
            transform.localPosition = Vector3.zero;
            transform.rotation = Quaternion.identity;
        }
    }
}

namespace Bacterio.Game
{
    /// <summary>
    ///  This is the same as GlobalContext, but for in-game stuff only
    /// </summary>
    public sealed class GameContext
    {
        public static UI.GameUIController uiController { get; private set; }
        public static GameStatus gameStatus { get; private set; }
        public static TimerController timerController { get; private set; }
        public static EffectsController effectsController { get; private set; }
        public static Input.IInputHandler inputHandler { get; private set; }

        //Time variables for synchronization
        public static double serverTimeSec { get; private set; }
        public static long serverTimeMs { get; private set; }
        public static double serverDeltaTimeSec { get; private set; }
        public static long serverDeltaTimeMs { get; private set; }

        private TimerController.Dispatcher _timerDispatcher = null;

        public GameContext(UI.GameUIController gameUiController)
        {
            //Start the timer controller
            _timerDispatcher = new TimerController.Dispatcher(GlobalContext.localTimeMs);

            timerController = _timerDispatcher._controller;
            uiController = gameUiController;
            inputHandler = new Input.PcInputHandler();
            gameStatus = new GameStatus();
            effectsController = new EffectsController(timerController);

            //Update time
            serverTimeSec = Mirror.NetworkTime.time;
            serverTimeMs = (long)(serverTimeSec * Constants.ONE_SECOND_MS);
            serverDeltaTimeSec = 0;
            serverDeltaTimeMs = 0;
        }

        public void Update()
        {
            _timerDispatcher.Dispatch(GlobalContext.localTimeMs);
            inputHandler.RunOnce();

            //Update the server time
            //get new times
            var newServerTimeSec = Mirror.NetworkTime.time;
            var newServerTimeMs = (long)(serverTimeSec * Constants.ONE_SECOND_MS);

            //calculate delta times
            serverDeltaTimeSec = newServerTimeSec - serverTimeSec;
            serverDeltaTimeMs = newServerTimeMs - serverTimeMs;

            //Update times
            serverTimeSec = newServerTimeSec;
            serverTimeMs = newServerTimeMs;
        }

        public void ClearTimers()
        {
            _timerDispatcher.Clear();
        }

        public void Dispose()
        {
            uiController.Dispose();
            effectsController.Dispose();
            _timerDispatcher.Clear();

            uiController = null;
            gameStatus = null;
            timerController = null;
            effectsController = null;
            _timerDispatcher = null;
        }
    }

}
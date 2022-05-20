using RO.Common;
using RO.Media;
using UnityEngine;

public class GlobalLoop : MonoBehaviour
{
    [SerializeField]
    private RO.UI.EventSystem _uiEventSystem = default;
    private CursorAnimator.Updater _cursorUpdater;

    // Start is called before the first frame update
    void Awake()
    {
        _cursorUpdater = new CursorAnimator.Updater();
        Globals.Time = Time.time;
    }

    void Update()
    {
        Globals.Time += Time.deltaTime; //This is the clock that will be used everywhere to get current time
        Globals.TimeSinceLevelLoad = Time.timeSinceLevelLoad;

        // This will tell us if we need to frame skip. Since it's a heavy calculation, do it here so we only calc once per loop
        Globals.FrameIncrement = Mathf.RoundToInt((Time.deltaTime * 1000) / (Constants.FPS_TIME_INTERVAL * 1000));

        _cursorUpdater.UpdateCursorAnimation();
        _uiEventSystem.Process();
    }
}

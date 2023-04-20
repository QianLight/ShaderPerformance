#if UNITY_EDITOR
using CFEngine;
using CFUtilPoolLib;
using UnityEditor;
using UnityEngine;
using UnityEngine.CFUI;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[RequireComponent(typeof(PlayableDirector))]
[ExecuteInEditMode]
public class TimelineTool : MonoBehaviour,
    INotificationReceiver
{
    private bool backward = false;
    private PlayableDirector dir;
    private GUIStyle style = new GUIStyle();
    private Rect rect = new Rect(20, 20, 150, 40);
    public bool showGUI = true;

    void Init()
    {
        if (dir == null)
        {
            dir = GetComponent<PlayableDirector>();
            RTimeline.singleton.Start(EngineContext.instance);
        }
    }

    void Awake()
    {
        if (!EngineContext.IsRunning)
        {
            Initial();
            RTimeline.InitEditor();
        }
    }

    void Start()
    {
        if (!EngineContext.IsRunning)
        {
            Init();
            style.normal.textColor = Color.red;
            style.fontSize = 18;
            dir.timeUpdateMode = DirectorUpdateMode.GameTime;
            dir.extrapolationMode = DirectorWrapMode.Hold;
            Application.targetFrameRate = EnvironmentExtra.frameRate.Value;
            var env = GetComponent<EnvironmentExtra>();
            if (env) env.forceIgnore = true;
            Selection.activeGameObject = this.gameObject;
        }
    }

    public static void Initial()
    {
        ExternalHelp.Interface = RTimeline.singleton;       
        TimelineConfig.SetEditorMode();
    }
    public static void AddTimelineTool()
    {
        if (EngineContext.director != null && EngineContext.director.playableAsset != null)
        {
            var asset = EngineContext.director.playableAsset.name;
            bool isOrignal = asset.StartsWith("Orignal_");
            if (!isOrignal)
            {
                TimelineTool tet = EngineContext.director.gameObject.GetComponent<TimelineTool>();
                if (tet == null)
                {
                    EngineContext.director.gameObject.AddComponent<TimelineTool>();
                }
            }
        }
    }


    private void Update()
    {
        if (!EngineContext.IsRunning)
        {
            Init();
            if (dir != null)
            {
                if (EngineContext.director == null && !Application.isPlaying)
                {
                    EngineContext.director = dir;
                }
                // AddTimelineTool ();
                XTimerMgr.singleton.Update(Time.deltaTime);
                if (Input.GetKeyUp(KeyCode.Space))
                {
                    dir.Stop();
                    dir.time = 0d;
                    dir.Play();
                    //DirectorHelper.PostBindTrack ();
                }
                if (Input.GetKeyUp(KeyCode.F2))
                {
                    CameraPlayableBehaviour.TrigerShaking();
                }
                if (Input.GetKeyUp(KeyCode.F3))
                {
                    dir.Pause();
                    backward = true;
                }
                if (Input.GetKeyUp(KeyCode.F4))
                {
                    backward = false;
                    dir.Play();
                }
                if (backward)
                {
                    dir.time = dir.time - Time.deltaTime;
                    dir.Evaluate();
                }
            }
        }

    }

    private void OnGUI()
    {
        if (showGUI && dir)
        {
            GUI.Label(rect, "frame: " + (dir.time * 30).ToString("f0"), style);
        }
    }

    public void OnNotify(Playable origin, INotification notification, object context)
    {
        RTimeline.singleton.OnNotify(origin, notification, context);
    }

}

#endif
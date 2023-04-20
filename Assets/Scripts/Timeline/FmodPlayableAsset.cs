using CFEngine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
#if UNITY_EDITOR
using System.IO;
#endif

#if UNITY_EDITOR
[SerializeField, ExecuteInEditMode]
#endif
public class FmodPlayableAsset : DirectBasePlayable<FmodPlayableBehaviour>, ITimelineClipAsset
{
    public FacialExpressionCurve facialCurve;
    [HideInInspector] public string clip;

    [HideInInspector] public string curvePath;
    [HideInInspector] public string curvePath1;

    [HideInInspector] public int curveTransID;

    [HideInInspector] public bool isExecuted = false;

    [System.NonSerialized]
    public TrackAsset m_trackAsset;

    [System.NonSerialized]
    public TimelineClip m_timelineClip;

    public bool m_isPause = false;
    public bool m_isRecord = false;

    public int windowSize;
    public float amplitudeThreshold;
    public float facialSpeed;

    public ClipCaps clipCaps { get { return ClipCaps.None; } }

    public override void Load(CFBinaryReader reader)
    {
        base.Load(reader);
        clip = DirectorHelper.singleton.ReadStr(reader);
    }

    public override void Reset()
    {
        base.Reset();
        clip = null;
        facialCurve = null;
        curvePath = null;
        curvePath1 = null;
        isExecuted = false;
        m_isRecord = false;
    }

#if UNITY_EDITOR
    public static void SaveAsset(BinaryWriter bw, PlayableAsset asset, bool presave)
    {
        FmodPlayableAsset fpa = asset as FmodPlayableAsset;
        DirectorHelper.SaveStringIndex(bw, fpa.clip, presave);
    }
#endif

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        //if (TimelineGlobalConfig.Instance.m_keyValues == null)
        //{
        //    TimelineGlobalConfig.Instance.ReadConfig();
        //}
        //windowSize = int.Parse(TimelineGlobalConfig.Instance.m_keyValues["LipsyncWindowSize"]);
        //amplitudeThreshold = float.Parse(TimelineGlobalConfig.Instance.m_keyValues["LipsyncThreshold"]);
        //facialSpeed = float.Parse(TimelineGlobalConfig.Instance.m_keyValues["LipsyncMoveTowardsSpeed"]);

        var behaviour = GetBehavior();
        behaviour.asset = this;
        return ScriptPlayable<FmodPlayableBehaviour>.Create(graph, behaviour);
    }
}
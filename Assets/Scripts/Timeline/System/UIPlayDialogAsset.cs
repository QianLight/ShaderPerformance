using CFEngine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
#if UNITY_EDITOR
using System.IO;
[System.Serializable]
#endif

public class UIPlayDialogAsset : DirectBasePlayable<UIPlayDialogBehaviour>, ITimelineClipAsset
{

#if UNITY_EDITOR
    [Header("标题")]
#endif
    public string title;

#if UNITY_EDITOR
    [Header("内容")]
#endif
    public string content;

    [System.NonSerialized]
    public TimelineClip m_clip;

    public ClipCaps clipCaps { get { return ClipCaps.None; } }

    public override void Reset()
    {
        base.Reset();
        content = null;
    }
}

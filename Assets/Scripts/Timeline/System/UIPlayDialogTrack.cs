using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

#if UNITY_EDITOR
using System.Collections.Generic;
using CFEngine;
#endif

[TrackClipType(typeof(UIPlayDialogAsset))]
#if UNITY_EDITOR
[TrackColor(0.66f, 0.24f, 0.44f)]
[CSDiscriptor("玩法介绍")]
#endif
public class UIPlayDialogTrack : TrackAsset
{

    private GameObject canvas;
    private Camera uiCamera;


#if UNITY_EDITOR

    protected override bool CheckCondition()
    {
        canvas = GameObject.Find("Canvas");
        if (canvas)
        {
            uiCamera = canvas.GetComponentInChildren<Camera>();
            return canvas != null;
        }
        return false;
    }
    public override byte GetTrackType()
    {
        return RTimeline.TrackType_Dialog;
    }

#endif

    protected override void OnCreateClip(TimelineClip clip)
    {
        base.OnCreateClip(clip);
        clip.displayName = "PlayDialog";
    }

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        foreach (var c in GetClips())
        {
            UIPlayDialogAsset ca = (UIPlayDialogAsset)(c.asset);
            ca.m_clip = c;
            ca.m_clip.displayName = ca.title;
        }

        return base.CreateTrackMixer(graph, go, inputCount);
    }
}

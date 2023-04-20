using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;


[TrackClipType(typeof(UIDialogAsset))]
#if UNITY_EDITOR
[TrackColor(0.66f, 0.24f, 0.44f)]
[CSDiscriptor("对话")]
#endif
public class UIDialogTrack : TrackAsset
#if UNITY_EDITOR 
    , IDisplayTrack
#endif
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

    public void GUIDisplayName()
    {
        var clips = GetClips();
        foreach (var clip in clips)
        {
            if (clip.asset != null)
            {
                UIDialogAsset asset = clip.asset as UIDialogAsset;
                clip.displayName = asset.content;
            }
        }
    }
#endif

    protected override void OnCreateClip(TimelineClip clip)
    {
        base.OnCreateClip(clip);
        clip.displayName = "Dialog";
    }

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        foreach (var c in GetClips())
        {
            UIDialogAsset ca = (UIDialogAsset)(c.asset);
            ca.m_clip = c;
        }

        return base.CreateTrackMixer(graph, go, inputCount);
    }
}

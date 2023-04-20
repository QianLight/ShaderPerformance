using UnityEngine;
using UnityEngine.Timeline;


[TrackClipType(typeof(UISubtitleAsset))]
#if UNITY_EDITOR
[TrackColor(0.66f, 0.24f, 0.44f)]
[CSDiscriptor("字幕")]
#endif
public class UISubtitleTrack : TrackAsset
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
        return RTimeline.TrackType_Subtitle;
    }

    protected override void OnCreateClip(TimelineClip clip)
    {
        base.OnCreateClip(clip);
        clip.displayName = "Subtitle";
    }

    public void GUIDisplayName()
    {
        var clips = GetClips();
        foreach (var clip in clips)
        {
            if (clip.asset != null)
            {
                UISubtitleAsset asset = clip.asset as UISubtitleAsset;
                clip.displayName = asset.subTitle;
            }
        }
    }
#endif
}

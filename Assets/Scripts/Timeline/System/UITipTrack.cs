using UnityEngine.Timeline;


[TrackColor(0.66f, 0.54f, 0.34f)]
[TrackClipType(typeof(UITipAsset))]
#if UNITY_EDITOR
[CSDiscriptor("提示")]
#endif
public class UITipTrack : TrackAsset
#if UNITY_EDITOR
    , IDisplayTrack
#endif
{
#if UNITY_EDITOR
    public override byte GetTrackType()
    {
        return RTimeline.TrackType_Tip;
    }

    protected override void OnCreateClip(TimelineClip clip)
    {
        base.OnCreateClip(clip);
        clip.displayName = clip != null ? (clip.asset as UITipAsset).uiText : "tip";
    }

    public void GUIDisplayName()
    {
        var clips = GetClips();
        foreach (var clip in clips)
        {
            if (clip.asset != null)
            {
                UITipAsset asset = clip.asset as UITipAsset;
                clip.displayName = asset.uiText;
            }
        }
    }
#endif
}

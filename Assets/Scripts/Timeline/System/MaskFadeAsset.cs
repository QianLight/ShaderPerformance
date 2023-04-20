using UnityEngine.Timeline;


[System.Serializable]
public class MaskFadeAsset : DirectBasePlayable<MaskFadeBehaviour>, ITimelineClipAsset
{
    
    public string clip;
    
    public ClipCaps clipCaps { get { return ClipCaps.None; } }
}

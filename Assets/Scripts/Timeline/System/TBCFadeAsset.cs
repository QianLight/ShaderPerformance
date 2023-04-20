using UnityEngine.Timeline;


[System.Serializable]
public class TBCFadeAsset : DirectBasePlayable<TBCFadeBehaviour>, ITimelineClipAsset
{

    public string clip;
    
    public ClipCaps clipCaps { get { return ClipCaps.None; } }
}

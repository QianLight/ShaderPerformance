using UnityEngine.Timeline;


public abstract class XTrack
{
    public DirectorTrackAsset track = null;


    public virtual void Reset()
    {
        track = null;
    }

    public abstract void LoadRefAsset();


    public abstract void UnloadRefAsset();


}
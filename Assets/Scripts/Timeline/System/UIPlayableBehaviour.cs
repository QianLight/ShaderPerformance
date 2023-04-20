using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class UIPlayerBehaviour : DirectBaseBehaviour
{
    private UIPlayerAsset UIAsset
    {
        get { return asset as UIPlayerAsset; }
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        var upa = UIAsset;
        if (upa != null)
        {
            TimelineUI.singleton.Show(UIAsset);
        }
        TimelineUI.singleton.CleanBG();
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        TimelineUI.singleton.Clean();
        if (Application.isPlaying)
        {
            TimelineUI.singleton.CleanBG();
        }
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        var upa = UIAsset;
        if (upa != null && upa.clip != null)
        {
            if (!TimelineUI.singleton.isShow) OnBehaviourPlay(playable, info);
            GameObject go = TimelineUI.singleton.GetUIRoot();
            UpdateUIClip(playable, upa.clip, go);
        }
    }

}
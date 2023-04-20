using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class UIBossBehaviour : DirectBaseBehaviour
{
    private UIBossShowAsset UIAsset
    {
        get { return asset as UIBossShowAsset; }
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (UIAsset != null)
        {
            TimelineBoss.singleton.Show(UIAsset);
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        TimelineBoss.singleton.Clean();
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        if (UIAsset != null && UIAsset.clip != null)
        {
            if (!TimelineBoss.singleton.isShow) OnBehaviourPlay(playable, info);
            GameObject go = TimelineBoss.singleton.GetUIRoot();
            UpdateUIClip(playable, UIAsset.clip, go);
        }
    }
}

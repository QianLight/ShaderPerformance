using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class UITipBehaviour : DirectBaseBehaviour
{
    private UITipAsset UIAsset
    {
        get { return asset as UITipAsset; }
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        var upa = UIAsset;
        if (upa != null)
        {
            TimelineTip.singleton.Show(upa);
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (asset != null)
            TimelineTip.singleton.Clean();
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        if (UIAsset != null && UIAsset.clip != null)
        {
            float len = UIAsset.clip.length;
            double scale = len / playable.GetDuration();
            double time = playable.GetTime();
            GameObject go = TimelineUI.singleton.GetUIRoot();
            UIAsset.clip.SampleAnimation(go, (float)(time * scale));
        }
    }
}

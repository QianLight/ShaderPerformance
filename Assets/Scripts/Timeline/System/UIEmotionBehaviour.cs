using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CFUtilPoolLib;
using CFEngine;

public class UIEmotionBehaviour : DirectBaseBehaviour
{

    private AnimationClip clip;

    private AssetHandler ah;
    private UIEmotionAsset Asset
    {
        get { return asset as UIEmotionAsset; }
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (Asset != null)
        {
            TimelineEmotionUI.singleton.Show(Asset);
            LoadMgr.singleton.Destroy(ref ah);
            if (!string.IsNullOrEmpty(Asset.clip))
            {
                EngineUtility.LoadAnim(Asset.clip, ref ah);
                clip = ah.obj as AnimationClip;
            }
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        TimelineEmotionUI.singleton.Clean();
        if (clip)
        {
            clip = null;
        }
        LoadMgr.singleton.Destroy(ref ah);
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        if (clip != null)
        {
            GameObject go = TimelineEmotionUI.singleton.GetUIRoot();
            UpdateUIClip(playable, clip, go);
        }
        float t = (float)playable.GetTime();
        TimelineEmotionUI.singleton.Update(t);
    }
}

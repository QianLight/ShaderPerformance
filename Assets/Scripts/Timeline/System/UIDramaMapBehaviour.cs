using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CFEngine;

public class UIDramaMapBehaviour : DirectBaseBehaviour
{

    private AnimationClip clip;
    private AssetHandler ah;
    private UIDramaMapAsset Asset
    {
        get { return asset as UIDramaMapAsset; }
    }


    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (Asset != null)
        {
            TimelineDramaMapUI.singleton.Show(Asset);
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
        TimelineDramaMapUI.singleton.Clean();
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
            if(!TimelineDramaMapUI.singleton.isShow) OnBehaviourPlay(playable,info);
            GameObject go = TimelineDramaMapUI.singleton.GetUIRoot();
            UpdateUIClip(playable, clip, go);
        }
    }
}

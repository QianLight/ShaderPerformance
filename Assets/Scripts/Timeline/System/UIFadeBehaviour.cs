using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CFUtilPoolLib;
using CFEngine;
public class MaskFadeBehaviour : DirectBaseBehaviour
{
    private AnimationClip clip;
    private AssetHandler ah;
    private MaskFadeAsset Asset
    {
        get { return asset as MaskFadeAsset; }
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        base.OnBehaviourPlay(playable, info);
        MaskFadeUI.singleton.Show(Asset);
        LoadMgr.singleton.Destroy(ref ah);
        if (!string.IsNullOrEmpty(Asset.clip))
        {
            EngineUtility.LoadAnim(Asset.clip, ref ah);
            clip = ah.obj as AnimationClip;
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        base.OnBehaviourPause(playable, info);
        MaskFadeUI.singleton.Clean();
        if (clip)
        {
            clip = null;
        }
        LoadMgr.singleton.Destroy(ref ah);
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        var time = (float)playable.GetTime();
        GameObject go = MaskFadeUI.singleton.GetUIRoot();
        UpdateUIClip(playable, clip, go);
    }
}


public class TBCFadeBehaviour : DirectBaseBehaviour
{
    private bool trigerPlay = false;
    private AnimationClip clip;
    private AssetHandler ah;
    private TBCFadeAsset Asset
    {
        get { return asset as TBCFadeAsset; }
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        var time = (float)playable.GetTime();
        GameObject go = TBCFadeUI.singleton.GetUIRoot();
        UpdateUIClip(playable, clip, go);
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        base.OnBehaviourPlay(playable, info);
        trigerPlay = true;
        TBCFadeUI.singleton.Show(Asset);
        if (clip == null)
        {
            EngineUtility.LoadAnim(Asset.clip, ref ah);
            clip = ah.obj as AnimationClip;
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        base.OnBehaviourPause(playable, info);
        if (trigerPlay && Application.isPlaying)
        {
            var director = RTimeline.singleton.Director;
            var graph = director.playableGraph;
            if (graph.IsValid())
            {
                RTimeline.singleton.SetSpeed(0);
                //graph.GetRootPlayable(0).SetSpeed(0);
            }
        }
        else
        {
            TBCFadeUI.singleton.Clean();
        }
        if (clip)
        {
            clip = null;
        }
        LoadMgr.singleton.Destroy(ref ah);
    }
}


public class UIFadeBehaviour : DirectBaseBehaviour
{
    //private float time1, time2;
    //private AnimationClip clip1, clip2;
    //private AssetHandler ah1, ah2;
    //private bool onlyAnimOne;

    private UIFadeAsset Asset
    {
        get { return asset as UIFadeAsset; }
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (Asset != null)
        {
            TimelineFadeUI.singleton.Show(Asset);

            //onlyAnimOne = string.IsNullOrEmpty(Asset.clip2);

            //EngineUtility.LoadAnim(Asset.clip1, ref ah1);
            //clip1 = ah1.obj as AnimationClip;
            //time1 = clip1?.length ?? 0;

            //if (!onlyAnimOne)
            //{
            //    EngineUtility.LoadAnim(Asset.clip2, ref ah2);
            //    clip2 = ah1.obj as AnimationClip;
            //    time2 = (float)playable.GetDuration() - clip2?.length ?? 0;
            //}
            //else
            //{
            //    time2 = (float)playable.GetDuration();
            //}
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        TimelineFadeUI.singleton.Clean();
        //LoadMgr.singleton.Destroy(ref ah1);
        //LoadMgr.singleton.Destroy(ref ah2);
        //if (clip1)
        //{
        //    clip1 = null;
        //}
        //if (clip2)
        //{
        //    clip2 = null;
        //}
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        //var time = (float)playable.GetTime();

        //if (onlyAnimOne)
        //{
        //    GameObject go = TimelineFadeUI.singleton.GetUIRoot();
        //    UpdateUIClip(playable, clip1, go);
        //}
        //else
        //{
        //    if (time < time1)
        //    {
        //        SampleFadein(time);
        //    }
        //    if (time > time2)
        //    {
        //        SampleFadeout(time - time2);
        //    }
        //}
    }

    //private void SampleFadein(float time)
    //{
    //    if (clip1)
    //    {
    //        GameObject go = TimelineFadeUI.singleton.GetUIRoot();
    //        clip1.SampleAnimation(go, time);
    //    }
    //}

    //private void SampleFadeout(float time)
    //{
    //    if (clip2)
    //    {
    //        GameObject go = TimelineFadeUI.singleton.GetUIRoot();
    //        clip2.SampleAnimation(go, time);
    //    }
    //}
}

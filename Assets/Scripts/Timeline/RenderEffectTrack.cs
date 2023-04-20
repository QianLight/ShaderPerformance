using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using CFEngine;
#endif

[TrackColor(0.86f, 0.84f, 0.44f)]
[TrackClipType(typeof(RenderEffectAsset))]
[TrackBindingType(typeof(Animator))]
[ExcludeFromPreset]
#if UNITY_EDITOR
[CSDiscriptor("渲染效果")]
#endif

public class RenderEffectTrack : TrackAsset
#if UNITY_EDITOR
    , ITimelineAsset, IDisplayTrack
#endif
{

#if UNITY_EDITOR
    public override byte GetTrackType()
    {
        return RTimeline.TrackType_RenderEffect;
    }

    PlayableAssetType ITimelineAsset.assetType
    {
        get { return PlayableAssetType.ANIM; }
    }

    List<string> ITimelineAsset.ReferenceAssets(PlayableBinding pb)
    {
        List<string> list = CFAllocator.AllocateList<string>();
        var tack = pb.sourceObject as RenderEffectTrack;
        var clips = tack.GetClips();
        //foreach (var item in clips)
        //{
        //    RenderEffectAsset asset = item.asset as RenderEffectAsset;
        //    string path = asset.clip.name;
        //}
        return list;
    }

    public void GUIDisplayName()
    {
        var clips = GetClips();
        foreach (var clip in clips)
        {
            if (clip.asset != null)
            {
                //RenderEffectAsset asset = clip.asset as RenderEffectAsset;
                //if (asset.clip != null)
                //    clip.displayName = asset.clip.name;
            }
        }
    }
#endif

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        foreach (var c in GetClips())
        {
            RenderEffectAsset ca = (RenderEffectAsset)(c.asset);
            ca.trackAsset = this;
        }
        return base.CreateTrackMixer(graph, go, inputCount);
    }
}
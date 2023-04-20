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
[TrackClipType(typeof(CustomAnimationAsset))]
[TrackBindingType(typeof(Animator))]
[ExcludeFromPreset]
#if UNITY_EDITOR
[CSDiscriptor("默认动作")]
#endif

public class CustomAnimationTrack : TrackAsset
#if UNITY_EDITOR
    , ITimelineAsset, IDisplayTrack
#endif
{

#if UNITY_EDITOR
    public override byte GetTrackType()
    {
        return RTimeline.TrackType_CustomAnim;
    }

    PlayableAssetType ITimelineAsset.assetType
    {
        get { return PlayableAssetType.ANIM; }
    }

    List<string> ITimelineAsset.ReferenceAssets(PlayableBinding pb)
    {
        List<string> list = CFAllocator.AllocateList<string>();
        var tack = pb.sourceObject as CustomAnimationTrack;
        var clips = tack.GetClips();
        foreach (var item in clips)
        {
            CustomAnimationAsset asset = item.asset as CustomAnimationAsset;
            string path = asset.clip.name;
            //if (!string.IsNullOrEmpty(path))
            //{
            //    path = "Animation/Main_Camera/" + asset.path;
            //    list.Add(path);
            //}
        }
        return list;
    }

    public void GUIDisplayName()
    {
        var clips = GetClips();
        foreach (var clip in clips)
        {
            if (clip.asset != null)
            {
                CustomAnimationAsset asset = clip.asset as CustomAnimationAsset;
                if (asset.clip != null)
                    clip.displayName = asset.clip.name;
            }
        }
    }
#endif

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        //var binding = go.GetComponent<PlayableDirector>().GetGenericBinding(this);
        //var bindingGo = binding as GameObject;

        foreach (var c in GetClips())
        {
            CustomAnimationAsset ca = (CustomAnimationAsset)(c.asset);
            //if (bindingGo != null)
                //ca.ator = bindingGo.GetComponent<Animator>();
                ca.trackAsset = this;
        }

        return base.CreateTrackMixer(graph, go, inputCount);
    }
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CFUtilPoolLib;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using CFEngine;
#endif

namespace UnityEngine.Timeline
{
    /// <summary>
    /// A PlayableTrack is a track whose clips are custom playables.
    /// </summary>
    /// <remarks>
    /// This is a track that can contain PlayableAssets that are found in the project and do not have their own specified track type.
    /// </remarks>
    [TrackColor(0.86f, 0.84f, 0.44f)]
    [TrackClipType(typeof(ControlPartAsset))]
    [TrackBindingType(typeof(Animator))]
    [ExcludeFromPreset]
#if UNITY_EDITOR
    [CSDiscriptor("控制部件")]
#endif
    public class ControlPartTrack : TrackAsset
#if UNITY_EDITOR
    , ITimelineAsset, IDisplayTrack
#endif
    {

#if UNITY_EDITOR
        public override byte GetTrackType()
        {
            return RTimeline.TrackType_ControlPart;
        }

        PlayableAssetType ITimelineAsset.assetType
        {
            get { return PlayableAssetType.ANIM; }
        }

        public void GUIDisplayName()
        {
            var clips = GetClips();
            foreach (var clip in clips)
            {
                if (clip.asset != null)
                {
                    //BoneRotateAsset asset = clip.asset as BoneRotateAsset;
                    //if (asset.m_clip != null)
                    //    asset.m_clip.displayName = clip.displayName;
                }
            }
        }

        List<string> ITimelineAsset.ReferenceAssets(PlayableBinding pb)
        {
            List<string> list = CFAllocator.AllocateList<string>();
            var tack = pb.sourceObject as ControlPartTrack;
            var clips = tack.GetClips();
            //foreach (var item in clips)
            //{
            //    CustomPictureAsset asset = item.asset as CustomPictureAsset;
            //    //string path = asset.clip.name;
            //}
            return list;
        }
#endif

        protected override void OnCreateClip(TimelineClip clip)
        {
            if (clip.asset != null)
                clip.displayName = clip.asset.GetType().Name;
        }

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            foreach (var c in GetClips())
            {
                ControlPartAsset asset = (ControlPartAsset)(c.asset);
                asset.m_trackAsset = this;
                asset.m_clip = c;
            }
            return base.CreateTrackMixer(graph, go, inputCount);
        }
    }
}

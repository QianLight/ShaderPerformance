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
    [TrackClipType(typeof(FmodPlayableAsset))]
    [ExcludeFromPreset]
#if UNITY_EDITOR
    [CSDiscriptor("音频")]
#endif
    public class FmodPlayableTrack : TrackAsset
#if UNITY_EDITOR
    , ITimelineAsset, IDisplayTrack
#endif
    {
        public AudioChannel m_audioChannel;

#if UNITY_EDITOR
        public override byte GetTrackType()
        {
            return RTimeline.TrackType_FmodPlable;
        }

        PlayableAssetType ITimelineAsset.assetType
        {
            get { return PlayableAssetType.ANIM; }
        }

        public List<string> ReferenceAssets(PlayableBinding pb)
        {
            List<string> list = CFAllocator.AllocateList<string>();
            //var tack = pb.sourceObject as RenderEffectTrack;
            //var clips = tack.GetClips();
            //foreach (var item in clips)
            //{
            //    RenderEffectAsset asset = item.asset as RenderEffectAsset;
            //    string path = asset.clip.name;
            //}
            return list;
        }

        public void GUIDisplayName()
        {
            //throw new NotImplementedException();
        }
#endif

        /// <inheritdoc />
        protected override void OnCreateClip(TimelineClip clip)
        {
            if (clip.asset != null)
                clip.displayName = clip.asset.GetType().Name;
        }
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            foreach (var c in GetClips())
            {
                FmodPlayableAsset fmodAsset = (FmodPlayableAsset)(c.asset);
                fmodAsset.m_trackAsset = this;
                fmodAsset.m_timelineClip = c;
            }
            return base.CreateTrackMixer(graph, go, inputCount);
        }
    }
}

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

/// <summary>
/// 功能说明：BoneRotateTrack为扩展轨道，旨在旋转角色头部Bip001 Head骨骼节点，轨道资源使用BoneRotateAsset，轨道资源运行控制使用BoneRotateBehaviour。
/// 其特殊点在于，此轨道override了头部骨骼点默认动画（比如使用AnimationTrack，上面的clip会默认已经k了头部骨骼节点关键帧）而此时如果要覆盖此动画，
/// 则要在LateUpdate中执行，这是实现此轨道的核心。逻辑在LaterUpdateRotation，其中的tween动画，仿照TransformTweenMixerBehaviour。
/// </summary>
namespace UnityEngine.Timeline
{
    /// <summary>
    /// A PlayableTrack is a track whose clips are custom playables.
    /// </summary>
    /// <remarks>
    /// This is a track that can contain PlayableAssets that are found in the project and do not have their own specified track type.
    /// </remarks>
    [TrackColor(0.86f, 0.84f, 0.44f)]
    [TrackClipType(typeof(BoneRotateAsset))]
    [TrackBindingType(typeof(Animator))]
    [ExcludeFromPreset]
#if UNITY_EDITOR
    [CSDiscriptor("骨骼旋转")]
#endif
    public class BoneRotateTrack : TrackAsset
#if UNITY_EDITOR
    , ITimelineAsset, IDisplayTrack
#endif
    {

#if UNITY_EDITOR
        public override byte GetTrackType()
        {
            return RTimeline.TrackType_BoneRotate;
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
            var tack = pb.sourceObject as CustomPictureTrack;
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
                BoneRotateAsset asset = (BoneRotateAsset)(c.asset);
                asset.m_trackAsset = this;
            }
            return base.CreateTrackMixer(graph, go, inputCount);
        }
    }
}

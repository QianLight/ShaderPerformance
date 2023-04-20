using System.Collections.Generic;
using CFEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Timeline
{
    public partial class DirectorAnimationPlayableAsset : DirectPlayableAsset, ITimelineClipAsset
    {
        private AnimationClip m_Clip;
        private Vector3 m_Position = Vector3.zero;
        private Vector3 m_EulerAngles = Vector3.zero;
        private bool m_RemoveStartOffset = true; // set by animation track prior to compilation
        private bool m_ApplyFootIK = true;

        private uint flag = 0;
        private AnimationPlayableAsset.LoopMode m_Loop = AnimationPlayableAsset.LoopMode.UseSourceAsset;

        // used for legacy 'scene' mode.
        internal AppliedOffsetMode appliedOffsetMode { get; set; }
        AssetHandler animHandle;

        static ResLoadCb cb = LoadFinish;

        public static uint Flag_Override = 0x00000001;
        public AnimationClip AnimClip
        {
            get { return m_Clip; }
            set
            {
                m_Clip = value;
            }
        }

        /// <summary>
        /// Returns the duration required to play the animation clip exactly once
        /// </summary>
        public override double duration
        {
            get
            {
                if (m_Clip == null || m_Clip.empty)
                    return base.duration;

                double length = m_Clip.length;
                if (length < float.Epsilon)
                    return base.duration;

                // fixes rounding errors from using single precision for length
                if (m_Clip.frameRate > 0)
                {
                    double frames = Mathf.Round (m_Clip.length * m_Clip.frameRate);
                    length = frames / m_Clip.frameRate;
                }
                return length;
            }
        }

        public override IEnumerable<PlayableBinding> outputs
        {
            get { yield return AnimationPlayableBinding.Create (name, this); }
        }

        public override void Reset ()
        {
            base.Reset ();
            flag = 0;
            LoadMgr.singleton.Destroy (ref animHandle);
        }

        private static void LoadFinish (AssetHandler ah, LoadInstance li)
        {
            DirectorHelper.singleton.loadResCount--;
            var asset = li.loadHolder as DirectorAnimationPlayableAsset;
            if (asset != null)
            {
                asset.m_Clip = ah.obj as AnimationClip;
            }
        }

        public override void Load (CFBinaryReader reader)
        {
            reader.ReadVector (ref m_Position);
            reader.ReadVector (ref m_EulerAngles);
            m_RemoveStartOffset = reader.ReadBoolean ();
            m_ApplyFootIK = reader.ReadBoolean();
            m_Loop = (AnimationPlayableAsset.LoopMode)reader.ReadByte();
#if UNITY_EDITOR
            if (DirectorHelper.singleton.Version >= DirectorHelper.Timeline_Version_AnimOverride)
#endif
            {
                flag = reader.ReadUInt32();
            }
            if ((flag & Flag_Override) == 0)
            {
                LoadAsset<AnimationClip>(reader, 0, ResObject.ResExt_Anim, ref animHandle, this, cb);
            }
        }

        public void LoadAnim(string resPath)
        {
            Vector4Int param = new Vector4Int (directorClip.index, 0, 0, 0);
            if (!string.IsNullOrEmpty (resPath))
            {
                param.x = directorClip.index;
                LoadMgr.GetAssetHandler(ref animHandle, resPath, ResObject.ResExt_Anim);
                LoadMgr.loadContext.Init(null, null, LoadMgr.LoadForceImmediate);
                LoadMgr.singleton.LoadAsset<AnimationClip> (animHandle, ResObject.ResExt_Anim,true);
                m_Clip = animHandle.obj as AnimationClip;
            }
        }

        public override Playable CreatePlayable (PlayableGraph graph, GameObject go)
        {
            if (m_Clip == null || m_Clip.legacy)
                return Playable.Null;

            var clipPlayable = AnimationClipPlayable.Create (graph, m_Clip);
            clipPlayable.SetRemoveStartOffset (m_RemoveStartOffset);
            clipPlayable.SetApplyFootIK (m_ApplyFootIK);
            clipPlayable.SetOverrideLoopTime (m_Loop != AnimationPlayableAsset.LoopMode.UseSourceAsset);
            clipPlayable.SetLoopTime (m_Loop == AnimationPlayableAsset.LoopMode.On);

            Playable root = clipPlayable;

            if (ShouldApplyScaleRemove (appliedOffsetMode))
            {
                var removeScale = AnimationRemoveScalePlayable.Create (graph, 1);
                graph.Connect (root, 0, removeScale, 0);
                removeScale.SetInputWeight (0, 1.0f);
                root = removeScale;
            }

            if (ShouldApplyOffset (appliedOffsetMode, m_Clip))
            {
                var offsetPlayable = AnimationOffsetPlayable.Create (graph, m_Position, Quaternion.Euler (m_EulerAngles), 1);
                graph.Connect (root, 0, offsetPlayable, 0);
                offsetPlayable.SetInputWeight (0, 1.0F);
                root = offsetPlayable;
            }

            return root;
        }

        internal static bool HasRootTransforms (AnimationClip clip)
        {
            if (clip == null || clip.empty)
                return false;

            return clip.hasRootMotion || clip.hasGenericRootTransform || clip.hasMotionCurves || clip.hasRootCurves;
        }
        private static bool ShouldApplyOffset (AppliedOffsetMode mode, AnimationClip clip)
        {
            if (mode == AppliedOffsetMode.NoRootTransform || mode == AppliedOffsetMode.SceneOffsetLegacy)
                return false;

            return HasRootTransforms (clip);
        }

        private static bool ShouldApplyScaleRemove (AppliedOffsetMode mode)
        {
            return mode == AppliedOffsetMode.SceneOffsetLegacyEditor ||
                mode == AppliedOffsetMode.SceneOffsetLegacy ||
                mode == AppliedOffsetMode.TransformOffsetLegacy;
        }

        /// <summary>
        /// Returns the capabilities of TimelineClips that contain a AnimationPlayableAsset
        /// </summary>
        public ClipCaps clipCaps
        {
            get
            {
                var caps = ClipCaps.All;
                if (m_Clip == null || (m_Loop == AnimationPlayableAsset.LoopMode.Off) ||
                    (m_Loop == AnimationPlayableAsset.LoopMode.UseSourceAsset && !m_Clip.isLooping))
                    caps &= ~ClipCaps.Looping;

                // empty clips don't support clip in. This allows trim operations to simply become
                //  move operations
                if (m_Clip == null || m_Clip.empty)
                    caps &= ~ClipCaps.ClipIn;

                return caps;
            }
        }

    }
}
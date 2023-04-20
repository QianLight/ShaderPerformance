using System;
using System.Collections.Generic;
using CFEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    /// <summary>
    /// A Timeline track used for playing back animations on an Animator.
    /// </summary>
    public partial class DirectorAnimationTrack : DirectorTrackAsset, IDirectorEvaluate
    {
        public static uint Flag_RootTransform = 0x00008000;
        public static uint Flag_BindObject = 0x00010000;

        public static uint LocalFlag_Valid = 0x00000001;
        public static uint LocalFlag_HasLayerMixer = 0x00000002;
        private Vector3 m_Position = Vector3.zero;
        private Vector3 m_EulerAngles = Vector3.zero;
        private TrackOffset m_TrackOffset = TrackOffset.ApplyTransformOffsets;
        private AvatarMask m_AvatarMask = null;

        private AnimationLayerMixerPlayable layerMixer;
        private AnimationMixerPlayable poseMixer;
        private AnimationMotionXToDeltaPlayable motionXToDeltaPlayable;
        AnimationPlayableOutput animOutput;

        private uint localFlag;
        public Animator animator;

        /// <summary>
        /// The rotation offset of the entire track, expressed as a quaternion.
        /// </summary>
        public Quaternion rotation
        {
            get { return Quaternion.Euler (m_EulerAngles); }
        }

        /// <summary>
        /// Specifies the AvatarMask to be applied to all clips on the track.
        /// </summary>
        /// <remarks>
        /// Applying an AvatarMask to an animation track will allow discarding portions of the animation being applied on the track.
        /// </remarks>
        public AvatarMask avatarMask
        {
            get { return m_AvatarMask; }
        }

        // is this track compilable
        /// <inheritdoc/>
        public override IEnumerable<PlayableBinding> outputs
        {
            get { yield return AnimationPlayableBinding.Create (name, this); }
        }

        /// <summary>
        /// Specifies whether the Animation Track has clips, or is in infinite mode.
        /// </summary>
        public bool inClipMode
        {
            get { return ClipCount > 0; }
        }
        public override PlayableBinding GetPlayableBinding ()
        {
            return AnimationPlayableBinding.Create (name, this);
        }

        public override void Reset ()
        {
            playable = Playable.Null;
            playableOutput = PlayableOutput.Null;
            base.Reset ();
            // if (!layerMixer.IsNull () && layerMixer.IsValid ())
            // {
            //     layerMixer.Destroy ();
            // }
            layerMixer = AnimationLayerMixerPlayable.Null;
            // if (!poseMixer.IsNull () && poseMixer.IsValid ())
            // {
            //     poseMixer.Destroy ();
            // }
            poseMixer = AnimationMixerPlayable.Null;
            // if (!motionXToDeltaPlayable.IsNull () && motionXToDeltaPlayable.IsValid ())
            // {
            //     motionXToDeltaPlayable.Destroy ();                
            // }
            motionXToDeltaPlayable = AnimationMotionXToDeltaPlayable.Null;
            // if (!animOutput.IsOutputNull () && animOutput.IsOutputValid ())
            // {
            //     var director = DirectorHelper.GetDirector ();
            //     director.playableGraph.DestroyOutput (animOutput);
            // }
            animOutput = AnimationPlayableOutput.Null;
            localFlag = 0;
        }

        public override void Load (CFBinaryReader reader)
        {
            base.Load (reader);
            reader.ReadVector (ref m_Position);
            reader.ReadVector (ref m_EulerAngles);
            m_TrackOffset = (TrackOffset) reader.ReadByte ();
            string avatarStr = DirectorHelper.singleton.ReadStr (reader);
            //load avatar
            DirectorHelper.singleton.BindObject (DirectorHelper.TrackType_Animation, this, streamName);
        }

        Playable CompileTrackPlayable (PlayableGraph graph, DirectorAnimationTrack track,
            GameObject go, IntervalTree<RuntimeElement> tree, AppliedOffsetMode mode)
        {
            var clips = DirectorHelper.singleton.clips;
            poseMixer = AnimationMixerPlayable.Create (graph, track.ClipCount);
            int index = 0;
            for (int i = track.clipStart; i < track.clipEnd; i++, index++)
            {
                var c = clips[i];
                var asset = c.asset as PlayableAsset;
                if (asset == null)
                    continue;

                var animationAsset = asset as DirectorAnimationPlayableAsset;
                if (animationAsset != null)
                    animationAsset.appliedOffsetMode = mode;

                var source = asset.CreatePlayable (graph, go);
                if (source.IsValid ())
                {
                    var drc = DirectorRuntimeClip.GetRuntimeClip (DirectorRuntimeClip.CommonClip, c, source, poseMixer);
                    tree.Add (drc);
                    graph.Connect (source, 0, poseMixer, index);
                    poseMixer.SetInputWeight (index, 0.0f);
                }
            }

            return ApplyTrackOffset (graph, poseMixer, mode);
        }

        void CreatePlayerableGraph (PlayableGraph graph, GameObject go,
            IntervalTree<RuntimeElement> tree,
            AppliedOffsetMode mode, DirectorAnimationTrack track, int i)
        {
            var compiledTrackPlayable = CompileTrackPlayable (graph, track, go, tree, mode);
            graph.Connect (compiledTrackPlayable, 0, layerMixer, i);
            layerMixer.SetInputWeight (i, track.inClipMode ? 0 : 1);
            if (track.avatarMask != null)
            {
                layerMixer.SetLayerMaskFromAvatarMask ((uint) i, track.avatarMask);
            }

        }

        internal override Playable OnCreateClipPlayableGraph (PlayableGraph graph, GameObject go, IntervalTree<RuntimeElement> tree)
        {
            if (parentTrackIndex == -1 && trackIndex >= 0)
            {
                var tracks = DirectorHelper.singleton.tracks;
                int count = 1;
                for (int i = 0; i < DirectorHelper.singleton.trackCount; ++i)
                {
                    var track = tracks[i];
                    if ((track is DirectorAnimationTrack) && track.parentTrackIndex == trackIndex)
                    {
                        count++;
                    }
                }
                AppliedOffsetMode mode = GetOffsetMode (HasFlag (Flag_RootTransform));
                Playable mixer;
                if (count == 1)
                {
                    mixer = CompileTrackPlayable (graph, this, go, tree, mode);
                }
                else
                {
                    layerMixer = AnimationLayerMixerPlayable.Create (graph, count);

                    CreatePlayerableGraph (graph, go, tree, mode, this, 0);
                    //has child
                    for (int i = 0, index = 1; i < DirectorHelper.singleton.trackCount; ++i, ++index)
                    {
                        var track = tracks[i];
                        if (track != this ||
                            (track is DirectorAnimationTrack) &&
                            track.parentTrackIndex == trackIndex)
                        {
                            CreatePlayerableGraph (graph, go, tree, mode, track as DirectorAnimationTrack, index);
                        }
                    }
                    mixer = layerMixer;
                    localFlag |= LocalFlag_HasLayerMixer;
                }
                localFlag |= LocalFlag_Valid;
                if (!RequiresMotionXPlayable (mode))
                    return mixer;

                // // If we are animating a root transform, add the motionX to delta playable as the root node
                var motionXToDelta = AnimationMotionXToDeltaPlayable.Create (graph);
                graph.Connect (mixer, 0, motionXToDelta, 0);
                motionXToDelta.SetInputWeight (0, 1.0f);
                bool usesAbsoluteMotion = mode != AppliedOffsetMode.SceneOffset &&
                    mode != AppliedOffsetMode.SceneOffsetLegacy;
                motionXToDelta.SetAbsoluteMotion (usesAbsoluteMotion);
                return motionXToDelta;
            }
            else
            {
                return Playable.Null;
            }
        }

        bool RequiresMotionXPlayable (AppliedOffsetMode mode)
        {
            if (mode == AppliedOffsetMode.NoRootTransform)
                return false;
            if (mode == AppliedOffsetMode.SceneOffsetLegacy)
            {
                return animator != null && animator.hasRootMotion;
            }
            return true;
        }

        Playable ApplyTrackOffset (PlayableGraph graph, Playable root, AppliedOffsetMode mode)
        {
            // offsets don't apply in scene offset, or if there is no root transform (globally or on this track)
            if (mode == AppliedOffsetMode.SceneOffsetLegacy ||
                mode == AppliedOffsetMode.SceneOffset ||
                mode == AppliedOffsetMode.NoRootTransform ||
                !HasFlag (Flag_RootTransform)
            )
                return root;

            var offsetPlayable = AnimationOffsetPlayable.Create (graph, m_Position, rotation, 1);
            graph.Connect (root, 0, offsetPlayable, 0);
            offsetPlayable.SetInputWeight (0, 1);

            return offsetPlayable;
        }

        // calculate which offset mode to apply
        AppliedOffsetMode GetOffsetMode (bool animatesRootTransform)
        {
            if (!animatesRootTransform)
                return AppliedOffsetMode.NoRootTransform;

            if (m_TrackOffset == TrackOffset.ApplyTransformOffsets)
                return AppliedOffsetMode.TransformOffset;

            if (m_TrackOffset == TrackOffset.ApplySceneOffsets)
                return (Application.isPlaying) ? AppliedOffsetMode.SceneOffset : AppliedOffsetMode.SceneOffsetEditor;

            if (animator != null && animator.runtimeAnimatorController != null)
            {
                if (!Application.isPlaying)
                    return AppliedOffsetMode.SceneOffsetLegacyEditor;
                return AppliedOffsetMode.SceneOffsetLegacy;
            }

            return AppliedOffsetMode.TransformOffsetLegacy;
        }

        public void InitOutput (ref PlayableOutput playableOutput)
        {
            animOutput = (AnimationPlayableOutput) playableOutput;
            animOutput.SetWeight (0);
        }

        public void Evaluate ()
        {
            float weight = 1;
            animOutput.SetWeight (1);
            if (!poseMixer.IsNull () && poseMixer.GetOutputCount () > 0)
            {
                poseMixer.SetInputWeight (0, 1.0f);
            }

            // only write the final weight in player/playmode. In editor, we are blending to the appropriate defaults
            // the last mixer in the list is the final blend, since the list is composed post-order.
            if (Application.isPlaying)
                animOutput.SetWeight (weight);

            // if ((localFlag & LocalFlag_Valid) != 0)
            // {
            //     if ((localFlag & LocalFlag_HasLayerMixer) != 0)
            //     {

            //     }
            //     else
            //     {
            //         if (!poseMixer.IsNull () && poseMixer.GetOutputCount () > 0)
            //         {
            //             poseMixer.SetInputWeight (0, 1.0f);
            //         }

            //         animOutput.SetWeight (1);
            //     }
            // }
        }
    }
}
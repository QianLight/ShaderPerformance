using System.Collections.Generic;
using CFEngine;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    public interface IBindTransform
    {
        Transform BindTransform { get; set; }
        bool IsActive { get; set; }
    }

    public abstract partial class DirectorTrackAsset : PlayableAsset, ISharedObject, IBindTransform
    {
        public static uint Flag_HasClips = 0x00000001;
        public static uint Flag_HasNotifications = 0x00000002;
        public static uint Flag_Mute = 0x00000004;
        public static uint Flag_NeedBind = 0x00000008;
        public static uint Flag_Created = 0x01000000;
        public byte trackType = DirectorHelper.TrackType_Playable;
        private DiscreteTime m_Start;
        private DiscreteTime m_End;
        public short clipStart;
        public short clipEnd;
        // public short signalStart;
        // public short signalEnd;
        public int parentTrackIndex = -1;
        public int trackIndex = -1;
        public string streamName;

        private uint flag = 0;
        public Playable playable;
        public PlayableOutput playableOutput;

#if UNITY_EDITOR
        public TrackAsset track;
#endif
        public Transform BindTransform { get; set; }
        public bool IsActive { get; set; }
        public ICallbackData LoadCbData { get; set; }

        public sealed override double duration
        {
            get
            {
                return (double) (m_End - m_Start);
            }
        }

        public int ClipCount
        {
            get
            {
                return clipEnd - clipStart;
            }
        }

        public void SetFlag (uint f, bool add)
        {
            if (add)
            {
                flag |= f;
            }
            else
            {
                flag &= ~(f);
            }
        }

        public bool HasFlag (uint f)
        {
            return (flag & f) != 0;
        }

        public virtual void Reset ()
        {
            DirectorHelper.singleton.UnBindObject (this, LoadCbData);
            if (!playable.IsNull () && playable.IsValid ())
            {
                // if (trackType != DirectorHelper.TrackType_Mark)
                //     playable.Destroy ();
            }
            playable = Playable.Null;
            if (!playableOutput.IsOutputNull () && playableOutput.IsOutputValid ())
            {
                // var director = DirectorHelper.GetDirector ();
                // director.playableGraph.DestroyOutput (playableOutput);
            }
            playableOutput = PlayableOutput.Null;
            streamName = null;
            BindTransform = null;
            LoadCbData = null;
            parentTrackIndex = -1;
            trackIndex = -1;
            trackType = DirectorHelper.TrackType_Playable;
            flag = 0;
        }

        public virtual void Load (CFBinaryReader reader)
        {
            flag = reader.ReadUInt32 ();
            m_Start = (DiscreteTime) reader.ReadSingle ();
            m_End = (DiscreteTime) reader.ReadSingle ();
            clipStart = reader.ReadInt16 ();
            clipEnd = reader.ReadInt16 ();
            var directorHelper = DirectorHelper.singleton;
            streamName = directorHelper.ReadStr (reader);
            if (HasFlag (Flag_NeedBind))
            {
                directorHelper.BindObject (trackType, this, streamName);
            }
        }

        internal void CreatePlayableGraph (ref PlayableGraph graph, GameObject go,
            IntervalTree<RuntimeElement> tree)
        {
            playable = Playable.Null;
            if (HasFlag (Flag_HasClips))
            {
                playable = OnCreateClipPlayableGraph (graph, go, tree);
            }

            if (!playable.IsValid ())
            {
                DebugLog.AddErrorLog ("Track has no notifications and returns an invalid mixer Playable");
                playable = Playable.Create (graph);
            }
        }

        internal virtual Playable OnCreateClipPlayableGraph (PlayableGraph graph, GameObject go,
            IntervalTree<RuntimeElement> tree)
        {
            var clips = DirectorHelper.singleton.clips;
            var blend = CreateTrackMixer (graph, go, ClipCount);
            for (int i = clipStart, index = 0; i < clipEnd; ++i, ++index)
            {
                var srcClip = clips[i];
                var asset = srcClip.asset as DirectPlayableAsset;
                if (asset != null)
                    asset.track = this;
                var runtimePlayable = CreatePlayable (graph, go, srcClip);
                if (runtimePlayable.IsValid ())
                {
                    runtimePlayable.SetDuration (srcClip.duration);
                    var drc = DirectorRuntimeClip.GetRuntimeClip (DirectorRuntimeClip.CommonClip, srcClip, runtimePlayable, blend);
                    tree.Add (drc);
                    graph.Connect (runtimePlayable, 0, blend, index);
                    blend.SetInputWeight (index, 0.0f);
                }
            }

            return blend;
        }

        public virtual Playable CreateTrackMixer (PlayableGraph graph, GameObject go, int inputCount)
        {
            return Playable.Create (graph, inputCount);
        }

        protected virtual Playable CreatePlayable (PlayableGraph graph, GameObject gameObject, DirectorClip clip)
        {
            var asset = clip.asset as IPlayableAsset;
            if (asset != null)
            {
                var handle = asset.CreatePlayable (graph, gameObject);
                if (handle.IsValid ())
                {
                    // handle.SetAnimatedProperties (clip.curves);
                    // handle.SetSpeed (clip.timeScale);
                }
                return handle;
            }
            return Playable.Null;
        }

        Playable CreateNotificationsPlayable (PlayableGraph graph, ref Playable mixerPlayable, GameObject go, Playable timelinePlayable)
        {
            var notificationPlayable = NotificationUtilities.CreateNotificationsPlayable (graph, null, go);
            if (notificationPlayable.IsValid ())
            {
                notificationPlayable.GetBehaviour ().timeSource = timelinePlayable;
                if (mixerPlayable.IsValid ())
                {
                    notificationPlayable.SetInputCount (1);
                    graph.Connect (mixerPlayable, 0, notificationPlayable, 0);
                    notificationPlayable.SetInputWeight (mixerPlayable, 1);
                }
            }

            return notificationPlayable;
        }

        public virtual PlayableBinding GetPlayableBinding ()
        {
            return ScriptPlayableBinding.Create (name, this, typeof (GameObject));
        }

        public override IEnumerable<PlayableBinding> outputs
        {
            get
            {
                yield return ScriptPlayableBinding.Create (name, this, typeof (GameObject));
            }
        }

        /// <summary>
        /// Overrides PlayableAsset.CreatePlayable(). Not used in Timeline.
        /// </summary>
        public sealed override Playable CreatePlayable (PlayableGraph graph, GameObject go)
        {
            return Playable.Null;
        }
    }
}
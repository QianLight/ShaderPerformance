using CFEngine;
using UnityEngine;
using UnityEngine.Playables;
#if UNITY_EDITOR
using System;
#endif

namespace UnityEngine.Timeline
{
    public partial class DirectorActivationTrack : DirectorTrackAsset
    {
        public ActivationTrack.PostPlaybackState postPlaybackState = ActivationTrack.PostPlaybackState.LeaveAsIs;
        public string subTransformName;
        private DirectorActivationPlayableBehviour behaviour;
        private ScriptPlayable<DirectorActivationPlayableBehviour> mixer;
        public override void Reset ()
        {
            postPlaybackState = ActivationTrack.PostPlaybackState.LeaveAsIs;
            if (behaviour != null)
            {
                behaviour.Reset ();
            }
            base.Reset();
        }

        public override void Load (CFBinaryReader reader)
        {
            base.Load (reader);
            postPlaybackState = (ActivationTrack.PostPlaybackState) reader.ReadByte ();
            DirectorHelper.singleton.BindObject (trackType, this, streamName);
        }

        public override Playable CreateTrackMixer (PlayableGraph graph, GameObject go, int inputCount)
        {
            if (behaviour == null)
            {
                mixer = ScriptPlayable<DirectorActivationPlayableBehviour>.Create (graph, inputCount);
                behaviour = mixer.GetBehaviour ();
                behaviour.track = this;
            }
            return mixer;
        }
    }
}
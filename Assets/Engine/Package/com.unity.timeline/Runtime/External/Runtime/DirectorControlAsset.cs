#if UNITY_EDITOR
using System.ComponentModel;
#endif
using UnityEngine.Playables;
using CFEngine;
namespace UnityEngine.Timeline
{
    public partial class DirectorControlAsset : DirectBasePlayable<DirectorControlBehaviour>, ITimelineClipAsset
    {
        public SFX sfx;
        public ActivationControlPlayable.PostPlaybackState postPlayback = ActivationControlPlayable.PostPlaybackState.Revert;
        public ClipCaps clipCaps { get { return ClipCaps.None; } }

    }
}
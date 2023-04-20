using UnityEngine;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    /// <summary>
    /// A Track whose clips control time-related elements on a GameObject.
    /// </summary>
    [TrackClipType (typeof (ControlPlayableAsset), false)]
	[ExcludeFromPreset]
#if UNITY_EDITOR
    [CSDiscriptor("³¡¾°ÌØÐ§")]
#endif
    public partial class ControlTrack : TrackAsset
    {
        public override bool CanCompileClips ()
        {
            return !muted && base.CanCompileClips ();
        }
    }
}
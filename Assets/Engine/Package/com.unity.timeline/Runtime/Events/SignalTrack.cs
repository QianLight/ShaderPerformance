using System;

namespace UnityEngine.Timeline
{
    /// <summary>
    /// Use this track to emit signals to a bound SignalReceiver.
    /// </summary>
    /// <remarks>
    /// This track cannot contain clips.
    /// </remarks>
    /// <seealso cref="UnityEngine.Timeline.SignalEmitter"/>
    /// <seealso cref="UnityEngine.Timeline.SignalReceiver"/>
    /// <seealso cref="UnityEngine.Timeline.SignalAsset"/>
    [Serializable]
    [TrackBindingType(typeof(SignalReceiver))]
    [TrackColor(0.25f, 0.25f, 0.25f)]
    [ExcludeFromPreset]
#if UNITY_EDITOR
    [CSDiscriptor("����,��Marker���")]
#endif
    public class SignalTrack : MarkerTrack {}
}

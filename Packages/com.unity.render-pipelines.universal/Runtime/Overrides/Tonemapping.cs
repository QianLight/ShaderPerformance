using System;

namespace UnityEngine.Rendering.Universal
{
    public enum TonemappingMode
    {
        None,
        Neutral, // Neutral tonemapper
        ACES,    // ACES Filmic reference tonemapper (custom approximation)
        Custom,
    }

    [Serializable, VolumeComponentMenu("Post-processing/Tonemapping")]
    public sealed class Tonemapping : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Select a tonemapping algorithm to use for the color grading process.")]
        public TonemappingModeParameter mode = new TonemappingModeParameter(TonemappingMode.None);
        
        public ClampedFloatParameter toneCurveToeStrength = new ClampedFloatParameter(0, 0f, 2f);
        public ClampedFloatParameter toneCurveToeLength = new ClampedFloatParameter(1f, 0f, 3f);
        public ClampedFloatParameter toneCurveShoulderStrength = new ClampedFloatParameter(0f, 0f, 1f);
        public ClampedFloatParameter toneCurveShoulderLength = new ClampedFloatParameter(0.5f, 0, 10);
        public ClampedFloatParameter toneCurveShoulderAngle = new ClampedFloatParameter(0, 0f, 1f);
        public ClampedFloatParameter toneCurveGamma = new ClampedFloatParameter(0.5f, 0f, 1f);

        public bool IsActive() => mode.value != TonemappingMode.None;

        public bool IsTileCompatible() => true;
    }

    [Serializable]
    public sealed class TonemappingModeParameter : VolumeParameter<TonemappingMode> { public TonemappingModeParameter(TonemappingMode value, bool overrideState = false) : base(value, overrideState) { } }
}

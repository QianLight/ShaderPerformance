using System;

namespace UnityEngine.Rendering.Universal
{

    [Serializable]
    public sealed class BloomModeParameter : VolumeParameter<BloomMode> { public BloomModeParameter(BloomMode value, bool overrideState = false) : base(value, overrideState) { } }
    
    public enum BloomMode
    {
        Default,
        Minus,
        Legacy,
    }
    
    [Serializable, VolumeComponentMenu("Post-processing/Bloom")]
    public sealed class Bloom : VolumeComponent, IPostProcessComponent
    {
        public BloomModeParameter mode = new BloomModeParameter(BloomMode.Default);
        
        [Tooltip("Filters out pixels under this level of brightness. Value is in gamma-space.")]
        public MinFloatParameter threshold = new MinFloatParameter(0.9f, 0f);

        [Tooltip("Strength of the bloom filter.")]
        public MinFloatParameter intensity = new MinFloatParameter(0f, 0f);

        [Tooltip("Changes the extent of veiling effects.")]
        public ClampedFloatParameter scatter = new ClampedFloatParameter(0.7f, 0f, 1f);

        [Tooltip("Clamps pixels to control the bloom amount.")]
        public MinFloatParameter clamp = new MinFloatParameter(65472f, 0f);

        [Tooltip("Global tint of the bloom filter.")]
        public ColorParameter tint = new ColorParameter(Color.white, false, false, true);

        [Tooltip("Use bicubic sampling instead of bilinear sampling for the upsampling passes. This is slightly more expensive but helps getting smoother visuals.")]
        public BoolParameter highQualityFiltering = new BoolParameter(false);

        [Tooltip("The number of final iterations to skip in the effect processing sequence.")]
        public ClampedIntParameter skipIterations = new ClampedIntParameter(1, 0, 16);

        [Tooltip("Dirtiness texture to add smudges or dust to the bloom effect.")]
        public TextureParameter dirtTexture = new TextureParameter(null);

        [Tooltip("Amount of dirtiness.")]
        public MinFloatParameter dirtIntensity = new MinFloatParameter(0f, 0f);
        
        [Tooltip("Filters out pixels under this level of brightness. Value is in gamma-space.")]
        public MinFloatParameter directThreshold = new MinFloatParameter(0.9f, 0f);

        [Tooltip("Hue Shift平衡")] public ClampedFloatParameter balance = new ClampedFloatParameter(0.27f, 0f, 1f);

        public static readonly Vector4 blend0 = new Vector4 (0.24f, 0.24f, 0.28f, 0.24f);
        public static readonly Vector4 blend1 = new Vector4(0.24f, 0.24f, 0.00f, 0f);

        public bool IsActive() => intensity.value > 0f;

        public bool IsTileCompatible() => false;

        public static class Pass
        {
            public const int DownSample = 0;
            public const int DownSampleAndFilter = 1;
            public const int Blur0 = 2;
            public const int FastDownSample = 3;
            public const int Blur1 = 4;
            public const int Blur2 = 5;
            public const int Blur3 = 6;
            public const int UpSampleH = 7;
            public const int UpSampleM = 8;
            public const int UpSampleL = 9;
        }
    }
}

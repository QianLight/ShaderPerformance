using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityEngine.Rendering.Universal
{
    public enum NeighborhoodType
    {
        MinMax3x3,
        MinMax3x3Rounded,
        MinMax4TapVarying,
    };
    public enum DepthDilation
    {
        One,
        Tap5,
        Tap9
    };
    [System.Serializable]
    public sealed class TemporaAASetting
    {
        [SerializeField]
        public bool MixAA = false;
        [Range(0.0f, 2.0f)]
        public float JitterPoint = 1.0f;
        [Range(0.0f, 1.0f)]
        public float AABBSize = 0.75f;
        [SerializeField]
        public Pattern Pattern = Pattern.Halton_2_3_X16;
        [SerializeField]
        public NeighborhoodType Neighborhood = NeighborhoodType.MinMax3x3Rounded;
        [SerializeField]
        public DepthDilation Dilation = DepthDilation.One;
        [SerializeField]
        public bool UnJitterColorSamples = false;
        [SerializeField]
        public bool OptimizationsClipAABB = true;
        [SerializeField]
        public bool UseYCoOCg = false;
        [SerializeField]
        public bool UseClipping = true;
        [SerializeField]
        public bool UseMotionBlur = true;
        [SerializeField]
        public bool UseFlickering= true;

        [Range(0.0f, 1.0f)] 
        public float FeedbackMin = 0.2f;
        [Range(0.0f, 1.0f)] 
        public float FeedbackMax = 0.95f;
        [Range(0.0f, 1.0f)]
        public float FeedbackSpeed = 0.9f;
        [Range(0.5f, 2.0f)]
        public float Sharpness = 1.5f;

        [Range(0.0f, 1.0f)]
        public float MotionBlurStrength = 0.3f;
        public Vector2 MotionFallOff = new Vector2(15, 25);
    }

}
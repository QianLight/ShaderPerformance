using System;

namespace UnityEngine.Rendering.Universal
{
    [Serializable, VolumeComponentMenu("Post-processing/ScreenSpacePlannarReflection")]
    public class ScreenSpacePlannarReflection :  VolumeComponent, IPostProcessComponent
    {
        public BoolParameter m_isActive = new BoolParameter(false);
        [Tooltip("水位高度")]
        [InspectorName("水位高度")]
        public FloatParameter m_waterLevel = new FloatParameter(1f);

        [Tooltip("RT分辨率系数(在不影响画面情况下要尽可能小)")]
        [InspectorName("RT分辨率系数(在不影响画面情况下要尽可能小)")]
        public ClampedFloatParameter m_scale = new ClampedFloatParameter(1f, 0f, 1f);
        
        public FloatParameter m_StretchInensity = new FloatParameter(4.5f);
        public FloatParameter m_StretchThreshold = new FloatParameter(0.2f);
        
        public bool IsActive()
        {
            return  m_isActive.value;
        }

        public bool IsTileCompatible() => false;
    }
}

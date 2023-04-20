using System;

namespace UnityEngine.Rendering.Universal
{
    [Serializable, VolumeComponentMenu("Post-processing/Color Adjustments")]
    public sealed class ColorAdjustments : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Adjusts the overall exposure of the scene in EV100. This is applied after HDR effect and right before tonemapping so it won't affect previous effects in the chain.")]
        public FloatParameter postExposure = new FloatParameter(0f);

        [Tooltip("场景曝光度.")]
        public FloatParameter sceneExposure = new FloatParameter(0f);

        [Tooltip("Expands or shrinks the overall range of tonal values.")]
        public ClampedFloatParameter contrast = new ClampedFloatParameter(0f, -100f, 100f);

        [Tooltip("Tint the render by multiplying a color.")]
        public ColorParameter colorFilter = new ColorParameter(Color.white, true, false, true);

        [Tooltip("Shift the hue of all colors.")]
        public ClampedFloatParameter hueShift = new ClampedFloatParameter(0f, -180f, 180f);

        [Tooltip("Pushes the intensity of all colors.")]
        public ClampedFloatParameter saturation = new ClampedFloatParameter(0f, -100f, 100f);
        
        [Tooltip("RGB SpecularColor; A SpecularIntensity")]
        public ColorParameter sceneSpecularColor = new ColorParameter(Color.white, false, true, true);

        private readonly int _sceneColorAdjustmentParamsID = Shader.PropertyToID("_SceneColorAdjustmentParams");
        private readonly int _sceneSpecularColorParamsID = Shader.PropertyToID("_sceneSpecularColorParams");

        public bool IsActive()
        {
            return postExposure.value != 0f
                || contrast.value != 0f
                || colorFilter != Color.white
                || hueShift != 0f
                || saturation != 0f
                || sceneExposure != 0f
                || sceneSpecularColor.overrideState;
        }

        public bool IsTileCompatible() => true;

        public override void OnOverrideFinish()
        {
            base.OnOverrideFinish();
            float sceneExposure = Mathf.Pow(2, this.sceneExposure.value);
            Vector4 sceneColorAdjustmentParams = new Vector4(sceneExposure, 0, 0, 0);
            Shader.SetGlobalVector(_sceneColorAdjustmentParamsID, sceneColorAdjustmentParams);

            if (sceneSpecularColor.overrideState)
            {
                Shader.SetGlobalColor(_sceneSpecularColorParamsID, sceneSpecularColor.value);
            }
            else
            {
                Shader.SetGlobalColor(_sceneSpecularColorParamsID, Color.white);
            }
            
        }
    }
}

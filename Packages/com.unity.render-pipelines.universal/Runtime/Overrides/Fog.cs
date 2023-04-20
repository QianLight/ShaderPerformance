using System;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering.Universal
{
    [Serializable]
    public struct FogData
    {
        public float start;
        public float end;
        public float intensityMin;
        public float intensityMax;
        public float fallOff;
    }

    [Serializable]
    public sealed class FogParameter : VolumeParameter<FogData>
    {
        public string title;

        
        public FogParameter(string title, float start = 0, float end = 50, float intensityMin = 0,
            float intensityMax = 1,
            float fallOff = 1)
        {
            this.title = title;
            var val = value;
            val.start = start;
            val.end = end;
            val.intensityMin = intensityMin;
            val.intensityMax = intensityMax;
            val.fallOff = fallOff;
            value = val;
        }
    }

    [Serializable, VolumeComponentMenu("Post-processing/Fog")]
    public sealed class Fog : VolumeComponent
    {
        #region Parameters

        public BoolParameter fogEnable = new BoolParameter(false);
        public MinFloatParameter fogIntensity = new MinFloatParameter(1f, 0f);
        public ColorParameter startColor = new ColorParameter(Color.white, true, true, false);
        public ColorParameter endColor = new ColorParameter(Color.white, true, true, false);
        // public ColorParameter bottomColor = new ColorParameter(Color.white, true, true, false);
        public ColorParameter topColor = new ColorParameter(Color.white, true, true, false);
        public ClampedFloatParameter shaftOffset = new ClampedFloatParameter(1f, 0f, 0.999f);
        #region Base

        public BoolParameter baseFogEnable = new BoolParameter(false);
        public FogParameter baseDistance = new FogParameter("距离雾效");
        public FogParameter baseHeight = new FogParameter("高度雾效");

        #endregion

        #region Noise

        public BoolParameter noiseEnable = new BoolParameter(false);
        public TextureParameter noise3d = new TextureParameter(null);
        public FogParameter noiseDistance = new FogParameter("噪声距离雾效");
        public FogParameter noiseHeight = new FogParameter("噪声高度雾效");
        public FloatParameter noiseScale = new FloatParameter(0.01f);

        [FormerlySerializedAs("windDirection")]
        public Vector3Parameter noiseSpeed = new Vector3Parameter(Vector3.zero);

        public ClampedFloatParameter noiseDensity = new ClampedFloatParameter(0.5f, 0f, 1f);

        #endregion

        #region Scatter

        public BoolParameter scatterEnable = new BoolParameter(false);
        // public ColorParameter scatterColor = new ColorParameter(Color.white, true, true, false);
        public MinFloatParameter scatterScale = new MinFloatParameter(1f, 0f, true);

        #endregion

        #endregion

        public bool IsActive() => true;

        public bool IsTileCompatible() => true;

        public enum AzureScatteringMode
        {
            Automatic,
            CustomColor
        }

        [Serializable]
        public sealed class TonemappingModeParameter : VolumeParameter<AzureScatteringMode>
        {
            public TonemappingModeParameter(AzureScatteringMode value, bool overrideState = false) : base(value,
                overrideState)
            {
            }
        }

        public TonemappingModeParameter scatteringMode = new TonemappingModeParameter(AzureScatteringMode.Automatic);

        public override void OnOverrideFinish()
        {
            base.OnOverrideFinish();
            ApplyFogParams();
        }

        private void ApplyFogParams()
        {
            Vector4 starts = new Vector4(
                baseDistance.value.start,
                baseHeight.value.end,
                noiseDistance.value.start,
                noiseHeight.value.end
            );

            Vector4 ends = new Vector4(
                baseDistance.value.end,
                baseHeight.value.start,
                noiseDistance.value.end,
                noiseHeight.value.start
            );

            Vector4 intensityMin = new Vector4(
                baseDistance.value.intensityMin,
                baseHeight.value.intensityMin,
                noiseDistance.value.intensityMin,
                noiseHeight.value.intensityMin
            );

            Vector4 intensityMax = new Vector4(
                baseDistance.value.intensityMax,
                baseHeight.value.intensityMax,
                noiseDistance.value.intensityMax,
                noiseHeight.value.intensityMax
            );

            Vector4 fallOff = new Vector4(
                Mathf.Log(baseDistance.value.fallOff + 1),
                Mathf.Log(baseHeight.value.fallOff + 1),
                noiseDistance.value.fallOff,
                noiseHeight.value.fallOff
            );

            Vector4 noiseScaleOffset = new Vector4(
                noiseSpeed.value.x * Time.time,
                noiseSpeed.value.y * Time.time,
                noiseSpeed.value.z * Time.time,
                noiseScale.value
            );

            bool fogEnable = this.fogEnable.value && (this.baseFogEnable.value || noiseEnable.value || scatterEnable.value);
            float fogIntensity = fogEnable ? this.fogIntensity.value : 0;
            float scatterIntensity = this.fogEnable.value && scatterEnable.value && scatterScale != null && scatterScale.value != 0 ? scatterScale.value : 0;
            float noiseDensity = noiseEnable.value && this.fogEnable.value && this.noiseDensity.value > 0 ? this.noiseDensity.value * 2 : 0;
            float baseFogEnable = this.baseFogEnable.value && baseDistance.value.intensityMax * baseHeight.value.intensityMax > 0 ? 1 : 0;
            Vector4 miscParams = new Vector4(scatterIntensity, fogIntensity, noiseDensity, baseFogEnable);

            Shader.SetGlobalTexture(ShaderPropertyId.noise3DTex, noise3d.value);
            Shader.SetGlobalVector(ShaderPropertyId.fogStartParams, starts);
            Shader.SetGlobalVector(ShaderPropertyId.fogEndParams, ends);
            Shader.SetGlobalVector(ShaderPropertyId.fogIntensityMin, intensityMin);
            Shader.SetGlobalVector(ShaderPropertyId.fogIntensityMax, intensityMax);
            Shader.SetGlobalVector(ShaderPropertyId.fogFalloffParams, fallOff);
            Shader.SetGlobalVector(ShaderPropertyId.fogNoiseScaleOffset, noiseScaleOffset);
            Shader.SetGlobalColor(ShaderPropertyId.fogStartColor, this.startColor.value);
            Shader.SetGlobalColor(ShaderPropertyId.fogEndColor, this.endColor.value);
            // Shader.SetGlobalColor(ShaderPropertyId.fogBottomColor, this.bottomColor.value);
            Shader.SetGlobalFloat(ShaderPropertyId.fogShaftOffset, 1-shaftOffset.value);
            Shader.SetGlobalColor(ShaderPropertyId.fogTopColor, topColor.value);
            Shader.SetGlobalVector(ShaderPropertyId.fogMiscParams, miscParams);
            // Shader.SetGlobalColor(ShaderPropertyId.fogScatterColor, this.scatterColor.value);
        }
    }
}
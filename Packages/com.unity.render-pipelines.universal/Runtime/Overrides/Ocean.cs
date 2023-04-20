using System;

namespace UnityEngine.Rendering.Universal
{    

    [Serializable, VolumeComponentMenu("Post-processing/Ocean")]
    public class Ocean : VolumeComponent
    { 
        public FloatParameter waterLevel = new FloatParameter(1.3f);
        public FloatParameter period = new FloatParameter(1.5f);
        public FloatParameter intensity = new FloatParameter(0.25f);
        public FloatParameter speed = new FloatParameter(1.5f);

        public ColorParameter OceanColor = new ColorParameter(new Color(0.2f, 0.5f, 0.8f, 1f), true, true, true);
        private static class Uniforms
        {
            public static readonly int _WaterDisturbParam = Shader.PropertyToID("_WaterDisturbParam");  
            
            public static readonly int _OceanColor = Shader.PropertyToID("_OceanColor"); 
        }

        public override void OnOverrideFinish()
        {
            base.OnOverrideFinish();
            Update();
        }

        public void Update()
        {
            Shader.SetGlobalVector(Uniforms._WaterDisturbParam, new Vector4(waterLevel.value, period.value, intensity.value, speed.value));
            Shader.SetGlobalColor(Uniforms._OceanColor, OceanColor.value);
        }
    }
}

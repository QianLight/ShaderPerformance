using System.Collections.Generic;
using UnityEngine;
namespace CFEngine.Editor
{
    [System.Serializable]
    public class LightmapConfig
    {
        public string name = "Default";

        public bool useDoubleBakePipeline = true;
        public bool blurShadowmask = false;
        public int blurIterationCount = 3;
        public float blurOffset = 0.05f;
        public UnityEngine.Rendering.AmbientMode ambientType = UnityEngine.Rendering.AmbientMode.Flat;
        public Color ambientLight = Color.gray;
        public Color ambientSkyColor = Color.gray;
        public Color ambientEquatorColor = Color.gray;
        public Color ambientGroundColor = Color.gray;
        public float skyIntensity = 1;
        public Material skyMaterial;
        public bool mis = false;
        public int directSampleCount = 32;
        public int indirectSampleCount = 1000;
        public int environmentSampleCount = 512;
        public int bounces = 4;

        public float bakeResolution = 17;
        public int padding = 2;
        public int maxAtlasSize = 1024;

        public bool enableAmbientOcclusion = true;

        public float aoMaxDistance = 0.3f;

        public float aoExponentIndirect = 0.3f;
        public float aoExponentDirect = 0.3f;
        public float indirectOutputScale = 2;
        public float bounceBoost = 1;
        public bool enableShadowMask = true;
    }

    [System.Serializable]
    public class LightmapConfigData : ScriptableObject
    {
        public List<LightmapConfig> configs = new List<LightmapConfig>();
    }
}
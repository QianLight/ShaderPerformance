#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine
{
    [System.Serializable]
    public class LightData
    {
        public int objID = -1;
        public int lightType = -1;
        public int cullingMask;
        public LightmapBakeType lightmapBakeType;
        public LightRenderMode renderMode;
        public Color color;
        public float intensity;
        public float bounceIntensity;
        public float colorTemperature;
        public LightShadows shadows;
        public float shadowStrength;
        public UnityEngine.Rendering.LightShadowResolution shadowResolution;
        public float shadowBias;
        public float shadowNormalBias;
        public float shadowNearPlane;
        public int shadowCustomResolution;
        public float spotAngle;
        public float range;
        public Vector2 areaSize;
        public LightBakingOutput bakingOutput;
        public Texture cookie;
        public float cookieSize;

        public void Save (Light light)
        {
            lightType = (int) light.type;
            cullingMask = light.cullingMask;
            lightmapBakeType = light.lightmapBakeType;
            renderMode = light.renderMode;
            color = light.color;
            intensity = light.intensity;
            bounceIntensity = light.bounceIntensity;
            colorTemperature = light.colorTemperature;

            shadows = light.shadows;
            shadowStrength = light.shadowStrength;
            shadowResolution = light.shadowResolution;
            shadowBias = light.shadowBias;
            shadowNormalBias = light.shadowNormalBias;
            shadowNearPlane = light.shadowNearPlane;
            shadowCustomResolution = light.shadowCustomResolution;

            spotAngle = light.spotAngle;
            range = light.range;
        }

        public void Load (GameObject go)
        {
            Light light = go.AddComponent<Light> ();
            light.type = (LightType) lightType;
            light.cullingMask = cullingMask;
            light.lightmapBakeType = lightmapBakeType;
            light.renderMode = renderMode;
            light.color = color;
            light.intensity = intensity;
            light.bounceIntensity = bounceIntensity;
            light.colorTemperature = colorTemperature;

            light.shadows = shadows;
            light.shadowStrength = shadowStrength;
            light.shadowResolution = shadowResolution;
            light.shadowBias = shadowBias;
            light.shadowNormalBias = shadowNormalBias;
            light.shadowNearPlane = shadowNearPlane;
            light.shadowCustomResolution = shadowCustomResolution;

            light.spotAngle = spotAngle;
            light.range = range;
        }
    }
}
#endif
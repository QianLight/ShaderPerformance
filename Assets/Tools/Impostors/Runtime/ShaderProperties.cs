﻿using UnityEngine;

namespace Impostors
{
    internal static class ShaderProperties
    {
        public static readonly int _ImpostorsWorldSpaceCameraPosition = Shader.PropertyToID("_ImpostorsWorldSpaceCameraPosition");
        public static readonly int _WorldSpaceLightPos0 = Shader.PropertyToID("_WorldSpaceLightPos0");
        public static readonly int _LightColor0 = Shader.PropertyToID("_LightColor0");
        public static readonly int _MainLightColor = Shader.PropertyToID("_MainLightColor"); //URP
        public static readonly int _ImpostorsDebugColor = Shader.PropertyToID("_ImpostorsDebugColor");
        public static readonly int _ImpostorsTimeProvider = Shader.PropertyToID("_ImpostorsTimeProvider");
        public static readonly int _ImpostorsNoiseTexture = Shader.PropertyToID("_ImpostorsNoiseTexture");
        public static readonly int _ImpostorsNoiseTextureResolution = Shader.PropertyToID("_ImpostorsNoiseTextureResolution");
        public static readonly int _ImpostorsCutout = Shader.PropertyToID("_ImpostorsCutout");
        public static readonly int _ImpostorsMinAngleToStopLookAt = Shader.PropertyToID("_ImpostorsMinAngleToStopLookAt");
        public static readonly int _ProjectionParams = Shader.PropertyToID("_ProjectionParams");
        public static readonly int unity_LightmapST = Shader.PropertyToID("unity_LightmapST");
        public static readonly int unity_Lightmap = Shader.PropertyToID("unity_Lightmap");
        public static readonly int unity_LightmapInd = Shader.PropertyToID("unity_LightmapInd");
        public static readonly int unity_ShadowMask = Shader.PropertyToID("unity_ShadowMask");
        public static readonly int _WorldSpaceCameraPos = Shader.PropertyToID("_WorldSpaceCameraPos");
        public static readonly int _FogToggle = Shader.PropertyToID("_FogToggle");
        public static readonly int _ImpostorAlpha = Shader.PropertyToID("_ImpostorAlpha");
        public static readonly int _ZWriteImpostor = Shader.PropertyToID("_ZWriteImpostor");
        public static readonly int _ZTestImpostor = Shader.PropertyToID("_ZTestImpostor");
        public static readonly int _ImpostorEnable = Shader.PropertyToID("_ImpostorEnable");
    }
}
Shader "URP/Scene/SFXUber"
{
    Properties
    {
        //[HideInInspector] _WorkflowMode("WorkflowMode", Float) = 1.0
        [Header(Base Setting)]
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
		[Toggle] _IgnoreAlpha("忽略Alpha通道", float) = 0

        [Toggle] _AlphaTest("Alpha Test", Float) = 0.0
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        [XToggle] _NeedImpostor("Need Impostor", Float) = 0.0

        [XToggle] _Emission("Emission", Float) = 0.0
        [HDR]_EmissionColor("Emission Color", Color) = (0,0,0)

        [NoScaleOffset]_BumpMap("Normal Map", 2D) = "bump" {}

        _RoughnessMin("Roughness Min", Range(0.0, 1.0)) = 0.0
        _RoughnessMax("Roughness Max", Range(0.0, 1.0)) = 1.0
        [XToggle] _Metallic("Metallic", Float) = 0.0
        _MetallicMin("Metallic Min", Range(0.0, 1.0)) = 0.0
        _MetallicMax("Metallic Max", Range(0.0, 1.0)) = 1.0
        _BumpZ("_BumpZ",Range(0.0, 1.0))=0
        _BumpW("_BumpW",Range(0.0, 1.0)) = 0
        [HDR]_ReflectionLow("ReflectionLow", Color) = (0.03,0.04,0.05,1)
        [HDR]_ReflectionHigh("ReflectionHigh", Color) = (0.08,0.10,0.15,1)
        [XToggle] _Occlusion("Occlusion", Float) = 0.0
        _OcclusionScale("Occlusion Strength", Range(0.0, 1.0)) = 1.0

        [HideInInspector]_DitherTransparency("Dither Transparency", Range(0,1)) = 1.0
        [Header(Scale)]
        _BaseMapScale("BaseMap Scale(0, 5)", Range(0, 5)) = 1
        _BumpScale("Bump Scale", Range(0, 2)) = 1.0
        _SpecularScale("SpecularScale(0, 5)", Range(0, 5)) = 1
        _IBLScale("IBLScale(0, 5)", Range(0, 5)) = 1
        [Toggle(_IBLMASK)] _IBLMASK("_IBLMASK", Float) = 0.0
        [Toggle(_BAKEGIOFF)] _BAKEGIOFF("BakeGI_OFF", Float) = 0.0

        [Header(Offset)]
        _DarkEnviromentRefOffset("Dark EnviromentRefOffset", Range(-1.0, 1.0)) = 0.0
        _DarkMetallicIBLOffset("DarkMetallic IBLOffset", Range(-1, 1)) = 0

        [Header(Environment)]
        [XToggle] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [XToggle] _EnvironmentReflections("Environment Reflections", Float) = 1.0

        [Header(Advance)]
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend", Float) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("DstBlend", Float) = 0.0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4
        [Enum(Off, 0, On, 1)]_ZWrite("ZWrite", Float) = 1.0
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull", Float) = 2.0
        [HideInInspector]_DistinguishAlpha("GlobalAlpha", Range(0,1)) = 1
        [Header(TerrainBlending)]
        [XToggle] _TerrainOn("Terrain Blening", Float) = 0
        _BlendOffset("BlendOffset", Range(0.0, 1.0)) = 0.35
        _BlendFactor("BlendFactor", Range(0.0, 5.0)) = 2.5
        [HDR]_ColorTerr("_ColorTerr",Color) = (1,1,1,1)
        
        [XToggle]_ShadowOff("Do Not Receive Shadow", int) = 0

        /////////////////////////   For vertical gradient color    //////////////////////
        [HideInInspector]_WorldPositionScale("World Position Scale", Vector) = (1,1,1,0)
        [HideInInspector]_WorldPositionOffset("World Position Offset ", Vector) = (0,0,0,0)
        [HideInInspector]_GradientScale("Gradient Scale", Range( 0.01 , 10)) = 1
        [HideInInspector][XToggle]_GradientSwitch("Gradient Switch", Float) = 0
        [HideInInspector]_GradientBegin("Gradient Begin", Color) = (1,1,1,0)
        [HideInInspector]_GradientEnd("Gradient End", Color) = (0,0,0,0)
        [HideInInspector][Enum(Multiply,0,AddMultiply,1,Lerp,2)]_BlendMode("Blend Mode", Float) = 0
        [HideInInspector]_BlendIntensity("Blend Intensity", Range( 0 , 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"
        }
        LOD 300
        Pass
        {
            Name "ForwardSFXLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            ZTest[_ZTest]
            Cull[_Cull]
            HLSLPROGRAM
            #pragma target 3.5
            #pragma multi_compile _SHADER_LEVEL_HIGH _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW _SHADER_LEVEL_VERY_LOW
            #include "../Include/ShaderLevel.hlsl"
            #if defined(_SHADER_LEVEL_VERY_HIGH)
                    #define _MAIN_LIGHT_SHADOWS
            #elif defined(_SHADER_LEVEL_HIGH) || defined(_SHADER_LEVEL_MEDIUM) || defined(_SHADER_LEVEL_LOW)
                    //如果开启Distance模式需要关闭
                    #define LIGHTMAP_SHADOW_MIXING
                    #define _MAIN_LIGHT_SHADOWS
            #else

            #endif

            #if defined(_USE_ADDITIONAL) && defined(_SHADER_LEVEL_HIGH)
                    #define _ADDITIONAL_LIGHTS
                    #define _ADDITIONAL_LIGHTS_AS_SHADOW
            #endif

            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _IBLMASK
            #pragma shader_feature_local_fragment _BAKEGIOFF
             #pragma multi_compile _ _DITHER_TRANSPARENCY

            //#pragma multi_compile _ LIGHTMAP_ON
            //#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            //#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            //#pragma multi_compile _ _ADDITIONAL_LIGHTS_AS_SHADOW
            //#pragma multi_compile _ _ADDITIONAL_LIGHTS
            #define SPOT_LIGHT_AS_SPECULAR
            #ifdef _ADDITIONAL_LIGHTS_AS_SHADOW
                #define _ADDITIONAL_LIGHT_SHADOWS
            #endif

            #define _FORCE_SHADOWMASK
            #define SHADOWS_SHADOWMASK

            #define _SHADOWS_SOFT
            #define _SHADOW_COLOR_ENABLE
            #define _BASEMAP_SCALE_ENABLE
            #define _SPECULAR_SCALE_ENABLE
            #define _IBL_SCALE_ENABLE
            #define _DARK_ENVIROMENT_REFLECT_SCALE_ENABLE
            #define _DARK_METALLIC_IBL_SCALE_ENABLE
            #define _IBLMASKFOAT 0.5
            #define _IBLMASKMAX 0.1
            #define _URP_DEBUG
            #define _GLOBAL_SPECULAR_ADJUST_ENABLE

            #if !defined(_SHADER_LEVEL_VERY_LOW) || !defined(_SHADER_LEVEL_LOW)
                #define _BRIGHT_AO_ENABLE
            #endif

            #include "SFXUberInput.hlsl"
            #include "SFXUberForwardPass.hlsl"
            #pragma vertex SFXLitPassVertex
            #pragma fragment SFXLitPassFragment
            ENDHLSL
        }
        Pass
        {
            Name "SFXShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]
            HLSLPROGRAM
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "SFXUberInput.hlsl"
            #include "../Include/OPPShadowCaster.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "SFXDepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }
            ZWrite On
            ColorMask 0
            Cull[_Cull]
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #include "SFXUberInput.hlsl"
            #include "../Include/OPPShadowCaster.hlsl"
            ENDHLSL
        }
    }
}

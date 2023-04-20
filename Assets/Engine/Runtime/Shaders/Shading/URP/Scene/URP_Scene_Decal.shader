Shader "URP/Scene/Decal"
{
    Properties
    {
        [MainTexture]_BaseTex("_BaseTex",2D) = "white"{}
        [Toggle] _AlphaTest("Alpha Test", Float) = 0.0
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [XToggle] _Occlusion("Occlusion", Float) = 0.0
        [XToggle] _Emission("Emission", Float) = 0.0
        [HDR]_EmissionColor("Emission Color", Color) = (0,0,0)
        [Header(Base Setting)]
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        _AlphaAdjust("Alpha Adjust", Range(0.0, 1.0)) = 1.0
        _BaseColorInstensity("_BaseColorInstensity", Float) = 1.0
        [NoScaleOffset]_BumpMap("Normal Map", 2D) = "bump" {}

        [Space(10)]
        _RoughnessMin("Roughness Min", Range(0.0, 1.0)) = 0.0
        _RoughnessMax("Roughness Max", Range(0.0, 1.0)) = 1.0
        [XToggle] _Metallic("Metallic", Float) = 0.0
        _MetallicMin("Metallic Min", Range(0.0, 1.0)) = 0.0
        _MetallicMax("Metallic Max", Range(0.0, 1.0)) = 1.0
        
        _BumpZ("_BumpZ",Range(0.0, 1.0))=0
        _BumpW("_BumpW",Range(0.0, 1.0)) = 0

        [Space(10)]
        [Header(Scale)]
        _BumpScale("Bump Scale", Range(0, 2)) = 1.0
        _SpecularScale("SpecularScale(0, 5)", Range(0, 5)) = 1
        _IBLScale("IBLScale(0, 5)", Range(0, 5)) = 1
        
        [HideInInspector][Toggle(_DECAL_DEBUG)] _DecalDebug ("_DecalDebug", Float) = 0
        [HideInInspector]_CullMode ("_CullMode", Float) = 0
        [HideInInspector]_DistinguishAlpha("GlobalAlpha", Range(0,1)) = 1
        [HideInInspector]_DitherTransparency("Dither Transparency", Range(0,1)) = 1.0
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
    }

    SubShader
    {
        Tags
        {
           "RenderType" = "Opaque" "Queue" = "Geometry"
        }
        
        Pass
        {
            Tags
            {
                "LightMode" = "DecalForward"
            }
            
            NAME "Decal"
            
            // 剔除角色
            Stencil
			{
				Ref[_Stencil]
				Comp equal
			}

            Cull [_CullMode]
            ZTest off
            ZWrite off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma target 4.5
            #define _SHADER_LEVEL_HIGH
            #include "../../Include/ShaderLevel.hlsl"
            // #pragma multi_compile _SHADER_LEVEL_HIGH _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW
            
            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature _SHADER_DEBUG
            #pragma shader_feature _DECAL_DEBUG
            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_local _ _DITHER_TRANSPARENCY

             //HUAWEI机型的阴影会卡死，需要特殊处理。
            // #pragma multi_compile_fragment _ _HUAWEI

            #define SPOT_LIGHT_AS_SPECULAR
          
			#pragma multi_compile _ _RAIN
            //#pragma multi_compile _ _PARKOUR
            #pragma multi_compile_instancing

            #define LIGHTMAP_ON
            #if defined(LIGHTMAP_ON)
            #define _FORCE_SHADOWMASK
            #define _SHADOWS_SOFT
            #define SHADOWS_SHADOWMASK
            #define LIGHTMAP_SHADOW_MIXING
            #else
	            #define _SHADOWS_SOFT
            #endif

            //Feature
            #define _SPECULAR_SCALE_ENABLE
            #define _IBL_SCALE_ENABLE
            #define _SHADOW_COLOR_ENABLE
            #define _URP_DEBUG
            #define _GLOBAL_SPECULAR_ADJUST_ENABLE

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            #include "../Include/URP_Scene_Decal_ForwardPass.hlsl"
            ENDHLSL
        }
    }
    
    CustomEditor "Engine.Editor.ShaderGUI.DecalShaderGUI"
}
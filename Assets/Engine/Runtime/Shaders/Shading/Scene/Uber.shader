Shader "URP/Scene/Uber"
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
        [Toggle(_IBLMASK)] _IBLMASK("_IBLMASK(卡牌)", Float) = 0.0
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

        [XToggle]_Parkout("Parkout",Int) = 1
        [XToggle]_Ripples("_Ripples",Int) = 1
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
        /////////////////////////   For vertical gradient color   ///////////////////////

        //_ReceiveShadows("Receive Shadows", Float) = 1.0
        // Editmode props
        //[HideInInspector] _QueueOffset("Queue offset", Float) = 0.0

        // Will set "ENABLE_FANCY" shader keyword when set
        //[Toggle(ENABLE_FANCY)] _Fancy("Fancy?", Float) = 0
        //[KeywordEnum(None, Add, Multiply)] _Overlay ("Overlay mode", Float) = 0
        //[PowerSlider(3.0)] _Shininess("Shininess", Range(0.01, 1)) = 0.08
        //[Space] _Prop1("Prop1", Float) = 0
    }

    SubShader
    {
        // Universal Pipeline tag is required. If Universal render pipeline is not set in the graphics settings
        // this Subshader will fail. One can add a subshader below or fallback to Standard built-in to make this
        // material work with both Universal Render Pipeline and Builtin Unity Pipeline
        Tags
        {
            "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"
        }
        LOD 300

        //真正渲染的步骤 需要保持默认第一个pass位置 
        // ------------------------------------------------------------------
        //  Forward pass. Shades all light in a single pass. GI + emission + Fog
        Pass
        {
            // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
            // no LightMode tag are also rendered by Universal Render Pipeline
            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            ZTest[_ZTest]
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard SRP library
            // All shaders must be compiled with HLSLcc and currently only gles is not using HLSLcc by default
            //#pragma exclude_renderers gles gles3 glcore
            #pragma target 3.5

            //接入北京技术中台地形融合功能
            //为了节省内存，如果是华为垃圾手机，高级效果不走了。
            #pragma multi_compile_fragment _NO_FUNC _ENABLE_TERRAIN_BLENDING _USE_ADDITIONAL

            //画质分级
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

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON

            // 烘焙草的Lightmap用
            #pragma shader_feature_local_fragment _LIGHTMAPUNENABLE
            #pragma shader_feature_local_fragment _GROUNDCOLORUNENABLE
            #pragma shader_feature_local_fragment _SHADER_DEBUG
            #pragma shader_feature_local _GRADIENT_ON
            // -------------------------------------
            // Universal Pipeline keywords

             #pragma multi_compile _ _DITHER_TRANSPARENCY

            #pragma multi_compile _ LIGHTMAP_ON
            //#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            //#pragma multi_compile _ _MAIN_LIGHT_SHADOWS

            //#pragma multi_compile _ _ADDITIONAL_LIGHTS_AS_SHADOW
            //#pragma multi_compile _ _ADDITIONAL_LIGHTS
            #define SPOT_LIGHT_AS_SPECULAR
            #ifdef _ADDITIONAL_LIGHTS_AS_SHADOW
                #define _ADDITIONAL_LIGHT_SHADOWS
            #endif

            //#define LIGHTMAP_ON
			#pragma multi_compile _ _RAIN
            #define _FORCE_SHADOWMASK
            #define SHADOWS_SHADOWMASK
            #pragma shader_feature_local_fragment _IBLMASK
            #pragma shader_feature_local_fragment _BAKEGIOFF
            
            #define _IBLMASKFOAT 0.3
            #define _IBLMASKMAX 0.1

            #define _SHADOWS_SOFT
            #define _SHADOW_COLOR_ENABLE
            #define _BASEMAP_SCALE_ENABLE
            #define _SPECULAR_SCALE_ENABLE
            #define _IBL_SCALE_ENABLE
            #define _DARK_ENVIROMENT_REFLECT_SCALE_ENABLE
            #define _DARK_METALLIC_IBL_SCALE_ENABLE
            
            #define _URP_DEBUG
            #define _GLOBAL_SPECULAR_ADJUST_ENABLE

            #if !defined(_SHADER_LEVEL_VERY_LOW) || !defined(_SHADER_LEVEL_LOW)
                #define _BRIGHT_AO_ENABLE
            #endif

            #include "UberInput.hlsl"
            #include "../Include/ShaderDebug.hlsl"
            #include "UberForwardPass.hlsl"
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            //#pragma exclude_renderers gles gles3 glcore
            //#pragma target 4.5

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            // #pragma multi_compile_instancing
            // #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "UberInput.hlsl"
            #include "../Include/OPPShadowCaster.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            //#pragma exclude_renderers gles gles3 glcore
            #pragma target 3.5

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            // #pragma multi_compile_instancing
            // #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "UberInput.hlsl"
            #include "../Include/OPPShadowCaster.hlsl"
            ENDHLSL
        }

        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags
            {
                "LightMode" = "Meta"
            }

            Cull Off

            HLSLPROGRAM
            //#pragma exclude_renderers gles gles3 glcore
            #pragma target 3.5

            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMeta

            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            // #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            //#pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED

            #pragma shader_feature_local_fragment _SPECGLOSSMAP
            #include "UberInput.hlsl"
            #include "UberMetaPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowBake"
            Tags
            {
                "LightMode" = "ShadowBake"
            }

            ZWrite On
            ZTest LEqual

            Cull Off

            HLSLPROGRAM
            #pragma target 3.5
            #define SMART_SHADOW_DEPTH_OUTPUT
            #include "UberInput.hlsl"
            #include "../Include/SmartShadow.hlsl"
            #pragma vertex object_vert
            #pragma fragment object_frag
            ENDHLSL
        }
        
        
        Pass 
        {
            // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
            // no LightMode tag are also rendered by Universal Render Pipeline
            Name "TerrainBlending"
            Tags
            {
                "LightMode" = "TerrainBlending"
            }

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            ZTest[_ZTest]
            Cull[_Cull]

            HLSLPROGRAM
            #include "../Include/ShaderLevel.hlsl"
            #define _SHADER_LEVEL_LOW
            #define TERRAIN_BLENDING_ALBEDO
            

            #pragma target 3.5
            #include "UberInput.hlsl"
            #include "../Include/ShaderDebug.hlsl"
            #include "UberForwardPass.hlsl"
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            ENDHLSL
        }
    }

//    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    //CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.UberShader"
}

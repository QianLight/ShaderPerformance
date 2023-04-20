Shader "URP/Scene/Layer"
{
    Properties
    {
        [Header(Base)]
        [Space(10)]
        [KeywordEnum(1X, 2X, 3X, 4X)] _SPLAT("_SPLAT", int) = 0

        [Space(10)]
        [Toggle] _NO_METALLIC ("_NO_METALLIC", int) = 1
        _AOBias("AOBias(0.01, 1)", Range(0.01, 1)) = 1
        _NormalScale("NormalScale(0.1, 5)", Range(0.1, 5)) = 1
        _SpecularScale("SpecularScale(0, 5)", Range(0, 5)) = 1
        _IBLScale("IBLScale(0, 5)", Range(0, 5)) = 1
        _MetallicScale("MetallicScale(0, 1)", Range(0, 1)) = 1

        [Space(10)]
        _RoughnessMin("RoughnessMin(0, 1)", Range(0, 1)) = 0
        _RoughnessMax("RoughnessMax(0, 1)", Range(0, 1)) = 1
        _MetallicMin("MetallicMin(0, 1)", Range(0, 1)) = 0
        _MetallicMax("MetallicMax(0, 1)", Range(0, 1)) = 1

        [Space(10)]
        _BlendThreshold("BlendThreshold(0.01,1)", Range(0.01,1)) = 0.1
        _BlendScale("BlendScale(0.01,1)", Range(0.01,1)) = 0


        [Header(Blend Mode)]
        [Toggle] _BlendNormalize("BlendNormalize", int) = 0
        [Toggle] _VCMode("VCMode", int) = 0
        [NoScaleOffset]_BlendTex("RGBA BlendWeight", 2D) = "" {}

        [Header(Texture 0)]

        [NoScaleOffset]_MainTex("Splat Tex 0", 2D) = "" {}
        [NoScaleOffset]_ProcedureTex0("PBS0", 2D) = "" {}
        _PBS0Z("PBS0_Z",range(0,1)) = 0
        _PBS0W("PBS0_W",range(0,1)) = 0
        _Layer0UVST("UV 0", Vector) = (1,1,0,0)
        _Color0("Color 0", Color) = (1,1,1,1)



        [Header(Texture 1)]

        [NoScaleOffset]_MainTex1("Splat Tex 1", 2D) = "" {}
        [NoScaleOffset]_ProcedureTex1("PBS1", 2D) = "" {}
        _PBS1Z("PBS1_Z",range(0,1)) = 0
        _PBS1W("PBS1_W",range(0,1)) = 0
        _Layer1UVST("UV 1", Vector) = (1,1,0,0)
        _Color1("Color 1", Color) = (1,1,1,1)



        [Header(Texture 2)]
        [NoScaleOffset]_MainTex2("Splat Tex 2", 2D) = "" {}
        [NoScaleOffset]_ProcedureTex2("PBS2", 2D) = "" {}
        _PBS2Z("PBS2_Z",range(0,1)) = 0
        _PBS2W("PBS2_W",range(0,1)) = 0
        _Layer2UVST("UV 2", Vector) = (1,1,0,0)
        _Color2("Color 2", Color) = (1,1,1,1)

        [Header(Texture 3)]
        [NoScaleOffset]_MainTex3("Splat Tex 3", 2D) = "black" {}
        [NoScaleOffset]_ProcedureTex3("PBS3", 2D) = "bump" {}
        _PBS3Z("PBS3_Z",range(0,1)) = 0
        _PBS3W("PBS3_W",range(0,1)) = 0
        _Layer3UVST("UV 3", Vector) = (1,1,0,0)
        _Color3("Color 3", Color) = (1,1,1,1)



        [Space(10)]
        _Rot01("UV01 Rot", Vector) = (1,0,1,0)
        _Rot23("UV12 Rot", Vector) = (1,0,1,0)
        [HideInInspector] _Occlusion("Occlusion",Range(0,1)) = 0
        [HideInInspector] _Metallic("Metallic",Range(0,1)) = 0
        [HideInInspector] _Emission("Emission",Range(0,1)) = 0
        [HideInInspector] _OcclusionScale("Occlusion Strength", Range(0.0, 1.0)) = 0.0

        [HDR]_EmissionColor("Emission Color", Color) = (0,0,0)

        [HideInInspector]_DitherTransparency("Dither Transparency", Range(0,1)) = 1.0

        [Header(Properties)]

        //_Param1("SpecularScale:x Spec:y IBLScale:z Cutout:w", Vector) = (1,0.5,0,0.5)		


        _TextureIntensity("TextureIntensity", Vector) = (1,1,1,1)

        _ReceiveShadows("Receive Shadows", Float) = 1.0

        [Toggle]_Parkout("Parkout",Int) = 1
        [Toggle]_Ripples("_Ripples",Int) = 1
        // Blending state
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0

        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

        //   For vertical gradient color 
        [HideInInspector]_WorldPositionScale("World Position Scale", Vector) = (1,1,1,0)
        [HideInInspector]_WorldPositionOffset("World Position Offset ", Vector) = (0,0,0,0)
        [HideInInspector]_GradientScale("Gradient Scale", Range( 0.01 , 10)) = 1
        [HideInInspector][Toggle(_GRADIENT_ON)]_GradientSwitch("Gradient Switch", Float) = 0
        [HideInInspector]_GradientBegin("Gradient Begin", Color) = (1,1,1,0)
        [HideInInspector]_GradientEnd("Gradient End", Color) = (0,0,0,0)
        [HideInInspector][Enum(Multiply,0,AddMultiply,1,Lerp,2)]_BlendMode("Blend Mode", Int) = 0
        [HideInInspector]_BlendIntensity("Blend Intensity", Range( 0 , 1)) = 1
    }



    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry+100" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True"
        }
        LOD 300

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
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 3.5

        #pragma multi_compile _NO_FUNC _ENABLE_TERRAIN_BLENDING _USE_ADDITIONAL
        #pragma multi_compile _SHADER_LEVEL_HIGH _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW _SHADER_LEVEL_VERY_LOW
        #include "../../Include/ShaderLevel.hlsl"
        #if defined(_SHADER_LEVEL_VERY_HIGH)
                #define _MAIN_LIGHT_SHADOWS
        #elif defined(_SHADER_LEVEL_HIGH) || defined(_SHADER_LEVEL_MEDIUM) || defined(_SHADER_LEVEL_LOW)
                //�������Distanceģʽ��Ҫ�ر�
                #define LIGHTMAP_SHADOW_MIXING
                #define _MAIN_LIGHT_SHADOWS
        #else

        #endif

            #if defined(_USE_ADDITIONAL) && defined(_SHADER_LEVEL_HIGH)
                    #define _ADDITIONAL_LIGHTS
                    #define _ADDITIONAL_LIGHTS_AS_SHADOW
            #endif

            //Feature
            #define _SHADOW_COLOR_ENABLE
            #define _SPECULAR_SCALE_ENABLE
            #define _IBL_SCALE_ENABLE
            #define _HEIGHT_BLEND
            #define _BLEND_NORMAL_MASK
            #define _URP_DEBUG
            #define _GLOBAL_SPECULAR_ADJUST_ENABLE

            // Function
            #pragma shader_feature_local _VCMODE_ON
            //	#pragma shader_feature_local _SPLAT_1X _SPLAT_2X _SPLAT_3X
            #pragma shader_feature_local _SPLAT_2X _SPLAT_3X _SPLAT_4X
            // #pragma shader_feature_local _NO_METALLIC_ON
            // #pragma shader_feature_local _BLENDNORMALIZE_ON

            // -------------------------------------
            // Material Keywords

            #define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR

            //#pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            //#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            //#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            //#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            //#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            #pragma shader_feature_local_fragment _LIGHTMAPUNENABLE
            #pragma shader_feature_local_fragment _GROUNDCOLORUNENABLE
            #pragma shader_feature _SHADER_DEBUG
            // -------------------------------------
            // Universal Pipeline keywords
			#pragma multi_compile _ _RAIN
            //#pragma multi_compile_fragment _ _HUAWEI
            #pragma multi_compile_local _ _DITHER_TRANSPARENCY

            #pragma multi_compile _ LIGHTMAP_ON


            //#pragma multi_compile _ _ADDITIONAL_LIGHTS_AS_SHADOW
            //#pragma multi_compile _ _ADDITIONAL_LIGHTS
            #define SPOT_LIGHT_AS_SPECULAR
            #ifdef _ADDITIONAL_LIGHTS_AS_SHADOW
                #define _ADDITIONAL_LIGHT_SHADOWS
            #endif

            #define _SHADOWS_SOFT
            //#define LIGHTMAP_ON
            #define _FORCE_SHADOWMASK
            #define SHADOWS_SHADOWMASK

            #if !defined(_SHADER_LEVEL_VERY_LOW) || !defined(_SHADER_LEVEL_LOW)
                #define _BRIGHT_AO_ENABLE
            #endif

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "../Include/URP_SceneLayerForwardPass.hlsl"

            //Vertical Gradient Color Keyword
            #pragma shader_feature_local _GRADIENT_ON
            ENDHLSL
        }

        Pass
        {
            Name "TerrainBlending"
            Tags
            {
                "LightMode" = "TerrainBlending"
            }

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]

            HLSLPROGRAM
            //#pragma exclude_renderers gles gles3 glcore
            #pragma target 3.5


            //#pragma multi_compile _SHADER_LEVEL_HIGH _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW
            //Feature
            //#define _SHADOW_COLOR_ENABLE
            //#define _SPECULAR_SCALE_ENABLE
            //#define _IBL_SCALE_ENABLE
            #define _HEIGHT_BLEND
            #define _BLEND_NORMAL_MASK

            // Function
            #pragma shader_feature_local _VCMODE_ON
            //	#pragma shader_feature_local _SPLAT_1X _SPLAT_2X _SPLAT_3X
            #pragma shader_feature_local _SPLAT_2X _SPLAT_3X _SPLAT_4X
            // #pragma shader_feature_local _NO_METALLIC_ON
            // #pragma shader_feature_local _BLENDNORMALIZE_ON
            


            // #define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
            // #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            // #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            // #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            // #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            // //   #pragma shader_feature_local_fragment _FOGUNENABLE
            //    #pragma shader_feature_local_fragment _LIGHTMAPUNENABLE
            //       #pragma shader_feature_local_fragment _GROUNDCOLORUNENABLE
            // #if defined(LIGHTMAP_ON)
            //          #define _FORCE_SHADOWMASK
            //          #define _SHADOWS_SOFT
            // #define SHADOWS_SHADOWMASK
            // //#define LIGHTMAP_SHADOW_MIXING
            //      #else
            //          #define _SHADOWS_SOFT
            //      #endif
            #define TERRAIN_BLENDING_ALBEDO
            //#define _MRTEnable 1
            //#pragma vertex TerrainBlendingVertex
            //#pragma fragment TerrainBlendingFragment


            #pragma vertex Vertex
            #pragma fragment Fragment


            #include "../Include/URP_SceneLayerForwardPass.hlsl"
            //#include "../Include/URP_TerrainBlendingForwardPass.hlsl"
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
            #define _Cutoff 0.5

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            //#pragma multi_compile_instancing
            //#pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "../Include/URP_SceneLayerForwardPass.hlsl"
            #include "../../Scene/Urp_Layer_ShadowCaster.hlsl"
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
            //#pragma multi_compile_instancing
            //#pragma multi_compile _ DOTS_INSTANCING_ON
            #include "../Include/URP_SceneLayerForwardPass.hlsl"
            #include "../Include/URP_SceneLayerDepthOnlyPass.hlsl"
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
            #define _Cutoff 0.5

            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMeta

            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED

            #pragma shader_feature_local_fragment _SPECGLOSSMAP

            #include "../Include/URP_SceneLayerForwardPass.hlsl"
            #include "../../Scene/Urp_Layer_MetaPass.hlsl"
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
            #include "../Include/URP_SceneLayerForwardPass.hlsl"
            #include "../../Include/SmartShadow.hlsl"
            #pragma vertex object_vert
            #pragma fragment object_frag
            ENDHLSL
        }
    }
}

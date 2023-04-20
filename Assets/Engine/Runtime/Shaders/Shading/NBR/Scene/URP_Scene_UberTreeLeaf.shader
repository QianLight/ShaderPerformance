Shader "URP/Scene/UberTreeLeaf" {
    Properties {
        [NoScaleOffset]_MainTex ("Base Tex", 2D) = "white" {}
        [GAMMA]_Color0("Main Color", Color) = (0.0,0.33,0.05,1)

        _Cutoff("_Cutoff",Range(0,1))=0.5
        _Treecenter("tree center", Vector) = (0,0,0,0)
     //   _TreeCenterPosBias("_TreeCenterPosBias",Range(0,5))=2.5
     //   _TreeRimPower("_TreeRimPower",Range(1,10))=1
     //   _TreeRimIntensity("_TreeRimIntensity",Range(0,1))=1
        _TreeFresnelPower("_TreeFresnelPower",Range(1,10))=10
        _TreeFresnelIntensity("_TreeFresnelIntensity",Range(0,5))=2
    //    _DackIntensity("_DackIntensity",Range(0,2))=1
     //  _DackRange("_DackRange",Float)=1
        _InverseLightIntensity("_InverseLightIntensity",Range(-1,1))=-0.7
        _TargetLightIntensity("_TargetLightIntensity",Range(0,5))=4
        _LightFlatness("_LightFlatness",Range(0,1))=0.5
    //   _SubSurfaceGain("_SubSurfaceGain",Range(0,2))=0
        _ShadowOffset("_ShadowOffset",Range(0,1))=0.4
        _DackIntensity("_DackIntensity",Range(0,2))=0.3
        _FackLight("_FackLight",Range(0,1))=0.21
        _InsideDack("_InsideDack",Range(0,2))=1
         _InsideDackpow("_InsideDackpow",Range(1,10))=1
         _ScaleInsideDack("_ScaleInsideDack",Range(0.1,1))=1
   //     [HDR]_Color1("AmbientSky", Color) = (0.5,0.5,0.5,0)
     //   [HDR]_Color2("AmbientEquator", Color) = (0.5,0.5,0.5,0)
  //      [HDR]_Color3("AmbientGround", Color) = (0.5,0.5,0.5,0)
         [Space(3)]
        [Header(WindControl)]
         [NoScaleOffset]_AmbientWind("_AmbientWind",2D)="white"{}
        _AmbientWindDir("风方向",Vector)=(1,1,1,1)
        _AmbientTreeWindSpeed("_AmbientTreeWindSpeed",Range(1,10))=1
        _AmbientWindFrequency("_AmbientWindFrequency",Float)=1
        _Magnitude("_Magnitude",Float)=5
        _Frenquency("_Frenquency",Float)=2.39
        _ModelScaleCorrection("_ModelScaleCorrection",Float)=0.35
        _MaskRange("_MaskRange",Float)=0.06
        _OffsetCorrection("_OffsetCorrection",Vector)=(-0.5,0,0.5,1)
        _Blend("_Blend",Float)=1
        _StableRange("_StableRange",Range(-1,1))=0.5
        [HideInInspector] _DitherTransparency("_DitherTransparency", Range(0.0, 1.0)) = 1.0

        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _DebugMode("__debugMode", Float) = 0.0

     //   _DebugMode("", float) = 0
    }

    HLSLINCLUDE
    //input layout
    #define _INPUT_UV2		
    #define _VERTEX_COLOR
    //feature
    #define _MAIN_COLOR
    #define _SCENE_EFFECT	
    #define _NO_NORMAL_MAP
    #define _OUTPUT_VERTEX_COLOR
    #define _ALPHATEST_ON
    //lighting		
    #define _NO_METALLIC
    #define _NO_COMMON_EFFECT
    #define _NO_LIGHTMAP
    #define _NO_ADDLIGHT
    #define _GAMMA_CORRECT
    #define _CUSTOM_VERTEX_PARAM
    #define _CUSTOM_LOD
    #define _CUSTOM_VERTEX_OFFSET
    #define _NO_MRT
    #define _DETIAL_TREE_MOVE
    #pragma multi_compile _ _SMARTSOFTSHADOW_ON
    ENDHLSL

    SubShader {
        Tags {
            "RenderType" = "Opaque" "Queue" = "AlphaTest" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" 
        }
        ZWrite[_ZWrite]
        Cull Off
        LOD 100

        Pass {
            Name "FORWARD"
            Tags {
                "LightMode" = "UniversalForward"
            }
            Blend[_SrcBlend][_DstBlend]

            HLSLPROGRAM
            /////////////////////////////////Lod/////////////////////////////////
     

            //----------------------
            #define URP_BASE
            #define REDEFINE_URP
            #define LIGHTMAP_ON
            #define _SHADOW_COLOR_ENABLE
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            //#pragma multi_compile_fragment _ _SHADOWS_SOFT

            #pragma multi_compile_instancing

            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            //----------------------

            #ifdef _SM_4
            #pragma target 5.0
            #else//!_SM_4
				#pragma target 3.0
            #endif//_SM_4
            //render type
            #pragma shader_feature_local _ _ALPHA_BLEND _ALPHA_TEST

			#pragma multi_compile_local _ _DITHER_TRANSPARENCY

			#include "../../Include/URP_TreeLeaf_ForwardPass.hlsl"
			#pragma vertex LitPassVertex
			#pragma fragment LitPassFragment
            ENDHLSL
        }
        
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode" = "ShadowCaster"
            }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            //#pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            // -------------------------------------
            // Material Keywords
            #define _ALPHATEST_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON
			            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                        float4 _MainTex_ST;
                        float4 _Param;
                        float4 _Param0;
                        float4 _Param1;
                        float4 _Param2;
                        float4 _Param3;
                        float4 _Param4;
                        float4 _Param5;
                        float4 _Param6;
                        float4 _Color0;
                        float4 _Color1;
                        float4 _Color2;
                        float4 _Color3;
                        float4 _Color4;
                        float4 _Color5;
                        float4 _PBRParam;
                        float _Cutoff;
                        float _DebugMode;
            CBUFFER_END

            TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);
            TEXTURE2D(_ProcedureTex3);            SAMPLER(sampler_ProcedureTex3);
            TEXTURE2D(_ProcedureTex4);            SAMPLER(sampler_ProcedureTex4);

            #define _BaseMap _MainTex 
            #define _BaseMap_ST _MainTex_ST
            #define sampler_BaseMap sampler_MainTex
        //    #define _Cutoff _Param1.w
            #include "../../Include/OPPShadowCaster.hlsl"

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            ENDHLSL
        }
            
            Pass{
                Name "ShadowBake"
                Tags{"LightMode" = "ShadowBake"}

                ZWrite On
                ZTest LEqual

                Cull Off

                HLSLPROGRAM

                #pragma target 4.5
                #define SMART_SHADOW_DEPTH_OUTPUT
                
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                CBUFFER_START(UnityPerMaterial)
                            float4 _MainTex_ST;
                            float4 _Param;
                            float4 _Param0;
                            float4 _Param1;
                            float4 _Param2;
                            float4 _Param3;
                            float4 _Param4;
                            float4 _Param5;
                            float4 _Param6;
                            float4 _Color0;
                            float4 _Color1;
                            float4 _Color2;
                            float4 _Color3;
                            float4 _Color4;
                            float4 _Color5;
                            float4 _PBRParam;
                            float _DebugMode;
                CBUFFER_END

                TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);
                TEXTURE2D(_ProcedureTex3);            SAMPLER(sampler_ProcedureTex3);
                TEXTURE2D(_ProcedureTex4);            SAMPLER(sampler_ProcedureTex4);

                #define _BaseMap _MainTex 
                #define _BaseMap_ST _MainTex_ST
                #define sampler_BaseMap sampler_MainTex

                #include "../../Include/SmartShadow.hlsl"
                #pragma vertex object_vert
                #pragma fragment object_frag

                ENDHLSL
            }
            
    }
	//CustomEditor "CFEngine.Editor.PBSShaderGUI"
}
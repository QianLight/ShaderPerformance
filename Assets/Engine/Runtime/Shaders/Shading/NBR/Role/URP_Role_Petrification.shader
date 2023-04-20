Shader "URP/Role/Petrification" {
    Properties {
        // [NoScaleOffset]_MainTex ("Base Tex", 2D) = "white" {}
        _UVST0("Base Scale Offset", Vector) = (1,1,0,0)
        [NoScaleOffset]_MainTex1 ("Base Tex", 2D) = "white" {}
        [HDR]_Color0("Main Color", Color) = (1,1,1,1)

        [NoScaleOffset]_ProcedureTex0("Normal:rg Roughness:b Metallic:a", 2D) = "" {}
        _UVST1("PBR Scale Offset", Vector) = (1,1,0,0)
        _Param0("AO:x normalscale:y w:metallicMask", Vector) = (0,0,1,0)
        _Param1("IBLScale:z Cutout:w", Vector) = (0.1,0.5,0,0.5)
        // _PBRParam("x: Roughness y: Metallic", Vector) = (1,0,0,0)

        [NoScaleOffset]_ProcedureTex2 ("Ramp", 2D) = "" {}
        _Color3("ShadowColorMultiply",Color)=(1,1,1,1)

        //Dark Rim 		
        _Color4("Dark Rim Color", Color) = (0.8,0.8,0.8,1)

        _ColorOutline("Outline Color", Color) = (0,0,0,0.003)
        _ColorOutline2("Outline Color2", Color) = (0,0,0,0)
        [HideInInspector]_MatEffectOutlineParam("Outline Color", Color) = (0,0,0,0)

        _Color("Effect Color", Color) = (1,1,1,1)
        [HDR]_RimColortoon("RimColor",Color) = (1,1,1,1)
        _HeightGradientColor("Gradient Color", Color) = (0,0,0,0)

        [HideInInspector] _SrcBlend("__src", FLOAT) = 1.0
        [HideInInspector] _DstBlend("__dst", FLOAT) = 0.0
        [HideInInspector] _ZWrite("__zw", FLOAT) = 1.0
        
        
        //CBuffer Fill
    	[HideInInspector] _DebugMode("__debugMode", Float) = 0.0
    	[HideInInspector]_Color1("_Color1", Color) = (1,1,1,1)
		[HideInInspector]_Color2("_Color2", Color) = (1,1,1,1)
    	[HideInInspector]_Color5("_Color5", Color) = (1,1,1,1)
    	[HideInInspector]_Color6("_Color6", Color) = (1,1,1,1)
    	[HideInInspector]_PBRParam("_PBRParam", Vector) = (1,1,1,1)
    	[HideInInspector]_Param("_Param", Vector) = (1,1,1,1)
    	[HideInInspector]_Param2("_Param2", Vector) = (1,1,1,1)
    	[HideInInspector]_Param3("_Param3", Vector) = (1,1,1,1)
		[HideInInspector]_GrassCollisionParam("_GrassCollisionParam", Vector) = (1,1,1,1)
		[HideInInspector]_HeightGradientPreivewColor("_HeightGradientPreivewColor", Vector) = (1,1,1,1)
		[HideInInspector]_HeightGradientPreviewParam("_HeightGradientPreviewParam", Vector) = (1,1,1,1)
		[HideInInspector]_LightMapUVST("_LightMapUVST", Vector) = (1,1,1,1)
		[HideInInspector]_Param4("_Param4", Vector) = (1,1,1,1)
		[HideInInspector]_Param5("_Param5", Vector) = (1,1,1,1)
		[HideInInspector]_ProcedureTex1_TexelSize("_ProcedureTex1_TexelSize", Vector) = (1,1,1,1)
		[HideInInspector]_OutlineScale("_OutlineScale", Vector) = (1,1,1,1)
		[HideInInspector]_LaserColor("LaserColor", Color) = (0,0,0,0)
		[HideInInspector]_SSRimColor("_SSRimColor", Color) = (0,0,0,0)
		[HideInInspector]_SSRimParam0("_SSRimParam0", Color) = (0,0,0,0)
		[HideInInspector]_EdgeColor("EdgeColor",color) = (1,1,1,1)
		[HideInInspector]_BackColor("BackColor",color) = (1,1,1,1) 
		[HideInInspector]_AnisotropyParam ("AnisotropyScale:x WarpIntensity:y WarpIntensity:z WarpBendFactor:w", Vector) = (1,3,5,1.5)
        [HideInInspector]_FabricColor1 ("WarpColor:rgb, WarpRoughness:a", Color) = (1,1,1,1)
        [HideInInspector]_FabricColor2 ("WeftColor:rgb, WeftRoughness:a", Color) = (1,1,1,1)
		[HideInInspector] _LocalOffset("pos", Vector) = (0,1,0,1)
    	[HideInInspector] _UVST3("_UVST3", Vector) = (0,1,0,1)
    	[HideInInspector] _ShadowPos("ShadowPos", Vector) = (0,0,0,0)
    }

    HLSLINCLUDE

    #define ROLE_EFFECT
    //feature
    #define _MAIN_COLOR
    #define _VERTEX_GI
    //uv
    #define _UV_SCALE
    #define _INPUT_UV2
    #define _CUSTOM_UV2

    #define _SM_RIM
    half4 _RimParam;
    half4 _LightFadePram;
    #define _RimNormalMapWeight _LightFadePram.x
    #define _RimOrientLight _LightFadePram.y
    #define _RimDisFadeBegin _LightFadePram.z
    #define _RimDisFadeEnd _LightFadePram.w
    #define _RimIntensity _Param4.w
    #define _RimCutLocation _RimParam.x
    #define _RimCutSmoothness _RimParam.y
    // half4 _RimColortoon;
    #define _RoleRimColor (_RimColortoon.xyz)
    #define _RoleRimMultiply (_RimColortoon.w)
    #ifdef _SM_DARK_RIM
			#define _DarkRimCutLocation _RimParam.z
			#define _DarkRimCutSmoothness _RimParam.w
			#define _DarkRimLightColor _Color4
    #endif

    //----------------------
    #define URP_BASE
    #define REDEFINE_URP
    #define BUILTIN_SM_OFF

    ENDHLSL

    SubShader {
        Tags {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" 
        }
        LOD 100
        ZWrite[_ZWrite]

        Pass {
            Name "UniversalForward"
            Tags {
                "LightMode" = "UniversalForward"
            }
            Blend[_SrcBlend][_DstBlend]

            HLSLPROGRAM
            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _SMARTSOFTSHADOW_ON
            
            #pragma multi_compile _SHADER_LEVEL_HIGH _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW _SHADER_LEVEL_VERY_LOW
            
            #define _VERTEX_COLOR
			#define _CUSTOM_VERTEX_PARAM
            #define _ROLE_HEIGHT_GRADIENT
            #define _ROLE_ADDITIONAL_LIGHT_ON
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "../Include/Pragam_Head.hlsl"
            #ifdef _SM_4
            #pragma target 5.0
            #else//!_SM_4
					#pragma target 3.0
            #endif//_SM_4

            #include "Role_Head.hlsl"
            #include "../Include/URP_LightingHead.hlsl"
            
			inline void CustomVertex(in FVertexInput Input,inout FInterpolantsVSToPS Interpolants)
			{
				TransformHeightGradient(Input, Interpolants);
                SetSmartShadowValue(Interpolants, GetRoleSmartShadow());
			}
            
            inline void CustomPPBaseColor(in FFragData FragData, inout FMaterialData MaterialData)
            {
                FLOAT2 uv = GET_FRAG_UV;
                FLOAT4 color = SAMPLE_TEX2D(_MainTex1, uv);
                MaterialData.BaseColor = color * _MainColor;
                MaterialData.DyeColor = MaterialData.BaseColor.rgb;
            }

            #define CalcCustomBaseColor CustomPPBaseColor
            #define _CUSTOM_BASECOLOR
            #include "../Include/URP_Vertex.hlsl"
            #include "../Include/URP_Pixel.hlsl"

            //only editor use
            //#pragma shader_feature_local _ _DEBUG_APP

            //render type
            #pragma shader_feature_local _ _ALPHA_BLEND _ALPHA_TEST

            //lighting

            #pragma vertex vertForwardBase
            #pragma fragment fragForwardBase
            ENDHLSL
        }

        // UsePass "Hidden/Custom/Tools/OutLine/OUTLINE"

        //		Pass {
        //			Name "OverdrawForwardBase"
        //			Tags {
        //				"LightMode" = "OverdrawForwardBase"
        //			}
        //
        //			Blend One One
        //			ZWrite[_ZWrite]
        //			Stencil {
        //				Ref[_Stencil]
        //				Comp equal
        //			}
        //			CGPROGRAM
        //			#pragma vertex Vert
        //			#pragma fragment Frag
        //
        //			#include "UnityCG.cginc"
        //
        //			struct Attributes
        //			{
        //				float4 vertex : POSITION;
        //			};
        //
        //			struct Varyings
        //			{
        //				float4 vertex : SV_POSITION;
        //			};
        //
        //			Varyings Vert(Attributes v)
        //			{
        //				Varyings o;
        //				float4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
        //				o.vertex = mul(unity_MatrixVP, WorldPosition);
        //				return o;
        //			}
        //
        //			half4 Frag(Varyings i) : SV_Target
        //			{
        //				return half4(0.1, 0.04, 0.02, 1);
        //			}
        //			ENDCG
        //		}
    }
    CustomEditor "CFEngine.Editor.PBSShaderGUI"
}

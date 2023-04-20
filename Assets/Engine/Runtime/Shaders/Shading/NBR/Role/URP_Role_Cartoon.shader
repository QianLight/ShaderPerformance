Shader "URP/Role/Cartoon"
{
	Properties
	{
		//pbr
		[NoScaleOffset]_MainTex ("Base Tex", 2D) = "white" {}
		_Color0("Main Color", Color) = (1,1,1,1)

		[NoScaleOffset]_ProcedureTex0 ("Normal:rg Mask:ba", 2D) = "" {}
		_UVST1("PBR Scale Offset", Vector) = (1,1,0,0)
		_Param0("AO:x normalscale:y w:metallicMask", Vector) = (0,1,1,0)
		_Param1("IBLScale:z Cutout:w", Vector) = (0.1,0.5,0,0.5)
		_PBRParam("x: Roughness y: Metallic", Vector) = (0,1,0,1)
		//mask
		[NoScaleOffset]_MainTex1("r:rim mask g: emissive b: shadow ", 2D) =  "black" {}
		_Param2("x:maskEnable g:emissive b:shadowMask a:toonAoScale", Vector) = (0,0,0,0) 

		//ramp
		[NoScaleOffset]_ProcedureTex2 ("Ramp", 2D) = "" {}  
		_Color3("ShadowColorMultiply",Color)=(1,1,1,1)

		//Dark Rim 		
		_Color4("Dark Rim Color", Color) = (0.8,0.8,0.8,1)
		
		//emissive
		[HDR]_Color5("EmissionColor",Color)=(1,1,1,1)

		//outline	
		_ColorOutline("Outline Color", Color) = (0,0,0,0.003)
		[HideInInspector]_MatEffectOutlineParam("Outline Color", Color) = (0,0,0,0)
			
		_Param3("x:outlineZOffset y:VerticalGradient z:ShadowInt w:_VerticalGradientOffset", Vector) = (0,-1,0,-1) 
		//_ColorOutline2("Outline Color2", Color) = (0,0,0,0)
		
		//VerticalGradient
		_Color6("VerticalGradientColor",Color)=(1,1,1,1)
		
        [HideInInspector]_Color1("", Color) = (0,0,0,0)
		_Param("", Vector) = (0,0,0,0)
		_Color("Effect Color", Color) = (1,1,1,1)
		[HDR]_RimColortoon("RimColor",Color) = (1,1,1,1)
		_HeightGradientColor("Gradient Color", Color) = (0,0,0,0)
		_HeightGradientPreviewParam("Gradient PreviewParam", Vector) = (0,0,0,0)
		_HeightGradientPreivewColor("Gradient PreivewColor", Vector) = (0,0,0,0)
		_RootPosWS ("RootPosWS", Vector) = (0,0,0,0)
    	_FaceShadowParam ("FaceShadowParam", Vector) = (0.01,0,0,0)
    	_SceneColor("SceneColor", Vector) = (0,0,0,1)

		[HideInInspector] _SrcBlend("__src", FLOAT) = 1.0
		[HideInInspector] _DstBlend("__dst", FLOAT) = 0.0
		[HideInInspector] _ZWrite("__zw", FLOAT) = 1.0
		[HideInInspector] _Stencil ("Stencil ID", Float) = 5
		[HideInInspector] _OLZWrite ("_OutlineZWrite", Float) = 0
		// [HideInInspector]_UVST0("", Vector) = (1,1,0,0)
		
		_SSRimColor ("SSRim Color",Color) = (1,1,1,0.3)
		_SSRimParam0("x:Distance Threshold w:RimScale",Vector) = (0.1,0,0,2)
		[HideInInspector] _ShadowPos("ShadowPos", Vector) = (0,0,0,0)
		_Tessellation("Tessellation", Vector) = (1, 0, 0, 0)
	}

	HLSLINCLUDE

		#define ROLE_EFFECT
		#define UNIFORM_PCH_OFF
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		#define ROLE_SRP_BATCH

		#include "../API/CommonAPI.hlsl"

		CBUFFER_START(UnityPerMaterial)
		float4 _Color0;
		float4 _UVST1;
		float4 _Param0;
		float4 _Param1;
		float4 _PBRParam;
		float4 _Param2;
		float4 _Color3;
		float4 _Color4;
		float4 _Color5;
		float4 _ColorOutline;
		float4 _MatEffectOutlineParam;
		float4 _Param3;
		float4 _Color6;
		float4 _Param;
		float4 _Color;
		float4 _RimColortoon;
		float4 _HeightGradientColor;
		float4 _HeightGradientPreviewParam;
		float4 _HeightGradientPreivewColor;
		float4 _Color1;
		float4 _RootPosWS;
		float4 _ShadowPos;
		float4 _FaceShadowParam;
	    float4 _SceneColor;
		float4 _SSRimColor;
		float4 _SSRimParam0;
		float4 _Tessellation;
		CBUFFER_END
		#define SCREEN_SPACE_RIM_IS_IN_COMMON_CBUFFER
		#define UNITY_PERMATERIAL_DECLARED

		#define _MainColor _Color0
		TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
		TEXTURE2D(_MainTex1);SAMPLER(sampler_MainTex1);
		TEXTURE2D(_ProcedureTex3);SAMPLER(sampler_ProcedureTex3);
		TEXTURE2D(_ProcedureTex2);SAMPLER(sampler_ProcedureTex2);
		TEXTURE2D(_ProcedureTex0);SAMPLER(sampler_ProcedureTex0);

		//feature
		#define _MAIN_COLOR
		#define _UV_SCALE2	

		//lighting
		#define _VERTEX_GI
		#define _SELF_SHADOW
	    #define _CARTOON		
		#define _SM_DARK_RIM
		#define _CSM3

		#if !defined(_ADD_LIGHT) && !defined(SHADER_API_MOBILE)
			#define _ADD_LIGHT
		#endif
		#ifdef _SPHERE_DITHER_FADE
			#define _ALPHA_TEST
			#define _DITHER_TRANSPARENT
		#endif

		#define TESS_POS Position
		#define TESS_NOR TangentX
		#define TESS_TAN TangentZ
		#define TESS_OUT FMobileShadingVSToPS

		#define _SM_RIM
		float4 _RimParam;
		float4 _LightFadePram;
		#define _RimNormalMapWeight _LightFadePram.x
		#define _RimOrientLight _LightFadePram.y
		#define _RimDisFadeBegin _LightFadePram.z
		#define _RimDisFadeEnd _LightFadePram.w
		#define _RimIntensity _Param4.w
		#define _RimCutLocation _RimParam.x
		#define _RimCutSmoothness _RimParam.y
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
		#define _ROLE_VERTEX_SHADOW
		#define _CUSTOM_VERTEX_PARAM

	ENDHLSL

		  

	SubShader
	{
		Tags{"RenderType" = "Opaque"  "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" }

		Pass
		{
			//Name "NoTessForward"
			//Tags{"LightMode" = "NoTessForward" }
			Name "UniversalForward"
			Tags{"LightMode" = "UniversalForward" }
			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]
			Cull Back
            Stencil
            {
                Ref [_StencilRef_Role]
                Comp Always
                Pass Replace
            }

			HLSLPROGRAM
			// -------------------------------------
			// Universal Pipeline keywords
			// #pragma multi_compile _ _SMARTSOFTSHADOW_ON
			#define _SMARTSOFTSHADOW_ON
			#define _ROLE_ADDITIONAL_LIGHT_ON
			
			//#pragma multi_compile _ _PARKOUR
            #pragma multi_compile _SHADER_LEVEL_HIGH _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW _SHADER_LEVEL_VERY_LOW

			#define _VERTEX_COLOR
			#define _CUSTOM_EFFECT
			#define _ROLE_HEIGHT_GRADIENT
			#define ROLE_SHADER_LEVEL
			#define _SRP_DEBUG
			#define _NOIMPOSTOR

			#include "../../Include/ShaderLevel.hlsl"

			//----------------------
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "../Include/Pragam_Head.hlsl"
			#ifdef _SM_4
				#pragma target 5.0
			#else//!_SM_4
				#pragma target 3.0
			#endif//_SM_4
			
            #include "URP_Role_Cartoon.hlsl"
			
                     #pragma enable_d3d11_debug_symbols
			#pragma shader_feature_local _ _PBS_FROM_PARAM
			#pragma shader_feature_local _ _ALPHA_BLEND _ALPHA_TEST

			//lighting
			#pragma multi_compile_local _ _DITHER_TRANSPARENCY

			// #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			// #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

			#pragma shader_feature _ _TESSELLATION_ON

			#pragma shader_feature _SHADER_DEBUG

			#pragma vertex  vertForwardBase
			#pragma fragment fragForwardBase


			ENDHLSL
		}

		/*Pass
		{
			Name "TessForward"
			Tags{"LightMode" = "TessForward" }
			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]
			Cull Back
			Stencil
			{
				Ref[_StencilRef_Role]
				Comp Always
				Pass Replace
			}

			HLSLPROGRAM
		#define _SMARTSOFTSHADOW_ON

		#pragma multi_compile _SHADER_LEVEL_HIGH _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW

		#define _VERTEX_COLOR
		#define _CUSTOM_EFFECT
		#define _ROLE_HEIGHT_GRADIENT
		#define ROLE_SHADER_LEVEL
		#define _SRP_DEBUG

		#include "../../Include/ShaderLevel.hlsl"

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
		#include "../Include/Pragam_Head.hlsl"
		#ifdef _SM_4
			#pragma target 5.0
		#else
			#pragma target 3.0
		#endif


		#include "URP_Role_Cartoon.hlsl"
		#include "../Include/URP_Tessellation.hlsl"

		#pragma shader_feature_local _ _PBS_FROM_PARAM
		#pragma shader_feature_local _ _ALPHA_BLEND _ALPHA_TEST

		#pragma multi_compile_local _ _DITHER_TRANSPARENCY

		#pragma shader_feature _ _TESSELLATION_ON

		#pragma shader_feature _SHADER_DEBUG

		#pragma vertex TessVert
		#pragma fragment fragForwardBase
		#pragma hull Hull
		#pragma domain Domain

		ENDHLSL
	}*/

		Pass
		{
			Name "Outline"
			Tags{ "LightMode" = "Outline" "RenderType" = "Transparent" "Queue" = "Transparent" }
			Cull Front
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite[_OLZWrite]
//			Offset 1,[_OutlineZOffset]

			HLSLPROGRAM
				#pragma target 3.0
						   #pragma enable_d3d11_debug_symbols
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

				#include "../Tools/Outline.hlsl"
				#pragma multi_compile_local _ _DITHER_TRANSPARENCY
				#pragma shader_feature _ _TESSELLATION_ON
				//#pragma multi_compile _ _PARKOUR
				#pragma vertex vertForwardBase
				#pragma fragment fragForwardBase

			ENDHLSL
		}
	
			/*Pass
		{
			Name "TessOutline"
			Tags{ "LightMode" = "TessOutline" "RenderType" = "Transparent" "Queue" = "Transparent" }
			Cull Front
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite[_OLZWrite]

			HLSLPROGRAM
				#pragma target 3.0
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

				#include "../Tools/Outline.hlsl"
				#include "../Include/URP_Tessellation.hlsl"
				#pragma require tessellation tessHW

				#pragma multi_compile_local _ _DITHER_TRANSPARENCY
				#pragma shader_feature _ _TESSELLATION_ON
			#pragma vertex TessVert
			#pragma fragment fragForwardBase
			#pragma hull Hull
			#pragma domain Domain
		ENDHLSL
		}*/
		
		Pass
        {
            Name "PlanarShadow"
            
            Tags { "LightMode" = "PlanarShadow" }

        	ZWrite Off
        	
        	// 在UI界面中：因为没有背景alpha和没有背景颜色，需要特殊处理。
        	Blend SrcAlpha Zero
        	// 如果要用在正常战斗中，有背景颜色，得使用正片叠底：
        	//Blend DstColor Zero, SrcAlpha OneMinusSrcAlpha

            //深度稍微偏移防止阴影与地面穿插
            Offset -1 , 0
            
            Stencil
            {
                Ref [_StencilRef_PlanarShadow]
                Comp Equal
                Pass incrWrap
                Fail keep
                ZFail keep
            }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Role_PlanarShadow.hlsl"

            ENDHLSL
        }
		
		Pass
		{
			Name "ShadowCaster"
			Tags{"LightMode" = "ShadowCaster"}

			ZWrite On
			ZTest LEqual
			ColorMask 0
			Cull[_Cull]

			HLSLPROGRAM

			#pragma shader_feature_local_fragment _ALPHATEST_ON

			#pragma vertex ShadowPassVertex
			#pragma fragment ShadowPassFragment
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			//#include "Assets/Engine/Runtime/Shaders/Shading/Scene/ParkourDistort.hlsl"
			FLOAT3 _LightDirection;

			struct Attributes
			{
			    FLOAT4 positionOS   : POSITION;
			    FLOAT3 normalOS     : NORMAL;
			};

			struct Varyings
			{
			    FLOAT4 positionCS   : SV_POSITION;
			};

			FLOAT4 GetShadowPositionHClip(Attributes input)
			{
			    FLOAT3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
			    FLOAT3 normalWS = TransformObjectToWorldNormal(input.normalOS);
			    FLOAT4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

				#if UNITY_REVERSED_Z
				    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
				#else
				    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
				#endif

			    return positionCS;
			}

			Varyings ShadowPassVertex(Attributes input)
			{
			    Varyings output;
			    output.positionCS = GetShadowPositionHClip(input);
			    return output;
			}

			half4 ShadowPassFragment(Varyings input) : SV_TARGET
			{
				return 0;
			}
				
			ENDHLSL
		}

		Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

			ZWrite On
			ZTest LEqual
			ColorMask 0
			Cull Back

			HLSLPROGRAM

			#pragma multi_compile_local _ _DITHER_TRANSPARENCY
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
			// #pragma multi_compile _ _PARKOUR
			
			#include "URP_Role_DepthOnly.hlsl"

            ENDHLSL
        }
		
//		Pass
//        {
//            Name "ScreenSpaceRim"
//            
//            Tags { "LightMode" = "ScreenSpaceRim" }
//
//            ZTest Equal
//        	ZWrite Off
//            ColorMask RGB
//            Blend One One
//            
////            Stencil
////            {
////                Ref [_StencilRef_StencilShadow]
////                Comp NotEqual
////                Pass Keep
////                Fail Keep
////                ZFail Keep
////            }
//
//            HLSLPROGRAM
//            #pragma vertex ScreenRimVert
//            #pragma fragment ScreenRimFrag
//            #pragma multi_compile _ _SMARTSOFTSHADOW_ON
//
//            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 
//			#include "Role_Simple_Screen_Space_Rim.hlsl" 
//            ENDHLSL
//        }
		
		Pass
        {
            Name "FaceShadowCaster"
            
            Tags { "LightMode" = "FaceShadowCaster" "Queue"="Opaque" "RenderType"="Opaque" }

        	ZWrite Off
        	
        	ColorMask 0

            Stencil
            {
                Ref [_StencilRef_StencilShadow]
                Comp NotEqual
                Pass Replace
                Fail Keep
                ZFail Keep
            }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
			
            
            #ifdef UNIFORM_PCH_OFF
				#undef UNIFORM_PCH_OFF
            #endif

			#include "Role_StencilShadowCaster.hlsl"

            ENDHLSL
        }
			/*
		Pass
		{
			Name "OverdrawForwardBase"
			Tags{"LightMode" = "OverdrawForwardBase"}

			Blend One One
			ZWrite[_ZWrite]
			Stencil
			{
				Ref[_StencilRef_Role]
				Comp equal
			}
			HLSLPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag

			#include "../StdLib.hlsl"

			struct Attributes
			{
				FLOAT4 vertex : POSITION;
			};
			
			struct Varyings
			{
				FLOAT4 vertex : SV_POSITION;
			};
			Varyings Vert(Attributes v)
			{
				Varyings o;
				FLOAT4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = mul(unity_MatrixVP, WorldPosition);
				return o;
			}

			half4 Frag(Varyings i) : SV_Target
			{
				return half4(0.1, 0.04, 0.02, 1);
			}
			
			ENDHLSL
		}


		Pass
		{
			Name "OverdrawOutline"
			Tags{"LightMode" = "OverdrawOutline"}

			Blend One One
			Cull Front
			Offset 1,1
			Stencil
			{
				Ref[_StencilRef_Outline]
				Comp equal
			}
			HLSLPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag

			#include "../StdLib.hlsl"

			struct Attributes
			{
				FLOAT4 vertex : POSITION;
			};
			
			struct Varyings
			{
				FLOAT4 vertex : SV_POSITION;
			};
			Varyings Vert(Attributes v)
			{
				Varyings o;
				FLOAT4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = mul(unity_MatrixVP, WorldPosition);
				return o;
			}

			half4 Frag(Varyings i) : SV_Target
			{
				return half4(0.1, 0.04, 0.02, 1);
			}
			
			ENDHLSL
		}*/
		//UsePass "Hidden/Custom/Tools/OutLine/OUTLINE"	
	}
	CustomEditor "CFEngine.Editor.PBSShaderGUI"
}

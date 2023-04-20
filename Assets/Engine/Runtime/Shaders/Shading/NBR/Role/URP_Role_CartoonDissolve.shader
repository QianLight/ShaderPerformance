Shader "URP/Role/CartoonDissolve"
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

		//Dissolve
		[NoScaleOffset]_ProcedureTex3 ("NoiseTex", 2D) = "" {} 
		[NoScaleOffset]_ProcedureTex4 ("EdgeTex", 2D) = "" {} 	
		[HDR]_EdgeColor("EdgeColor",color) = (1,1,1,1)
		[HDR]_BackColor("BackColor",color) = (1,1,1,1) 
		_Param4("x:MaxDistance y:NoiseEffect z:ColScale w:EdgeWidth ", Vector) = (0,0,0,0)     
				
		_Param("", Vector) = (0,0,0,0)
		_Color("Effect Color", Color) = (1,1,1,1)
		[HDR]_RimColortoon("RimColor",Color) = (1,1,1,1)
		_HeightGradientColor("Gradient Color", Color) = (0,0,0,0)

		[HideInInspector] _LocalOffset("pos", Vector) = (0,1,0,1)

		[HideInInspector] _SrcBlend("__src", FLOAT) = 1.0
		[HideInInspector] _DstBlend("__dst", FLOAT) = 0.0
		[HideInInspector] _ZWrite("__zw", FLOAT) = 1.0
		
		//CBuffer Fill
		[HideInInspector] _DebugMode("_DebugMode", FLOAT) = 1.0
		[HideInInspector]_Color1("_Color1", Color) = (1,1,1,1)
		[HideInInspector]_Color2("_Color2", Color) = (1,1,1,1)
		[HideInInspector]_GrassCollisionParam("_GrassCollisionParam", Vector) = (1,1,1,1)
		[HideInInspector]_HeightGradientPreivewColor("_HeightGradientPreivewColor", Vector) = (1,1,1,1)
		[HideInInspector]_HeightGradientPreviewParam("_HeightGradientPreviewParam", Vector) = (1,1,1,1)
		[HideInInspector]_LightMapUVST("_LightMapUVST", Vector) = (1,1,1,1)
		[HideInInspector]_Param5("_Param5", Vector) = (1,1,1,1)
		[HideInInspector]_ProcedureTex1_TexelSize("_ProcedureTex1_TexelSize", Vector) = (1,1,1,1)
		[HideInInspector]_UVST0("_UVST0", Vector) = (1,1,1,1)
		[HideInInspector]_OutlineScale("_OutlineScale", Vector) = (1,1,1,1)
		[HideInInspector]_AnisotropyParam ("AnisotropyScale:x WarpIntensity:y WarpIntensity:z WarpBendFactor:w", Vector) = (1,3,5,1.5)
        [HideInInspector]_FabricColor1 ("WarpColor:rgb, WarpRoughness:a", Color) = (1,1,1,1)
        [HideInInspector]_FabricColor2 ("WeftColor:rgb, WeftRoughness:a", Color) = (1,1,1,1)
		[HideInInspector]_UVST3("_UVST3", Vector) = (1,1,1,1)
		[HideInInspector]_LaserColor("LaserColor", Color) = (0,0,0,0)
		[HideInInspector]_SSRimColor("_SSRimColor", Color) = (0,0,0,0)
		[HideInInspector]_SSRimParam0("_SSRimParam0", Color) = (0,0,0,0)
		[HideInInspector] _ShadowPos("ShadowPos", Vector) = (0,0,0,0)
	}
//
	HLSLINCLUDE

		#define ROLE_EFFECT
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

	SubShader
	{
		ZWrite[_ZWrite]		Tags{"RenderType" = "Opaque"  "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" }

		Cull Off
		
		Pass
		{
			Name "UniversalForward"
			Tags{ "LightMode" = "UniversalForward" }
			Blend[_SrcBlend][_DstBlend]
			
			HLSLPROGRAM
			
				// -------------------------------------
				// Universal Pipeline keywords
				#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
				//#pragma multi_compile _ _SMARTSOFTSHADOW_ON
				
			
				#define _VERTEX_COLOR
				#define _CUSTOM_EFFECT
				#define _CUSTOM_VERTEX_PARAM
				#define _ROLE_HEIGHT_GRADIENT
				#define _ROLE_ADDITIONAL_LIGHT_ON

				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
				#include "../Include/Pragam_Head.hlsl"
				//#ifdef _SM_4
				//	#pragma target 5.0
				//#else//!_SM_4
					#pragma target 3.0
				//#endif//_SM_4	

				#include "Role_Head.hlsl"
				#include "../Include/URP_LightingHead.hlsl" 

				#define _CUSTOM_VERTEX_PARAM
				inline void CustomVertex(in FVertexInput Input,inout FInterpolantsVSToPS Interpolants)
				{
					 TransformHeightGradient(Input, Interpolants);
					 SetSmartShadowValue(Interpolants, GetRoleSmartShadow());
				}
			
				#include "../Include/URP_Vertex.hlsl" 
				#define _CUSTOM_LIGHT
				#define _NoiseTex _ProcedureTex3 
				#define _EdgeTex _ProcedureTex4 
				#define _MaxDistance _Param4.x
				#define _NoiseEffect _Param4.y
				#define _ColScale _Param4.z
				#define _EdgeWidth _Param4.w
				#define _DissolveTiling _EdgeColor.w
				// FLOAT4 _EdgeColor,_BackColor;
				void CustomLighting(FFragData FragData, FMaterialData MaterialData,
					FShadowData ShadowData,FLOAT ShadowMask,
					inout REAL3 DirectDiffuse,inout REAL3 DirectSpecular DEBUG_ARGS)
				{
					FLightingContext LC = InitLight(FragData,MaterialData,_SpecMult,
					_MainLightDir.xyz,_MainLightColor.xyz,1, ShadowData,_ShadowColor.xyz);
					SM_Ramp01(MaterialData,LC,ShadowMask,FragData,DirectDiffuse DEBUG_PARAM);

					//FLOAT3 PointPos = mul(unity_WorldToObject , FLOAT4(_LocalOffset.xyz,1)).xyz;
					FLOAT4 LocalPosition = FragData.LocalPosition;				
					REAL Distance = length(LocalPosition.xyz - _LocalOffset.xyz);
					REAL Noise = SAMPLE_TEX2D(_NoiseTex, GET_FRAG_UV*_DissolveTiling).r;

					REAL distCutout = Distance - _MaxDistance * _LocalOffset.w * 0.2 - Noise * _NoiseEffect;
					clip(distCutout);
					REAL2 EdgeColUV = FLOAT2(_ColScale*(distCutout / _EdgeWidth), (distCutout / _EdgeWidth));
					REAL4 EdgeCol = pow(abs(SAMPLE_TEX2D(_EdgeTex, EdgeColUV)*_EdgeColor) , 3);
					REAL Range= smoothstep(0, 1, distCutout / _EdgeWidth);
					DirectDiffuse = lerp(EdgeCol, DirectDiffuse, Range);
					DirectDiffuse = FragData.facing > 0 ? DirectDiffuse : _BackColor;
					//DirectDiffuse = Distance - _MaxDistance>0;
					SM_StandardSpec(LC,DirectSpecular DEBUG_PARAM);
					DEBUG_CUSTOMDATA_PARAM(LC0, LC)
					//UNITY_BRANCH
					//if (_AddLightEnable)
					//{
					//	#define specScale1 _SpecMult*0.5f
					//	FLightingContext LC1 = InitLight(FragData,MaterialData,specScale1,
					//	_AddLightDir.xyz,_AddLightColor.xyz,1,1,1);
					//	SM_SupplementDiffuse(LC1,DirectDiffuse DEBUG_PARAM);
					//	SM_SupplementSpec(LC1,DirectSpecular DEBUG_PARAM);
					//	DEBUG_CUSTOMDATA_PARAM(LC1, LC1)
					//}
				}



				#include "../Include/URP_Pixel.hlsl"


				//only editor use
				//#pragma shader_feature_local _ _DEBUG_APP
				#pragma shader_feature_local _ _PBS_FROM_PARAM
				//render type
				#pragma shader_feature_local _ _ALPHA_BLEND _ALPHA_TEST

				//lighting
				//#pragma shader_feature_local _ _SHADOW_MAP
				//#pragma shader_feature_local _ _NO_CSM

				//#pragma shader_feature_local _ _ADD_LIGHT
				#pragma multi_compile_local _ _DITHER_TRANSPARENCY


				#pragma vertex  vertForwardBase
				#pragma fragment fragForwardBase


			ENDHLSL
		}
		Pass
		{
			Name "OverdrawForwardBase"
			Tags{"LightMode" = "OverdrawForwardBase"}

			Blend One One
			HLSLPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag

			// #include "UnityCG.cginc"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "../API/CommonAPI.hlsl"
			
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
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

			ZWrite On
			ZTest LEqual
			ColorMask 0
			Cull[_Cull]

			HLSLPROGRAM

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
			#pragma multi_compile_local _ _DITHER_TRANSPARENCY
			#include "URP_Role_DepthOnly.hlsl"

            ENDHLSL
        }
		
		// Pass
		// {
		// 	Offset 1,1
		//  Name "Outline"
		//  Tags{ "LightMode" = "Outline" "RenderType" = "Transparent" "Queue" = "Transparent" }
		//  Cull Front
		//  Blend SrcAlpha OneMinusSrcAlpha
		// 	Stencil
		// 	{
		// 		Ref[_Stencil]
		// 		Comp equal
		// 	}
		// 	HLSLPROGRAM
		// 		#pragma target 3.0
 
		// 		#include "../Tools/Outline.hlsl"
		// 		//#pragma shader_feature_local _ _LOCAL_WORLD_OFFSET

		// 		#pragma vertex vertForwardBase
		// 		#pragma fragment fragForwardBase
		// 	ENDHLSL
		// }

	}
}

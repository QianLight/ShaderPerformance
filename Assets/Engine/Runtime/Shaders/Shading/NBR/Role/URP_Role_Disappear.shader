Shader "URP/Role/Disappear"
{
	Properties
	{
		//pbr
		[NoScaleOffset]_MainTex ("Base Tex", 2D) = "white" {}	
		_Color0("Main Color", Color) = (1,1,1,1)

		[NoScaleOffset]_ProcedureTex0("Normal:rg Roughness:b Metallic:a", 2D) = "" {}
		_UVST1("PBR Scale Offset", Vector) = (1,1,0,0)
		_Param0("AO:x normalscale:y w:metallicMask", Vector) = (0,1,1,0)
		_Param1("IBLScale:z Cutout:w", Vector) = (0.1,0.5,0,0.5)
		_PBRParam("x: Roughness y: Metallic", Vector) = (0,1,0,1)

		_Param2("Emissive a:toonAoScale", Vector) = (0,0,0,0)
		[HDR]_Color1("Emission Color",Color) = (1,1,1,1)
//ramp
		[NoScaleOffset]_ProcedureTex2 ("Ramp", 2D) = "" {}  
		_Color3("ShadowColorMultiply",Color)=(1,1,1,1)

		_Color4("Dark Rim Color", Color) = (0.8,0.8,0.8,1)
		_Color6("Rim dri",Color)=(1,0,0,1)

		//emissive
		[HDR]_Color5("EmissionColor",Color)=(1,1,1,1)

		_ColorOutline("Outline Color", Color) = (0,0,0,0.003)
		_ColorOutline2("Outline Color2", Color) = (0,0,0,0)
		[HideInInspector]_MatEffectOutlineParam("Outline Color", Color) = (0,0,0,0)

		_Param("", Vector) = (0,0,0,0)
		_Param3("Param", Vector) = (1,1,0,0)
		[HDR]_RimColortoon("RimColor",Color) = (1,1,1,1)
		_HeightGradientColor("Gradient Color", Color) = (0,0,0,0)

		// [HideInInspector] _SrcBlend("__src", FLOAT) = 1.0
		// [HideInInspector] _DstBlend("__dst", FLOAT) = 0.0
		// [HideInInspector] _ZWrite("__zw", FLOAT) = 1.0
		[HideInInspector] _DebugMode("__debugMode", Float) = 0.0
		
		//CBuffer Fill
		[HideInInspector]_Color("_Color", Color) = (1,1,1,1)
		[HideInInspector]_Color2("_Color2", Color) = (1,1,1,1)
		[HideInInspector]_GrassCollisionParam("_GrassCollisionParam", Vector) = (1,1,1,1)
		[HideInInspector]_HeightGradientPreivewColor("_HeightGradientPreivewColor", Vector) = (1,1,1,1)
		[HideInInspector]_HeightGradientPreviewParam("_HeightGradientPreviewParam", Vector) = (1,1,1,1)
		[HideInInspector]_LightMapUVST("_LightMapUVST", Vector) = (1,1,1,1)
		[HideInInspector]_Param4("_Param4", Vector) = (1,1,1,1)
		[HideInInspector]_Param5("_Param5", Vector) = (1,1,1,1)
		[HideInInspector]_ProcedureTex1_TexelSize("_ProcedureTex1_TexelSize", Vector) = (1,1,1,1)
		[HideInInspector]_UVST0("_UVST0", Vector) = (1,1,1,1)
		[HideInInspector]_OutlineScale("_OutlineScale", Vector) = (1,1,1,1)
		[HideInInspector]_LaserColor("LaserColor", Color) = (0,0,0,0)
		[HideInInspector]_SSRimColor("_SSRimColor", Color) = (0,0,0,0)
		[HideInInspector]_SSRimParam0("_SSRimParam0", Color) = (0,0,0,0)
		[HideInInspector]_EdgeColor("EdgeColor",color) = (1,1,1,1)
		[HideInInspector]_BackColor("BackColor",color) = (1,1,1,1) 
		[HideInInspector]_AnisotropyParam ("AnisotropyScale:x WarpIntensity:y WarpIntensity:z WarpBendFactor:w", Vector) = (1,3,5,1.5)
        [HideInInspector]_FabricColor1 ("WarpColor:rgb, WarpRoughness:a", Color) = (1,1,1,1)
        [HideInInspector]_FabricColor2 ("WeftColor:rgb, WeftRoughness:a", Color) = (1,1,1,1)
		[HideInInspector]_UVST3("_UVST3", Vector) = (1,1,1,1)
		[HideInInspector] _LocalOffset("pos", Vector) = (0,1,0,1)
		[HideInInspector] _ShadowPos("ShadowPos", Vector) = (0,0,0,0)
	}

	HLSLINCLUDE

		#define ROLE_EFFECT
		#define _ROLE_RENDER_V2
		//feature
		#define _MAIN_COLOR
		//lighting
		//#define _UNREAL_MODE
		#define _NO_COLOR_EFFECT

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
		#define _SM_DARK_RIM
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
		Tags{"RenderType" = "Opaque"  "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" }

		//Tags { "RenderType"="Transparent" "Queue"="Transparent-10" "PerformanceChecks" = "False" }
		LOD 100
		
		// Pass
		// {
		// 	Name "ForwardTransparent0"
		// 	Tags{ "LightMode" = "ForwardTransparent0" "RenderType"="Opaque" "PerformanceChecks" = "False" }
		// 	ColorMask 0

		// 	HLSLPROGRAM
		// 		#define _UN_LIGHT
		// 		#define _PBS_NO_IBL	
		// 		#pragma target 3.0

		// 		#include "../Include/PCH.hlsl"
		// 		#include "../Include/Vertex.hlsl"


		// 		#define _CUSTOM_PS
		// 		#define _NO_MRT
		// 		FLOAT4 CustomPS(in FInterpolantsVSToPS Interpolants, in FLOAT4 SvPosition,inout FLOAT4 rt1)
		// 		{
		// 			rt1.xy = EncodeFloatRG(Interpolants.Depth01.x/Interpolants.Depth01.y);
		// 			return 0;
		// 		}

		// 		#include "../Include/Pixel.hlsl"
		// 		//#pragma shader_feature_local _ _DEBUG_APP

		// 		#pragma vertex vertForwardBase
		// 		#pragma fragment fragForwardBase
			
		// 	ENDHLSL
		// }
		Pass
		{
			Name "UniversalForward"
			Tags{ "LightMode" = "UniversalForward" "RenderType"="Opaque" }
			// Blend SrcAlpha OneMinusSrcAlpha
			// ZWrite Off

			HLSLPROGRAM	
				//#define _ALPHA_BLEND
				#define _CARTOON
				#define _VERTEX_GI
			
				// -------------------------------------
				// Universal Pipeline keywords
				// #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
				// #pragma multi_compile _ _SMARTSOFTSHADOW_ON
			    #define _SMARTSOFTSHADOW_ON
				#define _ROLE_ADDITIONAL_LIGHT_ON
				
				#pragma multi_compile _SHADER_LEVEL_HIGH _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW _SHADER_LEVEL_VERY_LOW

				//----------------------
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			
				#ifdef _SM_4
					#pragma target 5.0
				#else//!_SM_4
					#pragma target 3.0
				#endif//_SM_4

				#define _CUSTOM_VERTEX_PARAM
				#define _VERTEX_COLOR
				#define _ROLE_HEIGHT_GRADIENT

				#include "Role_Head.hlsl"
				#include "../Include/URP_LightingHead.hlsl"

				#define _CUSTOM_VERTEX_PARAM

				FLOAT4 _ObjectPos;
				inline void CustomVertex(in FVertexInput Input,inout FInterpolantsVSToPS Interpolants)
				{
					TransformHeightGradient(Input, Interpolants);
					FLOAT3 center = Interpolants.WorldPosition.xyz - _ObjectPos.xyz;
					Interpolants.CustomData.xyz = center;
				}

				#include "../Include/URP_Vertex.hlsl"

				#define _DirIndex _Param3.x
				#define _DirOffset _Param3.y
				#define _Height _Param3.z
				#define _ColorHeight _Param3.w



				#define _CUSTOM_BASECOLOR
				static const REAL3x3 Dir = { 1,0,0, 0,1,0, 0,0,1};
				void CalcCustomBaseColor(in FFragData FragData,inout FMaterialData MaterialData)
				{
					REAL2 uv = GET_FRAG_UV;
					REAL4 color =  SAMPLE_TEX2D(_MainTex, uv);
					MaterialData.BaseColor = color*_MainColor;
					MaterialData.DyeColor = MaterialData.BaseColor.rgb;
					REAL dirMask = dot(Dir[(int)_DirIndex], FragData.CustomData.xyz) + _DirOffset;
					
					REAL noiseHeight = _ColorHeight * saturate(_Height);
					REAL mask = dirMask - _Height + noiseHeight;
					REAL noiseHeightLerp = noiseHeight < 0.001 ? 0 : saturate(0.5 * mask / noiseHeight);


					REAL noiseCut = SAMPLE_TEX2D(_GlobalNoise, GET_FRAG_UV * 5).r - 0.45;
					REAL noiseCutoutMask = lerp(0,noiseCut, noiseHeightLerp);
					clip(mask - noiseHeight + noiseCutoutMask);
					MaterialData.BaseColor.a = 1;
		
					REAL colorHeightLerp = noiseHeight < 0.001 ? 1 : saturate(2*(dirMask - _Height) / noiseHeight);
					MaterialData.Emissive = lerp(_Color5.xyz, MaterialData.DyeColor, colorHeightLerp);
				}
				//Lighting
				
				#define _CUSTOM_LIGHT

				void CustomLighting(FFragData FragData, FMaterialData MaterialData,
					FShadowData ShadowData,FLOAT ShadowMask,
					inout REAL3 DirectDiffuse,inout REAL3 DirectSpecular DEBUG_ARGS)
				{
					REAL2 Shadow = ShadowData.Shadow.xy;
					REAL3 shadowLightColor = lerp(_MainLightColor.xyz, _RoleShadowColor.rgb, 1 - Shadow.y);
					FLightingContext LC = InitLight(FragData,MaterialData,_SpecMult,
						_MainLightDir.xyz, shadowLightColor.xyz,1, ShadowData,_ShadowColor.xyz);
					SM_Ramp01(MaterialData,LC,ShadowMask,FragData,DirectDiffuse DEBUG_PARAM);
					SM_StandardSpec(LC,DirectSpecular DEBUG_PARAM);
					DEBUG_CUSTOMDATA_PARAM(LC0, LC)
	/*				UNITY_BRANCH
					if(_AddLightEnable)
					{
						#define specScale1 _SpecMult*0.5f
						FLightingContext LC1 = InitLight(FragData,MaterialData,specScale1,
						_AddLightDir.xyz,_AddLightColor.xyz,1,1,1);
						SM_SupplementDiffuse(LC1,DirectDiffuse DEBUG_PARAM);
						SM_SupplementSpec(LC1,DirectSpecular DEBUG_PARAM);
						DEBUG_CUSTOMDATA_PARAM(LC1, LC1)
					}*/
				}
				#include "../Include/URP_Pixel.hlsl"

				//only editor use
				//#pragma shader_feature_local _ _DEBUG_APP

				#pragma vertex vertForwardBase
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
	}
	
	CustomEditor "CFEngine.Editor.PBSShaderGUI"
}

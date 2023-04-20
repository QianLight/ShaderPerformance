Shader "URP/SFX/GPUAnimation"
{
    Properties
    {
        //pbr
		[NoScaleOffset]_MainTex ("Base Tex", 2D) = "white" {}
		_Color0("Main Color", Color) = (1,1,1,1)

		[NoScaleOffset]_ProcedureTex0 ("Normal:rg Mask:ba", 2D) = "" {}
		_UVST1("PBR Scale Offset", Vector) = (1,1,0,0)
		_Param0("AO:x normalscale:y w:metallicMask", Vector) = (0,0,1,0)
		_Param1("IBLScale:z Cutout:w", Vector) = (0.1,0.5,0,0.5)
		_PBRParam("x: Roughness y: Metallic", Vector) = (0,1,0,1)
		//mask
		[NoScaleOffset]_MainTex1("r:rim mask g: emissive b: shadow ", 2D) =  "black" {}
		_Param2("x:maskEnable g:emissive b:shadowMask ", Vector) = (0,0,0,0) 

		//ramp
		[NoScaleOffset]_ProcedureTex2 ("Ramp", 2D) = "" {}  
		_Color3("ShadowColorMultiply",Color)=(1,1,1,1)

				//outline	
		_ColorOutline("Outline Color", Color) = (0,0,0,0.003)
		_Param3("x:outlineZOffset y:VerticalGradient z:ShadowInt w:_VerticalGradientOffset", Vector) = (0,-1,0,-1) 
		[HideInInspector]_MatEffectOutlineParam("Outline Color", Color) = (0,0,0,0)

        //animation
        [NoScaleOffset]_ProcedureTex3 ("Animation", 2D) = "" {}   
        _CustomTime("", Vector) = (0,0,0,0) 

		//Dark Rim 		
		_Color4("Dark Rim Color", Color) = (0.8,0.8,0.8,1)
		_Color6("Rim dri",Color)=(1,0,0,1)

		_Param("", Vector) = (1,1,0,0)
		_Color("Effect Color", Color) = (1,1,1,1)

		[HideInInspector] _SrcBlend("__src", FLOAT) = 1.0
		[HideInInspector] _DstBlend("__dst", FLOAT) = 0.0
		[HideInInspector] _ZWrite("__zw", FLOAT) = 1.0
		[HideInInspector] _Stencil ("Stencil ID", Float) = 0
		[HideInInspector] _DebugMode("__debugMode", Float) = 0.0
    }
	HLSLINCLUDE
		//feature
		#define _MAIN_COLOR
		#define _INPUT_UV2_4	
		#define _VERTEX_COLOR
		#define _GPU_ANIMATION	
		#define _ROLE_GPU
		#define _USE_CONSTANT_BUFFER
		//lighting
		#define _VERTEX_GI
		#define _CARTOON		
		#define _SM_DARK_RIM
		// #define _CUSTOM_EFFECT

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
	ENDHLSL

	SubShader
	{
		Tags { "RenderType"="Opaque" "PerformanceChecks" = "False" "IgnoreProjector"="False"}

		Pass
		{
			Name "FORWARD"
			Tags{ "LightMode" = "UniversalForward" }
			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]
            Stencil
            {
                Ref [_StencilRef_Role]
                Comp Always
                Pass Replace
            }
			HLSLPROGRAM

				#define URP_BASE
				#define REDEFINE_URP

			    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
				#include "../Include/Pragam_Head.hlsl"
				#ifdef _SM_4
					#pragma target 5.0
				#else//!_SM_4
					#pragma target 3.0
				#endif//_SM_4

				#include "../Role/Role_Head.hlsl"
				#include "../Include/GpuAnimation.hlsl"

				inline void CustomVertex(in FVertexInput Input,inout FInterpolantsVSToPS Interpolants)
				{
					
				}
			
				#include "../Include/URP_Vertex.hlsl"

				//Lighting
				#define _CUSTOM_LIGHT
				void CustomLighting(FFragData FragData, FMaterialData MaterialData,
					FShadowData ShadowData,FLOAT ShadowMask,
					inout FLOAT3 DirectDiffuse,inout FLOAT3 DirectSpecular DEBUG_ARGS)
				{
					FLightingContext LC = InitLight(FragData,MaterialData,_SpecMult,
						_MainLightDir.xyz,_MainLightColor.xyz,1,ShadowData,_ShadowColor.xyz);
					SM_Ramp01(MaterialData,LC,ShadowMask,FragData,DirectDiffuse DEBUG_PARAM);
					SM_StandardSpec(LC,DirectSpecular DEBUG_PARAM);
					DEBUG_CUSTOMDATA_PARAM(LC0, LC)
				}

				#include "../Include/URP_Pixel.hlsl"

				#pragma multi_compile _ _INSTANCE
				#pragma multi_compile _ _BONE_2
				// #pragma multi_compile _SHADER_LEVEL_HIGH _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW _SHADER_LEVEL_VERY_LOW
				
				#pragma vertex  vertForwardBase
				#pragma fragment fragForwardBase
			
		
			ENDHLSL
		}	
		
		Pass
		{
			Name "OUTLINE"
			Offset 1,[_OutlineZOffset]
			Tags{ "LightMode" = "Outline" "RenderType" = "Transparent" "Queue" = "Transparent" }
			Cull Front
			Blend SrcAlpha OneMinusSrcAlpha
			Stencil
			{
				Ref[_StencilRef_Outline]
				Comp LEqual
			}
			HLSLPROGRAM
					
				#define URP_BASE
				#define REDEFINE_URP
			    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

				#include "../Include/Pragam_Head.hlsl"
				#ifdef _SM_4
					#pragma target 5.0
				#else//!_SM_4
					#pragma target 3.0 
				#endif//_SM_4

				#define _CUSTOM_VERTEX_PARAM

				//mask
				#include "../Role/Role_Head.hlsl"
 				#include "../Include/GpuAnimation.hlsl"
				#include "../Tools/Outline.hlsl"
				#pragma multi_compile _ _INSTANCE
				#pragma multi_compile _ _BONE_2
				#pragma multi_compile_local _ _DITHER_TRANSPARENCY
				// #pragma multi_compile _SHADER_LEVEL_HIGH _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW _SHADER_LEVEL_VERY_LOW
				#pragma vertex vertForwardBase
				#pragma fragment fragForwardBase
			ENDHLSL
		}

//		Pass
//		{
//			Name "OverdrawFS"
//			Tags{"LightMode" = "OverdrawForwardBaseT"}
//
//			Blend One One
//			ZWrite[_ZWrite]
//			Stencil
//			{
//				Ref[_StencilRef_Role]
//				Comp equal
//			}
//			HLSLPROGRAM
//
//			#pragma vertex Vert
//			#pragma fragment Frag
//
//			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
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
//
//			ENDHLSL
//		}
//		Pass
//		{
//			Name "OverdrawOC"
//			Tags{"LightMode" = "OverdrawOutline"}
//
//			Blend One One
//			Cull Front
//			Offset 1,1
//			Stencil
//			{
//				Ref[_StencilRef_Outline]
//				Comp equal
//			}
//			HLSLPROGRAM
//
//			#pragma vertex Vert
//			#pragma fragment Frag
//
//			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
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
//			
//			ENDHLSL
//		}
	}
CustomEditor "CFEngine.Editor.PBSShaderGUI"
}

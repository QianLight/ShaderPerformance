Shader "URP/Role/Hair"
{
	Properties
	{
		//pbr
		[NoScaleOffset]_MainTex ("Base Tex", 2D) = "white" {}
		_UVST0("Base Scale Offset", Vector) = (1,1,0,0)
		_Color0("Main Color", Color) = (1,1,1,1)
		
		[NoScaleOffset]_ProcedureTex0 ("Normal:rg Mask:ba", 2D) = "gray" {}
		_UVST1("PBR Scale Offset", Vector) = (1,1,0,0)
		_Param0("AO:x normalscale:y w:metallicMask", Vector) = (0,1,1,0)
		_Param1("IBLScale:z Cutout:w", Vector) = (0.1,0.5,0,0.5)
		_PBRParam("x: Roughness y: Metallic", Vector) = (0,1,0,1)
		//mask
		[NoScaleOffset]_MainTex1("r:rim mask g: emissive b: shadow a:hair", 2D) =  "black" {}
		_Param2("x:maskEnable g:emissive b:shadowMask ", Vector) = (0,0,0,0)

		//ramp
		[NoScaleOffset]_ProcedureTex2 ("Ramp", 2D) = "" {}  
		_Color3("ShadowColorMultiply",Color)=(1,1,1,1)

		//Customshadow
		// _Color6("Dark Color", Color) = (1,1,1,1)
		// _Param3("x: y: z: w: ", Vector) = (1,2,0,0)

		//Dark Rim 		
		_Color4("Dark Rim Color", Color) = (0.8,0.8,0.8,1)

		//kajiya
		_KajiyaParam0("ShiftOffset:x ShiftScale:y", Vector) = (1.62,1.62,1,0.9)
		_KajiyaParam1("ShiftIntensity:x ShiftOffset:y SpecualrIntensity1:z SpecualrIntensity2:w", Vector) = (1,-0.41,5,2)
		
		[NoScaleOffset]_AnisoShift("anisoShift Tex", 2D) = "white" {}

		[HDR]_Color1("Specualr Color0",Color) = (1,1,1,1)
		[HDR]_Color2("Specualr Color1",Color) = (1,1,1,1)
		
		//_ParamOutline("x:ouline fall start y:ouline fall end z:outline width falloff ", Vector) = (0.0,0.2,0.2,0.5)
		_ColorOutline("Outline Color", Color) = (0,0,0,0.003)
		//_ColorOutline2("Outline Color2", Color) = (0,0,0,0)
		[HideInInspector]_MatEffectOutlineParam("Outline Color", Color) = (0,0,0,0)

		_Param("SupplementLight:x AddLight:y", Vector) = (1,1,0,0)
		_Color("Effect Color", Color) = (1,1,1,1)
		[HDR]_RimColortoon("RimColor",Color) = (1,1,1,1)
		_HeightGradientColor("Gradient Color", Color) = (0,0,0,0)
		_FaceShadowParam ("FaceShadowParam", Vector) = (0.01,0,0,0)
		
		// SRP Batcher added
		_Param3("Param3", Color) = (1,1,1,1)
		_Color5("Color5", Color) = (1,1,1,1)
		_Color6("Color6", Color) = (1,1,1,1)
		_HeightGradientPreviewParam("Gradient PreviewParam", Vector) = (0,0,0,0)
		_HeightGradientPreivewColor("Gradient PreivewColor", Vector) = (0,0,0,0)
		_RootPosWS ("RootPosWS", Vector) = (0,0,0,0)
        _ScreenSpaceRimParam("Screen Space Rim", Vector) = (5,5,0,0)
        _GlobalFaceShadowParam("Screen Space Rim", Vector) = (5,5,0,0)
        _SceneColor("SceneColor", Vector) = (0,0,0,1)

		[HideInInspector] _SrcBlend("__src", FLOAT) = 1.0
		[HideInInspector] _DstBlend("__dst", FLOAT) = 0.0
		[HideInInspector] _ZWrite("__zw", FLOAT) = 1.0
		[HideInInspector] _Stencil ("Stencil ID", Float) = 0
		
		_SSRimColor ("SSRim Color",Color) = (1,1,1,0.3)
		_SSRimParam0("x:Distance Threshold w:RimScale",Vector) = (0.1,0,0,2)
		
		[HideInInspector] _OLZWrite ("_OutlineZWrite", Float) = 0
		[HideInInspector] _ShadowPos("ShadowPos", Vector) = (0,0,0,0)
	}

	HLSLINCLUDE

		#define ROLE_EFFECT
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		#define UNIFORM_PCH_OFF
		#define ROLE_SRP_BATCH
		#define URP_ROLE_HAIR

        CBUFFER_START(UnityPerMaterial)
		float4 _UVST0;
		float4 _Color0;
		float4 _UVST1;
		float4 _Param0;
		float4 _Param1;
		float4 _PBRParam;
		float4 _Param2;
		float4 _Color3;
		float4 _Color4;
		float4 _KajiyaParam0;
		float4 _KajiyaParam1;
		float4 _Color1;
		float4 _Color2;
		float4 _ColorOutline;
		float4 _MatEffectOutlineParam;
		float4 _Param;
		float4 _Color;
		float4 _RimColortoon;
		float4 _HeightGradientColor;
        float4 _HeightGradientPreviewParam;
        float4 _HeightGradientPreivewColor;
		float4 _FaceShadowParam;
	    float4 _RootPosWS;
		float4 _ShadowPos;
		float4 _GlobalFaceShadowParam;
		float4 _Param3;
		float4 _Color5;
		float4 _Color6;
	    float4 _SceneColor;
		float4 _SSRimColor;
		float4 _SSRimParam0;
		CBUFFER_END
		#define SCREEN_SPACE_RIM_IS_IN_COMMON_CBUFFER

		#define UNITY_PERMATERIAL_DECLARED

	    #define _MainColor _Color0
        TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
        TEXTURE2D(_MainTex1);SAMPLER(sampler_MainTex1);
	    TEXTURE2D(_AnisoShift);SAMPLER(sampler_AnisoShift);
        TEXTURE2D(_ProcedureTex0);SAMPLER(sampler_ProcedureTex0);
		TEXTURE2D(_ProcedureTex2);SAMPLER(sampler_ProcedureTex2);
        TEXTURE2D(_ProcedureTex3);SAMPLER(sampler_ProcedureTex3);
        TEXTURE2D(_ProcedureTex4);SAMPLER(sampler_ProcedureTex4);

		#define KAJIYA_ON

		//feature
		#define _MAIN_COLOR
		#define _UV_SCALE2
		#define _UV_SCALE
		#define _CUSTOM_UV2
		
		//lighting
		#define _VERTEX_GI
		#define _NO_AO
		#define _NO_EMISSIVE
		#define _SHIFT_TEX
		// #define _PBS_FROM_PARAM
		#define _CARTOON
		#define _CSM3

		#if !defined(_ADD_LIGHT) && !defined(SHADER_API_MOBILE)
			#define _ADD_LIGHT
		#endif

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
		Tags{"RenderType" = "Opaque"  "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" }
		LOD 100

		Pass
		{
			Name "UniversalForward"
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
			
				// -------------------------------------
				// Universal Pipeline keywords
			//	#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
				// #pragma multi_compile _ _SMARTSOFTSHADOW_ON
				// #pragma multi_compile _ _PARKOUR
				#define _SMARTSOFTSHADOW_ON
				#define _VERTEX_COLOR
				#define _CUSTOM_EFFECT
				#define _CUSTOM_VERTEX_PARAM
				#define _ROLE_HEIGHT_GRADIENT
				#define ROLE_SHADER_LEVEL
				#define _SRP_DEBUG

			    #include "../../Include/ShaderLevel.hlsl"
			
				//----------------------
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
				#include "../Include/Pragam_Head.hlsl"
				#ifdef _SM_4
					#pragma target 5.0
				#else//!_SM_4
					#pragma target 3.0
				#endif//_SM_4

				#include "Role_Head.hlsl"
			
				void CustomVertex(in FVertexInput Input,inout FInterpolantsVSToPS Interpolants)
				{
					TransformHeightGradient(Input, Interpolants);

					FLOAT3 binormal = cross( normalize(Input.TangentX), normalize(Input.TangentZ.xyz) ) * Input.TangentZ.w;
					FLOAT3x3 rotation = FLOAT3x3( Input.TangentZ.xyz, binormal, Input.TangentX );
					FLOAT3 objSpaceCameraPos = mul(_worldToObject, FLOAT4(_CameraPos.xyz, 1)).xyz;
					FLOAT3 viewDir = normalize(objSpaceCameraPos -Input.Position.xyz);
					Interpolants.CustomData.xyz = normalize(mul(rotation, viewDir));

					FLOAT3 objSpaceLightPos = mul(_worldToObject, FLOAT4(_MainLightDir.xyz,0)).xyz;
					FLOAT3 lightDir = normalize(objSpaceLightPos);

					Interpolants.CustomData1.xyz = normalize(mul(rotation, lightDir));
					SetSmartShadowValue(Interpolants, GetRoleSmartShadow());
				}

				#include "../Include/MaterialTemplateHead.hlsl"				
				#define _CUSTOM_LIGHT
				void CustomLighting(FFragData FragData, inout FMaterialData MaterialData,
					FShadowData ShadowData,FLOAT ShadowMask,
					inout FLOAT3 DirectDiffuse,inout FLOAT3 DirectSpecular DEBUG_ARGS)
				{
					FLOAT2 Shadow = ShadowData.Shadow.xy;
					FLOAT3 shadowLightColor = lerp(_MainLightColor.xyz, _RoleShadowColor.rgb, 1 - Shadow.y);
					FLightingContext LC = InitLight(FragData,MaterialData,_SpecMult,
						_MainLightDir.xyz,shadowLightColor, ShadowData,_ShadowColor.xyz);

					SM_Ramp(FragData,MaterialData,LC, Shadow.y, DirectDiffuse DEBUG_PARAM);

					// UNITY_BRANCH
					// if(_AddLightEnable)
					// {
					// 	#define specScale1 _SpecMult*0.5f
					// 	FLightingContext LC1 = InitLight(FragData,MaterialData,specScale1,
					// 	_AddLightDir.xyz,_AddLightColor.xyz, DefaultShadowData(),1);
					// 	SM_SupplementDiffuse(LC1,DirectDiffuse DEBUG_PARAM);
					// 	DEBUG_CUSTOMDATA_PARAM(LC1, LC1)
					// }

					SM_Kajiya(FragData,MaterialData,GetFadeShadow(ShadowData),
							_MainLightDir.xyz, shadowLightColor,
							_MainLightDir.xyz,
							DirectSpecular DEBUG_PARAM);
					DirectSpecular *= 1 - MaterialData.CustomParam.a;

					MaterialData.BaseColor.a = 1;
				}

				#include "../Include/URP_Vertex.hlsl"
				#include "../../Include/ShaderDebug.hlsl"
				#include "../Include/URP_Pixel.hlsl"

				//only editor use
				//#pragma shader_feature_local _ _DEBUG_APP
			    #pragma multi_compile _SHADER_LEVEL_HIGH _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW _SHADER_LEVEL_VERY_LOW

				#pragma shader_feature_local _ _PBS_FROM_PARAM
				
				//render type
				#pragma shader_feature_local _ _ALPHA_BLEND _ALPHA_TEST

				//lighting
				//#pragma shader_feature_local _ _SHADOW_MAP
				//#pragma shader_feature_local _ _EXTRA_SHADOW
				//#pragma shader_feature_local _ _NO_CSM

				//#pragma shader_feature_local _ _ADD_LIGHT
				#pragma multi_compile_local _ _DITHER_TRANSPARENCY
				#pragma shader_feature_local _ _ARRAY_LIGHTBUFFER
				#pragma shader_feature _SHADER_DEBUG

				//#pragma shader_feature_local _ _LOCAL_WORLD_OFFSET

				#pragma vertex vertForwardBase
				#pragma fragment fragForwardBase
			
			ENDHLSL
		}

		Pass
		{
			Name "Outline"
//			Offset 1,[_OutlineZOffset]
			Tags{ "LightMode" = "Outline" "RenderType" = "Transparent" "Queue" = "Transparent" }
			Cull Front
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite[_OLZWrite]
			
			HLSLPROGRAM
				#pragma target 3.0
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

				#include "../Tools/Outline.hlsl"
				//#pragma shader_feature_local _ _LOCAL_WORLD_OFFSET
				#pragma multi_compile_local _ _DITHER_TRANSPARENCY
				// #pragma multi_compile _ _PARKOUR
				#pragma vertex vertForwardBase
				#pragma fragment fragForwardBase
			ENDHLSL
		}
		
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
                Comp equal
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

			#include "Role_StencilShadowCaster.hlsl"

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
//            Blend One one
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
//
//            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//			#include "Role_Simple_Screen_Space_Rim.hlsl"
//            
//            ENDHLSL
//        }
		
		Pass
		{
			Name "ShadowCaster"
			Tags{"LightMode" = "ShadowCaster"}

			ZWrite On
			ZTest LEqual
			ColorMask 0
			Cull[_Cull]

			HLSLPROGRAM

			#pragma vertex ShadowPassVertex
			#pragma fragment ShadowPassFragment

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

			float3 _LightDirection;

			struct Attributes
			{
			    float4 positionOS   : POSITION;
			    float3 normalOS     : NORMAL;
			};

			struct Varyings
			{
			    float4 positionCS   : SV_POSITION;
			};

			float4 GetShadowPositionHClip(Attributes input)
			{
			    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
			    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

			    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

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
			Cull[_Cull]

			HLSLPROGRAM

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

			#include "URP_Role_DepthOnly.hlsl"

            ENDHLSL
        }
		////UsePass "Hidden/Custom/Tools/OutLine/OUTLINE"			

		//Pass
		//{
		//	Name "OverdrawForwardBase"
		//	Tags{"LightMode" = "OverdrawForwardBase"}

		//	Blend One One
		//	ZWrite[_ZWrite]
		//	Stencil
		//	{
		//		Ref[_StencilRef_Role]
		//		Comp equal
		//	}
		//	CGPROGRAM

		//	#pragma vertex Vert
		//	#pragma fragment Frag

		//	#include "UnityCG.cginc"

		//	struct Attributes
		//	{
		//		float4 vertex : POSITION;
		//	};
		//	
		//	struct Varyings
		//	{
		//		float4 vertex : SV_POSITION;
		//	};
		//	Varyings Vert(Attributes v)
		//	{
		//		Varyings o;
		//		float4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
		//		o.vertex = mul(unity_MatrixVP, WorldPosition);
		//		return o;
		//	}

		//	half4 Frag(Varyings i) : SV_Target
		//	{
		//		return half4(0.1, 0.04, 0.02, 1);
		//	}

		//	ENDCG
		//}

		//Pass
		//{
		//	Name "OverdrawOutline"
		//	Tags{"LightMode" = "OverdrawOutline"}

		//	Blend One One
		//	Cull Front
		//	Offset 1,1
		//	Stencil
		//	{
		//		Ref[_StencilRef_Outline]
		//		Comp equal
		//	}
		//	HLSLPROGRAM

		//	#pragma vertex Vert
		//	#pragma fragment Frag

		//	#include "UnityCG.cginc"

		//	struct Attributes
		//	{
		//		float4 vertex : POSITION;
		//	};
		//	
		//	struct Varyings
		//	{
		//		float4 vertex : SV_POSITION;
		//	};
		//	Varyings Vert(Attributes v)
		//	{
		//		Varyings o;
		//		float4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
		//		o.vertex = mul(unity_MatrixVP, WorldPosition);
		//		return o;
		//	}

		//	half4 Frag(Varyings i) : SV_Target
		//	{
		//		return half4(0.1, 0.04, 0.02, 1);
		//	}
		//	
		//	ENDHLSL
		//}

	}
	CustomEditor "CFEngine.Editor.PBSShaderGUI"
}

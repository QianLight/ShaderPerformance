Shader "URP/Role/StyleHair"
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
		_Color6("Dark Color" , Color) = (1,1,1,1)	

		//StyleHair
		_Color1("SpeculrColor", Color) = (1,1,1,1)
		[NoScaleOffset]_ProcedureTex3("LightmapTex", 2D) = "white" {}
		_Param4("x:LightWith y:LightLength z:Feather w:Threshold ", Vector) = (1,2,0,0)

		//Dark Rim 		
		_Color4("Dark Rim Color", Color) = (0.8,0.8,0.8,1)	
		
		//_ParamOutline("x:ouline fall start y:ouline fall end z:outline width falloff ", Vector) = (0.0,0.2,0.2,0.5)
		_ColorOutline("Outline Color", Color) = (0,0,0,0.003)
		_ColorOutline2("Outline Color2", Color) = (0,0,0,0)
		[HideInInspector]_MatEffectOutlineParam("Outline Color", Color) = (0,0,0,0)

		_Param("SupplementLight:x AddLight:y", Vector) = (1,1,0,0)
		_Color("Effect Color", Color) = (1,1,1,1)
		_Color5("Effect Color5", Color) = (1,1,1,1)
		[HDR]_RimColortoon("RimColor",Color) = (1,1,1,1)
		_HeightGradientColor("Gradient Color", Color) = (0,0,0,0)

		[HDR]_RimColor("RimColor",Color)=(1,1,1,1)
		_GradientColor("GradientColor",Color)=(1,1,1,1)
		_Param5("x:RimSmooth y:RimOffset z:RimInt w: ", Vector) = (1,1,1,0)
		_FaceShadowParam ("FaceShadowParam", Vector) = (0.01,0,0,0)
		_HairLineColor ("Hair Line Color", Color) = (0,0,0,0) 
		
		[HideInInspector] _SrcBlend("__src", FLOAT) = 1.0
		[HideInInspector] _DstBlend("__dst", FLOAT) = 0.0
		[HideInInspector] _ZWrite("__zw", FLOAT) = 1.0
		[HideInInspector] _Stencil ("Stencil ID", Float) = 0
		
		_SceneColor("SceneColor", Vector) = (0,0,0,1)
		
		_SSRimColor ("SSRim Color",Color) = (1,1,1,0.3)
		_SSRimParam0("x:Distance Threshold w:RimScale",Vector) = (0.1,0,0,2)
		
		[HideInInspector] _OLZWrite ("_OutlineZWrite", Float) = 0
		[HideInInspector] _ShadowPos("ShadowPos", Vector) = (0,0,0,0)
		
	}

	HLSLINCLUDE

	#define UNIFORM_PCH_OFF
	#define UNITY_PERMATERIAL_DECLARED
	#include "../API/CommonAPI.hlsl"
	
	CBUFFER_START(UnityPerMaterial)
		float4 _UVST0;
		float4 _UVST1;
		float4 _Color;//only for custom effect
		float4 _Color0;
		float4 _Color1;
		float4 _Param;
		float4 _Param0;
		float4 _Param1;
		float4 _Param2;
		float4 _Param4;
		float4 _Param5;
		float4 _PBRParam;
		float4 _Color3;
		float4 _Color4;
		float4 _Color5;
		float4 _Color6;
		float4 _HeightGradientColor;
		float4 _SceneColor;
		float4 _ShadowPos;
		float4 _ColorOutline;
		float4 _MatEffectOutlineParam;
		float4 _SSRimColor;
		float4 _SSRimParam0;
		float4 _FaceShadowParam;
		half4 _HairLineColor;
		half4 _RimColortoon;
		float _DitherTransparency_On;
	CBUFFER_END
	
		TEX2D_SAMPLER(_MainTex);
		TEX2D_SAMPLER(_MainTex1);

		TEX2D_SAMPLER(_ProcedureTex0);
		TEX2D_SAMPLER(_ProcedureTex2);
		TEX2D_SAMPLER(_ProcedureTex3);

	    float4 _GlobalFaceShadowParam;
		#define _MainColor _Color0
		
		#define ROLE_EFFECT
		//feature
		#define _MAIN_COLOR
		#define _UV_SCALE2
		#define _UV_SCALE
		#define _CUSTOM_UV2
		
		#define _CUSTOMSHADOW
		
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

		#define _STYLE_HAIR
		#define _StyleHairLightMap _ProcedureTex3
		#define _StyleHairSpecualrColor (_Color1)
		#define _StyleHairSpeculrInt (_Color1.w)
		#define _StyleHairLightWith (_Param4.x)
		#define _StyleHairLightLength (_Param4.y)
		#define _StyleHairFeather (_Param4.z)
		#define _StyleHairThreshold (_Param4.w)

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
			#define _DarkRimCutLocation 0
			#define _DarkRimCutSmoothness 0
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

		Pass
		{
			Name "UniversalForward"
			Tags{"LightMode" = "UniversalForward" }
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
				// #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
				// #pragma multi_compile _ _SMARTSOFTSHADOW_ON
				#define _SMARTSOFTSHADOW_ON
				#define _ROLE_ADDITIONAL_LIGHT_ON
				// #pragma multi_compile _ _PARKOUR

			    #pragma multi_compile _SHADER_LEVEL_HIGH _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW _SHADER_LEVEL_VERY_LOW

				#ifdef _SM_4
					#pragma target 5.0
				#else//!_SM_4
					#pragma target 3.0
				#endif//_SM_4

				#define ROLE_SHADER_LEVEL
			    #include "../../Include/ShaderLevel.hlsl"
			
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

				#include "../Include/Pragam_Head.hlsl"
			
				#define _CUSTOM_EFFECT
				#define _CUSTOM_VERTEX_PARAM
				#define _VERTEX_COLOR
				#define _ROLE_HEIGHT_GRADIENT
				#define _SRP_DEBUG
			
			
			/* Add by: Takeshi
             * Date:   21/9/02
             * Modify: Defining this macro to change calculation of SM_Ramp
             * Notice: This solution should change modeling and texture painting at the same time,so leaving it to next game version.*/
                #define _SPHERE_LIGHTING
			/* End add*/
			
				#include "Role_Head.hlsl"
				#include "../Include/URP_LightingHead.hlsl"

				inline void CustomVertex(in FVertexInput Input,inout FInterpolantsVSToPS Interpolants)
				{
					TransformHeightGradient(Input, Interpolants);
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
						_MainLightDir.xyz, shadowLightColor, ShadowData,_ShadowColor.xyz);

					SM_Ramp(FragData,MaterialData,LC, Shadow.y, DirectDiffuse DEBUG_PARAM);
					// UNITY_BRANCH
					// if(_AddLightEnable)
					// {
					// 	#define specScale1 _SpecMult*0.5f
					// 	FLightingContext LC1 = InitLight(FragData,MaterialData,specScale1,
					// 	_AddLightDir.xyz,_AddLightColor.xyz,DefaultShadowData(),1);
					// 	SM_SupplementDiffuse(LC1,DirectDiffuse DEBUG_PARAM);
					// 	DEBUG_CUSTOMDATA_PARAM(LC1, LC1)
					// }
							
					SM_StyleHair(FragData,MaterialData,GetFadeShadow(ShadowData),
							_MainLightDir.xyz, shadowLightColor,
							_MainLightDir.xyz,
							DirectSpecular DEBUG_PARAM);

					MaterialData.BaseColor.a = 1;
				}

				#include "../Include/URP_Vertex.hlsl"
				#include "../../Include/ShaderDebug.hlsl"
				#include "../Include/URP_Pixel.hlsl"

				//only editor use
				//#pragma shader_feature_local _ _DEBUG_APP
				#pragma shader_feature_local _ _PBS_FROM_PARAM
				
				//render type
				#pragma shader_feature_local _ _ALPHA_BLEND _ALPHA_TEST

				//lighting
				//#pragma shader_feature_local _ _SHADOW_MAP
				#pragma shader_feature_local _ _EXTRA_SHADOW
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
//            // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//            // TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
//			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
//            #include "../Include/Pragam_Head.hlsl"
//            #include "Role_Head.hlsl"
//            
//			#include "Role_Simple_Screen_Space_Rim.hlsl"
//            
//            ENDHLSL
//        }
		
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
			// #pragma multi_compile _ _PARKOUR
			#include "URP_Role_DepthOnly.hlsl"

            ENDHLSL
        }
		
//		Pass
//		{
//			Name "ShadowCaster"
//			Tags{"LightMode" = "ShadowCaster"}
//
//			ZWrite On
//			ZTest LEqual
//			ColorMask 0
//			Cull[_Cull]
//
//			HLSLPROGRAM
//			//#pragma exclude_renderers gles gles3 glcore
//			#pragma target 4.5
//
//			// -------------------------------------
//			// Material Keywords
//			#pragma shader_feature_local_fragment _ALPHATEST_ON
//			#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
//
//			//--------------------------------------
//			// GPU Instancing
//			#pragma multi_compile_instancing
//			#pragma multi_compile _ DOTS_INSTANCING_ON
//
//			#pragma vertex ShadowPassVertex
//			#pragma fragment ShadowPassFragment
//
//			#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
//			#include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
//			ENDHLSL
//		}
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

//		//UsePass "Hidden/Custom/Tools/OutLine/OUTLINE"	
//		Pass
//		{
//			Name "OverdrawForwardBase"
//			Tags{"LightMode" = "OverdrawForwardBase"}
//
//			Blend One One
//			ZWrite[_ZWrite]
//			Cull Off
//			Stencil
//			{
//				Ref[_Stencil]
//				Comp equal
//			}
//			CGPROGRAM
//
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
//			ENDCG
//		}
//		Pass
//		{
//			Name "OverdrawOutline"
//			Tags{"LightMode" = "OverdrawOutline"}
//
//			Blend One One
//			Cull Front
//			Offset 1,1
//			Stencil
//			{
//				Ref[_Stencil]
//				Comp equal
//			}
//			HLSLPROGRAM
//
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

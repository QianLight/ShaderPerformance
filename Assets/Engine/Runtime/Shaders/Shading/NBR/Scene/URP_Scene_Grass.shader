Shader "URP/Scene/Grass" {
    Properties {
        [NoScaleOffset]_MainTex ("Base Tex", 2D) = "white" {}
        _Color0("Main Color", Color) = (1,1,1,1)
        [NoScaleOffset]_MainTex1 ("Color Map", 2D) = "white" {}

        _Param0("xyz:ss diffuse w:pivot wind", Vector) = (1,0,1,0)
        _Param1("xyz:ss spec w:occ", Vector) = (1,0.5,0,0.5)
        _Param2("xyz:normalParam ", Vector) = (25,0,0,0)
        [HDR]_Color1("Dark Color", Color) = (0,0,0,0)

        _Param3("x:GustingStrength y:GustingFrequency z:GustingWave", Vector) = (0.01,1,0,0)

        _PBRParam("", Vector) = (0,0,0,0)
		_UVST0("", Vector) = (0,0,0,0)
        _GrassCollisionParam("x:_PushAngle y:_PushValue z:null w:null", Vector) = (90,1,0,0)
        _Param("", Vector) = (0,0,0,0)
        _Color("Effect Color", Color) = (1,1,1,1)
        [NoScaleOffset]_CustomLightmap("LightMap", 2D) = "" {}

        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _DebugMode("__debugMode", Float) = 0.0
    }

    HLSLINCLUDE
    #define _INPUT_UV2
    #define _MAIN_COLOR
    #define _WORLD_UV_OFFSET
    #define _NO_NORMAL_MAP
    #define _SIMPLE_NORMAL
    #define _SCENE_EFFECT
    #define _VERTEX_GI
    #define _NO_COMMON_EFFECT
    #define _USE_CUSTOM_SHADOW_BIAS
    #define _CUSTOM_SHADOW_BIAS 0.01
    #define _CUSTOM_LOD	
    #define _NO_MRT
    ENDHLSL

    SubShader {
        Tags {
            "RenderType" = "Opaque"
        	"RenderPipeline" = "UniversalPipeline"
        	"UniversalMaterialType" = "Lit"
        	"IgnoreProjector" = "True"
        	
        }
        ZWrite[_ZWrite]
        Cull Off

		Pass
		{
			Name "FORWARD"
			Tags{ "LightMode" = "UniversalForward" }
			Blend[_SrcBlend][_DstBlend]
			
			HLSLPROGRAM

			    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"			
				#include "../Include/Pragam_Head.hlsl"
				#pragma target 4.5
				#pragma multi_compile _ _SHADER_LEVEL_HIGH

				#define UNIFORM_PCH_OFF
				CBUFFER_START(UnityPerMaterial)
							float4 _MainTex_ST;
							float4 _Param;
							float4 _Param0;
							float4 _Param1;
							float4 _Param2;
							float4 _Param3;
							float4 _Param5;
							float4 _Param6;
							float4 _GrassCollisionParam;
							float4 _Color;
							float4 _Color0;
							float4 _Color1;
							float4 _Color2;
							float4 _Color5;
							float4 _PBRParam;
							float4 _UVST0;
							float _DebugMode;
				CBUFFER_END

				TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);
				TEXTURE2D(_MainTex1);            SAMPLER(sampler_MainTex1);
				TEXTURE2D(_CustomLightmap);            SAMPLER(sampler_CustomLightmap);
				TEXTURE2D(_ProcedureTex3);            SAMPLER(sampler_ProcedureTex3);
				TEXTURE2D(_ProcedureTex4);            SAMPLER(sampler_ProcedureTex4);
                #define _MainColor _Color0
				/////////////////////////////////Lod/////////////////////////////////
				//High
				#if defined(_LOD0)
					#define _CUSTOM_VERTEX_OFFSET
				    #define _EXTRA_SHADOW
				//	#define _GRASS_COLLISION
				#elif defined(_LOD1)
					#define _CUSTOM_VERTEX_OFFSET
				#endif

				//Mid
				#if defined(_LOD_M0)
					#define _CUSTOM_VERTEX_OFFSET
				#endif

				//Low
				/////////////////////////////////Lod/////////////////////////////////

	            #define URP_BASE
	            #define REDEFINE_URP
			
				#include "../Scene/Scene_Head.hlsl"
				#include "Wind_Effect.hlsl"
	            #include "../Include/URP_LightingHead.hlsl"

				#define _GustingStrength _Param3.x
				#define _GustingFrequency _Param3.y
				#define _Swinging _Param3.z
				#define _Blend _Param3.w

			
				FLOAT3 CustomWindEffect( inout FVertexInput Input,FLOAT4 WorldPosition
				#ifdef _ENABLE_DEBUG
					,inout FLOAT4 debugData
				#endif
				)
				{
					FLOAT3 offset = 0;
					UNITY_BRANCH
					if(_AmbientWindSpeed>0.01)
					{
						FLOAT hBlend = saturate(Input.uv2.y)*0.01;
						FLOAT2 WaveUV = (WorldPosition.xz*_GustingFrequency*0.01)+(_TimeParameters.x *_AmbientWindSpeed *_GustingFrequency * 0.01) *_AmbientWindDir.xz;
						FLOAT WaveTex = SAMPLE_TEX2D_LOD(_AmbientWind,WaveUV,0).r;
						FLOAT WaveScale = WaveTex*_GustingStrength*hBlend;
						FLOAT Wave = sin(_AmbientWindSpeed * _TimeParameters.x );
						Wave = lerp(Wave* 0.5 + 0.5, Wave, _Swinging);
						Wave = Wave * hBlend * _Blend;
						offset.xz = Wave + WaveScale;
					}
					//#ifdef _GRASS_COLLISION
						UNITY_BRANCH
					if(_GRASS_COLLISION){									
							#ifndef _FLOWER						
							    FLOAT2 UV = saturate((WorldPosition.xyz - _CollisionCenter.xyz).xz/_CollisionRadiusInv + FLOAT2(0.5,0.5));
								FLOAT4 CollisionOffset = SAMPLE_TEX2D_LOD(_CollisionTex, UV,0);						
								FLOAT3 dirctions=(FLOAT3(CollisionOffset.x,0,CollisionOffset.y)*2-FLOAT3(1,0,1))*CollisionOffset.z*_PushValue;													
								offset += dirctions;							
								offset *= Input.uv0.y; 
							#endif
						}
				//	#endif//_GRASS_COLLISION
					return offset;
				}

				#define _RandomColorTex _MainTex1
				#define _USE_RANDOM_COLOR _Param2.w>0.5
				inline FLOAT2 WorldPositionToGroundTexUV(FLOAT3 world, FLOAT2 offset, FLOAT2 scale)
				{
					return (world.xz - offset) * scale;
				}

				FLOAT hash12(FLOAT2 p)
				{
					FLOAT3 p3  = frac(FLOAT3(p.xyx) * .1031);
					p3 += dot(p3, p3.yzx + 33.33);
					return frac((p3.x + p3.y) * p3.z);
				}      
				
				inline void MassiveBaseColor(in FFragData FragData,inout FMaterialData MaterialData)
				{
					FLOAT2 uv = GET_FRAG_UV;
					FLOAT4 color =  SAMPLE_TEX2D(_MainTex, uv);
					UNITY_BRANCH
					if(_USE_RANDOM_COLOR)
					{
						FLOAT3 Pivot = mul(_objectToWorld, FLOAT4(0,0,0,1)).xyz;
						FLOAT2 randomUV = FLOAT2( hash12(Pivot.xz*100),uv.y);
						FLOAT3 randomColor = SAMPLE_TEX2D(_RandomColorTex, randomUV).xyz;
						color.xyz = randomColor;
					}		
					
					MaterialData.BaseColor = color*_MainColor;	
					MaterialData.DyeColor = MaterialData.BaseColor.rgb;
				}
                #define CalcCustomBaseColor MassiveBaseColor
				#define _CUSTOM_BASECOLOR
				//---------------------------------------------------------------------------------
				inline FLOAT3 MassivePBS(in FFragData FragData,inout FMaterialData MaterialData,out FLOAT2 rm)
				{
					rm = FLOAT2(1,0);
					return FLOAT3(0.5,0.5,1);
				}

				#define CalcCustomPBS MassivePBS
				#define _CUSTOM_PBS

				//---------------------------------------------------------------------------------
				FLOAT GetMassiveShadowMapMask(in FFragData FragData,in FMaterialData MaterialData)
				{
					return 1;
				}
				#define GetCustumShadowMapMask GetMassiveShadowMapMask
				#define _CUSTOM_SHADOW_MAP_MASK

				//---------------------------------------------------------------------------------
				FLOAT CalcMassiveCustomAddNoL(in FFragData FragData,in FMaterialData MaterialData,FLOAT3 lightDir)
				{
					return 1;
				}
				#define CalcCustomAddNoL CalcMassiveCustomAddNoL
				#define _CUSTOM_ADDNOL
				//--------------------------------------------------------------------------------
				void CalcMassiveLighting(FFragData FragData, FMaterialData MaterialData, FShadowData ShadowData, FLOAT ShadowMask, inout FLOAT3 DirectDiffuse,inout FLOAT3 DirectSpecular DEBUG_ARGS)
				{
					//FLOAT2 Shadow = ShadowData.Shadow.xy;
					//CalcShadowColor(_MainLightColor.xyz,Shadow DEBUG_PARAM);
					SM_SS(FragData,MaterialData,_MainLightDir.xyz,_MainLightColor.xyz, ShadowData,DirectDiffuse,DirectSpecular,_Param0,_Color1, _Param2.x,
						 _Param1.x,_Param1.y,_Param1.z,_Param1.w,_Param2.y,_Param2.z
						DEBUG_PARAM);
				}
				#define CustomLighting CalcMassiveLighting
				#define _CUSTOM_LIGHT
				//---------------------------------------------------------------------------------
				#include "../Include/URP_Vertex.hlsl"
				#include "../Include/URP_Pixel.hlsl"

				//debug
				//#pragma shader_feature_local _ _DEBUG_APP

			    // Universal Pipeline keywords
	            #pragma shader_feature_local _ _MAIN_LIGHT_SHADOWS
	            #pragma shader_feature_local _ _MAIN_LIGHT_SHADOWS_CASCADE

				//#pragma shader_feature_local _ _SMARTSOFTSHADOW_ON
				#define _SMARTSOFTSHADOW_ON 1
				#pragma shader_feature_local _ _SHADOWPRO_ON

	            #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
	            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
	            #pragma multi_compile _ SHADOWS_SHADOWMASK
	            //----------------------

				//render type
				#pragma shader_feature_local _ _ALPHA_BLEND _ALPHA_TEST

				#pragma shader_feature_local _ _INSTANCE

				#pragma shader_feature_local _ _FLOWER

				#pragma shader_feature_local _ _LOD0 _LOD1 _LOD2 _LOD_FAR
				#pragma shader_feature_local _ _LOD_M0 _LOD_M1 _LOD_M2 _LOD_MFAR
				#pragma shader_feature_local _ _LOD_L0 _LOD_L1 _LOD_L2 _LOD_LFAR

				#pragma vertex vertForwardBase
				#pragma fragment fragForwardBase

			ENDHLSL
		}	
    }
}
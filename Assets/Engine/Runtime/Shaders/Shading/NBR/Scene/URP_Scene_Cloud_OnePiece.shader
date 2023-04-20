
Shader "URP_Scene_Cloud_OnePiece"
{
	Properties
	{
		[NoScaleOffset]_MainTex1("LengthTex", 2D) = "white" {}
		_Param1("UVParam(xy:speed,zw:tiling)", Vector) = (0.1,0.1,2,2)
		[NoScaleOffset]_AlphaTex3D ("AlphaNoise", 2D) = "white" {}
		_Param0("UVParam(xy:speed,zw:tiling)", Vector) = (0.1,0.1,8,8)
		_Color0("Main Color", Color) = (1,1,1,1)
		_Color1("Dark Color", Color) = (0,0,0,0) 
		_ShadowIntensity("ShadowIntensity", Range(0,1)) = 0.4

		_Length("z:_FurLengthMin w:_FurLengthMax", Vector) = (0,0,0.1,1)
		_CloudIntensity("x:Intensity,y:OpaqueDist", Vector) = (2,20,0,0)
		_SSS("x:LightColor,y:SSSRange,z:SSSIntensity", Vector) = (0.1,1,1,0)
	}

	SubShader
	{
		LOD 0
	
		Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent-1" }
		
		Cull Back
		AlphaToMask Off
		HLSLINCLUDE
		#pragma target 2.0
		ENDHLSL

		
		Pass
		{
			
			Name "CloudTransparent"
			Tags { "LightMode"="UniversalForward" "RenderType"="Transparent" }
			
			Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			ZTest LEqual
			ColorMask RGBA
			

			HLSLPROGRAM
			#pragma multi_compile_instancing
			#pragma multi_compile _FX_LEVEL_HIGH _FX_LEVEL_MEDIUM _FX_LEVEL_LOW
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
		    //#pragma multi_compile_fragment _ _HUAWEI

			#if defined(_FX_LEVEL_HIGH)
				#define REQUIRE_DEPTH_TEXTURE 1
				#define _READ_DEPTH
                #define _HEIGHT_TEX
            #elif defined(_FX_LEVEL_MEDIUM)
                #define _HEIGHT_TEX
            #endif 

			#define MAIN_LIGHT_CALCULATE_SHADOWS
			#define _SMARTSOFTSHADOW_ON 1
			
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "../Include/Fog.hlsl"
			#include "Assets/Engine/Runtime/Shaders/Shading/Include/SmartShadow.hlsl"

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos 	: SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float2 uv 		: TEXCOORD1 ;
				float4 worldNormal : TEXCOORD2;//xyz存法线，w存深度
				float4 screenPos : TEXCOORD3;
				

				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			sampler2D _MainTex1;
			sampler2D _AlphaTex3D;
			CBUFFER_START(UnityPerMaterial)
			float4 _Param0;
			float4 _Param1;
			
			float4 _Color0;
			float4 _Color1;
			float4 _CloudIntensity;
			float4 _Length;
			float4 _SSS;
			float _ShadowIntensity;
			CBUFFER_END

			#define _FurLengthTex _MainTex1
			#define _FurLengthMin _Length.z
			#define _FurLengthMax _Length.w

			#define _UseLight _SSS.x;
			#define _BackSSSRange _SSS.y
			#define _BackSSSInt _SSS.z	

			

			uniform float4 _CameraDepthTexture_TexelSize;


						
			VertexOutput VertexFunction ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

		
				o.worldNormal.xyz = TransformObjectToWorldDir(v.normal);
				o.worldNormal.w = -TransformWorldToView(TransformObjectToWorld( v.vertex.xyz )).z;

				o.uv = v.uv;

				#ifdef _HEIGHT_TEX
					float2 uv = ( v.uv.xy * _Param1.zw ) + ( _Param1.xy * _Time.x );
					float furLength = tex2Dlod(_FurLengthTex, float4(uv, 0,0)).r;
				#else
					float furLength = 0.5;
				#endif
				furLength = lerp(_FurLengthMin, _FurLengthMax, furLength);
			//	furLength *= offset;


				float3 positionWS = TransformObjectToWorld( v.vertex.xyz ) + o.worldNormal * furLength;
	
				float4 positionCS = TransformWorldToHClip( positionWS );
				o.screenPos = ComputeScreenPos(positionCS);
				o.worldPos = positionWS;


				o.clipPos = positionCS;
				return o;
			}
			
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}

			FLOAT3 CalcColorEffect(float NdotV,in FLOAT3 color)
			{
				FLOAT3 c = color;
				#ifndef _NO_COLOR_EFFECT
					//add color
					UNITY_BRANCH
					if(_SceneColor.a < 0.45)
					{
						//c = color * _Color.rgb;
						c = saturate(lerp(_SceneColor.rgb*_SceneColor.a,_SceneColor.rgb,dot(c.rgb,0.333)));
					}
					//rim
					UNITY_BRANCH
					if(_SceneColor.a > 0.49&&_SceneColor.a < 0.91)
					{
						float temp = saturate(1 - NdotV);
						c = color + temp * temp * (40 *_SceneColor.a - 20) * _SceneColor.rgb;
					}
				#endif//_NO_COLOR_EFFECT

				return c;
			}

			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				float3 worldNormal = IN.worldNormal ;//TransformObjectToWorldDir(IN.worldNormal);


				float2 uv = ( IN.uv.xy * _Param0.zw ) + ( _Param0.xy * _Time.x );
				float noise = tex2D( _AlphaTex3D, uv).r;
				//worldNormal.y *= lerp(1, noise, 0.1) ;
				worldNormal.y *= 0.1 * noise + 0.9;

				float3 WorldPosition = IN.worldPos;
				float3 WorldView = normalize(IN.worldPos - _WorldSpaceCameraPos);
				
				float3 BackDir = normalize(worldNormal *_BackSSSRange + _MainLightPosition.xyz);
				FLOAT BackSSS = saturate(dot(-WorldView,-BackDir));
				BackSSS = saturate(pow((BackSSS),2)*_BackSSSInt);

				float NdotL = saturate(dot( _MainLightPosition.xyz , worldNormal));
				float NdotV = saturate(dot( -WorldView , worldNormal));

				float fresnel = pow(1 - NdotV, 8);
			//	return float4(fresnel.xxx, 1);


				FLOAT SmoothNV = saturate( pow( NdotV, 2)) ;		
				
				float shadow = 1;
				//实时阴影
				#ifdef _MAIN_LIGHT_SHADOWS
					float4 ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					half realtimeShadow = saturate(MainLightRealtimeShadow(ShadowCoords));
					half shadowFade = GetShadowFade(WorldPosition);
					shadow = lerp(realtimeShadow, 1, saturate(shadowFade));
				#endif
				
				//SmartShadow阴影
				float smartShadow = GetSmartShadow(_MainLightPosition.xyz, worldNormal.xyz, float4(WorldPosition,1), _SmartShadowIntensity);
				shadow = min(shadow, smartShadow);
				shadow = lerp(1,shadow,_ShadowIntensity);
				
				float3 Color = lerp(_Color1, _Color0 ,  noise * NdotL * shadow);
			
				Color = (Color + BackSSS + (SmoothNV + fresnel) * 0.2) * lerp(1, _MainLightColor.xyz, _SSS.x);
				Color.xyz = CalcColorEffect(NdotV, Color.xyz);

				float4 fog = ApplyFog(WorldPosition); 
				Color.rgb = lerp(Color.rgb, fog.rgb, fog.a);

			#ifdef _READ_DEPTH
				float4 screenPos = IN.screenPos / IN.screenPos.w;
				screenPos.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? screenPos.z : screenPos.z * 0.5 + 0.5;
				float depthTex = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( screenPos.xy ), _ZBufferParams);
				float Alpha = max(0, depthTex - IN.worldNormal.w) * _CloudIntensity.x;// + noise * 0.2;
				//Alpha *= 1 - (1 - noise) * 0.2;
				Alpha *= 0.8 + 0.2 * noise;
			#else
				float Alpha = 1;//noise * noise;
			#endif
				
			//	Alpha *= (1 + NdotV * 100);
				Alpha = saturate(Alpha);
			//	Color = Alpha.xxx;

				return half4( Color, Alpha );
			}

			ENDHLSL
		}

		Pass
		{
			
			Name "CloudOpaque"
			Tags { "LightMode"="SRPDefaultUnlit" "RenderType"="Opaque" "Queue"="Geometry"}
			
			ZWrite On
			ZTest LEqual
			ColorMask RGBA
			

			HLSLPROGRAM
			#pragma multi_compile_instancing
			#pragma multi_compile _FX_LEVEL_HIGH _FX_LEVEL_MEDIUM _FX_LEVEL_LOW
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			
			#if defined(_FX_LEVEL_HIGH)
                #define _HEIGHT_TEX
            #elif defined(_FX_LEVEL_MEDIUM)
                #define _HEIGHT_TEX
            #endif 
			
			#define MAIN_LIGHT_CALCULATE_SHADOWS
			#define _SMARTSOFTSHADOW_ON 1

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "../Include/Fog.hlsl"
			#include "Assets/Engine/Runtime/Shaders/Shading/Include/SmartShadow.hlsl"

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos 	: SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float2 uv 		: TEXCOORD1 ;
				float4 worldNormal : TEXCOORD2;//xyz存法线，w存深度
				float4 screenPos : TEXCOORD3;
				

				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			CBUFFER_START(UnityPerMaterial)
			float4 _Param0;
			float4 _Param1;

			float4 _Color0;
			float4 _Color1;
			float4 _CloudIntensity;
			float4 _Length;
			float4 _SSS;
			float _ShadowIntensity;
			CBUFFER_END
			
			sampler2D _MainTex1;
			sampler2D _AlphaTex3D;
			


			#define _FurLengthTex _MainTex1
			#define _FurLengthMin _Length.z
			#define _FurLengthMax _Length.w

			#define _UseLight _SSS.x;
			#define _BackSSSRange _SSS.y
			#define _BackSSSInt _SSS.z	

	
						
			VertexOutput VertexFunction ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.worldNormal.xyz = TransformObjectToWorldDir(v.normal);
				o.worldNormal.w = -TransformWorldToView(TransformObjectToWorld( v.vertex.xyz )).z;

				o.uv = v.uv;

				#ifdef _HEIGHT_TEX
					float2 uv = ( v.uv.xy * _Param1.zw ) + ( _Param1.xy * _Time.x );
					float furLength = tex2Dlod(_FurLengthTex, float4(uv, 0,0)).r;
				#else
					float furLength = 0.5;
				#endif
				furLength = lerp(_FurLengthMin, _FurLengthMax, furLength);
			//	furLength *= offset;


				float3 positionWS = TransformObjectToWorld( v.vertex.xyz ) ;
				//不透明云四周抬高，遮挡半透明，以避免半透明自排序问题
				float dist = distance(_WorldSpaceCameraPos, positionWS);
				positionWS += smoothstep(_CloudIntensity.y, _CloudIntensity.y + 10, dist) * o.worldNormal * (furLength + 0.01);

				float4 positionCS = TransformWorldToHClip( positionWS );
				o.screenPos = ComputeScreenPos(positionCS);
				o.worldPos = positionWS;
				o.clipPos = positionCS;
				return o;
			}
			
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}

			FLOAT3 CalcColorEffect(float NdotV,in FLOAT3 color)
			{
				FLOAT3 c = color;
				#ifndef _NO_COLOR_EFFECT
					//add color
					UNITY_BRANCH
					if(_SceneColor.a < 0.45)
					{
						//c = color * _Color.rgb;
						c = saturate(lerp(_SceneColor.rgb*_SceneColor.a,_SceneColor.rgb,dot(c.rgb,0.333)));
					}
					//rim
					UNITY_BRANCH
					if(_SceneColor.a > 0.49&&_SceneColor.a < 0.91)
					{
						float temp = saturate(1 - NdotV);
						c = color + temp * temp * (40 *_SceneColor.a - 20) * _SceneColor.rgb;
					}
				#endif//_NO_COLOR_EFFECT

				return c;
			}

			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				float3 worldNormal = IN.worldNormal ;//TransformObjectToWorldDir(IN.worldNormal);


				float2 uv = ( IN.uv.xy * _Param0.zw ) + ( _Param0.xy * _Time.x );
				float noise = tex2D( _AlphaTex3D, uv).r;
				worldNormal.y *= lerp(1, noise, 0.1) ;


				float3 WorldPosition = IN.worldPos;
				float3 WorldView = normalize(IN.worldPos - _WorldSpaceCameraPos);
				
				float3 BackDir = normalize(worldNormal *_BackSSSRange + _MainLightPosition.xyz);
				FLOAT BackSSS = saturate(dot(-WorldView,-BackDir));
				BackSSS = saturate(pow((BackSSS),2)*_BackSSSInt);

				float NdotL = saturate(dot( _MainLightPosition.xyz , worldNormal));
				float NdotV = saturate(dot( -WorldView , worldNormal));

				float fresnel = pow(1 - NdotV, 8);
			//	return float4(fresnel.xxx, 1);

				float shadow = 1;
				//实时阴影
				#ifdef _MAIN_LIGHT_SHADOWS
					float4 ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					half realtimeShadow = MainLightRealtimeShadow(ShadowCoords);
					half shadowFade = GetShadowFade(WorldPosition);
					shadow = lerp(realtimeShadow, 1, saturate(shadowFade));
				#endif
				//SmartShadow阴影
				float smartShadow = GetSmartShadow(_MainLightPosition.xyz, worldNormal.xyz, float4(WorldPosition,1), _SmartShadowIntensity);
				shadow = min(shadow, smartShadow);
				shadow = lerp(1, shadow, _ShadowIntensity);
				
				FLOAT SmoothNV = saturate( pow( NdotV, 2)) ;		
				float3 Color = lerp(_Color1, _Color0 , noise * NdotL * shadow);
			
				Color = (Color + BackSSS + (SmoothNV + fresnel) * 0.2) * lerp(1, _MainLightColor.xyz, _SSS.x);
			
				Color.xyz = CalcColorEffect(NdotV, Color.xyz);				

				float4 fog = ApplyFog(WorldPosition); 
				Color.rgb = lerp(Color.rgb, fog.rgb, fog.a);

				float Alpha = 1;
				return half4( Color, Alpha );
			}

			ENDHLSL
		}
	}
	Fallback "Hidden/InternalErrorShader"
}

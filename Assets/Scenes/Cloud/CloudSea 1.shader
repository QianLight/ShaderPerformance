Shader "URP_Scene_CloudSea_OnePiece"
{
	Properties
	{
		_BaseColor("BaseColor", Color) = (0,0,0,0)
		_CloudTexture("Cloud Texture", 2D) = "white" {}
		_NoiseTex("Noise Tex", 2D) = "white" {}
		_LightColor("Light Color", Color) = (0.8490566,0.9014675,0.9433962,1)
		_DarkColor("Dark Color", Color) = (0.627451,0.764706,0.8745099,1)
		_CloudHeight("Cloud Height", Float) = 1
		_CloudDensity("Cloud Density", Float) = 0.4
		_Speed("Speed", Float) = 0
		_WorldTile("WorldTile", Float) = 300
		_Tile1_XYDir1_ZW("Tile1_XY/Dir1_ZW", Vector) = (6,6,1,1)
		_Tile2_XYDir2_ZW("Tile2_XY/Dir2_ZW", Vector) = (3,3,-1,-1)
		_RimIntensity("Rim Intensity", Range( 0 , 1)) = 0
		_RimRange("Rim Range", Float) = 2
		_DepthDistance("Depth Distance", Float) = 4.5
		_SkylineFadeWidth("Skyline Fade Width", Float) = 50
		_SkylineFadeDistance("Skyline Fade Distance", Float) = 93.2
		
		_ShadowIntensity("ShadowIntensity", Range(0,1)) = 0.2
	}

	SubShader
	{

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
		
		Cull Back
	
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			//Blend SrcAlpha OneMinusSrcAlpha, OneMinusDstColor One
			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite On
			ZTest LEqual


			HLSLPROGRAM

			#define REQUIRE_DEPTH_TEXTURE 1
			#define MAIN_LIGHT_CALCULATE_SHADOWS
			#define _SMARTSOFTSHADOW_ON 1
			
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x


			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Assets/Engine/Runtime/Shaders/Shading/NBR/Include/Fog.hlsl"
			#include "Assets/Engine/Runtime/Shaders/Shading/Include/SmartShadow.hlsl"
			
		

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 uv0 : TEXCOORD0;

			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float fogFactor : TEXCOORD1;
				float4 normalWS : TEXCOORD2;
				float4 screenPos : TEXCOORD3;
				float4 uv : TEXCOORD4;

				float4 shadowCoord:TEXCOORD5;
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Tile1_XYDir1_ZW;
			float4 _Tile2_XYDir2_ZW;
			float4 _BaseColor;
			float4 _DarkColor;
			float4 _LightColor;
			float _WorldTile;
			float _Speed;
			float _CloudHeight;
			float _RimRange;
			float _RimIntensity;
			float _CloudDensity;
			float _DepthDistance;
			float _SkylineFadeWidth;
			float _SkylineFadeDistance;

			float _ShadowIntensity;
			CBUFFER_END

			sampler2D _CloudTexture;
			uniform float4 _CameraDepthTexture_TexelSize;
			sampler2D _NoiseTex;

			

			VertexOutput vert ( VertexInput v )
			{
				VertexOutput o = (VertexOutput)0;


				float2 tile1 = float2(_Tile1_XYDir1_ZW.x , _Tile1_XYDir1_ZW.y);
				float2 direction1 = float2(_Tile1_XYDir1_ZW.z , _Tile1_XYDir1_ZW.w);
				float2 tile2 = float2(_Tile2_XYDir2_ZW.x , _Tile2_XYDir2_ZW.y);
				float2 direction2 = float2(_Tile2_XYDir2_ZW.z , _Tile2_XYDir2_ZW.w);

				float3 worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float2 worldUV = (float2(worldPos.x , worldPos.z))/ _WorldTile ;

				float time = _TimeParameters.x * _Speed;
				float noise1 = tex2Dlod( _CloudTexture, float4(  ( tile1 * worldUV ) + ( direction1 * time ) , 0, 0.0) ).r ;
				float noise2 = tex2Dlod( _CloudTexture, float4(  ( tile1 / tile2 ) * worldUV  + ( direction2 * time ) , 0, 0.0) ).r;
				float finalNoise = saturate( max(noise1 , noise2 ) );
				finalNoise = smoothstep( 0.0 , 1.0 , finalNoise);
				//finalNoise -= 0.5;
				float3 vertexOffset =  finalNoise * v.normal * _CloudHeight;
				v.vertex.xyz += vertexOffset;
				

				o.uv.xy = v.uv0.xy;
				o.uv.zw = 0;
				

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				o.screenPos = ComputeScreenPos(positionCS);
				o.worldPos = positionWS;


				o.normalWS.xyz = TransformObjectToWorldNormal(v.normal);
				o.normalWS.w = -TransformWorldToView(positionWS).z;//w存深度
				
				o.shadowCoord = TransformWorldToShadowCoord(positionWS);
				o.clipPos = positionCS;
				return o;
			}


			half4 frag ( VertexOutput IN  ) : SV_Target
			{

				float3 WorldPosition = IN.worldPos;
				
				float3 viewDirWS = normalize( _WorldSpaceCameraPos.xyz - WorldPosition );
				
				float3 normalWS = IN.normalWS.xyz;
				float ndotv = dot( viewDirWS , normalWS );
				float2 tile1 = (float2(_Tile1_XYDir1_ZW.x , _Tile1_XYDir1_ZW.y));
				float2 worldUV = (float2(WorldPosition.x , WorldPosition.z))/ _WorldTile ;

				float2 direction1 = float2(_Tile1_XYDir1_ZW.z , _Tile1_XYDir1_ZW.w);
				float time = _TimeParameters.x * _Speed;
				float2 tile2 = float2(_Tile2_XYDir2_ZW.x , _Tile2_XYDir2_ZW.y);
				float2 direction2 = float2(_Tile2_XYDir2_ZW.z , _Tile2_XYDir2_ZW.w);
				float noise = saturate( max( tex2D( _CloudTexture, ( ( tile1 * worldUV ) + ( direction1 * time ) ) ).r , tex2D( _CloudTexture, ( ( ( tile1 / tile2 ) * worldUV ) + ( direction2 * time ) ) ).r ) );

				float3 cloudCol = lerp( _DarkColor , _LightColor , noise);
				
				

				 float4 screenPos = IN.screenPos / IN.screenPos.w;
				 screenPos.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? screenPos.z : screenPos.z * 0.5 + 0.5;
				 float screenDepth = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( screenPos.xy ),_ZBufferParams);
				 float distanceDepth = abs( ( screenDepth - LinearEyeDepth( screenPos.z,_ZBufferParams ) ) / ( _DepthDistance ) );
				
				 float depth = IN.normalWS.w;
				 float cameraDepthFade = (( depth -_ProjectionParams.y - _SkylineFadeDistance ) / _SkylineFadeWidth);
				 cameraDepthFade = smoothstep( 0.0 , 1.0 , ( 1.0 - cameraDepthFade ));
				
				 float2 panner = ( 0.005 * _Time.y * float2( 1,-1 ) + IN.uv.xy);
				 float Alpha = ( saturate( ( min( ( noise + _CloudDensity ) , distanceDepth ) - tex2D( _NoiseTex, panner ).r ) ) * cameraDepthFade );
				
				//实时阴影
				float shadow = 1;
				#ifdef _MAIN_LIGHT_SHADOWS
					float4 ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					half realtimeShadow = saturate(MainLightRealtimeShadow(ShadowCoords));
					half shadowFade = GetShadowFade(WorldPosition);
					shadow = lerp(realtimeShadow, 1, saturate(shadowFade));
				#endif

				float4 loopPos = float4(WorldPosition,1);
				loopPos.z = loopPos.z % 100;
				//SmartShadow阴影
				float smartShadow = GetSmartShadow(_MainLightPosition.xyz, normalWS.xyz, loopPos, _SmartShadowIntensity);
				shadow = min(shadow, smartShadow);
				

				//云阴影
				
				//	float cloud = CloudShadowColor(WorldPosition,normalWS);
				//	shadow = min(cloud, shadow);//zh

				
				shadow = lerp(1, shadow, _ShadowIntensity);
				//float2 panner2 = ( 0.008 * _Time.y * float2( -1,-1 ) + IN.uv.xy * float2(3,3));
				 shadow = min(shadow ,Alpha) ;//
				 //return shadow;
				 float3 Color  =  _BaseColor * ( ( pow( ( 1.0 - ndotv ) , _RimRange ) * _RimIntensity ) + cloudCol ) * _MainLightColor * shadow  ;
				
				
				 float4 fog = ApplyFog(WorldPosition);
				 Color.rgb = lerp(Color.rgb, fog.rgb, fog.a);
				
				 return half4( Color, Alpha );
			}

			ENDHLSL
		}
	
	}
	Fallback "Hidden/InternalErrorShader"
	
}

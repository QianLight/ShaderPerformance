// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "URP/SFX/Fresnel_effect"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[ASEBegin]_Float0("Float 0", Range( -50 , 50)) = -0.6941177
		[HDR]_Color0("Color 0", Color) = (0,0,0,0)
		[ASEEnd]_Float2("Float 2", Range( -10 , 50)) = 1.15648
	}

	SubShader
	{
		LOD 0
		
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
		
		Cull Front
		AlphaToMask Off
		HLSLINCLUDE
		#pragma target 3.0

		ENDHLSL
	
		Pass
		{		
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend SrcAlpha One, SrcAlpha One
			ZWrite Off
			ZTest Less
			Offset 0,0
			ColorMask RGBA
			
			HLSLPROGRAM
			#pragma multi_compile_instancing

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_NORMAL


			struct VertexInput
			{
				half4 vertex : POSITION;
				half3 normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				half4 clipPos : SV_POSITION;
				half3 worldPos : TEXCOORD0;
				half4 normalWS : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			half4 _Color0;
			half _Float0;
			half _Float2;
			CBUFFER_END
						
			VertexOutput VertexFunction ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				half3 worldNormal = TransformObjectToWorldNormal(v.normal);
				o.normalWS.xyz = worldNormal;
				o.normalWS.w = 0;
				half3 defaultVertexValue = half3(0, 0, 0);
				half3 vertexValue = defaultVertexValue;
				v.vertex.xyz += vertexValue;

				half3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				half4 positionCS = TransformWorldToHClip( positionWS );

				o.worldPos = positionWS;

				o.clipPos = positionCS;
				return o;
			}

			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}

			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				float3 WorldPosition = IN.worldPos;

				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = normalize(ase_worldViewDir);
				half fresnelNdotV1 = dot( IN.normalWS.xyz, ase_worldViewDir );
				half fresnelNode1 = ( _Float0 + _Float2 * pow( 1.0 - fresnelNdotV1, 5.0 ) );
				half4 Color = ( _Color0 * fresnelNode1 );
				

				#ifdef _ALPHATEST_ON
					clip( Color.a - 0.5 );
				#endif

				
				return half4( Color );
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual
			AlphaToMask Off

			HLSLPROGRAM
			#pragma multi_compile_instancing
			#define ASE_SRP_VERSION 100500


			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_NORMAL


			struct VertexInput
			{
				half4 vertex : POSITION;
				half3 normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				half4 clipPos : SV_POSITION;
				half3 worldPos : TEXCOORD0;

				half4 normalWS : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			half4 _Color0;
			half _Float0;
			half _Float2;
			CBUFFER_END

			half3 _LightDirection;

			VertexOutput VertexFunction( VertexInput v )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float3 worldNormal = TransformObjectToWorldNormal(v.normal);
				o.normalWS.xyz = worldNormal;
				
				o.normalWS.w = 0;
				half3 defaultVertexValue = half3(0, 0, 0);
				half3 vertexValue = defaultVertexValue;
				v.vertex.xyz += vertexValue;


				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );

				o.worldPos = positionWS;

				float3 normalWS = TransformObjectToWorldDir( v.normal );

				half4 clipPos = TransformWorldToHClip( ApplyShadowBias( positionWS, normalWS, _LightDirection ) );

				#if UNITY_REVERSED_Z
					clipPos.z = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
				#else
					clipPos.z = max(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
				#endif


				o.clipPos = clipPos;

				return o;
			}
			
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );
				
				#ifdef _ALPHATEST_ON
					clip(Alpha - 0.5);
				#endif

				return 0;
			}

			ENDHLSL
		}

	
	}
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
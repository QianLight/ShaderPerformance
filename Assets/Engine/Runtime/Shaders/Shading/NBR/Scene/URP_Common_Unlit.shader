Shader "URP/Common_Unlit"
{
	Properties
	{
		[NoScaleOffset]_MainTex ("Base Tex", 2D) = "white" {}
		[HDR]_Color0("Main Color", Color) = (1,1,1,1)
		_Param1("SpecularScale:x Spec:y IBLScale:z Cutout:w", Vector) = (1,0.5,0,0.5)
		
		[HideInInspector] _DebugMode("__debugMode", Float) = 0.0
		[HideInInspector] _Stencil ("Stencil ID", Float) = 0
	}

	SubShader
	{
		LOD 0

		Tags {
            "RenderPipeline" = "UniversalPipeline" "RenderType" = "TransparentCutout" "Queue"="Geometry+1"  
        }
		
		Cull Off
		AlphaToMask Off
		
		HLSLINCLUDE
		#pragma target 4.5
		ENDHLSL

		Pass
        {
            Name "DepthPrepass"
            Tags
            {
                "LightMode" = "DepthPrepass"
            }
            Cull Off
            ColorMask 0
			ZWrite On
			
            HLSLPROGRAM
            #pragma target 4.5
        //    #define URP_BASE
        //    #define REDEFINE_URP

            #define _ALPHATEST_ON

            #pragma multi_compile_instancing
        //    #pragma multi_compile _ _INSTANCING_HIGH

            #define DEPTH_PRE_PASS
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

            #pragma vertex vert
            #pragma fragment frag
			
			struct VertexInput
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float4 uv : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			float4 _Color0;
			float4 _Param1;
			CBUFFER_END
			sampler2D _MainTex;
			
			VertexOutput vert ( VertexInput v )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.uv.xy = v.uv.xy;
				o.uv.zw = 0;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				o.worldPos = positionWS;
				o.clipPos = positionCS;
				return o;
			}


			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				float2 uv_MainTex = IN.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
	
				float4 rt0 = tex2D( _MainTex, uv_MainTex );
				float clipThreshold = 0.5; //_Param1.w
			//	clip(rt0.a - clipThreshold);
				
				#ifdef _ALPHATEST_ON
				AlphaDiscard(rt0.a, clipThreshold);
				#endif
				
				return 1;//float4(0.5,0.5,0.5,1);
			}
            ENDHLSL
        }
		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			ZWrite Off
            ZTest Equal
            Cull Off
			ColorMask RGBA

			HLSLPROGRAM
			#define _RECEIVE_SHADOWS_OFF 1
			#define _ALPHATEST_ON 1
			#pragma multi_compile_instancing
			#pragma multi_compile _DEBUG_MODE
			#pragma shader_feature_local _ _ALPHA_BLEND _ALPHA_TEST
			#pragma multi_compile _SHADER_LEVEL_HIGH _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW _SHADER_LEVEL_VERY_LOW
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Assets/Engine/Runtime/Shaders/Shading/NBR/Include/Fog.hlsl"

			struct VertexInput
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float4 uv : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			float4 _Color0;
			float4 _Param1;
			CBUFFER_END
			sampler2D _MainTex;
			float _HSREnable;

				
			VertexOutput VertexFunction ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.uv.xy = v.uv.xy;
				o.uv.zw = 0;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				o.worldPos = positionWS;
				o.clipPos = positionCS;
				return o;
			}
	
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}

			float _ImpostorAlpha;
			half4 _SceneColorAdjustmentParams;
			#define _SceneExposure _SceneColorAdjustmentParams.x
			
			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				float3 WorldPosition = IN.worldPos;

				float2 uv_MainTex = IN.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
	
				float4 rt0 = tex2D( _MainTex, uv_MainTex );
				rt0 *= _Color0 ;
				float clipThreshold = 0.5; //_Param1.w

				if (_HSREnable == 1)				
					clip(rt0.a - clipThreshold + 0.0h);
				
				//AlphaDiscard(rt0.a, clipThreshold);
				// UNITY_BRANCH
			 //    if (_ScatterEnable)
			 //    {
			 //        float depth01;
			 //        float3 scatter = GetScatterRGB(normalize(WorldPosition - _WorldSpaceCameraPos), WorldPosition, _WorldSpaceCameraPos, _MainLightDir0.xyz, depth01, _ProjectionParams, _CameraBackward);
			 //    	rt0.rgb = lerp(rt0.rgb, scatter, saturate(depth01 * 2));
			 //    }
				APPLY_FOG(rt0, WorldPosition.xyz);
				rt0.rgb *= _SceneExposure;
				rt0.rgb *= lerp(_SceneColor.rgb, 1, _SceneColor.a);
				return rt0;
			}

			ENDHLSL
		}	
		
    	Pass
		{
			Name "OverdrawF"
			Tags{"LightMode" = "OverdrawForwardBase"}

			Blend One One
			HLSLPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			
			struct Attributes
			{
				float4 vertex : POSITION;
			};
			
			struct Varyings
			{
				float4 vertex : SV_POSITION;
			};
			Varyings Vert(Attributes v)
			{
				Varyings o;
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );
				o.vertex = positionCS;
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
	Fallback "Hidden/InternalErrorShader"
}

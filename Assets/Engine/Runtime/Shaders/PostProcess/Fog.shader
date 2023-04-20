Shader "Hidden/PostProcessing/Fog"
{
    SubShader
    {
        Cull Off
		ZWrite Off
		ZTest Off
		Blend SrcAlpha OneMinusSrcAlpha

		// PreEffect
		Pass
		{
			CGPROGRAM

			#pragma target 3.0

			#if defined(_DEBUG_APP)&&!defined(SHADER_API_MOBILE)
			#define _ENABLE_DEBUG
			#endif

			#define PRECOMPUTE_SCATTER

			#include "UnityCG.cginc"
			#if defined(SHADER_API_D3D11)
				#include "../API/D3D11.hlsl"    
			// #elif defined(SHADER_API_D3D12)
			//     #include "API/D3D12.hlsl"
			#elif defined(SHADER_API_VULKAN) || defined(SHADER_API_SWITCH)
				#include "../API/Vulkan.hlsl"
			#elif defined(SHADER_API_METAL)
				#include "../API/Metal.hlsl"
			#else
				#include "../API/OpenGL.hlsl"
				#define ANDROID
			#endif

			half4 _MainLightColor0;
			half4 _MainLightDir0;

			#include "../Include/Fog.hlsl"

			#pragma multi_compile_local _ _FRAMEBUFFER_FETCH
			#pragma shader_feature_local _ _DEBUG_APP
			TEX2D_SAMPLER(_CameraDepthRT);
			//TEX2D_SAMPLER(_PrecomputeScatterTex);

			#if defined(_FRAMEBUFFER_FETCH)
				#define _FB_FETCH
			#else			
				TEX2D_SAMPLER(_GrabTex);
			#endif

			struct a2v
			{
				float3 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;    
				float3 ray : TEXCOORD1;
			};

			inline float2 TransformTriangleVertexToUV(float2 vertex)
			{
				float2 uv = vertex * 0.5 + float2(0.5, 0.5);
				return uv;
			}

			inline float DecodeFloatRG2(float2 enc)
			{
				float2 kDecodeDot = float2(1.0, 1 / 255.0);
				return dot(enc, kDecodeDot);
			}
			inline float DecodeFloatRGB2(float3 enc)
			{
				float3 kDecodeDot = float3(1.0, 1 / 255.0, 1 / 65025.0);
				return dot(enc, kDecodeDot);
			}
			inline float FetchFromDepthTex(TEX2D_ARGS(tex), float2 uv)
			{
				return DecodeFloatRGB2(SAMPLE_TEX2D(tex, uv).xyz);
			}
			v2f VertFog(a2v v)
			{
				v2f o = (v2f)0;
				o.vertex = float4(v.vertex.xy, 0.0, 1.0);
				o.texcoord = TransformTriangleVertexToUV(v.vertex.xy);
				#if UNITY_UV_STARTS_AT_TOP
					o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
				#endif
				o.ray = mul(unity_CameraInvProjection, float4(v.vertex.x,-v.vertex.y, 0, -1)).xyz;
				return o;
			}

			//// -----------------------------------------------------------------------------
			#if defined(_FB_FETCH)
			void FragFog (v2f i,inout float4 rt0 : SV_Target0,in float4 rt1 : SV_Target1)
			#else
			float4 FragFog(v2f i) : SV_Target
			#endif
			{
				float2 uv = i.texcoord;

				//return float4(SAMPLE_TEX2D(_PrecomputeScatterTex, uv).rgb, 1);

				#if defined(_FB_FETCH)
					float rawDepth = DecodeFloatRGB2(rt1.xyz);
				#else
					float rawDepth = FetchFromDepthTex(TEX2D_PARAM(_CameraDepthRT), uv);
				#endif
				float depth = LinearEyeDepth(rawDepth);
				float3 vpos = i.ray * depth;
				float3 wpos = mul(unity_CameraToWorld, float4(vpos,1)).xyz;

				float4 fogColor = ApplyFog(wpos);

				#if defined(_FB_FETCH)
					rt0 = fogColor;
				#else
					return fogColor;
				#endif
			}

			#pragma vertex VertFog
			#pragma fragment FragFog

			ENDCG
		}
    }
}

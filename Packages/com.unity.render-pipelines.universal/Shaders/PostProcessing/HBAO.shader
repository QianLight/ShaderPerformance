Shader "Hidden/Universal Render Pipeline/HBAO"
{
	SubShader
	{

		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "AO Pass"

			HLSLPROGRAM

			#include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

			#pragma vertex FullscreenVert
			#pragma fragment fragAO
			#pragma target 4.5

			struct v2f
			{
				float4 uv : TEXCOORD0;
			};
            #define SampleDepth(uv) SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uv)

	        float4 _CameraDepthTexture_TexelSize;
			float4 _AORadius;
			#define _MaxPixelRadius _AORadius.y

			float4 _UVToView;

			float4 _TargetScalePara;
			#define _Intensity _TargetScalePara.z
			#define _NegInvRadius2 _TargetScalePara.w

			float4 _DistanceFalloff;
			#define _AngleBiasValue _DistanceFalloff.z
			#define _AOmultiplier _DistanceFalloff.w

			sampler2D _HbaoNoiseTexture;
			float4 _HbaoNoiseTexture_TexelSize;

			inline float Falloff2(float distance, float radius)
			{
				float a = distance / radius;
				return clamp(1.0 - a * a, 0.0, 1.0);
			}

			inline float2 RotateDirections(float2 dir, float2 rot)
			{
				return float2(dir.x * rot.x - dir.y * rot.y,
					dir.x * rot.y + dir.y * rot.x);
			}

			inline float2 GetRayMarchingDir(float angle, float2 rand)
			{
				float sinValue, cosValue;
				sincos(angle, sinValue, cosValue);
				return RotateDirections(float2(cosValue, sinValue), rand);
			}

			inline float3 FetchViewPos(float2 uv, float eyeDepth)
			{
				return float3((uv * _UVToView.xy + _UVToView.zw) * eyeDepth, eyeDepth);
			}

			inline float3 FetchViewPos(float2 uv)
			{
				float depth = SampleDepth(uv * _TargetScalePara.xy);
				depth = LinearEyeDepth(depth, _ZBufferParams);
				return FetchViewPos(uv, depth);
			}

			inline float Falloff(float distanceSquare)
			{
				// 1 scalar mad instruction
				return distanceSquare * _NegInvRadius2 + 1.0;
			}

			inline float ComputeAO(float3 P, float3 N, float3 S)
			{
				float3 V = S - P;
				float VdotV = dot(V, V);
				float NdotV = dot(N, V) * rsqrt(VdotV);

				// Use saturate(x) instead of max(x,0.f) because that is faster on Kepler
				return saturate(NdotV - _AngleBiasValue) * saturate(Falloff(VdotV));
			}

			inline float3 MinDiff(float3 P, float3 Pr, float3 Pl)
			{
				float3 V1 = Pr - P;
				float3 V2 = P - Pl;
				return (dot(V1, V1) < dot(V2, V2)) ? V1 : V2;
			}

			float3 ReconstructNormal(float2 uv, float3 viewPos, float2 InvScreenParams, out float3 Pr, out float3 Pl, out float3 Pt, out float3 Pb)
			{
				Pr = FetchViewPos(uv + float2(InvScreenParams.x, 0));
				Pl = FetchViewPos(uv + float2(-InvScreenParams.x, 0));
				Pt = FetchViewPos(uv + float2(0, InvScreenParams.y));
				Pb = FetchViewPos(uv + float2(0, -InvScreenParams.y));
				float3 N = normalize(cross(MinDiff(viewPos, Pr, Pl), MinDiff(viewPos, Pt, Pb)));
				return N;
			}

			float4 fragAO(Varyings i) : SV_Target
			{
				int _RayMarchingDirectionCount = 4;
			    int _RayMarchingStepCount = 2;
				//_RayMarchingDirectionCount = _AORadius.z;
				//_RayMarchingStepCount = _AORadius.w;
				//float2 InvScreenParams = _ScreenParams.zw - 1.0;
				float2 InvScreenParams = _CameraDepthTexture_TexelSize.xy;
				float3 viewPos = FetchViewPos(i.uv);
				if (viewPos.z > _DistanceFalloff.y)
					return 1;
				float3 Pr, Pl, Pt, Pb;
				float3 viewNormal = ReconstructNormal(i.uv, viewPos, InvScreenParams, Pr, Pl, Pt, Pb);

				float3 rand = tex2D(_HbaoNoiseTexture, _CameraDepthTexture_TexelSize.zw * i.uv * _HbaoNoiseTexture_TexelSize.xy);
				float rayAngleStepSize = 2.0 * PI / _RayMarchingDirectionCount;
				float stepSize = min(_AORadius.x / viewPos.z, _MaxPixelRadius) / (_RayMarchingStepCount + 1.0);

				float oc = 0.0;

				for (int j = 0; j < _RayMarchingDirectionCount; j++)
				{
					float angle = rayAngleStepSize * float(j);
					float2 direction = RotateDirections(float2(cos(angle), sin(angle)), rand.xy);

					float rayPixels = (rand.z * stepSize + 1.0);

					for (int k = 0; k < _RayMarchingStepCount; k++)
					{
						float2 snappedUV = round(rayPixels * direction) * InvScreenParams + i.uv.xy;
						float3 sviewPos = FetchViewPos(snappedUV);

						rayPixels += stepSize;

						float contrib = ComputeAO(viewPos, viewNormal, sviewPos);
						oc += contrib;
					}
				}
				oc += ComputeAO(viewPos, viewNormal, Pr);
				oc += ComputeAO(viewPos, viewNormal, Pl);
				oc += ComputeAO(viewPos, viewNormal, Pt);
				oc += ComputeAO(viewPos, viewNormal, Pb);
				oc *= (_AOmultiplier / (_RayMarchingDirectionCount * _RayMarchingStepCount)) * _Intensity;

				oc = lerp(saturate(1.0 - oc), 1.0, saturate((viewPos.z - _DistanceFalloff.x) / (_DistanceFalloff.y - _DistanceFalloff.x)));

				return oc;
			}
			ENDHLSL
		}

		Pass
		{
			Name "Blur_X"

			HLSLPROGRAM
			#include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv: TEXCOORD1;
				float4 uv01: TEXCOORD2;
				float4 uv02: TEXCOORD3;
			};

			sampler2D _ColorTexture;
			half4 _ColorTexture_TexelSize;
			half4 _BlurOffset;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = TransformObjectToHClip(v.vertex.xyz);
				o.uv = v.uv;
				o.uv01.xy = v.uv - _ColorTexture_TexelSize * _BlurOffset.xy;
				o.uv01.zw = v.uv + _ColorTexture_TexelSize * _BlurOffset.xy;
				o.uv02.xy = v.uv - _ColorTexture_TexelSize * _BlurOffset.xy * 2;
				o.uv02.zw = v.uv + _ColorTexture_TexelSize * _BlurOffset.xy * 2;
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float4 sum = tex2D(_ColorTexture, i.uv) * 0.2;
				sum += tex2D(_ColorTexture, i.uv01.xy) * 0.2;
				sum += tex2D(_ColorTexture, i.uv01.zw) * 0.2;
				sum += tex2D(_ColorTexture, i.uv02.xy) * 0.2;
				sum += tex2D(_ColorTexture, i.uv02.zw) * 0.2;
				return sum;
			}
			ENDHLSL
		}

		Pass
		{
			Name "Blur_Y"
			HLSLPROGRAM
			#include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv: TEXCOORD1;
				float4 uv01: TEXCOORD2;
				float4 uv02: TEXCOORD3;
			};

			sampler2D _ColorTexture;
			half4 _ColorTexture_TexelSize;
			half4 _BlurOffset;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = TransformObjectToHClip(v.vertex);
				o.uv = v.uv;
				o.uv01.xy = v.uv - _ColorTexture_TexelSize * _BlurOffset.zw;
				o.uv01.zw = v.uv + _ColorTexture_TexelSize * _BlurOffset.zw;
				o.uv02.xy = v.uv - _ColorTexture_TexelSize * _BlurOffset.zw * 2;
				o.uv02.zw = v.uv + _ColorTexture_TexelSize * _BlurOffset.zw * 2;
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float4 sum = tex2D(_ColorTexture, i.uv) * 0.2;
				sum += tex2D(_ColorTexture, i.uv01.xy) * 0.2;
				sum += tex2D(_ColorTexture, i.uv01.zw) * 0.2;
				sum += tex2D(_ColorTexture, i.uv02.xy) * 0.2;
				sum += tex2D(_ColorTexture, i.uv02.zw) * 0.2;
				return sum;
			}
			ENDHLSL
		}
		Pass
		{
			Name "Composite Pass"

			HLSLPROGRAM
			#include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _ColorTexture;
			sampler2D _HbaoRT;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = TransformObjectToHClip(v.vertex);
				o.uv = v.uv;
				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
				half4 color = tex2D(_ColorTexture, i.uv);
				half4 ao = tex2D(_HbaoRT, i.uv);
				color.rgb *= ao.g;
				return color;
			}
			ENDHLSL
		}
	}
}

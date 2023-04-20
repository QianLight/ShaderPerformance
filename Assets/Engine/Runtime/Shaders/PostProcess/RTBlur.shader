Shader "Hidden/PostProcessing/RTBlur"
{
	HLSLINCLUDE
	
		#include "../StdLib.hlsl"
		#include "../Colors.hlsl"
		#include "../Sampling.hlsl"

		TEX2D_SAMPLER(_MainTex);

		FLOAT4 _MainTex_TexelSize;
		FLOAT _DownSampleNum;

		struct VertexInput
		{

			FLOAT4 vertex : POSITION;

			FLOAT2 texcoord : TEXCOORD0;
		};


		struct VertexOutput_DownSmpl
		{

			FLOAT4 pos: SV_POSITION;

			FLOAT2 uv20 : TEXCOORD0;

			FLOAT2 uv21 : TEXCOORD1;

			FLOAT2 uv22 : TEXCOORD2;

			FLOAT2 uv23 : TEXCOORD3;
		};



		static const FLOAT4 GaussWeigth[7] =
		{
			FLOAT4(0.0205, 0.0205, 0.0205, 0),
			FLOAT4(0.0855, 0.0855, 0.0855, 0),
			FLOAT4(0.232, 0.232, 0.232, 0),
			FLOAT4(0.324, 0.324, 0.324, 1),
			FLOAT4(0.232, 0.232, 0.232, 0),
			FLOAT4(0.0855, 0.0855, 0.0855, 0),
			FLOAT4(0.0205, 0.0205, 0.0205, 0),
		};



		VertexOutput_DownSmpl vert_DownSmpl(VertexInput v)
		{

			VertexOutput_DownSmpl o;

			//o.pos = UnityObjectToClipPos(v.vertex);
			o.pos = v.vertex;
			FLOAT2 uv = TransformTriangleVertexToUV2(v.vertex.xy);

			o.uv20 = uv +_MainTex_TexelSize.xy * FLOAT2(0.5h, 0.5h);
			o.uv21 = uv +_MainTex_TexelSize.xy * FLOAT2(-0.5h, -0.5h);
			o.uv22 = uv +_MainTex_TexelSize.xy * FLOAT2(0.5h, -0.5h);
			o.uv23 = uv +_MainTex_TexelSize.xy * FLOAT2(-0.5h, 0.5h);


			return o;
		}


		FLOAT4 frag_DownSmpl(VertexOutput_DownSmpl i) : SV_Target
		{

			FLOAT4 color = FLOAT4(0, 0, 0, 0);


			color += SAMPLE_TEX2D(_MainTex, i.uv20);
			color += SAMPLE_TEX2D(_MainTex, i.uv21);
			color += SAMPLE_TEX2D(_MainTex, i.uv22);
			color += SAMPLE_TEX2D(_MainTex, i.uv23);


			return color / 4;
		}


		struct VertexOutput_Blur
		{

			FLOAT4 pos : SV_POSITION;

			FLOAT4 uv : TEXCOORD0;

			FLOAT2 offset :TEXCOORD1;
		};


		VertexOutput_Blur vert_BlurHorizontal(VertexInput v)
		{

			VertexOutput_Blur o;

			//o.pos = UnityObjectToClipPos(v.vertex);
			o.pos = v.vertex;

			FLOAT2 uv = TransformTriangleVertexToUV2(v.vertex.xy);
			o.uv = FLOAT4(uv, 1, 1);

			o.offset = _MainTex_TexelSize.xy * FLOAT2(1.0, 0.0) * _DownSampleNum;


			return o;
		}


		VertexOutput_Blur vert_BlurVertical(VertexInput v)
		{

			VertexOutput_Blur o;
			o.pos = v.vertex;

			FLOAT2 uv = TransformTriangleVertexToUV2(v.vertex.xy);
			o.uv = FLOAT4(uv, 1, 1);

			o.offset = _MainTex_TexelSize.xy * FLOAT2(0.0, 1.0) * _DownSampleNum;

			return o;
		}


		FLOAT4 frag_Blur(VertexOutput_Blur i) : SV_Target
		{

			FLOAT2 uv = i.uv.xy;

			FLOAT2 OffsetWidth = i.offset;

			FLOAT2 uv_withOffset = uv - OffsetWidth * 3.0;

			FLOAT4 color = 0;
			for (int j = 0; j < 7; j++)
			{

				FLOAT4 texCol = SAMPLE_TEX2D(_MainTex, uv_withOffset);

				color += texCol * GaussWeigth[j];

				uv_withOffset += OffsetWidth;
			}

			return color;
		}

	ENDHLSL

	SubShader
	{
		ZWrite Off
		Blend Off


		//通道0：降采样通道 || Pass 0: Down Sample Pass 
		Pass
		{
			ZTest Off
			Cull Off

			HLSLPROGRAM

				#pragma vertex vert_DownSmpl
				#pragma fragment frag_DownSmpl

			ENDHLSL

		}


		//通道1：垂直方向模糊处理通道 || Pass 1: Vertical Pass
		Pass
		{
			ZTest Always
			Cull Off

			HLSLPROGRAM


				#pragma vertex vert_BlurVertical
				#pragma fragment frag_Blur

			ENDHLSL
		}


		//通道2：水平方向模糊处理通道 || Pass 2:Horizontal Pass
		Pass
		{
			ZTest Always
			Cull Off

			HLSLPROGRAM

				#pragma vertex vert_BlurHorizontal
				#pragma fragment frag_Blur

			ENDHLSL
		}
	}
}

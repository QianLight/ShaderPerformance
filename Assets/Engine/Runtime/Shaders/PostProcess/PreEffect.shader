Shader "Hidden/PostProcessing/PreEffect"
{
    HLSLINCLUDE

        #pragma target 3.0
		
        #include "../StdLib.hlsl"
		#include "../Noise.hlsl"
		#include "../Colors.hlsl"
		// #include "Fog.hlsl"

        TEX2D_SAMPLER(_MainTex);
		FLOAT4 _MainTex_TexelSize;
        // Camera depth texture
		//#define SKYBOX_THREASHOLD_VALUE 0.9999

		FLOAT4 _GhostBlurParam;//x:blurDiverge delta zw:velocity
		#define _BlurSize _GhostBlurParam.x

		FLOAT4 _RadialBlurParam;
		TEX2D_SAMPLER(_RadialBlurTex);

		FLOAT4x4 _CurrentVPInverse;
		FLOAT4x4 _PreviousVP;

		TEX2D_SAMPLER(_DistortionTex);
		FLOAT4 _DistortionTex_TexelSize;
		TEX2D_SAMPLER(_CameraDepthRT);

		FLOAT4 _PreffectParam;
		#define GhostBlurEnable _PreffectParam.x>0.5
		#define MotionBlurEnable _PreffectParam.y>0.5		
		#define RadialBlurEnable _PreffectParam.z>0.5

		TEX2D_SAMPLER(_DistortionPPTex0);
		TEX2D_SAMPLER(_DistortionPPTex1);
		FLOAT4 _DistortionPPParam;
		#define DistortionPP_enable _DistortionPPParam.x>0
		#define T0PannerU 0.5
		#define T0PannerV 0.5
		#define T1PannerU -0.5
		#define T1PannerV -0.5
		#define T0TilingU _DistortionPPParam.z 
		#define T0TilingV _DistortionPPParam.z 
		#define T1TilingU _DistortionPPParam.z*1.25 
		#define T1TilingV _DistortionPPParam.z*1.25 
		#define DistortionPP_intensity _DistortionPPParam.x //0.05
		#define DistortionPP_speed _DistortionPPParam.y //2

        //// -----------------------------------------------------------------------------
        ///////////////////////////////////Blur///////////////////////////////////
		FLOAT4 FragGhostBlur(VaryingsDefault i, FLOAT4 color)
		{
			UNITY_BRANCH
			if(GhostBlurEnable)
			{
				FLOAT2 velocity =  _GhostBlurParam.xy;
				FLOAT2 texCoord = i.texcoord + velocity;
				FLOAT4 currentColor = SAMPLE_TEX2D(_MainTex, texCoord);
				color += currentColor;
				texCoord += velocity;
				currentColor = SAMPLE_TEX2D(_MainTex, texCoord);
				color += currentColor;

				color/=3;
			}
			UNITY_BRANCH
			if(MotionBlurEnable)
			{
				FLOAT2 uv = i.texcoord*2-1;
				FLOAT d = FetchFromDepthTex(TEX2D_PARAM(_CameraDepthRT), i.texcoord);
				FLOAT4 H = FLOAT4(uv, d * 2 - 1, 1);
				FLOAT4 D = mul(_CurrentVPInverse, H);
				FLOAT4 worldPos = D / D.w;	
				FLOAT4 currentPos = H;  
				FLOAT4 previousPos = mul(_PreviousVP, worldPos);
				previousPos /= previousPos.w;
				FLOAT2 velocity = (currentPos.xy - previousPos.xy)/2.0f;
				velocity = clamp(velocity,-_MainTex_TexelSize.xy*4,_MainTex_TexelSize.xy*4);

				uv = i.texcoord;
				color.xyz += SAMPLE_TEX2D(_MainTex, uv).xyz;
				
				uv += velocity * _BlurSize;
				color.xyz += SAMPLE_TEX2D(_MainTex, uv).xyz;
				uv += velocity * _BlurSize;
				color.xyz += SAMPLE_TEX2D(_MainTex, uv).xyz;
				uv += velocity * _BlurSize;
				color.xyz += SAMPLE_TEX2D(_MainTex, uv).xyz;
				color.xyz *= 0.2;
			}
			return color;
		}

		FLOAT4 FragRadialBlur(VaryingsDefault i, FLOAT4 color)
		{
			UNITY_BRANCH
			if(RadialBlurEnable)
			{
				FLOAT4 blur = SAMPLE_TEX2D(_RadialBlurTex, i.texcoord);
				FLOAT4 clear = SAMPLE_TEX2D(_MainTex, i.texcoord);
				color.rgb = lerp(clear.rgb, blur.rgb, blur.a);

//				FLOAT2 uv = i.texcoord - _RadialBlurCenter;
//				FLOAT offset = 1;
//				FLOAT4 sum = color;
//				FLOAT scale = _BlurRadius + offset;  //1 MAD
//				sum += SAMPLE_TEX2D(_MainTex, uv * scale + _RadialBlurCenter); //1 MAD
				
//				scale = 2 * _BlurRadius + offset; //1 MAD
//				sum += SAMPLE_TEX2D(_MainTex, uv * scale + _RadialBlurCenter); //1 MAD
				
//				scale = 3 * _BlurRadius + offset; //1 MAD
//				sum += SAMPLE_TEX2D(_MainTex, uv * scale + _RadialBlurCenter); //1 MAD
				
//				scale = 4 * _BlurRadius + offset; //1 MAD
//				sum += SAMPLE_TEX2D(_MainTex, uv * scale + _RadialBlurCenter); //1 MAD
				
//				scale = 5 * _BlurRadius + offset; //1 MAD
//				sum += SAMPLE_TEX2D(_MainTex, uv * scale + _RadialBlurCenter); //1 MAD
//#ifdef _HIGHT_QUALITY
//				scale = 6 * _BlurRadius + offset; //1 MAD
//				sum += SAMPLE_TEX2D(_MainTex, uv * scale + _RadialBlurCenter); //1 MAD
				
//				scale = 7 * _BlurRadius + offset; //1 MAD
//				sum += SAMPLE_TEX2D(_MainTex, uv * scale + _RadialBlurCenter); //1 MAD

//				sum *= 0.125f; // 1/6
//#else
//				sum *= 0.1667f; // 1/6
//#endif
//				FLOAT distToCenter = saturate(length(uv)/(_CleanRadius+0.001));
//				// distToCenter *= distToCenter;
//				color = lerp(color,sum,distToCenter);
			}
			return color;
		}

		FLOAT4 FragPreEffectBlur(VaryingsDefault i) : SV_Target
		{			
			FLOAT4 color = SAMPLE_TEX2D(_MainTex, i.texcoord);
			color = FragGhostBlur(i, color);
			color = FragRadialBlur(i, color);			
			return color;
		}
    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

		Pass
		{
			Name "PreEffect"
			HLSLPROGRAM
				#pragma multi_compile_local _ _HIGH_QUALITY
				#pragma vertex VertDefault
				#pragma fragment FragPreEffectBlur

			ENDHLSL
		}
    }
}

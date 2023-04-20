Shader"URP/UI/OldFilm"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

    	_Saturation("Saturation", Range(0, 1)) = 0
    	_Threshold("Threshold", Vector) = (0.1, 0.05, 0, 0)
		_DirtSample("Dirt Sample", 2D) = "black"{}
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        	"LightMode" = "UniversalForward"
        }
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]

        ColorMask [_ColorMask]

    	Pass
        {
            Name "UIDefault"
			Blend One OneMinusSrcAlpha
            Tags{ "LightMode" = "UniversalForward" }
			HLSLPROGRAM
            #define SEQUENCE_LENGTH 24.0
			#define FPS 12

			#pragma target 4.5
			#define _UI_GAMMA

			#include "UIVertex.hlsl"
			
			#pragma multi_compile __ UNITY_UI_CLIP_RECT
			#pragma multi_compile_local _ UNITY_UI_ALPHACLIP
			#pragma vertex uiVert
			#pragma fragment uiFrag
			

			Texture2D _DirtSample;
            SamplerState sampler_DirtSample;
            float4 _DirtSample_ST;

            half _Saturation;
			half2 _Threshold;

			float dirtline(half2 pp, float time)
            {
	            float aaRad = 0.1;
				float threshD = _Threshold.x;
            	float threshL = _Threshold.y;
				half2 lineLookup = half2(pp.x,0) * 3 + time*100;
            	half2 dirtLookup = pp + time*100;
				half3 nesL2 = SAMPLE_TEX2D(_DirtSample,.1*lineLookup.xy).xyz * 3;
            	half3 nesD2 = SAMPLE_TEX2D(_DirtSample,.1*dirtLookup.xy).xyz * 3;
            	float3 mulsL;
				mulsL = smoothstep(threshL-aaRad,threshL+aaRad,nesL2.xzy);
				// mulsL.y = smoothstep(threshL-aaRad,threshL+aaRad,nesL2.z);
				// mulsL.z = smoothstep(threshL-aaRad,threshL+aaRad,nesL2.y);
            	float3 mulsD;
				mulsD = smoothstep(threshD-aaRad,threshD+aaRad,nesD2.xzy);
				// mulsD.y = smoothstep(threshD-aaRad,threshD+aaRad,nesD2.z);
				// mulsD.z = smoothstep(threshD-aaRad,threshD+aaRad,nesD2.y);
				time = time * .5;
				float seed = SAMPLE_TEX2D(_DirtSample,half2(time*.35,time)).x;
				float result = clamp(0.,1.,seed+.7) + .3*smoothstep(0.,24,time);
				float band = .05;
            	float3 k = half3(0.3,0.6,0.9);
            	float3 condition = step(k, seed) * step(seed, k + band);
            	float split1 = dot(mulsL*result, condition);
            	float split2 = dot(mulsD*result, condition);
            	float split = min(split1, split2);
            	float l = max(split1, split2);
            	return lerp(result,split, l);
            }
            float limitFPS(float time, float fps) 
			{
			    time = time % SEQUENCE_LENGTH;
			    return float(int(time * fps)) / fps;
			}
			
			#define INITIALIZE_COLOR(Interpolants,color) OldFilmColor(Interpolants,color)
			void OldFilmColor(UIInterpolantsVSToPS Interpolants, out FLOAT4 color)
			{
				half4 old = SAMPLE_TEX2D(_MainTex, Interpolants.uv0) + _TextureSampleAdd;
                half luminance = Luminance(old);
            	half time = limitFPS(_TimeParameters.x, FPS);
            	half3 dirt = dirtline(Interpolants.uv0 * _DirtSample_ST.xy + _DirtSample_ST.zw/**2-1*/,time);
                color = FLOAT4(lerp(old, luminance/* * _Color*/ * dirt, _Saturation).xyz, 1);
				// color = half4(1,1,1,1);
			}
			#include "UIPixel.hlsl"
			ENDHLSL
        }

    }
}

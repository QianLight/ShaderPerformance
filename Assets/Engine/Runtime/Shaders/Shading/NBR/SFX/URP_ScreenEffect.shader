Shader "URP/SFX/ScreenEffect" 
{
	Properties
	{
		[Header(oldfilm)]
	    _OldFilmColor ("Tint", Color) = (1,1,1,1)
	    _OldFilmSaturation("Saturation", Range(0, 1)) = 0
	    _OldFilmThreshold("Threshold", Vector) = (0.1, 0.05, 0, 0)
		_OldFilmDirtSample("Dirt Sample", 2D) = "black"{}
		[Header(CartoonShine)]
		_CartoonShineMin("Min", Range(0,1)) = 0.5
		[Toggle]_CartoonShineFlip("Flip", Float) = 0
		_CartoonShineStrength("Strength", Range(0,1)) = 1
		[Header(MaskGray)]
		_MaskGrayColor("MaskGrayColor", Color) = (1,1,1,1)
	}
    SubShader 
    {
    	Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZTest Always ZWrite Off Cull Off

    	HLSLINCLUDE
			Texture2D _SourceTex;
			SamplerState sampler_SourceTex;
    	
			Texture2D _OldFilmDirtSample;
            SamplerState sampler_OldFilmDirtSample;
            float4 _OldFilmDirtSample_ST;
    	
    	    Texture2D _ScreenCopyTexture;
            SamplerState sampler_ScreenCopyTexture;

    	ENDHLSL

    	Pass
        {
            Name "NoEffect"
            Tags
        	{ 
        		"LightMode" = "UniversalForward" 
        	}
        	
        	ZWrite Off
        	stencil
        	{
        		Ref [_StencilRef_PostProcessMask]
        		Comp Equal
        		Pass Keep
        	}
        	
            HLSLPROGRAM

            #pragma target 4.5
            #pragma vertex FullscreenVert
            #pragma fragment frag
		    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

            Texture2D _OldTex;
            SamplerState sampler_OldTex;
            //float4 frag (Varyings input) : SV_Target {
            half4 frag(Varyings input) : SV_Target{
                half4 old = SAMPLE_TEXTURE2D(_OldTex, sampler_OldTex, input.uv);
            	return old;
            }
            ENDHLSL
        }
    	
        Pass
        {
            Name "OldFilm"
            Tags{ "LightMode" = "UniversalForward" }
			HLSLPROGRAM
            #define SEQUENCE_LENGTH 24.0
			#define FPS 12

			#pragma target 4.5
			#pragma vertex FullscreenVert
			#pragma fragment frag
		        
		    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
	        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			
	        CBUFFER_START(UnityPerMaterial)

            half _OldFilmSaturation;
			half2 _OldFilmThreshold;
			half4 _OldFilmColor;
			CBUFFER_END

			half dirtline(half2 pp, float time)
            {
	            half aaRad = 0.1;
				half threshD = _OldFilmThreshold.x;
            	half threshL = _OldFilmThreshold.y;
				half2 lineLookup = half2(pp.x,0) * 3 + time*100;
            	half2 dirtLookup = pp + time*100;
				half3 nesL2 = SAMPLE_TEXTURE2D(_OldFilmDirtSample, sampler_OldFilmDirtSample, .1*lineLookup.xy).xyz * 3;
            	half3 nesD2 = SAMPLE_TEXTURE2D(_OldFilmDirtSample, sampler_OldFilmDirtSample, .1*dirtLookup.xy).xyz * 3;
            	half3 mulsL;
				mulsL = smoothstep(threshL-aaRad,threshL+aaRad,nesL2.xzy);
            	half3 mulsD;
				mulsD = smoothstep(threshD-aaRad,threshD+aaRad,nesD2.xzy);
				time = time * .5;
				half seed = SAMPLE_TEXTURE2D(_OldFilmDirtSample, sampler_OldFilmDirtSample, half2(time*.35,time)).x;
				half result = clamp(0.,1.,seed+.7) + .3*smoothstep(0.,24,time);
				half band = .05;
            	half3 k = half3(0.3,0.6,0.9);
				half3 t1 = step(k, seed);
				half3 t2 = step(seed.xxx, k + band);
    			half3 condition = half3(t1.x*t2.x, t1.y*t2.y, t1.z*t2.z);
    			half split1 = dot(mulsL*result, condition);
    			half split2 = dot(mulsD*result, condition);
    			half split = min(split1, split2);
    			half l = max(split1, split2);
				return lerp(result,split,l);
            }
            float limitFPS(float time, float fps) 
			{
			    time = time % SEQUENCE_LENGTH;
			    return float(int(time * fps)) / fps;
			}
			
			//float4 frag (Varyings input) : SV_Target {
            half4 frag(Varyings input) : SV_Target{
				half4 old = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, input.uv);
                half luminance = Luminance(old);
            	half time = limitFPS(_TimeParameters.x, FPS);
            	half3 dirt = dirtline(input.uv * _OldFilmDirtSample_ST.xy + _OldFilmDirtSample_ST.zw/**2-1*/,time);

                //return float4(lerp(old, luminance * _OldFilmColor * dirt, _OldFilmSaturation).xyz, 1);
                return half4(lerp(old, luminance * _OldFilmColor * dirt, _OldFilmSaturation).xyz, 1);
            }
			ENDHLSL
        }

        Pass
        {
            Name "BooleanShine"
            Tags{ "LightMode" = "UniversalForward" }
            HLSLPROGRAM

            #pragma target 4.5
            #pragma vertex FullscreenVert
            #pragma fragment frag
		    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
			half _CartoonShineMin;
            half _CartoonShineStrength;
            half _Width;
            half _CartoonShineFlip;
            CBUFFER_END
            
            //float4 frag (Varyings input) : SV_Target {
            half4 frag(Varyings input) : SV_Target{
                half4 old = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, input.uv);
                half luminance = Luminance(old);
                luminance = saturate(Smootherstep(_CartoonShineMin, _CartoonShineMin + 0.01, luminance));
            	
            	//half result = lerp(luminance, 1-luminance, _CartoonShineFlip);
            	[BRANCH]
                if (_CartoonShineFlip)
                {
                    luminance = 1 - luminance;
                }
                half result = luminance;
            	return lerp(old, result, _CartoonShineStrength);
            }
            ENDHLSL
        }
    	
    	//4
    	Pass
        {
            Name "MaskGray"
            Tags
        	{ 
        		"LightMode" = "UniversalForward" 
        	}
        	
        	ZWrite Off
//        	ZTest Off
//        	Blend SrcAlpha OneMinusSrcAlpha
        	stencil
        	{
        		Ref [_StencilRef_PostProcessMask]
        		Comp NotEqual
        		Pass Keep
        	}
        	
            HLSLPROGRAM

            #pragma target 4.5
            #pragma vertex FullscreenVert
            #pragma fragment frag
            // #include "../Sampling.hlsl"
		    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
            // #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

            // Texture2D _CameraDepthAttachment;
            // SamplerState sampler_CameraDepthAttachment;
            CBUFFER_START(UnityPerMaterial)
			half3 _MaskGrayColor;
            CBUFFER_END

            
            //float4 frag (Varyings input) : SV_Target {
            half4 frag(Varyings input) : SV_Target{
            	// return 1;
                half4 old = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, input.uv);
            	// half mask = SAMPLE_TEXTURE2D(_CameraDepthAttachment, sampler_CameraDepthAttachment, input.uv).w / 256;
                half3 luminance = Luminance(old) * _MaskGrayColor;
            	return half4(luminance,1);
            }
            ENDHLSL
        }
    	
    	//5
    	Pass
        {
            Name "Transition"
            Tags
        	{ 
        		"LightMode" = "UniversalForward" 
        	}
        	
        	ZWrite Off
//        	Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM

            #pragma target 4.5
            #pragma vertex FullscreenVert
            #pragma fragment frag
            // #include "../Sampling.hlsl"
		    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
            // #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

            CBUFFER_START(UnityPerMaterial)
			half _TransitionTransparent;
            CBUFFER_END

            
            //float4 frag (Varyings input) : SV_Target {
            half4 frag (Varyings input) : SV_Target {
            	// return 1;
                half4 current = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, input.uv);
            	half4 old = SAMPLE_TEXTURE2D(_ScreenCopyTexture, sampler_ScreenCopyTexture, input.uv);
            	return lerp(current, old, _TransitionTransparent);
            }
            ENDHLSL
        }
    }
}
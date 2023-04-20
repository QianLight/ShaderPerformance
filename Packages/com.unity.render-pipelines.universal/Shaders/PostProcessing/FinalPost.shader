Shader "Hidden/Universal Render Pipeline/FinalPost"
{
    HLSLINCLUDE
        //#pragma exclude_renderers gles
        #pragma multi_compile_fragment _ _FXAA _FXAA_LUMINANCE _TAA
        //#pragma multi_compile_local_fragment _ _FILM_GRAIN
        //#pragma multi_compile_local_fragment _ _DITHERING
        #pragma multi_compile_fragment _ _LINEAR_TO_SRGB_CONVERSION _SRGB_TO_LINEAR_CONVERSION
		#pragma multi_compile_fragment _ _MIX_AA
        #pragma multi_compile_fragment _ _CUSTOM_UI_DISTORTION
        //#pragma multi_compile _ _USE_DRAW_PROCEDURAL

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

        TEXTURE2D_X(_SourceTex);
        SAMPLER(sampler_SourceTex_LinearClamp);
        float4 _SourceTex_TexelSize;

        //#define _FXAA
        #if defined(_FXAA) || defined(_FXAA_LUMINANCE)
            #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/FXAA.hlsl"
        #endif

        //#pragma multi_compile_local MINMAX_3X3 MINMAX_3X3_ROUNDED MINMAX_4TAP_VARYING
        //#pragma multi_compile_local __ UNJITTER_COLORSAMPLES
        //#pragma multi_compile_local __ USE_YCOCG
        //#pragma multi_compile_local __ USE_CLIPPING
        //#pragma multi_compile_local __ USE_MOTION_BLUR
        //#pragma multi_compile_local __ USE_OPTIMIZATIONS
        //#pragma multi_compile_local __ USE_DILATION_3X3 USE_DILATION_5TAP
        //#pragma multi_compile_local __ USE_ANTI_FLICKERING

        #if defined(_TAA)
        #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/TAA.hlsl"
        #endif

        #ifdef _CUSTOM_UI_DISTORTION
        TEXTURE2D(_URPDistortionTex);
        SAMPLER(sampler_URPDistortionTex);
        float2 CustomDistortionUV(float2 uv)
        {
            float3 distortion = SAMPLE_TEXTURE2D(_URPDistortionTex, sampler_URPDistortionTex, uv).xyz;
			uv += (distortion.xy - 0.5) * distortion.z;
            return uv;
        }
        #endif

        #if defined(_TAA)
            struct fragmentOutput
            {
                half4 screen : SV_Target0;
                half4 buffer : SV_Target1;
            };
            fragmentOutput Frag(Varyings input)
        #else
            half4 Frag(Varyings input) : SV_Target
        #endif
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            #ifdef _CUSTOM_UI_DISTORTION
            input.uv = CustomDistortionUV(input.uv);
            #endif

            #if defined(_FXAA) || defined(_TAA) || defined(_FXAA_LUMINANCE)
                #if defined(_FXAA) || defined(_FXAA_LUMINANCE)
                    half4 col = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_SourceTex_LinearClamp, input.uv);
                    col = FXAA(col, input.uv);
                #endif

                #if defined(_TAA)
                    float4 toBuffer, col;
                    TAA(input.uv, col, toBuffer);
                #endif
            #else
                    half4 col = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_SourceTex_LinearClamp, input.uv);
            #endif

			#if _LINEAR_TO_SRGB_CONVERSION
                col = LinearToSRGB(col);
            #endif
        	
        	#if _SRGB_TO_LINEAR_CONVERSION
                col = SRGBToLinear(col);/* By:Takeshi Apply:矫正UI透明度Gamma的分支 */
        	#endif

            #if defined(_TAA)
                fragmentOutput OUT;
                OUT.screen = col;
                OUT.buffer = toBuffer;
                return OUT;
            #else
                return col;
            #endif
        }

    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "FinalPost"

            HLSLPROGRAM
                #pragma vertex FullscreenVert
                #pragma fragment Frag
            ENDHLSL
        }
    }
}

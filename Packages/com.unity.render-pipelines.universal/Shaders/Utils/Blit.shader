Shader "Hidden/Universal Render Pipeline/Blit"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

        Pass
        {
            Name "Blit"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment Frag
            /* By:Takeshi Apply:矫正UI透明度Gamma的分支 */
            #pragma multi_compile_fragment _ _LINEAR_TO_SRGB_CONVERSION _SRGB_TO_LINEAR_CONVERSION
            #pragma multi_compile_fragment _ _FXAA _FXAA_LUMINANCE _TAA
            #pragma multi_compile_fragment _ _MIX_AA
            #pragma multi_compile_fragment _ _CUSTOM_UI_DISTORTION
        
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Fullscreen.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            TEXTURE2D_X(_SourceTex);
            SAMPLER(sampler_SourceTex_LinearClamp);
            float4 _SourceTex_TexelSize;

            //#define _FXAA
            #if defined(_FXAA) || defined(_FXAA_LUMINANCE)
                #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/FXAA.hlsl"
            #endif

            //#pragma multi_compile_local_fragment _ _TAA
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

            #if defined(_CUSTOM_UI_DISTORTION)
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
            #if defined(_CUSTOM_UI_DISTORTION)
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

            #ifdef _LINEAR_TO_SRGB_CONVERSION
                col = LinearToSRGB(col);
            #endif

            #ifdef _SRGB_TO_LINEAR_CONVERSION  /* By:Takeshi Apply:矫正UI透明度Gamma的分支 */
                col = SRGBToLinear(col);
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
        }
    }
}

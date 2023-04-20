Shader "Custom/UI/StaticGaussianBlur" 
{
    SubShader
    {

		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

        Pass
        {
            Tags{ "LightMode" = "UniversalForward" }
            Name "MakeBlurRT"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment Fragment
            #pragma multi_compile _ _USE_DRAW_PROCEDURAL

            #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Fullscreen.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            TEXTURE2D_X(_SourceTex);
            SAMPLER(sampler_SourceTex);
			half2 _UvOffset;
            half4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                half2 uv = input.uv - _UvOffset * half2(3,3);
                
                half4 color0 = SAMPLE_TEXTURE2D_X(_SourceTex,sampler_SourceTex, uv) * 0.006; uv += _UvOffset;
            	half4 color1 = SAMPLE_TEXTURE2D_X(_SourceTex,sampler_SourceTex, uv) * 0.061; uv += _UvOffset;
            	half4 color2 = SAMPLE_TEXTURE2D_X(_SourceTex,sampler_SourceTex, uv) * 0.242; uv += _UvOffset;
            	half4 color3 = SAMPLE_TEXTURE2D_X(_SourceTex,sampler_SourceTex, uv) * 0.382; uv += _UvOffset;
            	half4 color4 = SAMPLE_TEXTURE2D_X(_SourceTex,sampler_SourceTex, uv) * 0.242; uv += _UvOffset;
            	half4 color5 = SAMPLE_TEXTURE2D_X(_SourceTex,sampler_SourceTex, uv) * 0.061; uv += _UvOffset;
            	half4 color6 = SAMPLE_TEXTURE2D_X(_SourceTex,sampler_SourceTex, uv) * 0.006;
            	
            	half4 color = color0 + color1 + color2 + color3 + color4 + color5 + color6;


                return color;
            }
            ENDHLSL
        }
    }
}
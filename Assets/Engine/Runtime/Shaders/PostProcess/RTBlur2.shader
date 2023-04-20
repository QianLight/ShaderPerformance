Shader "Hidden/PostProcessing/RTBlur2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    struct Attributes
    {
        float4 positionOS : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 uv : TEXCOORD0;
        float4 uv01 : TEXCOORD1;
        float4 uv23 : TEXCOORD2;
        float4 uv45 : TEXCOORD3;
    };

    float4 _MainTex_TexelSize;
    TEXTURE2D(_MainTex);
    SAMPLER(sampler_MainTex);

    half4 _BlurOffset;

    Varyings GaussianBlurVert(Attributes input)
    {
        Varyings output = (Varyings)0;
        output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
        output.uv = input.uv;
        output.uv01 = input.uv.xyxy + _BlurOffset.xyxy * half4(1, 1, -1, -1);
        output.uv23 = input.uv.xyxy + _BlurOffset.xyxy * half4(1, 1, -1, -1) * 2.0;
        output.uv45 = input.uv.xyxy + _BlurOffset.xyxy * half4(1, 1, -1, -1) * 6.0;

        return output;
    }

    float4 GaussianBlurFrag(Varyings input): SV_Target
    {
        half4 color = float4(0, 0, 0, 0);

        color += 0.40 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
        color += 0.15 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv01.xy);
        color += 0.15 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv01.zw);
        color += 0.10 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv23.xy);
        color += 0.10 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv23.zw);
        color += 0.05 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv45.xy);
        color += 0.05 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv45.zw);

        return color;
    }
    ENDHLSL

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        Pass
        {
            Name "pass0 Blur"

            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex GaussianBlurVert
            #pragma fragment GaussianBlurFrag
            ENDHLSL
        }

        Pass
        {
            Name "pass1 output"

            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                return col;
            }
            ENDHLSL
        }
    }
}
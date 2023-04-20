Shader "Hidden/PostProcessing/Copy"
{
    HLSLINCLUDE

        #include "../StdLib.hlsl"
        VaryingsDefault VertDefault2(AttributesDefault v)
        {
            VaryingsDefault o = (VaryingsDefault)0;
            o.vertex = FLOAT4(v.vertex.xy, 0.0, 1.0);
            o.texcoord = TransformTriangleVertexToUV(v.vertex.xy * _UVTransform.xy);

    #if UNITY_UV_STARTS_AT_TOP
            o.texcoord = o.texcoord * FLOAT2(1.0, -1.0) + FLOAT2(0.0, 1.0);
    #endif
            return o;
        }

        TEX2D_SAMPLER(_MainTex);

        FLOAT4 Frag(VaryingsDefault i) : SV_Target
        {
            FLOAT4 color = SAMPLE_TEX2D(_MainTex, i.texcoord);
            return color;
        }

        TEX2D_SAMPLER(_CameraDepthRT);
        FLOAT4 _BackgroundColor;
        FLOAT4 Frag2(VaryingsDefault i) : SV_Target
        {
            FLOAT4 color = SAMPLE_TEX2D(_MainTex, i.texcoord);
            FLOAT alphaMask = DecodeAlpha(SAMPLE_TEX2D(_CameraDepthRT, i.texcoord).w, 1);
            return lerp(_BackgroundColor,color,alphaMask);
        }
    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        // 0 - Fullscreen triangle copy
        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment Frag

            ENDHLSL
        }

        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertUVTransform
                #pragma fragment Frag

            ENDHLSL
        }
            
        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault2
                #pragma fragment Frag2

            ENDHLSL
        }
    }
}

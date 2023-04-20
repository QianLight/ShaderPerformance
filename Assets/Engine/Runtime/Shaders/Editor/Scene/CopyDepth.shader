Shader "Hidden/Custom/Tools/CopyDepth"
{
    HLSLINCLUDE

        #include "../../StdLib.hlsl"
        struct Attributes
        {
            float4 positionOS   : POSITION;
            float2 uv           : TEXCOORD0;
        };
        TEX2D_SAMPLER(_MainTex);
        VaryingsDefault Vert(Attributes v)
        {
            VaryingsDefault o;
            o.texcoord = v.uv;
            o.vertex = mul(_matrixVP, mul(_objectToWorld, float4(v.positionOS.xyz, 1.0)));

            return o;
        }
        FLOAT4 Frag(VaryingsDefault i) : SV_Target
        {
            FLOAT4 color = SAMPLE_TEX2D(_MainTex, i.texcoord);
            return color;
        }

    ENDHLSL

    SubShader
    {
        ZTest Always ZWrite On ColorMask 0

        Pass
        {
            HLSLPROGRAM

                #pragma vertex Vert
                #pragma fragment Frag

            ENDHLSL
        }
    }
}

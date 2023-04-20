Shader "Hidden/PostProcessing/Background"
{
	HLSLINCLUDE

        #include "../StdLib.hlsl"

        TEX2D_SAMPLER(_BackgroundTex);
        FLOAT4 _BackgroundUV;
        #define _BackStartU _BackgroundUV.x
        #define _BackEndU _BackgroundUV.y
        #define _BlackLerp _BackgroundUV.z
        FLOAT4 _BackgroundColor;
		VaryingsDefault Vert(AttributesDefault v)
		{
			VaryingsDefault o;
			o.vertex = mul(_matrixVP, mul(_objectToWorld, FLOAT4(v.vertex.xyz, 1.0)));
			o.texcoord = TransformTriangleVertexToUV(v.vertex.xy);
			return o;
		}
        FLOAT4 Frag(VaryingsDefault i) : SV_Target
        {
            FLOAT2 backUV = FLOAT2(lerp(_BackStartU,_BackEndU,i.texcoord.x),i.texcoord.y);
            FLOAT4 backTex = SAMPLE_TEX2D(_BackgroundTex, backUV);
            backTex.xyz = lerp(backTex.xyz,_BackgroundColor.xyz,_BackgroundColor.w);
            return backTex;
        }

    ENDHLSL

    SubShader
    {
		Tags { "RenderType"="Opaque" "PerformanceChecks" = "False"}
		Cull Off
        Pass
        {
			Name "FORWARD"
			Tags{ "LightMode" = "ForwardBase" }
            HLSLPROGRAM

                #pragma vertex Vert
                #pragma fragment Frag

            ENDHLSL
        }
    }
}

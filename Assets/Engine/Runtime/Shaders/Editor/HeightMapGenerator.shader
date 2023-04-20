Shader "Hidden/HeightMapGenerator"
{
    Properties { }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "PerformanceChecks" = "False" "IgnoreProjector" = "False"}

        Pass
        {
            Tags{ "LightMode" = "Forwardbase" }
            Cull Off
            ZWrite Off
            ZTest Always

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float height : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD1;
                float3 posOS : TEXCOORD2;
            };

            float4 _GradientData[128];
            float4 _HeightData;
            #define _BeginHeight _HeightData.x
            #define _EndHeight _HeightData.y
            #define _TextureSize _HeightData.zw

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = float4(v.uv * 2 - 1, 0.0, 1.0);
                o.vertex.y *= -1;
                o.height = v.vertex.x;
                o.posOS = v.vertex.xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // TODO: 叠加上一次的RT和这一次的RT
                float th = saturate((i.height - _BeginHeight) / (_EndHeight - _BeginHeight));
                float mTh = th * 127;
                int intTh = (int)(mTh);
                return lerp(_GradientData[intTh], _GradientData[intTh + 1], frac(mTh));
            }
            ENDCG
        }
    }
}

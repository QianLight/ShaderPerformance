Shader "Unlit/PreviewSH"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SHScale ("_SHLevel", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "PerformanceChecks" = "False" "IgnoreProjector" = "False"}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            half3 _SHScale;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            float3 SHEvalLinearL0(float4 shAr, float4 shAg, float4 shAb)
            {
                return float3(shAr.w, shAg.w, shAb.w);
            }

            float3 SHEvalLinearL1(float3 N, float4 shAr, float4 shAg, float4 shAb)
            {
                float3 x1;
                x1.r = dot(shAr.rgb, N);
                x1.g = dot(shAg.rgb, N);
                x1.b = dot(shAb.rgb, N);
                return x1;
            }

            float3 SHEvalLinearL0L1(float3 N, float4 shAr, float4 shAg, float4 shAb)
            {
                float4 vA = float4(N, 1.0);

                float3 x1;
                // Linear (L1) + constant (L0) polynomial terms
                x1.r = dot(shAr, vA);
                x1.g = dot(shAg, vA);
                x1.b = dot(shAb, vA);

                return x1;
            }

            float3 SHEvalLinearL2(float3 N, float4 shBr, float4 shBg, float4 shBb, float4 shC)
            {
                float3 x2;
                // 4 of the quadratic (L2) polynomials
                float4 vB = N.xyzz * N.yzzx;
                x2.r = dot(shBr, vB);
                x2.g = dot(shBg, vB);
                x2.b = dot(shBb, vB);

                // Final (5th) quadratic (L2) polynomial
                float vC = N.x * N.x - N.y * N.y;
                float3 x3 = shC.rgb * vC;

                return x2 + x3;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                half3 L0 = SHEvalLinearL0(unity_SHAr, unity_SHAg, unity_SHAb);
                half3 L1 = SHEvalLinearL1(i.normal, unity_SHAr, unity_SHAg, unity_SHAb);
                half3 L2 = SHEvalLinearL2(i.normal, unity_SHAr, unity_SHAg, unity_SHAb, unity_SHC);
                return half4(_SHScale.r * L0 + _SHScale.g * L1 + _SHScale.b * L2, 1);
            }
            ENDCG
        }
    }
}
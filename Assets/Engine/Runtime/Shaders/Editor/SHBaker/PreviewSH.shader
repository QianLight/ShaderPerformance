
Shader "Hidden/Custom/Editor/PreviewSH"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "FORWARD"
			Tags{ "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"


            struct appdata
            {
                float4 vertex : POSITION;
				float3 normal : NORMAL;

            };

            struct v2f
            {
				float3 worldNormal : NORMAL;
                float4 vertex : SV_POSITION;
                uint id : TEXCOORD0;
            };

            static const uint MAX_INSTANCE_COUNT = 128;
            float4 custom_SHAr[MAX_INSTANCE_COUNT];
            float4 custom_SHAg[MAX_INSTANCE_COUNT];
            float4 custom_SHAb[MAX_INSTANCE_COUNT];
            float4 custom_SHBr[MAX_INSTANCE_COUNT];
            float4 custom_SHBg[MAX_INSTANCE_COUNT];
            float4 custom_SHBb[MAX_INSTANCE_COUNT];
            float4 custom_SHC[MAX_INSTANCE_COUNT];

            StructuredBuffer<float3> posOffset;
            uint instanceOffset;
            v2f vert (appdata v,uint instanceID : SV_InstanceID)
            {
                v2f o;
                float3 wpos = float3(v.vertex.xyz+posOffset[instanceID + instanceOffset]);                
                o.vertex = mul(unity_MatrixVP, float4(wpos.xyz, 1.0));
				o.worldNormal = normalize(v.normal);// normalize(mul(v.normal, (float3x3)unity_WorldToObject));
                o.id = instanceID;
                return o;
            }

            half3 CustomSHEvalLinearL0L1 (half4 normal,uint id)
            {
                half3 x;

                // Linear (L1) + constant (L0) polynomial terms
                x.r = dot(custom_SHAr[id],normal);
                x.g = dot(custom_SHAg[id],normal);
                x.b = dot(custom_SHAb[id],normal);

                return x;
            }

            half3 CustomSHEvalLinearL2 (half4 normal,uint id)
            {
                half3 x1, x2;
                // 4 of the quadratic (L2) polynomials
                half4 vB = normal.xyzz * normal.yzzx;
                x1.r = dot(custom_SHBr[id],vB);
                x1.g = dot(custom_SHBg[id],vB);
                x1.b = dot(custom_SHBb[id],vB);

                // Final (5th) quadratic (L2) polynomial
                half vC = normal.x*normal.x - normal.y*normal.y;
                x2 = custom_SHC[id].rgb * vC;

                return x1 + x2;
            }

            half3 CustomShadeSH9 (half4 normal,uint id)
            {
                // Linear + constant polynomial terms
                half3 res = CustomSHEvalLinearL0L1 (normal,id);

                // Quadratic polynomials
                res += CustomSHEvalLinearL2 (normal,id);

                return res;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				return fixed4(CustomShadeSH9(half4(i.worldNormal,1.0),i.id),1);
            }
            ENDCG
        }
    }
}

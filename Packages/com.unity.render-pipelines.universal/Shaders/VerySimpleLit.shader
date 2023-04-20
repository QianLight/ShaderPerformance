// Shader targeted for low end devices. Single Pass Forward Rendering.
Shader "Universal Render Pipeline/Very Simple Lit"
{
    // Keep properties of StandardSpecular shader for upgrade reasons.
    Properties
    {
        _BaseColor("Base Color", Color) = (0.5, 0.5, 0.5, 0.5)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"  "IgnoreProjector" = "True" }
        LOD 300

        Pass
        {
            Tags {"LightMode"="UniversalForward"}
            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
              //  float2 uv : TEXCOORD0;
            };

            struct v2f
            {
               // float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(v.vertex);
                o.vertex =positionInputs.positionCS;
               // o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // sample the texture
               // float4 col = tex2D(_MainTex, i.uv);
                return _BaseColor;
            }

            #pragma vertex vert
            #pragma fragment frag


            ENDHLSL
        }


    }
}

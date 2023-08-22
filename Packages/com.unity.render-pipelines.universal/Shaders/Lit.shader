Shader "Universal Render Pipeline/Lit"
{
    // Keep properties of StandardSpecular shader for upgrade reasons.
    Properties
    {
        //_MainTex("tex",2D)="white"{}
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (0.5, 0.5, 0.5, 0.5)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"  "IgnoreProjector" = "True" }
        LOD 300

        Pass
        {
            HLSLPROGRAM

            //#include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            //sampler2D _MainTex;
            sampler2D _BaseMap;
            float4 _BaseMap_ST;
            //float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(v.vertex);
                o.vertex =positionInputs.positionCS;
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // sample the texture
               //float4 col = tex2D(_MainTex, i.uv);
                float4 col = tex2D(_BaseMap, i.uv);
                return col;
            }
            
            #pragma enable_d3d11_debug_symbols
            #pragma use_dxc
            
            #pragma vertex vert
            #pragma fragment frag


            ENDHLSL
        }


    }
}

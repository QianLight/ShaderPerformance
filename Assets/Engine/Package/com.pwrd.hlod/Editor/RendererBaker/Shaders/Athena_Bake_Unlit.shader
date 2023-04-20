Shader "Athena/Bake/Unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [Toggle(_RENDERER_BAKE_)] _SHOWBAKE("show bake",float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _RENDERER_BAKE_

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 lightmapUV: TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                #if _RENDERER_BAKE_
                o.vertex = float4((v.lightmapUV.x - 0.5) * 2, v.lightmapUV.y * -2 + 1, 0.1, 1);
                #else
                o.vertex = UnityObjectToClipPos(v.vertex);
                #endif
                
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}

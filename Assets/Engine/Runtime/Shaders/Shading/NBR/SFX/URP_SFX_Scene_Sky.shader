Shader "URP/SFX/Scene/UVEffect_Sky"
{
    Properties
    {
        // 背景贴图
        _BgTex("背景贴图",2D) = ""{}
        _BgColorR("背景颜色R",Color) = (1,1,1,1)
        _BgColorG("背景颜色G",Color) = (1,1,1,1)
        _EdgeStrength("背景边缘 宽度X 强度Y",Vector) = (.15 ,2,0,0)
        _BgColorB("背景颜色B",Color) = (1,1,1,1)
        _BgParams("背景流速 XY",Vector) = (.5,1,0,0)
        _CloudTex("云层贴图",2D) = ""{}
        _CloudColor2("云层颜色2",Color) = (1,1,1,1)
        _CloudTex2("云层贴图2",2D) = ""{}
        _CloudParams("云层1流速 XY 云层2流速 ZW",Vector) = (.5,1,0,0)

        // 星星贴图
        _StarTex("星星贴图",2D) = "" {}
        _StarReMapTex("星星ReMap",2D) = ""{}
        _StarMaskTex("星星Mask",2D) = ""{}
        _StarParams("星星流速 XY 星星Mask流速 ZW ",Vector) = (.5,1,0,0)



    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        Cull back

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
            half4 _BgColorR;
            half4 _BgColorG;
            half4 _BgColorB;
            half4 _BgTex_ST;
            half4 _BgParams;
            half4 _CloudTex_ST;
            half4 _CloudColor;
            half4 _CloudTex2_ST;
            half4 _CloudColor2;
            half4 _CloudParams;
            half4 _StarTex_ST;
            half4 _EdgeStrength;
            half4 _StarParams;
            CBUFFER_END


            sampler2D _BgTex;
            sampler2D _CloudTex;
            sampler2D _CloudTex2;
            sampler2D _StarReMapTex;
            sampler2D _StarMaskTex;
            sampler2D _StarTex;


            struct appdata
            {
                float4 vertex : POSITION;
                half2 uv : TEXCOORD0;
            };

            struct v2f
            {
                half2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;

                return o;
            }


            half4 frag(v2f i) : SV_Target
            {
                half2 bgUV = i.uv.xy * _BgTex_ST.xy + _BgTex_ST.zw + _Time.x * _BgParams.xy;
                half4 bgColor = tex2D(_BgTex, bgUV);
                bgColor = bgColor.r * _BgColorR + bgColor.g * _BgColorG + bgColor.b * _BgColorB;
                half2 cloudUV = i.uv.xy * _CloudTex_ST.xy + _CloudTex_ST.zw + _Time.x * _CloudParams.xy;
                half2 cloudUV2 = i.uv.xy * _CloudTex2_ST.xy + _CloudTex2_ST.zw + _Time.x * _CloudParams.zw;
                half4 cloud = tex2D(_CloudTex, cloudUV);
                half4 cloud2 = tex2D(_CloudTex2, cloudUV2);
                half2 starUv = _StarTex_ST * i.uv + + _Time.x * _StarParams.xy;
                half2 star = tex2D(_StarTex, starUv);
                half4 starRemap = tex2D(_StarReMapTex, i.uv.xy - _Time.x * .1);
                half4 starRemap1 = tex2D(_StarReMapTex, i.uv.xy - .2 - _Time.x * .1);
                half starMask = tex2D(_StarMaskTex, i.uv + + _Time.x * _StarParams.zw) + .1;
                half4 starColor = (star.x * starRemap + star.y * starRemap1) * starMask;
                half rate = max(max(starColor.r, starColor.g), starColor.b);
                half4 resColor = starColor * 4 + (1 - rate) * (cloud.r * _CloudColor) + (1 - rate) * cloud2.r *
                    _CloudColor2 + (1 - rate) * bgColor;
                rate = (smoothstep(0, _EdgeStrength.x, i.uv.y));
                rate =pow(rate,_EdgeStrength.y);
                resColor = (1 - rate) * 1 * _BgColorG + resColor;
                return resColor;
            }
            ENDHLSL
        }
    }
}
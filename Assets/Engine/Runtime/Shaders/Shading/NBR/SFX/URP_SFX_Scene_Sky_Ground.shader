Shader "URP/SFX/Scene/UVEffect_Sky_Ground"
{
    Properties
    {
        // 背景贴图
        _BgColor("底部颜色",Color) = (1,1,1,1)
        _EdgeColor("边缘颜色",Color) = (1,1,1,1)
        _CloudColor("云层颜色",Color) = (1,1,1,1)
        _CloudTex("云层贴图",2D) = ""{}
        _BgParams("背景旋转速度 X 边缘宽度 Y",Vector) = (.5,1,0,0)


        // 星星贴图
        _StarTex("星星贴图",2D) = "" {}
        _StarReMapTex("星星ReMap",2D) = ""{}
        _StarMaskTex("星星Mask",2D) = ""{}
        _StarParams("星星流速 XY 星星Mask流速 ZW ",Vector) = (.5,1,0,0)

        //云层贴图


    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        Cull back
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            sampler2D _CloudTex;
            sampler2D _StarReMapTex;
            sampler2D _StarMaskTex;
            sampler2D _StarTex;
            CBUFFER_START(UnityPerMaterial)
            float4 _EdgeColor;
            float4 _BgColor;
            float4 _BgParams;
            float4 _CloudTex_ST;
            float4 _CloudColor;
            float4 _CloudParams;
            float4 _StarTex_ST;
            float4 _StarParams;
            CBUFFER_END

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 posVs : TEXCOORD1;
                float4 posMs : TEXCOORD2;
                float4 tSpace0 : TEXCOORD3;
                float4 tSpace1 : TEXCOORD4;
                float4 tSpace2 : TEXCOORD5;
            };


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.posMs = v.vertex;
                float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
                float3 positionVS = TransformWorldToView(positionWS.xyz);
                o.posVs.xyz = positionVS;

                VertexNormalInputs normalInput = GetVertexNormalInputs(v.normal, v.tangent);
                o.uv = v.uv;

                return o;
            }

            half2 RotationUV(half2 uv, half radius)
            {
                uv.xy = uv.xy - float2(0.5, 0.5);
                uv.xy = float2(uv.x * cos(radius) - uv.y * sin(radius),
                               uv.x * sin(radius) + uv.y * cos(radius));

                uv += float2(0.5, 0.5);
                return uv;
            }

            half4 frag(v2f i) : SV_Target
            {
                half2 uv1 =  RotationUV(i.uv,_Time.x * _BgParams.x);
                float4 cloud = tex2D(_CloudTex, uv1).r;
                float2 uv = lerp(i.uv.xy, i.uv.xy + cloud, .1);
                half rate = length(uv * 2 - 1);
                rate = smoothstep(_BgParams.y+cloud.x *.2, 1, rate);
                half2 starUv = _StarTex_ST.xy * i.uv +  _StarTex_ST.zw ;
                 starUv =  RotationUV(starUv,_Time.x * _BgParams.x);

                half2 star = tex2D(_StarTex, starUv);
                half4 starRemap = tex2D(_StarReMapTex, i.uv.xy - _Time.x * .1);
                half4 starRemap1 = tex2D(_StarReMapTex, i.uv.xy - .2 - _Time.x * .1);
                half starMask = tex2D(_StarMaskTex, i.uv + + _Time.x * _StarParams.zw) + .1;
                half4 starColor = (star.x * starRemap + star.y * starRemap1) * starMask ;
                return half4(rate * _EdgeColor.rgb + (1 - rate) * _BgColor.rgb + starColor.rgb ,1-rate);
                // 第二层云
            }
            ENDHLSL
        }
    }
}
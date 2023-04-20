Shader"URP/SFX/URP_UVEffect_Z_Normal"
{
    Properties
    {
        [Header(builtin)]
        [Enum(UnityEngine.Rendering.BlendMode)]_BlendMode("Blend Mode", Float) = 10
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("Cull Mode", Float) = 0
        [Header(MainControl)]
        //_CUTOUT("裁剪", Range(0,1)) = 0.5
        _ColorInt("ColorInt", Float) = 1
        _AlphaInt("AlphaInt", Float) = 1
        _Contrast("Contrast", Float) = 1
        [HDR]_MainColor("Main Color", Color) = (1, 1, 1, 1)
        _MainTex ("Main Tex", 2D) = "white" {}
        _MainTex2U("2U",Vector) = (1,1,0,0)
        [Toggle]_AlphaR("Alpha / R", Float) = 0
        _MainPannerX("MainPannerX", Float) = 0
        _MainPannerY("MainPannerY", Float) = 0
        [Normal]_BumpMap("Normal",2D)="bump"{}
        _NormalScale("_NormalScale",Range(-3,3))=1

        [Header(Mask)]
        _MaskTex("Mask Tex", 2D) = "white" {}
        _MaskInt("MaskInt",float)=1
        [Header(CustomData)]
        //-硬度-溶解
        [Header(Dissolve)]
        _Hardness("Hardness", Range(0, 0.99)) = 0
        _Dissolve("Dissolve", Range(0,2)) = 0    

        //_Angle("MainUV1Angle", Range(-3.14,3.14)) = 0
        //_Angle1("MainUV2Angle", Range(-3.14,3.14)) = 0
        [Header(Angle)]
        _Angle("MainUV1Angle", Range(-1,1)) = 0
        _Angle1("MainUV2Angle", Range(-1,1)) = 0
    }
    SubShader
    {
        Tags 
        { 
            "RenderPipeline"="UniversalPipeline" 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
        }
        LOD 100
        Blend SrcAlpha [_BlendMode]
        Cull [_CullMode]
        Offset 0 , 0
		ColorMask RGBA
        AlphaToMask Off

        Pass
        {
            Tags {"LightMode"="UniversalForward"}
            //ZTest [_ZTest]
            ZTest LEqual
            //ZWrite [_DepthMode]
            ZWrite Off
            HLSLPROGRAM

            #pragma multi_compile_instancing
            #pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float4 uv2 : TEXCOORD1;
                float4 normal : NORMAL;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 tangentOS : TANGENT;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half4 color : COLOR;
                float4 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
                float3 tSpace0:TEXCOORD5;
                float3 tSpace1:TEXCOORD6;
                float3 tSpace2:TEXCOORD7;

                float3 Pos : TEXCOORD8;
            };

            Texture2D _MainTex;
            SamplerState sampler_MainTex;
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);
            Texture2D _MaskTex;
            SamplerState sampler_MaskTex;
            
            CBUFFER_START(UnityPerMaterial)
            half4 _MainTex2U;
            half4 _MainTex_ST;
            half4 _BumpMap_ST;
            half4 _MaskTex_ST;
            half4 _MainColor;
            half _NormalScale,_Angle,_Angle1;
            half _AlphaR;
            half _ColorInt;
            half _AlphaInt;
            half _Contrast;
            half _MaskInt;
            half _MainPannerX;
            half _MainPannerY;
            half _Dissolve;
            half _Hardness;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv.xy = v.uv;
                o.uv.zw = v.uv2;
                o.color = v.color;
                o.Pos=v.vertex;
                o.worldPos = TransformObjectToWorld(v.vertex);
                half3 worldTangent = TransformObjectToWorld(v.tangentOS.xyz);
                half tangentSign = v.tangentOS.w * unity_WorldTransformParams.w;
                half3 worldBinormal = cross(o.worldPos.xyz, worldTangent.xyz) * tangentSign;
                o.tSpace0 = half3(worldTangent.x,worldBinormal.x,o.worldPos.x);
                o.tSpace1 = half3(worldTangent.y,worldBinormal.y,o.worldPos.y);
                o.tSpace2 = half3(worldTangent.z,worldBinormal.z,o.worldPos.z);
                return o;
            }

            half4 frag (v2f i/*, half vface : VFACE*/) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                half2x2 Rot = half2x2(cos(_Angle),-sin(_Angle) ,sin(_Angle),cos(_Angle));
                half2x2 Rot1 = half2x2(cos(_Angle1),-sin(_Angle1) ,sin(_Angle1),cos(_Angle1));
                half2 posY = mul(Rot,i.Pos.xy);
                half2 posY1 = mul(Rot1,i.Pos.xy);
                // half3 normalTex = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,i.uv.xy),_NormalScale);
                // half3 worldNormal = half3(dot(i.tSpace0.xyz,normalTex.rgb),dot(i.tSpace1.xyz,normalTex.rgb),dot(i.tSpace2.xyz,normalTex.rgb));
                // half3 N = normalize(worldNormal);
                // Light myligth = GetMainLight();
                // half3 L = normalize(myligth.direction);
                // half lbt = max(dot(N,L)*0.5+0.5,0.5);
                half mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv.xy * _MaskTex_ST.xy + _MaskTex_ST.zw)*_MaskInt;;

                half clipXY = 2 * _Dissolve.xx - _Hardness * _Dissolve;//half2(_Dissolve, _Dissolve) * (2 - _Hardness);
                //half clipXY0 = ((1 + (1-posY.y) - clipXY)+mask);
                //clipXY0 = saturate((clipXY0 - _Hardness) / (1 - _Hardness));
                //half clipXY1 = ((1 + (1-posY1.y) - clipXY)+mask);
                //clipXY1 = saturate((clipXY1 - _Hardness) / (1 - _Hardness));
                half rOneMinusHardness = 1 / (1 - _Hardness);
                half stat = - _Hardness * rOneMinusHardness;
                half2 clipXY01 = half2(mask - posY.y - clipXY, mask - posY1.y - clipXY + 2);
                clipXY01 = saturate(clipXY01 * rOneMinusHardness - stat);

                half4 mainUV01 = i.uv - 0.5;
                // float2 mainUV = (i.uv.xy - 0.5) ;
                // float2 mainUV1 = (i.uv.zw - 0.5) ;
                half2 panner = float2(_MainPannerX, _MainPannerY) * _TimeParameters.x;
                panner = frac(panner);
                half3 custom1 = float3(panner.x, panner.y, _Dissolve);
                
                /*mainUV*/mainUV01.xy += i.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw + custom1.xy;
                /*mainUV1*/mainUV01.zw += i.uv.zw * _MainTex2U.xy + _MainTex2U.zw + custom1.xy;
                
                half4 main1U = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainUV01.xy/*mainUV*/);
                half4 main2U = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainUV01.zw/*mainUV1*/);

                // half setA = _MainColor.a* _AlphaInt;

                main1U.a = min(lerp(main1U.a, main1U.r, _AlphaR) * clipXY01.x/*clipXY0.y*/,1);
                main2U.a = min(lerp(main2U.a, main2U.r, _AlphaR) * clipXY01.y/*clipXY1.y*/,1);

                half3 mainColor = pow(main1U.rgb + main2U.rgb, _Contrast) * _ColorInt * _MainColor.rgb;

                half mainA =  max(main1U.a + main2U.a,0);

                return half4(mainColor,mainA);
            }
            ENDHLSL
        }
    }
}

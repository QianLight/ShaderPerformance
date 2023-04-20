Shader "Effect/UVEffect_card1"
{
    Properties
    {
        [HDR]_EdgeColor("EdgeColor",Color) = (1,1,1,1)
        [HDR]_EdgeColorBase("EdgeColorBase",Color) = (1,1,1,1)
        [HDR]_EdgeAlpha("_EdgeAlpha",Range(0,1)) = .9
        _CharacterTex ("角色贴图", 2D) = "white" {}
        //_MaskTex ("Mask", 2D) = "black" {}
        //_ShapeSpeInt("形状高光强度",Range(0,5)) = 0.5
        _CharacterColor ("角色颜色", Color) = (1,1,1,1)
        _CharacterUVClamp("角色贴图UV钳制",Vector) = (0,1,0,.94)
        _CharacterOffset("角色深度",Range(0,1)) = 0.5
        _BgTex ("背景贴图", 2D) = "white" {}
        _BgOffset("背景深度",Range(0,1)) = 0.5
        _BackgroundColor("背景颜色",Color) = (0,0,0,0.5)
        _GlassColor("玻璃颜色",Color) = (0,0,0,0.5)
        [NoScaleOffset]_MatCapTex("MapCapTex",2D) = "white"{}
        [NoScaleOffset]_MatCapTex2("MapCapTex2",2D) = "black"{}
        _Matcap2Foat("流动高光Matcap",Range(0,2)) = 0.5
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend", Float) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("DstBlend", Float) = 0.0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4
        [Enum(Off, 0, On, 1)]_ZWrite("ZWrite", Float) = 1.0

    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        Cull back
        Blend[_SrcBlend][_DstBlend]
        ZWrite[_ZWrite]
        ZTest[_ZTest]

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

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
                float4 tSpace0 : TEXCOORD3;
                float4 tSpace1 : TEXCOORD4;
                float4 tSpace2 : TEXCOORD5;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _EdgeColor;
            float4 _EdgeColorBase;
            float4 _CharacterColor;
            float4 _BaseEdgeColor;
            float4 _CharacterTex_ST;
            float4 _BgTex_ST;
            float4 _CharacterUVClamp;
            float4 _BackgroundColor;
            float4 _GlassColor;
            float _CharacterOffset;
            float _BgOffset;
            float _EdgeAlpha;
            //float _ShapeSpeInt;
            float _Matcap2Foat;
            CBUFFER_END
            sampler2D _CharacterTex;
            //sampler2D _MaskTex;
            sampler2D _MatCapTex;
            sampler2D _MatCapTex2;
            sampler2D _BgTex;

            float2 MatCapUV(in float3 N, in float3 viewPos)
            {
                float3 vTangent = SafeNormalize(cross(viewPos, float3(0, 1, 0)));
                float3 vBinormal = SafeNormalize(cross(viewPos, vTangent));
                float3 viewNorm = SafeNormalize(mul((float3x3)UNITY_MATRIX_V, N));
                float2 matCapUV = float2(dot(vTangent, viewNorm), dot(vBinormal, viewNorm)) * 0.47 + 0.5;
                return matCapUV;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
                float3 positionVS = TransformWorldToView(positionWS.xyz);
                o.posVs.xyz = positionVS;

                VertexNormalInputs normalInput = GetVertexNormalInputs(v.normal, v.tangent);
                o.uv = v.uv;
                o.tSpace0 = float4(normalInput.normalWS, positionWS.x);
                o.tSpace1 = float4(normalInput.tangentWS, positionWS.y);
                o.tSpace2 = float4(normalInput.bitangentWS, positionWS.z);
                return o;
            }

            bool ClampUV(half2 uv, half4 range)
            {
                return uv.x > range.x && uv.x < range.y &&
                    uv.y > range.z && uv.y < range.w;
            }

            half4 frag(v2f i) : SV_Target
            {
                float3 WorldNormal = normalize(i.tSpace0.xyz);
                float3 WorldTangent = i.tSpace1.xyz;
                float3 WorldBiTangent = i.tSpace2.xyz;
                float3 WorldPosition = float3(i.tSpace0.w, i.tSpace1.w, i.tSpace2.w);
                float3 WorldViewDirection = _WorldSpaceCameraPos.xyz - WorldPosition;
                float3 tanToWorld0 = float3(WorldTangent.x, WorldBiTangent.x, WorldNormal.x);
                float3 tanToWorld1 = float3(WorldTangent.y, WorldBiTangent.y, WorldNormal.y);
                float3 tanToWorld2 = float3(WorldTangent.z, WorldBiTangent.z, WorldNormal.z);
                float3 tanViewDir = tanToWorld0 * WorldViewDirection.x + tanToWorld1 * WorldViewDirection.y +
                    tanToWorld2 * WorldViewDirection.z;
                tanViewDir = SafeNormalize(tanViewDir);
                tanViewDir /= tanViewDir.z;
                // 主角色
                half2 charUV = i.uv.xy * _CharacterTex_ST.xy + _CharacterTex_ST.zw;
                half2 offsetChar = (_CharacterOffset - 1) * tanViewDir.xy  + charUV;
                half4 charTex = tex2D(_CharacterTex, offsetChar) * ClampUV(offsetChar, _CharacterUVClamp);
                charTex *= _CharacterColor.rgba;
                half3 charColor = charTex.rgb * charTex.a;
                // 背景
                half2 bgUv = i.uv.xy * _BgTex_ST.xy + _BgTex_ST.zw;
                half2 offsetBg = (_BgOffset - 1) * tanViewDir.xy + bgUv;
                half4 bgTex = tex2D(_BgTex, offsetBg) ;
                bgTex *= _BackgroundColor.rgba;
                half3 bgColor = bgTex.rgb * bgTex.a;
                half3 resColor = charColor.rgb + (1 - charTex.a) * bgColor.rgb;//区分主纹理和背景
                half forwardAlpha =  max(charTex.a,bgTex.a);//取主和背景的a最大值
                resColor = resColor  + (1-forwardAlpha) * _GlassColor.rgb;//用
                half alpha = forwardAlpha + (1 - forwardAlpha) * _GlassColor.a;
                //speculr
                //half4 Mask = tex2D(_MaskTex, i.uv.xy);
                Light light = GetMainLight();
				float3 H =normalize (normalize(WorldViewDirection) + normalize(light.direction));
				//float speculr = dot(normalize(WorldNormal) , H );
				//speculr = pow(abs(speculr) ,55);//*_ShapeSpeInt*Mask.r
                // MatCap
                half3 refNor = SafeNormalize(reflect(WorldViewDirection, WorldNormal));
                half2 matCapUV = MatCapUV(refNor, i.posVs);
                half3 matcap = tex2D(_MatCapTex, matCapUV);
                half3 matcap2 = tex2D(_MatCapTex2, matCapUV);
                half spec = saturate(pow(matcap.r, 4));
                half3 col = lerp(_EdgeColorBase, _EdgeColor, spec) * (matcap);
                half cull = i.uv.x < .1;
                col = resColor * (1 - cull) + col * saturate((1 - charTex.a) * .1 + cull);
                //col.rgb += speculr;
                col.rgb += matcap2*_Matcap2Foat;
                alpha = _EdgeAlpha * cull + (1 - cull) * alpha;
                return half4(col.rgb+matcap2*0.1, alpha);
            }
            ENDHLSL
        }
    }
}
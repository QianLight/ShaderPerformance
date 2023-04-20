Shader "Effect/UVEffect_Card_Edge"
{
    Properties
    {
        [HDR]_EdgeColor("EdgeColor",Color) = (1,1,1,1)
        [HDR]_EdgeColorBase("EdgeColorBase",Color) = (1,1,1,1)
        _EdgeAlpha("_EdgeAlpha",Range(0,1)) = .9
        _BaseColor ("基础色", Color) = (1,1,1,1)
        [NoScaleOffset]_MatCapTex("MapCapTex",2D) = "white"{}
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
            float4 _BaseColor;
            float4 _BackgroundColor;
            float4 _GlassColor;
            float _CharacterOffset;
            float _BgOffset;
            float _EdgeAlpha;
            CBUFFER_END
            sampler2D _MatCapTex;

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

                half3 refNor = SafeNormalize(reflect(WorldViewDirection, WorldNormal));
                half2 matCapUV = MatCapUV(refNor, i.posVs);
                half3 matcap = tex2D(_MatCapTex, matCapUV);
                half spec = saturate(pow(matcap.r, 4));
                half3 col = lerp(_EdgeColorBase, _EdgeColor, spec) * matcap + (1-matcap)*_BaseColor.rgb;
                return half4(col.rgb, _EdgeAlpha);
            }
            ENDHLSL
        }
    }
}
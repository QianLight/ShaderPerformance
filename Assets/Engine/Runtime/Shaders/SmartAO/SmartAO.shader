Shader "URP/SmartAO"
{
    Properties
    {
        [MainTexture] _MainTex("Texture", 3D) = "white" {}
        _Scale("Scale", float) = 2.0
        _MaxWeight("Max Weight", float) = 0.5
        _RefVal("stencil ref value",int) = 0

    }
    SubShader
    {
        Tags{ "Queue" = "AlphaTest" }
        LOD 100
        Pass
        {
            Tags{"LightMode" = "UniversalForwardHigh" "RenderType" = "Transparent" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline"}
            
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest Off
            Cull Front
            Stencil
            {
                Ref[_RefVal]
                Comp GEqual
                Pass Replace
            }
            Name "SmartAO"
            HLSLPROGRAM
            #pragma target 3.0

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            TEXTURE2D_FLOAT(_CameraDepthTexture);    SAMPLER(sampler_CameraDepthTexture);
            //TEXTURE2D(_BaseMap);    SAMPLER(sampler_BaseMap);
            sampler3D _MainTex;

            float4x4 unity_MatrixVP;
            float4x4 unity_MatrixV;
            float4x4 unity_CameraToWorld;
            float4 _ZBufferParams;
            float4 _ProjectionParams;

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half _Scale;
                half _MaxWeight;
            CBUFFER_END

            CBUFFER_START(UnityPerDraw)
                float4x4 unity_ObjectToWorld;
                float4x4 unity_WorldToObject;
                float4 unity_LODFade;
                float4 unity_ProbesOcclusion;
                float4 unity_LightmapST;
                //float4 unity_LightmapIndex;
                float4 unity_DynamicLightmapST;
				
                real4 unity_WorldTransformParams;
                real4 unity_LightData;
                real4 unity_LightIndices[2];

                real4 unity_SpecCube0_HDR;
                real4 unity_SHAr;
                real4 unity_SHAg;
                real4 unity_SHAb;
                real4 unity_SHBr;
                real4 unity_SHBg;
                real4 unity_SHBb;
                real4 unity_SHC;
            CBUFFER_END

            struct Attributes
            {
                float4 vertex : POSITION;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float4 screenUV : TEXCOORD0;
                float3 ray : TEXCOORD1;
            };

            float3 TransformObjectToWorld(float3 positionOS)
            {
                return mul(unity_ObjectToWorld, float4(positionOS, 1.0f)).xyz;
            }

            float4 TransformObjectToHClip(float3 positionWS)
            {
                return mul(unity_MatrixVP, float4(positionWS, 1.0f));
            }

            float3 TransformWorldToView(float3 positionWS)
            {
                return mul(unity_MatrixV, float4(positionWS, 1.0f)).xyz;
            }

            float3 CreateViewRay(float3 positionWS)
            {
                return TransformWorldToView(positionWS) * float3(-1.0f, -1.0f, 1.0f);
            }

            float4 ComputeScreenPos(float4 positionCS)
            {
                float4 o = positionCS * 0.5f;
                o.xy = float2(o.x, o.y * _ProjectionParams.x) + o.w;
                o.zw = positionCS.zw;
                return o;
            }

            float3 GetUV(Varyings i)
            {
                float2 uv = i.screenUV.xy / i.screenUV.w;
                float depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
                depth = Linear01Depth(depth, _ZBufferParams);

                i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
                float4 vpos = float4(i.ray * depth, 1.0f);

                float3 wpos = mul(unity_CameraToWorld, vpos).xyz;
                float3 opos = mul(unity_WorldToObject, float4(wpos, 1.0f)).xyz;

                //clip(float3(0.5,0.5,0.5) - abs(opos.xyz));
                return opos.xyz + 0.5;
            }

            Varyings vert(Attributes i)
            {
                Varyings o;
                float3 wpos = TransformObjectToWorld(i.vertex.xyz);
                o.vertex = TransformObjectToHClip(wpos);
                o.screenUV = ComputeScreenPos(o.vertex);
                o.ray = CreateViewRay(wpos);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 texUV = GetUV(i);
                float4 col = tex3D(_MainTex, texUV);
                col.a = min(_MaxWeight, col.r * _Scale);
                col.rgb = 0;
                return col;
            }
            ENDHLSL
        }
    }
}

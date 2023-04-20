Shader "URP/UI/UIReflect"
{
    Properties
    {
        [Header(Albedo)]
        _BaseMap("基础贴图",2D) = "white" {}
        [HDR] _BaseColor("基础色",Color) = (1,1,1,1)
        [HDR] _ReflectColor("反射",Color) = (1,1,1,1)
        _FresnelParams ("X 菲涅尔基础值 Y 菲尼尔强度",Vector) = (.03,10,0,0)
        [Header(Ripples)]
        _RippleParams ("XY涟漪中心 Z最大距离 W 强度衰减",Vector) = (.5,.5, 1,1)
        _RippleParams2 ("X振幅 Y频率 Z波速 ",Vector) = (.5,.5, 1,1)
        [HDR]RippleSpecular("涟漪高光颜色", Color) = (0.172549,0.7803922,0.9019608,1)
        [Toggle]_CSharpSync("同步C#,精确时间，忽略衰减", Int) = 0
        [Enum(On,1,Off,0)]_DepthMode("Depth Mode", Float) = 0
    }

    SubShader
    {
        LOD 0

        Tags
        {
            "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"
        }

        Cull Back
        AlphaToMask Off
        HLSLINCLUDE
        #pragma target 2.0
        ENDHLSL


        Pass
        {

            Name "Forward"
            Tags
            {
                "LightMode"="UniversalForward" "Queue"="Transparent"
            }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite [_DepthMode]
            Cull Back
            Offset 0 , 0
            ColorMask RGBA


            HLSLPROGRAM
            #pragma multi_compile _FX_LEVEL_HIGH _FX_LEVEL_MEDIUM _FX_LEVEL_LOW
            #pragma multi_compile_instancing

            #if defined(_FX_LEVEL_HIGH)
            #define REQUIRE_DEPTH_TEXTURE 1
            #define USE_DEPTH_TEX
            #define CALC_VERTEX_NORMAL
            #elif defined(_FX_LEVEL_MEDIUM)
                #define CALC_VERTEX_NORMAL
            #else
				#define IGNORE_SHADOW
            #endif

            #define MAIN_LIGHT_CALCULATE_SHADOWS


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "Assets/Engine/Runtime/Shaders/Shading/NBR/Include/Fog.hlsl"
            #include "Assets/Engine/Runtime/Shaders/Shading/Scene/ParkourDistort.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            struct VertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 uv : TEXCOORD0;
                float4 tangent : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput
            {
                float4 clipPos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float4 uv : TEXCOORD1;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            #define UNITY_PI            3.14159265359f
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _ReflectColor;
            float4 _BaseMap_ST;
            float4 _FresnelParams;
            float4 _RippleParams;
            float4 _RippleParams2;
            float4 RippleSpecular;

            float4 _PlannarRelfectDistorb;


            float _CSharpSync;
            CBUFFER_END

            sampler2D _BaseMap;
            sampler2D _SSPlanarReflectionTexture;
            float _RealTime;

            #define _RipplesCenter (_RippleParams.xy)
            #define _RipplesMaxDistance (_RippleParams.z)
            #define _RipplesAtten (_RippleParams.w)
            #define _RipplesStrength (_RippleParams2.x)
            #define _RipplesFreq (_RippleParams2.y)
            #define _RipplesSpeed (_RippleParams2.z)


            half3 SampleReflection(half2 reflectionUV)
            {
                half4 sample = 0;
                #if defined(_FX_LEVEL_HIGH)
                sample = tex2D(_SSPlanarReflectionTexture, reflectionUV.xy); //planar reflection
                #endif
                return sample.rgb;
            }

            VertexOutput VertexFunction(VertexInput v)
            {
                VertexOutput o = (VertexOutput)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                //使用世界坐标当uv还是模型坐标当uv
                float3 srcWorldPos = TransformObjectToWorld(v.vertex.xyz);
                float3 positionWS = srcWorldPos;
                float4 positionCS = TransformWorldToHClip(positionWS);

                o.worldPos = positionWS;
                o.clipPos = positionCS;

                o.uv = v.uv;
                return o;
            }


            VertexOutput vert(VertexInput v)
            {
                return VertexFunction(v);
            }

            half4 frag(VertexOutput IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);


                float4 clipPos = TransformWorldToHClip(IN.worldPos);
                float4 screenPos = ComputeScreenPos(clipPos);
                float2 screenUV = screenPos.xy / screenPos.w;


                half2 uvDir = IN.uv.xy - _RippleParams.xy;
                half2 uvDir_N = normalize(uvDir);
                half uvDistance = length(uvDir);
                float divivde = _RipplesMaxDistance * .1;
                half strengthdis = step(uvDistance, divivde) * (1 - RangeRemap(0, divivde, uvDistance)) * .1 +
                    step(divivde, uvDistance) * RangeRemap(divivde, 1, uvDistance - divivde);
                half strength = pow(saturate(1 - strengthdis / _RipplesMaxDistance),_RipplesAtten);
                half offsetRate = strength * sin(2 * UNITY_PI * _RipplesFreq * uvDistance - _Time.y * _RipplesSpeed) *
                    _RipplesStrength;
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - IN.worldPos.xyz);

                half2 offset = uvDir_N * offsetRate;
                //环境反射

                half3 distortNormal = 0;
                half2 reflectionUV = screenUV + offset + distortNormal.zx * half2(0.1, 0.3) * _PlannarRelfectDistorb.w;
                float3 reflection = SampleReflection(reflectionUV);
                float3 reflectionColor = saturate(reflection * _ReflectColor.rgb);

                half2 baseUV = IN.uv.xy * _BaseMap_ST.xy + _BaseMap_ST.zw + offset;
                baseUV = baseUV + distortNormal.zx * half2(0.1, 0.3) * _PlannarRelfectDistorb.w;

                float3 baseColor = tex2D(_BaseMap, baseUV) * _BaseColor.rgb * _BaseColor.a;
                float fresene = _FresnelParams.x + (1 - _FresnelParams.x) * pow(
                    1 - saturate(dot(half3(0, 1, 0), viewDir)), _FresnelParams.y);
                float maxChannel = max(max(reflection.r, reflection.g), reflection.b);
                float rate = (1 - maxChannel);
                float3 res = baseColor + reflectionColor * (1 - rate) * fresene;
                return float4(res, 1);
            }
            ENDHLSL
        }
    }
    CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
    Fallback "Hidden/InternalErrorShader"

}
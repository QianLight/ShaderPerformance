 Shader "Hidden/UrpScatterPrecomputer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            #pragma vertex vert
            #pragma fragment frag

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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            //half4 _MainLightColor0;
            //half4 _MainLightDir0;

            #include "../Include/PrecomputedScatter.hlsl"

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float usage = i.uv.y * ROW_COUNT;
                
                float3 result;
                if (usage < FOG_SUN_VDL_SCATTER + 1)
                {
                    float vdl = i.uv.x * 2.0 - 1.0;
                    result = ComputeFogSunVdlScatter(vdl);
                }
                else if (usage < FOG_SUN_DEPTH_SCATTER + 1)
                {
                    float depth = i.uv.x;
                    result = ComputeFogSunDepthScatter(depth);
                }
                else if (usage < SKYBOX_VDL_SCATTER + 1)
                {
                    float vdl = i.uv.x * 2.0 - 1.0;
                    result = ComputeSkyboxVdlScatter(vdl);
                }
                else if (usage < SKYBOX_HEIGHT_SCATTER + 1)
                {
                    float height = i.uv.x * 2.0 - 1.0;
                    result = ComputeSkyboxHeightScatter(height);
                }
                #if MOON
                else if (usage < FOG_MOON_VDL_SCATTER + 1)
                {
                    float vdl = i.uv.x * 2.0 - 1.0;
                    result = ComputeFogMoonVdlScatter(vdl);
                }
                else if (usage < FOG_MOON_DEPTH_SCATTER + 1)
                {
                    float depth = i.uv.x;
                    result = ComputeFogMoonDepthScatter(depth);
                }
                #endif
                else
                {
                    result = 1;
                }
                return float4(result, 1);
            }
            ENDHLSL
        }
    }
}

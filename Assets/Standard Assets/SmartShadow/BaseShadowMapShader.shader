// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Shadow/BaseShadowMapShader"
{
    Properties
    {
        _Color("Color(RGB)",Color) = (1,1,1,1)
        _MainTex("MainTex",2D) = "gary"{}
    }
        SubShader
    {
        Tags
        {
            // 渲染管线标记，对应的管线C#代码UniversalRenderPipeline.cs中的
            // Shader.globalRenderPipeline = UniversalPipeline,LightweightPipeline,
            // 只有带有UniversalPipeline或LightweightPipeline的Tag的SubShader才会生效.
            // 主要作用是用于标记当前这个SubShader是属于哪个管线下的.
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry+0"
        }

        Pass
        {
            Name "Pass"
            Tags
            {

            }

            // Render State
            Blend One Zero, One Zero
            Cull Back
            ZTest LEqual
            ZWrite On

        HLSLPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 4.5
        #pragma multi_compile_instancing
        #pragma shader_feature _SMARTSOFTSHADOW_ON

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
        #include "../../Engine/Runtime/Shaders/Shading/Include/SmartShadow.hlsl"

        CBUFFER_START(UnityPerMaterial)

        half4 _Color;
        CBUFFER_END

            float4 _MainTex_ST;

            #define smp SamplerState_Point_Repeat
            SAMPLER(smp);

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv :TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv :TEXCOORD0;
                float3 normalDir : TEXCOORD1;
                float4 worldPos: TEXCOORD2;
            };


            // 顶点着色器
            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;
                o.uv = TRANSFORM_TEX(v.uv,_MainTex);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.normalDir = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));

                o.worldPos.xyz = TransformObjectToWorld(v.positionOS);
                o.worldPos.w = 1;
                return o;
            }

            // 片段着色器
            half4 frag(Varyings i) : SV_TARGET
            {
                //half4 mainTex = SAMPLE_TEXTURE2D(_MainTex,smp,i.uv);
                //half4 c = _Color * mainTex;
                return GetSmartShadow(float3(1,1,1), i.normalDir, i.worldPos, _SmartShadowIntensity);
                //return GetSmartShadow9Point(i.worldPos, 0.5, 1);
            }

            ENDHLSL
        }
    }
        FallBack "Hidden/Shader Graph/FallbackError"
}
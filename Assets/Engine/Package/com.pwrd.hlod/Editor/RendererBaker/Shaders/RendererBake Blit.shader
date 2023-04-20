Shader "Hidden/RendererBake/Blit"
{
    Properties 
    {
        _OriginMap("origin", 2D) = "white" {}
        _MiniMap("mini", 2D) = "clear" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
        LOD 300

        Pass
        {
            Name "ForwardLit" 
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            
            #pragma shader_feature_local _ _FILL_
            
            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 
            #include "UnityCG.cginc"

            sampler2D _OriginMap;
            sampler2D _MiniMap;
            float _Fill;
            struct Attributes
            {
                float2 texcoord      : TEXCOORD0;
                float4 positionOS    : POSITION;
            };

            struct Varyings
            {
                float2 uv                       : TEXCOORD0;
                float3 posWS                    : TEXCOORD2;    // xyz: posWS
                float4 positionCS               : SV_POSITION;
            };

            Varyings LitPassVertexSimple(Attributes input)
            {
                Varyings output = (Varyings)0;

                output.uv = input.texcoord;
                output.positionCS = UnityObjectToClipPos(input.positionOS.xyz);
            
                return output;
            }

            half4 LitPassFragmentSimple(Varyings input) : SV_Target
            {
                float4 color = tex2D(_OriginMap, input.uv).rgba;
#if _FILL_
                if((color.a) < 0.01)
                {
                    color.rgb = tex2D(_MiniMap, input.uv).rgb;
                }
#endif      
                return color;
            };

            #pragma vertex LitPassVertexSimple
            #pragma fragment LitPassFragmentSimple

            ENDHLSL
        }
    }
    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}

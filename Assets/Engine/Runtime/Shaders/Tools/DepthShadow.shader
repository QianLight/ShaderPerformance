// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Hidden/Custom/Tools/DepthShadow"
{
    Properties{}

    SubShader
    {
        Tags { "RenderType" = "Opaque"}
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "Opaque" }
     
            ColorMask 0
            //ZTest LEqual
            //ZWrite On
          //    Cull Off
            HLSLINCLUDE
            #include "../StdLib.hlsl"
            #include "../Incluce/Common.hlsl"
            #pragma vertex vert
            #pragma fragment frag

            struct a2v
            {
                float4 positionOS: POSITION;
            };

            struct v2f
            {
                float4 positionCS: SV_POSITION;
            };


            v2f vert(a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToClipPos(v.positionOS.xyz);
                return o;
            }

            half4 frag(v2f i): SV_Target
            {
                return (1, 0, 0, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ColorDepth"
            Tags { "LightMode" = "Opaque" }
            Cull Off

          //  ZWrite Off 

            /*HLSLINCLUDE
            #include "../StdLib.hlsl"
            #include "../Incluce/Common.hlsl"
            #pragma vertex vert
            #pragma fragment frag

            struct a2v
            {
                float4 positionOS: POSITION;
            };

            struct v2f
            {
                float4 positionCS: SV_POSITION;
                float4 pos: TEXCOORD0;

            };

            v2f vert(a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToClipPos(v.positionOS.xyz);
                o.pos = o.positionCS;
                return o;
            }

            float4 frag(v2f i): SV_Target
            {
                float depth = i.pos.z * 0.5 + 0.5 / i.pos.w;
                return float4(1, depth, 1, 0);
            }
            ENDHLSL*/

            CGPROGRAM
                #include "UnityCG.cginc"
                #pragma vertex vert
                #pragma fragment frag
           //     float4x4 _DepthsShadowMatrix;
                struct a2v
                {
                    float4 positionOS: POSITION;
                };

                struct v2f
                {
                    float4 positionCS: SV_POSITION;
                 //   float4 Clippostion:TEXCOORD0;

                };

                v2f vert(a2v v)
                {
                    v2f o;
                    o.positionCS = UnityObjectToClipPos(v.positionOS.xyz);
                    //float4 postionworld=mul(unity_ObjectToWorld,float4(v.positionOS.xyz,1));
                    //o.Clippostion=mul(_DepthsShadowMatrix,float4(postionworld.xyz,1));

                    return o;
                }

                half4 frag(v2f i) : SV_Target
                {
                    float depth = (i.positionCS.z/i.positionCS.w)* 0.5 + 0.5;
                    return float4(1,depth, 0, 1);
                }
            ENDCG
        }
    }
}

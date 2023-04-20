Shader "Hidden/Custom/Tools/SceneViewPreZ"
{
HLSLINCLUDE

        #include "../../StdLib.hlsl"

        TEX2D_SAMPLER(_MainTex);

		struct FVertexInput
		{  
			FLOAT4	Position	: POSITION;
		}; 
		struct FV2F
		{
			FLOAT4	Position : SV_POSITION;
			FLOAT2 Depth01 : TEXCOORD14;
		};
		FV2F Vert(FVertexInput v)
		{
			FV2F v2f = (FV2F)0;
			v2f.Position = mul(_matrixVP, mul(_objectToWorld, FLOAT4(v.Position.xyz, 1.0)));
			v2f.Depth01 = v2f.Position.zw;
			return v2f;
		}

        FLOAT4 Frag(in FV2F v2f) : SV_Target
        {
			FLOAT4 rt = 0;
			rt.xyz = EncodeFloatRGB(v2f.Depth01.x/ v2f.Depth01.y);
			return rt;
        }

    ENDHLSL

    SubShader
    {
		Tags { "RenderType"="Opaque" "PerformanceChecks" = "False"}

        Pass
        {
			Name "SceneViewPreZ"
			Tags{ "LightMode" = "Always" }
            ZWrite On
			Cull Back
			ColorMask 0
            HLSLPROGRAM

                #pragma vertex Vert
                #pragma fragment Frag

            ENDHLSL
        }
    	
		Pass
		{
			Name "OverdrawA"
			Tags{"LightMode" = "OverdrawAlways" "RenderType"="Opaque" "PerformanceChecks" = "False"}
			ZWrite On
			Cull Back
			ColorMask 0

			Blend One One
			HLSLPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag

			struct Attributes
			{
				float4 vertex : POSITION;
			};
			
			struct Varyings
			{
				float4 vertex : SV_POSITION;
			};
			Varyings Vert(Attributes v)
			{
				Varyings o;
				float4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = mul(unity_MatrixVP, WorldPosition);
				return o;
			}

			half4 Frag(Varyings i) : SV_Target
			{
				return half4(0.1, 0.04, 0.02, 1);
			}
			
			ENDHLSL
		}
    }	
}

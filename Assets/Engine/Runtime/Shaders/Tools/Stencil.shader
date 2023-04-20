Shader "Hidden/Custom/Tools/Stencil"
{
	Properties
    {
		_Param("Line Fun",Vector) = (0,0,0,1)
		_Color("Background",Color) = (0,0,0,1)
		_Stencil ("Stencil ID", Float) = 0
	}
	HLSLINCLUDE
	#include "../StdLib.hlsl"

	FLOAT4 _Param;
	FLOAT4 _Color;
	#define _a0 _Param.x
	#define _b0 _Param.y
	#define _a1 _Param.z
	#define _b1 _Param.w
	FLOAT4 Frag (VaryingsDefault i) : SV_Target
	{
		FLOAT2 uv = i.texcoord;
		FLOAT x0 = _a0*uv.y+_b0;
		FLOAT x1 = _a1*uv.y+_b1;
		FLOAT mask = (uv.x>=x0&&uv.x<=x1)?1:-1;
		clip(mask);
		return _Color;
	}

	ENDHLSL

	SubShader
	{
		Cull Off ZWrite On ZTest Always
		Stencil 
		{  
            Ref [_Stencil]
            Comp always
            Pass replace
        }  

		Pass
		{
			Name "Draw"

			HLSLPROGRAM
			#pragma vertex VertDefault
			#pragma fragment Frag
			ENDHLSL
		}
		Pass
		{
			Name "OverdrawF"
			Tags{"LightMode" = "OverdrawForwardBase"}

			Blend One One
			CGPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag

			#include "UnityCG.cginc"

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

			ENDCG
		}
	}

	FallBack Off
}

Shader "UI/CircleMask2"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Param0("x:Softness y:Clip z:AlphaScale", Vector) = (-1.5,0.25,0.8,0)
		_Color ("Tint", Color) = (1,1,1,1)
		_Param1("xy:offset zw:scale", Vector) = (0,0,1,0)
		_RempInt("RempInt",Range(-1,1))=1
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" }
		LOD 100

		Pass
		{
			Tags{ "LightMode" = "UniversalForward" }
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

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
			float4 _Param0;
			float4 _Param1;
			float4 _Color;
			half _RempInt;
			#define _Softness _Param0.x
			#define _Clip _Param0.y
			#define _AlphaScale _Param0.z
			#define _OffsetX _Param1.x
			#define _OffsetY _Param1.y
			#define _ScaleX _Param1.z
			#define _ScaleY _Param1.w
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = fixed4(0,0,0,1);//tex2D(_MainTex, i.uv);
				fixed xx = (i.uv.x - 0.5f - _OffsetX)*_ScaleX;
				fixed yy = (i.uv.y - 0.5f - _OffsetY)*_ScaleY;
				fixed r = (xx * xx + yy * yy);
				//return r;
				//col.a *= saturate((_Clip - r) * _Softness)*_AlphaScale;
				col.a *= smoothstep(0.4,1,r)*_AlphaScale;//渐变 _RempInt
				col.rgb = lerp(col.rgb,_Color.rgb,_Color.a);
				return col;
			}
			ENDCG
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
}

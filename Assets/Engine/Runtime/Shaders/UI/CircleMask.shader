Shader "UI/CircleMask"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Softness("Softness", Range(0,1)) =0.01
		_Saturation("Saturation", Range(0,1)) =0.01
		_ColorInt("ColorInt", Range(1,2)) =1
		//[HDR]_Color("Color", Color) = (1,1,1,1)
		_offse("xy:offset zw:scale", Vector) = (0,0,1,0)
		
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
			fixed4 _offse;
			float _Softness,_Saturation,_ColorInt;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed xx = (i.uv.x - 0.5f-_offse.x)*_offse.z;
				fixed yy = (i.uv.y - 0.5f-_offse.y)*_offse.w;
				fixed r = sqrt(xx * xx + yy * yy);
				col.a *= saturate((0.5f - r) * (1.0f / _Softness));

				//color adjust
				half gray = 0.2125 * col.r + 0.7154 * col.g + 0.0721 * col.b;
	            half3 grayColor = half3(gray, gray, gray);
	            col.xyz = lerp(grayColor, col, _Saturation);
				col.xyz *=_ColorInt;
				
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

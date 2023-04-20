Shader "UI/SpeedLine"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_SpeedX("_SpeedX", Float) = 0.1
		_SpeedY("_SpeedY", Float) = 0.1
		_Color("Tint", Color) = (1,1,1,1)
	}
		SubShader
		{
			Tags { "Queue" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off

			Pass
			{
				Tags{ "LightMode" = "UniversalForward" }
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float _SpeedX;
				float _SpeedY;
				uniform fixed4 _Color;

				struct IN {
					float4 vertex : POSITION;
					float4 color : COLOR;
					float4 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 vert : SV_POSITION;
					fixed4 color : COLOR;
					float2 uv : TEXCOORD0;
				};

				v2f vert(IN v)
				{
					v2f o;
					o.vert = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.color = v.color * _Color;
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					i.uv *= 0.8;
					i.uv.x = (i.uv.x + _Time.y * _SpeedX);
					i.uv.y = (i.uv.y + _Time.y * _SpeedY);
					fixed4 col = tex2D(_MainTex, i.uv);
					return col * i.color;
				}
				ENDCG
			}
			Pass
			{
				Name "OverdrawF"
				Tags{"LightMode" = "OverdrawForwardBase"}

				Blend One One
				HLSLPROGRAM

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

				ENDHLSL
			}
		}
}

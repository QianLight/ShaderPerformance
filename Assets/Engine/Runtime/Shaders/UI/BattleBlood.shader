Shader "Custom/UI/BattleBlood"
{
	Properties
	{
		_Shadow("Shadow", Float) = 0.2
		_MinAlpha("MinAlpha", Float) = 0.2
        _ForeColor0 ("Foreground0", Color) = (1,1,1,1)
		_ForeColor1 ("Foreground1", Color) = (1,1,1,1)
		_ForeColor2 ("Foreground2", Color) = (1,1,1,1)
        _BackColor ("Background", Color) = (1,1,1,1)
		_ShadowColor("ShadowColor", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }

		Pass
		{
			Tags{ "LightMode" = "UniversalForward" }
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma fragment frag
			#pragma vertex vert
			#include "UnityCG.cginc"
			
			float _Shadow;
			float _MinAlpha;
			fixed4 _ForeColor0;
			fixed4 _ForeColor1;
			fixed4 _ForeColor2;
			fixed4 _BackColor;
			fixed4 _ShadowColor;

			struct a2v
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
				float4 texcoord2 : TEXCOORD1;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
			};
			

			v2f vert(a2v v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				o.uv2 = v.texcoord2;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				//fixed mid = 1;
				//fixed2 uv = i.uv;
				//uv -= fixed2(_Center, 0.5);
				//uv.y *= _Radian;
				//mid *= step(_Radian, length(uv));

				//uv.x += _Center * 2 - 1;
				//mid *= step(_Radian, length(uv));

				//fixed mid2 = step(i.uv.x, 1.0 - _Radian);
				//mid2 *= step(_Radian, i.uv.x);

				//return (1 - mid + mid2) * fixed4(1, 0, 0, 1);

				fixed mid = step(i.uv.x, i.uv2.x);
				fixed mid2 = step(i.uv.y, _Shadow);
				fixed4 Color = _ShadowColor;
				Color.a = (i.uv.y / _Shadow) * (_ShadowColor.a - _MinAlpha) + _MinAlpha;
				float x1 = step(0.5f, i.uv2.y);
				float x2 = step(1.5f, i.uv2.y);
				fixed4 foreColor = _ForeColor0 * (1 - x1) +_ForeColor1 * x1 * (1 - x2) + _ForeColor2 * x2;
				return  mid2 * Color + (1 - mid2) *( mid * foreColor + (1 - mid) * _BackColor);
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

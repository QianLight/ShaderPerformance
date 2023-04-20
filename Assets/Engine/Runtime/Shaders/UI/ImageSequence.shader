Shader "UI/ImageSequence"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Frame("Frame", Int) = 1
		_TimePerFrame("TimePerFrame", Float) = 0.5
		_RemainTime("RemainTime", Float) = 0.0
	}
	SubShader
	{
		Tags { "Queue"="Transparent" }
		Pass
		{
			Tags{ "LightMode" = "UniversalForward" }
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			int _Frame;
			float _TimePerFrame;
			float _RemainTime;

			struct v2f
			{
				float4 vert : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert (appdata_base v)
			{
				v2f o;
				o.vert = UnityObjectToClipPos(v.vertex);

				float width = 1.0f / _Frame;
				int index = floor(fmod(_Time.y, _TimePerFrame * _Frame + _RemainTime) / _TimePerFrame);
				int finalIndex = clamp(index, 0, _Frame - 1);
				o.uv = float2((finalIndex + v.texcoord.x) * width, v.texcoord.y);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return tex2D(_MainTex, i.uv);
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

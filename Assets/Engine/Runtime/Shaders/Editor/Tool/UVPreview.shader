Shader "Hidden/Custom/Editor/UVPreview"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase" }
		LOD 100
		Cull Off
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				float4 vertex = float4(v.uv2.x, v.uv2.y,  0, 1);
				vertex.xy = vertex.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				vertex.xy = vertex.xy - 0.5;
				o.vertex = UnityObjectToClipPos(vertex);
				o.uv = v.uv;
				o.uv2 = v.uv2;
				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target
			{
				// sample the texture
				fixed4 color = tex2D(_MainTex,i.uv2* _MainTex_ST.xy+ _MainTex_ST.zw);
				return color;
			}
			ENDCG
		}
	}
}

Shader "Custom/DebugNoise"
{
	Properties
	{
		[MaterialToggle] _isToggled("Use 3D Noise", Float) = 0
		_2DNoise ("2D Noise", 2D) = "white" {}
		_3DNoise ("3D Noise", 3D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Tags
			{
				"LightMode" = "ForWardBase"
			}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float3 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			float _isToggled;
			
			sampler2D _2DNoise;
			sampler3D _3DNoise;	
			
			v2f vert (appdata v)
			{
				v2f o;
				//VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
				o.vertex =UnityObjectToClipPos(v.vertex);
				o.uv = v.vertex.xyz * 0.5 + 0.5;
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{
				half4 col;
				if(_isToggled) col = tex3D(_3DNoise, i.uv);
				else col = tex2D(_2DNoise, i.uv.xy);
				return col.xxxx;
			}
			ENDCG
		}
	}
}

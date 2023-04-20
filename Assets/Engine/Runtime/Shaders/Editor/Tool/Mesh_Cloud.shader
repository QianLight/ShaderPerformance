Shader "Hidden/Custom/Editor/Mesh_Cloud"
{
	Properties
	{
		_Color("Color",Color) = (1,1,1,1)
	}

	SubShader
	{
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			LOD 100
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM


			#pragma vertex Vert
			#pragma fragment Frag
			#pragma target 4.5

			#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};
			
			StructuredBuffer<float3> points;

			v2f Vert(uint id : SV_VertexID, uint instance_id : SV_InstanceID)
			{
				v2f o = (v2f)0;
				float4 p =  float4(points[id],1);
				o.vertex = mul(unity_MatrixVP, p);
				return o;
			}

			float4 _Color;
			half4 Frag(v2f i) : SV_Target
			{
				half4 color = _Color;
				color.a = 1;
				return color;
			}
			ENDCG
		}
	}
}

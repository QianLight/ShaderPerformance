Shader "Hidden/Custom/Editor/DrawTerrainMeshProcedural"
{
	Properties
	{
		_Color("Outline Color",Color) = (1,1,1,1)

		_Width("Outline Width",float) = 0.99
	}

	SubShader
	{
		ZWrite Off
		// ZTest Off
		Pass
		{
			Name "FORWARD"
			Tags{ "LightMode" = "ForwardBase" "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			LOD 100

			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM


			#pragma vertex Vert
			#pragma fragment Frag
			// #pragma target 4.5

			#include "UnityCG.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv0 : TEXCOORD0;
				float2 localPos : TEXCOORD1;
			};
			

			float4 _Color;
			float4 _GridSize;

			StructuredBuffer<float> vertexHeight;

			v2f Vert(uint id : SV_VertexID, 
				uint instanceID : SV_InstanceID)
			{
				uint pointIndex[4];
				uint index0 = instanceID + instanceID/64;
				pointIndex[0] = index0;
				pointIndex[3] = index0 + 1;
				pointIndex[1] = index0 + 65;
				pointIndex[2] = index0 + 66;
				uint vertexIndex = pointIndex[id];
				float h = vertexHeight[vertexIndex]+0.01;

				uint x = instanceID%64;
				uint z = instanceID/64;
				float2 points[4];
				points[0] = float2(x*2,z*2),
				points[3] = points[0] + float2(2,0);
				points[1] = points[0] + float2(0,2);
				points[2] = points[0] + float2(2,2);

				v2f o = (v2f)0;
				float4 p = float4(points[id].x,h,points[id].y,1);
				o.uv0 = float2(p.x*_GridSize.z,p.z*_GridSize.w);
				o.localPos = p.xz;
				p.x += _GridSize.x;
				p.z += _GridSize.y;				
				o.pos = mul(UNITY_MATRIX_VP, p);				
				return o;
			}
			float _Width;
			half4 Frag(v2f i) : SV_Target
			{
				half4 c = _Color;
				float2 roundPos = (frac(i.localPos/2)-0.5)*2;
				if(abs(roundPos.x)>_Width||abs(roundPos.y)>_Width)
				{
				}
				else
				{
					c.a = 0;
				}
				
				return c;
			}
			ENDCG
		}
	}
}

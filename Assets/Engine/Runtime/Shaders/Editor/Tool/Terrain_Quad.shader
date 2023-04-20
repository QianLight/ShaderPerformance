Shader "Hidden/Custom/Editor/TerrainQuad"
{
	Properties
	{
		_Color("Outline Color",Color) = (1,1,1,1)
		_WireColor("Wire Color",Color) = (0,1,0,1)
		_WireThickness ("Wire Thickness", RANGE(0, 800)) = 100
		_WireSmoothness ("Wire Smoothness", RANGE(0, 20)) = 3
	}

	SubShader
	{

		// ZTest Off
		Pass
		{
			Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        	}
			Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
			Cull Back
			CGPROGRAM


			#pragma vertex Vert
			#pragma geometry geom
			#pragma fragment Frag
			// #pragma target 4.5

			#include "UnityCG.cginc"

			struct v2g
			{
				float4 projectionSpaceVertex : SV_POSITION;
			};
			
			struct g2f
			{
				float4 projectionSpaceVertex : SV_POSITION;
				float4 dist : TEXCOORD2;
			};

			float4 _Color;
			float4 _WireColor;
			StructuredBuffer<float3> points;
			float2 offset;
			float _WireThickness;
			float _WireSmoothness;
			v2g Vert(uint id : SV_VertexID, 
				uint instanceID : SV_InstanceID)
			{				
				float4 p = float4(points[instanceID*3+id],1);
				p.x += offset.x;
				p.z += offset.y;
				v2g o = (v2g)0;
				o.projectionSpaceVertex = mul(unity_MatrixVP, p);
				return o;
			}

			[maxvertexcount(3)]
			void geom(triangle v2g i[3], inout TriangleStream<g2f> triangleStream)
			{
				float2 p0 = i[0].projectionSpaceVertex.xy / i[0].projectionSpaceVertex.w;
				float2 p1 = i[1].projectionSpaceVertex.xy / i[1].projectionSpaceVertex.w;
				float2 p2 = i[2].projectionSpaceVertex.xy / i[2].projectionSpaceVertex.w;
				// float2 p3 = i[3].projectionSpaceVertex.xy / i[3].projectionSpaceVertex.w;

				float2 edge0 = p2 - p1;
				float2 edge1 = p2 - p0;
				float2 edge2 = p1 - p0;

				// To find the distance to the opposite edge, we take the
				// formula for finding the area of a triangle Area = Base/2 * Height, 
				// and solve for the Height = (Area * 2)/Base.
				// We can get the area of a triangle by taking its cross product
				// divided by 2.  However we can avoid dividing our area/base by 2
				// since our cross product will already be double our area.
				float area = abs(edge1.x * edge2.y - edge1.y * edge2.x);
				float wireThickness = 800 - _WireThickness;

				g2f o = (g2f)0;
				
				o.projectionSpaceVertex = i[0].projectionSpaceVertex;
				o.dist.xyz = float3( (area / length(edge0)), 0.0, 0.0) * o.projectionSpaceVertex.w * wireThickness;
				o.dist.w = 1.0 / o.projectionSpaceVertex.w;
				triangleStream.Append(o);

				o.projectionSpaceVertex = i[1].projectionSpaceVertex;
				o.dist.xyz = float3(0.0, (area / length(edge1)), 0.0) * o.projectionSpaceVertex.w * wireThickness;
				o.dist.w = 1.0 / o.projectionSpaceVertex.w;
				triangleStream.Append(o);

				o.projectionSpaceVertex = i[2].projectionSpaceVertex;
				o.dist.xyz = float3(0.0, 0.0, (area / length(edge2))) * o.projectionSpaceVertex.w * wireThickness;
				o.dist.w = 1.0 / o.projectionSpaceVertex.w;
				triangleStream.Append(o);


				// float2 edge3 = p2 - p3;
				// float2 edge4 = p3 - p0;
				// float2 edge5 = edge2;

				// o.projectionSpaceVertex = i[0].projectionSpaceVertex;
				// o.dist.xyz = float3( (area / length(edge3)), 0.0, 0.0) * o.projectionSpaceVertex.w * wireThickness;
				// o.dist.w = 1.0 / o.projectionSpaceVertex.w;
				// triangleStream.Append(o);

				// o.projectionSpaceVertex = i[2].projectionSpaceVertex;
				// o.dist.xyz = float3(0.0, (area / length(edge4)), 0.0) * o.projectionSpaceVertex.w * wireThickness;
				// o.dist.w = 1.0 / o.projectionSpaceVertex.w;
				// triangleStream.Append(o);

				// o.projectionSpaceVertex = i[3].projectionSpaceVertex;
				// o.dist.xyz = float3(0.0, 0.0, (area / length(edge5))) * o.projectionSpaceVertex.w * wireThickness;
				// o.dist.w = 1.0 / o.projectionSpaceVertex.w;
				// triangleStream.Append(o);
			}

			half4 Frag(g2f i) : SV_Target
			{
				float minDistanceToEdge = min(i.dist[0], min(i.dist[1], i.dist[2])) * i.dist[3];

				float4 baseColor = _Color;

				// Early out if we know we are not on a line segment.
				if(minDistanceToEdge > 0.9)
				{
					return fixed4(baseColor.rgb,0);
				}

				// Smooth our line out
				float t = exp2(_WireSmoothness * -1.0 * minDistanceToEdge * minDistanceToEdge);
				fixed4 finalColor = lerp(baseColor, _WireColor, t);
				finalColor.a = 0.1;

				return finalColor;
			}
			ENDCG
		}

		// Pass
		// {
		// 	CGPROGRAM


		// 	#pragma vertex Vert
		// 	#pragma fragment Frag
		// 	#pragma target 4.5

		// 	#include "UnityCG.cginc"

		// 	struct v2f
		// 	{
		// 		float4 projectionSpaceVertex : SV_POSITION;
		// 	};
			

		// 	float4 _Color;
		// 	StructuredBuffer<float3> points;
		// 	float2 offset;
		// 	v2f Vert(uint id : SV_VertexID, 
		// 		uint instanceID : SV_InstanceID)
		// 	{				
		// 		float4 p = float4(points[instanceID*3+id],1);
		// 		p.x += offset.x;
		// 		p.z += offset.y;
		// 		v2f o = (v2f)0;
		// 		o.projectionSpaceVertex = mul(unity_MatrixVP, p);
		// 		return o;
		// 	}

		// 	half4 Frag(v2f i) : SV_Target
		// 	{
		// 		half4 color = _Color;
		// 		color.a = 1;
		// 		return color;
		// 	}
		// 	ENDCG
		// }
	}
	FallBack off
}

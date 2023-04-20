//float4 _Tessellation;
//float _DisplacementStrength;

#ifndef URP_TESSELLATION_INCLUDED
#define URP_TESSELLATION_INCLUDED

float4 UnityCalcTriEdgeTessFactors(float3 triVertexFactors);
float CalcDistanceTessFactor(float3 wpos, float minDist, float maxDist, float tess);
bool TriangleIsBelowClipPlane(float3 p0, float3 p1, float3 p2, int planeIndex, float bias);
bool TriangleIsCulled(float3 p0, float3 p1, float3 p2, float bias);
float3 ProjectPointOnPlane(float3 position, float3 planePosition, float3 planeNormal);
float3 PhongTessellation(float3 positionWS, float3 p0, float3 p1, float3 p2, float3 n0, float3 n1, float3 n2, float3 baryCoords, float shape);

struct OutputPatchConstant
{
	float edge[3] : SV_TESSFACTOR;
	float inside : SV_INSIDETESSFACTOR;
};

OutputPatchConstant HSConst(float4 patchPos0, float4 patchPos1, float4 patchPos2)
{
	float minDist = _Tessellation[1];
	float maxDist = _Tessellation[2];

	float3 p0 = mul(unity_ObjectToWorld, patchPos0).xyz;
	float3 p1 = mul(unity_ObjectToWorld, patchPos1).xyz;
	float3 p2 = mul(unity_ObjectToWorld, patchPos2).xyz;

	OutputPatchConstant o;
	float edge0 = CalcDistanceTessFactor(p0, minDist, maxDist, _Tessellation[0]);
	float edge1 = CalcDistanceTessFactor(p1, minDist, maxDist, _Tessellation[0]);
	float edge2 = CalcDistanceTessFactor(p2, minDist, maxDist, _Tessellation[0]);

	if (TriangleIsCulled(p0, p1, p2, -1))
	{
		o.edge[0] = o.edge[1] = o.edge[2] = o.inside = 0;
	}
	else
	{
		o.edge[0] = (edge1 + edge2) / 2;
		o.edge[1] = (edge2 + edge0) / 2;
		o.edge[2] = (edge0 + edge1) / 2;
		o.inside = (edge0 + edge1 + edge2) / 3;
	}
	return o;
}

float4 PhongTess(float4 patchPos0, float4 patchPos1, float4 patchPos2, float3 patchN0, float3 patchN1, float3 patchN2, float4 vPos, float3 bary)
{
	float3 p0 = mul(unity_ObjectToWorld, patchPos0).xyz;
	float3 p1 = mul(unity_ObjectToWorld, patchPos1).xyz;
	float3 p2 = mul(unity_ObjectToWorld, patchPos2).xyz;

	float3 n0 = TransformObjectToWorldNormal(patchN0);
	float3 n1 = TransformObjectToWorldNormal(patchN1);
	float3 n2 = TransformObjectToWorldNormal(patchN2);
	float3 posWS = mul(unity_ObjectToWorld, vPos).xyz;

	posWS = PhongTessellation(posWS, p0, p1, p2, n0, n1, n2, bary, _Tessellation[3]);
	return mul(unity_WorldToObject, float4(posWS, 1.0));
}

float4 UnityCalcTriEdgeTessFactors(float3 triVertexFactors)
{
	float4 tess;
	tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
	tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
	tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
	tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
	return tess;
}

float CalcDistanceTessFactor(float3 wpos, float minDist, float maxDist, float tess)
{
	float dist = distance(wpos, GetCameraPositionWS());
	float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
	return (f);
}

bool TriangleIsBelowClipPlane(float3 p0, float3 p1, float3 p2, int planeIndex, float bias)
{
	float4 plane = unity_CameraWorldClipPlanes[planeIndex];
	return
		dot(float4(p0, 1), plane) < bias &&
		dot(float4(p1, 1), plane) < bias &&
		dot(float4(p2, 1), plane) < bias;
}

bool TriangleIsCulled(float3 p0, float3 p1, float3 p2, float bias)
{
	return
		TriangleIsBelowClipPlane(p0, p1, p2, 0, bias) ||
		TriangleIsBelowClipPlane(p0, p1, p2, 1, bias) ||
		TriangleIsBelowClipPlane(p0, p1, p2, 2, bias) ||
		TriangleIsBelowClipPlane(p0, p1, p2, 3, bias);
}

float3 ProjectPointOnPlane(float3 position, float3 planePosition, float3 planeNormal)
{
	return position - (dot(position - planePosition, planeNormal) * planeNormal);
}

float3 PhongTessellation(float3 positionWS, float3 p0, float3 p1, float3 p2, float3 n0, float3 n1, float3 n2, float3 baryCoords, float shape)
{
	float3 c0 = ProjectPointOnPlane(positionWS, p0, n0);
	float3 c1 = ProjectPointOnPlane(positionWS, p1, n1);
	float3 c2 = ProjectPointOnPlane(positionWS, p2, n2);

	float3 phongPositionWS = baryCoords.x * c0 + baryCoords.y * c1 + baryCoords.z * c2;

	return lerp(positionWS, phongPositionWS, shape);
}

OutputPatchConstant hsconst(InputPatch<TessVertex, 3> patch)
{
	return HSConst(patch[0].TESS_POS, patch[1].TESS_POS, patch[2].TESS_POS);
}

[domain("tri")]
[partitioning("integer")]
[outputtopology("triangle_cw")]
[patchconstantfunc("hsconst")]
[outputcontrolpoints(3)]
TessVertex Hull(InputPatch<TessVertex, 3> patch, uint id : SV_OutputControlPointID)
{
	return patch[id];
}

[domain("tri")]
TESS_OUT Domain(OutputPatchConstant tessFactors, const OutputPatch<TessVertex, 3> patch, float3 bary : SV_DOMAINLOCATION)
{
	FVertexInput v;
	v.Position = patch[0].TESS_POS * bary.x + patch[1].TESS_POS * bary.y + patch[2].TESS_POS * bary.z;
	v.TangentX = patch[0].TESS_NOR * bary.x + patch[1].TESS_NOR * bary.y + patch[2].TESS_NOR * bary.z;
	v.TangentZ = patch[0].TESS_TAN * bary.x + patch[1].TESS_TAN * bary.y + patch[2].TESS_TAN * bary.z;
#ifdef _INPUT_UV0 
	v.uv0 = patch[0].uv0 * bary.x + patch[1].uv0 * bary.y + patch[2].uv0 * bary.z;
#endif//_INPUT_UV

#ifdef _INPUT_UV2_4 
	v.uv2 = patch[0].uv2 * bary.x + patch[1].uv2 * bary.y + patch[2].uv2 * bary.z;
#else
#ifdef _INPUT_UV2 
	v.uv2 = patch[0].uv2 * bary.x + patch[1].uv2 * bary.y + patch[2].uv2 * bary.z;
#endif//_INPUT_UV2
#endif//_INPUT_UV2_4

#ifdef _VERTEX_COLOR
	v.Color = patch[0].Color * bary.x + patch[1].Color * bary.y + patch[2].Color * bary.z;
#endif //_VERTEX_COLOR

	/*
	//Phone2.0
	float3 p0 = mul(unity_ObjectToWorld, patch[0].Position).xyz;
	float3 p1 = mul(unity_ObjectToWorld, patch[1].Position).xyz;
	float3 p2 = mul(unity_ObjectToWorld, patch[2].Position).xyz;

	//float3 nn0 = TransformObjectToWorldNormal(patch[0].TangentX);
	//float3 nn1 = TransformObjectToWorldNormal(patch[1].TangentX);
	//float3 nn2 = TransformObjectToWorldNormal(patch[2].TangentX);
	//float3 t0 = TransformObjectToWorldNormal(patch[0].TangentZ.xyz);
	//float3 t1 = TransformObjectToWorldNormal(patch[1].TangentZ.xyz);
	//float3 t2 = TransformObjectToWorldNormal(patch[2].TangentZ.xyz);
	//float3 b0 = cross(nn0, t0) * patch[0].TangentZ.w;
	//float3 b1 = cross(nn1, t1) * patch[1].TangentZ.w;
	//float3 b2 = cross(nn2, t2) * patch[2].TangentZ.w;
	//
	//float3x3 TBN0 = float3x3(t0, b0, nn0);
	//float3x3 TBN1 = float3x3(t1, b1, nn1);
	//float3x3 TBN2 = float3x3(t2, b2, nn2);

	//float3 n0 = mul(normalize(patch[0].Color),TBN0);
	//float3 n1 = mul(normalize(patch[1].Color), TBN1);
	//float3 n2 = mul(normalize(patch[2].Color), TBN2);

	float3 n0 = TransformObjectToWorldNormal(patch[0].TangentX);
	float3 n1 = TransformObjectToWorldNormal(patch[1].TangentX);
	float3 n2 = TransformObjectToWorldNormal(patch[2].TangentX);
	float3 posWS = mul(unity_ObjectToWorld, v.Position).xyz;
	posWS = PhongTessellation(posWS,
		p0, p1, p2, n0, n1, n2,
		bary,
		_PhongShape);
	v.Position = mul(unity_WorldToObject, float4(posWS, 1.0));
	*/

	v.TESS_POS = PhongTess(patch[0].TESS_POS, patch[1].TESS_POS, patch[2].TESS_POS, patch[0].TESS_NOR, patch[1].TESS_NOR, patch[2].TESS_NOR, v.TESS_POS, bary);

	TESS_OUT o;
	uint instanceID;
	vertForwardBase(v, instanceID, o);
	return o;
}

#endif
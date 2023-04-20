#ifndef URP_PARKOURDISTORT_HLSL
#define URP_PARKOURDISTORT_HLSL

half _ParkourStrength = 0.0001;
half _FlatDistance = 15;

float4 _BendPivot;
float _BendAmount;
float _IsParkour;


float4 ParkourDistortVertex(float3 posWS)
{
	UNITY_BRANCH
	if(_BendAmount < 0.5)
	{
		//x轴
		float dist = distance(posWS.xyz, _WorldSpaceCameraPos.xyz);
		dist = max(0, dist - _FlatDistance);
		half y = _ParkourStrength * dist * dist * _ProjectionParams.x;
		float3 xAxisBend = posWS;
		xAxisBend.y -= y ;
		return float4(xAxisBend, 1);
	}
	else{
		//z轴
		float bendAngle = _ParkourStrength;
		float3 pos = posWS - _BendPivot.xyz;
		float amountX = pos.x;
		//amount *= step(0,abs(amount) -_FlatDistance);
		//UNITY_BRANCH
		amountX = pos.x > 0 ? max(0,amountX - _FlatDistance): min(0,amountX + _FlatDistance);
		float3x3 bendZMatrix =  float3x3(
			cos(bendAngle * amountX),-sin(bendAngle * amountX),0,
			sin(bendAngle * amountX),cos(bendAngle * amountX),0,
			0,0,1);
		
		float3 blendPosZ = mul(bendZMatrix, pos);
		blendPosZ += _BendPivot.xyz;
		return float4(blendPosZ, 1);
	}
}
#endif
#ifndef SCENE_WEATHER
#define SCENE_WEATHER

float _darkValueInRain;
float _RainRoughness;
float _NormalTSScale;

float _RippleRadius;
float _RingCount;
//float _RippleDensity;
//float _DropSpeed;
//float _RippleFade;
float _RippleTiling;
float _RippleSpeed;
float _RippleIntensity;
//_RippleTex  R:涟漪范围 GB:高度差类似法线 A:时间差
TEXTURE2D(_RippleTex);       SAMPLER(sampler_RippleTex);

float3 voronoihash6( float3 p )
{
    p = float3( dot( p, float3( 127.1, 311.7, 211.5 )), dot( p, float3( 269.5, 183.3, 301.1 )), dot( p, float3( 371.3, 290.5, 153.7)));
    
	return frac(p);
}

float4 GetRippleNormalWS(float3 worldPos, float rippleDensity, float rippleRadius, float ringCount, float speed, float fade)
{
	float t = _Time.y - floor(_Time.y / 900) * 900.0;//这句为了避免随时间增大导致精度变低。每隔15分钟雨会突跃一次
	float3 WorldPosition = worldPos * rippleDensity + float3(0, speed * t, 0);

	float radiusWithDensity = rippleRadius * rippleDensity;
	//rippleRadius = radiusWithDensity;

	float3 n = floor(WorldPosition);
	float3 f = frac(WorldPosition);

	float3 voronoiCenter = float3(0, -10000, 0);
	//遍历3x2x3个晶格，找出在半径内且最上的一个
	for (int z = -1; z <= 1; z++)
	{
		for (int y = 0; y <= 1; y++)
		{
			for (int x = -1; x <= 1; x++)
			{
				float3 g = float3(x, y, z);
				float3 o = voronoihash6(n + g);

				float3 vCenter = n + g + o;
				float3 r = WorldPosition - vCenter;
				if (vCenter.y > voronoiCenter.y && dot(r, r) < radiusWithDensity * radiusWithDensity)
				{
					voronoiCenter = vCenter;
				}
			}
		}
	}

	float3 dir = (WorldPosition - voronoiCenter); //真正的距离

	//不被任何一个球覆盖到
	float alpha = length(dir) < radiusWithDensity;

	//水平距离
	float horizontalDistance = length(float3(dir.x, 0, dir.z));
	//这个球与表面相交后，最远的一个点到球心的水平距离
	float maxHorizontalDistance = sqrt(radiusWithDensity * radiusWithDensity - dir.y * dir.y);
	//涟漪上的点到涟漪最边缘的距离
	float distance2Border = (maxHorizontalDistance - horizontalDistance);
	//屏蔽球缩小的部分
	alpha *= saturate((voronoiCenter.y - WorldPosition.y) *  fade);
	//中心减弱
	alpha *= saturate(horizontalDistance / maxHorizontalDistance);
	//外边缘渐弱
	alpha *= saturate(distance2Border * 8);
	
	//从涟漪边缘往中心的-1到1的循环
	float distance2Border01 = saturate(distance2Border / radiusWithDensity);
	float distancePeriod = sin(distance2Border01 * ringCount * PI * 2);
	//涟漪造成的世界法线
	float3 normal = normalize(dir);
	normal.y = 0;
	normal *= distancePeriod;
	normal.y = sqrt(1 - (normal.x * normal.x + normal.z * normal.z));
	normal = clamp(-1, 1, normal);
	return float4(normal.xyz, alpha);
}

float3x3 Inverse3x3Rain(float3x3 input)
{
	float3 a = input._11_21_31;
	float3 b = input._12_22_32;
	float3 c = input._13_23_33;
	return float3x3(cross(b, c), cross(c, a), cross(a, b)) * (1.0 / dot(a, cross(b, c)));
}

//高配
float4 ComputeRippleWS(float3 worldPos)
{
	//float4 normalRipple = GetRippleNormalWS(worldPos, _RippleDensity, _RippleRadius, _RingCount, _DropSpeed, _RippleFade);
	//float4 normalRipple = GetRippleNormalWS(worldPos, _RippleTiling, 0.25, 7, _RippleSpeed, _RippleIntensity);
	float4 normalRipple = GetRippleNormalWS(worldPos, 2, 0.25, 5.5, _RippleSpeed * 0.5, _RippleIntensity * 2);
	return normalRipple;
}

//中低配
float3 ComputeRippleTS(float2 uv, float t)
{
    float4 ripple = SAMPLE_TEXTURE2D(_RippleTex, sampler_RippleTex, uv / _RippleTiling);
    ripple.yz = ripple.yz * 2.0 - 1.0;
    float dropFrac = frac(ripple.a + t * _RippleSpeed);
    float timeFrac = dropFrac - 1.0 + ripple.x;
    float dropFactor = 1 - saturate(dropFrac);
    float final = dropFactor * sin(clamp(timeFrac * 9.0, 0.0, 4.0) * 3.1415926);
    return float3(ripple.yz * final, 1.0);
}

float3 BlendNormals(float3 n1, float3 n2) {
    return normalize(float3(n1.xy + n2.xy, n1.z * n2.z));
}
#endif
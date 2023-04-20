#ifndef _FX_UV_ANIMATE_INCLUDE
#define _FX_UV_ANIMATE_INCLUDE

#define FX_PI 3.1415926
#define FX_DEG2RAD(d) ((d) * 0.0174532925)
#define FX_RAD2DEG(r) ((r) * 57.295779513)

float2x2 Rot(float degree)
{
	float2 sc;
	sincos(FX_DEG2RAD(degree),sc.x,sc.y);
	float2x2 rot = float2x2(sc.y,-sc.x,sc.x,sc.y);

	return rot;
}

float3x3 Rot(float degree,float2 offset)
{
	float2 sc;
	sincos(FX_DEG2RAD(degree),sc.x,sc.y);
	float3x3 rot = float3x3(
		sc.y,-sc.x, offset.x,
		sc.x, sc.y, offset.y,
		0,0,1);

	return rot;
}

float2 UVOffset(float2 uv,float2 scale,float2 offset)
{
	return uv * scale + offset;
}

float UVOffset(float u,float scale,float offset)
{
	return u * scale + offset;
}

float2 UVMul(float2x2 mat,float2 uv)
{
	return mul(mat,uv);
}

float2 UVMul(float3x3 mat,float2 uv)
{
	return mul(mat,float3(uv,1)).xy;
}

#endif
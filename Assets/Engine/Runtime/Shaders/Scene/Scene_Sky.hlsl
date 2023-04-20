#include "../StdLib.hlsl"
#include "../Colors.hlsl"

#ifndef SCENE_SKY_INCLUDE
#define SCENE_SKY_INCLUDE

#ifdef _VERTEX_FOG
	#include "../Include/Head.hlsl"
	#include "../Include/Fog.hlsl"
#endif

FLOAT _RotateSpeed;
#if !defined(SHADER_API_MOBILE)
FLOAT _FogDisable;
#endif

v2f vert (appdata_t v)
{
	v2f o;
	FLOAT3 rotated = RotateAroundYInDegrees(v.vertex.xyz,_Time.y*_RotateSpeed);
	FLOAT4 worldPos = mul(unity_ObjectToWorld, FLOAT4(rotated, 1.0));
	FLOAT3 viewPos = mul(unity_MatrixV, worldPos).xyz;
	o.vertex = mul(unity_MatrixVP, worldPos);
	o.texcoord = v.vertex.xyz;
	o.depth01 = o.vertex.zw;
	o.WorldPosition = worldPos;
	return o;
}			

MRTOutput frag (v2f i)// : SV_Target
{
	FLOAT4 tex;
	FLOAT3 tint;
	FLOAT exposure;
	GetData(tex,tint,exposure,i);
	DECLARE_OUTPUT(MRTOutput, mrt);


	tint*= FLOAT3(4.59479380, 4.59479380, 4.59479380);
	FLOAT3 c = DecodeHDR (tex, _Tex_HDR);
	c = c * tint;//unity_ColorSpaceDouble.rgb;
	c *= exposure;
	// c = ApplyFog_Sky(c, i.WorldPosition.xyz);
	c *= _SceneColor.rgb;
	mrt.rt0 = FLOAT4(c, 1);
	mrt.rt1.xyz = EncodeFloatRGB(i.depth01.x/i.depth01.y);
	SET_BLOOM(mrt, EncodeAlpha(1, _IsRt1zForUIRT));
	return mrt;
}

#endif //SCENE_SKY_INCLUDE 
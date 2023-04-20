// Copyright 2018- PWRD, Inc. All Rights Reserved.

#ifndef PBS_SH_INCLUDE
#define PBS_SH_INCLUDE

// SH lighting environment

FLOAT4 _AmbientParam;
#ifdef _LOCAL_VERTEX_GI
	FLOAT4 _Custom_SHAr;
	FLOAT4 _Custom_SHAg;
	FLOAT4 _Custom_SHAb;
	FLOAT4 _Custom_SHBr;
	FLOAT4 _Custom_SHBg;
	FLOAT4 _Custom_SHBb;
	FLOAT4 _Custom_SHC;
	// #define _SHWeight custom_SHC.w
	#define _SHAr _Custom_SHAr
	#define _SHAg _Custom_SHAg
	#define _SHAb _Custom_SHAb
	#define _SHBr _Custom_SHBr
	#define _SHBg _Custom_SHBg
	#define _SHBb _Custom_SHBb
	#define _SHC _Custom_SHC
#else //!_LOCAL_VERTEX_GI
	#ifdef _Role_Lighting
		FLOAT4 _Custom_SHAr;
		FLOAT4 _Custom_SHAg;
		FLOAT4 _Custom_SHAb;
		FLOAT4 _Custom_SHBr;
		FLOAT4 _Custom_SHBg;
		FLOAT4 _Custom_SHBb;
		FLOAT4 _Custom_SHC;
		#define _SHAr _Custom_SHAr
		#define _SHAg _Custom_SHAg
		#define _SHAb _Custom_SHAb
		#define _SHBr _Custom_SHBr
		#define _SHBg _Custom_SHBg
		#define _SHBb _Custom_SHBb
		#define _SHC _Custom_SHC
	#else//!_Role_Lighting
		#ifdef _Scene_Lighting
			FLOAT4 _Scene_SHAr;
			FLOAT4 _Scene_SHAg;
			FLOAT4 _Scene_SHAb;
			FLOAT4 _Scene_SHBr;
			FLOAT4 _Scene_SHBg;
			FLOAT4 _Scene_SHBb;
			FLOAT4 _Scene_SHC;
			#define _SHAr _Scene_SHAr
			#define _SHAg _Scene_SHAg
			#define _SHAb _Scene_SHAb
			#define _SHBr _Scene_SHBr
			#define _SHBg _Scene_SHBg
			#define _SHBb _Scene_SHBb
			#define _SHC _Scene_SHC
		#else//!_Scene_Lighting
			#define _SHAr unity_SHAr
			#define _SHAg unity_SHAg
			#define _SHAb unity_SHAb
			#define _SHBr unity_SHBr
			#define _SHBg unity_SHBg
			#define _SHBb unity_SHBb
			#define _SHC unity_SHC
		#endif//_Scene_Lighting
	#endif//_Role_Lighting
#endif//_LOCAL_VERTEX_GI



inline FLOAT3 SHEvalLinearL0L1(FLOAT4 normal,FLOAT4 ar,FLOAT4 ag,FLOAT4 ab)
{
	FLOAT3 x;

	// Linear (L1) + constant (L0) polynomial terms
	x.r = dot(ar, normal);
	x.g = dot(ag, normal);
	x.b = dot(ab, normal);

	return x;
}

inline FLOAT3 SHEvalLinearL2(FLOAT4 normal,FLOAT4 br,FLOAT4 bg,FLOAT4 bb,FLOAT4 c)
{
	FLOAT3 x1, x2;
	// 4 of the quadratic (L2) polynomials
	FLOAT4 vB = normal.xyzz * normal.yzzx;
	x1.r = dot(br, vB);
	x1.g = dot(bg, vB);
	x1.b = dot(bb, vB);

	// Final (5th) quadratic (L2) polynomial
	FLOAT vC = normal.x*normal.x - normal.y*normal.y;
	x2 = c.rgb * vC;

	return x1 + x2;
}

//SH Lighting

//calc L0L1
inline FLOAT3 SHPerVertex(FLOAT3 WorldNormal)
{
	return SHEvalLinearL0L1(FLOAT4(WorldNormal,1.0),_SHAr,_SHAg,_SHAb);//*(1-_SHWeight)+
		//SHEvalLinearL0L1(FLOAT4(WorldNormal,1.0),custom_SHAr,custom_SHAg,custom_SHAb)*_SHWeight;
}

//calc L2
FLOAT3 SHPerPixel(FLOAT3 normal, FLOAT3 ambient, FLOAT3 worldPos)
{
#ifdef _VERTEX_GI
	// Completely per-pixel
	ambient += SHEvalLinearL2(FLOAT4(normal, 1.0),_SHBr,_SHBg,_SHBb,_SHC);//*(1-_SHWeight)+
		//SHEvalLinearL2(FLOAT4(normal,1.0),custom_SHBr,custom_SHBg,custom_SHBb,custom_SHC)*_SHWeight;

	ambient = max(FLOAT3(0,0,0), ambient);

#endif

	return ambient;
}

#endif //PBS_SH_INCLUDE
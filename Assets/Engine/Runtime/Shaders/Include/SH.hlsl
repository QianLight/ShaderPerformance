// Copyright 2018- PWRD, Inc. All Rights Reserved.

#ifndef PBS_SH_INCLUDE
#define PBS_SH_INCLUDE

// SH lighting environment

FLOAT4 _AmbientParam;
#ifdef _LOCAL_VERTEX_GI
	FLOAT4 custom_SHAr;
	FLOAT4 custom_SHAg;
	FLOAT4 custom_SHAb;
	FLOAT4 custom_SHBr;
	FLOAT4 custom_SHBg;
	FLOAT4 custom_SHBb;
	FLOAT4 custom_SHC;
	// #define _SHWeight custom_SHC.w
	#define _SHAr custom_SHAr
	#define _SHAg custom_SHAg
	#define _SHAb custom_SHAb
	#define _SHBr custom_SHBr
	#define _SHBg custom_SHBg
	#define _SHBb custom_SHBb
	#define _SHC custom_SHC
#else //!_LOCAL_VERTEX_GI
	#ifdef _Role_Lighting
		#ifndef _ROLE_RENDER_V2
			FLOAT4 custom_SHAr;
			FLOAT4 custom_SHAg;
			FLOAT4 custom_SHAb;
			FLOAT4 custom_SHBr;
			FLOAT4 custom_SHBg;
			FLOAT4 custom_SHBb;
			FLOAT4 custom_SHC;
			#define _SHAr custom_SHAr
			#define _SHAg custom_SHAg
			#define _SHAb custom_SHAb
			#define _SHBr custom_SHBr
			#define _SHBg custom_SHBg
			#define _SHBb custom_SHBb
			#define _SHC custom_SHC
		#else
			FLOAT4 custom_ShV2Ar;
			FLOAT4 custom_ShV2Ag;
			FLOAT4 custom_ShV2Ab;
			FLOAT4 custom_ShV2Br;
			FLOAT4 custom_ShV2Bg;
			FLOAT4 custom_ShV2Bb;
			FLOAT4 custom_ShV2C;
			#define _SHAr custom_ShV2Ar
			#define _SHAg custom_ShV2Ag
			#define _SHAb custom_ShV2Ab
			#define _SHBr custom_ShV2Br
			#define _SHBg custom_ShV2Bg
			#define _SHBb custom_ShV2Bb
			#define _SHC  custom_ShV2C
		#endif
	#else//!_Role_Lighting
		#ifdef _Scene_Lighting
			FLOAT4 scene_SHAr;
			FLOAT4 scene_SHAg;
			FLOAT4 scene_SHAb;
			FLOAT4 scene_SHBr;
			FLOAT4 scene_SHBg;
			FLOAT4 scene_SHBb;
			FLOAT4 scene_SHC;
			#define _SHAr scene_SHAr
			#define _SHAg scene_SHAg
			#define _SHAb scene_SHAb
			#define _SHBr scene_SHBr
			#define _SHBg scene_SHBg
			#define _SHBb scene_SHBb
			#define _SHC scene_SHC
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
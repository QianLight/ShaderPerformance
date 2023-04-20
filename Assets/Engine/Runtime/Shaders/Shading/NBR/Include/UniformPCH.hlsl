#if !defined(UNIFORM_PCH_INCLUDE) && !defined(UNIFORM_PCH_OFF)
#define UNIFORM_PCH_INCLUDE

CBUFFER_START(UnityPerMaterial)
// #ifndef UNIFORM_PCH_OFF
FLOAT4 _HeightGradientColor;
FLOAT4 _HeightGradientPreviewParam;
FLOAT4 _HeightGradientPreivewColor;
// #endif
// #if !defined(ROLE_SRP_BATCH)
FLOAT4 _ColorOutline;
FLOAT4 _OutlineScale;
FLOAT4 _MatEffectOutlineParam;
// #endif

FLOAT4 _SSRimColor;
FLOAT4 _SSRimParam0;

FLOAT4 _EdgeColor;
FLOAT4 _BackColor;
FLOAT4 _GrassCollisionParam;
FLOAT4 _LaserColor;
FLOAT4 _FabricColor1;
FLOAT4 _FabricColor2;
FLOAT4 _LocalOffset;
FLOAT4 _ShadowPos;

TYPE4 _Param00, _Param01, _Param02, _Color01,_Color00;;
REAL4 _UVST0;
REAL4 _UVST1;

REAL4 _Color;//only for custom effect
REAL4 _Color0;
REAL4 _Color1;
REAL4 _Color2;

REAL4 _Param;
REAL4 _Param0;
REAL4 _Param1;
REAL4 _Param2;
REAL4 _Param3;
REAL4 _Param4;
REAL4 _Param5;
//TYPE4 _Param00, _Param01, _Param02, _Color01;
REAL4 _PBRParam;
REAL4 _LightMapUVST;

REAL4 _ProcedureTex1_TexelSize;
// REAL4 custom_SHAr;
// REAL4 custom_SHAg;
// REAL4 custom_SHAb;
// REAL4 custom_SHBr;
// REAL4 custom_SHBg;
// REAL4 custom_SHBb;
// REAL4 custom_SHC;
//FLOAT4 _BloomMaskParam;
REAL4 _Color3;
REAL4 _Color4;
REAL4 _Color5;
REAL4 _Color6;

//Laster
half4 _RimColortoon;
REAL4 _AnisotropyParam;
REAL4 _UVST3;
REAL _DebugMode;
CBUFFER_END

REAL4 _Param6;
REAL4 _Param7;
REAL4 _Param8;

REAL4 _UVST2;

REAL4 _CustomTime;

TEX2D_SAMPLER(_MainTex);
TEX2DARRAY_SAMPLER(_AtlasTex0);
TEX2DARRAY_SAMPLER(_AtlasTex1);
TEX2DARRAY_SAMPLER(_AtlasTex2);
TEX2D_SAMPLER(_MainTex1);
TEX2D_SAMPLER(_MainTex2);
TEX2D_SAMPLER(_MainTex3);

TEX2D_SAMPLER(_ProcedureTex0);
TEX2D_SAMPLER(_ProcedureTex1);
TEX2D_SAMPLER(_ProcedureTex2);
TEX2D_SAMPLER(_ProcedureTex3);
TEX2D_SAMPLER(_ProcedureTex4);

TEXCUBE_SAMPLER(_EnvCube);
TEXCUBE_SAMPLER(_LocalEnvCube);
TEX2D_SAMPLER(_PreIntegrateSSS);
TEX2D_SAMPLER(_AnisoShift);

#define _MainColor _Color0

#endif //UNIFORM_PCH_INCLUDE
#define SCREEN_SPACE_RIM_IS_IN_COMMON_CBUFFER
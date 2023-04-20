// Copyright 1998-2018 Epic Games, Inc. All Rights Reserved.

/*=============================================================================
	BRDF.usf: Bidirectional reflectance distribution functions.
=============================================================================*/

#ifndef __BRDF_COMMON__
#define __BRDF_COMMON__

// Physically based shading model
// parameterized with the below options

// Microfacet specular = D*G*F / (4*NoL*NoV) = D*Vis*F
// Vis = G / (4*NoL*NoV)


inline FLOAT3 Diffuse_Lambert( FLOAT3 DiffuseColor )
{
	return DiffuseColor;
}

// [Burley 2012, "Physically-Based Shading at Disney"]
inline FLOAT3 Diffuse_Burley( FLOAT3 DiffuseColor, FLOAT Roughness, FLOAT NoV, FLOAT NoL, FLOAT VoH )
{
	FLOAT FD90 = 0.5 + 2 * VoH * VoH * Roughness;
	FLOAT FdV = 1 + (FD90 - 1) * Pow5( 1 - NoV );
	FLOAT FdL = 1 + (FD90 - 1) * Pow5( 1 - NoL );
	return DiffuseColor * ( /*(1 / PI) **/ FdV * FdL );//hack divide PI
}



// [Gotanda 2012, "Beyond a Simple Physically Based Blinn-Phong Model in Real-Time"]
inline FLOAT3 Diffuse_OrenNayar( FLOAT3 DiffuseColor, FLOAT Roughness, FLOAT NoV, FLOAT NoL, FLOAT VoH )
{
	FLOAT a = Roughness * Roughness;
	FLOAT s = a;// / ( 1.29 + 0.5 * a );
	FLOAT s2 = s * s;
	FLOAT VoL = 2 * VoH * VoH - 1;		// double angle identity
	FLOAT Cosri = VoL - NoV * NoL;
	FLOAT C1 = 1 - 0.5 * s2 / (s2 + 0.33);
	FLOAT C2 = 0.45 * s2 / (s2 + 0.09) * Cosri * ( Cosri >= 0 ? rcp( max( NoL, NoV ) ) : 1 );
	return DiffuseColor / PI * ( C1 + C2 ) * ( 1 + Roughness * 0.5 );
}

// [Gotanda 2014, "Designing Reflectance Models for New Consoles"]
inline FLOAT3 Diffuse_Gotanda( FLOAT3 DiffuseColor, FLOAT Roughness, FLOAT NoV, FLOAT NoL, FLOAT VoH )
{
	FLOAT a = Roughness * Roughness;
	FLOAT a2 = a * a;
	FLOAT F0 = 0.04;
	FLOAT VoL = 2 * VoH * VoH - 1;		// double angle identity
	FLOAT Cosri = VoL - NoV * NoL;
#if 1
	FLOAT a2_13 = a2 + 1.36053;
	FLOAT Fr = ( 1 - ( 0.542026*a2 + 0.303573*a ) / a2_13 ) * ( 1 - pow( 1 - NoV, 5 - 4*a2 ) / a2_13 ) * ( ( -0.733996*a2*a + 1.50912*a2 - 1.16402*a ) * pow( 1 - NoV, 1 + rcp(39*a2*a2+1) ) + 1 );
	//FLOAT Fr = ( 1 - 0.36 * a ) * ( 1 - pow( 1 - NoV, 5 - 4*a2 ) / a2_13 ) * ( -2.5 * Roughness * ( 1 - NoV ) + 1 );
	FLOAT Lm = ( max( 1 - 2*a, 0 ) * ( 1 - Pow5( 1 - NoL ) ) + min( 2*a, 1 ) ) * ( 1 - 0.5*a * (NoL - 1) ) * NoL;
	FLOAT Vd = ( a2 / ( (a2 + 0.09) * (1.31072 + 0.995584 * NoV) ) ) * ( 1 - pow( 1 - NoL, ( 1 - 0.3726732 * NoV * NoV ) / ( 0.188566 + 0.38841 * NoV ) ) );
	FLOAT Bp = Cosri < 0 ? 1.4 * NoV * NoL * Cosri : Cosri;
	FLOAT Lr = (21.0 / 20.0) * (1 - F0) * ( Fr * Lm + Vd + Bp );
	return DiffuseColor / PI * Lr;
#else
	FLOAT a2_13 = a2 + 1.36053;
	FLOAT Fr = ( 1 - ( 0.542026*a2 + 0.303573*a ) / a2_13 ) * ( 1 - pow( 1 - NoV, 5 - 4*a2 ) / a2_13 ) * ( ( -0.733996*a2*a + 1.50912*a2 - 1.16402*a ) * pow( 1 - NoV, 1 + rcp(39*a2*a2+1) ) + 1 );
	FLOAT Lm = ( max( 1 - 2*a, 0 ) * ( 1 - Pow5( 1 - NoL ) ) + min( 2*a, 1 ) ) * ( 1 - 0.5*a + 0.5*a * NoL );
	FLOAT Vd = ( a2 / ( (a2 + 0.09) * (1.31072 + 0.995584 * NoV) ) ) * ( 1 - pow( 1 - NoL, ( 1 - 0.3726732 * NoV * NoV ) / ( 0.188566 + 0.38841 * NoV ) ) );
	FLOAT Bp = Cosri < 0 ? 1.4 * NoV * Cosri : Cosri / max( NoL, 1e-8 );
	FLOAT Lr = (21.0 / 20.0) * (1 - F0) * ( Fr * Lm + Vd + Bp );
	return DiffuseColor / PI * Lr;
#endif
}

// [Blinn 1977, "Models of light reflection for computer synthesized pictures"]
inline FLOAT D_Blinn( FLOAT Roughness, FLOAT NoH )
{
	FLOAT a = Roughness * Roughness;
	FLOAT a2 = a * a;
	FLOAT n = 2 / a2 - 2;
	return (n+2) / (2*PI) * PhongShadingPow( NoH, n );		// 1 mad, 1 exp, 1 mul, 1 log
}

// [Beckmann 1963, "The scattering of electromagnetic waves from rough surfaces"]
inline FLOAT D_Beckmann( FLOAT Roughness, FLOAT NoH )
{
	FLOAT a = Roughness * Roughness;
	FLOAT a2 = a * a;
	FLOAT NoH2 = NoH * NoH;
	return exp( (NoH2 - 1) / (a2 * NoH2) ) / ( PI * a2 * NoH2 * NoH2 );
}

// GGX / Trowbridge-Reitz
// [Walter et al. 2007, "Microfacet models for refraction through rough surfaces"]
// float D_GGX( float a2, float NoH )
// {
// 	float d = ( NoH * a2 - NoH ) * NoH + 1;	// 2 mad
// 	return a2 / ( PI*d*d );					// 4 mul, 1 rcp
// }

inline float D_GGX( float a2, float NoH )
{
	float d = ( NoH * a2 - NoH ) * NoH + 1;	// 2 mad
	return INV_PI * a2 / (d * d + 1e-7f);
}
inline float D_GGX2( float a,float a2, float NoH )
{
	float d = ( NoH * a2 - NoH ) * NoH + 1;	// 2 mad
	float ad = a/d;
	return INV_PI * ad*ad;
}
// Anisotropic GGX
// [Burley 2012, "Physically-Based Shading at Disney"]
inline FLOAT D_GGXaniso( FLOAT RoughnessX, FLOAT RoughnessY, FLOAT NoH, FLOAT3 H, FLOAT3 X, FLOAT3 Y )
{
	FLOAT ax = RoughnessX * RoughnessX;
	FLOAT ay = RoughnessY * RoughnessY;
	FLOAT XoH = dot( X, H );
	FLOAT YoH = dot( Y, H );
	FLOAT d = XoH*XoH / (ax*ax) + YoH*YoH / (ay*ay) + NoH*NoH;
	return 1 / ( PI * ax*ay * d*d );
}
inline FLOAT D_GGXaniso2( FLOAT RoughnessT, FLOAT RoughnessB, FLOAT NoH, FLOAT ToH, FLOAT BoH )
{
	FLOAT aT = RoughnessT;
	FLOAT aB = RoughnessB;
	FLOAT d = ToH * ToH / (aT*aT) + BoH * BoH / (aB*aB) + NoH * NoH;
	return 1.0 / ( aT*aB * d*d * PI + 1e-4h );
}

inline FLOAT V_SmithJointGGXAniso(FLOAT ToV, FLOAT BoV, FLOAT NoV, FLOAT ToL, FLOAT BoL, FLOAT NoL, FLOAT RoughnessT, FLOAT RoughnessB)
{
	FLOAT aT = RoughnessT;
	FLOAT aT2 = aT * aT;
	FLOAT aB = RoughnessB;
	FLOAT aB2 = aB * aB;
	FLOAT lambdaV = NoL * sqrt(aT2 * ToV * ToV + aB2 * BoV * BoV + NoV * NoV);
	FLOAT lambdaL = NoV * sqrt(aT2 * ToL * ToL + aB2 * BoL * BoL + NoL * NoL);
	return 0.5 / (lambdaV + lambdaL + 1e-4h );
}
// inline float GGXTerm (float NdotH, float roughness)
// {
//     float a2 = roughness * roughness;
//     float d = (NdotH * a2 - NdotH) * NdotH + 1.0f; // 2 mad
//     return UNITY_INV_PI * a2 / (d * d + 1e-7f); // This function is not intended to be running on Mobile,
//                                             // therefore epsilon is smaller than what can be represented by FLOAT
// }
inline FLOAT Vis_Implicit()
{
	return 0.25;
}

// [Neumann et al. 1999, "Compact metallic reflectance models"]
inline FLOAT Vis_Neumann( FLOAT NoV, FLOAT NoL )
{
	return 1 / ( 4 * max( NoL, NoV ) );
}

// [Kelemen 2001, "A microfacet based coupled specular-matte brdf model with importance sampling"]
inline FLOAT Vis_Kelemen( FLOAT VoH )
{
	// constant to prevent NaN
	return rcp( 4 * VoH * VoH + 1e-5);
}

// Tuned to match behavior of Vis_Smith
// [Schlick 1994, "An Inexpensive BRDF Model for Physically-Based Rendering"]
inline FLOAT Vis_Schlick( FLOAT Roughness, FLOAT NoV, FLOAT NoL )
{
	FLOAT k = Square( Roughness ) * 0.5;
	FLOAT Vis_SchlickV = NoV * (1 - k) + k;
	FLOAT Vis_SchlickL = NoL * (1 - k) + k;
	return 0.25 / ( Vis_SchlickV * Vis_SchlickL );
}

// Smith term for GGX
// [Smith 1967, "Geometrical shadowing of a random rough surface"]
inline FLOAT Vis_Smith( FLOAT Roughness, FLOAT NoV, FLOAT NoL )
{
	FLOAT a = Square( Roughness );
	FLOAT a2 = a*a;

	FLOAT Vis_SmithV = NoV + sqrt( NoV * (NoV - NoV * a2) + a2 );
	FLOAT Vis_SmithL = NoL + sqrt( NoL * (NoL - NoL * a2) + a2 );
	return rcp( Vis_SmithV * Vis_SmithL );
}

// Appoximation of joint Smith term for GGX
// [Heitz 2014, "Understanding the Masking-Shadowing Function in Microfacet-Based BRDFs"]
inline FLOAT Vis_SmithJointApprox( FLOAT a, FLOAT NoV, FLOAT NoL )
{
	// FLOAT a = Roughness;//Square( Roughness );
	// FLOAT Vis_SmithV = NoL * ( NoV * ( 1 - a ) + a );
	// FLOAT Vis_SmithL = NoV * ( NoL * ( 1 - a ) + a );
	// // Note: will generate NaNs with Roughness = 0.  MinRoughness is used to prevent this
	// return 0.5 * rcp( Vis_SmithV + Vis_SmithL + 1e-5f );

	FLOAT vis_SmithApprox = lerp(NoV*NoL,(NoV+NoL),a);
	return 0.5 * rcp(vis_SmithApprox + 1e-5f);

}

// // Ref: http://jcgt.org/published/0003/02/03/paper.pdf
// inline FLOAT SmithJointGGXVisibilityTerm (FLOAT NdotL, FLOAT NdotV, FLOAT roughness)
// {
//     // Approximation of the above formulation (simplify the sqrt, not mathematically correct but close enough)
//     FLOAT a = roughness;
//     FLOAT lambdaV = NdotL * (NdotV * (1 - a) + a);
//     FLOAT lambdaL = NdotV * (NdotL * (1 - a) + a);

//     return 0.5f * rcp(lambdaV + lambdaL + 1e-5f);
// }

inline FLOAT3 F_None( FLOAT3 SpecularColor )
{
	return SpecularColor;
}

// [Schlick 1994, "An Inexpensive BRDF Model for Physically-Based Rendering"]
inline FLOAT3 F_Schlick( FLOAT3 SpecularColor, FLOAT VoH )
{
	FLOAT Fc = Pow5( 1 - VoH );					// 1 sub, 3 mul
	//return Fc + (1 - Fc) * SpecularColor;		// 1 add, 3 mad
	
	// Anything less than 2% is physically impossible and is instead considered to be shadowing
	return saturate( 50.0 * SpecularColor.g ) * Fc + (1 - Fc) * SpecularColor;
	
}

inline FLOAT3 F_Fresnel( FLOAT3 SpecularColor, FLOAT VoH )
{
	FLOAT3 SpecularColorSqrt = sqrt( clamp( FLOAT3(0, 0, 0), FLOAT3(0.99, 0.99, 0.99), SpecularColor ) );
	FLOAT3 n = ( 1 + SpecularColorSqrt ) / ( 1 - SpecularColorSqrt );
	FLOAT3 g = sqrt( n*n + VoH*VoH - 1 );
	return 0.5 * Square( (g - VoH) / (g + VoH) ) * ( 1 + Square( ((g+VoH)*VoH - 1) / ((g-VoH)*VoH + 1) ) );
}

inline FLOAT3 FresnelLerp (FLOAT3 F0, FLOAT3 F90, FLOAT cosA)
{
    FLOAT t = Pow5 (1 - cosA);   // ala Schlick interpoliation
    return lerp (F0, F90, t);
}

inline FLOAT3 FresnelTerm (FLOAT3 F0, FLOAT cosA)
{
    FLOAT t = Pow5 (1 - cosA);   // ala Schlick interpoliation
    return F0 + (1-F0) * t;
}
//---------------
// EnvBRDF
//---------------

//Texture2D		PreIntegratedGF;
//SamplerState	PreIntegratedGFSampler;
//
//FLOAT3 EnvBRDF( FLOAT3 SpecularColor, FLOAT Roughness, FLOAT NoV )
//{
//	// Importance sampled preintegrated G * F
//	FLOAT2 AB = Texture2DSampleLevel( PreIntegratedGF, PreIntegratedGFSampler, FLOAT2( NoV, Roughness ), 0 ).rg;
//
//	// Anything less than 2% is physically impossible and is instead considered to be shadowing 
//	FLOAT3 GF = SpecularColor * AB.x + saturate( 50.0 * SpecularColor.g ) * AB.y;
//	return GF;
//}

inline FLOAT3 EnvBRDFApprox( FLOAT3 SpecularColor, FLOAT Roughness, FLOAT NoV )
{
	// [ Lazarov 2013, "Getting More Physical in Call of Duty: Black Ops II" ]
	// Adaptation to fit our G term.
	const FLOAT4 c0 = { -1, -0.0275, -0.572, 0.022 };
	const FLOAT4 c1 = { 1, 0.0425, 1.04, -0.04 };
	FLOAT4 r = Roughness * c0 + c1;
	FLOAT a004 = min( r.x * r.x, exp2( -9.28 * NoV ) ) * r.x + r.y;
	FLOAT2 AB = FLOAT2( -1.04, 1.04 ) * a004 + r.zw;

	// Anything less than 2% is physically impossible and is instead considered to be shadowing
	// Note: this is needed for the 'specular' show flag to work, since it uses a SpecularColor of 0
	AB.y *= saturate( 50.0 * SpecularColor.g );

	return SpecularColor * AB.x + AB.y;
}

inline FLOAT EnvBRDFApproxNonmetal( FLOAT Roughness, FLOAT NoV )
{
	// Same as EnvBRDFApprox( 0.04, Roughness, NoV )
	const FLOAT2 c0 = { -1, -0.0275 };
	const FLOAT2 c1 = { 1, 0.0425 };
	FLOAT2 r = Roughness * c0 + c1;
	return min( r.x * r.x, exp2( -9.28 * NoV ) ) * r.x + r.y;
}

float3 EnvironmentBRDF( float g, float NoV, float3 rf0 )
{
    float4 t = float4( 1/0.96, 0.475, (0.0275 - 0.25 * 0.04)/0.96, 0.25 );
    t *= float4( g, g, g, g );
    t += float4( 0, 0, (0.015 - 0.75 * 0.04)/0.96, 0.75 );
    float a0 = t.x * min( t.y, exp2( -9.28 * NoV ) ) + t.z;
    float a1 = t.w;
    return saturate( a0 + rf0 * ( a1 - a0 ) );
}
inline FLOAT D_InvBlinn( FLOAT Roughness, FLOAT NoH )
{
	FLOAT m = Roughness * Roughness;
	FLOAT m2 = m * m;
	FLOAT A = 4;
	FLOAT Cos2h = NoH * NoH;
	FLOAT Sin2h = 1 - Cos2h;
	//return rcp( PI * (1 + A*m2) ) * ( 1 + A * ClampedPow( Sin2h, 1 / m2 - 1 ) );
	return rcp( PI * (1 + A*m2) ) * ( 1 + A * exp( -Cos2h / m2 ) );
}

inline FLOAT D_InvBeckmann( FLOAT Roughness, FLOAT NoH )
{
	FLOAT m = Roughness * Roughness;
	FLOAT m2 = m * m;
	FLOAT A = 4;
	FLOAT Cos2h = NoH * NoH;
	FLOAT Sin2h = 1 - Cos2h;
	FLOAT Sin4h = Sin2h * Sin2h;
	return rcp( PI * (1 + A*m2) * Sin4h ) * ( Sin4h + A * exp( -Cos2h / (m2 * Sin2h) ) );
}

inline FLOAT D_InvGGX( FLOAT Roughness, FLOAT NoH )
{
	FLOAT a = Roughness * Roughness;
	FLOAT a2 = a * a;
	FLOAT A = 4;
	FLOAT d = ( NoH - a2 * NoH ) * NoH + a2;
	return rcp( PI * (1 + A*a2) ) * ( 1 + 4 * a2*a2 / ( d*d ) );
}

inline FLOAT Vis_Cloth( FLOAT NoV, FLOAT NoL )
{
	return rcp( 4 * ( NoL + NoV - NoL * NoV ) );
}

#define MEDIUMP_FLT_MAX    65504.0
#define MEDIUMP_FLT_MIN    0.00006103515625

#define saturateMediump(x) min(x, MEDIUMP_FLT_MAX)

inline FLOAT CalcSpecular(FLOAT Roughness, FLOAT NoH, FLOAT3 H, FLOAT3 N)
{
	//GGX_Mobile
	FLOAT3 NxH = cross(N, H);
	FLOAT OneMinusNoHSqr = dot(NxH, NxH);

	FLOAT a = max(0.01,Roughness * Roughness);
	FLOAT n = NoH * a;
	FLOAT p = a / (OneMinusNoHSqr + n * n);
	FLOAT d = p * p;

	return (Roughness*0.25 + 0.25) * saturateMediump(d);
}

inline FLOAT CalcSpecular(FLOAT a, FLOAT NoH, FLOAT3 H, FLOAT3 N,FLOAT param)
{
	//GGX_Mobile
	FLOAT3 NxH = cross(N, H);
	FLOAT OneMinusNoHSqr = dot(NxH, NxH);

	// FLOAT a = max(0.01,Roughness * Roughness);
	FLOAT n = NoH * a;
	FLOAT p = a / (OneMinusNoHSqr + n * n);
	FLOAT d = p * p;

	return param * saturateMediump(d);
}

inline FLOAT3 Diffuse_Burley(FLOAT Roughness, FLOAT NoL, FLOAT VoH,FLOAT forawrdCalc )
{
	FLOAT FD90 = 0.5 + 2 * VoH * VoH * Roughness;
	FLOAT FdV = 1 + (FD90 - 1) * forawrdCalc;
	FLOAT FdL = 1 + (FD90 - 1) * Pow5( 1 - NoL );
	return FdV * FdL;//hack divide PI
}
inline FLOAT3 Diffuse_OrenNayar(FLOAT Roughness, FLOAT NoV, FLOAT NoL, FLOAT VoH )
{
	FLOAT a = Roughness * Roughness;
	FLOAT s = a;// / ( 1.29 + 0.5 * a );
	FLOAT s2 = s * s;
	FLOAT VoL = 2 * VoH * VoH - 1;		// double angle identity
	FLOAT Cosri = VoL - NoV * NoL;
	FLOAT C1 = 1 - 0.5 * s2 / (s2 + 0.33);
	FLOAT C2 = 0.45 * s2 / (s2 + 0.09) * Cosri * ( Cosri >= 0 ? rcp( max( NoL, NoV ) ) : 1 );
	return ( C1 + C2 ) * ( 1 + Roughness * 0.5 );
}

// inline FLOAT3 SpecularGGX( FLOAT a2, FLOAT NdotH,FLOAT NdotC, FLOAT NoL, FLOAT LdotH,FLOAT3 SpecularColor)
// {
// 	//FLOAT a2 = Roughness * Roughness;//Pow4( Roughness );
	
// 	// Generalized microfacet specular
// 	FLOAT D = D_GGX( a2, NdotH );
// 	FLOAT Vis = Vis_SmithJointApprox( a2, NdotC, NoL );
// 	FLOAT3 F = F_Schlick( SpecularColor, LdotH );
// 	return (D * Vis) * F;
// }

inline FLOAT SpecularGGX( FLOAT Roughness, FLOAT NoH, FLOAT LdotH)
{
	// GGX Distribution multiplied by combined approximation of Visibility and Fresnel
	// See "Optimizing PBR for Mobile" from Siggraph 2015 moving mobile graphics course
	// https://community.arm.com/events/1155
	FLOAT a = Roughness;
	FLOAT a2 = a*a;

	FLOAT d = NoH * NoH * (a2 - 1) + 1.00001;
  	return a2 / (max(0.1, LdotH*LdotH) * (Roughness + 0.5) * (d * d) * 4);
}

///////////////////////////////////Modify-BSDF///////////////////////////////////////

FLOAT Diffuse_RenormalizeBurley_NoPi(FLOAT LoH, FLOAT NoL, FLOAT NoV, FLOAT Roughness)
{
	Roughness = Roughness*Roughness;
	FLOAT EnergyBias = lerp(0, 0.5, Roughness);
	FLOAT EnergyFactor = lerp(1, 1 / 0.662, Roughness);
	FLOAT F90 = EnergyBias + 2 * LoH*LoH * Roughness;
	FLOAT lightScatter = 1 + (F90 - 1) * Pow5( 1 - NoL );
	FLOAT viewScatter = 1 + (F90 - 1) * Pow5( 1 - NoV );
	return lightScatter * viewScatter * EnergyFactor;
}

// Note: Disney diffuse must be multiply by diffuseAlbedo / PI. This is done outside of this function.
FLOAT DisneyDiffuse(FLOAT Roughness,FLOAT NdotV, FLOAT NdotL, FLOAT LdotH)
{
    FLOAT fd90 = 0.5 + 2 * LdotH * LdotH * Roughness;
    // Two schlick fresnel term
    FLOAT lightScatter   = (1 + (fd90 - 1) * Pow5(1 - NdotL));
    FLOAT viewScatter    = (1 + (fd90 - 1) * Pow5(1 - NdotV));

    return lightScatter * viewScatter;
}

FLOAT DisneyDiffuse_Supplement(FLOAT Roughness,FLOAT NdotV5, FLOAT NdotL, FLOAT LdotH)
{
	
    FLOAT fd90 = 0.5 + 2 * LdotH * LdotH * Roughness;
    FLOAT lightScatter  = (1 + (fd90 - 1) * Pow5(1 - NdotL));
	FLOAT viewScatter   = (1 + (fd90 - 1) * NdotV5);

    return lightScatter*viewScatter;
}

#endif // __BRDF_COMMON__
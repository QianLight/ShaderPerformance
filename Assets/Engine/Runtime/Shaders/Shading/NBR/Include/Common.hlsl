
/**
* Common.hlsl: Common fun and paramaters.
*/
#ifndef PBS_COMMON_INCLUDE
#define PBS_COMMON_INCLUDE
#if !defined(REDEFINE_URP)
inline FLOAT4 TransformWorldToClipPos(in FLOAT4 WorldPosition,out FLOAT3 viewPos)
{
	viewPos = mul(unity_MatrixV, WorldPosition).xyz;
	return mul(_matrixVP, FLOAT4(WorldPosition.xyz, 1.0));
}
#endif //REDEFINE_URP
inline FLOAT4 TransformWorldToClipPos2(in FLOAT4 WorldPosition)
{
	return mul(_matrixVP, WorldPosition);
}
#if !defined(REDEFINE_URP)
inline FLOAT4 TransformObjectToClipPos(in FLOAT3 pos)
{
	// More efficient than computing M*VP matrix product
	return mul(_matrixVP, mul(_objectToWorld, FLOAT4(pos, 1.0)));
}

// Transforms direction from object to world space
inline FLOAT3 ObjectToWorldDir(in FLOAT3 dir)
{
	return normalize(mul((FLOAT3x3)_objectToWorld, dir));
}
inline REAL3 SafeNormalize(REAL3 inVec)
{
    REAL dp3 = max(FLT_MIN, dot(inVec, inVec));
    return inVec * rsqrt(dp3);
}
#endif
// Transforms normal from object to world space
inline FLOAT3 ObjectToWorldNormal(in FLOAT3 norm)
{
#ifdef UNITY_ASSUME_UNIFORM_SCALING
	return ObjectToWorldDir(norm);
#else
	// mul(IT_M, norm) => mul(norm, I_M) => {dot(norm, I_M.col0), dot(norm, I_M.col1), dot(norm, I_M.col2)}
	return SafeNormalize(mul(norm, (FLOAT3x3)_worldToObject));
#endif
}

#if !defined(REDEFINE_URP)

inline FLOAT RangeMask(FLOAT v,FLOAT minV,FLOAT maxV)
{
	v = maxV - saturate(maxV - v);
	return saturate(v - minV);
}

#endif //REDEFINE_URP
FLOAT3 Rot(in FLOAT4 rotation, FLOAT3 vec)
{
	FLOAT x = rotation.x * 2;
	FLOAT y = rotation.y * 2;
	FLOAT z = rotation.z * 2;
	FLOAT xx = rotation.x * x;
	FLOAT yy = rotation.y * y;
	FLOAT zz = rotation.z * z;
	FLOAT xy = rotation.x * y;
	FLOAT xz = rotation.x * z;
	FLOAT yz = rotation.y * z;
	FLOAT wx = rotation.w * x;
	FLOAT wy = rotation.w * y;
	FLOAT wz = rotation.w * z;

	FLOAT3 res;
	res.x = (1 - (yy + zz)) * vec.x + (xy - wz) * vec.y + (xz + wy) * vec.z;
	res.y = (xy + wz) * vec.x + (1 - (xx + zz)) * vec.y + (yz - wx) * vec.z;
	res.z = (xz - wy) * vec.x + (yz + wx) * vec.y + (1 - (xx + yy)) * vec.z;
	return res;
}

inline FLOAT3 TransformObjectToWorldDir(FLOAT3 dirOS,FLOAT4 rot)
{
	#if defined(_INSTANCE) && !defined(_SIMPLE_MATRIX)
		return SafeNormalize(mul((FLOAT3x3)custom_ObjectToWorld, SafeNormalize(Rot(rot,dirOS))));
	#else
		// Normalize to support uniform scaling
		#ifdef _SIMPLE_MATRIX		
			return SafeNormalize(mul((FLOAT3x3)custom_ObjectToWorld, dirOS));
		#else		
			return SafeNormalize(mul((FLOAT3x3)unity_ObjectToWorld, dirOS));
		#endif
	#endif
}


inline REAL3 UnpackNormal(REAL2 packednormal,REAL normalScale)
{
	REAL3 normal = REAL3(packednormal,1)*2-1;
	normal = normalize(normal);
	normal.xy = packednormal.xy * 2 - 1;
	#ifdef _TICK_NORMAL
		normal.xy *= normalScale;
	#endif//_TICK_NORMAL
	normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
	// #ifdef _TICK_NORMAL
	// 	normal.z *= 0.5;
	// #endif//_TICK_NORMAL
	return normal;
}
#if !defined(REDEFINE_URP)

inline FLOAT3 BlendValue(FLOAT v0, FLOAT v1, FLOAT2 blend)
{
	return v0*blend.x + v1*blend.y;
}

inline FLOAT3 BlendValue(FLOAT v0, FLOAT v1, FLOAT v2, FLOAT3 blend)
{
	return v0*blend.x + v1*blend.y+ v2*blend.z;
}

inline FLOAT3 BlendValue(FLOAT v0, FLOAT v1, FLOAT v2, FLOAT v3, FLOAT4 blend)
{
	return v0*blend.x + v1*blend.y+ v2*blend.z+ v3*blend.w;
}

inline REAL3 BlendNormalRNM(REAL3 n1, REAL3 n2)
{
    REAL3 t = n1.xyz + REAL3(0.0, 0.0, 1.0);
    REAL3 u = n2.xyz * REAL3(-1.0, -1.0, 1.0);
    REAL3 r = (t / t.z) * dot(t, u) - u;
    return r;
}


//REAL3 VertexNormal;
#endif

inline REAL CalcWorldNormal(REAL3x3 TangentToWorld, REAL2 Normal,REAL normalScale,
	out REAL3 WorldNormal, out REAL3 TangentSpaceNormal)
{
#if defined(_NO_NORMAL_MAP)||defined(_DEFAULT_PBS_PARAM)
	TangentSpaceNormal = TangentToWorld[0].xyz;
	WorldNormal = TangentToWorld[2].xyz;
#else
	// REAL3 worldTangent = TangentToWorld[0].xyz;
	// // #ifdef _TICK_NORMAL
	// // 	worldTangent *= 0.5;
	// // #endif//_TICK_NORMAL
	// REAL3 worldBinormal = TangentToWorld[1].xyz;
	// REAL3 worldNormal =  TangentToWorld[2].xyz;

	//TangentSpaceNormal = FLOAT3(0,0,1);
	//VertexNormal = worldTangent * TangentSpaceNormal.x + worldBinormal * TangentSpaceNormal.y + worldNormal * TangentSpaceNormal.z;
	TangentSpaceNormal = UnpackNormal(Normal,normalScale);
	//WorldNormal = worldTangent * TangentSpaceNormal.x + worldBinormal * TangentSpaceNormal.y + worldNormal * TangentSpaceNormal.z;
	WorldNormal = normalize(mul(TangentSpaceNormal, TangentToWorld));
#endif

	// WorldNormal /= normalLength;
	// return normalLength;
	return 1;
}
#if !defined(REDEFINE_URP)

inline void CalcWorldNormal1(REAL3x3 TangentToWorld, REAL2 Normal,REAL normalScale,
	out REAL3 WorldNormal, out REAL3 TangentSpaceNormal)
{
	// REAL3 worldTangent = TangentToWorld[0].xyz;
	// REAL3 worldBinormal = TangentToWorld[1].xyz;
	// REAL3 worldNormal =  TangentToWorld[2].xyz;
	TangentSpaceNormal = UnpackNormal(Normal,normalScale);
	//WorldNormal = normalize(worldTangent * TangentSpaceNormal.x + worldBinormal * TangentSpaceNormal.y + worldNormal * TangentSpaceNormal.z);
	WorldNormal = normalize(mul(TangentSpaceNormal, TangentToWorld));
}

inline void CalcWorldNormal2(REAL3x3 TangentToWorld, REAL3 Normal,
	out REAL3 WorldNormal,out REAL3 TangentSpaceNormal)
{
	REAL3 worldTangent = TangentToWorld[0].xyz;
	REAL3 worldBinormal = TangentToWorld[1].xyz;
	REAL3 worldNormal = TangentToWorld[2].xyz;

	TangentSpaceNormal = Normal;
	//WorldNormal = normalize(worldTangent * TangentSpaceNormal.x + worldBinormal * TangentSpaceNormal.y + worldNormal * TangentSpaceNormal.z);
	WorldNormal = normalize(mul(TangentSpaceNormal, TangentToWorld));
}

// Return modified perceptualSmoothness based on provided variance (get from GeometricNormalVariance + TextureNormalVariance)
inline REAL NormalFiltering(REAL roughness, REAL variance, REAL threshold)
{
    // Ref: Geometry into Shading - http://graphics.pixar.com/library/BumpRoughness/paper.pdf - equation (3)
    REAL squaredRoughness = saturate(roughness * roughness + min(2.0 * variance, threshold * threshold)); // threshold can be really low, square the value for easier control

    return sqrt(squaredRoughness);
}

// Normal map filtering based on The Order : 1886 SIGGRAPH course notes implementation.
// Basically Toksvig with an intermediate single vMF lobe induced dispersion (Han et al. 2007)
//
// This returns 2 times the variance of the induced "mesoNDF" lobe (an NDF induced from a section of
// the normal map) from the level 0 mip normals covered by the "current texel".
//
// avgNormalLength gives the dispersion information for the covered normals.
//
// Note that hw filtering on the normal map should be trilinear to be conservative, while anisotropic
// risk underfiltering. Could also compute average normal on the fly with a proper normal map format,
// like Toksvig.
inline REAL TextureNormalVariance(REAL avgNormalLength)
{
    REAL variance = 0.0;

    if (avgNormalLength < 1.0)
    {
        REAL avgNormLen2 = avgNormalLength * avgNormalLength;
        REAL kappa = (3.0 * avgNormalLength - avgNormalLength * avgNormLen2) / (1.0 - avgNormLen2);

        // Ref: Frequency Domain Normal Map Filtering - http://www.cs.columbia.edu/cg/normalmap/normalmap.pdf (equation 21)
        // Relationship between between the standard deviation of a Gaussian distribution and the roughness parameter of a Beckmann distribution.
        // is roughness^2 = 2 variance    (note: variance is sigma^2)
        // (Ref: Filtering Distributions of Normals for Shading Antialiasing - Equation just after (14))
        // Relationship between gaussian lobe and vMF lobe is 2 * variance = 1 / (2 * kappa) = roughness^2
        // (Equation 36 of  Normal map filtering based on The Order : 1886 SIGGRAPH course notes implementation).
        // So to get variance we must use variance = 1 / (4 * kappa)
        variance = 0.25 / kappa;
    }

    return variance;
}

inline REAL TextureNormalFiltering(REAL roughness, REAL avgNormalLength, REAL threshold)
{
    REAL variance = TextureNormalVariance(avgNormalLength);
    return NormalFiltering(roughness, variance, threshold);
}

#endif 

inline REAL CalcNdotV(REAL3 WorldNormal,REAL3 CameraVector)
{
#define HANDLE_CORRECTLY_NEGATIVE_NDOTV 0

#if HANDLE_CORRECTLY_NEGATIVE_NDOTV
    FLOAT shiftAmount = dot(WorldNormal, CameraVector);
    FLOAT normal = shiftAmount < 0.0f ? WorldNormal + CameraVector * (-shiftAmount + 1e-5f) : CameraVector;
    return = saturate(dot(normal, CameraVector));
#else
	return saturate(dot(WorldNormal, CameraVector));
#endif
}
FLOAT3 Blend_RNM(FLOAT3 n0,FLOAT3 n1)
{
	FLOAT3 t = n0.xyz + FLOAT3(0.0, 0.0, 1.0);
    FLOAT3 u = n1.xyz * FLOAT3(-1.0, -1.0, 1.0);
    FLOAT3 r = (t / t.z) * dot(t, u) - u;

	// FLOAT3 t = n0*FLOAT3(2,2,2) + FLOAT3(-1,-1,0);
	// FLOAT3 u = n1*FLOAT3(-2,-2,2) + FLOAT3(1,1,-1);
	// FLOAT3 r = t*dot(t,u) - u*t.z;
	return r;
}

#endif //PBS_COMMON_INCLUDE
#ifndef UNITY_SHADER_VARIABLES_FUNCTIONS_INCLUDED
#define UNITY_SHADER_VARIABLES_FUNCTIONS_INCLUDED

float4 _WaterDisturbParam;
VertexPositionInputs GetVertexPositionInputs(float3 positionOS)
{
    VertexPositionInputs input;
    input.positionWS = TransformObjectToWorld(positionOS);
	
	#ifdef _ReflectVertex
		input.positionWS.x += sin(_Time * 29.3 * _WaterDisturbParam.w + input.positionWS.z * _WaterDisturbParam.y) * cos(_Time * 11.7 * _WaterDisturbParam.w + input.positionWS.y * _WaterDisturbParam.y) * _WaterDisturbParam.z;
		input.positionWS.z += cos(_Time * 31.7 * _WaterDisturbParam.w + input.positionWS.x * _WaterDisturbParam.y) * sin(_Time * 13.3 * _WaterDisturbParam.w + input.positionWS.y * _WaterDisturbParam.y) * _WaterDisturbParam.z;
		input.positionWS.y += cos(_Time * 7.3 * _WaterDisturbParam.w + input.positionWS.z * _WaterDisturbParam.y) * sin(_Time * 5.3 * _WaterDisturbParam.w + input.positionWS.x * _WaterDisturbParam.y) * _WaterDisturbParam.z;
		input.positionWS.y = _WaterDisturbParam.x - (input.positionWS.y - _WaterDisturbParam.x);
		
	//	input.positionWS.y = min(input.positionWS.y, _WaterDisturbParam.x);
	#endif
    input.positionVS = TransformWorldToView(input.positionWS);
    input.positionCS = TransformWorldToHClip(input.positionWS);

    float4 ndc = input.positionCS * 0.5f;
    input.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    input.positionNDC.zw = input.positionCS.zw;

    return input;
}
VertexNormalInputs GetVertexNormalInputs(float3 normalOS)
{
    VertexNormalInputs tbn;
    tbn.tangentWS = real3(1.0, 0.0, 0.0);
    tbn.bitangentWS = real3(0.0, 1.0, 0.0);
    tbn.normalWS = TransformObjectToWorldNormal(normalOS);
    return tbn;
}
VertexNormalInputs GetVertexNormalInputs(float3 normalOS, float4 tangentOS)
{
    VertexNormalInputs tbn;
    // mikkts space compliant. only normalize when extracting normal at frag.
    real sign = tangentOS.w * GetOddNegativeScale();
    tbn.normalWS = TransformObjectToWorldNormal(normalOS);
    tbn.tangentWS = TransformObjectToWorldDir(tangentOS.xyz);
    tbn.bitangentWS = cross(tbn.normalWS, tbn.tangentWS) * sign;
    return tbn;
}

float4 GetScaledScreenParams()
{
    return _ScaledScreenParams;
}

// Returns 'true' if the current view performs a perspective projection.
bool IsPerspectiveProjection()
{
    return (unity_OrthoParams.w == 0);
}

float3 GetCameraPositionWS()
{
    // Currently we do not support Camera Relative Rendering so
    // we simply return the _WorldSpaceCameraPos until then
    return _WorldSpaceCameraPos;

    // We will replace the code above with this one once
    // we start supporting Camera Relative Rendering
    //#if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
    //    return float3(0, 0, 0);
    //#else
    //    return _WorldSpaceCameraPos;
    //#endif
}

// Could be e.g. the position of a primary camera or a shadow-casting light.
float3 GetCurrentViewPosition()
{
    // Currently we do not support Camera Relative Rendering so
    // we simply return the _WorldSpaceCameraPos until then
    return GetCameraPositionWS();

    // We will replace the code above with this one once
    // we start supporting Camera Relative Rendering
    //#if defined(SHADERPASS) && (SHADERPASS != SHADERPASS_SHADOWS)
    //    return GetCameraPositionWS();
    //#else
    //    // This is a generic solution.
    //    // However, for the primary camera, using '_WorldSpaceCameraPos' is better for cache locality,
    //    // and in case we enable camera-relative rendering, we can statically set the position is 0.
    //    return UNITY_MATRIX_I_V._14_24_34;
    //#endif
}

// Returns the forward (central) direction of the current view in the world space.
float3 GetViewForwardDir()
{
    float4x4 viewMat = GetWorldToViewMatrix();
    return -viewMat[2].xyz;
}

// Computes the world space view direction (pointing towards the viewer).
float3 GetWorldSpaceViewDir(float3 positionWS)
{
    if (IsPerspectiveProjection())
    {
        // Perspective
        return GetCurrentViewPosition() - positionWS;
    }
    else
    {
        // Orthographic
        return -GetViewForwardDir();
    }
}

float3 GetWorldSpaceNormalizeViewDir(float3 positionWS)
{
    if (IsPerspectiveProjection())
    {
        // Perspective
        float3 V = GetCurrentViewPosition() - positionWS;
        return normalize(V);
    }
    else
    {
        // Orthographic
        return -GetViewForwardDir();
    }
}

// UNITY_MATRIX_V defines a right-handed view space with the Z axis pointing towards the viewer.
// This function reverses the direction of the Z axis (so that it points forward),
// making the view space coordinate system left-handed.
void GetLeftHandedViewSpaceMatrices(out float4x4 viewMatrix, out float4x4 projMatrix)
{
    viewMatrix = UNITY_MATRIX_V;
    viewMatrix._31_32_33_34 = -viewMatrix._31_32_33_34;

    projMatrix = UNITY_MATRIX_P;
    projMatrix._13_23_33_43 = -projMatrix._13_23_33_43;
}

void AlphaDiscard(real alpha, real cutoff, real offset = 0.0h)
{
    #ifdef _ALPHATEST_ON
        clip(alpha - cutoff + offset);
    #endif
}

half OutputAlpha(half outputAlpha, half surfaceType = 0.0)
{
    return surfaceType == 1 ? outputAlpha : 1.0;
}

// A word on normalization of normals:
// For better quality normals should be normalized before and after
// interpolation.
// 1) In vertex, skinning or blend shapes might vary significantly the lenght of normal.
// 2) In fragment, because even outputting unit-length normals interpolation can make it non-unit.
// 3) In fragment when using normal map, because mikktspace sets up non orthonormal basis.
// However we will try to balance performance vs quality here as also let users configure that as
// shader quality tiers.
// Low Quality Tier: Normalize either per-vertex or per-pixel depending if normalmap is sampled.
// Medium Quality Tier: Always normalize per-vertex. Normalize per-pixel only if using normal map
// High Quality Tier: Normalize in both vertex and pixel shaders.
real3 NormalizeNormalPerVertex(real3 normalWS)
{
    #if defined(SHADER_QUALITY_LOW) && defined(_NORMALMAP)
        return normalWS;
    #else
        return normalize(normalWS);
    #endif
}

real3 NormalizeNormalPerPixel(real3 normalWS)
{
    #if defined(SHADER_QUALITY_HIGH) || defined(_NORMALMAP)
        return normalize(normalWS);
    #else
        return normalWS;
    #endif
}

// TODO: A similar function should be already available in SRP lib on master. Use that instead
float4 ComputeScreenPos(float4 positionCS)
{
    float4 o = positionCS * 0.5f;
    o.xy = float2(o.x, o.y * _ProjectionParams.x) + o.w;
    o.zw = positionCS.zw;
    return o;
}

real ComputeFogFactor(float z)
{
    float clipZ_01 = UNITY_Z_0_FAR_FROM_CLIPSPACE(z);

    #if defined(FOG_LINEAR)
        // factor = (end-z)/(end-start) = z * (-1/(end-start)) + (end/(end-start))
        float fogFactor = saturate(clipZ_01 * unity_FogParams.z + unity_FogParams.w);
        return real(fogFactor);
    #elif defined(FOG_EXP) || defined(FOG_EXP2)
        // factor = exp(-(density*z)^2)
        // -density * z computed at vertex
        return real(unity_FogParams.x * clipZ_01);
    #else
        return 0.0h;
    #endif
}

real ComputeFogIntensity(real fogFactor)
{
    real fogIntensity = 0.0h;
    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
        #if defined(FOG_EXP)
            // factor = exp(-density*z)
            // fogFactor = density*z compute at vertex
            fogIntensity = saturate(exp2(-fogFactor));
        #elif defined(FOG_EXP2)
            // factor = exp(-(density*z)^2)
            // fogFactor = density*z compute at vertex
            fogIntensity = saturate(exp2(-fogFactor * fogFactor));
        #elif defined(FOG_LINEAR)
            fogIntensity = fogFactor;
        #endif
    #endif
    return fogIntensity;
}

half3 MixFogColor(real3 fragColor, real3 fogColor, real fogFactor)
{
    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
        real fogIntensity = ComputeFogIntensity(fogFactor);
        fragColor = lerp(fogColor, fragColor, fogIntensity);
    #endif
    return fragColor;
}

half3 MixFog(real3 fragColor, real fogFactor)
{
    return MixFogColor(fragColor, unity_FogColor.rgb, fogFactor);
}

void TransformScreenUV(inout float2 uv, float screenHeight)
{
    #if UNITY_UV_STARTS_AT_TOP
    uv.y = screenHeight - (uv.y * _ScaleBiasRt.x + _ScaleBiasRt.y * screenHeight);
    #endif
}

void TransformScreenUV(inout float2 uv)
{
    #if UNITY_UV_STARTS_AT_TOP
    TransformScreenUV(uv, GetScaledScreenParams().y);
    #endif
}

void TransformNormalizedScreenUV(inout float2 uv)
{
    #if UNITY_UV_STARTS_AT_TOP
    TransformScreenUV(uv, 1.0);
    #endif
}

float2 GetNormalizedScreenSpaceUV(float2 positionCS)
{
    float2 normalizedScreenSpaceUV = positionCS.xy * rcp(GetScaledScreenParams().xy);
    TransformNormalizedScreenUV(normalizedScreenSpaceUV);
    return normalizedScreenSpaceUV;
}

float2 GetNormalizedScreenSpaceUV(float4 positionCS)
{
    return GetNormalizedScreenSpaceUV(positionCS.xy);
}

#if defined(UNITY_SINGLE_PASS_STEREO)
    float2 TransformStereoScreenSpaceTex(float2 uv, float w)
    {
        // TODO: RVS support can be added here, if Universal decides to support it
        float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
        return uv.xy * scaleOffset.xy + scaleOffset.zw * w;
    }

    float2 UnityStereoTransformScreenSpaceTex(float2 uv)
    {
        return TransformStereoScreenSpaceTex(saturate(uv), 1.0);
    }
#else
    #define UnityStereoTransformScreenSpaceTex(uv) uv
#endif // defined(UNITY_SINGLE_PASS_STEREO)

float4 _CameraBlockParams0;
float4 _CameraBlockParams1;
#define _CameraBlockInvFocusZ _CameraBlockParams1.x
#define _CameraBlockRangeX _CameraBlockParams1.y
#define _CameraBlockRangeY _CameraBlockParams1.z

void DitherTransparent(float2 positionCS, float transparency)
{
    #if defined(_DITHER_TRANSPARENCY)// && !defined(_ALPHA_TEST)
    float4x4 thresholdMatrix =
    {
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };
    float2 index = fmod(positionCS.xy, 4);
    clip(transparency - thresholdMatrix[index.x][index.y]);
    #endif
}

void SphereDitherTransparent(float4 positionCS, float alpha)
{
    #if defined(_DITHER_TRANSPARENCY)// && !defined(_ALPHA_TEST)

    // TODO: Move codes to CSharp.
    
    // CSharp Settings
    float roleDistance = 5.0;
    float2 rolePositionVS = float2(0.5, 0.5);
    float2 blockRange = float2(0.3, 1.0);

    // CSharp pack params
    float4 _CameraBlockParams0;
    // float4 _CameraBlockParams1;
    // _CameraBlockParams1 = float4(1.0 / roleDistance, blockRange.x, blockRange.y, 0);
    float2 xzScale = float2(1.5, 1.5) * float2(1.0, _ScreenParams.y / _ScreenParams.x);
    _CameraBlockParams0.xy = 1.0 / _ScreenParams.xy * xzScale;
    _CameraBlockParams0.zw = -rolePositionVS * xzScale;

    // Shader redefine
    #define _CameraBlockInvFocusZ _CameraBlockParams1.x
    #define _CameraBlockRangeX _CameraBlockParams1.y
    #define _CameraBlockRangeY _CameraBlockParams1.z

    float3 sphere;
    sphere.xy = mad(positionCS.xy, _CameraBlockParams0.xy, _CameraBlockParams0.zw);
    sphere.z = dot(unity_MatrixInvP[2], positionCS) * _CameraBlockInvFocusZ;
    float len = dot(sphere, sphere);
    float sphereFade = saturate((len - _CameraBlockRangeX) / (_CameraBlockRangeY - _CameraBlockRangeX));
    alpha = lerp(sphereFade, 1, alpha);
    DitherTransparent(positionCS.xy, alpha);
    
    #endif
}

#endif // UNITY_SHADER_VARIABLES_FUNCTIONS_INCLUDED

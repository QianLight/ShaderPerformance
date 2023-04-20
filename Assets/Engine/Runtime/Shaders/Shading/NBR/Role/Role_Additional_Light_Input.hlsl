#ifndef ROLE_ADDITIONAL_LIGHT_INPUT_INCLUDE
#define ROLE_ADDITIONAL_LIGHT_INPUT_INCLUDE

#ifndef UNIVERSAL_INPUT_INCLUDED
half4 _AdditionalLightsCount;
#define MAX_VISIBLE_LIGHTS 256
#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
StructuredBuffer<LightData> _AdditionalLightsBuffer;
StructuredBuffer<int> _AdditionalLightsIndices;
#else
// GLES3 causes a performance regression in some devices when using CBUFFER.
#ifndef SHADER_API_GLES3
CBUFFER_START(AdditionalLights)
#endif
float4 _AdditionalLightsPosition[MAX_VISIBLE_LIGHTS];
half4 _AdditionalLightsColor[MAX_VISIBLE_LIGHTS];
half4 _AdditionalLightsAttenuation[MAX_VISIBLE_LIGHTS];
half4 _AdditionalLightsSpotDir[MAX_VISIBLE_LIGHTS];
half4 _AdditionalLightsOcclusionProbes[MAX_VISIBLE_LIGHTS];
#ifndef SHADER_API_GLES3
CBUFFER_END
#endif
#endif
#endif

#ifndef UNIVERSAL_LIGHTING_INCLUDED
struct Light
{
    half3   direction;
    half3   color;
    half    distanceAttenuation;
    half    shadowAttenuation;
    half    type;//0 direction 1 other
};
#endif

#endif
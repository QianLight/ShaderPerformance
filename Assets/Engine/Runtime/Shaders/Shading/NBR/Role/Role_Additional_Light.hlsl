#include "Assets/Engine/Runtime/Shaders/Shading/NBR/Include/Head.hlsl"

#ifndef ROLE_ADDITIONAL_LIGHT_INCLUDE
#define ROLE_ADDITIONAL_LIGHT_INCLUDE

float RoleDistanceAttenuation(float distanceSqr, half2 distanceAttenuation)
{
    float lightAtten = rcp(distanceSqr);
    half factor = distanceSqr * distanceAttenuation.x;
    half smoothFactor = saturate(1.0h - factor * factor);
    return lightAtten * smoothFactor;
}

Light GetRoleAdditionalLight(uint count, FInterpolantsVSToPS interpolant)
{
    Light light = (Light)0;
    half4 dir = _AdditionalLightsPosition[count] - interpolant.WorldPosition;
    float distanceSqr = max(dot(dir, dir), 6.103515625e-5);
    light.direction =  normalize(dir).xyz;
    light.distanceAttenuation = RoleDistanceAttenuation(distanceSqr,_AdditionalLightsAttenuation[count].xy);
    light.color = _AdditionalLightsColor[count].xyz;
    return light;
}

half3 RoleAdditionalLighting(FInterpolantsVSToPS interpolant, half3 baseColor)
{
    uint count = min(_AdditionalLightsCount.x,4u);
    half3 col = 0;
    for (uint lightIndex = 0u; lightIndex < count; lightIndex++)
    {
        Light light = GetRoleAdditionalLight(lightIndex,interpolant);
        half NdotL = saturate(dot(light.direction,interpolant.NormalWS.xyz));
        NdotL *= NdotL;
        half3 lighting = light.color * light.distanceAttenuation * NdotL;
        lighting *= baseColor;
        col += lighting;
    }
    return col;
}

#endif
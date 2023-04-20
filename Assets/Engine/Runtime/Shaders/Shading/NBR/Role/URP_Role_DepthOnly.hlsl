#ifndef URP_ROLE_DEPTHONLY_INCLUDED
#define URP_ROLE_DEPTHONLY_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Assets/Engine/Runtime/Shaders/Shading/Scene/ParkourDistort.hlsl"
struct Attributes
{
    float4 position     : POSITION;
};

struct Varyings
{
    float4 positionCS   : SV_POSITION;
};

FLOAT _DitherTransparency;

Varyings DepthOnlyVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
   
    UNITY_BRANCH
    if(_IsParkour == 1)
    {
        float3 posWS = TransformObjectToWorld(input.position.xyz);
        posWS = ParkourDistortVertex(posWS);
		output.positionCS = TransformWorldToHClip(posWS);
    }
	else{
		output.positionCS = TransformObjectToHClip(input.position.xyz);
	}
    
    return output;
}
half4 DepthOnlyFragment(Varyings input) : SV_TARGET
{
    DitherTransparent(input.positionCS.xy, _DitherTransparency);
    return 0;
}

#endif
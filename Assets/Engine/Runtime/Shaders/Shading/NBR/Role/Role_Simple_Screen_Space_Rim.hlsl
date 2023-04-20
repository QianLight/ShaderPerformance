#ifndef ROLE_SIMPLE_SCREEN_SPACE_RIM_INCLUDE
#define ROLE_SIMPLE_SCREEN_SPACE_RIM_INCLUDE
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Assets/Engine/Runtime/Shaders/Shading/Include/SmartShadow.hlsl"

#ifndef SCREEN_SPACE_RIM_IS_IN_COMMON_CBUFFER
CBUFFER_START(UnityPerMaterial)
float4 _SSRimColor;
float4 _SSRimParam0; 
CBUFFER_END
#endif

TEXTURE2D(_CameraDepthTexture);     SAMPLER(sampler_CameraDepthTexture);

#define SCREEN_WIDTH _ScreenParams.x
#define SCREEN_HEIGHT _ScreenParams.y
#define CAMERA_NEAR_PLANE _ProjectionParams.y
#define HEIGHT_TO_WIDTH_RATIO (SCREEN_WIDTH/SCREEN_HEIGHT)
#define FHD_HEIGHT 1080
#define FHD_WIDTH (FHD_HEIGHT*HEIGHT_TO_WIDTH_RATIO)
#define RIM_COLOR _SSRimColor.xyz
#define RIM_INTENSITY _SSRimColor.w
#define DISTANCE_THRESHOLD_BASE _SSRimParam0.x
#define DISTANCE_THRESHOLD_MAX (DISTANCE_THRESHOLD_BASE+0.05)
#define RIM_SCALE _SSRimParam0.w
#define RIM_SCALE_MAX 6
#define RIM_SCALE_MIN clamp((sqrt(60-_CameraFov)-2.3),1,4)
#define RIM_DISAPPEARANCE_START_DISTANCE 8.0
#define RIM_DISAPPEARANCE_END_DISTANCE 20.0

struct Attributes
{
    float2  uv           : TEXCOORD0;
    float4  positionOS   : POSITION;
    float3  normal       : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2  screenUV     : TEXCOORD0;
    float   fogCoord     : TEXCOORD1;
    float3  positionVS   : TEXCOORD2;
    float4  positionWS   : TEXCOORD3;
    float2  uv           : TEXCOORD4;
    float3  normalWS     : TEXCOORD5;
    float4  positionCS   : SV_POSITION;
    float   halfNdotL    : COLOR0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

float halfNdotL(float3 noramlWS)
{
    Light light = GetMainLight();
    return dot(light.direction, noramlWS) *0.5 + 0.5;    
}

Varyings ScreenRimVert(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    output.positionCS = vertexInput.positionCS;
    float4 screenPos = vertexInput.positionNDC;
    output.positionWS = float4(vertexInput.positionWS,1);
    output.screenUV = screenPos.xy / screenPos.w;
    output.uv = input.uv;
    output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normal);
    output.normalWS = normalInput.normalWS;
    output.halfNdotL = halfNdotL(output.normalWS);
    return output;
}


//#define InverseLerp(a,b,t)  (((t)-(a))/((b)-(a)))
//#define LinearStep(a,b,t)   saturate(InverseLerp(a,b,t))

#define SAMPLE_DEPTH_RIM(uv)  SampleDepthRim(_CameraDepthTexture, sampler_CameraDepthTexture, uv, positionCS)
half SampleDepthRim(Texture2D _depthMap, SamplerState sampler_depthMap, half2 uv, float4 positionCS)
{
    half depthMap = SAMPLE_TEXTURE2D(_depthMap, sampler_depthMap, uv).x;
    half comparedDepth = LinearEyeDepth(depthMap,_ZBufferParams);
    half selfDepth = positionCS.w; // LinearDepth == positionCS.w == -positionVS.w
    half rim = LinearStep(selfDepth + DISTANCE_THRESHOLD_BASE, selfDepth + DISTANCE_THRESHOLD_MAX, comparedDepth);
    return rim;
}

half4 ScreenRimFrag(Varyings input) : SV_Target
{                
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    
    float4 positionCS = input.positionCS;
    half2 screenUV = input.screenUV;

    half rimScale = clamp(RIM_SCALE / positionCS.w, RIM_SCALE_MIN, RIM_SCALE_MAX);
    
    half4 uv02 = screenUV.xyxy;
    half4 uv13 = screenUV.xyxy;
    
    #define UV0 uv02.xy
    #define UV1 uv13.xy
    #define UV2 uv02.zw
    #define UV3 uv13.zw

    half2 uvOffset = rimScale * rcp(half2(FHD_WIDTH, FHD_HEIGHT));
    uv02.xw -= uvOffset;
    uv13.xw += uvOffset;

    half rim0 = SAMPLE_DEPTH_RIM(UV0);
    half rim1 = SAMPLE_DEPTH_RIM(UV1);
    half rim2 = SAMPLE_DEPTH_RIM(UV2);
    half rim3 = SAMPLE_DEPTH_RIM(UV3);

    float halfNdotL = 1- input.halfNdotL;
    float3 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).xyz;
    baseColor = baseColor * 0.6 + 0.4;
    baseColor *= halfNdotL;
    float shadow = GetSmartShadow9Point(input.positionWS,0.5,0.6);
    half3 color = max(max(rim0,rim1), max(rim2,rim3)) * RIM_COLOR * RIM_INTENSITY * shadow * baseColor; 

    half RimDisappearance = 1-LinearStep(RIM_DISAPPEARANCE_START_DISTANCE,RIM_DISAPPEARANCE_END_DISTANCE,positionCS.w);

    #ifdef _ALPHAPREMULTIPLY_ON
    color *= alpha;
    #endif

    color = MixFog(color, input.fogCoord);
    
    return half4(color * RimDisappearance, 0);
}

#endif
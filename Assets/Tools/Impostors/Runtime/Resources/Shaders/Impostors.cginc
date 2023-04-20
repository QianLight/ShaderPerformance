//#include "UnityCG.cginc"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#define PRECOMPUTE_SCATTER
#define FOG_NOISE_OFF

//half4 _MainLightDir0;
//half4 _MainLightColor0;

//#include "../StdLib.hlsl"
//#include "../../Include/PrecomputedScatter.hlsl"
//#include "../Include/Fog.hlsl"
//#include "../../../../../Engine/Runtime/Shaders/Shading/NBR/Include/Fog.hlsl"


uniform float4 _ImpostorsTimeProvider;
uniform float _ImpostorsNoiseTextureResolution;
uniform sampler2D _ImpostorsNoiseTexture;
uniform float _ImpostorsCutout;
uniform float _ImpostorsMinAngleToStopLookAt;
uniform float4 _ImpostorsDebugColor;
uniform float3 _ImpostorsWorldSpaceCameraPosition;
uniform float4 _SceneColor;

CBUFFER_START(UnityPerMaterial)
sampler2D _MainTex;
float _AlphaToOne;
float _ImpostorAlpha;
CBUFFER_END



inline float invLerp(float from, float to, float value)
{
    return (value - from) / (to - from);
}

inline float angleBetween(float3 colour, float3 original)
{
    return acos(dot(colour, original) / (length(colour) * length(original)));
}

/*inline float4 ComputeDitherScreenPos(float4 clipPos)
{

    float4 screenPos = ComputeScreenPos(clipPos).xyww;
    screenPos.xy *= _ScreenParams.xy * 0.0025;
    return screenPos;
}

inline void DitherCrossFade(float3 ditherScreenPos, float ditherProgress, float ditherSide)
{
    float2 projUV = ditherScreenPos.xy / ditherScreenPos.z * 100;
    projUV.xy = frac(projUV.xy + 0.001) + frac(projUV.xy * 2.0 + 0.001);
    float dither = ditherProgress - (projUV.y + projUV.x) * 0.25;
    clip(lerp(dither, -dither, ditherSide));
}*/

// side = 0 if fading in
// side = 1 if fading out 
inline void TextureCrossFade(float4 screenPos, float progress, float side)
{
    float2 noiseUV = screenPos.xy / screenPos.w;
    noiseUV.x *= _ScreenParams.x / _ImpostorsNoiseTextureResolution;
    noiseUV.y *= _ScreenParams.y / _ImpostorsNoiseTextureResolution;
    float noiseValue = tex2D(_ImpostorsNoiseTexture, noiseUV).a;

    if (abs(side - noiseValue) > abs(side - progress))
    {
        discard;
    }
}

struct appdata
{
    // (x,y) - quad corner position, (z) - z offset. 
    float4 vertex : POSITION;
    // (x,y,z) - impostor's direction
    float3 normal : NORMAL;
    // (x,y) - uv, (w) - fade duration, (z) - not implemented, controls "always look at camera" 
    float4 texcoord : TEXCOORD0;
    // (x,y,z) - center of impostor, (w) - time when fade should end (if (w>0) then {fading in} else {fading out})
    float4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    
};

struct v2f
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
    // (x) - progress 0..1, (y) - side if (side == 0) {fading in} else {fading out}
    float2 fade : COLOR;
    // required for cross fade
    float4 screenPos : TEXCOORD1;
    //UNITY_FOG_COORDS(2)
    
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

v2f impostors_vert(appdata v)
{
    v2f o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    
    float scale = 100000;
    float3 world = v.color.xyz;
    float3 worldPos = float3(
        world.x * scale - scale / 2,
        world.y * scale - scale / 2,
        world.z * scale - scale / 2
    );

    float3 upVector = float3(0, 100, 0);
    float3 eyeVector = _ImpostorsWorldSpaceCameraPosition - worldPos;
    float deg = abs(degrees(angleBetween(eyeVector, upVector)));
    if (deg < 1.25 * _ImpostorsMinAngleToStopLookAt)
    {
        float val = invLerp(_ImpostorsMinAngleToStopLookAt * 1.25, _ImpostorsMinAngleToStopLookAt, deg);
        eyeVector = lerp(normalize(v.normal), normalize(eyeVector), 1 - clamp(val, 0, 1));
    }

    float3 right = cross(eyeVector, upVector);
    float3 up = cross(eyeVector, right);

    up = normalize(up);
    right = normalize(right);
    eyeVector = normalize(eyeVector);

    float3 finalPosition = worldPos;
    finalPosition += v.vertex.x * right;
    finalPosition -= v.vertex.y * up;
    finalPosition += v.vertex.z * eyeVector;

    float4 pos = float4(finalPosition, 1);
    float fadeProgress = 1;
    float side = -2;
    float targetTime = v.color.a * 100000.0;
    float timeDelta = abs(targetTime) - _ImpostorsTimeProvider.x;

    if (timeDelta > 0 && v.texcoord.w > 0.001)
    {
        fadeProgress = 1 - timeDelta / v.texcoord.w;
        fadeProgress = clamp(fadeProgress, 0, 1);
        // if fading in
        if (v.color.a > 0)
        {
            side = 0;
            // add little position to get rid of z-fighting
            pos += float4(eyeVector, 0) * lerp(0.2, 0, fadeProgress);
        }
        // else, if fading out
        if (v.color.a < 0)
        {
            side = 1;
            // add little position to get rid of z-fighting
            pos -= float4(eyeVector, 0) * lerp(0, 0.2, fadeProgress);
        }
    }
    else
    {
        // if faded out
        if (v.color.a < 0)
        {
            side = 1;
            fadeProgress = 1;
        }
    }

    //o.pos = UnityObjectToClipPos(pos);
    o.pos = TransformObjectToHClip (pos.xyz);
    
    o.screenPos = ComputeScreenPos(o.pos);
    o.fade = float2(fadeProgress, side);
    o.uv = float2(v.texcoord.x, v.texcoord.y);
    //UNITY_TRANSFER_FOG(o, o.pos);
    return o;
}

float4 impostors_frag(v2f i) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID( i );
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );
    
    float4 color = tex2D(_MainTex, i.uv.xy);
    float blurAlpha = 0;
    float2 pixel = float2(1/ _ScreenParams.x, 1/ _ScreenParams.y);
    blurAlpha = tex2D(_MainTex, i.uv.xy + half2(pixel.x, 0)).a;
    blurAlpha += tex2D(_MainTex, i.uv.xy + half2(-pixel.x, 0)).a;
    blurAlpha += tex2D(_MainTex, i.uv.xy + half2(0, pixel.y)).a;
    blurAlpha += tex2D(_MainTex, i.uv.xy + half2(0, -pixel.y)).a;
    float oldalpha = color.a;
    float newalpha = (color.a + blurAlpha * 0.25) * 0.5;
    clip(oldalpha - _ImpostorsCutout);
    color.a = saturate(newalpha + _ImpostorsCutout) * ceil(oldalpha);
    // if(color.a!=_ImpostorAlpha)
    // {
    //     clip(color.a - _ImpostorsCutout);
    // }
    // else
    // {
    //     color.a=step(_AlphaToOne,color.a);
    // }
  
  

    if (i.fade.y > -1)
        TextureCrossFade(i.screenPos, i.fade.x, i.fade.y);

    color.rgb *= lerp(_SceneColor.rgb, 1, _SceneColor.a);
	//APPLY_FOG(color, i.pos.xyz);
    //UNITY_APPLY_FOG(i.fogCoord, color);
    return color + _ImpostorsDebugColor;
}

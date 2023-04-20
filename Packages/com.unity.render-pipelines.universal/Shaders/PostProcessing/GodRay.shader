Shader "Hidden/Universal Render Pipeline/GodRay" 
{    
    HLSLINCLUDE
    //#pragma exclude_renderers gles

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

    TEXTURE2D_X(_SourceTex);
    
    float4 _SourceTex_TexelSize;
    float4 _CameraDepthAttachment_TexelSize;
    float4 _GodRayViewPortLightPos;
	
    float4 _GodRayOffset;
    float4 _GodRayParam;
    float4 _GodRayParam1;
    float4 _GodRayColor;

    #define RADIAL_SAMPLE_COUNT 4
    #define _GodRayThreshold _GodRayParam.x
    #define _GodRayLinearDistance _GodRayParam.y
    #define _GodRayLightPower _GodRayParam.z
    #define _GodRayLightRadius _GodRayParam.w
    #define _GodRayLightMaxPower _GodRayParam1.x

    struct v2f_blur
    {
        float4 pos : POSITION;
        float2 uv : TEXCOORD0;
        float2 blurOffset : TEXCOORD1;
    };

    inline float DecodeAlpha(float a, float mask)
    {
        return  lerp(a * 5, 1 - a, mask);
    }
    inline float Linear01Depth(float z)
    {
        return 1.0 / (_ZBufferParams.x * z + _ZBufferParams.y);
    }
    float4 frag_threshold(Varyings i) : SV_Target
    {
        float2 uv = UnityStereoTransformScreenSpaceTex(i.uv);
        //float depth = depth_resolve_linear(SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_LinearClamp, uv).r);
        float depth = Linear01Depth(SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_LinearClamp, uv).r);
        //luminanceColor *= step(_GodRayLinearDistance, depth);
        UNITY_BRANCH
        if (depth < _GodRayLinearDistance)
            return 0;
        //else
            //return 1;

        half4 color = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv);
        float distFromLight = length((_GodRayViewPortLightPos.xy - uv) * float2(_SourceTex_TexelSize.y / _SourceTex_TexelSize.x, 1));
        float distanceControl = saturate(_GodRayLightRadius - distFromLight);

        float4 thresholdColor = saturate(color - _GodRayThreshold) * distanceControl;
        float luminanceColor = Luminance(thresholdColor.rgb);
        //luminanceColor *= step(depth, 700);
        luminanceColor = pow(luminanceColor, _GodRayLightPower);
        luminanceColor = min(luminanceColor, _GodRayLightMaxPower);
        return float4(_GodRayColor.rgb * luminanceColor, 1);
    }

    v2f_blur vert_blur(Attributes i)
    { 
        v2f_blur o;
        o.pos = TransformObjectToHClip(i.positionOS.xyz);
        o.uv = UnityStereoTransformScreenSpaceTex(i.uv);
        o.blurOffset = _GodRayOffset * (_GodRayViewPortLightPos.xy - o.uv);
        return o;
    }
#if _GODRAY_USE_NOISE
    float2 random2(float2 p) 
    {
        return frac(sin(float2(
            dot(p, float3(114.5, 141.9, 198.10)),
            dot(p, float3(364.3, 648.8, 946.4))
            )) * 643.1);
    }
#endif
    float4 frag_blur(v2f_blur i) : SV_Target
    {  
        half4 color = half4(0,0,0,0);
#if _GODRAY_USE_NOISE
        float jitter = random2(i.uv);
#endif
        [unroll(RADIAL_SAMPLE_COUNT)]
        for (int j = 0; j < RADIAL_SAMPLE_COUNT; j++)
        {
            color += SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, i.uv.xy); 
#if _GODRAY_USE_NOISE
            i.uv.xy += i.blurOffset * jitter;
#else
            i.uv.xy += i.blurOffset;
#endif
        }
        return color / RADIAL_SAMPLE_COUNT;
    }
    ENDHLSL

    SubShader  
    {  
        Cull Off ZWrite Off ZTest Off  

        Pass  
        {   
            Name "Filter"
            HLSLPROGRAM  
            #pragma shader_feature_local_fragment _ _GODRAY_USE_NOISE
            #pragma vertex FullscreenVert  
            #pragma fragment frag_threshold  
            ENDHLSL  
        }

        Pass  
        { 
            Name "Blur"
            HLSLPROGRAM  
            #pragma shader_feature_local_fragment _ _GODRAY_USE_NOISE
            #pragma vertex vert_blur  
            #pragma fragment frag_blur  
            ENDHLSL  
        }      
    }  
}
Shader "Hidden/PostProcessing/GodRay" 
{    
    HLSLINCLUDE

    #include "../StdLib.hlsl"
    #include "../Colors.hlsl"

    TEX2D_SAMPLER(_MainTex);
    TEX2D_SAMPLER(_CameraDepthRT);

    FLOAT4 _MainTex_TexelSize;
    FLOAT4 _ViewPortLightPos;
    
    FLOAT4 _Offset;
    FLOAT4 _GodrayParam;
    FLOAT4 _GodRayColor;
    #define _LightPower _GodrayParam.z
    #define _LightRadius _GodrayParam.w

    FLOAT4 frag_threshold(VaryingsDefault i) : SV_Target  
    {
        FLOAT distFromLight = length((_ViewPortLightPos.xy - i.texcoord)*FLOAT2(_MainTex_TexelSize.y/_MainTex_TexelSize.x,1));
        FLOAT distanceControl = step(0, _ViewPortLightPos.z) * saturate(_LightRadius - distFromLight);
        FLOAT mask = saturate(1-DecodeAlpha(SAMPLE_TEX2D(_CameraDepthRT, i.texcoord).w,0));
        mask *= distanceControl;
        mask *= saturate((0.5 - abs(i.texcoord - 0.5)));
        mask = pow(abs(mask), _LightPower);
        return FLOAT4(mask * _GodRayColor.rgb, 1);
    }

    struct v2f_blur
    {  
        FLOAT4 pos : SV_POSITION;  
        FLOAT4 uv01 : TEXCOORD0;
        FLOAT4 uv23 : TEXCOORD1;
    };
    
    v2f_blur vert_blur(AttributesDefault v)  
    { 
        VaryingsDefault vo = VertDefault(v);
        v2f_blur o;
        o.pos = vo.vertex;
        float2 delta = _ViewPortLightPos.xy - vo.texcoord;
        float2 blurOffset = _Offset.xy * delta;
        o.uv01 = mad(blurOffset.xyxy, FLOAT4(0,0,1,1), vo.texcoord.xyxy);
        o.uv23 = mad(blurOffset.xyxy, FLOAT4(2,2,3,3), vo.texcoord.xyxy);
        return o;
    }

    FLOAT4 frag_blur(v2f_blur i) : SV_Target
    {  
        FLOAT4 color;
        color  = SAMPLE_TEX2D(_MainTex, i.uv01.xy);
        color += SAMPLE_TEX2D(_MainTex, i.uv01.zw);
        color += SAMPLE_TEX2D(_MainTex, i.uv23.xy);
        color += SAMPLE_TEX2D(_MainTex, i.uv23.zw);
        return color * 0.25;
    }

    ENDHLSL

    SubShader  
    {  
        Cull Off ZWrite Off ZTest Off  

        Pass  
        {   
            Name "Filter"
            HLSLPROGRAM  
            #pragma vertex VertDefault  
            #pragma fragment frag_threshold  
            ENDHLSL  
        }

        Pass  
        { 
            Name "Blur"
            HLSLPROGRAM  
            #pragma vertex vert_blur  
            #pragma fragment frag_blur  
            ENDHLSL  
        }      
    }  
}
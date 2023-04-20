// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Tree_Shaders/Transparent/Cutout/VertexLitVMTest" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _SpecColor ("Spec Color", Color) = (1,1,1,0)
    _Emission ("Emissive Color", Color) = (0,0,0,0)
    _Shininess ("Shininess", Range (0.1, 1)) = 0.7
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5

    [Toggle(_GLOBAL_WIND_EFFECT)]_WindEffectOn("Wind Effect On",Float) = 1
}

SubShader {
    Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
    LOD 100


    CGINCLUDE

        #define PI  3.1415926536
        #define PI2 6.283185307
        
        #define TEST1 0

#if TEST1
        float CosLike(float x) { return cos(PI2*x);}
        float2 CosLike(float2 x) { return cos(PI2*x);}
        float3 CosLike(float3 x) { return cos(PI2*x);}
        float4 CosLike(float4 x) { return cos(PI2*x);}
#else
        float CosLike(float x)
        {
            float z = abs(frac(x)-0.5)*2;
            z = 1 - z * z; 
            z = 1 - z * z; 
            z = (z - 0.5) * 2; 
            return z;
        }

        float2 CosLike(float2 x)
        {
            float2 z = abs(frac(x)-0.5)*2;
            z = 1 - z * z; 
            z = 1 - z * z; 
            z = (z - 0.5) * 2; 
            return z;
        }

        float3 CosLike(float3 x)
        {
            float3 z = abs(frac(x)-0.5)*2;
            z = 1 - z * z; 
            z = 1 - z * z; 
            z = (z - 0.5) * 2; 
            return z;
        }

        float4 CosLike(float4 x)
        {
            float4 z = abs(frac(x)-0.5)*2;
            z = 1 - z * z; 
            z = 1 - z * z; 
            z = (z - 0.5) * 2; 
            return z;
        }
#endif

    ENDCG

    // no lightmap
    Pass
    {
        Tags{ "LIGHTMODE" = "Vertex" "QUEUE" = "AlphaTest" "IGNOREPROJECTOR" = "true" "RenderType" = "TransparentCutout" }
        AlphaToMask On
        ColorMask RGB


        CGPROGRAM

        #pragma vertex vert
        #pragma fragment frag
        #pragma target 3.0
        #include "UnityCG.cginc"
        #pragma multi_compile_fog
        #pragma multi_compile_instancing
        #pragma multi_compile _ _GLOBAL_WIND_EFFECT

        #define USING_FOG (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))
        
        float4 unity_Lightmap_ST;
        float4 _MainTex_ST;

        struct appdata
        {
            float3 pos : POSITION;
        #ifdef _GLOBAL_WIND_EFFECT    
            float4 tangent : TANGENT;
        #endif    
            half4 color : COLOR;
            float3 uv1 : TEXCOORD1;
            float3 uv0 : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f
        {
            fixed4 color : COLOR0;
            float2 uv0 : TEXCOORD0;
            float2 uv1 : TEXCOORD1;
            float2 uv2 : TEXCOORD2;
#if USING_FOG
            fixed fog : TEXCOORD3;
#endif
            float4 pos : SV_POSITION;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        half4 _WindDir;
        half4 _WindPos;

        v2f vert(appdata IN)
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(IN);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            half4 color = IN.color;
            o.color = saturate(color);

            o.uv0 = IN.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
            o.uv1 = IN.uv1.xy * unity_Lightmap_ST.xy + unity_Lightmap_ST.zw;
            o.uv2 = IN.uv0.xy * _MainTex_ST.xy + _MainTex_ST.zw;

#if USING_FOG
            float3 eyePos = UnityObjectToViewPos(IN.pos);
            float fogCoord = length(eyePos.xyz);
            UNITY_CALC_FOG_FACTOR_RAW(fogCoord);
            o.fog = saturate(unityFogFactor);
#endif

#ifdef _GLOBAL_WIND_EFFECT
            float4 WSPos = mul(unity_ObjectToWorld,IN.pos);
            
            
            float3 windPos = WSPos.xyz - _WindPos.xyz;
            float  windDist = length(windPos);
            float3 nWindDir = windPos * rcp(windDist);

            float x = CosLike(windDist*4.1459827 + _Time.y) * 0.5 + 0.5;

            WSPos.xyz += normalize(-IN.tangent.xyz + _WindDir.xyz + nWindDir) * (x * 0.1) * (IN.color.x);
            
            float4 OWpos =mul(unity_WorldToObject,WSPos);

            o.pos = UnityObjectToClipPos(OWpos);
#else
            o.pos = UnityObjectToClipPos(IN.pos);
#endif

            return o;
        }

        sampler2D _MainTex;
        fixed4 _Color;
        fixed _Cutoff;


        fixed4 frag(v2f IN) : SV_Target
        {
            half4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, IN.uv0.xy);
            half4 bakedColor = half4(DecodeLightmap(bakedColorTex), 1.0);

            fixed4 col =  _Color;

            fixed4 tex = tex2D(_MainTex, IN.uv2.xy);

            col.rgb = tex.rgb * col.rgb;
            col.a = tex.a * IN.color.a;

            clip(col.a - _Cutoff);

#if USING_FOG
            col.rgb = lerp(unity_FogColor.rgb, col.rgb, IN.fog);
#endif
            return col;
        }

        ENDCG
    }

    // Lightmapped
    Pass
    {
        Tags{ "LIGHTMODE" = "VertexLM" "QUEUE" = "AlphaTest" "IGNOREPROJECTOR" = "true" "RenderType" = "TransparentCutout" }
        AlphaToMask On
        ColorMask RGB

        CGPROGRAM

        #pragma vertex vert
        #pragma fragment frag
        #pragma target 3.0
        #include "UnityCG.cginc"
        #pragma multi_compile_fog
        #pragma multi_compile_instancing
        #pragma multi_compile _ _GLOBAL_WIND_EFFECT
        
        #define USING_FOG (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))

        float4 unity_Lightmap_ST;
        float4 _MainTex_ST;
        
        struct appdata
        {
            float3 pos : POSITION;
        #ifdef _GLOBAL_WIND_EFFECT    
            float4 tangent : TANGENT;
        #endif    
            half4 color : COLOR;
            float3 uv1 : TEXCOORD1;
            float3 uv0 : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f
        {
            fixed4 color : COLOR0;
            float2 uv0 : TEXCOORD0;
            float2 uv1 : TEXCOORD1;
            float2 uv2 : TEXCOORD2;
#if USING_FOG
            fixed fog : TEXCOORD3;
#endif
            float4 pos : SV_POSITION;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        half4 _WindDir;
        half4 _WindPos;

        v2f vert(appdata IN)
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(IN);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            half4 color = IN.color;
            o.color = saturate(color);

            o.uv0 = IN.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
            o.uv1 = IN.uv1.xy * unity_Lightmap_ST.xy + unity_Lightmap_ST.zw;
            o.uv2 = IN.uv0.xy * _MainTex_ST.xy + _MainTex_ST.zw;

#if USING_FOG
            float3 eyePos = UnityObjectToViewPos(IN.pos);
            float fogCoord = length(eyePos.xyz);
            UNITY_CALC_FOG_FACTOR_RAW(fogCoord);
            o.fog = saturate(unityFogFactor);
#endif

#ifdef _GLOBAL_WIND_EFFECT
            float4 WSPos = mul(unity_ObjectToWorld,IN.pos);
            
            float3 windPos = WSPos.xyz - _WindPos.xyz;
            float  windDist = length(windPos);
            float3 nWindDir = windPos * rcp(windDist);

            //sim curl
            float x = CosLike(windDist*4.1459827 + _Time.y) * 0.5 + 0.5;

            WSPos.xyz += normalize(-IN.tangent.xyz + _WindDir.xyz + nWindDir) * (x * _WindDir.w) * (IN.color.x);

            float4 OWpos =mul(unity_WorldToObject,WSPos);

            o.pos = UnityObjectToClipPos(OWpos);
#else
            o.pos = UnityObjectToClipPos(IN.pos);
#endif            
            return o;
        }

        sampler2D _MainTex;
        fixed4 _Color;
        fixed _Cutoff;


        fixed4 frag(v2f IN) : SV_Target
        {
            half4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, IN.uv0.xy);
            half4 bakedColor = half4(DecodeLightmap(bakedColorTex), 1.0);

            fixed4 col = bakedColor * _Color;

            fixed4 tex = tex2D(_MainTex, IN.uv2.xy);

            col.rgb = tex.rgb * col.rgb;
            col.a = tex.a * IN.color.a;

            clip(col.a - _Cutoff);

#if USING_FOG
            col.rgb = lerp(unity_FogColor.rgb, col.rgb, IN.fog);
#endif
            return col;
        }

        ENDCG
    }

    // Pass to render object as a shadow caster
    Pass {
        Name "Caster"
        Tags { "LightMode" = "ShadowCaster" }

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 2.0
#pragma multi_compile_shadowcaster
#pragma multi_compile_instancing // allow instanced shadow pass for most of the shaders
#include "UnityCG.cginc"

struct v2f {
    V2F_SHADOW_CASTER;
    float2  uv : TEXCOORD1;
    UNITY_VERTEX_OUTPUT_STEREO
};

uniform float4 _MainTex_ST;

v2f vert( appdata_base v )
{
    v2f o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
    o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
    return o;
}

uniform sampler2D _MainTex;
uniform fixed _Cutoff;
uniform fixed4 _Color;

float4 frag( v2f i ) : SV_Target
{
    fixed4 texcol = tex2D( _MainTex, i.uv );
    clip( texcol.a*_Color.a - _Cutoff );

    SHADOW_CASTER_FRAGMENT(i)
}
ENDCG

    }

}

}

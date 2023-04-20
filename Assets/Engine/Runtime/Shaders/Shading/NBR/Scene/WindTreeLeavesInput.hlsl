#ifndef WINDTREELEAVESINPUT
#define WINDTREELEAVESINPUT   
    
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

	#define UNITY_PI            3.14159265359f

	CBUFFER_START(UnityPerMaterial)
        TYPE4 _MainTex_ST;
        half4 _noiseTexTree_ST;

	    half4 _BaseColor,_BaseColor1;
		half4 _OffsetCorrection;
		half4 _LerpColor;
        half4 _DarkColor;
		half4 _AOTint;
        half _saturate;
        half _ColorDetails;
        half _TreeLerpTop;
        half _TreeLerpRoot;
        half _TreeLerpIntensity;

        half _stepA;
        half _Cutoff;
        half _CutIntensity;

        half _LightIntensity;
        half _FaceLightGrayIntensity;
        half _ToonCutPos;
        half _SHIntensity;
        half _SHColorIntensity;
        half _LightSHPow;

        half _LocalShadowDepthBias;
        half _shadowAttenInt;
        half _SmartShadowInt;
        half _noiseIntTree;
        half _noiseOffestTree;

        half _AORange;

        half _SubSurfaceGain;
        half _SubSurfaceScale;
        half _subSurfaceTermInt;

        //half4 _HardRimTint;
        //TYPE _LodMask;
        //TYPE _HardRimDistanceIntensity;
        //TYPE _HardRimWidth;

        half _Magnitude,_Frenquency,_ModelScaleCorrection,_MaskRange;
        half _Blend,_StableRange;

     //    half4 _LightColor;
	    // half _Frequency;
	    // half _WindSineIntensity;
     //    half _WindTexScale;
     //    half _WindTexIntensity;
     //    half _WindTexMoveSpeed;
     //    half4 _WindDirection;
        
        half _Debug;
        // TYPE _CustomBloomIntensity;
        // TYPE _CustomBloomAlphaOffset;

        half _DitherAmountMax;
        half _DitherAmountMin;

        half _DitherTransparency;
	CBUFFER_END

    TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);
    TEXTURE2D(_WindTex);
    TEXTURE2D(_noiseTexTree);       SAMPLER(sampler_noiseTexTree);
    SAMPLER(sampler_WindTex);
    TEXTURE2D(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);

    struct appdata
    {
        TYPE_POSITION3 positionOS           : POSITION;
        TYPE3 normalOS             : NORMAL;
        TYPE3 tangentOS            : TANGENT;
        half4 color                 : COLOR;
        TYPE2 uv                   : TEXCOORD0;
        TYPE2 uv2                   : TEXCOORD1;
        TYPE2 uv3                  : TEXCOORD2;
        //float3	Color : COLOR;
        //UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2f
    {
        TYPE_POSITION4 positionHCS			: SV_POSITION;
        TYPE2 uv                   : TEXCOORD0;
        TYPE_POSITION3 positionWS	    	: TEXCOORD1;
        TYPE3 normalWS	            : TEXCOORD2;
        half treeParam              : TEXCOORD3;
        half4 ambient               : TEXCOORD4;       
        TYPE3 baseNormal           : TEXCOORD5;        // 目前是把模型平滑前的法线存进了切线中
        //HEIGHT_FOG_COORDS(7)
		//half  fogCoord		        : TEXCOORD6;
        #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        TYPE4 shadowCoord              : TEXCOORD6;
        #endif
        #if _NeedScreenPos
            half2 screenUV          : TEXCOORD7;
        #endif
        #if _DEBUGMODE
            half debugWind          : TEXCOORD8;
            half4 VertexColor       : TEXCOORD9;
        #endif
        half4 normalOS       : TEXCOORD10;

        //UNITY_VERTEX_INPUT_INSTANCE_ID
		//UNITY_VERTEX_OUTPUT_STEREO
    };

//* wanghaoyu 用到的函数
    void Unity_Dither(TYPE In, TYPE2 ScreenPosition, out TYPE Out)
    {
        TYPE2 SCREEN_PARAM = TYPE2(1, 1);
        TYPE2 uv = ScreenPosition.xy * SCREEN_PARAM;
        TYPE DITHER_THRESHOLDS[16] = {
            1.0 / 17.0, 9.0 / 17.0, 3.0 / 17.0, 11.0 / 17.0,
            13.0 / 17.0, 5.0 / 17.0, 15.0 / 17.0, 7.0 / 17.0,
            4.0 / 17.0, 12.0 / 17.0, 2.0 / 17.0, 10.0 / 17.0,
            16.0 / 17.0, 8.0 / 17.0, 14.0 / 17.0, 6.0 / 17.0
        };
        uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
        Out = In - DITHER_THRESHOLDS[index];
    }

#endif

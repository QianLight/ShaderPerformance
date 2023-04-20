Shader"URP/SFX/UVEffect_Manual"
{
    Properties
    {
        [HideInInspector]_StencilRef("Stencil Ref", Int) = 0
        [Toggle]_ShaderMode("Shader Mode", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)]_BlendMode("Blend Mode", Float) = 10
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("Cull Mode", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("ZTest", Float) = 4
        [Enum(On,1,Off,0)]_DepthMode("Depth Mode", Float) = 0
        [Toggle]_UseTurbulence("Use Turbulence", Float) = 0
        [Toggle]_UseClip("Use Clip", Float) = 0
        [Toggle]_UseMask("Use Mask", Float) = 0
        _CUTOUT("Cutout", Range(0,1)) = 0.5
        _Brightness("Brightness", Float) = 1
        _Contrast("Contrast", Float) = 1
        [HDR]_MainColor("Main Color", Color) = (1, 1, 1, 1)
        [Toggle]_UseBackColor("Use BackColor", Float) = 0
        [HDR]_BackColor("Back Color", Color) = (1, 1, 1, 1)
        _MainTex ("Main Tex", 2D) = "white" {}
        [Toggle]_AlphaR("Alpha / R", Float) = 0
        _MainPannerX("MainPannerX", Float) = 0
        _MainPannerY("MainPannerY", Float) = 0
		_MainRamp("Main Ramp", 2D) = "white" {}
		[Toggle]_UseMainRamp("Use MainRamp", Range( 0 , 1)) = 0
        _TurbulenceTex("TurbulenceTex", 2D) = "white" {}
        _DistortPower("DistortPower", Float) = 0
        _PowerU("PowerU", Float) = 0
        _PowerV("PowerV", Float) = 0
        _MaskTex("Mask Tex", 2D) = "white" {}
        _Hardness("Hardness", Range(0, 0.99)) = 0
        _Dissolve("Dissolve", Float) = 0    
        [HDR]_WidthColor("Width Color", Color) = (1, 1, 1, 1)
        _EdgeWidth("EdgeWidth", Range(0,1)) = 0
        _Alpha("Alpha", Range(0, 10)) = 1
        [Toggle]_UseDepthFade("Use Depth Fade", Float) = 0
        _FadeLength("Fade Length", Range(0, 10)) = 0.5
        [Toggle]_Usefresnel("Use Fresnel", Float) = 0
        _fresnelpower("fresnel Power", Range(0,20)) = 1
        _fresnelmultiply("fresnel Multiply", Range(0,30))= 1
        _Flip("Flip", Range(0,1)) = 0
        [XToggle]_UseRamp("Use Ramp", Float) = 0
        _Color0("Color 0", Color) = (1, 0, 0, 0)
        _Color1("Color 1", Color) = (0, 1, 0, 0)
        _Color2("Color 2", Color) = (0, 0, 1, 0)
        _RampParam("Ramp Param", Vector) = (0.3, 0.33, 0.6, 0.66)
        [XToggle]_CameraFade("Camera Fade", Float) = 0
        _CameraFadeOffset("Camera Fade Offset", Float) = 0
        _CameraFadeLength("Camera Fade Length", Float) = 5
        [Toggle]_UseVertexColor("Use VertexColor", Float) = 1
        [Toggle]_UseFog("Use Fog", Float) = 0
        [Toggle]_UseShadowMask("Use Shadowmask", Float) = 0
        _ShadowStr("Shadow Str", Range(0,1)) = 0.25
        _Sm("square mask(UI用)",Range(0.001,0.1))=0.012
        [HideInInspector]_ClipRect("Clip Rect(UI用)", Vector) = (1,1,0,0)
        [Toggle]_ForceNoParkour("强制不受跑酷关弯曲效果影响",Float) = 0
        _FlashStr("Flash Strength", range(0,1)) = 0.5
        [Toggle]_StaticAlpha("强制不受组队alpha变化控制", Float) = 0
        [HideInInspector]_DistinguishAlpha("GlobalAlpha", Range(0,1)) = 1
    }
    SubShader
    {
        Tags 
        { 
            "RenderPipeline"="UniversalPipeline" 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "LightMode"="UniversalForward"
        }
        stencil
        {
            Ref [_StencilRef]
            Comp Always
            Fail Replace
            Pass Replace
        }
        
        LOD 100
        Blend SrcAlpha [_BlendMode]
        ZWrite [_DepthMode]
        Cull [_CullMode]
        ZTest [_ZTest]
        Offset 0 , 0
		ColorMask RGBA
        AlphaToMask Off
        
        Pass
        {
            HLSLPROGRAM
            
            #pragma shader_feature_local _ _SHADERMODE_ON
            #pragma shader_feature_local _ _USEFRESNEL_ON
            #pragma shader_feature_local _ _USETURBULENCE_ON
            #pragma shader_feature_local _ _USECLIP_ON
            #pragma shader_feature_local _ _USEMASK_ON
            // #pragma shader_feature_local _ _DEPTHMODE_ON
            #pragma shader_feature_local _ _USEDEPTHFADE_ON
            // #pragma shader_feature_local _ _USERAMP_ON
            // #pragma shader_feature_local _ _CAMERAFADE_ON
            #pragma shader_feature_local _ _USEMAINRAMP_ON
            #pragma shader_feature_local _ _USEFOG_ON
            #pragma shader_feature_local _ _USESHADOWMASK_ON
            #pragma shader_feature_local _ _FORCENOPARKOUR_ON
            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #define MAIN_LIGHT_CALCULATE_SHADOWS
            #define _MAIN_LIGHT_SHADOWS
            // _FX_LEVEL_MEDIUM _FX_LEVEL_LOW
			#pragma multi_compile _ _FX_LEVEL_HIGH
            // #pragma multi_compile_fragment _ _HUAWEI
			// #pragma multi_compile _ _SHADER_LEVEL_HIGH
            // #pragma multi_compile_instancing
            #pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
            #define _SHADOWS_SOFT
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "../Include/Fog.hlsl"
            #include "Assets/Engine/Runtime/Shaders/Shading/Scene/ParkourDistort.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 normal : NORMAL;
                half4 color : COLOR;
            #ifndef _SHADERMODE_ON
                half4 custom1 : TEXCOORD1;
                half4 custom2 : TEXCOORD2;
            #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                //x:? y:? z:dissolve w:distort
                half4 custom1 : TEXCOORD1;
                //x:? y:? z:edgeWidth w:edgeBrightness
                half4 custom2 : TEXCOORD2;
                //x: clipX y:clipY z:fresnel w:camerafade
                half4 deliver : TEXCOORD3;
            // #if defined _USEDEPTHFADE_ON || defined _CAMERAFADE_ON
                float4 screenPos : TEXCOORD4;
            // #endif
            // #if defined _USEFRESNEL_ON || defined _USERAMP_ON || defined _USESHADOWMASK_ON
                float3 worldNormal : TEXCOORD7;
            // #endif
            // #ifdef _USERAMP_ON
            //     half3 mixer : TEXCOORD5;
            // #endif
                float3 worldPos : TEXCOORD5;
            #if defined _USESHADOWMASK_ON /*&& defined _SHADER_LEVEL_HIGH*/
                float4 shadowCoord : TEXCOORD6;
            #endif
            #if defined UNITY_UI_CLIP_RECT
                float4 originalPos : TEXCOORD8;
            #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
            };

            Texture2D _MainTex;
            SamplerState sampler_MainTex;
            Texture2D _MainRamp;
            SamplerState sampler_MainRamp;
            

        #if defined _USETURBULENCE_ON || defined _USECLIP_ON
            Texture2D _TurbulenceTex;
            SamplerState sampler_TurbulenceTex;
        #endif

        #ifdef _USEMASK_ON
            Texture2D _MaskTex;
            SamplerState sampler_MaskTex;
        #endif

    #ifdef _FX_LEVEL_HIGH
        #ifdef _USEDEPTHFADE_ON
            Texture2D _CameraDepthTexture;
            SamplerState sampler_CameraDepthTexture;
        #endif
    #endif

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _MainRamp_ST;
            float4 _TurbulenceTex_ST;
            float4 _MaskTex_ST;
            half4 _BackColor;
            half4 _MainColor;
            half4 _WidthColor;
            half4 _Color0;
            half4 _Color1;
            half4 _Color2;
            half4 _RampParam;
            float _MainPannerX;
            float _MainPannerY;
            float _ShadowStr;
            half _AlphaR;
            half _UseBackColor;
            half _Brightness;
            half _Alpha;
            half _Contrast,_DistinguishAlpha;
            half _CUTOUT;
            half _PowerU;
            half _PowerV;
            half _fresnelpower;
            half _fresnelmultiply;
            half _Flip;
            half _DistortPower;
            half _Dissolve;
            half _EdgeWidth;
            half _Hardness;
            half _FadeLength;
            half _CameraFadeLength;
            half _CameraFadeOffset;
            half _UseVertexColor;
            half _FlashStr;
            half _StaticAlpha;

            half _UseClip;
            half _DepthMode;
            half _UseRamp;
            half _CameraFade;
            half _Sm;
            half4 _ClipRect;
            CBUFFER_END
            
            half _IsInUICamera;//Add By: Takeshi
            float4 _ThunderParams;

            float UnityGet2DClipping (float2 position,float4 clipRect)
            {
                //float2 inside = step(clipRect.xy, position.xy) * step(position.xy, clipRect.zw);
                //return inside.x * inside.y;

                float min01 = _Sm+clipRect.z;
                float min02 = (_Sm+clipRect.w);
                float min03 = (_Sm*-1+clipRect.x);
                float min04 = (_Sm*-1+clipRect.y);
                float insideXrame = smoothstep(min01,clipRect.z-_Sm,position.x) * smoothstep(min03,clipRect.x+_Sm,position.x);
                float insideYrame = smoothstep(min02,clipRect.w-_Sm,position.y) * smoothstep(min04,clipRect.y+_Sm,position.y);
                return insideXrame*insideYrame;
            }

            v2f vert (appdata v)
            {
                v2f o;

                o.uv = v.uv;
                o.color = v.color;
                half fresnel = 1;
                // #if defined _USEFRESNEL_ON || defined _USERAMP_ON || defined _USESHADOWMASK_ON
                o.worldNormal = TransformObjectToWorldNormal(v.normal, true);
                // #endif
            #if defined UNITY_UI_CLIP_RECT
                o.originalPos = v.vertex;
            #endif

                o.worldPos = TransformObjectToWorld(v.vertex);

            #ifndef _FORCENOPARKOUR_ON
                UNITY_BRANCH
                if(_IsParkour==1 && _IsInUICamera == 0)
                {
					o.worldPos = ParkourDistortVertex(o.worldPos);
                }
            #endif
				o.vertex = TransformWorldToHClip(o.worldPos);
                
            #ifdef _SHADERMODE_ON
                half2 panner = float2(_MainPannerX * _TimeParameters.x, _MainPannerY * _TimeParameters.x);
                panner = frac(panner);
                o.custom1 = half4(panner.x, panner.y, max(1 - fresnel, 1) * _Dissolve, _DistortPower);
                o.custom2 = half4(1, 1, _EdgeWidth, 1);
            #else
                o.custom1 = v.custom1;
                o.custom2 = half4(v.custom2.x, v.custom2.y, v.custom2.z, v.custom2.w + 1);
            #endif
                
                half2 xy = 1;
            #ifdef _USECLIP_ON
                // UNITY_BRANCH
                // if (_UseClip)
                // {
                    half dissolve = o.custom1.z;
                    half edgeWidth = o.custom2.z;
                    half edgeDissolve = dissolve * edgeWidth + dissolve;
                    xy = half2(edgeDissolve, edgeDissolve - edgeWidth);
                    xy = xy * 2 - xy * _Hardness;
                // }
            #endif

            #if defined _USESHADOWMASK_ON /*&& defined _SHADER_LEVEL_HIGH*/
				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex);
				o.shadowCoord = GetShadowCoord( vertexInput );
            #endif
                half cameraDepthFade = 1;
            // #ifdef _CAMERAFADE_ON
                // UNITY_BRANCH
                if (_CameraFade)
                {
                    half z = -TransformWorldToView(TransformObjectToWorld(v.vertex.xyz)).z;
                cameraDepthFade = (( z -_ProjectionParams.y - _CameraFadeOffset ) / _CameraFadeLength);
                cameraDepthFade = saturate(cameraDepthFade);
                }
            // #endif
                
                o.deliver = half4(xy.x,xy.y,1,cameraDepthFade);
                return o;
            }

            half4 frag (v2f i, half vface : VFACE) : SV_Target
            {
            #if defined _USETURBULENCE_ON || defined _USECLIP_ON
                float2 turbulenceUV = i.uv * _TurbulenceTex_ST.xy + _TurbulenceTex_ST.zw + frac(half2(_PowerU, _PowerV) * _TimeParameters.x);
                half turbulenceTex = SAMPLE_TEXTURE2D(_TurbulenceTex, sampler_TurbulenceTex, turbulenceUV);
            #endif
                
                half2 clipXY = i.deliver.xy;
            #ifdef _USECLIP_ON                
                clipXY = 1 + turbulenceTex - clipXY;
                clipXY = saturate((clipXY - _Hardness) / (1 - _Hardness));
            #endif

                half mask = 1;
            #ifdef _USEMASK_ON
                mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv * _MaskTex_ST.xy + _MaskTex_ST.zw);
            #endif
                
                // half dissolve = i.custom1.z;
                // half distort = i.custom1.w;
                // half2 edgeWB = i.custom2.zw;
                float2 customUV = i.custom2.xy - 1;
                float2 mainUV = (i.uv - 0.5) * customUV;/*i.uv * customUV - (customUV * 0.5);*/
                float2 offset = _MainTex_ST.zw + i.custom1.xy;
                float2 uvOffset = i.uv * _MainTex_ST.xy + offset;
                mainUV += uvOffset;
            #ifdef _USETURBULENCE_ON
                half turbulence = (turbulenceTex - 0.5) * i.custom1.w;
                mainUV += turbulence;
            #endif
                
                half4 main = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainUV);
                half3 color = pow(main.xyz, _Contrast) * _Brightness * lerp(lerp(_MainColor.xyz, _BackColor.xyz, _UseBackColor), _MainColor, max(vface, 0));
                // UNITY_BRANCH
                if (_UseVertexColor)
                {
                    color *= i.color.rgb;
                }
                color = lerp(color * _WidthColor * i.custom2.w, color, clipXY.x);
            #ifdef _USEMAINRAMP_ON
                float2 uv_MainRamp = i.uv * _MainRamp_ST.xy + _MainRamp_ST.zw;
                half4 mainRamp = SAMPLE_TEXTURE2D(_MainRamp, sampler_MainRamp, uv_MainRamp);//zh
                color.rgb *= mainRamp.rgb;
            #endif


                half fresnel = 1;
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
            #ifdef _USEFRESNEL_ON
                fresnel = abs(dot(i.worldNormal, viewDir));
                fresnel = pow(1 - fresnel, _fresnelpower);
                fresnel = min(fresnel, 1) * _fresnelmultiply;
                // UNITY_BRANCH
                // if(_Flip)
                // {
                    fresnel = _Flip ? (1 - fresnel) : fresnel;
                // }
            #endif
                main.a = _MainColor.a * lerp(main.a, main.r, _AlphaR) * clipXY.y * _Alpha * fresnel * mask;
                // UNITY_BRANCH
                // if(_UseVertexColor)
                // {
                    main.a = _UseVertexColor ? i.color.a * main.a : main.a;
                // }
                main.a = min(main.a, 1);
                // main.a = min((_UseVertexColor ? i.color.a : 1) * _MainColor.a * lerp(main.a, main.r, _AlphaR) * clipXY.y * _Alpha * fresnel * mask,1);
        
            // #ifdef _DEPTHMODE_ON
                // UNITY_BRANCH
                if (_DepthMode)
                {
                    clip(main.a - _CUTOUT);
                    main.a = 1;
                }
                
            // #endif

                float4 clip = TransformWorldToHClip(i.worldPos);
        #ifdef _FX_LEVEL_HIGH
            #if defined _USEDEPTHFADE_ON
                float4 screenPos = ComputeScreenPos(clip);
                screenPos.xy = screenPos.xy / screenPos.w;
                screenPos.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? screenPos.z : screenPos.z * 0.5 + 0.5;
                float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, screenPos.xy), _ZBufferParams);
                float objDepth = screenPos.w/*LinearEyeDepth(screenPos.z, _ZBufferParams)*/;
                float delta = saturate(smoothstep( 0, _FadeLength, abs(sceneDepth - objDepth)));
                main.a *= delta;
            #endif
        #endif
                
            // #ifdef _USERAMP_ON
                UNITY_BRANCH
                if (_UseRamp)
                {
                    half rate = 1 - saturate(dot(_MainLightPosition.xyz, i.worldNormal) * 0.5 + 0.5);
                    half rate1 = smoothstep(_RampParam.x, _RampParam.y, rate);
                    half rate2 = smoothstep(_RampParam.z, _RampParam.w, rate);
                    half3 mixer = lerp(_Color0, _Color1, rate1);
                    mixer = lerp(mixer, _Color2, rate2);
                    color *= mixer.xyz;
                }
                // main.a *= i.mixer.w;
            // #endif

            #if defined _USESHADOWMASK_ON/* && defined _SHADER_LEVEL_HIGH*/
                half4 shadowCoord = float4(0, 0, 0, 0);
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	               shadowCoord = i.shadowCoord;
                #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
	               shadowCoord = TransformWorldToShadowCoord(i.worldPos);
                #endif
                half shadow = MainLightRealtimeShadow(shadowCoord);
                color.rgb *= (shadow * _ShadowStr + 1 - _ShadowStr);
                // half shadow = MainLightShadow(shadowCoord, i.worldPos, 1, _MainLightOcclusionProbes,i.worldNormal);
                // color *= shadow;
            #endif
            
                //Thunder
                color.rgb *= 1 + saturate(_ThunderParams.y) * _FlashStr;

                half luminance = Luminance(color) + 0.001;
                color = max(color / luminance * min(luminance, 4), 0);
            #ifdef _USEFOG_ON
                UNITY_BRANCH
                if (_ScatterEnable)
                {
                    float depth01;
                    float3 scatter = GetScatterRGB(viewDir, i.worldPos, _WorldSpaceCameraPos, _MainLightDir0.xyz, depth01, _ProjectionParams, _CameraBackward);
                    // APPLY_FOG(scatter, inputData.positionWS);
                    color.rgb = lerp(color.rgb, scatter, saturate(depth01 * 2));
                }     
                APPLY_FOG(color.rgb, i.worldPos)
            #endif
                
                main.a *= i.deliver.w;
                main.a = max(main.a, 0);
                color = lerp(color,LinearToSRGB(color),_IsInUICamera);//Add by: Takeshi; Fix UI Gamma
                main.a *= _StaticAlpha ? 1 : _DistinguishAlpha;
                
#ifdef UNITY_UI_CLIP_RECT
                 main.a *= UnityGet2DClipping(i.originalPos.xy, _ClipRect);
#endif
                return half4(color,main.a);
            }
            ENDHLSL
        }
        Pass
        {
        	Name "OverdrawF"
        	Tags{"LightMode" = "OverdrawForwardBaseT"}

        	Blend One One
        	ZWrite [_DepthMode]
        	ZTest [_ZTest]
        	Offset 0 , 0
        	ColorMask RGBA
        	HLSLPROGRAM

        	#pragma vertex Vert
        	#pragma fragment Frag

        	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        	struct Attributes
        	{
        		float4 vertex : POSITION;
        	};
        	
        	struct Varyings
        	{
        		float4 vertex : SV_POSITION;
        	};
        	Varyings Vert(Attributes v)
        	{
        		Varyings o;
        		float4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
        		o.vertex = mul(unity_MatrixVP, WorldPosition);
        		return o;
        	}

        	half4 Frag(Varyings i) : SV_Target
        	{
        		return half4(0.1, 0.04, 0.02, 1);
        	}

        	ENDHLSL
        }
    }
}

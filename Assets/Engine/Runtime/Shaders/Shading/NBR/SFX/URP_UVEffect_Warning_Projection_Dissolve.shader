Shader "URP/SFX/URP_UVEffect_Warning_Projection_New"
{
    Properties
    {
        [Header(builtin)]
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("_SrcBlend (default = SrcAlpha)", Float) = 5 // 5 = SrcAlpha
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("_DstBlend (default = OneMinusSrcAlpha)", Float) = 10 // 10 = OneMinusSrcAlpha
        // TODO 固定写死
        //_StencilRef("_StencilRef", Float) = 5
        //[Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("StencilComp", Float) = 0 //0 = disable
        //[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("ZTest", Float) = 0 //0 = disable
        //[Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull", Float) = 1 //1 = Front
        [Header(MainControl)]
        _ColorInt("ColorInt", Float) = 1
        _AlphaInt("AlphaInt",Float) = 1
        _Contrast("Contrast", Float) = 1
        [HDR]_MainColor("Main Color", color) = (1,1,1,1)
        _MainTex("Main Tex", 2D) = "white" {}
        [Toggle]_AlphaR("Alpha / R", Float) = 0
        _MainPannerX("MainPannerX", Float) = 0
        _MainPannerY("MainPannerY", Float) = 0
        
        [Header(Turbulence)]
        _TurbulenceTex("TurbulenceTex", 2D) = "white" {}
        _DistortPower("DistortPower", Float) = 0
        _PowerU("PowerU", Float) = 0
        _PowerV("PowerV", Float) = 0
        
        [Header(Mask)]
        _MaskTex("MaskTex", 2D) = "white" {}
        
        [Header(CustomData)]
        _Hardness("Hardness", Range(0, 0.99)) = 0
        _Dissolve("Dissolve", Float) = 0    
        [HDR]_WidthColor("Width Color", Color) = (1, 1, 1, 1)
        _EdgeWidth("EdgeWidth", Range(0,1)) = 0
        
        [Header(Variant)]
        [Toggle]_UseClip("Use Clip", Float) = 0
        [Toggle]_UseTurbulence("Use Turbulence", Float) = 0
        [Toggle(_UseMask)]_UseMask("Use Mask", Float) = 0
        //[Toggle(_UnityFogEnable)] _UnityFogEnable("_UnityFogEnable (default = on)", Float) = 1
        //[Toggle(_Polar)] _Polar("Polar (default = on)", Float) = 1
        [Toggle] _PolarTog("PolarTog", Int) = 1
        //[Toggle(_Angle_Tog)] _AngleTog("AngleTog (default = on)", Float) = 1
        //[Toggle(_Particle)] _Particle("Particle (default = on)", Float) = 1
        [Toggle(_DebugColor)]_DebugColor("DebugColor", Float) = 0
        [Header(Angle)]
        _Angle("Angle",Range(0,360))=360
        
        [HideInInspector]_Scale("_Scale",float)=1
        //[Toggle(_SHOW_RED_ON)] _DisplayColor ("显示红色?", float) = 0
        [Header(Scale)]
        [Toggle(_UseScale)]_UseScale("Use Scale", Float) = 0
        _EdgeWidth_Mask("EdgeWidth_mask",float)=0.1
        _LeftAndRight("LeftAndRight",float)=1
        _UpAndBelow("_UpAndBelow",float)=1
        
        [Toggle]_UseShadowMask("Use Shadowmask", Float) = 0
        _ShadowStr("Shadow Str", Range(0,1)) = 0.25
    }

    SubShader
    {
        // 为了避免呈现顺序问题，Queue必须进入透明队列 >= 2501, 
        Tags { "RenderType" = "Overlay" "Queue" = "Transparent-499" "DisableBatching" = "True" }

        Pass
        {
            Stencil
            {
                Ref[_StencilRef_Warning]
                Comp Greater
            }
            Cull Front
            ZTest off
            ZWrite off
            Blend[_SrcBlend][_DstBlend]
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog
            // due to using ddx() & ddy()
            #pragma target 3.0
            //#pragma shader_feature_local _UnityFogEnable
            //#pragma shader_feature _Angle_Tog
            //#pragma shader_feature _SHOW_RED_ON
            #pragma multi_compile _ _UseMask
            #pragma multi_compile _ _DebugColor
            #pragma multi_compile _ _USETURBULENCE_ON
            #pragma multi_compile _ _USECLIP_ON
            #pragma multi_compile _ _UseScale
            #pragma shader_feature_local _ _USESHADOWMASK_ON
            #pragma multi_compile MAIN_LIGHT_CALCULATE_SHADOWS
            #define _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct appdata
            {
                float3 positionOS : POSITION;
                float4 center_spread : TEXCOORD1;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                float4 viewRayOS : TEXCOORD1; // xyz: viewRayOS, w: extra copy of positionVS.z 
                float4 cameraPosOSAndFogFactor : TEXCOORD2;
                float4 center_spread : TEXCOORD3;
                float2 clipXY : TEXCOOED4;
                float3 positionWS : TEXCOORD5;
            #ifdef _USESHADOWMASK_ON 
                float4 shadowCoord : TEXCOORD6;
            #endif
            };
            #if defined _USETURBULENCE_ON || defined _USECLIP_ON
                Texture2D _TurbulenceTex;
                SamplerState sampler_TurbulenceTex;
            #endif
            Texture2D _MainTex;
            SamplerState sampler_MainTex;
            sampler2D _CameraDepthTexture;
            Texture2D _MaskTex;
            SamplerState sampler_MaskTex;
            
            CBUFFER_START(UnityPerMaterial)               
            float4 _MaskTex_ST;
            half _PowerU,_PowerV;
            float4 _TurbulenceTex_ST;
            half _Hardness;
            half _Dissolve;
            half _EdgeWidth;
            float _MainPannerX,_MainPannerY;
            half _Contrast;
            int _PolarTog;
            float _DistortPower;
            float4 _MainTex_ST;
            float _ProjectionAngleDiscardThreshold;
            half4 _MainColor,_WidthColor;
            half _AlphaR,_Angle,_ColorInt,_AlphaInt;
            half _EdgeWidth_Mask,_LeftAndRight,_UpAndBelow;
            float _ShadowStr;
            CBUFFER_END

            v2f vert(appdata input)
            {
                v2f o;
                VertexPositionInputs vertexPositionInput = GetVertexPositionInputs(input.positionOS);
                o.positionCS = vertexPositionInput.positionCS;
                // #if _UnityFogEnable//雾效支持
                //                 o.cameraPosOSAndFogFactor.a = ComputeFogFactor(o.positionCS.z);
                // #else
                //                 o.cameraPosOSAndFogFactor.a = 0;
                // #endif
                o.screenPos = ComputeScreenPos(o.positionCS);
                float3 viewRay = vertexPositionInput.positionVS;
                o.viewRayOS.w = viewRay.z;//将除法值存储到不同的o.viewRayOS.w中
                viewRay *= -1;
                o.center_spread = input.center_spread;
                float4x4 ViewToObjectMatrix = mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V);
                o.viewRayOS.xyz = mul((float3x3)ViewToObjectMatrix, viewRay);
                o.cameraPosOSAndFogFactor.xyz = mul(ViewToObjectMatrix, float4(0,0,0,1)).xyz; // 硬代码0或1可以启用许多编译器优化
                half2 xy = 1;
                #ifdef _USECLIP_ON
                    half dissolve = _Dissolve;
                    half edgeWidth = _EdgeWidth;
                    //half rHardness = (2 - _Hardness);
                    half edgeDissolve = dissolve * (1 + edgeWidth);
                    xy = half2(edgeDissolve, edgeDissolve - edgeWidth);
                    xy = xy * (2 - _Hardness);
                #endif
                o.clipXY = xy;
                o.positionWS = TransformObjectToWorld(input.positionOS);
            #ifdef _USESHADOWMASK_ON
				VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS);
				o.shadowCoord = GetShadowCoord( vertexInput );
            #endif
                return o;
            }
            float2 Polar(float2 UV)
            {
                //return 1;
                float2 uv = UV-0.5;
                float distance=length(uv);
                distance *=2;
                float angle=atan2(uv.x,uv.y);
                float angle01=angle/3.1415926/2+0.5;
                return float2(angle01,distance);
            }
            half4 frag(v2f i) : SV_Target
            {
                #if _DebugColor
                return 0.5f;
                #endif
                i.viewRayOS.xyz /= i.viewRayOS.w;
                float2 screenSpaceUV = i.screenPos.xy / i.screenPos.w;
                float sceneRawDepth = tex2D(_CameraDepthTexture, screenSpaceUV).r;
                float3 decalSpaceScenePos;
                float sceneDepthVS = LinearEyeDepth(sceneRawDepth,_ZBufferParams);
                decalSpaceScenePos = i.cameraPosOSAndFogFactor.xyz + i.viewRayOS.xyz * sceneDepthVS;
                float2 decalSpaceUV = (decalSpaceScenePos.xz + 0.5);
                decalSpaceUV.y = 1-decalSpaceUV.y;
                float shouldClip = 0;
                
                // #if _ProjectionAngleDiscardEnable//投影角度
                //                 float3 decalSpaceHardNormal = normalize(cross(ddx(decalSpaceScenePos), ddy(decalSpaceScenePos)));
                //                 shouldClip = decalSpaceHardNormal.z > _ProjectionAngleDiscardThreshold ? 0 : 1;
                // #endif
                clip(0.5 - abs(decalSpaceScenePos) - shouldClip);
                
                // #ifndef _Polar
                //                 float2 uv = decalSpaceUV.xy * _MainTex_ST.xy + _MainTex_ST.zw;// 
                // #else
                //                 float2 uv = decalSpaceUV.xy;
                //                 uv = Polar(uv) * _MainTex_ST.xy  + _MainTex_ST.zw;//-half2(1,0)
                // #endif
                // TODO UV
                float2 uv=1;
                // if(_PolarTog==0)
                // {
                //     uv = decalSpaceUV.xy;
                // }else if(_PolarTog==1)
                // {
                //     uv = Polar(decalSpaceUV.xy);
                // }
                uv = lerp(decalSpaceUV.xy,Polar(decalSpaceUV.xy),_PolarTog);


                // TODO MY_Test Scale -----------------
                //half2 lerpUV_sum =1;
                #if _UseScale
                half leftMask = step(uv.x,_EdgeWidth_Mask);
                half leftlerpUV = lerp(uv.x,uv.x*_LeftAndRight,leftMask);
                half rightMask = 1-step(uv.x,1-_EdgeWidth_Mask);
                //return  rightMask;
                half rightlerpUV = lerp(uv.x,pow(uv.x,_LeftAndRight),rightMask);
                half Mask_leftAndRigh = step(uv.x,0.5);
                half leftAddRight = lerp(rightlerpUV,leftlerpUV,Mask_leftAndRigh);
                //return leftlerpUV;
                half upMask = 1- step(uv.y,1-_EdgeWidth_Mask);
                half uplerpUV = lerp(uv.y,pow(uv.y,_UpAndBelow),upMask);
                //return uplerpUV;
                half belowMask = step(uv.y,_EdgeWidth_Mask);
                half belowlerpUV = lerp(uv.y,uv.y*_UpAndBelow,belowMask);
                half Mask_upAndBelow = step(uv.y,0.5);
                half upAddBelow = lerp(uplerpUV,belowlerpUV,Mask_upAndBelow);
                //return upAddBelow;
                uv = lerp(half2(leftAddRight,uv.y),half2(uv.x,upAddBelow),0.5);
                #endif
                // TODO MY_Test Scale -----------------

                //TODO Mask
                half mask=1;
                #if _UseMask
                    half2 maskUV = uv*_MaskTex_ST.xy+_MaskTex_ST.zw;
                    mask = SAMPLE_TEXTURE2D_LOD(_MaskTex, sampler_MaskTex ,maskUV,0).r;
                #endif

                half2 mainUV = uv * _MainTex_ST.xy + _MainTex_ST.zw+(float2(_MainPannerX * _TimeParameters.x, _MainPannerY * _TimeParameters.x));

                // TODO 等宽
                //-- scale
                // uv.y *= _Scale;
                // uv.y += 1-_Scale;

                // TODO 扭曲&溶解
                #if defined _USETURBULENCE_ON || defined _USECLIP_ON
                    float2 turbulenceUV = uv * _TurbulenceTex_ST.xy + _TurbulenceTex_ST.zw + frac(half2(_PowerU, _PowerV)* _TimeParameters.x);
                    half turbulenceTex = SAMPLE_TEXTURE2D(_TurbulenceTex, sampler_TurbulenceTex, turbulenceUV);
                #endif
                #ifdef _USETURBULENCE_ON
                    half turbulence = (turbulenceTex - 0.5) * _DistortPower;
                    mainUV += turbulence;
                    //mainUV=lerp(mainUV,turbulenceTex,_DistortPower);
                #endif
                    half2 clipXY = i.clipXY;
                #ifdef _USECLIP_ON
                    clipXY = 1 + turbulenceTex - clipXY;
                    clipXY = saturate((clipXY-_Hardness) / (1 - _Hardness));
                #endif
                // #if _UnityFogEnable
                //                 col.rgb = MixFog(col.rgb, i.cameraPosOSAndFogFactor.a);
                // #endif
                // TODO COLOR&A
                // half4 col = tex2D(_MainTex, uv);
                // half4 col = tex2Dlod(_MainTex, uv, 1);
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainUV );
                half col_R = col.r;
                col.rgb = pow(col.rgb,_Contrast)*_MainColor.rgb*_ColorInt;//对比度+强度
                col.rgb = lerp(col.rgb*_WidthColor,col.rgb,clipXY.x);

            #ifdef _USESHADOWMASK_ON
                half4 shadowCoord = float4(0, 0, 0, 0);
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	               shadowCoord = i.shadowCoord;
                #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
	               shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                #endif
                half shadow = MainLightRealtimeShadow(shadowCoord);
                col.rgb *= (shadow * _ShadowStr + 1 - _ShadowStr);
            #endif
                
                half luminance = Luminance(col.rgb) + 1E-07;
                col.rgb = max(col.rgb / luminance * min(luminance, 4), 0);
                col.a = min(clipXY.y * lerp(col.a, col_R, _AlphaR) * mask * _MainColor.a * _AlphaInt,1);
                col.a = max(col.a,0);
                
                //TODO 切角
                //#if _Angle_Tog
                half A= 1-saturate(abs(-2*uv+1));
                half B= _Angle/360;
                half AB = A-B;
                half C = 1-ceil(saturate(AB));
                col.a *= C;
                //#endif

                return col;
            }
            ENDHLSL
        }
    }
}
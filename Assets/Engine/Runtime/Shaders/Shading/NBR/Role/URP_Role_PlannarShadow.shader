Shader "URP/Role/PlannarShadow"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("Base Tex", 2D) = "white" {}
        _Color0("Main Color", Color) = (1,1,1,1)
        _ColorOutline("Outline Color", Color) = (0,0,0,0.003)
        [HideInInspector]_MatEffectOutlineParam("Outline Color", Color) = (0,0,0,0)
        _Color("Effect Color", Color) = (1,1,1,1)

        [HideInInspector] _SrcBlend("__src", float) = 1.0
        [HideInInspector] _DstBlend("__dst", float) = 0.0
        [HideInInspector] _ZWrite("__zw", float) = 1.0
        [HideInInspector] _Stencil ("Stencil ID", Float) = 5
        [HideInInspector] _ShadowPos("ShadowPos", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "PlanarShadow"

            Tags
            {
                "LightMode" = "PlanarShadow"
            }

            ZWrite Off

            // 在UI界面中：因为没有背景alpha和没有背景颜色，需要特殊处理。
            Blend SrcAlpha Zero
            // 如果要用在正常战斗中，有背景颜色，得使用正片叠底：
            //Blend DstColor Zero, SrcAlpha OneMinusSrcAlpha

            //深度稍微偏移防止阴影与地面穿插
            Offset -1 , 0

            Stencil
            {
                Ref [_StencilRef_PlanarShadow]
                Comp Equal
                Pass incrWrap
                Fail keep
                ZFail keep
            }

            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            CBUFFER_END

            float4 _RootPosWS;

            #define _MainColor _Color0
            sampler2D _MainTex;

            #define _ShadowHight _RootPosWS.y

            half4 _PlanarShadowColor;
            half4 _PlanarShadowParam;
            #define _PlanarShadowDir _PlanarShadowParam.xyz
            #define _PlanarShadowFalloff _PlanarShadowParam.w

            struct appdata
            {
                half4 vertex : POSITION;
                half2 uv : TEXCOORD0;
            };

            struct v2f
            {
                half4 vertex : SV_POSITION;
                half4 color : COLOR;
            };

            half3 ShadowProjectPos(half3 worldPos)
            {
                half3 shadowPos;
                shadowPos.y = min(worldPos.y, _ShadowHight);
                shadowPos.xz = worldPos.xz - _PlanarShadowDir.xz * max(0, worldPos.y - _ShadowHight) / _PlanarShadowDir.
                    y;
                return shadowPos;
            }

            float4 GetRootPos()
            {
                return half4(_RootPosWS.xyz, 1);
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                half3 positionWS = TransformObjectToWorld(v.vertex);
                half3 shadowPos = ShadowProjectPos(positionWS);
                o.vertex = TransformWorldToHClip(shadowPos);
                half3 temp = cross(half3(0, 1, 0), _PlanarShadowDir.xyz);
                half3 shadowUp = cross(_PlanarShadowDir.xyz, temp);
                half shadowAlpha = 1 - saturate(dot(shadowUp, positionWS.xyz - GetRootPos().xyz) / _PlanarShadowFalloff);

                // 战斗场景中使用这个算法：
                // o.color.rgb = lerp(1, _PlanarShadowColor, shadowAlpha);
                // o.color.a = 1;

                // RT中使用这个算法： 
                o.color = _PlanarShadowColor;
                o.color.a *= shadowAlpha;

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDHLSL
        }
    }
    CustomEditor "CFEngine.Editor.PBSShaderGUI"
}
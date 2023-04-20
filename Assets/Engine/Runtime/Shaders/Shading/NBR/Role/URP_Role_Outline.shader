Shader "URP/Role/Outline"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("Base Tex", 2D) = "white" {}
        _Color0("Main Color", Color) = (1,1,1,1)
        _ColorOutline("Outline Color", Color) = (0,0,0,0.003)
        [HideInInspector]_MatEffectOutlineParam("Outline Color", Color) = (0,0,0,0)
        _Color("Effect Color", Color) = (1,1,1,1)

        [HideInInspector] _SrcBlend("__src", FLOAT) = 1.0
        [HideInInspector] _DstBlend("__dst", FLOAT) = 0.0
        [HideInInspector] _ZWrite("__zw", FLOAT) = 1.0
        [HideInInspector] _Stencil ("Stencil ID", Float) = 5
    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "../API/CommonAPI.hlsl"
    #define UNIFORM_PCH_OFF
    #define ROLE_SRP_BATCH

    CBUFFER_START(UnityPerMaterial)
    FLOAT4 _Color0;
    FLOAT4 _ColorOutline;
    FLOAT4 _MatEffectOutlineParam;
    FLOAT4 _Color;
    CBUFFER_END

    #define _MainColor _Color0
    sampler2D _MainTex;
    //feature
    ENDHLSL

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "Outline"
            Tags
            {
                "LightMode" = "Outline" "RenderType" = "Transparent" "Queue" = "Transparent"
            }
            Cull Front
            Blend SrcAlpha OneMinusSrcAlpha
            Offset 1,[_OutlineZOffset]

            HLSLPROGRAM
            #pragma target 3.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #pragma multi_compile_local _ _DITHER_TRANSPARENCY

            #pragma vertex vert
            #pragma fragment frag

            struct a2v
            {
                FLOAT3 TangentX : NORMAL;
                FLOAT4 TangentZ : TANGENT;
                FLOAT4 positionOS : POSITION;
                FLOAT4 uv : TEXCOORD0;
                FLOAT4 Color : COLOR;
            };

            struct v2f
            {
                FLOAT4 positionCS : SV_POSITION;
                FLOAT3 customData : TEXCOORD1;
            };

            FLOAT _IsUIScene;
            FLOAT4 _RenderScaledScreenParams;
            #ifdef _BASE_FROM_COLOR
                #define _OutlineColor _Color.xyz
                #define _OutlineWidth _Color.w
            #else
            #define _OutlineColor _ColorOutline.xyz
            #define _OutlineWidth _ColorOutline.w
            #endif

            #define _MatEffectOutlineColor (_MatEffectOutlineParam.xyz)
            #define _MatEffectOutlineBlend (_MatEffectOutlineParam.w)

            FLOAT3 Rot(in FLOAT4 rotation, FLOAT3 vec)
            {
                FLOAT x = rotation.x * 2;
                FLOAT y = rotation.y * 2;
                FLOAT z = rotation.z * 2;
                FLOAT xx = rotation.x * x;
                FLOAT yy = rotation.y * y;
                FLOAT zz = rotation.z * z;
                FLOAT xy = rotation.x * y;
                FLOAT xz = rotation.x * z;
                FLOAT yz = rotation.y * z;
                FLOAT wx = rotation.w * x;
                FLOAT wy = rotation.w * y;
                FLOAT wz = rotation.w * z;

                FLOAT3 res;
                res.x = (1 - (yy + zz)) * vec.x + (xy - wz) * vec.y + (xz + wy) * vec.z;
                res.y = (xy + wz) * vec.x + (1 - (xx + zz)) * vec.y + (yz - wx) * vec.z;
                res.z = (xz - wy) * vec.x + (yz + wx) * vec.y + (1 - (xx + yy)) * vec.z;
                return res;
            }

            v2f vert(in a2v Input)
            {
                // 计算Output
                FLOAT3 positionWS = TransformObjectToWorld(Input.positionOS);
                FLOAT4 projPos = TransformWorldToHClip(positionWS);

                // 从Input取值
                FLOAT3 normal = normalize(Input.TangentX);
                FLOAT4 tangent = Input.TangentZ;
                FLOAT3 binormal = cross(normal, normalize(tangent.xyz)) * tangent.w;
                FLOAT3 inputOutlineVectorTS = Input.Color.xyz;
                // TODO: 和顶点输入值不太一致，所以需要*3.695，得查一下是为什么。
                FLOAT inputOutlineLength = min(1, length(inputOutlineVectorTS) * 3.695);

                // 解决0向量导致的描边突然断掉的问题
                FLOAT3 outlineVectorTS = inputOutlineVectorTS + normal * 1e-4;

                // 空间转换
                float3x3 tangentToObjectMatrix = float3x3(normalize(tangent.xyz), binormal, normalize(normal));
                FLOAT3 outlineVectorOS = mul(outlineVectorTS, tangentToObjectMatrix);
                // TODO: Instancing旋转
                // outlineVectorOS = normalize(Rot(instanceRot, outlineVectorOS));
                #ifdef _GPU_ANIMATION
	                 float3x3 objectToWorldMatrix = custom_ObjectToWorld;
                #else
                    float3x3 objectToWorldMatrix = GetObjectToWorldMatrix();
                #endif
                FLOAT3 outlineNormalWS = mul(objectToWorldMatrix, outlineVectorOS);
                FLOAT3 outlineNormalVS = mul((float3x3)GetWorldToViewMatrix(), outlineNormalWS);
                FLOAT2 outlineNormalCS = mul((float2x2)GetViewToHClipMatrix(), outlineNormalVS.xy);

                // 1像素宽度的描边方向
                FLOAT2 outlineDirCS = normalize(outlineNormalCS.xy);
                FLOAT2 pixelSize = 2.0 / _RenderScaledScreenParams.xy * projPos.ww;
                // 单位为米的深度值
                FLOAT positionZVS = projPos.z / projPos.w;
                //FLOAT eyeDepth = LinearEyeDepth(projPos.z / projPos.w);
                // eyedepth [0, +∞] => distance scale [2, 1]
                FLOAT distanceWidthScale = 1 / (0.1 * positionZVS + 0.4) - 0.4;
                FLOAT paramScale = _OutlineWidth / 0.4;
                #if USE_PIXEL_SIZE
	                 FLOAT2 pixelPerfect = outlineDirCS * pixelSize;
	                 FLOAT2 pixelPerfect1To2 = pixelPerfect * inputOutlineLength * distanceWidthScale * paramScale;
                #else
                FLOAT2 constantScale = FLOAT2(_RenderScaledScreenParams.y / _RenderScaledScreenParams.x, 1) * projPos.ww
                    * 0.0015;
                FLOAT2 pixelPerfect1To2 = outlineDirCS * constantScale * inputOutlineLength * distanceWidthScale *
                    paramScale;
                #endif

                FLOAT2 absDir = abs(pixelPerfect1To2);
                FLOAT2 dirSign = sign(pixelPerfect1To2);
                FLOAT2 clamped = step(pixelSize, absDir);
                FLOAT2 finalDir = lerp(dirSign * pixelSize, pixelPerfect1To2, clamped);
                FLOAT subPixelAlpha = saturate(
                    length(pixelPerfect1To2 * FLOAT2(1.0, _RenderScaledScreenParams.y / _RenderScaledScreenParams.x)) /
                    pixelSize.x);

                projPos.xy += finalDir;

                // // 解决部分内凹面描边穿插出奇怪的线条的问题
                // // TODO: 在OpenGL下会有严重的闪烁，加了PRECESION_FIXER后没有任何改善，估计是变换矩阵或者length计算精度不足。
                // // 		 现在回退到旧版本，使用Offset来控制偏移。
                // const FLOAT PRECESION_FIXER = 100.0;
                // FLOAT4 outlineWS = mul(unity_MatrixInvVP, projPos);
                // FLOAT len = length((positionWS.xyz - outlineWS.xyz) * PRECESION_FIXER) * 2 / PRECESION_FIXER;
                // outlineWS.xyz -= normalize((_WorldSpaceCameraPos.xyz - outlineWS.xyz) * PRECESION_FIXER) * len;
                // projPos = mul(unity_MatrixVP, outlineWS);

                v2f o;
                o.customData = subPixelAlpha;
                o.customData.yz = Input.uv;
                o.positionCS = projPos;

                return o;
            }

            #define _CUSTOM_PS


            FLOAT4 frag(in v2f Interpolants) : SV_Target
            {
                #ifdef _DITHER_TRANSPARENT
	                clip(-1);
                #endif


                FLOAT alpha = 1;

                // pixel size alpha scale: while outline width thinner than 1 pixel, use alpha to optimize it.
                alpha *= Interpolants.customData * Interpolants.customData;

                // fov scale:
                // _matrixP[1][1] is cot(FOV/2)
                // while fov=55, _matrixVP[1][1]=-1.9(Metal/D3D11/Vulkan) or = 1.9(OpenGL/ES)
                #if UNITY_UV_STARTS_AT_TOP
                #define DEFAULT_FOV_SCALE -1.9
                #else
	                #define DEFAULT_FOV_SCALE +1.9
                #endif
                alpha *= GetViewToHClipMatrix()[1][1] * DEFAULT_FOV_SCALE;

                // disable transparency while in UIScene.
                alpha = max(alpha, _IsUIScene);

                // resolve problem of blending.
                alpha = saturate(alpha);

                // 计算颜色
                FLOAT4 BaseColor = tex2D(_MainTex, Interpolants.customData.yz);
                #ifdef _BASE_FROM_COLOR
	                BaseColor.rgb = FLOAT3(1, 1, 1);
	                clip(BaseColor.a - 0.5);
                #else
                BaseColor.rgb *= _MainColor.rgb;
                #endif
                FLOAT3 col = BaseColor.rgb * _OutlineColor.rgb;
                col = lerp(col, _MatEffectOutlineColor, _MatEffectOutlineBlend);

                return FLOAT4(col, alpha);
            }
            ENDHLSL
        }
    }
    CustomEditor "CFEngine.Editor.PBSShaderGUI"
}
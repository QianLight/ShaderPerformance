Shader "URP/DepthDecal"
{
    Properties
    {
        [HDR]_Color("Color(RGB)", Color) = (0, 0, 0, 0)
        _MainTex("MainTex",2D) = "white"{}
        _Angle("Angle",Range(0,1))=1
        _ColorInt("ColorInt",float)=1
    }
    SubShader
    {
        Tags
        {
//1
            //告诉shader，渲染管线是通用渲染管线
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        
        Pass
        {
            Name "Pass"
            Tags 
            { 
                // LightMode: <None>
            }
            Blend SrcAlpha OneMinusSrcAlpha

            //Cull Back
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        
            // Pragmas
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            // #pragma multi_compile_fog
            #pragma multi_compile_instancing

            //#pragma multi_compile_particles
//2
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"

            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            float4 _MainTex_ST;
            half _Angle,_ColorInt;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex); 

            TEXTURE2D(_CameraDepthTexture);SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D(_CameraDepthRT);SAMPLER(sampler_CameraDepthRT);
        
            //顶点着色器输入
            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 uv         :TEXCOORD0;   //Center Texcoord0.zw Texcoord1.x  粒子系统世界坐标
                float4 post       :TEXCOORD1;   //Size Texcoord1.yzw  粒子系统大小
                float4 Custom1    :TEXCOORD2;   //Custom1 Texcooed2.x  自定义参数：扩散控制
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float4 vertColor  : COLOR;
            };
        
            // 顶点着色器输出
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv:TEXCOORD;
                float3 positionVS : TEXCOORD1;
                float4 vertColor : TEXCOORD2;
                float4 post : TEXCOORD3;
                float3 scale : TEXCOORD4;
                
            };
            
            Varyings vert(Attributes v)
            {
                Varyings o=(Varyings)0;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv=TRANSFORM_TEX(v.uv.xy,_MainTex);
                o.positionVS = TransformWorldToView(TransformObjectToWorld(v.positionOS));
                o.vertColor = v.vertColor;
                o.post.xyz = float3(v.uv.zw,v.post.x);
                o.post.w = v.Custom1.x;
                o.scale = v.post.yzw;
                return o;
            }
            
            float2 Polar(float2 UV)
            {
                //0~1的1象限转-0.5~0.5的四象限
                float2 uv = UV-0.5;
                //d为各个象限坐标到0点距离,数值为0~0.5
                float distance=length(uv);
                //0~0.5放大到0~1
                distance *=2;
                //4象限坐标求弧度范围是 [-pi,+pi]
                float angle=atan2(uv.x,uv.y);
                //把 [-pi,+pi]转换为0~1
                float angle01=angle/3.14159/2+0.5;
                //输出角度与距离
                return float2(angle01,distance);
            }
            
            half4 frag(Varyings i):SV_TARGET
            {
                half4 c=1;
                float2 screenUV = i.positionCS.xy/_ScreenParams.xy;
                half depthMap = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture,screenUV);
                half depthZ = LinearEyeDepth(depthMap,_ZBufferParams);
                //return frac(depthZ);
                float4 depthVS = 1;
                depthVS.xy = i.positionVS.xy*depthZ/-i.positionVS.z;
                depthVS.z = depthZ;
                float3 depthWS = mul(unity_CameraToWorld,depthVS);
                //float3 depthOS = mul(unity_WorldToObject,float4(depthWS,1));
                float3 depthOS = TransformWorldToObject(depthWS);
                
                float2 uv0 = (depthWS.xz) * (1/i.scale.xy) + 0.5f;//_MainTex_ST.xy
                float2 uv = uv0 - i.post.xz * (1/i.scale.xy);//_MainTex_ST.xy
                
                float2 atan2UV = Polar(uv)+_MainTex_ST.zw*half2(1,i.post.w);//*_MainTex_ST.xy

                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,atan2UV);
                c = mainTex*_Color*i.vertColor;
                c.rgb *= _ColorInt;
                c.a*=step(atan2UV.x,_Angle);
                return c;
            }
        
            ENDHLSL
        }
    }
    //FallBack "Diffuse"
}
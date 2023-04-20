Shader "URP/SFX/BillboardImpostor"
{
    Properties
    {
        [NoScaleOffset]_Albedo("Impostor Albedo & Alpha", 2D) = "white" {}
        [NoScaleOffset]_Normals("Impostor Normal & Depth", 2D) = "white" {}
        [NoScaleOffset]_Mask("Mask", 2D) = "white" {}
        _Frames_Size_Clip("_Frames_Size_Clip",Vector)=(4,2.1,0.5,0)
        _SpecularColor ("SpecularColor", Color) = (1,1,1,1)
        _SpecularValue ("SpecularValue", Range(0,1)) = 1.0
        _Speed("Speed",range(1,100))=10
        _Num("_Num",Range(1,4))=2


    }


    SubShader
    {

        Tags { "RenderType"="Opaque" "Queue"="Geometry" "DisableBatching"="True" }
        Cull Back
        ZWrite On

        Pass
        {
            
            Name "ForwardBase"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
             #pragma target 3.0
            #define URP_BASE
            #define REDEFINE_URP
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"
            #include "../StdLib.hlsl"	 
            #include "../Include/Common.hlsl"
            #include "../Scene/Scene_Head.hlsl"

            #pragma  shader_feature_local  _INSTANCE           
            #pragma vertex vert
            #pragma fragment frag
 
            uniform sampler2D _Albedo; // 颜色
            uniform sampler2D _Normals; // 法线
            uniform sampler2D _Mask;// mask

            CBUFFER_START(UnityPerMaterial)
            float4 _Frames_Size_Clip;
            // 漫反射强度
            uniform float4 _SpecularColor;
            uniform float _SpecularValue;
            float _Speed;
            float _Num;
            CBUFFER_END

              struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
      


            float2 VectortoHemiOctahedron( float3 N )
            {
	            N.xy /= dot( 1.0, abs(N) );
                float _x= N.z;
                float _y=N.x;
	            return float2( _x +_y, _x - _y );
            }

            float3 HemiOctahedronToVector( float2 Oct )
            {
	            Oct = float2( Oct.x + Oct.y, Oct.x - Oct.y ) *0.5;
	            float3 N = float3( Oct, 1 - dot( 1.0, abs(Oct) ) );
	            return normalize(N);
            }
            //SHUIJI
            float3 HSVToRGB( float3 c )
			{
				float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
				float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
				return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
			}
            IBUFFER_START(Bilboard)
                  REAL4 param;
                  REAL4x4 WTLmatrixs;
                  REAL4x4 LTWmatrixs;
            IBUFFER_END(BilboardArray,64)

            inline void OctaImpostorVertex( inout appdata v,uint id, inout float4 uvsFrame1,inout float4 worldpos)
            {
                float framesXY = _Frames_Size_Clip.x;
                float prevFrame = framesXY - 1;
                float2 fractions = 1.0 / float2( framesXY, prevFrame );
                float fractionsFrame = fractions.x;
                float fractionsPrevFrame = fractions.y;

                float3 worldOrigin =  BilboardArray[id].param.xyz;//float3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
                worldpos=float4(worldOrigin,1);
                float3 worldCameraPos = _WorldSpaceCameraPos;



                // 模型空间：origin到相机的方向
              //  float3 objectCameraDirection = normalize( mul( worldCameraPos - worldOrigin, (float3x3) BilboardArray[id].WTLmatrixs));
                 float3 objectCameraDirection = normalize( mul((float3x3) BilboardArray[id].WTLmatrixs,worldCameraPos - worldOrigin));
                // 模型空间：相机的位置

                float3 upVector = abs(objectCameraDirection.y) > 0.999 ? float3(0, 0, 1) : float3(0, 1, 0);           
                // 模型水平竖直向量
                float3 rightDir = normalize( cross( objectCameraDirection, upVector ) );
                float3  upDir = cross( rightDir, objectCameraDirection );
                float3 center=float3(0,0,0);        
                float3 centerOffs = v.vertex.xyz - center;
                float3 billboard = center + rightDir * centerOffs.x + upDir * centerOffs.y + objectCameraDirection * centerOffs.z;
                float2 cameraPos = VectortoHemiOctahedron(objectCameraDirection.xzy)*0.5 + 0.5;
                float colFrame = round(abs(cameraPos.x) * prevFrame);
                float rowFrame = round(abs(cameraPos.y) * prevFrame);
          
                uvsFrame1 = 0;
                uvsFrame1.xy = (v.uv.xy *fractionsFrame)/_Num+ float2(colFrame, rowFrame) * fractionsFrame;
                
                //xuliez
                float time = floor((_Time.y+worldOrigin.x+worldOrigin.z)*_Speed);
                float totalnum=_Num*_Num;                   
                float rowdouble=floor(time/totalnum);
                float Difference=time - totalnum * rowdouble;
                float row11=floor(Difference/_Num);              
                float column = Difference - row11 * _Num;
                float basecut=framesXY*_Num;
       
                uvsFrame1.xy=uvsFrame1.xy+float2(row11/basecut,column/basecut);
                // 顶点
               v.vertex.xyz = billboard;
            }

            struct v2f_surf {     
              	float4	Pos : POSITION;
                float4 UVsFrame : TEXCOORD5;
                float4 worldpostion : TEXCOORD6;
            };

            // 顶点
            v2f_surf vert (appdata v INSTANCE_INPUT) {     
                v2f_surf o;
                
               	#ifdef _INSTANCE             
                       OctaImpostorVertex( v,instanceID, o.UVsFrame ,o.worldpostion); ///
                       o.Pos= mul(_matrixVP, mul(BilboardArray[instanceID].LTWmatrixs,  FLOAT4(v.vertex.xyz, 1.0)));

                  #else
                       o.Pos = TransformObjectToHClip(v.vertex);                  
                  #endif
                return o;
            }

            // 片段着色
            float4 frag (v2f_surf IN INSTANCE_INPUT) : SV_Target {     

             	#ifdef _INSTANCE             
                float4 blendedAlbedo = tex2D( _Albedo, IN.UVsFrame.xy);
                float4 blendedNormal = tex2D( _Normals, float3( IN.UVsFrame.xy, 0) );
                float3 localNormal = blendedNormal.rgb * 2.0 - 1.0;
				float3 worldNormal = normalize( mul( (float3x3)BilboardArray[instanceID].LTWmatrixs, localNormal ) );
                float3 worldlight =normalize(_MainLightDir).xyz;
                float NdotL=max(0,(dot(worldNormal,worldlight)));
                float3 _Specular = _MainLightColor.rgb*_SpecularColor * _SpecularValue * NdotL;
                float alpha = blendedAlbedo.a - _Frames_Size_Clip.z;
                clip(alpha);
                float4 color=float4(blendedAlbedo)*(0.6+float4(_Specular,0));
                return color;
                #else
                    return 1;
                #endif
            }
            ENDHLSL
        }
		Pass
		{
			Name "OverdrawF"
			Tags{"LightMode" = "OverdrawForwardBase"}

			Blend One One
			HLSLPROGRAM

			#pragma only_renderers d3d11
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
    //Fallback "Transparent/Cutout/VertexLit" 
   
}
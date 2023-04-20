Shader "Custom/Tools/RayColor"
{
	Properties
	{
		//_MainTex ("Base Tex", 2D) = "white" {}
		_Color0("Main Color", Color) = (1,1,1,1)
		// _ParamOutline("", Vector) = (0.5,2.0,1.0,0)
		//_Ray("x:RayInt y:Rayscale zw", Vector) = (0,0,0,0.03)
		//_Rayscale("Rayscale", Color) = (0,0,0,0)

	}

	HLSLINCLUDE
	ENDHLSL

	SubShader
	{
		Tags { "PerformanceChecks" = "False"  "IgnoreProjector"="False""LightMode" = "ForwardBase" }
		LOD 100
		Pass
		{
			Name "XRay"
			Tags{ "LightMode" = "XRay"  }

			// Cull Front
			// Offset 1,1
			// Cull Front

		 	Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off  
            ZTest Greater 

			HLSLPROGRAM
				#pragma target 3.0
				// #define _NO_MRT
				#define _XRAY
				#include "../Include/PCH.hlsl"
                #include "../Include/MaterialTemplate.hlsl"
				#define _CUSTOM_INTERPOLANTS	
				

				FLOAT4 _Ray;
			    FLOAT4 _RayColor;
				#define _RayInt _Ray.x
				#define _Rayscale _Ray.y
				

				// FLOAT4 _OutlineScale;
				// #define _MinDist (_OutlineScale.x)
				// #define _MaxDist (_OutlineScale.y)
				// #define _MinScale (_OutlineScale.z)
				// #define _MaxScale (_OutlineScale.w)

				// FLOAT4 _ColorOutline2;
				// #define _ColorMask (_ColorOutline2.w)

				FInterpolantsVSToPS CustomInterpolantsVSToPS(in FVertexInput Input, in FLOAT4 WorldPosition, out FLOAT4 projPos,in REAL4 instanceRot)
				{
					DECLARE_OUTPUT(FInterpolantsVSToPS, Interpolants);
					SET_UV(Input);

					//FLOAT3 binormal = cross(normalize(Input.TangentX),normalize(Input.TangentZ.xyz))*Input.TangentZ.w;
					//FLOAT3x3 rot = FLOAT3x3(normalize(Input.TangentZ.xyz),binormal,normalize(Input.TangentX));
					//FLOAT3 SmoothNor = mul(Input.Color.xyz,rot);
				

					 FLOAT3 N = Interpolants.NormalWS.xyz;
					 FLOAT3 V = _WorldSpaceCameraPos.xyz - WorldPosition.xyz;
					 FLOAT NDV = dot(N,V);
					// FLOAT cameraHorizontalDistanceLerp = saturate((cameraHorizontalDistance - _MinDist) / (_MaxDist -_MinDist));
					// FLOAT outlineScale = lerp(_MinScale, _MaxScale, cameraHorizontalDistanceLerp);




					FLOAT4 pos = mul(_matrixVP, FLOAT4(WorldPosition.xyz, 1.0));
					// FLOAT3 norm = mul((FLOAT3x3)unity_MatrixITMV,SmoothNor);
					// FLOAT2 extendDir = normalize(mul((FLOAT2x2)UNITY_MATRIX_P, norm.xy));

					// FLOAT width = _OutlineWidth*0.1*outlineScale*(_ColorMask+1);
					// pos.xy += extendDir*width*Input.Color.a;
					
					projPos = pos;

					SET_VS_DEPTH(Interpolants,projPos.zw);

					//Interpolants.NDV = NDV;
					// Interpolants.Color = Input.Color;
					return Interpolants;
				}


				#define _CUSTOM_PS
	
				
				FLOAT4 CustomPS(in FInterpolantsVSToPS Interpolants, in FLOAT4 SvPosition,inout FLOAT4 rt1)
				{

					// FLOAT4 BaseColor = SAMPLE_TEX2D(_MainTex,Interpolants.TexCoords[0].xy);
					// FLOAT3 col = BaseColor.rgb * _MainColor.rgb * _ColorOutline.rgb;
					// col = lerp(col,_ColorOutline2.rgb,saturate(_ColorMask));
					// rt1.xy = EncodeFloatRG(Interpolants.Depth01);
					// rt1.z = (_ColorOutline2.r+_ColorOutline2.b+_ColorOutline2.a);
					
                    //    FFragData FragData=GetFragData(Interpolants,SvPosition);
                    //    FMaterialData MaterialData=GetMaterialData(FragData);

			         FLOAT Rim =  pow(1-Interpolants.NDV,1)*0.5;
					//  rt1=Rim;

					// return FLOAT4(Interpolants.Color.www,0.8);
					return Rim;
				}


				#include "../Include/Vertex.hlsl"
				#include "../Include/Pixel.hlsl"
         
			
				#pragma vertex vertForwardBase
				#pragma fragment fragForwardBase
			ENDHLSL
		}
	}
	CustomEditor "CFEngine.Editor.PBSShaderGUI"
}

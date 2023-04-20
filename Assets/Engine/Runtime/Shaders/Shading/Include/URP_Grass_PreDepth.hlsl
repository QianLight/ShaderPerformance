#ifndef URP_GRASS_FORWARD_PASS_INCLUDED
#define URP_GRASS_FORWARD_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "../NBR/Include/Fog.hlsl"
#include "URP_GrassInput.hlsl"
#include "OPPInput.hlsl"
#include "../Scene/Wind_Effect.hlsl"
#include "SmartShadow.hlsl"

	//	#define UNIFORM_PCH_OFF


struct Attributes
{
    float4 positionOS    : POSITION;
    float2 texcoord      : TEXCOORD0;
    float2 lightmapUV    : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv                       : TEXCOORD0;
    float4 positionCS               : SV_POSITION;

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

	//	#define _RandomColorTex _MainTex1
	//	#define _USE_RANDOM_COLOR _Param2.w>0.5
		
	float hash12(float2 p)
	{
        float3 p3  = frac(float3(p.xyx) * .1031);
		p3 += dot(p3, p3.yzx + 33.33);
		return frac((p3.x + p3.y) * p3.z);
	}      
				
		half4 CalcCustomBaseColor(float2 uv)
	{					
		half4 color = Sample2D(uv, TEXTURE2D_ARGS(_MainTex, sampler_MainTex));
		//UNITY_BRANCH
		//if(_USE_RANDOM_COLOR)
		//{
		//	half3 Pivot = mul(unity_ObjectToWorld , half4(0,0,0,1)).xyz;
		//	half2 randomUV = half2( hash12(Pivot.xz*100),uv.y);
		////	half3 randomColor = SAMPLE_TEX2D(_RandomColorTex, randomUV).xyz;
//                   half3 randomColor = Sample2D(randomUV, TEXTURE2D_ARGS(_RandomColorTex, sampler_RandomColorTex)).xyz;
		//	color.xyz = randomColor;
		//}							
		//MaterialData.BaseColor = color*_MainColor;	
        //MaterialData.DyeColor = MaterialData.BaseColor.rgb;
        return color;	
	}


///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Vertex  functions    

        float3 CustomWindEffect(Attributes Input, float3 WorldPosition)
		{
            float3 offset=0;
			UNITY_BRANCH
			if(_AmbientWindSpeed>0.01)
			{     
                float hweight = saturate(Input.lightmapUV.y) * 0.01;
                float2 WaveUV = (WorldPosition.xz + _TimeParameters.x * _AmbientWindSpeed * float2(_AmbientWindDirx, _AmbientWindDirz)) * 0.01;
                float WaveTex = SAMPLE_TEX2D_LOD(_AmbientWind, WaveUV, 0).r;
                float Wavebig = WaveTex * WaveTex * hweight * _GustingStrength;
                float Wavesmall = sin(_GustingFrequency * _TimeParameters.x) * hweight;
				offset.xz = Wavebig + Wavesmall;        
			}
            UNITY_BRANCH
            if(_GRASS_COLLISION){
                #ifdef _ISFLOWER_OFF
                    float2 UV = saturate((WorldPosition.xyz - _CollisionCenter.xyz).xz/_CollisionRadiusInv + float2(0.5,0.5));
                    float4 CollisionOffset = SAMPLE_TEX2D_LOD(_CollisionTex, UV,0);
                    float3 dirctions=(float3(CollisionOffset.x,0,CollisionOffset.y)*2- float3(1,0,1))*CollisionOffset.z*_PushValue;
                    offset += dirctions;
                    offset *= Input.texcoord.y;
                #endif
                }
			return offset;
		}
half4 GrassEffecttest(Attributes Input)				
	{
		half4 waveWind1 = half4(Input.positionOS.xyz,1);
			#ifdef _ISFLOWER_OFF				
				half4 center = mul(unity_ObjectToWorld, half4(0,0,0,1));
				half2 UV = saturate((center.xyz - _CollisionCenter.xyz).xz/_CollisionRadiusInv + half2(0.5,0.5));
				half4 tmp_wind = SAMPLE_TEX2D_LOD(_CollisionTex, UV,0);					    
				half tmp_height = saturate(Input.lightmapUV.y);
				half3 calwind=half3(tmp_wind.x, 0, tmp_wind.y) * 2 - half3(1,0,1);       
				half3 windDirection =normalize(cross(calwind, half3(0, -1, 0)));
				half xddee= tmp_wind.z*tmp_wind.z*_PushAngle*tmp_height*any(windDirection);
				half3x3 tmp_rotation = RotationMatrix(windDirection, radians(xddee));
				half3 waveWind = mul(tmp_rotation, Input.positionOS.xyz);
				waveWind1=half4(waveWind,1);

			#endif
		//}
		return waveWind1;
	}

#if _INSTANCING_HIGH

//struct PS��
//float3 position;
//float3 scale;
//��;
//RWStructuredBuffer<PS> inss;
StructuredBuffer<float4x4> matrix4x4Buffer;
Varyings LitPassVertex(Attributes input, uint instanceID : SV_InstanceID)
#else
Varyings LitPassVertex(Attributes input)
#endif
{
    Varyings output = (Varyings)0;
#if _INSTANCING_HIGH
    float4x4 materix = matrix4x4Buffer[instanceID];
	
    float3 postionws = mul(materix, input.positionOS);
#else
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
	
    float3 postionws = TransformObjectToWorld(input.positionOS.xyz);
#endif
    UNITY_BRANCH
    if(_GRASS_COLLISION)
	{
	      input.positionOS = GrassEffecttest(input);		  
	 }
    float3 xyzmove = CustomWindEffect(input, postionws);
    postionws.xyz += xyzmove;
    output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
	output.positionCS =  TransformWorldToHClip(postionws.xyz);
    return output;
}

//Fragment functions 

half4 LitPassFragment(Varyings input,half facing : VFACE) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float2 uv = input.uv;
    half4 diffuseAlpha = CalcCustomBaseColor(input.uv);

    half alpha = diffuseAlpha.a * _Color0.a;
	
	#ifdef _ALPHATEST_ON
    AlphaDiscard(alpha, _Cutoff);
    #endif

    return 0;

}


#endif

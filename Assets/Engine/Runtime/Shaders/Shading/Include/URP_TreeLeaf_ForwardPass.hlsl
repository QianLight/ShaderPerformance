#ifndef URP_TREELEAF_FORWARD_PASS_INCLUDED
#define URP_TREELEAF_FORWARD_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "../NBR/Include/Fog.hlsl"
#include "OPPCore.hlsl"
#include "URP_TreeLeafInput.hlsl"
#include "SmartShadow.hlsl"
//#include "../Scene/Tree_Effect.hlsl"
	//	#define UNIFORM_PCH_OFF


struct Attributes
{
    float4 positionOS    : POSITION;
    float3 normalOS      : NORMAL;
    float4 tangentOS     : TANGENT;
    float2 texcoord      : TEXCOORD0;
    float2 lightmapUV    : TEXCOORD1;
    float4 Color         :COLOR0; 
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv                       : TEXCOORD0;
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

    float3 posWS                    : TEXCOORD2;    // xyz: posWS

#ifdef _NORMALMAP
    float4 normal                   : TEXCOORD3;    // xyz: normal, w: viewDir.x
    float4 tangent                  : TEXCOORD4;    // xyz: tangent, w: viewDir.y
    float4 bitangent                : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
#else
    float3  normal                  : TEXCOORD3;
    float3 viewDir                  : TEXCOORD4;
#endif

    half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD7;
#endif

    float4 positionCS               : SV_POSITION;
    float4 emission                 : COLOR0;
    float4 treecenterpos             :COLOR1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

	//	#define _RandomColorTex _MainTex1
	//	#define _USE_RANDOM_COLOR _Param2.w>0.5
		
	half hash12(half2 p)
	{
		half3 p3  = frac(half3(p.xyx) * .1031);
		p3 += dot(p3, p3.yzx + 33.33);
		return frac((p3.x + p3.y) * p3.z);
	}      
				
	half4 CalcCustomBaseColor(half2 uv)
	{					
		half4 color = Sample2D(uv, TEXTURE2D_ARGS(_MainTex, sampler_MainTex));
        return color;	
	}
    
    //inline void TreeBaseColor(in FFragData FragData, inout FMaterialData MaterialData)
    //{
    //    FLOAT2 uv = GET_FRAG_UV;
    //    FLOAT4 color = SAMPLE_TEX2D(_MainTex, uv);
    //    MaterialData.BaseColor = color * _MainColor;
    //    MaterialData.DyeColor = MaterialData.BaseColor.rgb;
    //}
    half3 CustomWindEffect(Attributes Input,half3 WorldPosition)
{
    half3 offset = 0;
    UNITY_BRANCH
    if (_AmbientTreeWindSpeed > 0.01)
    {
        half2 WindUV = (WorldPosition.xz * _AmbientWindFrequency * 0.01) + (_Time.y * _AmbientTreeWindSpeed * 0.1);
        half WindTex = SAMPLE_TEX2D_LOD(_AmbientWind, WindUV, 0).r;

    //    #ifdef _DETIAL_TREE_MOVE
        WindTex = WindTex * 2 - 1;
        half spaceOffset = -WorldPosition.x * _AmbientWindDir.x - WorldPosition.z * _AmbientWindDir.z;	 
        half weightedTime = fmod(_Frenquency * (spaceOffset * _ModelScaleCorrection + _Time.y), PI);
        half sway = max(sin(weightedTime + 0.5 * sin(weightedTime)), sin(weightedTime + PI + sin(weightedTime + PI))) - 0.9;
        half yoffset = abs(cos(sway)) * _AmbientWindDir.y;
        half horizontalOffset = sin(sway);
        half xoffset = horizontalOffset * _AmbientWindDir.x;
        half zoffset = horizontalOffset * _AmbientWindDir.z;
        //使用顶点色计算定点的偏移系数 effect ratio
        offset += REAL3(xoffset, yoffset , zoffset) *Input.lightmapUV.y * _ModelScaleCorrection * _Magnitude; 
       Input.Color.x = lerp(Input.Color.x,1,_MaskRange);
       offset *= Input.Color.x; 
     //   #endif//_DETIAL_TREE_MOVE       

        _OffsetCorrection.xz += sin(_OffsetCorrection.xz * WindTex);
        half3 blend = pow(abs(Input.lightmapUV.y),_Blend) + _OffsetCorrection * Input.lightmapUV.y;
        offset += blend;
        half Stable = lerp(Input.lightmapUV.y, 0, _StableRange);
        offset *= Stable;
    }
    return offset;
}
 
///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Vertex  functions    
#ifdef _CUSTOM_VERTEX_PARAM
void CustomVertex(in Attributes Input,inout Varyings output)
{
//	#define _TreeCenterPos _Param0.xyz

	#define _AmbientSky _Color1.xyz
	#define _AmbientEquator _Color2.xyz
	#define _AmbientGround _Color3.xyz

	output.treecenterpos.xyz = mul(unity_ObjectToWorld, half4(_Treecenter.xyz, 1)).xyz;

	//half3 leaveNormal = normalize(output.posWS.xyz - output.treecenterpos.xyz);
	//const FLOAT3 worldUp = half3(0,1,0);
	//half ndotUp = dot(leaveNormal, worldUp);
	//half GroundToSky =  ndotUp * 0.5 + 0.5;
	//half3 baseLerp = lerp(_AmbientGround, _AmbientSky, GroundToSky) ;
	//half squareNdotUp = ndotUp * ndotUp;
	//half equatorIntensity = lerp(0.3, 0.55, squareNdotUp);
	//output.emission.xyz = lerp(_AmbientEquator, baseLerp, equatorIntensity);
}
    #endif

        //    #define CustomLighting CalcTreeLighting
        //    #define _CUSTOM_LIGHT


void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData.positionWS = input.posWS;

#ifdef _NORMALMAP
    half3 viewDirWS = half3(input.normal.w, input.tangent.w, input.bitangent.w);
    inputData.normalWS = TransformTangentToWorld(normalTS,
        half3x3(input.tangent.xyz, input.bitangent.xyz, input.normal.xyz));
#else
    half3 viewDirWS = input.viewDir;
    inputData.normalWS = input.normal;
#endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    viewDirWS = SafeNormalize(viewDirWS);

    inputData.viewDirectionWS = viewDirWS;
#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    inputData.shadowCoord = input.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif

    inputData.fogCoord = input.fogFactorAndVertexLight.x;
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
    inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
 //   inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);

    #if defined(_SMARTSOFTSHADOW_ON)
    inputData.shadowMask = GetSmartShadow(_MainLightPosition.xyz, inputData.normalWS, float4(inputData.positionWS, 1), _SmartShadowIntensity);
#else
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
#endif
}



Varyings LitPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    float3 postionws = TransformObjectToWorld(input.positionOS);

    postionws.xyz += CustomWindEffect(input, postionws);

    input.positionOS.xyz = TransformWorldToObject(postionws).xyz;

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

    output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
    output.posWS.xyz = vertexInput.positionWS;
    output.positionCS = vertexInput.positionCS;
   // normalInput.normalWS=half3(0,1,0);
#ifdef _NORMALMAP
    output.normal = half4(normalInput.normalWS, viewDirWS.x);
    output.tangent = half4(normalInput.tangentWS, viewDirWS.y);
    output.bitangent = half4(normalInput.bitangentWS, viewDirWS.z);
#else
    output.normal = NormalizeNormalPerVertex(normalInput.normalWS);
    output.viewDir = viewDirWS;
#endif

    OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
    OUTPUT_SH(output.normal.xyz, output.vertexSH);

    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif
    //  output.bottomuv.z=saturate(input.lightmapUV.y);
//    #ifdef _CUSTOM_VERTEX_PARAM
    CustomVertex(input,output);
  //  #endif
    return output;
}

//Fragment functions 

half4 LitPassFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float2 uv = input.uv;
    half4 diffuseAlpha = CalcCustomBaseColor(uv);
    half3 diffuse = diffuseAlpha.rgb;

    half alpha = diffuseAlpha.a * _Color0.a;
    AlphaDiscard(alpha, _Cutoff);

    #ifdef _ALPHAPREMULTIPLY_ON
        diffuse *= alpha;
    #endif
    
    half3 normalTS = GetDefaultNormal();
    half3 emission= input.emission.xyz;
    half3 treecenterpos = input.treecenterpos.xyz;
    half smoothness =0;

    InputData inputData;
    InitializeInputData(input, normalTS, inputData);

      half4 color = TreeLeafBlinnPhong(inputData,diffuse, treecenterpos, emission, alpha,_Color0);
 //    half4 buttomcolor = Sample2D(input.bottomuv.xy, TEXTURE2D_ARGS(_CustomLightmap, sampler_CustomLightmap));
     // color=lerp(buttomcolor,color,result);
  //  color.rgb = MixFog(color.rgb, inputData.fogCoord);
   // color.a = OutputAlpha(color.a, _Surface);
	 APPLY_FOG(color.rgb, input.posWS.xyz);
 //  return color;
   // half  result=min(input.bottomuv.z+_BottomPersent,1);

    SphereDitherTransparent(input.positionCS, _DitherTransparency);

    return half4(color);

}


#endif

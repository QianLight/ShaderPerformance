#ifndef URP_GRASSINPUT_INCLUDED
#define URP_GRASSINPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

CBUFFER_START(UnityPerMaterial)

            float4 _MainTex_ST;
            float4 _AmbientWindDir;
            float4 _OffsetCorrection;
            float4 _Color0;
            float4 _Color1;
            float4 _Color2;
            float4 _Color3;
        //    float _AmbientWindDirz;
            float4 _Treecenter;
            //float _TreeCenterPosBias;
			float _Cutoff;
            float _TreeRimPower;
            float _TreeRimIntensity;
            float _TreeFresnelPower;
            float _TreeFresnelIntensity;
            float _DackIntensity;
         //   float _DackRange;
            float _InverseLightIntensity;
            float _TargetLightIntensity;
            float _LightFlatness;
            float _SubSurfaceGain;
            float _ShadowOffset;
            float _FackLight;
            float _AmbientTreeWindSpeed;
            float _AmbientWindFrequency;
            float _Magnitude;
            float _Frenquency;
            float _ModelScaleCorrection;
            float _MaskRange;
            float _Blend;
            float _StableRange;
            float _InsideDack;
            float _InsideDackpow;
            float _ScaleInsideDack;
            float _DitherTransparency;
      //      float4 _Color4;
        //    float4 _Color5;
       //     float4 _PBRParam;
         //   float _DebugMode;
CBUFFER_END

TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);
//TEXTURE2D(_ProcedureTex3);            SAMPLER(sampler_ProcedureTex3);
//TEXTURE2D(_ProcedureTex4);            SAMPLER(sampler_ProcedureTex4);
TEXTURE2D(_AmbientWind);            SAMPLER(sampler_AmbientWind);
#define _MainColor _Color0



    half3 CalcTreeLighting(Light light,InputData inputData,half3 treecenterpos,half3 emission,half4 _Color0,  inout half3 DirectSpecular)
{

	half3 TreeCenter =treecenterpos;
	half3 ambient = emission;


    // NdotL
    half3 leaveNormal = normalize(inputData.positionWS.xyz -TreeCenter);
    _ScaleInsideDack=_ScaleInsideDack*10;
    half xdis=(inputData.positionWS.x-TreeCenter.x)*(inputData.positionWS.x-TreeCenter.x);
    half ydis=(inputData.positionWS.y-TreeCenter.y)*(inputData.positionWS.y-TreeCenter.y);
    half zdis=(inputData.positionWS.z-TreeCenter.z)*(inputData.positionWS.z-TreeCenter.z)*_ScaleInsideDack;
    half rsqrtvalue=saturate(rsqrt(xdis+ydis+zdis));
    rsqrtvalue=1-saturate(pow(rsqrtvalue*_InsideDack,_InsideDackpow));
    rsqrtvalue=saturate(rsqrtvalue+0.3);
    half NdotL = dot(inputData.normalWS, light.direction.xyz);
    half positiveNL = saturate(NdotL);
    half negativeNL = saturate(-NdotL+_ShadowOffset);
    negativeNL *= _InverseLightIntensity;
    half diffuseTerm = lerp(positiveNL, _TargetLightIntensity, _LightFlatness);

 //   positiveNL = smoothstep(0, _ShadowOffset, positiveNL);
    diffuseTerm = lerp(diffuseTerm * _DackIntensity, diffuseTerm, positiveNL);
    diffuseTerm = lerp(diffuseTerm * _FackLight, diffuseTerm,light.shadowAttenuation+rsqrtvalue);
    diffuseTerm*=rsqrtvalue;
    //  diffuseTerm=rsqrtvalue;
    // Ao
    //diffuseTerm = lerp(diffuseTerm*0.5,diffuseTerm, FragData.VertexColor.x);

    // subSurface
    //half lv = saturate(-dot(light.direction.xyz, inputData.viewDirectionWS));
    //half subSurfaceTerm = saturate(pow(lv, 2) *light.shadowAttenuation) * _SubSurfaceGain;

    // Rim
    //half3 adn = normalize(inputData.positionWS.xyz - TreeCenter);
    //half adNv = saturate(abs(dot(adn, inputData.viewDirectionWS) + _TreeCenterPosBias));
    //half rim = pow(1 - adNv, _TreeRimPower) * _TreeRimIntensity;
    //rim = lerp(0, rim, light.shadowAttenuation);

    // Fresnel
  //  half NdotV=saturate(dot(inputData.normalWS, inputData.viewDirectionWS));
    half NdotV=saturate(dot(inputData.normalWS, inputData.viewDirectionWS)+0.4);
    half fresnel = pow(1-NdotV, _TreeFresnelPower) * _TreeFresnelIntensity;
    fresnel *= light.shadowAttenuation;


    //float shadow = GetSmartShadow(_MainLightDir.xyz, MaterialData.WorldNormal, FragData.WorldPosition);

   //   half3   DirectDiffuse = diffuseTerm * light.color.xyz + ambient + negativeNL;
     half3   DirectDiffuse = diffuseTerm * light.color.xyz+negativeNL;

  //   DirectDiffuse *= pow(abs(_Color0), 0.4545) * kDieletricSpec.w;


   //  DirectSpecular = (rim + diffuseTerm + fresnel + subSurfaceTerm) * light.color.xyz * kDieletricSpec.x;
      DirectSpecular = (fresnel)* light.color.xyz* kDieletricSpec.x;



    return DirectDiffuse;
}

half4 TreeLeafBlinnPhong(InputData inputData, half3 diffuse, half3 Treecenterpos, half3 emission, half alpha,half4 _Color0)
{
    // To ensure backward compatibility we have to avoid using shadowMask input, as it is not present in older shaders
#if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
    half4 shadowMask = inputData.shadowMask;
#elif !defined (LIGHTMAP_ON)
    half4 shadowMask = unity_ProbesOcclusion;
#else
    half4 shadowMask = half4(1, 1, 1, 1);
#endif

    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask,inputData.normalWS.xyz);

    half3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);
 //     half3 specularColor = GrassLightingSpecular(attenuatedLightColor, mainLight.direction, inputData.normalWS, inputData.viewDirectionWS, specularGloss, smoothness,emission.x);
 //   half3 diffuseColor = inputData.bakedGI+ LightingLambert(attenuatedLightColor, mainLight.direction, inputData.normalWS);
     half3 specularColor=half3(0,0,0);
     half3 diffuseColor =CalcTreeLighting(mainLight,inputData,Treecenterpos,emission,_Color0,specularColor);
   //    half3 specularColor=half3(0,0,0);

  //  half3 finalColor = diffuseColor ;
	  half3 finalColor =diffuseColor* pow(abs(_Color0*diffuse), 0.4545) * kDieletricSpec.w;;
//#if defined(_SPECGLOSSMAP) || defined(_SPECULAR_COLOR)
   finalColor +=saturate(specularColor);
//#endif

    return half4(finalColor, alpha);
}




#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED

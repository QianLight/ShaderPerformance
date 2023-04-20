#ifndef URP_SCENE_WATER_PIXEL03_INCLUDE
#define URP_SCENE_WATER_PIXEL03_INCLUDE

inline float SG(float x, float k) {
                float a = k * 1.44269504f + 1.089234755f;
                return exp2(a * x - a);
}

inline float LinearEyeDepth(float z)
{
    return rcp(_ZBufferParams.z * z + _ZBufferParams.w);
}


float4 Frag(in Waterv2f03 IN) :SV_TARGET
{

	float3 worldNormal = normalize( IN.tSpace0.xyz );
	float3 WorldTangent = IN.tSpace1.xyz;
	float3 WorldBiTangent = IN.tSpace2.xyz;
	float3x3 worldToTangent = float3x3(WorldTangent,WorldBiTangent,worldNormal);
	float3 worldPosition = (float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w));
	float3 CameraVector = _WorldSpaceCameraPos.xyz  - worldPosition;

	float2 waterMaskUV = worldPosition.xz - _WaterMaskBox.xy;
	waterMaskUV += float2(0.5,0.5);
	waterMaskUV *= float2(1/_waterMaskBoxSize,1/_waterMaskBoxSize);
	float mask = tex2D(_WaterMask,waterMaskUV);
	float2 uv = IN.uv.xy*_UVTiling;

	//Noise
	/*
	float4 offsetColor = (tex2D(_WaterNormal, uv + float2(_Speed*_Time.x,0))+tex2D(_WaterNormal, float2(1-uv.y,uv.x) + float2(_Speed*_Time.x,0)) )/2;
	float2 offset = ((offsetColor).xy*2-1) * _Refrac;
	float4 bumpColor1 = tex2D(_WaterNormal, uv + offset + float2(_Speed*_Time.x,0));
	float4 bumpColor2 = tex2D(_WaterNormal, float2(1-uv.y,uv.x)+offset + float2(_Speed*_Time.x,0));
	*/
	
	float2 tempOffsset = float2(_Speed*_Time.x,0);
	float2 uv_tempOffset = uv + tempOffsset;
	float2 tempOffset2 = float2(1-uv.y,uv.x) + tempOffsset;
	
	float4 offsetColor = ( tex2D(_WaterNormal, uv_tempOffset) + tex2D(_WaterNormal, tempOffset2) )/2;
	float2 offset = ((offsetColor).xy*2-1) * _Refrac;
	float4 bumpColor1 = tex2D(_WaterNormal, offset + uv_tempOffset);
	float4 bumpColor2 = tex2D(_WaterNormal, offset + tempOffset2);
	float3 NormalLocal = float3(((bumpColor1+bumpColor2)/2).xy*2-1 , 1);
	NormalLocal.xy *=_NormalScale;
	float3 tangentWorldNormal =normalize(mul(NormalLocal,worldToTangent).xyz);

	//common 
	float3 CustomDir = normalize(_LightDri);
	float3 LightingDir = _WaterLightDir;//_WaterLightPosition// CustomDir;//(_SceneLightDir.xyz);//
	float3 ViewDir =  normalize(CameraVector);
	float3 H = normalize(LightingDir + ViewDir); 
	float NdotH = dot( tangentWorldNormal , H ); 
	float NdotL = dot( tangentWorldNormal , LightingDir)*0.5+0.5;
	float NdotV = dot(ViewDir, tangentWorldNormal);

	//specular
	float4 Specular =(SG(NdotH,_SpecularRange) *_SpecularInt)*_SpecularColor;
	float3 ReflectionDir = reflect(-ViewDir, tangentWorldNormal);
	float4 Reflect = texCUBE(_LocalEnvCube, ReflectionDir)*_reflectionInt;									

	// SoftParticle
	float2 screenPos = float2(IN.screenPos.xy / IN.screenPos.w);
				
	#if SHADER_API_GLES3
		float depthTex = tex2D(_CameraDepthTexture, screenPos.xy).x;		/* "_CameraDepthTexture" in OpenGL has Different Gamma Value than DX‘s */
		depthTex *= depthTex;													/* it should be Squared to fix it */
	#else
		float depthTex = tex2D(_CameraDepthTexture, screenPos.xy).x;     /* "_CameraDepthTexture" in DX */
	#endif
	#ifdef USE_DEPTH_TEXTURE	
		float sceneDepth = LinearEyeDepth(depthTex);
		float depth = LinearEyeDepth (IN.Depth01.x / IN.Depth01.y);
		float fade = saturate((sceneDepth - depth) / _Depth );
		
		float2 OffsetUV = ( ViewDir*sceneDepth ).xz;	
		float NoiseCaustics = tex2D(_WaveTex, OffsetUV*_CausticsTiling*1.3  - _Time.y*_CausticsSpeed).r;	
		float Caustics = tex2D( _WaveTex, OffsetUV*_CausticsTiling + _Time.y*_CausticsSpeed*0.85+ offset*_CausticsDisturbInt ).r + NoiseCaustics;	
		float3 CausticsCol = Caustics.xxx* _CausticsColor.xyz*_CausticsInt;
		CausticsCol = SG(CausticsCol,2.5) * saturate(1-length(worldPosition.xyz - _WorldSpaceCameraPos.xyz)*_CausticsRange);
         
	//FoamLine
		#if SHADER_API_GLES3
			float foam_Width_factor = 3;
		#else
			float foam_Width_factor = 2;
		#endif
		float foamLine = 1-saturate((sceneDepth - depth) * _ColorDepth *foam_Width_factor);
		float foamLineon = 1-saturate((sceneDepth - depth) * _ColorDepth*_FoamLineOnAlpha *foam_Width_factor);
		foamLine -=foamLineon;
		float2 waterColorUV = float2(sceneDepth, fade);
	#else

		mask *=_WaterDensity;
		float foamLine = mask;
		float fade = 1;
		float2 waterColorUV = IN.uv;
		float3 CausticsCol = 0;
	#endif
	float4 shadowCoord = TransformWorldToShadowCoord(worldPosition.xyz);
	float shadow = saturate( MainLightRealtimeShadow(shadowCoord));
	
	//float3 foamLineColor = tex2D(_WaveTex, worldPosition.xz*_FoamLineTiling +_Time.y*_FoamLineSpeed*0.5 +worldPosition.y* offset*_FoamLineDisturbInt).g*_FoamLineColor.xyz*foamLine;
	
	float3 foamLineColor = tex2D(_WaveTex, worldPosition.xz*_FoamLineTiling +_Time.y*_FoamLineSpeed*0.5 +worldPosition.y* offset*_FoamLineDisturbInt).g*_FoamLineColor.xyz *foamLine;

    float4 waterColor = tex2D(_MainTex, waterColorUV) *_MainColor * NdotL * _MainLightColor0*shadow  ;
	waterColor.rgb = waterColor.rgb;

	float3 Color = waterColor.rgb + Reflect.xyz + Specular+ foamLineColor*_FoamLineInt + CausticsCol;
	float alpha = _MainColor.a * fade;


	return float4(Color, alpha + foamLineColor.r*_FoamLineInt);         
}	

#endif 

// float _Cutout;
// half4 fragForwardPreDepth(in Waterv2f IN) : SV_Target
// {
// 	REAL2 uv = IN.Texcoord[0].xy;
// 	float4 color =  tex2D(_MainTex, uv);
// 	//clip(color.a - _Cutout);
// 	return 0;
// }
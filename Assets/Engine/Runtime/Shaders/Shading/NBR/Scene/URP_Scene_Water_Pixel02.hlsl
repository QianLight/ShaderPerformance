#ifndef URP_SCENE_WATER_PIXEL02_INCLUDE
#define URP_SCENE_WATER_PIXEL02_INCLUDE

#define FLOAT3 float3
#define FLOAT4 float4
inline float D_GGX2( float a,float a2, float NoH )
{
	float d = ( NoH * a2 - NoH ) * NoH + 1;	// 2 mad
	float ad = a/d;
	return INV_PI * ad*ad;
}

inline FLOAT Vis_SmithJointApprox( FLOAT a, FLOAT NoV, FLOAT NoL )
{
	// FLOAT a = Roughness;//Square( Roughness );
	// FLOAT Vis_SmithV = NoL * ( NoV * ( 1 - a ) + a );
	// FLOAT Vis_SmithL = NoV * ( NoL * ( 1 - a ) + a );
	// // Note: will generate NaNs with Roughness = 0.  MinRoughness is used to prevent this
	// return 0.5 * rcp( Vis_SmithV + Vis_SmithL + 1e-5f );

	FLOAT vis_SmithApprox = lerp(NoV*NoL,(NoV+NoL),a);
	return 0.5 * rcp(vis_SmithApprox + 1e-5f);

}

inline FLOAT Pow5(FLOAT x)
{
	FLOAT xx = x*x;
	return xx * xx * x;
}

inline FLOAT3 WaterF_Schlick( FLOAT3 SpecularColor, FLOAT VoH )
{
	FLOAT Fc = Pow5( 1 - VoH );					// 1 sub, 3 mul
	//return Fc + (1 - Fc) * SpecularColor;		// 1 add, 3 mad
	
	// Anything less than 2% is physically impossible and is instead considered to be shadowing
	return saturate( 50.0 * SpecularColor.g ) * Fc + (1 - Fc) * SpecularColor;
	
}

inline float DecodeFloatRGB(float3 enc)
{
    float3 kDecodeDot = float3(1.0, 1 / 255.0, 1 / 65025.0);
    return dot(enc, kDecodeDot);
}

float Linear01DepthPers(float z)
{
    z *= _ZBufferParams.x;// 1-far/near
    return rcp(z + _ZBufferParams.y);//far/near
}

float4 Frag(in Waterv2f02 IN) :SV_TARGET
{
	float3 worldNormal = normalize( IN.tSpace0.xyz );
	float3 WorldTangent = IN.tSpace1.xyz;
	float3 WorldBiTangent = IN.tSpace2.xyz;
	float3x3 worldToTangent = float3x3(WorldTangent,WorldBiTangent,worldNormal);
	float3 worldPosition = (float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w));
	float3 CameraVector = _WorldSpaceCameraPos.xyz  - worldPosition;
	

	//Noise
	float2 Distory = float2(_DistoryInt * _Time.y, 0);
	float2 samplerNoise = float2(tex2D(_WaterNormal, float2(IN.uv.x*_SecTexTilingX, IN.uv.y*_SecTexTilingY).xy + Distory).r*_Distory,0);
	float2 samplerUV = float2(_Time.y * _Speed , 0);

	//normal 
	float4 Normal =  tex2D(_WaterNormal, float2(IN.uv.x*_FistTexTilingX , IN.uv.y*_FistTexTilingY) +samplerUV*0.25 + samplerNoise);
	float2 Normaloff = Normal.xy*2-float2(1,1);
	float3 NormalLocal = float3(Normaloff,1);
	float3 tangentWorldNormal = normalize(mul(NormalLocal, worldToTangent).xyz);

	
	tangentWorldNormal = lerp( worldNormal , tangentWorldNormal , _Reflection );

	// return float4(tangentNormal,1);
	//common 
    float3 LightingDir = normalize(_MainLightDir0.xyz);
	float3 ViewDir = normalize(CameraVector);
	float3 H = normalize(LightingDir + ViewDir); 
	float NdotH = dot( tangentWorldNormal , H ); 
	float NdotL = dot( tangentWorldNormal , LightingDir);
	float NdotV = dot(ViewDir, tangentWorldNormal);
	float VdotH = dot(ViewDir, H);
	float FixNdotL = saturate(NdotL);


	//wave
	float VdN = saturate(pow(abs(NdotV), _Reflect));
	float distortNoise = tex2D(_WaterNormal,  float2(IN.uv.x*_SecTexTilingX,IN.uv.y*_SecTexTilingY) + samplerUV + samplerNoise).r;
	//float waveMask = saturate((VertexColor.g - distortNoise) * 30 );
	float waveMask = 1;
	float4 waveTex = tex2D(_WaveTex,float2(IN.uv2.x*_WaveTilingX,IN.uv2.y*_WaveTilingY)-samplerUV + samplerNoise).r *waveMask*_WaveColor*_WaveColor.a;
	
	//diffuse
	float4 	Col = tex2D(_MainTex, IN.uv + samplerNoise.x) * (2 - VdN) * _MainColor;
	//return float4(Col.xyz, 1);
	//BRDF Speculr 			
    float Roughness = _Roughness * Normal.z;
	float Metallic = _Metallic * Normal.w;
	float R2 = Roughness * Roughness;
	float3 Metallicolor  =  lerp( _WakeSpec , Col.xyz , Metallic );
	float D = D_GGX2(Roughness , R2 , NdotH );
	float Vis = Vis_SmithJointApprox( R2 , NdotV, FixNdotL );
	float3 F = WaterF_Schlick( Metallicolor, VdotH );			 
	float3 SpecularColor =  min( abs((D*Vis)*F) , 10 ) ;

	//refelect
	float3 ReflectionDir = reflect(-ViewDir, tangentWorldNormal);
	float4 Reflect = texCUBE(_LocalEnvCube, ReflectionDir)*_Disturb;
	float ReflectDouble =saturate(smoothstep(0.6,0.62,pow( Reflect.r , 3.15) *50) ) * 0.4; 

	//Caustics
	float4 ScreenPosition = IN.screenPos; 
	float3 ViewVector = mul(unity_CameraInvProjection,float4(ScreenPosition.xy*2.0-1.0 , 0 , -1));
	float3 viewDir = mul(unity_CameraToWorld,float4(ViewVector,0));
	float4 depthTex = tex2D( _CameraDepthRT, ScreenPosition.xy );
	float depth = DecodeFloatRGB(depthTex.xyz);	
	float SceneDepth = Linear01DepthPers(depth);
	float2 OffsetUV = ( viewDir* SceneDepth + _WorldSpaceCameraPos.xyz ).xz;
	float NoiseCaustics = tex2D(_WaveTex, OffsetUV*_CausticsTiling*1.3  - _Time.y*_CausticsSpeed).g;	
    float Caustics = tex2D( _WaveTex, OffsetUV*_CausticsTiling + _Time.y*_CausticsSpeed*0.85+ NoiseCaustics*_CausticsDisturbInt ).g + NoiseCaustics;	
    float3 CausticsCol =Caustics.xxx* _CausticsColor.xyz*_CausticsInt;
	CausticsCol = pow(CausticsCol , 2.5 );

	Col.rgb += waveTex.xyz; 	
	float3 DirectDiffuse  = Col.rgb;
	float3 DirectSpecular =  Reflect.xyz + SpecularColor*_SpecularInt + CausticsCol;
	float a =  _MainColor.a ;
	return float4(DirectSpecular + DirectDiffuse , a);

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
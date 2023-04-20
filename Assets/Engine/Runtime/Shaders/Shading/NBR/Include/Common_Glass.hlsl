#ifndef COMMON_GLASS_INCLUDE
#define COMMON_GLASS_INCLUDE


#define _CUSTOM_MATERIAL
#define _Normal _ProcedureTex0
#define _MatCapTex _ProcedureTex1
#define _SpColor _Color1
#define _FresnelEdge _Param0.x
#define _FresnelIntensity _Param0.y
#define _RefIntensity _Param0.z
#define _BumpMapInt _Param0.w
#define _LaserContrast _Param1.x
#define _LaserInt _Param1.y
#define _LaserTiling _Param1.z

//function:			   
FLOAT2 MaTCapUV(in FLOAT3 N)
{

	FLOAT3 viewnormal = mul((FLOAT3x3)unity_WorldToCamera, N);
	// FLOAT3 viewdir = normalize(viewpos);
	// FLOAT3 viewcross = cross(viewdir,viewnormal);
	// viewnormal= FLOAT3(-viewcross.y , viewcross.x , 0.0);
	FLOAT2 matcapuv = viewnormal.xy * 0.5 + 0.5;
	return matcapuv;
}

FLOAT3 RFlerpColor(in FLOAT3 rfmatcap, in FLOAT thickness)
{
	FLOAT3 c1 = _MainColor.rgb * 0.5;
	FLOAT3 c2 = rfmatcap * _MainColor.rgb;
	FLOAT cmask = thickness;
	return lerp(c1, c2, cmask);
}

inline void CustomMaterial(in FFragData FragData, inout FMaterialData MaterialData)
{
	//diffuse
	FLOAT2 uv = GET_FRAG_UV;
	FLOAT4 Color = SAMPLE_TEX2D(_MainTex, uv) * _MainColor;
	MaterialData.BaseColor = Color;
	MaterialData.DyeColor = Color.xyz;

	//normal 
	//VertexNormal = FragData.TangentToWorld[2].xyz;
	FLOAT3x3 TangentToWorld = FragData.TangentToWorld;
	FLOAT3 Normal = SAMPLE_TEX2D(_Normal, uv).xyz;
	FLOAT Mask = SAMPLE_TEX2D(_Normal, uv).w;
	FLOAT2 Normaloff = (Normal.xy * 2 - FLOAT2(1, 1)) * _BumpMapInt;
	FLOAT3 NormalLocal = FLOAT3(Normaloff, 1);
	FLOAT3 WorldNormal = normalize(mul(NormalLocal, TangentToWorld).xyz);

	//Thickness
	FLOAT ThicknessTex = SAMPLE_TEX2D(_Normal, uv).z;
	FLOAT Thickness = ThicknessTex * 0.75;

	//Fresnel
	FLOAT3 ViewDir = FragData.CameraVector;
	FLOAT NdV = dot(WorldNormal.xyz, ViewDir);
	FLOAT EdgeThick = saturate((NdV - _FresnelEdge) * _FresnelIntensity);
	//EdgeThick = 1-EdgeThick*_Thickness;
	FLOAT Thicknessfix = Thickness + EdgeThick;

	//Laser
	//low 
	// FLOAT NdL= dot(WorldNormal.xyz,normalize(_SceneLightDir.xyz));
	// NdL = NdL *0.5 + 0.5;
	// FLOAT3 RampTex = SAMPLE_TEX2D(_RampTex,FLOAT2(NdL,NdL)).rgb *_RampScale ; 
	//high
#ifdef _USE_LASER
	FLOAT Tdir = dot(ViewDir, WorldNormal.xyz) * _LaserTiling;
	FLOAT3 NormalColor = FLOAT3(0.5, 0.5, 1);
	FLOAT3 ColorChang01 = cos(Tdir) * NormalColor;
	FLOAT3 ColorChang02 = cross(normalize(FLOAT3(1, 1, 1)), NormalColor) * sin(Tdir);
	FLOAT3 ColorChang03 = normalize(FLOAT3(1, 1, 1)) * dot(normalize(FLOAT3(1, 1, 1)), NormalColor);
	FLOAT3 RampColor = ColorChang01 + ColorChang02 + (1 - cos(Tdir)) * ColorChang03 * _LaserContrast;
#endif
	//matcap
	FLOAT2 MatcapUV = MaTCapUV(WorldNormal);
	FLOAT4 MatCapColor = SAMPLE_TEX2D(_MatCapTex, MatcapUV);
	FLOAT4 SpColor = MatCapColor * _SpColor;



	//reflect
	FLOAT  Refintensity = Thicknessfix * _RefIntensity;
	// FLOAT3 rfmatcap = tex2D(_MatCapTex, MatcapUV+Refintensity);
	FLOAT3 rfmatcap = SAMPLE_TEX2D(_MatCapTex, MatcapUV + Refintensity).xyz;
	FLOAT3 rfmatColor = RFlerpColor(rfmatcap, Thicknessfix);


	//Alpha
	Color.xyz = rfmatColor.rgb;
#ifdef _USE_LASER
	Color.xyz = lerp(Color.xyz, Color.xyz + RampColor, _LaserInt);
#endif
	Color.a = saturate(max(SpColor.r * _SpColor.a, Thicknessfix) * _MainColor.a);
	//Color.a =1;

	MaterialData.DiffuseColor = Color.xyz;
	MaterialData.SpecularColor = SpColor.xyz;
	MaterialData.BaseColor.a = Color.a;
	MaterialData.AO = 1;
}

#define _CUSTOM_LIGHT
void CustomLighting(FFragData FragData, FMaterialData MaterialData,
	FShadowData ShadowData, FLOAT ShadowMask,
	inout FLOAT3 DirectDiffuse, inout FLOAT3 DirectSpecular DEBUG_ARGS)
{
	FLOAT2 Shadow = ShadowData.Shadow.xy;
	DirectDiffuse = MaterialData.DiffuseColor;
	DirectSpecular = MaterialData.SpecularColor;
}
#endif//COMMON_GLASS_INCLUDE
#define _VERTEX_COLOR
#define _INPUT_UV2_4
#include "../Include/PCH.hlsl"
//#include "../Include/GpuAnimation.hlsl"
#define _CUSTOM_INTERPOLANTS	
 
FLOAT4 _ColorOutline;
#ifdef _BASE_FROM_COLOR
#define _OutlineColor _Color.xyz
#define _OutlineWidth _Color.w
#else
#define _OutlineColor _ColorOutline.xyz
#define _OutlineWidth _ColorOutline.w
#endif


FLOAT4 _OutlineScale;
#define _MinDist (_OutlineScale.x)
#define _MaxDist (_OutlineScale.y)
#define _MinScale (_OutlineScale.z)
#define _MaxScale (_OutlineScale.w)
#define _UseZOffset _Param3.x>0

FLOAT4 _MatEffectOutlineParam;
#define _MatEffectOutlineColor (_MatEffectOutlineParam.xyz)
#define _MatEffectOutlineBlend (_MatEffectOutlineParam.w)

// #define _AnimationTex _ProcedureTex3	
// FLOAT4 _ProcedureTex3_TexelSize;
// #define _AnimationTexWidthInv _ProcedureTex3_TexelSize.x
// #define _AnimationTexWidth _ProcedureTex3_TexelSize.z
// #define _AnimationTexHightInv _ProcedureTex3_TexelSize.y

// #define _AnimationTime _CustomTime.x
// #if defined(_INSTANCE_READY) && defined(_INSTANCE)
// 	IBUFFER_START(Param)
// 		REAL4 param;
// 	IBUFFER_END(paramArray,64)
// #endif
// inline REAL DecodeRealRG( REAL2 enc )
// {
// 	REAL2 kDecodeDot = REAL2(1.0, 1/255.0);
// 	return dot( enc, kDecodeDot );
// }
				
// REAL4 GPUAnimation( inout FVertexInput Input ,REAL4 localPos INSTANCE_ID)
// {
// 	#if defined(_INSTANCE_READY) && defined(_INSTANCE)
// 		REAL time = clamp(_AnimationTime - paramArray[instanceID].param.x,0,0.9999);
// 	#else
// 		REAL time = clamp(_AnimationTime,0,0.9999);
// 	#endif
// 	time *= _AnimationTexWidth;
// 	REAL4 pos = FLOAT4(localPos.xyz,1);
// 	REAL floorTime = floor(time*0.25)*4*_AnimationTexWidthInv;
// 	REAL x0 = _AnimationTexWidthInv*0.1 + floorTime;
// 	REAL x1 = _AnimationTexWidthInv*1.1 + floorTime;
// 	REAL x2 = _AnimationTexWidthInv*2.1 + floorTime;
// 	REAL x3 = _AnimationTexWidthInv*3.1 + floorTime;

// 	REAL y = Input.uv2.x*256*_AnimationTexHightInv+0.0001;

// 	REAL4 anim0 = SAMPLE_TEX2D_LOD(_AnimationTex, REAL2(x0,y),0);
// 	REAL4 anim1 = SAMPLE_TEX2D_LOD(_AnimationTex, REAL2(x1,y),0);
// 	REAL4 anim2 = SAMPLE_TEX2D_LOD(_AnimationTex, REAL2(x2,y),0);
// 	REAL4 anim3 = SAMPLE_TEX2D_LOD(_AnimationTex, REAL2(x3,y),0);

// 	REAL posX = DecodeRealRG(REAL2(anim0.a,anim3.r));
// 	REAL posY = DecodeRealRG(REAL2(anim1.a,anim3.g));
// 	REAL posZ = DecodeRealRG(REAL2(anim2.a,anim3.b));
	
// 	REAL4x4 boneMatrix = REAL4x4(
// 		anim0.xyz*2-1 , (posX*2-1)*10,
// 		anim1.xyz*2-1 , (posY*2-1)*10,
// 		anim2.xyz*2-1 , (posZ*2-1)*10,
// 		0,0,0,1
// 		);

// 	REAL4 pos0 = mul(boneMatrix,pos);
// 	REAL3 normal0 = normalize (mul((REAL3x3)boneMatrix,Input.TangentX));
// 	REAL3 Normal = normal0;
// 	REAL4 Pos = pos0;
	
// 	#ifdef _BONE_2
// 		REAL y1 = Input.uv2.y*256*_AnimationTexHightInv+0.0001;
// 		REAL4 anim0_1 = SAMPLE_TEX2D_LOD(_AnimationTex, REAL2(x0,y1),0);
// 		REAL4 anim1_1 = SAMPLE_TEX2D_LOD(_AnimationTex, REAL2(x1,y1),0);
// 		REAL4 anim2_1 = SAMPLE_TEX2D_LOD(_AnimationTex, REAL2(x2,y1),0);
// 		REAL4 anim3_1 = SAMPLE_TEX2D_LOD(_AnimationTex, REAL2(x3,y1),0);
// 		REAL posX_1 = DecodeRealRG(REAL2(anim0_1.a,anim3_1.r));
// 		REAL posY_1 = DecodeRealRG(REAL2(anim1_1.a,anim3_1.g));
// 		REAL posZ_1 = DecodeRealRG(REAL2(anim2_1.a,anim3_1.b));

// 		REAL4x4 boneMatrix1 = REAL4x4(
// 			anim0_1.xyz*2-1 , (posX_1*2-1)*10,
// 			anim1_1.xyz*2-1 , (posY_1*2-1)*10,
// 			anim2_1.xyz*2-1 , (posZ_1*2-1)*10,
// 			0,0,0,1
// 			);
// 		REAL4 pos1 = mul(boneMatrix1,pos);
// 		REAL3 normal1 = normalize(mul((REAL3x3)boneMatrix1,Input.TangentX));
// 		Normal = lerp(normal1,normal0,Input.uv2.z); 
// 		Pos = lerp(pos1,pos0,Input.uv2.z); 
// 	#endif//_BONE_2
// 	Input.TangentX = Normal;
// 	Pos.xyz+=Normal*0.01;
// 	return Pos;
// }

 

FInterpolantsVSToPS CustomInterpolantsVSToPS(in FVertexInput Input, in FLOAT4 WorldPosition, out FLOAT4 projPos ,in REAL4 instanceRot)
{
	DECLARE_OUTPUT(FInterpolantsVSToPS, Interpolants);
	SET_UV(Input);

	// build tangent to object space matrix
	FLOAT3 binormal = cross(normalize(Input.TangentX), normalize(Input.TangentZ.xyz)) * Input.TangentZ.w;
	FLOAT3x3 rot = FLOAT3x3(normalize(Input.TangentZ.xyz), binormal, normalize(Input.TangentX));
	FLOAT3 outlineRawData = Input.Color.xyz;
	FLOAT3 normal = mul(outlineRawData, rot);
	FLOAT vertexScaleData = min(0.3, length(Input.Color.rgb));

	// instancing rotation
	normal = SafeNormalize(Rot(instanceRot,normal));	

	FLOAT distanceWidthScale = log2(length(_CameraPos.xyz - WorldPosition.xyz) + 1);

	FLOAT4 pos = mul(_matrixVP, FLOAT4(WorldPosition.xyz, 1.0));

	// transform to world space
	#ifdef _GPU_ANIMATION
	FLOAT3 norm = mul(   (FLOAT3x3)(_matrixV), mul((FLOAT3x3)custom_ObjectToWorld,normal));
	#else
	FLOAT3 norm = mul(   (FLOAT3x3)(_matrixV), mul((FLOAT3x3)_objectToWorld,normal));
	#endif

	// get normalized projection direction
	FLOAT2 extendDir = normalize(mul((FLOAT2x2)_matrixP, norm.xy));
	// resolve screen aspect.
	extendDir.y *= _ScreenParams.x / _ScreenParams.y;
	// fov scale: _matrixP[0][0] = cot(FOV/2)
	extendDir *= atan(-_matrixP[1][1]);

	FLOAT width = _OutlineWidth * 0.015 * distanceWidthScale * vertexScaleData;
	pos.xy += extendDir * width;
	projPos = pos;
 
	UNITY_BRANCH
	if(_UseZOffset)
	{
		FLOAT mask = Input.uv2.z;
		FLOAT4 Z_V =mul(_matrixV, FLOAT4(WorldPosition.xyz, 1.0));
		Z_V.z-=5*mask;
		FLOAT4 Z_P = mul(_matrixP, Z_V);
		projPos.z = Z_P.z;
	}

	Interpolants.WorldPosition = WorldPosition; 
	
	SET_VS_DEPTH(Interpolants, projPos.zw);

	return Interpolants;
}

#define _CUSTOM_PS


FLOAT4 CustomPS(in FInterpolantsVSToPS Interpolants, in FLOAT4 SvPosition, inout FLOAT4 rt1)
{
#ifdef _DITHER_TRANSPARENT
	clip(-1);
#endif
	FLOAT4 BaseColor = SAMPLE_TEX2D(_MainTex, Interpolants.TexCoords[0].xy);
#ifdef _BASE_FROM_COLOR
	BaseColor.rgb = FLOAT3(1, 1, 1);
	clip(BaseColor.a - 0.5);
#else	
	BaseColor.rgb *= _MainColor.rgb;
#endif
	
	FLOAT3 col = BaseColor.rgb * _OutlineColor.rgb;
	col = lerp(col, _MatEffectOutlineColor, _MatEffectOutlineBlend);
	rt1.xyz = EncodeFloatRGB(Interpolants.Depth01.x / Interpolants.Depth01.y);
	rt1.w = EncodeAlpha(1, _IsRt1zForUIRT);
	return FLOAT4(col, 1);
}

#include "../Include/Vertex.hlsl"
#include "../Include/Pixel.hlsl"

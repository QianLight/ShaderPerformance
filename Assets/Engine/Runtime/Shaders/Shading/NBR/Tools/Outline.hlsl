#ifndef OUTLINE_DEFINED
#define OUTLINE_DEFINED
#define FOG_OFF
#define _VERTEX_COLOR
#define _INPUT_UV2_4
#ifndef _CUSTOM_VERTEX_PARAM
#define _CUSTOM_VERTEX_PARAM
#endif

#include "../Include/PCH.hlsl"
//#include "../Include/GpuAnimation.hlsl"
#define _CUSTOM_INTERPOLANTS	

// #if !defined(ROLE_SRP_BATCH)
// float4 _ColorOutline;
// float4 _OutlineScale;
// float4 _MatEffectOutlineParam;
// #endif

#ifdef _BASE_FROM_COLOR
#define _OutlineColor _Color.xyz
#define _OutlineWidth _Color.w
#else
#define _OutlineColor _ColorOutline.xyz
#define _OutlineWidth _ColorOutline.w
#endif


#define _MinDist (_OutlineScale.x)
#define _MaxDist (_OutlineScale.y)
#define _MinScale (_OutlineScale.z)
#define _MaxScale (_OutlineScale.w)
#define _UseZOffset _Param3.x>0

#define _MatEffectOutlineColor (_MatEffectOutlineParam.xyz)
#define _MatEffectOutlineBlend (_MatEffectOutlineParam.w)

float _IsUIScene;

float4 _RenderScaledScreenParams;

// #define _AnimationTex _ProcedureTex3	
// float4 _ProcedureTex3_TexelSize;
// #define _AnimationTexWidthInv _ProcedureTex3_TexelSize.x
// #define _AnimationTexWidth _ProcedureTex3_TexelSize.z
// #define _AnimationTexHightInv _ProcedureTex3_TexelSize.y

// #define _AnimationTime _CustomTime.x
// #ifdef _INSTANCE
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
// 	#ifdef _INSTANCE
// 		REAL time = clamp(_AnimationTime - paramArray[instanceID].param.x,0,0.9999);
// 	#else
// 		REAL time = clamp(_AnimationTime,0,0.9999);
// 	#endif
// 	time *= _AnimationTexWidth;
// 	REAL4 pos = float4(localPos.xyz,1);
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

inline void CustomVertex(in FVertexInput Input, inout FInterpolantsVSToPS Interpolants)
{
}

FInterpolantsVSToPS CustomInterpolantsVSToPS(in FVertexInput Input, in float4 WorldPosition, out float4 projPos ,in REAL4 instanceRot)
{
	DECLARE_OUTPUT(FInterpolantsVSToPS, Interpolants);
	SET_UV(Input);

	// 计算Output
	projPos = mul(_matrixVP, float4(WorldPosition.xyz, 1.0));
	Interpolants.WorldPosition = WorldPosition;

	// 从Input取值
	float3 normal = normalize(Input.TangentX);
	float4 tangent = Input.TangentZ;
	float3 binormal = cross(normal, normalize(tangent.xyz)) * tangent.w;
	float3 inputOutlineVectorTS = Input.Color.xyz;
	// TODO: 和顶点输入值不太一致，所以需要*3.695，得查一下是为什么。
	float inputOutlineLength = min(1, length(inputOutlineVectorTS) * 3.695);
	
	// 解决0向量导致的描边突然断掉的问题
	float3 outlineVectorTS = inputOutlineVectorTS + normal * 1e-4;

	// 空间转换
	float3x3 tangentToObjectMatrix = float3x3(normalize(tangent.xyz), binormal, normalize(normal));
	float3 outlineVectorOS = mul(outlineVectorTS, tangentToObjectMatrix);
	// Instancing旋转
	if (abs(dot(instanceRot,1)) > 0 )
	{
		outlineVectorOS = normalize(Rot(instanceRot, outlineVectorOS));
	}

	#ifdef _GPU_ANIMATION
	float3x3 objectToWorldMatrix = custom_ObjectToWorld;
	#else
	float3x3 objectToWorldMatrix = (float3x3)_objectToWorld;
	#endif
	float3 outlineNormalWS = mul(objectToWorldMatrix,outlineVectorOS);
	float3 outlineNormalVS = mul((float3x3)(_matrixV), outlineNormalWS);
	float2 outlineNormalCS = mul((float2x2)_matrixP, outlineNormalVS.xy);

	// 1像素宽度的描边方向
	float2 outlineDirCS = normalize(outlineNormalCS.xy);
	float2 pixelSize = 2.0 / _RenderScaledScreenParams.xy * projPos.ww;
	// 单位为米的深度值,线性深度
	float positionZVS = projPos.w; /* projPos.w == -mul(_matrixV, WorldPosition).z */
	float eyeDepth = LinearEyeDepth(projPos.z / projPos.w);
	// eyedepth [0, +∞] => distance scale [2, 1]

	// FOV占据透视变化的权重 
	float fovWeight = clamp(sqrt(_CameraFov - 10) * 0.036, 0.0, 0.4);//Add by: Takeshi
	// 距离淡出
	//float distanceWidthScale = 1 / (0.1 * positionZVS + 0.4) - 0.4;
	float distanceWidthScale = 1 / (0.1 * positionZVS + fovWeight) - 0.4;
	float paramScale = _OutlineWidth / 0.4;
	
	#if USE_PIXEL_SIZE
	float2 pixelPerfect = outlineDirCS * pixelSize;
	float2 pixelPerfect1To2 = pixelPerfect * inputOutlineLength * distanceWidthScale * paramScale;
	#else
	float2 constantScale = float2(_RenderScaledScreenParams.y/_RenderScaledScreenParams.x, 1) * projPos.ww * 0.0015;
	float2 pixelPerfect1To2 = outlineDirCS * constantScale * inputOutlineLength * distanceWidthScale * paramScale;
	#endif

	float2 absDir = abs(pixelPerfect1To2);
	float2 dirSign = sign(pixelPerfect1To2);
	float2 clamped = step(pixelSize, absDir);
	float2 finalDir = lerp(dirSign * pixelSize, pixelPerfect1To2, clamped);
	float subPixelAlpha = saturate(length(pixelPerfect1To2 * float2(1.0, _RenderScaledScreenParams.y / _RenderScaledScreenParams.x)) / pixelSize.x);

	projPos.xy += finalDir;

	// 解决部分内凹面描边穿插出奇怪的线条的问题
	// TODO: 在OpenGL下会有严重的闪烁，加了PRECESION_FIXER后没有任何改善，估计是变换矩阵或者length计算精度不足。
	//		 现在回退到旧版本，使用Offset来控制偏移。
	// const float PRECESION_FIXER = 100.0;
	// float4 outlineWS = mul(unity_MatrixInvVP, projPos);
	// float len = length((WorldPosition.xyz - outlineWS.xyz) * PRECESION_FIXER) * 2 / PRECESION_FIXER;
	// outlineWS.xyz -= normalize((_WorldSpaceCameraPos.xyz - outlineWS.xyz) * PRECESION_FIXER) * len;
	// projPos = mul(unity_MatrixVP, outlineWS);
	
	// TODO: UI下强制alpha为1，解决blend问题。
	Interpolants.CustomData = subPixelAlpha;
	
	SET_VS_DEPTH(Interpolants, projPos.zw);
	
#ifndef _FACEOUTLINE

	// Fix cloak mesh crossing 
	#if SHADER_API_GLES3
	projPos.z = lerp(projPos.z,projPos.z + 0.001,(step(0.5,Input.uv2.z)));
	#else
	projPos.z = lerp(projPos.z,projPos.z - projPos.z*projPos.z*projPos.z,saturate(step(0.5,Input.uv2.z)));
	#endif

#endif

	return Interpolants;
}

#define _CUSTOM_PS


float4 CustomPS(in FInterpolantsVSToPS Interpolants, in float4 SvPosition, inout float4 rt1)
{
#ifdef _DITHER_TRANSPARENT
	clip(-1);
#endif


	float alpha = 1;
	
	// pixel size alpha scale: while outline width thinner than 1 pixel, use alpha to optimize it.
	alpha *= Interpolants.CustomData.x * Interpolants.CustomData.x;
	
	// fov scale:
	// _matrixP[1][1] is cot(FOV/2)
	// while fov=55, _matrixVP[1][1]=-1.9(Metal/D3D11/Vulkan) or = 1.9(OpenGL/ES)
	#if UNITY_UV_STARTS_AT_TOP
	#define DEFAULT_FOV_SCALE -1.9
	#else
	#define DEFAULT_FOV_SCALE +1.9
	#endif
	alpha *= _matrixP[1][1] * DEFAULT_FOV_SCALE;

	// disable transparency while in UIScene.
	alpha = max(alpha, _IsUIScene);

	// resolve problem of blending.
	alpha = saturate(alpha);

	// 计算颜色
	float4 BaseColor = SAMPLE_TEX2D(_MainTex, Interpolants.TexCoords[0].xy);
	#ifdef _BASE_FROM_COLOR
	BaseColor.rgb = float3(1, 1, 1);
	clip(BaseColor.a - 0.5);
	#else	
	BaseColor.rgb *= _MainColor.rgb;
	#endif
	float3 col = BaseColor.rgb * _OutlineColor.rgb;
	col = lerp(col, _MatEffectOutlineColor, _MatEffectOutlineBlend);

	return float4(col, alpha);
}


#include "../Include/URP_Vertex.hlsl"
#include "../Include/URP_Pixel.hlsl"

#endif
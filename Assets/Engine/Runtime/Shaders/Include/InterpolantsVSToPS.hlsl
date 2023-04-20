// Copyright 2018- PWRD, Inc. All Rights Reserved.

#ifndef PBS_INTERPOLANTSVSTOPS_INCLUDE
#define PBS_INTERPOLANTSVSTOPS_INCLUDE

// Used for vertex factory shaders which need to use the resolved view
FLOAT4 SvPositionToResolvedScreenPosition(FLOAT4 SvPosition)
{
	FLOAT2 PixelPos = SvPosition.xy;// -ResolvedView.ViewRectMin.xy;

	// NDC (NormalizedDeviceCoordinates, after the perspective divide)
	FLOAT2 ViewSizeAndInvSize = FLOAT2(_ScreenParams.z - 1.0f, _ScreenParams.w - 1.0f);
	FLOAT3 NDCPos = FLOAT3((PixelPos * ViewSizeAndInvSize - 0.5f) * FLOAT2(2, -2), SvPosition.z);

	// SvPosition.w: so .w has the SceneDepth, some mobile code and the DepthFade material expression wants that
	return FLOAT4(NDCPos.xyz, 1) * SvPosition.w;
}



FInterpolantsVSToPS GetInterpolantsVSToPS(in FVertexInput Input, FLOAT4 WorldPosition,FLOAT4 rot, out FLOAT4 projPos)
{
	DECLARE_OUTPUT(FInterpolantsVSToPS, Interpolants);

	SET_UV(Input);
	SET_UV2(Input);
	SET_BACKUP_UV(Input);
	SET_LIGTHMAP_UV(Input);
	
#ifndef _NO_NORMAL
    FLOAT sign = Input.TangentZ.w * GetOddNegativeScale();
    Interpolants.NormalWS.xyz = TransformObjectToWorldDir(Input.TangentX,rot);
    Interpolants.TangentWS.xyz = TransformObjectToWorldDir(Input.TangentZ.xyz,rot);
    Interpolants.BitangentWS.xyz = cross(Interpolants.NormalWS.xyz, Interpolants.TangentWS.xyz) * sign;
	FLOAT3 viewDirWS = _CameraPos.xyz - WorldPosition.xyz;
	Interpolants.NormalWS.w = viewDirWS.x;
	Interpolants.TangentWS.w = viewDirWS.y;
	Interpolants.BitangentWS.w = viewDirWS.z;
#endif


#if defined(_OUTPUT_VERTEX_COLOR)
	#ifdef _VERTEX_COLOR
		Interpolants.Color = Input.Color;
	#endif//_VERTEX_COLOR
#endif//_OUTPUT_VERTEX_COLOR

#ifdef _VERTEX_GI
	Interpolants.DiffuseGI.xyz = SHPerVertex(Interpolants.NormalWS.xyz);
#endif//_VERTEX_GI

	Interpolants.WorldPosition = WorldPosition;
#ifdef _DECAL	
	Interpolants.WorldPosition = float4(ObjectToWorldDir(Input.Position.xyz),1);
#endif 
	
#ifdef _CUSTOM_VERTEX_PARAM
	CustomVertex(Input,Interpolants);
#endif //_CUSTOM_VERTEX_PARAM

#ifdef _CLOUD2
    sign = Input.TangentZ.w * GetOddNegativeScale();;
    Interpolants.NormalWS.xyz = Input.TangentX.xyz;
    Interpolants.TangentWS.xyz = Input.TangentZ.xyz;
    Interpolants.BitangentWS.xyz = cross(Interpolants.NormalWS.xyz, Interpolants.TangentWS.xyz) * sign;
    Interpolants.ObjectPosition = Input.Position;
#endif
Interpolants.LocalPosition = Input.Position;
	

#if defined(_SHADOW_MAP)&&!defined(_NO_SHADOWMAP)
	#if defined(_SIMPLE_SHADOW)
		EncodeSimpleShadowCoord(WorldPosition,
			Interpolants.NormalWS.xyz,
			Interpolants.ShadowCoord0
		);
	#else
		EncodeShadowCoord(WorldPosition,
			Interpolants.NormalWS.xyz,
			Interpolants.ShadowCoord0
			, Interpolants.ShadowCoord1
			, Interpolants.ShadowCoord2
		);
	#endif//_SIMPLE_SHADOW
#endif//_SHADOW_MAP

	// FLOAT3 viewPos = 0;
	projPos = TransformWorldToClipPos2(WorldPosition);	
	SET_VS_DEPTH(Interpolants,projPos.zw);
	return Interpolants;
}



#endif //PBS_INTERPOLANTSVSTOPS_INCLUDE

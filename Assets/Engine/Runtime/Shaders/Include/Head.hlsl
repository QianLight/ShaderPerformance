#ifndef PBS_HEAD_INCLUDE
#define PBS_HEAD_INCLUDE

// GlobalSettings
FLOAT4 _GlobalSettings0;
#define _IsRt1zForUIRT _GlobalSettings0.x

#define _INPUT_UV0
#if (defined(_CUSTOM_LIGHTMAP_ON)&&!defined(_TERRAIN)&&!defined(_WORLD_UV_OFFSET))||defined(_CUSTOM_UV2)
	#define _INPUT_UV2		
#endif//((_CUSTOM_LIGHTMAP_ON))&&!(_TERRAIN)


//uv0
#ifdef _UV_SCALE
	#define SET_UV(Input) Interpolants.TexCoords[0].xy = (Input.uv0) * _UVST0.xy + _UVST0.zw
#else//!_UV_SCALE
	#define SET_UV(Input) Interpolants.TexCoords[0].xy = (Input.uv0)		
#endif//_UV_SCALE
#define GET_FRAG_UV FragData.TexCoords[0].xy

#if defined(_CUSTOM_UV2)
	#define SET_UV2(Input) Interpolants.TexCoords[0].zw = (Input.uv2.xy) * _UVST0.xy + _UVST0.zw
	#define GET_FRAG_UV2 FragData.TexCoords[0].zw
	#define _UV_SLOT_1
#else
	#if defined(_UV_SCALE2)
		#define SET_UV2(Input) Interpolants.TexCoords[0].zw = (Input.uv0) * _UVST1.xy + _UVST1.zw
		#define GET_FRAG_UV2 FragData.TexCoords[0].zw
		#define _UV_SLOT_1
	#else
		#define SET_UV2(Input)
		#define GET_FRAG_UV2 FragData.TexCoords[0].xy
	#endif//_UV_SCALE2
#endif//_CUSTOM_UV2

//backuv
#if defined(_BACKUP_UV)
	#if !defined(_UV_SLOT_1)
		#define SET_BACKUP_UV(Input) Interpolants.TexCoords[0].zw = (Input.uv0)
		#define GET_FRAG_BACKUP_UV FragData.TexCoords[0].zw
		#define _UV_SLOT_1
	#else
		#define SET_BACKUP_UV(Input) Interpolants.TexCoords[1].xy = (Input.uv0)		
		#define GET_FRAG_BACKUP_UV FragData.TexCoords[1].xy
		#define _UV_SLOT_2
	#endif
#else//!_BACKUP_UV
	#define SET_BACKUP_UV(Input)
	#define GET_FRAG_BACKUP_UV FLOAT2(0,0)
#endif//_BACKUP_UV
//lightmap
#if defined(_CUSTOM_LIGHTMAP_ON)
	#if !defined(_UV_SLOT_1)
		#define _LIGHTMAP_UV_SLOT Interpolants.TexCoords[0].zw
		#define GET_FRAG_LIGTHMAP_UV FragData.TexCoords[0].zw
	#else
		#define _LIGHTMAP_UV_SLOT Interpolants.TexCoords[1].zw
		#define GET_FRAG_LIGTHMAP_UV FragData.TexCoords[1].zw
		#define _UV_SLOT_2
	#endif

	#if defined(_TERRAIN)
		#define SET_LIGTHMAP_UV(Input) _LIGHTMAP_UV_SLOT = (Input.uv0) * _LightMapUVST.xy + _LightMapUVST.zw
	#else//!_TERRAIN
		#if defined(_COMBINE_MESH)
			#define SET_LIGTHMAP_UV(Input) _LIGHTMAP_UV_SLOT = (Input.uv2.xy)//lightmap uvst bake in mesh in runtime			
		#else
			#if defined(_WORLD_UV_OFFSET)
				#define SET_LIGTHMAP_UV(Input) _LIGHTMAP_UV_SLOT = (WorldPosition.xz - _ChunkOffset.xy)*_ChunkOffset.zw
			#else
				#define SET_LIGTHMAP_UV(Input) _LIGHTMAP_UV_SLOT = (Input.uv2.xy) * _LightMapUVST.xy + _LightMapUVST.zw
			#endif			
		#endif			
	#endif//_TERRAIN
#else//!_CUSTOM_LIGHTMAP_ON
	#define SET_LIGTHMAP_UV(Input)
	#define GET_FRAG_LIGTHMAP_UV FLOAT2(0,0)
#endif

#ifdef _UV_SLOT_2
	#define _OUTPUT_UV_COUNT 2
#else
	#define _OUTPUT_UV_COUNT 1
#endif

#define GET_VERTEX_NORMAL FragData.TangentToWorld[2].xyz
#define GET_VERTEX_TANGENT FragData.TangentToWorld[0].xyz

#define GET_VS_NORMAL Interpolants.TangentToWorld2.xyz

#ifdef _NO_CLAC_DEPTH
#define SET_VS_DEPTH(vs2ps,depth) (vs2ps).Depth01 = (depth)
#define GET_VS_DEPTH(vs2ps) FragData.depth01 = 0
#else
#define SET_VS_DEPTH(vs2ps,depth) (vs2ps).Depth01 = (depth)
#define GET_VS_DEPTH(vs2ps) FragData.depth01 = ((vs2ps).Depth01.x/(vs2ps).Depth01.y)
#endif

struct FVertexInput
{  
	FLOAT4	Position	: POSITION;
	FLOAT3	TangentX	: NORMAL;
	FLOAT4	TangentZ 	: TANGENT;

#ifdef _INPUT_UV0 
	FLOAT2	uv0 : TEXCOORD0;
#endif//_INPUT_UV
 
#ifdef _INPUT_UV2_4 
	FLOAT4	uv2 : TEXCOORD1;
#else
	#ifdef _INPUT_UV2 
		FLOAT2	uv2 : TEXCOORD1;
	#endif//_INPUT_UV2
#endif//_INPUT_UV2_4

#ifdef _VERTEX_COLOR
	FLOAT4	Color : COLOR;
#endif //_VERTEX_COLOR

}; 

struct FInterpolantsVSToPS
{
	REAL4 NormalWS : TEXCOORD10;// xyz: normal, w: viewDir.x
	REAL4 TangentWS : TEXCOORD11;// xyz: tangent, w: viewDir.y
	REAL4 BitangentWS : TEXCOORD12;// xyz: bitangent, w: viewDir.z

#ifdef _VERTEX_COLOR
	FLOAT4	Color : COLOR0;
#endif//_VERTEX_COLOR

#if _OUTPUT_UV_COUNT
	REAL4	TexCoords[_OUTPUT_UV_COUNT]	: TEXCOORD0;
#endif//_OUTPUT_UV_COUNT

#ifdef _VERTEX_GI
	FLOAT4	DiffuseGI : TEXCOORD2;
#endif//_VERTEX_GI

#ifdef _XRAY
	FLOAT	NDV : TEXCOORD2;
#endif//_VERTEX_GI

	FLOAT4 WorldPosition : TEXCOORD3; // xyz = world position, w = clip z

#ifdef _INSTANCE
	FLOAT	InstanceID : TEXCOORD4;
#endif//_INSTANCE

#ifdef _SCREEN_POS
	FLOAT4 ScreenPosition : TEXCOORD4;
	FLOAT2 ScreenPositionW : TEXCOORD5;
#endif
#ifdef _DEPTH_SHADOW	
	FLOAT4  CLIPPostion : COLOR1;
	FLOAT4  CLIPScreenPosition : COLOR2;
	FLOAT2  CLIPScreenPositionW : COLOR3;
#endif 

#ifdef _CLOUD2
	FLOAT4 ObjectPosition : TEXCOORD2;
#endif


#if defined(_SHADOW_MAP)&&!defined(_NO_SHADOWMAP)
	#if defined(_SIMPLE_SHADOW)
		REAL4 ShadowCoord0 : TEXCOORD6;
	#else
		REAL4 ShadowCoord0 : TEXCOORD6;
		REAL4 ShadowCoord1 : TEXCOORD7;
		REAL4 ShadowCoord2 : TEXCOORD8;
	#endif//_SIMPLE_SHADOW
	//4 csm
#endif//_SHADOW_MAP

#ifdef _CUSTOM_VERTEX_PARAM
	FLOAT4 CustomData : TEXCOORD9;
	FLOAT4 CustomData1 : TEXCOORD13;
#endif//_CUSTOM_VERTEX_PARAM

	FLOAT2 Depth01 : TEXCOORD14;
#ifdef _ENABLE_DEBUG
	FLOAT4 VertexDebugData : TEXCOORD15;
#endif//_ENABLE_DEBUG

FLOAT4 LocalPosition : TEXCOORD16;

};

struct FFragData
{
#if _OUTPUT_UV_COUNT
	REAL4	TexCoords[_OUTPUT_UV_COUNT];
#endif//_OUTPUT_UV_COUNT
	FLOAT depth01;
	FLOAT4 SvPosition;
	/** Post projection position reconstructed from SvPosition, before the divide by W. left..top -1..1, bottom..top -1..1  within the viewport, W is the SceneDepth */
	FLOAT4 ScreenPosition;
#ifdef _DEPTH_SHADOW	
	FLOAT4 ClipScreenPosition;
	FLOAT4 CLIPSVPOSTION;
	FLOAT RIMMASK;
#endif
	FLOAT4 WorldPosition;
	REAL3 WorldPosition_CamRelative;
	FLOAT4 VertexColor;
	FLOAT4 Ambient;	
	REAL3 CameraVector;//viewDIR
	FLOAT3x3 TangentToWorld;
	FLOAT TangenSign;

#ifdef _CSM3
	FLOAT3 ShadowCoord[4];	
#else
	FLOAT3 ShadowCoord[3];	
#endif
	FLOAT3 ShadowCoord2;
#ifdef _CUSTOM_VERTEX_PARAM
	FLOAT4 CustomData;
	FLOAT4 CustomData1;
#endif//_CUSTOM_VERTEX_PARAM

#ifdef _INSTANCE
	FLOAT	InstanceID;
#endif//_INSTANCE

#ifdef _ENABLE_DEBUG
	FLOAT4 VertexDebugData;
#endif//_ENABLE_DEBUG

	FLOAT facing;
	FLOAT4 LocalPosition;
};

struct FMaterialData
{
	REAL3 WorldNormal;
	REAL3 TangentSpaceNormal;

#ifdef _ANISOTROPY
	REAL3 WorldTangent;
	REAL3 WorldBinormal;
#endif

	FLOAT4 BaseColor;	
	FLOAT3 DyeColor;
	FLOAT4 BlendMask;

	REAL PerceptualRoughness;
	REAL Roughness;
	REAL Roughness2;
	REAL OneMinusReflectivity;
	REAL Metallic;

	FLOAT SrcAO;
	FLOAT AO;
	FLOAT3 DiffuseColor;
	FLOAT3 SpecularColor;
	FLOAT4 ScaleParam;
	REAL NdotV;
	
	FLOAT ToonAO;
	FLOAT BloomIntensity;

	FLOAT3 Emissive;

#ifdef _FRAMEBUFFER_FETCH
	FLOAT4 FrameBuffer;
#endif//_FRAMEBUFFER_FETCH

	FLOAT4 CustomParam;
	FLOAT4 CustomParam1;

#ifdef _WATER
	FLOAT3 NoNoiseWorldNormal;
#endif//_WATER
};

#define _SpecMult MaterialData.ScaleParam.x
#define _IBLMult MaterialData.ScaleParam.y

struct FLightingContext
{
	FLOAT3 DiffuseColor;
	FLOAT3 SpecularColor;
	FLOAT3 LightDir;
	FLOAT3 LightColor;
	REAL FixNdotL;
	REAL NdotL;
	REAL NdotV;
	REAL VdotL;
	#ifdef _UNREAL_MODE
	REAL H;
	#else
	REAL3 H;
	#endif
	REAL NdotH;
	REAL LdotH;
	REAL VdotH;
	REAL R;
	REAL R2;
	REAL4 Falloff;
	FLOAT LightMask;
};

#endif //PBS_HEAD_INCLUDE
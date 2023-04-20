#ifndef _FX_COMMON_INCLUDE
#define _FX_COMMON_INCLUDE

//MARCO DEFINE BEGIN
#define DEFINE_SAMPLER_2D(name) uniform sampler2D name;float4 name##_ST
#define TEX_2D(uv,id)  tex2D(_Tex##id,TRANSFORM_TEX(uv, _Tex##id))
#define TEX_2D_UVOFFSET(uv,offset,id)  tex2D(_Tex##id,TRANSFORM_TEX(uv, _Tex##id)+offset)
#define TEXLOD_2D(uv,id,lod)  tex2Dlod(_Tex##id,float4(TRANSFORM_TEX(uv, _Tex##id),0,lod))
#define DEFINE_VECTOR4(name)  uniform float4 name
#define DEFINE_VECTOR3(name)  uniform float3 name
#define DEFINE_VECTOR2(name)  uniform float2 name
#define DEFINE_VECTOR1(name)  uniform float  name
//MARCO DEFINE END

struct CustomAppdata
{
	float4 vertex  : POSITION;
#ifdef NEED_BUMP_TEX
	float3 normal  : NORMAL;
	float4 tangent : TANGENT;
#elif defined (NEED_VERTEX_NORMAL) 
	float3 normal  : NORMAL;		
#endif	
	float2 uv 	   : TEXCOORD0;
#ifdef NEED_UV2	
	float2 uv2 	   : TEXCOORD1;
#endif	
#ifdef NEED_VERTEX_COLOR
	fixed4 color   : COLOR;
#endif	
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct CustomV2F
{
	float4 uv 		: TEXCOORD0;
	UNITY_FOG_COORDS(1)
#ifdef NEED_BUMP_TEX
	float4 tSpace0  : TEXCOORD2;
	float4 tSpace1  : TEXCOORD3;
	float4 tSpace2  : TEXCOORD4;
#elif defined (NEED_VERTEX_NORMAL) 
	float3 wsNormal : TEXCOORD2;
	float3 wsPos    : TEXCOORD3;
#else
	float3 wsPos    : TEXCOORD2;	
#endif	
#ifdef NEED_VERTEX_COLOR
	fixed4 vColor   : TEXCOORD5;
#endif
	float4 vertex 	: SV_POSITION;
};


//UNIFORMS BEGIN

//SAMPLERS
DEFINE_SAMPLER_2D(_Tex0);		//Always use it for Main Texture
DEFINE_SAMPLER_2D(_Tex1);		//Always use it for Bump Texture or Displace Texture
DEFINE_SAMPLER_2D(_Tex2);		//Always use it for Append Texture
DEFINE_SAMPLER_2D(_Tex3);		//Always use it for Append Displace Texture
DEFINE_SAMPLER_2D(_Tex4);		//Always use it for Append Displace Texture
//VECTORS
//DEFINE_VECTOR1(_IsLighted);
//DEFINE_VECTOR4(_DirectionalLightColor0);
DEFINE_VECTOR1(_GlobalIntensity);
DEFINE_VECTOR1(_VertexColorRange);
DEFINE_VECTOR4(_MainColor);
DEFINE_VECTOR1(_OffsetStrength);
DEFINE_VECTOR1(_IsMainTexDynamic);
DEFINE_VECTOR1(_IsDisTexDynamic);
DEFINE_VECTOR1(_IsBlendMainTexA);
DEFINE_VECTOR1(_UseClipTex);
DEFINE_VECTOR1(_DissolveTest);
DEFINE_VECTOR4(_DissolveTestPara);
DEFINE_VECTOR4(_DissolveTestPara1);
DEFINE_VECTOR4(_DissolveTestColor);
DEFINE_VECTOR4(_DisplaceUV);
DEFINE_VECTOR1(_DisplaceStrength);
DEFINE_VECTOR4(_ChannelSetRGBA);
DEFINE_VECTOR1(_IsAlphaBlend);
DEFINE_VECTOR4(_AlphaSetRGBA);
DEFINE_VECTOR1(_IsGray);
DEFINE_VECTOR4(_GraySetRGBA);
DEFINE_VECTOR1(_AlphaRange);
DEFINE_VECTOR1(_AlphaClip);
DEFINE_VECTOR1(_Intensity);
DEFINE_VECTOR1(_IntensityRange);
DEFINE_VECTOR1(_IsAddTexDynamic);
DEFINE_VECTOR1(_IsAddDisTexDynamic);
DEFINE_VECTOR4(_AddDisplaceUV);
DEFINE_VECTOR1(_AddDisplaceStrength);
DEFINE_VECTOR4(_AddChannelColor);
DEFINE_VECTOR1(_AddIntensity);
DEFINE_VECTOR1(_AddIntensityRange);
DEFINE_VECTOR4(_RGBAintensity);
DEFINE_VECTOR1(_ChannelRange);
DEFINE_VECTOR4(_ClipRect);

//UNIFORMS END

CustomV2F vertBase (CustomAppdata v)
{
	CustomV2F o;
	UNITY_INITIALIZE_OUTPUT(CustomV2F, o);
	UNITY_SETUP_INSTANCE_ID(v);
	
	o.uv.xy = v.uv;
#ifdef NEED_UV2	
	o.uv.zw = v.uv2;
#else
	o.uv.zw = v.uv;
#endif	
#ifdef NEED_BUMP_TEX
	float3 wsPos = mul(unity_ObjectToWorld,float4(v.vertex.xyz,1)).xyz;
	float3 wsNormal = UnityObjectToWorldNormal(v.normal);
	float3 wsTangent = UnityObjectToWorldDir(v.tangent.xyz);
	float3 wsBitangent = cross(wsNormal,wsTangent) * v.tangent.w * unity_WorldTransformParams.w;

	o.tSpace0 = float4(wsTangent.x,wsBitangent.x,wsNormal.x,wsPos.x);
	o.tSpace1 = float4(wsTangent.y,wsBitangent.y,wsNormal.y,wsPos.y);
	o.tSpace2 = float4(wsTangent.z,wsBitangent.z,wsNormal.z,wsPos.z);
	
	o.vertex = UnityWorldToClipPos(wsPos);
#else
#ifdef NEED_VERTEX_NORMAL 
	o.wsNormal = UnityObjectToWorldNormal(v.normal);
#endif
	o.wsPos = mul(unity_ObjectToWorld,float4(v.vertex.xyz,1)).xyz;
	o.vertex = UnityWorldToClipPos(o.wsPos);
#endif	
#ifdef NEED_VERTEX_COLOR
	o.vColor = v.color;
#endif	
	UNITY_TRANSFER_FOG(o,o.vertex);
	return o;
}

//Fragment Functions
void GetWorldNormalInFragment (CustomV2F i,inout float3 wsNormal)
{
#ifdef NEED_BUMP_TEX
	float3 tsNormal = UnpackNormal(tex2D(_Tex1,i.uv));
	wsNormal = float3(dot(i.tSpace0.xyz,tsNormal),dot(i.tSpace1.xyz,tsNormal),dot(i.tSpace2.xyz,tsNormal));
#elif defined (NEED_VERTEX_NORMAL)
	wsNormal = i.wsNormal;	
#endif
}

void GetWorldPostionInFragment (CustomV2F i,inout float3 wsPostion)
{
#ifdef NEED_BUMP_TEX
	float3 tsNormal = UnpackNormal(tex2D(_Tex1,i.uv));
	wsPostion = float3(i.tSpace0.w,i.tSpace1.w,i.tSpace2.w);
#else
	wsPostion = i.wsPos;
#endif
}

float rand(float2 co)
{
	float x = dot(co.xy ,float2(12.9898,78.233));
	x = abs(frac(x*3.1415926)-0.5)*2;
	x = x*x - 1;
	x = x*x - 1;
	//x = frac(sin(x));
	return frac(x * 43758.5453);
}


float Noise(float2 v)
{
	float2 i = floor(v);
	float2 t = frac(v);
	float2 u  = t*t*(3-2*t);

	return lerp(lerp(rand(i + float2(0,0)),rand(i + float2(1,0)),u.x),
				lerp(rand(i + float2(0,1)),rand(i + float2(1,1)),u.x),
				u.y);
}

float NoiseRange(float2 v,float2 range)
{
	return lerp(range.x,range.y,Noise(v));	
}

float UIClip(in float2 position, in float4 clipRect)
{
	float4 halfSizeAndMidPos =  (clipRect.zwzw + float4(-clipRect.xy,clipRect.xy)) * 0.5f;
	
	float2 xy = halfSizeAndMidPos.xy >= abs(halfSizeAndMidPos.zw - position.xy) ? 1 : 0;

	return xy.x*xy.y;
}

#endif

#ifndef WIND_EFFECT_INCLUDE
#define WIND_EFFECT_INCLUDE

FLOAT4 _AmbientWindParam;
#define _AmbientWindDir _AmbientWindParam.xyz
#define _AmbientWindSpeed _AmbientWindParam.w
FLOAT4 _AmbientWindParam1;
#define _AmbientWindFrequency _AmbientWindParam1.x
#define _AmbientTreeWindSpeed _AmbientWindParam1.y
#define _AmbientWindDir2 _AmbientWindParam1.zw
TEX2D_SAMPLER(_AmbientWind);

TEX2D_SAMPLER(_CollisionTex);
FLOAT4 _CollisionCenter;
#define _CollisionRadiusInv _CollisionCenter.w

FLOAT4  _InteractParam;
#define _GRASS_COLLISION _InteractParam.x>0.5


FLOAT3x3 RotationMatrix(FLOAT3 vAxis, FLOAT fAngle)
{
	
	FLOAT2 vSinCos;
    //  #ifdef OPENGL
	    vSinCos.x = sin(fAngle);
	    vSinCos.y = cos(fAngle);
    //#else
	    // FastSinCos(fAngle, vSinCos.x, vSinCos.y);
    //#endif

	        FLOAT c = vSinCos.y;
	        FLOAT s = vSinCos.x;
	        FLOAT t = 1.0 - c;
	        FLOAT x = vAxis.x;
	        FLOAT y = vAxis.y;
	        FLOAT z = vAxis.z;

	    return FLOAT3x3(t * x * x + c, t * x * y - s * z, t * x * z + s * y,
					    t * x * y + s * z, t * y * y + c, t * y * z - s * x,
					    t * x * z - s * y, t * y * z + s * x, t * z * z + c);
}
	
//FLOAT4 _GrassCollisionParam;
#define _PushAngle _GrassCollisionParam.x
#define _PushValue _GrassCollisionParam.y
//	#define _CollisionEnable _GrassCollisionParam.z

FLOAT4 GrassEffect(REAL4 localpos,FLOAT3 center1)				
	{
		FLOAT4 waveWind1 = FLOAT4(localpos.xyz,1);
		UNITY_BRANCH
				#ifdef _FLOWER				
				FLOAT4 center = FLOAT4(center1,1);
				FLOAT2 UV = saturate((center.xyz - _CollisionCenter.xyz).xz/_CollisionRadiusInv + FLOAT2(0.5,0.5));
				FLOAT4 tmp_wind = SAMPLE_TEX2D_LOD(_CollisionTex, UV,0);
				//FLOAT4 tmp_wind = SampleWind(center);						    
				FLOAT tmp_height =localpos.w;
				FLOAT3 calwind=FLOAT3(tmp_wind.x, 0, tmp_wind.y) * 2 - FLOAT3(1,0,1);       
				FLOAT3 windDirection =normalize(cross(calwind, FLOAT3(0, -1, 0)));
				FLOAT xddee= tmp_wind.z*_PushAngle*tmp_height*any(windDirection);
				FLOAT3x3 tmp_rotation = RotationMatrix(windDirection, radians(xddee));
				FLOAT3 waveWind = mul(tmp_rotation, (localpos.xyz));
				waveWind1=FLOAT4(waveWind,1);
				// localpos=waveWind1;
			#endif
		//}
		return waveWind1;
	}

FLOAT4 GrassEffecttest(REAL4 localpos)				
	{
		FLOAT4 waveWind1 = FLOAT4(localpos.xyz,1);
				#ifdef _FLOWER				
				FLOAT4 center = mul(unity_ObjectToWorld, FLOAT4(0,0,0,1));
				FLOAT2 UV = saturate((center.xyz - _CollisionCenter.xyz).xz/_CollisionRadiusInv + FLOAT2(0.5,0.5));
				FLOAT4 tmp_wind = SAMPLE_TEX2D_LOD(_CollisionTex, UV,0);					    
				FLOAT tmp_height =localpos.w;
				FLOAT3 calwind=FLOAT3(tmp_wind.x, 0, tmp_wind.y) * 2 - FLOAT3(1,0,1);       
				FLOAT3 windDirection =normalize(cross(calwind, FLOAT3(0, -1, 0)));
				FLOAT xddee= tmp_wind.z*tmp_wind.z*_PushAngle*tmp_height*any(windDirection);
				FLOAT3x3 tmp_rotation = RotationMatrix(windDirection, radians(xddee));
				FLOAT3 waveWind = mul(tmp_rotation, localpos.xyz);
				waveWind1=FLOAT4(waveWind,1);
				// localpos=waveWind1;
			#endif
		return waveWind1;
	}

#endif//WIND_EFFECT_INCLUDE
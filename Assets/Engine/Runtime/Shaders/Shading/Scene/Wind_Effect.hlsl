
#ifndef WIND_EFFECT_INCLUDE
#define WIND_EFFECT_INCLUDE

//half4 _AmbientWindParam;
//#define _AmbientWindDir _AmbientWindParam.xyz
//#define _AmbientWindSpeed _AmbientWindParam.w
//half4 _AmbientWindParam1;
//#define _AmbientWindFrequency _AmbientWindParam1.x
//#define _AmbientTreeWindSpeed _AmbientWindParam1.y
//#define _AmbientWindDir2 _AmbientWindParam1.zw


	

//	TEXTURE2D(_CollisionTex);           SAMPLER(sampler_CollisionTex);
//SAMPLER(_CollisionTex);
half4 _CollisionCenter;
#define _CollisionRadiusInv _CollisionCenter.w

half4  _InteractParam;
#define _GRASS_COLLISION _InteractParam.x>0.5


half3x3 RotationMatrix(half3 vAxis, half fAngle)
{
	
	half2 vSinCos;
    //  #ifdef OPENGL
	    vSinCos.x = sin(fAngle);
	    vSinCos.y = cos(fAngle);
    //#else
	    // FastSinCos(fAngle, vSinCos.x, vSinCos.y);
    //#endif

	        half c = vSinCos.y;
	        half s = vSinCos.x;
	        half t = 1.0 - c;
	        half x = vAxis.x;
	        half y = vAxis.y;
	        half z = vAxis.z;

	    return half3x3(t * x * x + c, t * x * y - s * z, t * x * z + s * y,
					    t * x * y + s * z, t * y * y + c, t * y * z - s * x,
					    t * x * z - s * y, t * y * z + s * x, t * z * z + c);
}
	
//half4 _GrassCollisionParam;
//#define _PushAngle _GrassCollisionParam.x
//#define _PushValue _GrassCollisionParam.y
//	#define _CollisionEnable _GrassCollisionParam.z

half4 GrassEffect(float4 localpos,half3 center1)				
	{
		half4 waveWind1 = half4(localpos.xyz,1);
		UNITY_BRANCH
				#ifdef _FLOWER				
				half4 center = half4(center1,1);
				half2 UV = saturate((center.xyz - _CollisionCenter.xyz).xz/_CollisionRadiusInv + half2(0.5,0.5));
				half4 tmp_wind = SAMPLE_TEX2D_LOD(_CollisionTex, UV,0);
				//half4 tmp_wind = SampleWind(center);						    
				half tmp_height =localpos.w;
				half3 calwind=half3(tmp_wind.x, 0, tmp_wind.y) * 2 - half3(1,0,1);       
				half3 windDirection =normalize(cross(calwind, half3(0, -1, 0)));
				half xddee= tmp_wind.z*_PushAngle*tmp_height*any(windDirection);
				half3x3 tmp_rotation = RotationMatrix(windDirection, radians(xddee));
				half3 waveWind = mul(tmp_rotation, (localpos.xyz));
				waveWind1=half4(waveWind,1);
				// localpos=waveWind1;
			#endif
		//}
		return waveWind1;
	}

//half4 GrassEffecttest(Attributes Input)				
//	{
//		half4 waveWind1 = half4(Input.positionOS.xyz,1);
//		UNITY_BRANCH

//			#ifdef _ISFLOWER_OFF				
//				half4 center = mul(unity_ObjectToWorld, half4(0,0,0,1));
//				half2 UV = saturate((center.xyz - _CollisionCenter.xyz).xz/_CollisionRadiusInv + half2(0.5,0.5));
//				half4 tmp_wind = SAMPLE_TEX2D_LOD(_CollisionTex, UV,0);					    
//			//	half tmp_height =localpos.w;
//				half hweight = saturate(Input.lightmapUV.y);
//				half3 calwind=half3(tmp_wind.x, 0, tmp_wind.y) * 2 - half3(1,0,1);       
//				half3 windDirection =normalize(cross(calwind, half3(0, -1, 0)));
//				half xddee= tmp_wind.z*tmp_wind.z*_PushAngle*tmp_height*any(windDirection);
//				half3x3 tmp_rotation = RotationMatrix(windDirection, radians(xddee));
//				half3 waveWind = mul(tmp_rotation, Input.positionOS.xyz);
//				waveWind1=half4(waveWind,1);
//				// localpos=waveWind1;
//			#endif
//		//}
//		return waveWind1;
//	}

#endif//WIND_EFFECT_INCLUDE

#ifndef TREE_EFFECT_INCLUDE
#define TREE_EFFECT_INCLUDE

#include "../Scene/Wind_Effect.hlsl"

//#define _CUSTOM_VERTEX_OFFSET

#define _Magnitude _Param2.x
#define _Frenquency _Param2.y
#define _ModelScaleCorrection _Param2.z
#define _MaskRange _Param2.w

#define _OffsetCorrection _Param3.xyz
#define _Blend _Param3.w
#define _StableRange _Param4.x

// #ifdef _WIND_TRUNK
//   #define _Range _Param2.x
// #endif

FLOAT3 CustomWindEffect( inout FVertexInput Input,FLOAT4 WorldPosition
#ifdef _ENABLE_DEBUG
    ,inout FLOAT4 debugData
#endif
)
{
    FLOAT3 offset = 0;
#ifdef _DETIAL_TREE_MOVE	
    //UNITY_BRANCH
    //if(_AmbientTreeWindSpeed>0.01)
    {
        FLOAT2 WindUV = (WorldPosition.xz*_AmbientWindFrequency*0.01)+(_Time.y *_AmbientTreeWindSpeed * 0.1);
        FLOAT WindTex = SAMPLE_TEX2D_LOD(_AmbientWind,WindUV,0).r;
        

        WindTex = WindTex * 2 - 1;
        FLOAT spaceOffset = -WorldPosition.x * _AmbientWindDir.x - WorldPosition.z * _AmbientWindDir.z;	 
        FLOAT weightedTime = fmod(_Frenquency * (spaceOffset * _ModelScaleCorrection + _Time.y), PI);
        FLOAT sway = max(sin(weightedTime + 0.5 * sin(weightedTime)), sin(weightedTime + PI + sin(weightedTime + PI))) - 0.9;
        FLOAT yoffset = abs(cos(sway)) * _AmbientWindDir.y;
        FLOAT horizontalOffset = sin(sway);
        FLOAT xoffset = horizontalOffset * _AmbientWindDir.x;
        FLOAT zoffset = horizontalOffset * _AmbientWindDir.z;
        //使用顶点色计算定点的偏移系数 effect ratio
        offset += REAL3(xoffset, yoffset , zoffset) *Input.uv2.y * _ModelScaleCorrection * _Magnitude; 
        Input.Color.x = lerp(Input.Color.x,1,_MaskRange);
        offset *= Input.Color.x; 
     

        _OffsetCorrection.xz += sin(_OffsetCorrection.xz*WindTex);
        FLOAT3 blend = pow(abs(Input.uv2.y),_Blend)+_OffsetCorrection*Input.uv2.y;
        offset += blend;
        FLOAT Stable =lerp(Input.uv2.y , 0 , _StableRange);
        offset *=Stable;
 
    }
#endif//_DETIAL_TREE_MOVE  
    return offset;
}
#endif//TREE_EFFECT_INCLUDE

#include "../Scene/Scene_Lod.hlsl"

#ifndef SCENE_HEAD_INCLUDE
#define SCENE_HEAD_INCLUDE

#ifdef _CUSTOM_EFFECT
#define _BACKUP_UV
#endif//_CUSTOM_EFFECT

#ifdef _NO_COMMON_EFFECT
    #define _NO_COLOR_EFFECT
    #define _NO_EMISSIVE
    #define _NO_AO
    #define _PBS_NO_IBL
#endif//_NO_COMMON_EFFECT


#if !defined(_NO_LIGHTMAP)
#define _CUSTOM_LIGHTMAP_ON
#endif

#ifdef _TERRAIN
    #define _MIN_SPEC
    #define _SIMPLE_NORMAL
#endif//_TERRAIN
#define _TICK_NORMAL

#ifndef _UNITY_AMBIENT
#define _Scene_Lighting
#endif

#include "../Include/PCH.hlsl"
#include "../Scene/Scene_Effect.hlsl"
#endif//SCENE_HEAD_INCLUDE
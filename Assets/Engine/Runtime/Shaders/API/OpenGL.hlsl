#ifndef UNITY_UV_STARTS_AT_TOP
#define UNITY_UV_STARTS_AT_TOP 0
#endif
#ifndef UNITY_REVERSED_Z
#define UNITY_REVERSED_Z 0
#endif
#include "CommonAPI.hlsl" 
#define INSTANCING_CBUFFER_BEGIN(name)  cbuffer name {
#define INSTANCING_CBUFFER_END          }
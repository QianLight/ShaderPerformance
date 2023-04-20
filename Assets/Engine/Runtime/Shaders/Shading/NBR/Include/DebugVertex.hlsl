#ifndef PBS_DEBUGVERTEX_INCLUDE
#define PBS_DEBUGVERTEX_INCLUDE

#ifdef _ENABLE_DEBUG

#define Debug_Vertex_None 0
#define Debug_Vertex_Wind_PivotPos0 (Debug_Vertex_None+1)
#define Debug_Vertex_Wind_Gradient0 (Debug_Vertex_Wind_PivotPos0+1)
#define Debug_Vertex_Wind_PivotPos1 (Debug_Vertex_Wind_Gradient0+1)
#define Debug_Vertex_Wind_Gradient1 (Debug_Vertex_Wind_PivotPos1+1)
#define Debug_Vertex_Wind_RandNoise (Debug_Vertex_Wind_Gradient1+1)
#define Debug_Vertex_Wind_ObjectScaleZ (Debug_Vertex_Wind_RandNoise+1)
#define Debug_Vertex_Wind_MajorMovement (Debug_Vertex_Wind_ObjectScaleZ+1)
#endif//_ENABLE_DEBUG

#endif //PBS_DEBUGVERTEX_INCLUDE
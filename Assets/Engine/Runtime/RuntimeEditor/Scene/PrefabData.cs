// using System;
// using System.Collections.Generic;
// using System.IO;
// using UnityEngine;
// using UnityEngine.Rendering;

// namespace CFEngine.Editor
// {
//     [System.Serializable]
//     public class PrefabFbxData : BaseSceneData<PrefabFbxData>
//     {
//         public GameObject prefabFbx;
//         public string name;
//         [System.NonSerialized]
//         public int subCount;

//         public override void Copy (PrefabFbxData src)
//         {
//             prefabFbx = src.prefabFbx;
//             name = src.name;
//         }
//     }

//     [System.Serializable]
//     public class GameObjectGroupData : BaseSceneData<GameObjectGroupData>
//     {
//         public string name = "";
//         public string path = "";
//         public string editTag = "";
//         public bool visible = true;

//         [System.NonSerialized]
//         public Transform transform;
//         public override string ToString ()
//         {
//             return name;
//         }
//         public override void Copy (GameObjectGroupData src)
//         {
//             name = src.name;
//             path = src.path;
//             editTag = src.editTag;
//             visible = src.visible;
//         }
//     }

//     [System.Serializable]
//     public class GameObjectInstanceData : BaseSceneData<GameObjectInstanceData>
//     {
//         public string name = "";
//         public string editTag = "";
//         public uint resType;
//         public int groupIndex = -1;
//         public int prefabIndex = -1;
//         public Vector3 pos;
//         public Quaternion rotate;
//         public Vector3 scale;
//         public string tag;
//         public int layer;
//         public Bounds aabb;

//         public int start;
//         public int end;
//         public FlagMask flag;
//         public static uint GameObjectActiveInHierarchy = 0x0001;
//         public static uint GameObjectActive = 0x0002;
//         public override void Copy (GameObjectInstanceData src)
//         {
//             name = src.name;
//             editTag = src.editTag;
//             resType = src.resType;
//             groupIndex = src.groupIndex;
//             prefabIndex = src.prefabIndex;
//             pos = src.pos;
//             rotate = src.rotate;
//             scale = src.scale;
//             tag = src.tag;
//             aabb = src.aabb;
//             start = src.start;
//             end = src.end;
//             flag.flag = src.flag.flag;
//         }

//         public override string ToString ()
//         {
//             return name;
//         }
//     }

//     [System.Serializable]
//     public class MeshData : BaseSceneData<MeshData>
//     {
//         public int renderIndex = 0;
//         public Material mat;
//         public Bounds aabb;
//         public Vector3 pos;
//         public Quaternion rotate;
//         public Vector3 scale;
//         public Vector3 localScale;

//         public int chunkID = -1;
//         public int blockID = 0;
//         public float lodDist2 = 1000000;
//         public float fadeDist2 = 1000000;

//         public int lightMapVolumnIndex = -1;
//         public int lightMapIndex = -1;
//         public Vector4 lightmapUVST = new Vector4 (1, 1, 0, 0);
//         public int reflectionProbeIndex = -2;
//         public string exString = "";
//         public int groupObject = -1;
//         public FlagMask flag;

//         public static uint GameObjectActiveInHierarchy = 0x0001;
//         public static uint GameObjectActive = 0x0002;
//         public static uint HasAnim = 0x0004;
//         public static uint RenderEnable = 0x0008;
//         public static uint IgnoreShadowCaster = 0x0020;
//         public static uint HideRender = 0x0040;
//         public static uint IgnoreShadowReceive = 0x0080;
//         public static uint MultiMaterial = 0x0100;
//         public static uint UVAnimation = 0x0200;
//         public static uint FadeEffect = 0x0400;
//         public override void Copy (MeshData src)
//         {
//             renderIndex = src.renderIndex;
//             mat = src.mat;
//             aabb = src.aabb;
//             mat = src.mat;
//             pos = src.pos;
//             rotate = src.rotate;
//             scale = src.scale;
//             localScale = src.localScale;

//             chunkID = src.chunkID;
//             blockID = src.blockID;
//             lodDist2 = src.lodDist2;
//             fadeDist2 = src.fadeDist2;

//             lightMapVolumnIndex = src.lightMapVolumnIndex;
//             lightMapIndex = src.lightMapIndex;
//             lightmapUVST = src.lightmapUVST;
//             reflectionProbeIndex = src.reflectionProbeIndex;

//             exString = src.exString;
//             groupObject = src.groupObject;
//             flag.flag = src.flag.flag;
//         }
//     }
// }
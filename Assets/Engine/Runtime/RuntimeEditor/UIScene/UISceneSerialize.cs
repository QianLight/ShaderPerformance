#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace CFEngine
{
    public class UISceneSerialize
    {
        public static void Save (string name)
        {

        }
    }
    //     public delegate void PreSaveExternalDataCb (SceneSerialize.SceneSharedChunkInfo ssci);
    //     public delegate void SaveHeadExternalDataCb (BinaryWriter binaryWriter, SceneSerialize.SceneSharedChunkInfo ssci);
    //     public delegate void SaveChunkExternalDataCb (BinaryWriter binaryWriter, int i);
    //     public class SceneSerialize
    //     {

    //         public static PreSaveExternalDataCb preSaveExternalDataCb;
    //         public static SaveHeadExternalDataCb saveHeadExternalDataCb;
    //         public static SaveChunkExternalDataCb saveChunkExternalDataCb;
    //         internal static void GetSceneChunkCount (SceneConfig sceneConfig, out int chunkWidth, out int chunkHeight, out int widthCount, out int heightCount)
    //         {
    //             chunkWidth = EngineContext.ChunkSize;
    //             chunkHeight = EngineContext.ChunkSize;
    //             widthCount = sceneConfig.widthCount;
    //             heightCount = sceneConfig.heightCount;
    //         }
    //         public static SceneConfig CreateSceneConfig (ref SceneContext context, int widthCount, int heightCount, string tagname)
    //         {
    //             SceneAssets.GetCurrentSceneContext (ref context);
    //             if (context.valid)
    //             {
    //                 string config = "Config";
    //                 string subDir = context.dir + "/" + config;
    //                 if (!AssetDatabase.IsValidFolder (subDir))
    //                     AssetDatabase.CreateFolder (context.dir, config);
    //                 string terrain = "Terrain";
    //                 if (!AssetDatabase.IsValidFolder (context.terrainDir))
    //                     AssetDatabase.CreateFolder (context.dir, terrain);

    //                 SceneConfig si = ScriptableObject.CreateInstance<SceneConfig> ();
    //                 si.widthCount = widthCount;
    //                 si.heightCount = heightCount;
    //                 if (!string.IsNullOrEmpty (tagname))
    //                     si.sceneEditorTag.Add (tagname);
    //                 si.chunks = new System.Collections.Generic.List<SceneConfig.TerrainChunk> ();
    //                 SceneConfig newSi = CommonAssets.CreateAsset<SceneConfig> (subDir,
    //                     context.name + SceneContext.SceneConfigSuffix, ".asset", si);
    //                 return newSi;
    //             }
    //             else
    //             {
    //                 EditorUtility.DisplayDialog ("Error", "Save Scene First.", "OK");
    //                 return null;
    //             }
    //         }

    //         private static string GetConfigName (ref SceneContext context, string name)
    //         {
    //             if (string.IsNullOrEmpty (name))
    //                 return string.IsNullOrEmpty (context.suffix) ? context.name : context.suffix;
    //             else
    //                 return name;
    //         }

    //         public static void LoadEditorChunkData (ref SceneContext context, string name, bool create, out EditorChunkData ecd)
    //         {
    //             string path = string.Format ("{0}/{1}.asset",
    //                 context.configDir,
    //                 GetConfigName (ref context, name));

    //             ecd = AssetDatabase.LoadAssetAtPath<EditorChunkData> (path);
    //             if (create && ecd == null)
    //             {
    //                 ecd = ScriptableObject.CreateInstance<EditorChunkData> ();
    //                 ecd = CommonAssets.CreateAsset<EditorChunkData> (context.configDir,
    //                     GetConfigName (ref context, name), ".asset", ecd);
    //             }
    //         }

    //         public static void SaveEditorChunkData (ref SceneContext context, EditorChunkData ecd)
    //         {
    //             CommonAssets.CreateAsset<EditorChunkData> (context.configDir, GetConfigName (ref context, ""), ".asset", ecd);
    //         }

    //         public static void LoadLightmapVolumnData (ref SceneContext context, string name, bool create, out LightmapVolumnData lvd)
    //         {
    //             string path = string.Format ("{0}/{1}_Lightmap.asset",
    //                 context.configDir,
    //                 GetConfigName (ref context, name));

    //             lvd = AssetDatabase.LoadAssetAtPath<LightmapVolumnData> (path);
    //             if (create && lvd == null)
    //             {
    //                 lvd = ScriptableObject.CreateInstance<LightmapVolumnData> ();
    //                 lvd = CommonAssets.CreateAsset<LightmapVolumnData> (context.configDir,
    //                     GetConfigName (ref context, name) + "_Lightmap", ".asset", lvd);
    //             }
    //         }
    //         public static void SaveLightmapVolumnData (ref SceneContext context, LightmapVolumnData lvd)
    //         {
    //             CommonAssets.CreateAsset<LightmapVolumnData> (context.configDir, context.name + "_Lightmap", ".asset", lvd);
    //         }
    //         public static void LoadChunkLightData (ref SceneContext context, out ChunkLightData cld)
    //         {
    //             string path = string.Format ("{0}/{1}_ChunkLightData.asset",
    //                 context.configDir,
    //                 context.name);
    //             cld = AssetDatabase.LoadAssetAtPath<ChunkLightData> (path);
    //         }
    //         interface IMatObject
    //         {
    //             Material Mat { get; }
    //             float LodDist { get; }
    //             ushort LightmapId { get; set; }
    //             // Vector4 LightmapST { get; set; }
    //             string LightmpaName { get; set; }
    //         }

    //         public class BaseSceneObject
    //         {
    //             public Matrix4x4 worldMatrix;
    //             public string resPath;
    //             public uint id;
    //             public uint flag = 0;
    //             public virtual Bounds bounds { get { return new Bounds (); } }
    //             public void SetFlag (uint f, bool add)
    //             {
    //                 if (add)
    //                 {
    //                     flag |= f;
    //                 }
    //                 else
    //                 {
    //                     flag &= ~(f);
    //                 }
    //             }

    //             public bool HasFlag (uint f)
    //             {
    //                 return (flag & f) != 0;
    //             }
    //         }
    //         public class PrefabSceneObject : BaseSceneObject, IQuadTreeObject
    //         {
    //             public EditorChunkData.GameObjectInfo goi;
    //             public int blockID = -1;
    //             public int BlockId { get { return blockID; } }

    //             public int QuadNodeId { get; set; }

    //             public override Bounds bounds { get { return goi != null?goi.bound : new Bounds (); } }
    //         }
    //         public class CustomSceneObject : BaseSceneObject, IQuadTreeObject, IMatObject
    //         {
    //             public EditorChunkData.SubPrefabOverrideInfo spoi;

    //             //lightmap

    //             // public string fileID = "";

    //             public virtual int BlockId { get { return spoi != null?spoi.blockId: -1; } }
    //             public virtual int QuadNodeId { get; set; }
    //             public override Bounds bounds { get { return spoi != null?spoi.sceneAABB : new Bounds (); } }

    //             public virtual Material Mat { get { return spoi != null?spoi.mat : null; } }
    //             public virtual float LodDist { get { return spoi != null?spoi.lodDist: -1; } }
    //             public virtual ushort LightmapId { get; set; }
    //             public virtual string LightmpaName { get; set; }

    //             // public virtual byte LightmapIndex { get; set; }
    //             // public virtual Vector4 LightmapST { get; set; }

    //         }
    //         public class InstanceObject : CustomSceneObject
    //         {
    //             public short instanceCount = 0;
    //             public Bounds sceneAABB;
    //             public int blockID = -1;
    //             public ushort instanceID = 0;

    //             public Material mat;
    //             public string billboardMeshPath;
    //             public override int BlockId { get { return blockID; } }

    //             public override Bounds bounds { get { return sceneAABB; } }

    //             public override Material Mat { get { return mat; } }

    //         }

    //         public class DynamicObject : BaseSceneObject
    //         {
    //             public EditorChunkData.GameObjectInfo goi;
    //             public EditorChunkData.DynamicObjectData dod = null;
    //         }
    //         public class PointLight
    //         {
    //             public LightingInfo li = new LightingInfo ();
    //             public int blockID = 0;
    //         }

    //         public struct BoxAreaInfo
    //         {
    //             public Vector4 box;
    //             public Vector4 size;
    //         }
    //         public class EditorEnvArea
    //         {
    //             public EnvProfile envProfile;
    //             public List<BoxAreaInfo> areaList = new List<BoxAreaInfo> ();
    //         }

    //         public class SceneSharedChunkInfo
    //         {
    //             public SceneContext context;
    //             // public string sceneName = "";
    //             public SceneChunkInfo[] chunks;
    //             public List<BaseSceneObject> globalSceneObjects = new List<BaseSceneObject> ();
    //             //public bool hasGlobalLightmap = false;
    //             public List<string> resNames = new List<string> ();
    //             // public Dictionary<string, CustomSceneObject> objLightMapCache = new Dictionary<string, CustomSceneObject> ();
    //             public MatSaveData matSaveData = new MatSaveData ();
    //             public SceneConfig sceneConfig;
    //             public QuadTreeContext treeContext = new QuadTreeContext ();
    //             public ChunkLightData cld;
    //             public uint objID = 0;
    //             public Dictionary<string, string> editorResReDirect = new Dictionary<string, string> ();
    //             public Dictionary<string, uint> instanceMeshMap = new Dictionary<string, uint> ();
    //             public ushort instanceID = 5000;
    //             public List<EditorEnvArea> envData = new List<EditorEnvArea> ();

    //             public Dictionary<string, LightmapObjectInfo> lightmapObjects = new Dictionary<string, LightmapObjectInfo> ();

    //             public int chunkWidth;
    //             public int chunkHeight;
    //             public int widthCount;
    //             public int heightCount;

    //             public byte[] groundFlags;
    //             public float[] vertex;

    //             public int[] ChunkStreamOffset;
    //             public int chunkStreamPos;
    //             public byte[] ChunkInfo;
    //             public int chunkInfoPos;

    //             public void MatAddResName (string path, Texture tex)
    //             {
    //                 if (AddResName (path))
    //                 {
    //                     AddResReDirct (this, tex, path);
    //                 }
    //             }
    //             public bool AddResName (string str)
    //             {
    //                 if (!string.IsNullOrEmpty (str) &&
    //                     !resNames.Contains (str))
    //                 {
    //                     resNames.Add (str);
    //                     return true;
    //                 }
    //                 return false;
    //             }

    //         }

    //         public class SceneChunkInfo
    //         {
    //             public List<BaseSceneObject> sceneObjects = new List<BaseSceneObject> ();
    //             public List<ushort> sceneObjectIndex = new List<ushort> ();
    //             public Dictionary<int, SceneQuadBlock> sceneQuadBlocks = new Dictionary<int, SceneQuadBlock> ();
    //             public List<DynamicObject> dynamicObjects = new List<DynamicObject> ();
    //             public List<CameraWall> walls = new List<CameraWall> ();
    //             public List<InteractiveItem> interactiveList = new List<InteractiveItem> ();
    //             public List<LightingInfo> pointLights = new List<LightingInfo> ();
    //             public List<InstanceInfo> instanceInfos = null;
    //             public List<uint> argArray = null;
    //             public byte terrainLightmapIndex = 255;
    //             public Vector4 terrainLightmapST = Vector4.zero;
    //             public int instanceCount = 0;
    //             // public ushort[] heightinfo;

    //         }

    //         // public static void CalcLightBlockId (Vector3 center, int x, int z, int width, int height,
    //         //     out int blockIndex)
    //         // {
    //         //     Vector2 chunkCorner = new Vector4 (x * width, z * height);
    //         //     SceneQuadTree.FindChunkIndex (new Vector3 (center.x - chunkCorner.x, 0, center.z - chunkCorner.y),
    //         //         EngineContext.LightBlockSize,
    //         //         EngineContext.ChunkLightBlockScale, EngineContext.ChunkLightBlockScale, out x, out z);
    //         //     blockIndex = x + z * EngineContext.ChunkLightBlockScale;
    //         // }

    //         private static void AddResReDirct (SceneSharedChunkInfo ssci, UnityEngine.Object obj, string assetName)
    //         {
    //             string path = AssetDatabase.GetAssetPath (obj);
    //             string dir = Path.GetDirectoryName (path);
    //             string name = assetName + Path.GetExtension (path).ToLower ();
    //             ssci.editorResReDirect[name] = dir + "/";
    //         }
    //         private static void AddResReDirct (SceneSharedChunkInfo ssci, string dir, string assetNameWithExt, bool outputError = true)
    //         {
    //             if (File.Exists (dir + assetNameWithExt))
    //                 ssci.editorResReDirect[assetNameWithExt] = dir;
    //             else
    //             if (outputError)
    //                 Debug.LogErrorFormat ("res not exist:{0}", dir + assetNameWithExt);
    //             else
    //                 Debug.LogWarningFormat ("res not exist:{0}", dir + assetNameWithExt);
    //         }

    //         private static void LoadGroundVertex (SceneSharedChunkInfo ssci)
    //         {
    //             string path = string.Format ("{0}/TerrainVertex.bytes", ssci.context.configDir);
    //             if (File.Exists (path))
    //             {
    //                 using (FileStream fs = new FileStream (path, FileMode.Open))
    //                 {
    //                     BinaryReader br = new BinaryReader (fs);
    //                     int count = br.ReadInt32 ();
    //                     int gridCount = EngineContext.terrainGridCount + 1;
    //                     gridCount *= gridCount;
    //                     ssci.groundFlags = new byte[gridCount * count];
    //                     ssci.vertex = new float[gridCount * count];
    //                     for (int i = 0; i < count; ++i)
    //                     {
    //                         int startOffset = i * gridCount;
    //                         for (int j = 0; j < gridCount; ++j)
    //                         {
    //                             ssci.groundFlags[startOffset + j] = br.ReadByte ();
    //                         }
    //                         for (int j = 0; j < gridCount; ++j)
    //                         {
    //                             ssci.vertex[startOffset + j] = br.ReadSingle ();
    //                         }
    //                     }
    //                 }
    //             }

    //         }

    //         private static string GetPrefabPath (SceneSharedChunkInfo ssci, GameObject prefab)
    //         {
    //             string path = AssetDatabase.GetAssetPath (prefab);
    //             path = path.Substring (AssetsConfig.instance.ResourcePath.Length + 1);
    //             path = path.Replace (".prefab", "");
    //             ssci.AddResName (path);
    //             return path;
    //         }
    //         private static void PreSavePrefab (SceneSharedChunkInfo ssci, EditorChunkData ecd,
    //             EditorChunkData.GameObjectInfo goi, EditorChunkData.PrefabInfo prefabInfo)
    //         {
    //             if (goi.subPrefabChunkInfo.Count == 1)
    //             {
    //                 string path = GetPrefabPath (ssci, prefabInfo.prefab);
    //                 Vector4Int chunkID = goi.subPrefabChunkInfo[0];
    //                 var chunk = ssci.chunks[chunkID.x];
    //                 PrefabSceneObject so = new PrefabSceneObject ();
    //                 so.goi = goi;
    //                 int chunkLevel = 2;
    //                 int x = chunkID.x % ssci.widthCount;
    //                 int z = chunkID.x / ssci.widthCount;
    //                 Bounds aabb = goi.bound;
    //                 SceneQuadTree.CalcBlockId (aabb.size, aabb.center, x, z, ssci.chunkWidth, ssci.chunkHeight,
    //                     out chunkLevel, out so.blockID);
    //                 //BaseSceneObject
    //                 so.worldMatrix = Matrix4x4.TRS (goi.pos, goi.rotate, goi.scale);
    //                 so.resPath = path;
    //                 so.id = ssci.objID++;
    //                 //so.SetFlag (SceneObject.IsDistortion, goi.HasFlag (ObjectFlag.Distortion));
    //                 chunk.sceneObjects.Add (so);
    //             }
    //         }

    //         private static void PreSaveMeshPrefab (SceneSharedChunkInfo ssci, EditorChunkData ecd,
    //             EditorChunkData.GameObjectInfo goi, EditorChunkData.PrefabInfo prefabInfo)
    //         {
    //             for (int j = 0; j < goi.subPrefabChunkInfo.Count; ++j)
    //             {
    //                 Vector4Int chunkID = goi.subPrefabChunkInfo[j];
    //                 EditorChunkData.SubPrefabOverrideInfo spoi = null;
    //                 List<BaseSceneObject> sceneObjects = null;
    //                 // List<ChunkMatInfo> mats = null;
    //                 if (chunkID.x >= 0 && chunkID.y >= 0)
    //                 {
    //                     var editorChunk = ecd.chunks[chunkID.x];
    //                     var chunk = ssci.chunks[chunkID.x];
    //                     sceneObjects = chunk.sceneObjects;
    //                     // mats = chunk.mats;
    //                     if (chunkID.y < editorChunk.subPrefabOverrideInfos.Count)
    //                     {
    //                         spoi = editorChunk.subPrefabOverrideInfos[chunkID.y];
    //                     }
    //                 }
    //                 else if (chunkID.y < ecd.globalObjectInfo.Count)
    //                 {
    //                     spoi = ecd.globalObjectInfo[chunkID.y];
    //                     sceneObjects = ssci.globalSceneObjects;
    //                 }
    //                 if (sceneObjects != null && spoi != null &&
    //                     spoi.HasFlag (ObjectFlag.GameObjectActiveInHierarchy) &&
    //                     spoi.HasFlag (ObjectFlag.RenderEnable))
    //                 {
    //                     string meshName = "";
    //                     if ((spoi.scale.x * spoi.scale.y * spoi.scale.z) < 0)
    //                     {
    //                         meshName = string.Format ("{0}_{1}_mirror", prefabInfo.prefab.name, chunkID.z);
    //                     }
    //                     else
    //                     {
    //                         meshName = string.Format ("{0}_{1}", prefabInfo.prefab.name, chunkID.z);
    //                     }
    //                     string meshPath = string.Format ("{0}{1}/{2}.asset",
    //                         AssetsConfig.instance.ResourcePath,
    //                         AssetsConfig.instance.EditorSceneRes,
    //                         meshName);

    //                     if (File.Exists (meshPath))
    //                     {
    //                         Material mat = spoi.mat;
    //                         if (mat != null)
    //                         {
    //                             CustomSceneObject so = new CustomSceneObject ();
    //                             so.spoi = spoi;
    //                             so.worldMatrix = Matrix4x4.TRS (spoi.pos, spoi.rotate, spoi.scale);
    //                             so.resPath = meshName;
    //                             ssci.AddResName (so.resPath);
    //                             AddResReDirct (ssci,
    //                                 string.Format ("{0}{1}/", AssetsConfig.instance.ResourcePath,
    //                                     AssetsConfig.instance.EditorSceneRes),
    //                                 string.Format ("{0}.asset", meshName));

    //                             so.id = ssci.objID++;

    //                             so.SetFlag (SceneObject.IgnoreShadowCaster, spoi.HasFlag (ObjectFlag.IgnoreShadowCaster));
    //                             so.SetFlag (SceneObject.IsHideRender, spoi.HasFlag (ObjectFlag.HideRender));
    //                             so.SetFlag (SceneObject.IgnoreShadowReceive, spoi.HasFlag (ObjectFlag.IgnoreShadowReceive));

    //                             so.LightmapId = ushort.MaxValue;
    //                             string lightmapHash = LightmapVolumn.CalcLightmapHash (prefabInfo.prefab.name, spoi.pos, spoi.rotate, spoi.scale);
    //                             LightmapObjectInfo lo;
    //                             if (ssci.lightmapObjects.TryGetValue (lightmapHash, out lo))
    //                             {
    //                                 so.SetFlag (SceneObject.HasLightmap, true);
    //                                 if (lo.resRef != null)
    //                                 {
    //                                     so.LightmpaName = lo.resRef.color != null?lo.resRef.color.name: "";
    //                                     so.LightmapId = lo.id;
    //                                 }
    //                             }

    //                             sceneObjects.Add (so);

    //                         }
    //                     }
    //                     else
    //                     {
    //                         DebugLog.AddErrorLog2 ("mesh not export:{0} fbx:{1}", meshPath, prefabInfo.prefab.name);
    //                     }
    //                 }

    //             }
    //         }

    //         private static void PreSaveDynamicObject (SceneSharedChunkInfo ssci, EditorChunkData ecd,
    //             EditorChunkData.GameObjectInfo goi)
    //         {
    //             if (goi.subPrefabChunkInfo.Count == 1)
    //             {
    //                 Vector4Int chunkID = goi.subPrefabChunkInfo[0];
    //                 if (chunkID.z < ecd.dynamicObjectDatas.Count)
    //                 {
    //                     var dynamicObjectData = ecd.dynamicObjectDatas[chunkID.z];

    //                     DynamicObject so = new DynamicObject ();
    //                     so.goi = goi;
    //                     so.dod = dynamicObjectData;

    //                     so.worldMatrix = Matrix4x4.TRS (goi.pos, goi.rotate, goi.scale);

    //                     uint hash = 0;
    //                     so.id = EngineUtility.XHashLowerRelpaceDot (hash, dynamicObjectData.hashStr.ToString ());
    //                     so.flag = dynamicObjectData.flag;
    //                     ssci.AddResName (so.dod.exString);
    //                     for (int i = 0; i < dynamicObjectData.effects.Count; ++i)
    //                     {
    //                         var effect = dynamicObjectData.effects[i];
    //                         if (effect.prefab != null)
    //                             effect.path = GetPrefabPath (ssci, effect.prefab);
    //                     }

    //                     ssci.chunks[chunkID.x].dynamicObjects.Add (so);
    //                 }

    //             }
    //         }
    //         private static void PreSaveAirWallObject (SceneSharedChunkInfo ssci, EditorChunkData ecd,
    //             EditorChunkData.GameObjectInfo goi)
    //         {
    //             if (goi.subPrefabChunkInfo.Count == 1 &&
    //                 goi.tag == "AirWall")
    //             {
    //                 Vector4Int chunkID = goi.subPrefabChunkInfo[0];
    //                 if (chunkID.z < ecd.colliderDatas.Count)
    //                 {
    //                     var colliderData = ecd.colliderDatas[chunkID.z];
    //                     if (colliderData.colliderType == EditorChunkData.ColliderType.Box)
    //                     {
    //                         Vector3 size = colliderData.size;
    //                         size.x *= goi.scale.x;
    //                         size.y *= goi.scale.y;
    //                         size.z *= goi.scale.z;

    //                         Vector3 center = colliderData.center + goi.pos;
    //                         Vector3 forward = goi.rotate * Vector3.forward;
    //                         Vector3 right = goi.rotate * Vector3.right;

    //                         Vector3 upOffset = Vector3.up * size.y * 0.5f;
    //                         Vector3 forwardOffset = forward * size.z * 0.5f;
    //                         Vector3 rightOffset = right * size.x * 0.5f;

    //                         CameraWall wall = new CameraWall ();
    //                         wall.corner0 = upOffset - forwardOffset - rightOffset + center;
    //                         wall.corner1 = upOffset - forwardOffset + rightOffset + center;
    //                         wall.corner2 = upOffset + forwardOffset + rightOffset + center;
    //                         wall.corner3 = upOffset + forwardOffset - rightOffset + center;

    //                         Vector3 corner0 = -upOffset - forwardOffset - rightOffset + center;
    //                         Vector3 corner1 = -upOffset - forwardOffset + rightOffset + center;

    //                         Vector3 corner2 = -upOffset + forwardOffset + rightOffset + center;
    //                         Vector3 corner3 = -upOffset + forwardOffset - rightOffset + center;

    //                         wall.normal0 = -forward;
    //                         Plane plane = new Plane (wall.corner0, wall.corner1, corner1);
    //                         wall.distance0 = plane.distance;
    //                         wall.normal0 = plane.normal;

    //                         wall.normal1 = right;
    //                         plane = new Plane (wall.corner1, wall.corner2, corner2);
    //                         wall.distance1 = plane.distance;
    //                         wall.normal1 = plane.normal;
    //                         wall.normal2 = forward;
    //                         plane = new Plane (wall.corner2, wall.corner3, corner3);
    //                         wall.distance2 = plane.distance;
    //                         wall.normal2 = plane.normal;
    //                         wall.normal3 = -right;
    //                         plane = new Plane (wall.corner3, wall.corner0, corner0);
    //                         wall.distance3 = plane.distance;
    //                         wall.pos.z = (size.x * size.x + size.z * size.z) * 3;
    //                         wall.normal3 = plane.normal;
    //                         wall.corner1.y = corner1.y;

    //                         int chunkLevel;
    //                         int blockId = 0;
    //                         int x = chunkID.x % ssci.widthCount;
    //                         int z = chunkID.x / ssci.widthCount;
    //                         SceneQuadTree.CalcBlockId (size, center, x, z, ssci.chunkWidth, ssci.chunkHeight, out chunkLevel, out blockId, true);
    //                         wall.blockId = (byte) SceneQuadTree.LocalBlockId2World (blockId);
    //                         ssci.chunks[chunkID.x].walls.Add (wall);
    //                     }
    //                 }
    //             }
    //         }

    //         private static void PreSaveInteractiveObject (SceneSharedChunkInfo ssci, EditorChunkData ecd,
    //             EditorChunkData.GameObjectInfo goi)
    //         {
    //             if (goi.subPrefabChunkInfo.Count == 1 &&
    //                 goi.tag == "Interactive")
    //             {
    //                 Vector4Int chunkID = goi.subPrefabChunkInfo[0];
    //                 if (chunkID.z < ecd.colliderDatas.Count)
    //                 {
    //                     var colliderData = ecd.colliderDatas[chunkID.z];
    //                     if (colliderData.colliderType == EditorChunkData.ColliderType.Box)
    //                     {
    //                         Vector3 size = colliderData.size;
    //                         size.x *= goi.scale.x;
    //                         size.y *= goi.scale.y;
    //                         size.z *= goi.scale.z;

    //                         InteractiveItem box = new InteractiveItem ();
    //                         box.center = colliderData.center + goi.pos;
    //                         box.forward = goi.rotate * Vector3.forward;
    //                         box.right = goi.rotate * Vector3.right;
    //                         box.up = goi.rotate * Vector3.up;
    //                         box.size = size;
    //                         box.exString = colliderData.exString;
    //                         ssci.AddResName (box.exString);

    //                         ssci.chunks[chunkID.x].interactiveList.Add (box);
    //                     }
    //                 }
    //             }
    //         }

    //         private static void PreSaveEnvObject (SceneSharedChunkInfo ssci, EditorChunkData ecd,
    //             EditorChunkData.GameObjectInfo goi)
    //         {
    //             if (goi.subPrefabChunkInfo.Count == 1)
    //             {
    //                 Vector4Int chunkID = goi.subPrefabChunkInfo[0];
    //                 var envData = ecd.envProfiles[chunkID.z];
    //                 if (envData != null && envData.settings.Count > 0)
    //                 {
    //                     EditorEnvArea envArea = new EditorEnvArea ();
    //                     envArea.envProfile = envData;
    //                     for (int i = 0; i < envData.areaList.Count; ++i)
    //                     {
    //                         var envBox = envData.areaList[i];
    //                         Vector3 worldPos = envBox.center + goi.pos;
    //                         BoxAreaInfo boxArea;
    //                         float cosA = Mathf.Cos (envBox.rotY * Mathf.Deg2Rad);
    //                         float sinA = Mathf.Sin (envBox.rotY * Mathf.Deg2Rad);
    //                         boxArea.box = new Vector4 (worldPos.x, worldPos.z, cosA, sinA);
    //                         float halfY = envBox.size.y * 0.5f;
    //                         boxArea.size = new Vector4 (envBox.size.x * 0.5f, envBox.size.z * 0.5f, worldPos.y - halfY, worldPos.y + halfY);
    //                         envArea.areaList.Add (boxArea);
    //                     }
    //                     ssci.envData.Add (envArea);
    //                 }
    //             }
    //         }

    //         private static void PreBindChunkRes (SceneSharedChunkInfo ssci, EditorChunkData ecd,
    //             EditorChunkData.EditorChunk editorChunk, SceneChunkInfo saveChunk, int x, int z, int i)
    //         {
    //             var terrainObjData = ssci.sceneConfig.chunks[i].terrainObjData;
    //             if (terrainObjData.isValid)
    //             {
    //                 int texCount = 0;
    //                 byte splatCount = (byte) ssci.sceneConfig.bundles.Count;
    //                 var terrainChunk = ssci.sceneConfig.chunks[i];
    //                 for (int j = 0; j < terrainChunk.splatID.Length; ++j)
    //                 {
    //                     byte splatID = terrainChunk.splatID[j];
    //                     if (splatID != 255 && splatID >= splatCount)
    //                     {

    //                     }
    //                     else
    //                     {
    //                         texCount++;
    //                     }
    //                 }

    //                 if (texCount > 1)
    //                 {
    //                     AddResReDirct (ssci,
    //                         string.Format ("{0}/Scene/{1}/", AssetsConfig.instance.ResourcePath, ssci.context.name),
    //                         string.Format ("Chunk_{0}_{1}_Blend.tga", x, z));
    //                 }
    //                 AddResReDirct (ssci,
    //                     string.Format ("{0}/Scene/{1}/", AssetsConfig.instance.ResourcePath, ssci.context.name),
    //                     string.Format ("Chunk_{0}_{1}.asset", x, z));
    //             }
    //         }

    //         private static void PreBindLightMap (SceneSharedChunkInfo ssci,
    //             LightmapVolumnData lvd)
    //         {
    //             ushort lightmapId = 0;
    //             for (int i = 0; i < lvd.volumns.Count; ++i)
    //             {
    //                 var volumn = lvd.volumns[i];
    //                 for (int j = 0; j < volumn.ligthmapRes.Count; ++j)
    //                 {
    //                     var res = volumn.ligthmapRes[j];
    //                     if (res.color != null)
    //                     {
    //                         ssci.AddResName (res.color.name);
    //                     }
    //                 }
    //                 for (int j = 0; j < volumn.lightmapObjects.Count; ++j)
    //                 {
    //                     var lo = volumn.lightmapObjects[j];
    //                     string hash = EditorCommon.GetReplaceStr (lo.hashStr);
    //                     if (!ssci.lightmapObjects.ContainsKey (hash))
    //                     {
    //                         int index = lo.lightmapIndex;
    //                         if (index >= 0 && index < volumn.ligthmapRes.Count)
    //                         {
    //                             lo.resRef = volumn.ligthmapRes[index];
    //                             lo.id = lightmapId++;
    //                         }
    //                         ssci.lightmapObjects[lo.hashStr] = lo;
    //                     }
    //                     else
    //                     {
    //                         DebugLog.AddErrorLog2 ("already has lightmap object", hash);
    //                     }

    //                 }
    //             }
    //         }

    //         private static void PreBindLight (SceneSharedChunkInfo ssci,
    //             SceneChunkInfo saveChunk, int index)
    //         {
    //             var cld = ssci.cld;
    //             if (cld != null && cld.chunkLightIndex != null && index < cld.chunkLightIndex.Length)
    //             {
    //                 var chunkLightIndex = cld.chunkLightIndex[index];
    //                 for (int i = 0; i < chunkLightIndex.lightIndex.Count; ++i)
    //                 {
    //                     var lightIndex = chunkLightIndex.lightIndex[i];
    //                     var lightData = cld.lights[lightIndex];
    //                     Vector4 color = new Vector4 (
    //                         Mathf.Pow (lightData.c.r * lightData.intensity, 2.2f),
    //                         Mathf.Pow (lightData.c.g * lightData.intensity, 2.2f),
    //                         Mathf.Pow (lightData.c.b * lightData.intensity, 2.2f), 1.0f / lightData.range);
    //                     saveChunk.pointLights.Add (new LightingInfo ()
    //                     {

    //                         posRange = new Vector4 (lightData.pos.x, lightData.pos.y, lightData.pos.z, lightData.rangeBias),
    //                             color = color,
    //                     });
    //                 }
    //             }
    //         }

    //         private static void PreBindInstance (SceneSharedChunkInfo ssci, EditorChunkData ecd,
    //             EditorChunkData.EditorChunk editorChunk, SceneChunkInfo saveChunk, int x, int z, int i)
    //         {
    //             for (int j = 0; j < editorChunk.blocks.Length; ++j)
    //             {
    //                 var ob = editorChunk.blocks[j];

    //                 for (int k = 0; k < ob.instanceObjects.Count; ++k)
    //                 {
    //                     var io = ob.instanceObjects[k];
    //                     if (io.prefabIndex >= 0 && io.prefabIndex < ecd.prefabInfos.Count &&
    //                         io.instanceInfo.Count > 0)
    //                     {
    //                         var prefabInfo = ecd.prefabInfos[io.prefabIndex];
    //                         string meshPath = string.Format ("{0}{1}/{2}_0.asset",
    //                             AssetsConfig.instance.ResourcePath,
    //                             AssetsConfig.instance.EditorSceneRes,
    //                             prefabInfo.prefab.name);
    //                         if (File.Exists (meshPath))
    //                         {
    //                             uint indexOffset;
    //                             if (!ssci.instanceMeshMap.TryGetValue (meshPath, out indexOffset))
    //                             {
    //                                 Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh> (meshPath);
    //                                 indexOffset = mesh.GetIndexCount (0);
    //                                 ssci.instanceMeshMap.Add (meshPath, indexOffset);
    //                             }

    //                             if (saveChunk.instanceInfos == null)
    //                             {
    //                                 saveChunk.instanceInfos = new List<InstanceInfo> ();
    //                             }
    //                             if (saveChunk.argArray == null)
    //                             {
    //                                 saveChunk.argArray = new List<uint> ();
    //                             }
    //                             short instanceCount = 0;
    //                             Bounds aabb = new Bounds ();
    //                             for (int xx = 0; xx < io.instanceInfo.Count; ++xx)
    //                             {
    //                                 var ip = io.instanceInfo[xx];
    //                                 if (ip.visible)
    //                                 {
    //                                     saveChunk.instanceInfos.Add (
    //                                         new InstanceInfo ()
    //                                         {
    //                                             posScale = new Vector4 (ip.pos.x, ip.pos.y, ip.pos.z, ip.scale),
    //                                                 rot = new Vector4 (ip.rot.x, ip.rot.y, ip.rot.z, ip.rot.w)
    //                                         });

    //                                     if (instanceCount == 0)
    //                                     {
    //                                         aabb = ip.aabb;
    //                                     }
    //                                     else
    //                                     {
    //                                         aabb.Encapsulate (ip.aabb);
    //                                     }
    //                                     instanceCount++;
    //                                 }
    //                             }
    //                             if (instanceCount > 0)
    //                             {
    //                                 saveChunk.argArray.Add (indexOffset);
    //                                 saveChunk.argArray.Add ((uint) instanceCount);
    //                                 saveChunk.argArray.Add (0);
    //                                 saveChunk.argArray.Add (0);
    //                                 saveChunk.argArray.Add (0);

    //                                 InstanceObject so = new InstanceObject ();
    //                                 so.instanceCount = instanceCount;
    //                                 so.instanceID = ssci.instanceID++;
    //                                 so.sceneAABB = aabb;
    //                                 int chunkLevel;
    //                                 int blockId = 0;
    //                                 SceneQuadTree.CalcBlockId (aabb.size, aabb.center, x, z, ssci.chunkWidth, ssci.chunkHeight, out chunkLevel, out blockId);
    //                                 so.blockID = blockId;
    //                                 so.mat = io.mat;
    //                                 so.billboardMeshPath = string.Format ("{0}_{1}_{2}", i, j, prefabInfo.prefab.name);
    //                                 ssci.AddResName (so.billboardMeshPath);
    //                                 //CustomSceneObject
    //                                 // string fileID = string.Format ("{0}_{1}_{2}_{3}", prefabInfo.prefab.name, i, j, k);
    //                                 // so.fileID = fileID;
    //                                 // so.LightmapST = saveChunk.terrainLightmapST;
    //                                 // so.LightmapIndex = saveChunk.terrainLightmapIndex;
    //                                 //BaseSceneObject
    //                                 so.resPath = prefabInfo.prefab.name + "_0";
    //                                 ssci.AddResName (so.resPath);
    //                                 AddResReDirct (ssci,
    //                                     string.Format ("{0}{1}/",
    //                                         AssetsConfig.instance.ResourcePath,
    //                                         AssetsConfig.instance.EditorSceneRes),
    //                                     string.Format ("{0}_0.asset", prefabInfo.prefab.name));
    //                                 so.id = ssci.objID++;
    //                                 saveChunk.sceneObjects.Add (so);
    //                             }
    //                         }
    //                     }
    //                 }
    //             }

    //         }

    //         private static void PreInitQuadTree (SceneSharedChunkInfo ssci, EditorChunkData ecd,
    //             EditorChunkData.EditorChunk editorChunk, SceneChunkInfo saveChunk, int x, int z, int i)
    //         {
    //             Vector3 chunkOffset = new Vector3 (x * ssci.chunkWidth, 0, z * ssci.chunkHeight);
    //             SceneQuadTree.InitQuadTree (ssci.treeContext, ssci.chunkWidth, ssci.chunkHeight, chunkOffset);
    //             var terrainChunk = ssci.sceneConfig.chunks[i];
    //             if (terrainChunk.mesh != null)
    //             {
    //                 SceneQuadBlock sceneQuadBlock = new SceneQuadBlock ();
    //                 Bounds bound = terrainChunk.mesh.bounds;
    //                 Vector3 center = bound.center;
    //                 Vector3 size = bound.size;
    //                 if (size.y == 0)
    //                     size.y = 0.1f;

    //                 center.x = (x + 0.5f) * ssci.chunkWidth;
    //                 center.z = (z + 0.5f) * ssci.chunkHeight;
    //                 bound.center = center;
    //                 bound.size = size;
    //                 sceneQuadBlock.aabb.Init (ref bound);
    //                 saveChunk.sceneQuadBlocks[0] = sceneQuadBlock;
    //                 EditorQuardTreeNode node = ssci.treeContext.treeNodes[0];
    //                 node.aabb = bound;
    //             }
    //         }

    //         private static void PreBindObject (SceneSharedChunkInfo ssci, BaseSceneObject so)
    //         {
    //             IMatObject mo = so as IMatObject;
    //             if (mo != null)
    //             {
    //                 ssci.matSaveData.FindOrAddMatInfo (mo.Mat);
    //             }
    //         }

    //         private static void PreBindObject (SceneSharedChunkInfo ssci, EditorChunkData ecd,
    //             EditorChunkData.EditorChunk editorChunk, SceneChunkInfo saveChunk)
    //         {
    //             for (int j = 0; j < saveChunk.sceneObjects.Count; ++j)
    //             {
    //                 var so = saveChunk.sceneObjects[j];
    //                 so.id = ssci.objID++;
    //                 PreBindObject (ssci, so);
    //                 IQuadTreeObject quadTreeObj = so as IQuadTreeObject;
    //                 if (quadTreeObj != null)
    //                 {
    //                     quadTreeObj.QuadNodeId = j;
    //                     if (quadTreeObj.BlockId >= 0)
    //                         SceneQuadTree.Add2QuadTree (ssci.treeContext, quadTreeObj, quadTreeObj.bounds);
    //                 }
    //             }
    //         }

    //         private static void PreBindQuadTree (SceneSharedChunkInfo ssci, EditorChunkData ecd,
    //             EditorChunkData.EditorChunk editorChunk, SceneChunkInfo saveChunk)
    //         {
    //             SceneQuadTree.EndQuadTree (ssci.treeContext);

    //             for (int j = 0; j < ssci.treeContext.treeNodes.Length; ++j)
    //             {
    //                 EditorQuardTreeNode node = ssci.treeContext.treeNodes[j];
    //                 if (node.hasData)
    //                 {
    //                     SceneQuadBlock sqb = new SceneQuadBlock ();
    //                     sqb.envIndex = 255;
    //                     sqb.quadIndex = (byte) j;
    //                     sqb.aabb.Init (ref node.aabb);
    //                     sqb.flag = 0;
    //                     if (node.data.Count > 0)
    //                     {
    //                         sqb.sceneObjectGroupIndex = (short) saveChunk.sceneObjectIndex.Count;
    //                         // Debug.LogError("node:"+j.ToString());
    //                         saveChunk.sceneObjectIndex.Add ((ushort) node.data.Count);
    //                         for (int kk = 0; kk < node.data.Count; ++kk)
    //                         {
    //                             var data = node.data[kk];
    //                             saveChunk.sceneObjectIndex.Add ((ushort) data.QuadNodeId);
    //                         }
    //                     }
    //                     saveChunk.sceneQuadBlocks[sqb.quadIndex] = sqb;
    //                 }
    //             }
    //         }
    //         private static void PreBindGlobalObject (SceneSharedChunkInfo ssci)
    //         {
    //             for (int j = 0; j < ssci.globalSceneObjects.Count; ++j)
    //             {
    //                 var so = ssci.globalSceneObjects[j];
    //                 PreBindObject (ssci, so);
    //                 // IMatObject mo = so as IMatObject;
    //                 // if (mo != null)
    //                 //     ssci.hasGlobalLightmap |= mo.LightmapIndex != 255;
    //             }
    //         }

    //         private static void PreSave (SceneSharedChunkInfo ssci, EditorChunkData ecd, LightmapVolumnData lvd)
    //         {
    //             PreBindLightMap (ssci, lvd);
    //             for (int i = 0; i < ecd.gameObjectInfos.Count; ++i)
    //             {
    //                 var goi = ecd.gameObjectInfos[i];
    //                 {
    //                     if (goi.HasFlag (ObjectFlag.GameObjectActiveInHierarchy) &&
    //                         goi.prefabIndex >= 0 && goi.prefabIndex < ecd.prefabInfos.Count)
    //                     {
    //                         var prefabInfo = ecd.prefabInfos[goi.prefabIndex];
    //                         if (prefabInfo.prefab != null)
    //                         {
    //                             if (goi.prefabType == (uint) EditorSceneObjectType.Effect)
    //                             {
    //                                 PreSavePrefab (ssci, ecd, goi, prefabInfo);
    //                             }
    //                             else if (!ssci.sceneConfig.useCombineMesh &&
    //                                 (goi.prefabType == (uint) EditorSceneObjectType.Prefab ||
    //                                     goi.prefabType == (uint) EditorSceneObjectType.StaticPrefab))
    //                             {
    //                                 PreSaveMeshPrefab (ssci, ecd, goi, prefabInfo);
    //                             }
    //                         }
    //                     }
    //                     else if (goi.prefabType == (uint) EditorSceneObjectType.DynamicObject)
    //                     {
    //                         PreSaveDynamicObject (ssci, ecd, goi);
    //                     }
    //                     else if (goi.HasFlag (ObjectFlag.GameObjectActiveInHierarchy) &&
    //                         goi.prefabType == (uint) EditorSceneObjectType.Collider)
    //                     {
    //                         PreSaveAirWallObject (ssci, ecd, goi);
    //                         PreSaveInteractiveObject (ssci, ecd, goi);
    //                     }
    //                     else if (goi.HasFlag (ObjectFlag.GameObjectActiveInHierarchy) &&
    //                         goi.prefabType == (uint) EditorSceneObjectType.Enverinment)
    //                     {
    //                         PreSaveEnvObject (ssci, ecd, goi);
    //                     }
    //                 }

    //             }
    //             if (ssci.sceneConfig.useCombineMesh)
    //             {

    //             }

    //             for (int i = 0; i < ecd.chunks.Count; ++i)
    //             {
    //                 var editorChunk = ecd.chunks[i];
    //                 var lightmapChunk = i < lvd.volumns.Count?lvd.volumns[i] : null;
    //                 var saveChunk = ssci.chunks[i];
    //                 int sceneObjectCount = saveChunk.sceneObjects.Count;
    //                 int x = i % ssci.widthCount;
    //                 int z = i / ssci.widthCount;
    //                 PreBindChunkRes (ssci, ecd, editorChunk, saveChunk, x, z, i);
    //                 PreBindLight (ssci, saveChunk, i);
    //                 //instance objects
    //                 PreBindInstance (ssci, ecd, editorChunk, saveChunk, x, z, i);
    //                 saveChunk.instanceCount = saveChunk.sceneObjects.Count - sceneObjectCount;
    //                 PreInitQuadTree (ssci, ecd, editorChunk, saveChunk, x, z, i);

    //                 //sort scene object by material
    //                 PreBindObject (ssci, ecd, editorChunk, saveChunk);

    //                 //save scene bolcks
    //                 PreBindQuadTree (ssci, ecd, editorChunk, saveChunk);
    //             }
    //             PreBindGlobalObject (ssci);
    //             ssci.matSaveData.Sort ();
    //         }
    //         private static void SaveAABB (BinaryWriter bw, Bounds aabb)
    //         {
    //             EditorCommon.WriteVector (bw, aabb.min);
    //             EditorCommon.WriteVector (bw, aabb.max);
    //         }

    //         private static void SaveAABB (BinaryWriter bw, ref Bounds aabb)
    //         {
    //             EditorCommon.WriteVector (bw, aabb.min);
    //             EditorCommon.WriteVector (bw, aabb.max);
    //         }
    //         private static void SaveAABB (BinaryWriter bw, ref AABB aabb)
    //         {
    //             EditorCommon.WriteVector (bw, aabb.min);
    //             EditorCommon.WriteVector (bw, aabb.max);
    //         }
    //         private static void PreSaveExternalData (SceneSharedChunkInfo ssci)
    //         {
    //             EditorCommon.CallInternalFunction (typeof (SceneSaveExternal), "Init", true, false, false, null, null);
    //             if (preSaveExternalDataCb != null)
    //             {
    //                 preSaveExternalDataCb (ssci);
    //             }
    //         }

    //         private static void SaveStringIndex (BinaryWriter bw, SceneSharedChunkInfo ssci, string str)
    //         {
    //             short index = (short) ssci.resNames.IndexOf (str);
    //             bw.Write (index);
    //         }

    //         private static void SaveHeadData (BinaryWriter bw,
    //             SceneSharedChunkInfo ssci,
    //             SceneConfig sceneConfig,
    //             string[] dynamicSceneNames)
    //         {
    //             bw.Write (EngineContext.VersionLatest);
    //             bw.Write (sceneConfig.widthCount * EngineContext.ChunkSize);
    //             bw.Write (sceneConfig.heightCount * EngineContext.ChunkSize);
    //             bw.Write (ssci.chunkWidth);
    //             bw.Write (ssci.chunkHeight);
    //             bw.Write (ssci.widthCount);
    //             bw.Write (ssci.heightCount);
    //             int dynamicSceneCount = dynamicSceneNames != null ? dynamicSceneNames.Length : 0;
    //             bw.Write (dynamicSceneCount);
    //             for (int i = 0; i < dynamicSceneCount; ++i)
    //             {
    //                 bw.Write (dynamicSceneNames[i]);
    //             }
    //             bw.Write (ssci.ChunkInfo.Length);
    //             ssci.chunkStreamPos = (int) bw.BaseStream.Length;
    //             for (int i = 0; i < ssci.ChunkStreamOffset.Length; ++i)
    //             {
    //                 bw.Write (ssci.ChunkStreamOffset[i]);
    //             }
    //             // bw.Write (ssci.ChunkInfo.Length);
    //             ssci.chunkInfoPos = (int) bw.BaseStream.Length;
    //             for (int i = 0; i < ssci.ChunkInfo.Length; ++i)
    //             {
    //                 bw.Write (ssci.ChunkInfo[i]);
    //             }
    //         }

    //         private static void PostSaveHeadData (BinaryWriter bw,
    //             SceneSharedChunkInfo ssci)
    //         {
    //             bw.Seek (ssci.chunkStreamPos, SeekOrigin.Begin);
    //             for (int i = 0; i < ssci.ChunkStreamOffset.Length; ++i)
    //             {
    //                 bw.Write (ssci.ChunkStreamOffset[i]);
    //             }
    //             bw.Seek (ssci.chunkInfoPos, SeekOrigin.Begin);
    //             for (int i = 0; i < ssci.ChunkInfo.Length; ++i)
    //             {
    //                 bw.Write (ssci.ChunkInfo[i]);
    //             }
    //         }
    //         private static void SaveHeadString (BinaryWriter bw, SceneSharedChunkInfo ssci)
    //         {
    //             bw.Write (ssci.resNames.Count);
    //             for (int i = 0; i < ssci.resNames.Count; ++i)
    //             {
    //                 bw.Write (ssci.resNames[i]);
    //             }
    //         }
    //         private static void SaveHeadTerrainSplat (BinaryWriter bw, SceneSharedChunkInfo ssci, SceneConfig sceneConfig)
    //         {
    //             byte splatCount = (byte) sceneConfig.bundles.Count;
    //             bw.Write (splatCount);
    //             for (int i = 0; i < splatCount; ++i)
    //             {
    //                 SceneConfig.TextureInfo ti = sceneConfig.bundles[i];
    //                 string name = ti.tex != null ? ti.tex.name : "";
    //                 bw.Write (name);
    //                 bw.Write (ti.scale);
    //                 if (ti.tex != null)
    //                     AddResReDirct (ssci, ti.tex, name);
    //                 string normalname = ti.pbs != null ? ti.pbs.name : "";
    //                 bw.Write (normalname);
    //                 if (ti.pbs != null)
    //                     AddResReDirct (ssci, ti.pbs, normalname);
    //             }
    //             if (splatCount > 0)
    //                 EditorCommon.WriteVector (bw, sceneConfig.terrainParam);
    //             bool hasBaseMap = sceneConfig.baseMapMesh != null && sceneConfig.baseMapTex != null;
    //             bw.Write (hasBaseMap);
    //             AddResReDirct (ssci, string.Format ("{0}/Scene/{1}/", AssetsConfig.instance.ResourcePath, ssci.context.name), "BaseMap.asset", false);
    //             AddResReDirct (ssci, string.Format ("{0}/Scene/{1}/", AssetsConfig.instance.ResourcePath, ssci.context.name), "BaseMap.png", false);
    //         }

    //         private static void SaveHeadMaterial (BinaryWriter bw, SceneSharedChunkInfo ssci)
    //         {
    //             byte materialGroupCount = (byte) ssci.matSaveData.matInfo.Count;
    //             bw.Write (materialGroupCount);
    //             for (ushort j = 0; j < materialGroupCount; ++j)
    //             {
    //                 var mi = ssci.matSaveData.matInfo[j];
    //                 var context = mi.context;
    //                 // bw.Write(j);
    //                 bw.Write (context.flag);

    //                 byte resCount = (byte) context.textureValue.Count;
    //                 bw.Write (resCount);
    //                 for (int i = 0; i < resCount; ++i)
    //                 {
    //                     var stpv = context.textureValue[i];
    //                     byte index = (byte) stpv.shaderID;
    //                     bw.Write (index);
    //                     if (stpv.shaderID >= 0)
    //                     {
    //                         if (stpv.shaderID > ShaderManager._ShaderKeyEffectKey.Length)
    //                         {
    //                             SaveStringIndex (bw, ssci, stpv.shaderKeyName);
    //                         }
    //                         SaveStringIndex (bw, ssci, stpv.path);
    //                     }

    //                 }
    //                 // long tmpPos = bw.BaseStream.Position - ssci.startPos;
    //                 byte shaderPropertyCount = (byte) context.shaderIDs.Count;
    //                 bw.Write (shaderPropertyCount);
    //                 for (int i = 0; i < shaderPropertyCount; ++i)
    //                 {
    //                     var spv = context.shaderIDs[i];
    //                     byte index = (byte) spv.shaderID;
    //                     bw.Write (index);
    //                     if (index > ShaderManager._ShaderKeyEffectKey.Length)
    //                     {
    //                         SaveStringIndex (bw, ssci, spv.shaderKeyName);
    //                     }
    //                     EditorCommon.WriteVector (bw, spv.value);
    //                     //ssci.debugLogStream.AppendFormatStr (string.Format ("shader property type:{0}_{1}", i, bw.BaseStream.Position - ssci.startPos));
    //                 }
    //                 //ssci.debugLogStream.AppendFormatStr (string.Format ("head mat:{0}_{1}:{2}:{3}", j, mi.mat.name, tmpPos, bw.BaseStream.Position - ssci.startPos));
    //             }
    //         }

    //         private static void SaveEnvData (BinaryWriter bw, SceneSharedChunkInfo ssci, EnvProfile profile)
    //         {
    //             var settings = profile.GetRuntimeSettings ();
    //             byte settingCount = (byte) settings.Count;
    //             bw.Write (settingCount);
    //             for (int j = 0; j < settingCount; ++j)
    //             {
    //                 var setting = settings[j];
    //                 byte settingType = (byte) setting.GetEnvType ();
    //                 bw.Write (settingType);
    //                 var saver = RenderingManager.saveEnv[settingType];
    //                 if (saver != null)
    //                 {
    //                     saver (bw, setting);
    //                 }
    //                 //ssci.debugLogStream.AppendFormatStr (string.Format ("env {0}:{1}", settingType, bw.BaseStream.Position - ssci.startPos));
    //             }
    //         }

    //         private static void SaveHeadEnvData (BinaryWriter bw, SceneSharedChunkInfo ssci, EditorChunkData editorChunkData)
    //         {
    //             byte globalEnv = 0;
    //             EnvProfile globalEnvProfile = null;
    //             string profilePath = string.Format ("{0}/{1}_Profiles.asset",
    //                 ssci.context.configDir,
    //                 ssci.context.name);
    //             if (File.Exists (profilePath))
    //             {
    //                 globalEnvProfile = AssetDatabase.LoadAssetAtPath<EnvProfile> (profilePath);
    //                 if (globalEnvProfile != null && globalEnvProfile.settings.Count > 0)
    //                 {
    //                     globalEnv = 1;
    //                 }
    //             }

    //             bw.Write (globalEnv);
    //             byte envCount = (byte) ssci.envData.Count;
    //             bw.Write (envCount);
    //             if (globalEnv == 1)
    //             {
    //                 SaveEnvData (bw, ssci, globalEnvProfile);
    //             }

    //             for (int i = 0; i < envCount; ++i)
    //             {
    //                 var envArea = ssci.envData[i];
    //                 byte index = (byte) editorChunkData.dynamicSceneNames.IndexOf (envArea.envProfile.dynamicSceneName);
    //                 bw.Write (index);
    //             }

    //             for (int i = 0; i < envCount; ++i)
    //             {
    //                 var envArea = ssci.envData[i];
    //                 var profile = envArea.envProfile;
    //                 byte index = (byte) editorChunkData.dynamicSceneNames.IndexOf (profile.dynamicSceneName);
    //                 bw.Write (index);
    //                 ushort size = 0;
    //                 long sizePos = bw.BaseStream.Position;
    //                 bw.Write (size);
    //                 long startPos = bw.BaseStream.Position;
    //                 SaveEnvData (bw, ssci, profile);
    //                 int areaLength = envArea.areaList.Count * 8 + 1;
    //                 bw.Write (areaLength);
    //                 bw.Write (profile.lerpTime);
    //                 for (int j = 0; j < envArea.areaList.Count; ++j)
    //                 {
    //                     var box = envArea.areaList[j];
    //                     EditorCommon.WriteVector (bw, box.box);
    //                     EditorCommon.WriteVector (bw, box.size);
    //                 }

    //                 long endPos = bw.BaseStream.Position;
    //                 size = (ushort) (endPos - startPos);
    //                 bw.Seek ((int) sizePos, SeekOrigin.Begin);
    //                 bw.Write (size);
    //                 bw.Seek ((int) endPos, SeekOrigin.Begin);

    //             }
    //         }

    //         private static void SaveHeadExternalData (BinaryWriter bw, SceneSharedChunkInfo ssci)
    //         {
    //             if (saveHeadExternalDataCb != null)
    //             {
    //                 saveHeadExternalDataCb (bw, ssci);
    //             }

    //         }
    //         private static void SaveHeadGlobalObject (BinaryWriter bw, SceneSharedChunkInfo ssci)
    //         {
    //             SaveChunkSceneObject (bw, ssci, ssci.globalSceneObjects, 0);
    //         }

    //         private static void SaveChunkTerrain (BinaryWriter bw, SceneSharedChunkInfo ssci,
    //             SceneConfig sceneConfig, SceneConfig.TerrainChunk chunk, SceneChunkInfo sci, int i)
    //         {
    //             byte splatCount = (byte) sceneConfig.bundles.Count;
    //             if (chunk != null)
    //             {
    //                 var terrainObject = chunk.terrainObjData;
    //                 ssci.ChunkInfo[i] = terrainObject.areaId;
    //                 bool valid = terrainObject.isValid && splatCount > 0;
    //                 bw.Write (valid);
    //                 if (valid)
    //                 {
    //                     string projMeshPath = string.Format ("{0}/Scene/{1}/ProjectionMeshBox_{2}_{3}.asset",
    //                         AssetsConfig.instance.ResourcePath, ssci.context.name, i % ssci.widthCount, i / ssci.widthCount);
    //                     bw.Write (System.IO.File.Exists (projMeshPath));
    //                     var terrainChunk = ssci.sceneConfig.chunks[i];

    //                     int x = i % ssci.widthCount;
    //                     int z = i / ssci.widthCount;
    //                     float offsetX = x * ssci.chunkWidth;
    //                     float offsetZ = z * ssci.chunkHeight;
    //                     Bounds aabb0 = terrainChunk.aabb0;
    //                     aabb0.center = aabb0.center + new Vector3 (offsetX, 0, offsetZ);
    //                     Bounds aabb1 = terrainChunk.aabb1;
    //                     aabb1.center = aabb1.center + new Vector3 (offsetX, 0, offsetZ);
    //                     Bounds aabb2 = terrainChunk.aabb2;
    //                     aabb2.center = aabb2.center + new Vector3 (offsetX, 0, offsetZ);
    //                     Bounds aabb3 = terrainChunk.aabb3;
    //                     aabb3.center = aabb3.center + new Vector3 (offsetX, 0, offsetZ);
    //                     SaveAABB (bw, ref aabb0);
    //                     if (!(ssci.widthCount == 1 && ssci.heightCount == 1))
    //                     {
    //                         SaveAABB (bw, ref aabb1);
    //                         SaveAABB (bw, ref aabb2);
    //                         SaveAABB (bw, ref aabb3);
    //                     }
    //                     // uint normalFlag = 0;
    //                     for (int j = 0; j < terrainChunk.splatID.Length; ++j)
    //                     {
    //                         byte splatID = terrainChunk.splatID[j];
    //                         if (splatID != 255 && splatID >= splatCount)
    //                         {
    //                             splatID = 255;
    //                             Debug.LogWarningFormat ("splat count error, total count:{0} chunk splat index:{1} chunk id:{2}", splatCount, splatID, i);
    //                         }
    //                         else { }
    //                         bw.Write (splatID);
    //                     }

    //                     bw.Write (sci.terrainLightmapIndex);
    //                     if (sci.terrainLightmapIndex != 255)
    //                         EditorCommon.WriteVector (bw, sci.terrainLightmapST);

    //                 }

    //             }
    //         }
    //         private static void SaveChunkRes (BinaryWriter bw,
    //             SceneSharedChunkInfo ssci, SceneChunkInfo sci, SceneConfig.TerrainChunk chunk)
    //         {
    //             int sceneObjectGroupCount = sci.sceneObjectIndex != null ? sci.sceneObjectIndex.Count : 0;
    //             bw.Write (sceneObjectGroupCount);
    //             for (int j = 0; j < sceneObjectGroupCount; ++j)
    //             {
    //                 bw.Write (sci.sceneObjectIndex[j]);
    //             }
    //             bool hasSceneObject = sci.sceneObjects.Count > 0 || sci.sceneQuadBlocks.Count > 0 || sci.walls.Count > 0;
    //             bw.Write (hasSceneObject);
    //         }

    //         private static void SaveChunkInstance (BinaryWriter bw, SceneSharedChunkInfo ssci, SceneChunkInfo sci)
    //         {
    //             int posCount = sci.instanceInfos != null ? sci.instanceInfos.Count : 0;
    //             bw.Write (posCount);
    //             for (int j = 0; j < posCount; ++j)
    //             {
    //                 var ii = sci.instanceInfos[j];
    //                 EditorCommon.WriteVector (bw, ii.posScale);
    //                 EditorCommon.WriteVector (bw, ii.rot);
    //             }
    //             int argCount = 0;
    //             if (posCount > 0)
    //             {
    //                 argCount = sci.argArray != null ? sci.argArray.Count : 0;
    //                 bw.Write (argCount);
    //                 for (int j = 0; j < argCount; ++j)
    //                 {
    //                     bw.Write (sci.argArray[j]);
    //                 }
    //             }
    //         }
    //         private static bool SaveSceneObjectBasicData (BinaryWriter bw, SceneSharedChunkInfo ssci, BaseSceneObject so)
    //         {
    //             var mo = so as IMatObject;
    //             bw.Write (so.id);
    //             bw.Write (so.flag);
    //             EditorCommon.WriteMatrix (bw, so.worldMatrix);
    //             SaveAABB (bw, so.bounds);
    //             SaveStringIndex (bw, ssci, so.resPath);

    //             ushort matId = mo != null?ssci.matSaveData.FindMatIndex (mo.Mat) : ushort.MaxValue;
    //             bw.Write (matId);
    //             if (matId != ushort.MaxValue)
    //             {
    //                 bw.Write (mo.LodDist);
    //                 if (so.HasFlag (SceneObject.HasLightmap) && mo != null)
    //                 {
    //                     SaveStringIndex (bw, ssci, mo.LightmpaName);
    //                     bw.Write (mo.LightmapId);
    //                 }
    //                 return true;
    //             }
    //             return false;

    //         }
    //         private static void SaveChunkSceneObject (
    //             BinaryWriter bw,
    //             SceneSharedChunkInfo ssci,
    //             List<BaseSceneObject> sceneObjects,
    //             int instanceCount)
    //         {
    //             ushort objCount = (ushort) (sceneObjects.Count - instanceCount);
    //             bw.Write (objCount);
    //             for (int j = 0; j < objCount; ++j)
    //             {
    //                 var so = sceneObjects[j];
    //                 SaveSceneObjectBasicData (bw, ssci, so);
    //             }
    //             ushort instanceObjCount = (ushort) instanceCount;
    //             bw.Write (instanceObjCount);
    //             for (int j = objCount; j < sceneObjects.Count; ++j)
    //             {
    //                 var so = sceneObjects[j];
    //                 if (SaveSceneObjectBasicData (bw, ssci, so))
    //                 {
    //                     var io = so as InstanceObject;
    //                     bw.Write (io.instanceCount);
    //                     bw.Write (io.instanceID);
    //                     SaveStringIndex (bw, ssci, io.billboardMeshPath);
    //                 }
    //                 else
    //                 {
    //                     DebugLog.AddErrorLog ("instance mat error");
    //                 }
    //             }
    //         }

    //         private static void SaveChunkQuadTree (BinaryWriter bw, SceneSharedChunkInfo ssci, SceneChunkInfo sci)
    //         {
    //             byte sqbCount = (byte) sci.sceneQuadBlocks.Count;
    //             bw.Write (sqbCount);
    //             var it = sci.sceneQuadBlocks.GetEnumerator ();
    //             while (it.MoveNext ())
    //             {
    //                 var kvp = it.Current;
    //                 var sqb = kvp.Value;
    //                 byte index = (byte) kvp.Key;
    //                 bw.Write (index);
    //                 SaveAABB (bw, ref sqb.aabb);
    //                 bw.Write (sqb.sceneObjectGroupIndex);

    //                 // ushort count = sqb.objectIndexes != null ? (ushort)sqb.objectIndexes.Count : (ushort)0;
    //                 // bw.Write(count);
    //                 // for (short j = 0; j < count; ++j)
    //                 // {
    //                 //     bw.Write(sqb.objectIndexes[j]);
    //                 // }
    //             }
    //         }

    //         private static void SaveChunkDynamicObject (BinaryWriter bw, SceneSharedChunkInfo ssci, EditorChunkData editorChunkData, SceneChunkInfo sci)
    //         {
    //             short dynmaicObjectCount = (short) sci.dynamicObjects.Count;
    //             bw.Write (dynmaicObjectCount);
    //             for (short j = 0; j < dynmaicObjectCount; ++j)
    //             {
    //                 DynamicObject so = sci.dynamicObjects[j];
    //                 EditorChunkData.DynamicObjectData dod = so.dod;
    //                 int index = editorChunkData.dynamicSceneNames.IndexOf (dod.dynamicSceneName);
    //                 byte dynamicSceneId = index >= 0 ? (byte) index : (byte) 255;
    //                 bw.Write (dynamicSceneId);
    //                 bw.Write (dod.flag);
    //                 bw.Write (so.id); //hash
    //                 byte objCount = (byte) dod.effects.Count;
    //                 bw.Write (objCount);
    //                 for (int i = 0; i < objCount; ++i)
    //                 {
    //                     var effect = dod.effects[i];
    //                     SaveStringIndex (bw, ssci, effect.path);
    //                     EditorCommon.WriteMatrix (bw, Matrix4x4.TRS (effect.pos, effect.rotate, effect.scale));
    //                     SaveAABB (bw, ref effect.bound);
    //                 }

    //                 if ((dod.flag & 0xff) != 0)
    //                 {
    //                     //has trigger
    //                     EditorCommon.WriteVector (bw, dod.pos0);
    //                     EditorCommon.WriteVector (bw, dod.pos1);
    //                     SaveStringIndex (bw, ssci, dod.exString);
    //                 }
    //             }
    //         }

    //         private static void SaveChunkWall (BinaryWriter bw, SceneSharedChunkInfo ssci, SceneChunkInfo sci)
    //         {
    //             short wallCount = (short) sci.walls.Count;
    //             bw.Write (wallCount);
    //             for (short j = 0; j < wallCount; ++j)
    //             {
    //                 CameraWall wall = sci.walls[j];
    //                 EditorCommon.WriteVector (bw, wall.corner0);
    //                 EditorCommon.WriteVector (bw, wall.corner1);
    //                 EditorCommon.WriteVector (bw, wall.corner2);
    //                 EditorCommon.WriteVector (bw, wall.corner3);
    //                 EditorCommon.WriteVector (bw, wall.normal0);
    //                 bw.Write (wall.distance0);
    //                 EditorCommon.WriteVector (bw, wall.normal1);
    //                 bw.Write (wall.distance1);
    //                 EditorCommon.WriteVector (bw, wall.normal2);
    //                 bw.Write (wall.distance2);
    //                 EditorCommon.WriteVector (bw, wall.normal3);
    //                 bw.Write (wall.distance3);
    //                 bw.Write (wall.pos.z);
    //                 bw.Write (wall.blockId);
    //             }
    //             short capsuleCount = 0; //(short)sci.capsules.Count;
    //             bw.Write (capsuleCount);
    //         }

    //         private static void SaveChunkInteractive (BinaryWriter bw, SceneSharedChunkInfo ssci, SceneChunkInfo sci)
    //         {
    //             short interactiveCount = (short) sci.interactiveList.Count;
    //             bw.Write (interactiveCount);
    //             for (short j = 0; j < interactiveCount; ++j)
    //             {
    //                 InteractiveItem box = sci.interactiveList[j];
    //                 EditorCommon.WriteVector (bw, box.center);
    //                 EditorCommon.WriteVector (bw, box.forward);
    //                 EditorCommon.WriteVector (bw, box.right);
    //                 EditorCommon.WriteVector (bw, box.up);
    //                 EditorCommon.WriteVector (bw, box.size);
    //                 SaveStringIndex (bw, ssci, box.exString);
    //             }
    //         }

    //         private static void SaveChunkLight (BinaryWriter bw, SceneSharedChunkInfo ssci, SceneChunkInfo sci)
    //         {
    //             int pointCount = sci.pointLights.Count;
    //             bw.Write (pointCount);
    //             for (short j = 0; j < pointCount; ++j)
    //             {
    //                 var point = sci.pointLights[j];
    //                 EditorCommon.WriteVector (bw, point.posRange);
    //                 EditorCommon.WriteVector (bw, point.color);
    //             }
    //         }

    //         private static void SaveChunkVertex (BinaryWriter bw, SceneSharedChunkInfo ssci,
    //             SceneConfig.TerrainChunk chunk, int i)
    //         {
    //             int gridCount = EngineContext.terrainGridCount + 1;
    //             gridCount *= gridCount;

    //             int flagCount = 0;
    //             if (ssci.groundFlags != null && chunk.terrainObjData.needCollider)
    //             {
    //                 flagCount = gridCount;
    //             }
    //             int startOffset = i * gridCount;
    //             bw.Write (flagCount);
    //             if (flagCount > 0)
    //             {
    //                 for (int j = 0; j < flagCount; ++j)
    //                 {
    //                     bw.Write (ssci.groundFlags[j + startOffset]);
    //                 }
    //             }
    //             int vertexCount = flagCount;
    //             bw.Write (vertexCount);
    //             if (vertexCount > 0)
    //             {
    //                 for (int j = 0; j < vertexCount; ++j)
    //                 {
    //                     bw.Write (ssci.vertex[j + startOffset]);
    //                 }
    //             }
    //         }

    //         private static void SaveChunkExternalData (BinaryWriter bw, int i)
    //         {
    //             if (saveChunkExternalDataCb != null)
    //             {
    //                 saveChunkExternalDataCb (bw, i);
    //             }
    //         }
    //         private static void OutputDebugInfo (List<string> writePos, string str)
    //         {
    //             var sb = new System.Text.StringBuilder ();
    //             sb.AppendFormat ("Chunk {0} offset\r\n", str);
    //             for (int j = 0; j < writePos.Count; ++j)
    //             {
    //                 sb.AppendLine (writePos[j]);
    //             }
    //             Debug.LogError (sb.ToString ());
    //         }

    //         public static void SaveScene (SceneConfig sceneConfig, ref SceneContext sceneContext, bool debugHead = false, int debugChunkIndex = -1)
    //         {
    //             try
    //             {
    //                 if (sceneConfig != null && sceneConfig.chunks != null)
    //                 {
    //                     int chunkWidth;
    //                     int chunkHeight;
    //                     int widthCount;
    //                     int heightCount;
    //                     GetSceneChunkCount (sceneConfig, out chunkWidth, out chunkHeight, out widthCount, out heightCount);
    //                     int[] ChunkStreamOffset = new int[sceneConfig.chunks.Count * 2];
    //                     byte[] ChunkInfo = new byte[sceneConfig.chunks.Count];
    //                     string targetSceneDir = string.Format ("{0}/EditorSceneRes/Scene/{1}",
    //                         AssetsConfig.instance.ResourcePath,
    //                         sceneContext.name);
    //                     EditorCommon.CreateDir (targetSceneDir);
    //                     EditorChunkData editorChunkData;
    //                     LoadEditorChunkData (ref sceneContext, "", true, out editorChunkData);
    //                     LightmapVolumnData lightmapVolumData;
    //                     LoadLightmapVolumnData (ref sceneContext, "", true, out lightmapVolumData);
    //                     ChunkLightData chunkLightData;
    //                     LoadChunkLightData (ref sceneContext, out chunkLightData);

    //                     if (editorChunkData != null)
    //                     {
    //                         SceneSharedChunkInfo ssci = new SceneSharedChunkInfo ();
    //                         ssci.context = sceneContext;
    //                         ssci.chunkWidth = chunkWidth;
    //                         ssci.chunkHeight = chunkHeight;
    //                         ssci.widthCount = widthCount;
    //                         ssci.heightCount = heightCount;
    //                         ssci.chunks = new SceneChunkInfo[sceneConfig.chunks.Count];
    //                         ssci.ChunkStreamOffset = ChunkStreamOffset;
    //                         ssci.ChunkInfo = ChunkInfo;
    //                         ssci.matSaveData.dmd = AssetsConfig.instance.sceneMat;
    //                         ssci.matSaveData.addResCb = ssci.MatAddResName;
    //                         for (int i = 0; i < sceneConfig.chunks.Count; ++i)
    //                         {
    //                             ChunkStreamOffset[i] = -1;
    //                             ssci.chunks[i] = new SceneChunkInfo ();
    //                         }
    //                         ssci.sceneConfig = sceneConfig;
    //                         ssci.cld = chunkLightData;
    //                         // AddResReDirct(ssci)
    //                         //PreSaveGlobalRes (ssci, re);
    //                         PreSave (ssci, editorChunkData, lightmapVolumData);
    //                         PreSaveExternalData (ssci);
    //                         LoadGroundVertex (ssci);

    //                         DebugPos.Clear (null);
    //                         int headLength = 0;
    //                         using (FileStream fs = new FileStream (string.Format ("{0}/EditorSceneRes/Scene/{1}/{1}.bytes",
    //                             AssetsConfig.instance.ResourcePath,
    //                             sceneContext.name), FileMode.Create))
    //                         {
    //                             BinaryWriter bw = new BinaryWriter (fs);

    //                             int headstart = (int) bw.BaseStream.Position;
    //                             SaveHeadData (bw, ssci, sceneConfig, editorChunkData.dynamicSceneNames.ToArray ());
    //                             DebugPos.Pos (bw, "head");
    //                             //write strings head
    //                             SaveHeadString (bw, ssci);
    //                             DebugPos.Pos (bw, "headString");
    //                             //terrain splat
    //                             SaveHeadTerrainSplat (bw, ssci, sceneConfig);
    //                             DebugPos.Pos (bw, "terrainSplat");
    //                             //mat
    //                             SaveHeadMaterial (bw, ssci);
    //                             DebugPos.Pos (bw, "headMat");
    //                             //env data
    //                             SaveHeadEnvData (bw, ssci, editorChunkData);
    //                             DebugPos.Pos (bw, "headEnv");
    //                             SaveHeadGlobalObject (bw, ssci);
    //                             DebugPos.Pos (bw, "headObj");
    //                             // write navigation
    //                             SaveHeadExternalData (bw, ssci);
    //                             DebugPos.Pos (bw, "headExternal");
    //                             headLength = (int) bw.BaseStream.Position;
    //                             if (debugHead)
    //                                 DebugPos.Pos (bw, "Scene head offset", true);

    //                             for (int i = 0; i < ssci.chunks.Length; ++i)
    //                             {
    //                                 SceneChunkInfo sci = ssci.chunks[i];
    //                                 var chunk = sceneConfig.chunks[i];
    //                                 if (sci != null)
    //                                 {
    //                                     DebugPos.Clear (bw);
    //                                     int start = (int) bw.BaseStream.Position;
    //                                     ChunkStreamOffset[i * 2] = start;
    //                                     ChunkInfo[i] = 0;
    //                                     //terrain
    //                                     SaveChunkTerrain (bw, ssci, sceneConfig, chunk, sci, i);
    //                                     DebugPos.Pos (bw, "terrain");
    //                                     SaveChunkRes (bw, ssci, sci, chunk);
    //                                     DebugPos.Pos (bw, "res");
    //                                     //instance
    //                                     SaveChunkInstance (bw, ssci, sci);
    //                                     DebugPos.Pos (bw, "instance");
    //                                     //scene objects
    //                                     SaveChunkSceneObject (bw, ssci, sci.sceneObjects, sci.instanceCount);
    //                                     DebugPos.Pos (bw, "sceneObject");
    //                                     //quad tree
    //                                     SaveChunkQuadTree (bw, ssci, sci);
    //                                     DebugPos.Pos (bw, "quadTree");
    //                                     //dynamic object
    //                                     SaveChunkDynamicObject (bw, ssci, editorChunkData, sci);
    //                                     DebugPos.Pos (bw, "dynamicobject");
    //                                     //camera wall
    //                                     SaveChunkWall (bw, ssci, sci);
    //                                     DebugPos.Pos (bw, "camerawall");
    //                                     //interactive item
    //                                     SaveChunkInteractive (bw, ssci, sci);
    //                                     DebugPos.Pos (bw, "interactive");
    //                                     //point light
    //                                     SaveChunkLight (bw, ssci, sci);
    //                                     DebugPos.Pos (bw, "light");

    //                                     SaveChunkVertex (bw, ssci, chunk, i);
    //                                     DebugPos.Pos (bw, "vertex");

    //                                     SaveChunkExternalData (bw, i);
    //                                     DebugPos.Pos (bw, "external");

    //                                     if (debugChunkIndex == i)
    //                                     {
    //                                         DebugPos.Pos (bw, string.Format ("Chunk {0} offset", i.ToString ()), true);
    //                                     }
    //                                     ChunkStreamOffset[i * 2 + 1] = (int) bw.BaseStream.Position - start;
    //                                 }
    //                                 else
    //                                 {
    //                                     ChunkStreamOffset[i * 2] = -1;
    //                                     ChunkStreamOffset[i * 2 + 1] = 0;
    //                                 }
    //                             }
    //                             PostSaveHeadData (bw, ssci);
    //                         }

    //                         string path = "Assets/BundleRes/EditorSceneRes/" + sceneContext.name + ".scenebytes";
    //                         using (FileStream fs = new FileStream (path, FileMode.Create))
    //                         {
    //                             BinaryWriter bw = new BinaryWriter (fs);
    //                             bw.Write (headLength);
    //                             int dynamicSceneCount = editorChunkData.dynamicSceneNames != null?editorChunkData.dynamicSceneNames.Count : 0;
    //                             bw.Write (dynamicSceneCount);
    //                             int chunkCount = ssci.chunks.Length;
    //                             bw.Write (chunkCount);
    //                             int splatCount = sceneConfig.bundles.Count;
    //                             bw.Write (splatCount);
    //                             int count = ssci.editorResReDirect.Count;
    //                             bw.Write (count);
    //                             var it = ssci.editorResReDirect.GetEnumerator ();
    //                             while (it.MoveNext ())
    //                             {
    //                                 var kvp = it.Current;
    //                                 bw.Write (kvp.Key);
    //                                 bw.Write (kvp.Value);
    //                             }
    //                             count = ssci.matSaveData.matInfo.Count;
    //                             bw.Write (count);
    //                             for (int j = 0; j < count; ++j)
    //                             {
    //                                 var mi = ssci.matSaveData.matInfo[j];
    //                                 string matPath = mi.mat != null?AssetDatabase.GetAssetPath (mi.mat): "";
    //                                 bw.Write (matPath);
    //                             }
    //                         }

    //                     }

    //                 }
    //             }
    //             catch (Exception e)
    //             {
    //                 Debug.LogError (e.StackTrace);
    //             }
    //         }
    //         private static long partLength = 1024 * 512;
    //         // #if UNITY_ANDROID
    //         private static byte[] sceneBuffer = new byte[partLength * 2];
    //         // #endif
    //         public static int PostSave (string name)
    //         {
    //             string path = string.Format ("{0}/EditorSceneRes/Scene/{1}/{1}.bytes", AssetsConfig.instance.ResourcePath, name);
    //             FileInfo fi = new FileInfo (path);
    //             if (fi.Exists)
    //             {

    //                 string dirPath = string.Format ("{0}/Scene/{1}", AssetsConfig.instance.ResourcePath, name);
    //                 DirectoryInfo di = new DirectoryInfo (dirPath);
    //                 FileInfo[] oldFiles = di.GetFiles ("*.bytes");
    //                 for (int i = 0; i < oldFiles.Length; ++i)
    //                 {
    //                     oldFiles[i].Delete ();
    //                 }
    //                 long length = fi.Length;
    //                 long partCount = length / partLength;
    //                 if (length > 0 && partCount == 0)
    //                 {
    //                     partCount = 1;
    //                 }
    //                 int readIndex = 0;
    //                 if (partCount > 0)
    //                 {
    //                     using (FileStream fs = new FileStream (path, FileMode.Open))
    //                     {

    //                         BinaryReader br = new BinaryReader (fs);
    //                         for (int i = 0; i < partCount; ++i)
    //                         {
    //                             long readCount = length > partLength ? partLength : length;
    //                             if (i == partCount - 1)
    //                             {
    //                                 readCount = length;
    //                             }
    //                             br.Read (sceneBuffer, 0, (int) readCount);
    //                             string partPath = string.Format ("{0}/{1}_{2}.bytes", dirPath, name, i);
    //                             using (FileStream wfs = new FileStream (partPath, FileMode.Create))
    //                             {
    //                                 BinaryWriter bw = new BinaryWriter (wfs);
    //                                 bw.Write (sceneBuffer, 0, (int) readCount);
    //                             }
    //                             AssetDatabase.ImportAsset (partPath);
    //                             readIndex += (int) readCount;
    //                             length -= readCount;
    //                         }
    //                     }

    //                 }
    //                 return (int) partCount;
    //             }
    //             return 0;
    //         }
    //         public static List<SceneLoadInfo> FastSaveSceneLoadList (SceneList sceneListConfig, bool splitScene = false)
    //         {

    //             if (sceneListConfig != null && sceneListConfig.sceneList != null)
    //             {
    //                 List<SceneLoadInfo> sceneName = new List<SceneLoadInfo> ();
    //                 for (int i = 0; i < sceneListConfig.sceneList.Count; ++i)
    //                 {
    //                     var scene = sceneListConfig.sceneList[i];
    //                     if (scene.sceneAsset != null && scene.sceneAsset.name.ToLower () != "test")
    //                     {
    //                         string scenePath = AssetDatabase.GetAssetPath (scene.sceneAsset);
    //                         string sceneDir = Path.GetDirectoryName (scenePath);
    //                         string sceneConfigPath = string.Format ("{0}/Config/{1}{2}.asset",
    //                             sceneDir, scene.sceneAsset.name, SceneContext.SceneConfigSuffix);
    //                         SceneConfig sc = AssetDatabase.LoadAssetAtPath<SceneConfig> (sceneConfigPath);
    //                         if (sc != null)
    //                         {
    //                             SceneContext context = new SceneContext ();
    //                             SceneAssets.GetSceneContext (ref context, scene.sceneAsset.name, scenePath);
    //                             SaveScene (sc, ref context);
    //                             int partCount = 1;
    //                             if (splitScene)
    //                             {
    //                                 partCount = PostSave (scene.sceneAsset.name);
    //                             }
    //                             if (partCount > 0)
    //                             {
    //                                 sceneName.Add (new SceneLoadInfo ()
    //                                 {
    //                                     name = scene.sceneAsset.name.ToLower (),
    //                                         count = partCount
    //                                 });
    //                             }
    //                         }

    //                     }

    //                 }
    //                 using (FileStream fs = new FileStream (string.Format ("{0}/Scene/SceneLoadList.bytes", AssetsConfig.instance.ResourcePath), FileMode.Create))
    //                 {
    //                     BinaryWriter bw = new BinaryWriter (fs);
    //                     int maxDynamicSceneCount = 0;
    //                     int maxChunkCount = 0;
    //                     int maxSplatCount = 0;

    //                     bw.Write (sceneName.Count);
    //                     for (int i = 0; i < sceneName.Count; ++i)
    //                     {
    //                         var SceneLoadInfo = sceneName[i];
    //                         bw.Write (SceneLoadInfo.name);
    //                         bw.Write (SceneLoadInfo.count);
    //                         int headLength = 0;
    //                         string path = "Assets/BundleRes/EditorSceneRes/" + SceneLoadInfo.name + ".scenebytes";
    //                         if (File.Exists (path))
    //                         {
    //                             using (FileStream otherfs = new FileStream (path, FileMode.Open))
    //                             {
    //                                 BinaryReader br = new BinaryReader (otherfs);
    //                                 headLength = br.ReadInt32 ();
    //                                 // bw.Write (headLength);
    //                                 int dynamicSceneCount = br.ReadInt32 ();
    //                                 if (dynamicSceneCount > maxDynamicSceneCount)
    //                                 {
    //                                     maxDynamicSceneCount = dynamicSceneCount;
    //                                 }
    //                                 int chunkCount = br.ReadInt32 ();
    //                                 if (chunkCount > maxChunkCount)
    //                                 {
    //                                     maxChunkCount = chunkCount;
    //                                 }
    //                                 int splatCount = br.ReadInt32 ();
    //                                 if (splatCount > maxSplatCount)
    //                                 {
    //                                     maxSplatCount = splatCount;
    //                                 }
    //                             }
    //                         }
    //                         bw.Write (headLength);
    //                     }
    //                     bw.Write (maxDynamicSceneCount);
    //                     bw.Write (maxChunkCount);
    //                     bw.Write (maxSplatCount);
    //                 }
    //                 return sceneName;
    //             }
    //             return null;
    //         }

    //         public static void PostExportScene (SceneList sceneListConfig)
    //         {
    // #if !UNITY_ANDROID
    //             if (sceneListConfig != null && sceneListConfig.sceneList != null)
    //             {
    //                 Debug.Log ("scene data count:" + sceneListConfig.sceneList.Count);
    //                 for (int i = 0; i < sceneListConfig.sceneList.Count; ++i)
    //                 {
    //                     var scene = sceneListConfig.sceneList[i];
    //                     string scenename = scene.sceneAsset.name.ToLower ();
    //                     if (scene.sceneAsset != null && scenename != "test")
    //                     {

    //                         string path = string.Format ("{0}/EditorSceneRes/Scene/{1}/{1}.bytes", AssetsConfig.instance.ResourcePath, scenename);
    //                         if (File.Exists (path))
    //                         {
    //                             string newPath = string.Format ("{0}/Bundles/assets/bundleres/scene/{1}/{1}.bytes", Application.streamingAssetsPath, scenename);
    //                             EditorCommon.CreateDir (string.Format ("Assets/StreamingAssets/Bundles/assets/bundleres/scene/{0}", scenename));
    //                             AssetDatabase.CopyAsset (path, newPath);
    //                             AssetDatabase.ImportAsset (newPath);
    //                             if (!File.Exists (newPath))
    //                             {
    //                                 Debug.LogErrorFormat ("not exist scene data:{0}", newPath);
    //                             }
    //                             else
    //                             {
    //                                 Debug.LogFormat ("export scene data:{0}", newPath);
    //                             }
    //                         }
    //                         else
    //                         {
    //                             Debug.LogFormat ("scene data not exist:{0}", path);
    //                         }

    //                     }
    //                 }
    //             }
    //             else
    //             {
    //                 Debug.Log ("sceneListConfig not exist scene data");
    //             }
    // #endif
    //         }
    //     }
}
#endif
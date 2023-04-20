#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CFEngine
{
    public class SceneAssets
    {
        public static void GetSceneContext(ref SceneContext context, string name, string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                context.valid = true;
                context.dir = Path.GetDirectoryName(path).Replace("\\", "/");
                context.configDir = context.dir + "/" + SceneContext.ConfigSuffix;
                context.terrainDir = context.dir + "/" + SceneContext.TerrainSuffix;

                string folder = context.dir;
                int index = folder.LastIndexOf("/");
                if (index >= 0)
                {
                    folder = folder.Substring(index + 1);
                }

                context.name = folder;
                context.suffix = SceneContext.MainTagName;
            }
            else
            {
                context.Reset();
            }
        }

        public static void GetSceneContext(ref SceneContext context, ref Scene scene)
        {
            GetSceneContext(ref context, scene.name, scene.path);
        }

        public static void GetCurrentSceneContext(ref SceneContext context)
        {
            var scene = SceneManager.GetActiveScene();
            GetSceneContext(ref context, ref scene);
        }

        public static string CreateFolder(string dir, string name)
        {
            string folder = dir + "/" + name;
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder(dir, name);
            return folder;
        }

        public static void SceneModify(bool save = false)
        {
            if (!Application.isPlaying)
            {
                UnityEngine.SceneManagement.Scene s = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(s);
                if (save)
                    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(s);
            }
        }
        public static void ReloadScene()
        {
            if (!Application.isPlaying)
            {
                Scene s = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                string path = s.path;
                EditorSceneManager.MarkSceneDirty(s);
                EditorSceneManager.SaveScene(s);
                EditorSceneManager.OpenScene("Assets/Scenes/empty.unity");
                EditorSceneManager.OpenScene(path);

            }
        }

        public static void SortSceneObjectName(Component comp, string path, Dictionary<string, int> objMap)
        {
            if (!string.IsNullOrEmpty(path))
            {
                int count = 0;
                objMap.TryGetValue(path, out count);
                //comp.name = path + "_" + count.ToString();
                count++;
                objMap[path] = count;
            }

        }

        public static TerrainData GetUnityTerrain(ref GameObject terrainGo)
        {
            Terrain t = null;
            var go = GameObject.Find("UnityTerrain");
            if (go != null && go.transform.childCount > 0)
            {
                go.transform.GetChild(0).TryGetComponent(out t);
            }
            else
            {
                t = Terrain.activeTerrain;
            }
            if (t != null)
            {
                terrainGo = t.gameObject;
            }
            return t != null ? t.terrainData : null;
        }

        public static void CalcBlock(EngineContext context, ref AABB aabb, ref ChunkInfo chunkInfo,
           bool forceGlobal = false, bool calcGlobalObj = true)
        {

            Vector3 center = aabb.center;
            Vector3 size = aabb.size;
            SceneQuadTree.FindChunkIndex(ref center,
                EngineContext.ChunkSize, out var x, out var z);

            if (x >= 0 && z >= 0 && x < context.xChunkCount && z < context.zChunkCount && !forceGlobal)
            {
                chunkInfo.chunkID = x + z * context.xChunkCount;
            }
            else
            {
                chunkInfo.chunkID = -1;
                if (x >= -1 && z >= -1 && x < (context.xChunkCount + 1) && z < (context.zChunkCount + 1))
                {
                    chunkInfo.virtualChunkID = x + 1 + (z + 1) * (context.xChunkCount + 2);
                }
            }
            chunkInfo.isGlobalObject = false;
            float length = size.x > size.z ? size.x : size.z;

            if (calcGlobalObj && length > EngineContext.ChunkSize * 0.4f)
            {
                float minX = x * EngineContext.ChunkSize;
                float maxX = minX + EngineContext.ChunkSize;
                float minZ = z * EngineContext.ChunkSize;
                float maxZ = minZ + EngineContext.ChunkSize;
                float objMinX = aabb.min.x;
                float objMaxX = aabb.max.x;
                float objMinZ = aabb.min.z;
                float objMaxZ = aabb.max.z;
                float width = 0;
                float height = 0;
                if (objMinX > minX && objMinX < maxX)
                {
                    width = objMaxX > maxX ? maxX - objMinX : size.x;
                }
                else if (objMaxX > minX && objMaxX < maxX)
                {
                    width = objMinX > minX ? size.x : objMaxX - minX;
                }
                if (objMinZ > minZ && objMinZ < maxZ)
                {
                    height = objMaxZ > maxZ ? maxZ - objMinZ : size.z;
                }
                else if (objMaxZ > minZ && objMaxZ < maxZ)
                {
                    height = objMinZ > minZ ? size.z : objMaxZ - minZ;
                }
                float crossArea = width * height;
                float area = size.x * size.z;
                if (crossArea < 0.0001f || crossArea / area < 0.4f)
                {
                    chunkInfo.chunkID = -1; //global object
                    chunkInfo.isGlobalObject = true;
                }

            }

            if (chunkInfo.chunkID >= 0)
            {
                chunkInfo.x = chunkInfo.chunkID % context.xChunkCount;
                chunkInfo.z = chunkInfo.chunkID / context.xChunkCount;
                int chunkLevel = 0;
                int blockId = 0;
                SceneQuadTree.CalcBlockId(size, center, chunkInfo.x, chunkInfo.z,
                   EngineContext.ChunkSize, EngineContext.ChunkSize, out chunkLevel, out blockId);
                chunkInfo.blockID = blockId;
            }
            else
            {
                chunkInfo.blockID = 0;
                chunkInfo.x = 0;
                chunkInfo.z = 0;
            }
        }
    }
}
#endif

public struct SceneContext
{
    public string name;
    public string suffix;
    public string dir;
    public string configDir;
    public string terrainDir;

    public bool valid;

    public static string SceneConfigSuffix = "_SceneConfig";

    public static string ConfigSuffix = "Config";

    public static string TerrainSuffix = "Terrain";

    public static string MainTagName = "MainScene";
    public void Reset()
    {
        name = "";
        dir = "";
        configDir = "";
        terrainDir = "";
        valid = false;
    }
}

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CFEngine
{
    [System.Serializable]
    public class BaseSceneData<T> where T : new ()
    {
        public virtual void Copy (T src)
        {

        }
    }

    [System.Serializable]
    public class PrefabSfxData : BaseSceneData<PrefabSfxData>
    {
        public string sfxName;
        public Vector3 pos;
        public Quaternion rotate = Quaternion.identity;
        public Vector3 scale = Vector3.one;
        public AABB aabb;
        public string path = "";
        public bool isSfx = true;
        public override void Copy (PrefabSfxData src)
        {
            sfxName = src.sfxName;
            pos = src.pos;
            rotate = src.rotate;
            scale = src.scale;
            aabb = src.aabb;
            path = src.path;
        }
    }

    // [System.Serializable]
    // public class SceneResData
    // {
    //     public string resName;
    //     public List<ScriptableObject> data = new List<ScriptableObject> ();
    // }

    public class SceneQuadBlock
    {
        public short sceneObjectGroupIndex = -1;
        public byte quadIndex = 0;
        public AABB aabb = AABB.zero;
    }
    public class ChunkSaveData
    {
        public List<SceneObjectData> sceneObjects = new List<SceneObjectData> ();
        public List<SceneObjectData> instanceObjects = new List<SceneObjectData> ();
        public Dictionary<int, SceneQuadBlock> sceneQuadBlocks = new Dictionary<int, SceneQuadBlock> ();
        public List<ushort> sceneObjectIndex = new List<ushort> ();
        public byte terrainLightmapIndex = 255;
        public Vector4 terrainLightmapST = Vector4.zero;
        public List<InstanceInfo> instanceInfos = null;
        public List<uint> argArray = null;
        public int instanceCount = 0;
        public int multiLayerCount = 0;
        public bool hasCollider = false;

    }
    public class SceneSaveData
    {
        public List<ChunkSaveData> chunks = new List<ChunkSaveData> ();
        public ChunkSaveData global = new ChunkSaveData ();
    }

    [System.Serializable]
    public class ChunkData
    {
        public LightProbeObject lpo = new LightProbeObject ();
        public List<LightingInfo> pointLights = new List<LightingInfo> ();
        public List<SceneAudioObject> audios = new List<SceneAudioObject> ();
        public List<GroupObject> groupObjects = new List<GroupObject> ();
        public List<SceneObjectData> sceneObjects = new List<SceneObjectData> ();
        public List<InstanceData> instanceObjects = new List<InstanceData> ();
        public List<InstanceObjectData> multiLayerObjects = new List<InstanceObjectData>();        
        public bool hasCollider = false;
        public List<string> colliderMesh = new List<string> ();

        public void Clear ()
        {
            audios.Clear ();
            pointLights.Clear();
            groupObjects.Clear ();
            sceneObjects.Clear ();
            instanceObjects.Clear ();
            multiLayerObjects.Clear();
            hasCollider = false;
            colliderMesh.Clear ();
        }
    }

    public class SceneData : ScriptableObject
    {
        public List<ChunkData> chunks = new List<ChunkData> ();
        public ChunkData global = new ChunkData ();
        public List<PrefabFbxData> prefabs = new List<PrefabFbxData> ();
        public List<GameObjectGroupData> groups = new List<GameObjectGroupData> ();
        public List<GameObjectInstanceData> gameObjects = new List<GameObjectInstanceData> ();
        public List<ColliderData> colliderDatas = new List<ColliderData> ();
        public List<LightData> lightDatas = new List<LightData> ();
        public List<SceneAnimationObject> animationDatas = new List<SceneAnimationObject> ();
        public List<EnvObject> envObjects = new List<EnvObject> ();
        public List<ReflectionProbeData> localReflectionProbes = new List<ReflectionProbeData> ();
        public List<InstanceMeshMat> instances = new List<InstanceMeshMat> ();
        public int pvsCellSize = 2;
        public int pvsMaskCount = 0;
        public List<PVSChunkData> pvsChunkData = new List<PVSChunkData>();
        public Vector2Int chunkXZ;
        public FlagMask saveFlag;

        public void Clear ()
        {
            PrefabFbxData.GlobalObjectId = 0;
            GameObjectInstanceData.GlobalObjectId = 0;
            SceneObjectData.objectID = 0;
            SceneObjectData.shadowObjectID = 0;
            foreach (var chunk in chunks)
            {
                chunk.Clear ();
            }
            global.Clear ();
            prefabs.Clear ();
            groups.Clear ();
            gameObjects.Clear ();
            colliderDatas.Clear ();
            lightDatas.Clear ();
            animationDatas.Clear ();
            envObjects.Clear ();
            localReflectionProbes.Clear ();
            pvsCellSize = 2;
            pvsMaskCount = 0;
            pvsChunkData.Clear();
            chunkXZ = Vector2Int.zero;
        }

        public ChunkData GetChunk (int chunkID, bool noGlobal = false)
        {
            if (chunkID >= 0 && chunkID < chunks.Count)
            {
                return chunks[chunkID];
            }
            return noGlobal?null : global;
        }
    }

    public class SceneSerializeContext
    {
        public int chunkWidth;
        public int chunkHeight;
        public int widthCount;
        public int heightCount;
        public Transform root;
        public EditorCommon.EnumTransform preSerialize;
        public EditorCommon.EnumTransform serialize;
        public SceneResProcess resProcess;
        public Dictionary<string, Transform> sceneResMap = new Dictionary<string, Transform> ();
        public Dictionary<GameObject, int> prefabMap = new Dictionary<GameObject, int> ();
        public Dictionary<string, int> objIDMap = new Dictionary<string, int> ();
        public Stack<int> folderIDStack = new Stack<int> ();
        public int lastFolderID = -1;
        public string dynamicSceneName = "";
        public int dynamicSceneID = 0;
        public SceneData sd;
        public DynamicSceneData dsd;
        public SceneConfig sceneConfig;

        public void Reset ()
        {
            objIDMap.Clear ();
            prefabMap.Clear ();
            folderIDStack.Clear ();
            lastFolderID = -1;
            dynamicSceneName = "";
            dynamicSceneID = 0;
        }

    }

    public class BaseSceneContext
    {
        public int chunkWidth;
        public int chunkHeight;
        public int widthCount;
        public int heightCount;
        public SceneData sd;
        public SceneSaveData saveSD = new SceneSaveData ();
        public SceneConfig sceneConfig;
        public SceneContext sceneContext;

    }

    public interface IMoveGrid
    {
        void Load(ref SceneContext context);
        int BlockCount { get; }
        void GetBlockOffset(int blockIndex, out float x, out float z);
        int GetGridCount(int blockIndex);
        bool QueryGrid(int blockIndex, int gridIndex, ref Vector3 pos);
    }
}
#endif
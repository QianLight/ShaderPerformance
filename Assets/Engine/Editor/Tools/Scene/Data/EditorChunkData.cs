using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine.Editor
{
    // [System.Serializable]
    //  public struct Vector4Int
    //  {
    //      public int x;
    //      public int y;
    //      public int z;
    //      public int w;

    //     public Vector4Int(int _x, int _y, int _z, int _w)
    //     {
    //         x = _x;
    //         y = _y;
    //         z = _z;
    //         w = _w;
    //     }
    //  }
    public enum ObjectFlag
    {
        GameObjectActiveInHierarchy = 0x0001,
        GameObjectActive = 0x0002,
        HasAnim = 0x0004,
        RenderEnable = 0x0008,
        IgnoreShadowCaster = 0x0020,
        HideRender = 0x0040,
        IgnoreShadowReceive = 0x0080,
        MultiMaterial = 0x0100,
        UVAnimation = 0x0200,
        FadeEffect = 0x0400,
    }

    [System.Serializable]
    public class AudioData
    {
        public string eventName = "";
        public Vector3 pos = Vector3.zero;
        public float range = 1;
        public int chunkID = -1;
    }

    [System.Serializable]
    public class GroupData
    {
        public Bounds aabb;
        public int chunkID = -1;
        [NonSerialized]
        public int index = 0;
    }

    public class EditorChunkData : ScriptableObject
    {
        [System.Serializable]
        public enum ColliderType
        {
            None,
            Box,
            Sphere,
            Capsule,
            Mesh
        }
        public enum DynamicObjectType
        {
            None,
            Dummy,
            Spawn,
            Terminal,
            Transfer,
            Circle,
        }

        [System.Serializable]
        public class ColliderData
        {
            public ColliderType colliderType = ColliderType.None;
            public Vector3 center;
            public Vector3 size;
            public float radius;
            public float height;
            public int direction;
            public Mesh sharedMesh;
            public bool convex;
            public MeshColliderCookingOptions cookingOptions;
            public string exString;

            public ColliderData Clone ()
            {
                return new ColliderData ()
                {
                    colliderType = this.colliderType,
                        center = this.center,
                        size = this.size,
                        radius = this.radius,
                        height = this.height,
                        direction = this.direction,
                        sharedMesh = this.sharedMesh,
                        convex = this.convex,
                        cookingOptions = this.cookingOptions,
                        exString = this.exString,
                };
            }
        }

        [System.Serializable]
        public class EffectPrefab
        {
            public GameObject prefab;
            public Vector3 pos;
            public Quaternion rotate;
            public Vector3 scale;
            public Bounds bound;
            [NonSerialized]
            public string path = "";
            public EffectPrefab Clone ()
            {
                return new EffectPrefab ()
                {
                    prefab = this.prefab,
                        pos = this.pos,
                        rotate = this.rotate,
                        scale = this.scale,
                        bound = this.bound,
                };
            }

        }

        [System.Serializable]
        public class DynamicObjectData
        {
            public DynamicObjectType type = DynamicObjectType.None;
            public Vector3 boxCenter;
            public Vector3 boxSize;
            public uint flag;
            public string exString;
            public string dynamicSceneName;
            public List<EffectPrefab> effects = new List<EffectPrefab> ();
            public Vector3 pos0;
            public Vector3 pos1;
            public string hashStr;
            public DynamicObjectData Clone ()
            {
                var dod = new DynamicObjectData ()
                {
                    type = this.type,
                    boxCenter = this.boxCenter,
                    boxSize = this.boxSize,
                    flag = this.flag,
                    exString = this.exString,
                    dynamicSceneName = this.dynamicSceneName,
                    pos0 = this.pos0,
                    pos1 = this.pos1,
                    hashStr = this.hashStr,
                };
                for (int i = 0; i < effects.Count; ++i)
                {
                    dod.effects.Add (effects[i].Clone ());
                }
                return dod;
            }
        }

        [System.Serializable]
        public class PrefabInfo
        {
            [System.NonSerialized]
            public string name;
            public GameObject prefab;

            [System.NonSerialized]
            public List<SubPrefabInfo> subPrefabs;
        }

        [System.Serializable]
        public class InstanceProperty
        {
            public Vector4 pos = Vector4.zero;
            public Quaternion rot = Quaternion.identity;
            public float scale = 1;
            public bool visible = true;

            public Bounds aabb;
            public int groupIndex = -1;
            public string tagName = "";

            public InstanceProperty Clone ()
            {
                return new InstanceProperty ()
                {
                    pos = this.pos,
                        rot = this.rot,
                        scale = this.scale,
                        visible = this.visible,
                        aabb = this.aabb,
                        groupIndex = this.groupIndex,
                        tagName = this.tagName,
                };
            }
        }

        [System.Serializable]
        public class InstanceObject
        {
            public List<InstanceProperty> instanceInfo = new List<InstanceProperty> ();
            public int prefabIndex = -1;
            public Material mat;
            public InstanceObject Clone ()
            {
                InstanceObject io = new InstanceObject ()
                {
                    prefabIndex = this.prefabIndex,
                    mat = this.mat
                };
                for (int i = 0; i < instanceInfo.Count; ++i)
                {
                    io.instanceInfo.Add (instanceInfo[i].Clone ());
                }
                return io;
            }

        }

        [System.Serializable]
        public class BlockObjectInfo
        {
            public Mesh mesh;
            public Material mat;
            public byte lightmapIndex;
            public int blockIndex = 0; //0-3
        }

        [System.Serializable]
        public class ObjectBlock
        {
            // public List<BlockObjectInfo> objects = new List<BlockObjectInfo> ();
            public List<InstanceObject> instanceObjects = new List<InstanceObject> ();
        }



        [System.Serializable]
        public class SubPrefabInfo
        {
            public Mesh mesh;
        }

        [System.Serializable]
        public class SubPrefabOverrideInfo : IObjData
        {
            public string name;
            public int renderIndex = -1;

            public Material mat;
            public Bounds sceneAABB;
            public Vector3 pos;
            public Quaternion rotate;
            public Vector3 scale;
            public Vector3 localScale;
            public int blockId = 0;
            public int colliderIndex = -1;

            public float lightMapScale = 1.0f;
            public int lightMapIndex = -1;
            public int lightMapVolumnIndex = -1;
            public Vector4 lightmapUVST = new Vector4 (1, 1, 0, 0);
            public int reflectionProbeIndex = -2;
            public bool autoLod = true;
            public float lodDist = -1;
            public string signelExString = "";
            public int groupObject = -1;
            public uint flag = 0;
            public int id = 0;
            public int ID { get { return id; } set { id = value; } }
            public SubPrefabOverrideInfo Clone ()
            {
                return new SubPrefabOverrideInfo ()
                {
                    name = this.name,
                        renderIndex = this.renderIndex,
                        mat = this.mat,
                        sceneAABB = this.sceneAABB,
                        pos = this.pos,
                        rotate = this.rotate,
                        scale = this.scale,
                        localScale = this.localScale,
                        blockId = this.blockId,
                        colliderIndex = this.colliderIndex,

                        lightMapScale = this.lightMapScale,
                        lightMapIndex = this.lightMapIndex,
                        lightMapVolumnIndex = this.lightMapVolumnIndex,
                        lightmapUVST = this.lightmapUVST,
                        reflectionProbeIndex = this.reflectionProbeIndex,
                        autoLod = this.autoLod,
                        lodDist = this.lodDist,
                        flag = this.flag,
                        signelExString = this.signelExString,
                        groupObject = this.groupObject,
                };
            }
            public void SetFlag (ObjectFlag f, bool add)
            {
                if (add)
                {
                    flag |= (uint) f;
                }
                else
                {
                    flag &= ~((uint) f);
                }
            }

            public bool HasFlag (ObjectFlag f)
            {
                return (flag & (uint) f) != 0;
            }
        }

        [System.Serializable]
        public class GameObjectInfo
        {
            public string goname = "";
            public string tagName = "";
            public uint prefabType;
            public int groupIndex = -1;
            public int prefabIndex = -1;
            public Vector3 pos;
            public Quaternion rotate;
            public Vector3 scale;
            public string tag;
            public int layer;
            public Bounds bound;
            public int groupObject = -1;
            public List<Vector4Int> subPrefabChunkInfo = new List<Vector4Int> ();

            public uint flag = 0;
            public GameObjectInfo Clone (bool cloneSubPrefab)
            {
                GameObjectInfo goi = new GameObjectInfo ()
                {
                    goname = this.goname,
                    tagName = this.tagName,
                    prefabType = this.prefabType,
                    prefabIndex = this.prefabIndex,
                    groupIndex = this.groupIndex,
                    pos = this.pos,
                    rotate = this.rotate,
                    scale = this.scale,
                    tag = this.tag,
                    layer = this.layer,
                    bound = this.bound,
                    // signelExString = this.signelExString,
                };
                if (cloneSubPrefab)
                {
                    goi.subPrefabChunkInfo.AddRange (subPrefabChunkInfo);
                }
                return goi;
            }

            public void SetFlag (ObjectFlag f, bool add)
            {
                if (add)
                {
                    flag |= (uint) f;
                }
                else
                {
                    flag &= ~((uint) f);
                }
            }

            public bool HasFlag (ObjectFlag f)
            {
                return (flag & (uint) f) != 0;
            }
            public override string ToString ()
            {
                return goname;
            }
        }

        [System.Serializable]
        public class GameObjectGroup
        {
            public string name = "";
            public string path = "";
            public int objType = 0;
            public string tagName = "";
            public bool visible = true;

            public int groupObject = -1;

            [System.NonSerialized]
            public Transform transform;
            public override string ToString ()
            {
                return name;
            }
            public GameObjectGroup Clone ()
            {
                return new GameObjectGroup ()
                {
                    name = this.name,
                        path = this.path,
                        objType = this.objType,
                        tagName = this.tagName,
                        visible = this.visible
                };
            }
        }

        [System.Serializable]
        public class EditorChunk
        {
            //for editor
            public List<SubPrefabOverrideInfo> subPrefabOverrideInfos = new List<SubPrefabOverrideInfo> ();
            public LightProbeObject lpo = new LightProbeObject ();
            //runtime       
            public ObjectBlock[] blocks = new ObjectBlock[4];
            public Vector3Int meshInfo = Vector3Int.zero;

            public EditorChunk Clone ()
            {
                EditorChunk ec = new EditorChunk ();
                for (int i = 0; i < subPrefabOverrideInfos.Count; ++i)
                {
                    ec.subPrefabOverrideInfos.Add (subPrefabOverrideInfos[i].Clone ());
                }
                // ec.terrainObjData.Copy (terrainObjData);
                for (int i = 0; i < blocks.Length; ++i)
                {
                    var block = blocks[i];
                    if (block != null)
                    {
                        var newBlock = new ObjectBlock ();
                        for (int j = 0; j < block.instanceObjects.Count; ++j)
                        {
                            newBlock.instanceObjects.Add (block.instanceObjects[j].Clone ());
                        }
                        ec.blocks[i] = newBlock;
                    }
                }
                return ec;
            }
        }

        public List<EditorChunk> chunks = new List<EditorChunk> ();
        //editor sync
        public List<PrefabInfo> prefabInfos = new List<PrefabInfo> ();
        public List<GameObjectGroup> gameObjectInfoGroups = new List<GameObjectGroup> ();
        public List<GameObjectInfo> gameObjectInfos = new List<GameObjectInfo> ();
        public List<ColliderData> colliderDatas = new List<ColliderData> ();
        public List<LightData> lightDatas = new List<LightData> ();
        //global
        public List<SceneAnimationData> animationDatas = new List<SceneAnimationData> ();
        public List<EnvObject> envObjects = new List<EnvObject> ();
        public List<SubPrefabOverrideInfo> globalObjectInfo = new List<SubPrefabOverrideInfo> ();
        public List<ReflectionProbeData> localReflectionProbes = new List<ReflectionProbeData> ();
        //chunk based
        
        public List<DynamicObjectData> dynamicObjectDatas = new List<DynamicObjectData> ();
        public List<AudioData> audios = new List<AudioData> ();
        public List<GroupData> groupObjects = new List<GroupData> ();
        public List<string> dynamicSceneNames = new List<string> ();

        [NonSerialized]
        public int id = 0;
        public void Clear ()
        {
            for (int i = 0; i < chunks.Count; ++i)
            {
                EditorChunkData.EditorChunk chunk = chunks[i];
                chunk.subPrefabOverrideInfos.Clear ();
                chunk.meshInfo = Vector3Int.zero;
                for (int j = 0; j < chunk.blocks.Length; ++j)
                {
                    var block = chunk.blocks[j];
                    if (block != null)
                        block.instanceObjects.Clear ();
                }
            }

            prefabInfos.Clear ();
            gameObjectInfoGroups.Clear ();
            gameObjectInfos.Clear ();

            colliderDatas.Clear ();
            lightDatas.Clear ();
            dynamicObjectDatas.Clear ();
            animationDatas.Clear ();
            envObjects.Clear ();
            globalObjectInfo.Clear ();
            dynamicSceneNames.Clear ();
            localReflectionProbes.Clear ();
            audios.Clear ();
            groupObjects.Clear ();
        }
    }
}
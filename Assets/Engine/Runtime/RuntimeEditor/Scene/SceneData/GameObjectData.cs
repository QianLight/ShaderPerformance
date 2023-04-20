#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine
{
    [System.Serializable]
    public class SceneObjectData : BaseSceneData<SceneObjectData>, IObjData, IQuadTreeObject
    {
        public int renderIndex = -1;
        public string resName = "";
        public Material mat;
        public AABB aabb;
        public Vector3 pos;
        public Quaternion rotate = Quaternion.identity;
        public Vector3 scale = Vector3.one;
        public Vector3 localScale = Vector3.one;
        public string exString = "";
        public int blockId = 0;
        public LodDist lodDist;
        public int groupObjectIndex = -1;
        public float lightMapScale = 1.0f;
        public int lightMapIndex = -1;
        public int lightMapVolumnIndex = -1;
        public Vector4 lightmapUVST = new Vector4 (1, 1, 0, 0);
        public int shadowID = -1;
        public int virtualChunkID = -1;
        public FlagMask flag;
        public uint areaMask = 0xffffffff;
        [NonSerialized]
        public ushort matId;
        [NonSerialized]
        public uint matHash;
        [NonSerialized]
        public string lightmapName = "";
        [NonSerialized]
        public string shadowmaskName = "";
        [NonSerialized]
        public string reflectionProbeName = "";
        public int gameObjectID = -1;
        [SerializeField]
        private int id = -1;
        public int ID { get { return id; } set { id = value; } }

        public static int objectID = 0;
        public static int shadowObjectID = 0;
        public void SetID ()
        {
            ID = objectID++;
        }

        public int BlockId { get { return blockId; } }

        public int QuadNodeId { get; set; }

        public AABB bounds { get { return aabb; } }

        public override void Copy (SceneObjectData src)
        {

        }
    }

    [System.Serializable]
    public class PrefabFbxData : IObjData
    {
        public GameObject prefab;
        [System.NonSerialized]
        public string name;
        [SerializeField]
        private int id = 0;
        public int ID { get { return id; } set { id = value; } }
        public static int GlobalObjectId = 0;

        public void SetID ()
        {
            ID = GlobalObjectId++;
        }
    }

    [System.Serializable]
    public class GameObjectGroupData : IObjData
    {
        public string name = "";
        public string scenePath = "";
        public bool visible = true;
        public int parentID = -1; //Root

        [System.NonSerialized]
        public Transform transform;
        [SerializeField]
        private int id = 0;
        public int ID { get { return id; } set { id = value; } }

        public void SetID ()
        {
            ID = GameObjectInstanceData.GlobalObjectId++;
        }
    }

    [System.Serializable]
    public class GameObjectInstanceData : IObjData
    {
        public string name = "";
        public Vector3 pos;
        public Quaternion rotate = Quaternion.identity;
        public Vector3 scale = Vector3.one;
        public AABB aabb;
        public string tag;
        public int layer;
        public int prefabID = -1;
        public int parentID = -1; //Root
        public FlagMask flag;
        [SerializeField]
        private int id = 0;
        public int ID { get { return id; } set { id = value; } }
        public static int GlobalObjectId = 0;

        public void SetID ()
        {
            ID = GlobalObjectId++;
        }
    }

}
#endif
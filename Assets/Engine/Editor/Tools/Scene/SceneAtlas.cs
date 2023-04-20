using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace CFEngine.Editor
{
    public class SceneAtlas : ScriptableObject
    {
        [System.Serializable]
        public class SpriteInfo
        {
            public Texture2D tex;
            public int texType = 0;
            public float scale = 0.0f;
            public int atlasIndex = -1;
            public Vector2Int atlasPos;
            public Vector2Int scalSize;

            [System.NonSerialized]
            public Rect rect;
            [System.NonSerialized]
            public Rect atlasRect;

            [System.NonSerialized]
            public Vector2Int previewSize;
            [System.NonSerialized]
            public Vector2Int drawSize;
            [System.NonSerialized]
            public bool fromAtlas = false;
            [System.NonSerialized]
            public bool used = true;

            [System.NonSerialized]
            public bool needRefresh = false;
        }

        public class MatInfo
        {
            public Material mat;
            public int[] spriteIndex = new int[3] { -1, -1, -1 };
            public Texture2D[] tex = new Texture2D[3] { null, null, null };

        }

        [System.Serializable]
        public class PrefabInfo
        {
            public GameObject prefab;

            public Bounds aabb;
            public string path;
            [System.NonSerialized]
            public List<MatInfo> mats = new List<MatInfo>();

        }

        [System.Serializable]
        public class AtlasInfo
        {
            public int sizeIndex = 0;
            public Texture2D atlas;

            [System.NonSerialized]
            public int size = 512;
            public bool empty = true;
        }

        [System.Serializable]
        public class PrefabSet
        {
            public List<SceneAtlas.PrefabInfo> prefabs = new List<PrefabInfo>();
            public int texCount = 0;
            public bool folder = false;
            [System.NonSerialized]
            public Vector2 prefabScroll;

            [System.NonSerialized]
            public int groupIndex = -1;
        }

        [System.Serializable]
        public class PrefabGroup
        {
            public string name = "";
            public List<AtlasInfo> atlas = new List<AtlasInfo>();

            public List<SpriteInfo> sprites = new List<SpriteInfo>();
            public List<PrefabSet> prefabSets = new List<PrefabSet>();

            [System.NonSerialized]
            public Dictionary<Texture2D, List<PrefabInfo>> texPrefabMap = new Dictionary<Texture2D, List<PrefabInfo>>();
        }
        public List<PrefabGroup> groups = new List<PrefabGroup>();
        public int selectPrefabGroupIndex = -1;
    }
}
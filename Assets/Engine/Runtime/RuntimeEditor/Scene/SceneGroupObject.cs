#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class SceneGroupObject : MonoBehaviour
    {
        public LodData lodData = new LodData ();
        public bool fadeEffect = false;
        public int index = 0;
        public AABB aabb;
        public ChunkInfo chunkInfo;
        public bool globalObject = false;
        public bool forceGlobalObj = false;
        public static int globalIndex = 0;

        public bool IsValid ()
        {
            return lodData.prefab != null || fadeEffect;
        }
        public void Reset ()
        {
            lodData.Reset ();
            globalObject = false;
        }
        public void Init (ref AABB aabb)
        {
            this.aabb.Init (ref aabb);
        }
        public void Add (ref AABB aabb)
        {
            this.aabb.Encapsulate (ref aabb);
        }

        public static void CalcLod(SceneConfig sceneConfig, ref AABB aabb, ref LodDist ld)
        {
            EngineUtility.CalcLod(ref sceneConfig.lodNearSize, ref sceneConfig.lodFarSize,ref aabb,ref ld);
        }

        public void CalcLod(SceneConfig sceneConfig, GameObject lodPrefab)
        {
            CalcLod(sceneConfig, ref aabb, ref lodData.lodDist);
            lodData.prefab = lodPrefab;
        }
        private void OnDrawGizmosSelected()
        {
            var c = Gizmos.color;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube (aabb.center, aabb.size);
            Gizmos.color = c;
        }
    }
}
#endif
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class MeshCombine : MonoBehaviour
    {
        public MergeObject[] mergeObjects;

        public void Sort ()
        {
            if (mergeObjects != null)
            {
                for (int i = 0; i < mergeObjects.Length; ++i)
                {
                    var mo = mergeObjects[i];
                    if (mo != null)
                    {
                        int count = mo.matGroups.Count;
                        if (count > 0)
                        {
                            int sqrtCount = (int) Mathf.Sqrt (count);
                            int x = 0;
                            int z = 0;
                            int index = 0;
                            while (index < count)
                            {
                                var mg = mo.matGroups[index];
                                mg.mergeMesh.Sort ((xx, yy) => { return xx.mergeObjets.Count - yy.mergeObjets.Count; });
                                mg.transform.localPosition = new Vector3 (x * 2, 0, z * 2);
                                x++;
                                if (x >= sqrtCount)
                                {
                                    x = 0;
                                    z++;
                                }
                                index++;
                            }
                        }
                    }
                }
            }
        }
    }

    [CanEditMultipleObjects, CustomEditor (typeof (MeshCombine))]
    public class MeshCombineEditor : UnityEngineEditor
    {
        public override void OnInspectorGUI ()
        {
            MeshCombine mc = target as MeshCombine;
            if (mc.mergeObjects != null)
            {
                int matGroupCount = 0;
                for (int i = 0; i < mc.mergeObjects.Length; ++i)
                {
                    var mo = mc.mergeObjects[i];
                    matGroupCount += mo.matGroups.Count;
                }
                EditorGUILayout.LabelField (
                    string.Format ("ChunkCount:{0} MatGroupCount:{1}",
                        mc.mergeObjects.Length,
                        matGroupCount));
                if (GUILayout.Button ("Sort", GUILayout.MaxWidth (80)))
                {
                    mc.Sort ();
                }
            }
        }
    }
}
#endif
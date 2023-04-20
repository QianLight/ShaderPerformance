#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{

    public class SubMeshData
    {
        public List<Vector3> pos = new List<Vector3>();
        public List<Vector3> normal = new List<Vector3>();
        public List<Vector4> tangent = new List<Vector4>();
        public List<Vector2> uv = new List<Vector2>();
        public List<Vector2> uv2 = new List<Vector2>();
        public List<Color> color = new List<Color>();

        public List<int> index = new List<int>();
    }

    [System.Serializable]
    public class SubMesh
    {       
        public Mesh m;
        public AABB aabb;
        public ChunkInfo chunkInfo;
        [System.NonSerialized]
        public LodDist lodDist;
        [System.NonSerialized]
        public List<int> index = new List<int>();
        [System.NonSerialized]
        public int id;
        [System.NonSerialized]
        public bool folder;
    }

    
    [System.Serializable]
    public class ChunkSubMesh : ScriptableObject
    {
        public List<SubMesh> subMesh = new List<SubMesh>();
        [System.NonSerialized]
        public bool folder = false;
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < subMesh.Count; ++i)
            {
                var sub = subMesh[i];
                Gizmos.DrawWireCube(sub.aabb.center, sub.aabb.size);
            }
        }

        public void OnInspector()
        {
            folder = EditorGUILayout.Foldout(folder, "SubMeshs");
            if(folder)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < subMesh.Count; ++i)
                {
                    var sub = subMesh[i];
                    if (sub != null && sub.m != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        sub.folder = EditorGUILayout.Foldout(sub.folder, sub.m.name);
                        EditorGUILayout.EndHorizontal();
                        if (sub.folder)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.ObjectField("", sub.m, typeof(Mesh), false);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.Vector3Field("Min", sub.aabb.min);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.Vector3Field("Max", sub.aabb.max);
                            EditorGUILayout.EndHorizontal();
                            EditorGUI.indentLevel--;
                        }                        
                    }
                }
                EditorGUI.indentLevel--;
            }
        }
    }

}
#endif
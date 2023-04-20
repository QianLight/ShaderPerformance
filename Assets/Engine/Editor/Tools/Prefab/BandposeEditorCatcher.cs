using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

#if UNITY_EDITOR

namespace CFEngine.Editor
{
    public class BandposeEditorCatcher :EditorComponetSelectObject<BandposeEditorCatcher>
    {
        private string _name = "Bandpose Editor";
        private BandposeData _bandpose;
        private Vector2 _scrollPosition;

        public override void Init(GameObject selectedGameObject = null)
        {
            if (selectedGameObject)
            {
                Mesh mesh = null;
                SkinnedMeshRenderer meshRenderer = selectedGameObject.GetComponentInChildren<SkinnedMeshRenderer>();
                MeshFilter meshFilter = selectedGameObject.GetComponentInChildren<MeshFilter>();
                if (meshRenderer)
                {
                    mesh = meshRenderer.sharedMesh;
                }
                else
                {
                    mesh = meshFilter.sharedMesh;
                }
                if (mesh)
                {
                    string path = AssetDatabase.GetAssetPath(mesh);
                    path = path.Replace("/"+mesh.name+".asset", "");
                    Debug.Log(path);
                    _bandpose = GetBandpose(path,mesh);
                }
            }
        }

        private static BandposeData GetBandpose(string dirPath, Mesh mesh)
        {
            BandposeData data = null;
            try
            {
                foreach (string path in Directory.GetFiles(dirPath, "*.Asset", SearchOption.AllDirectories))
                {
                    BandposeData bandpose = AssetDatabase.LoadAssetAtPath<BandposeData>(path);
                    if (bandpose)
                    {
                        foreach (var meshMatPair in bandpose.exportMesh)
                        {
                            if (meshMatPair.m.GetHashCode() == mesh.GetHashCode())
                            {
                                data = bandpose;
                                Debug.Log(AssetDatabase.GetAssetPath(bandpose));
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return data;
        }
        public override void DrawGUI()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition,GUILayout.Width(0),GUILayout.Height(0));
            if (_bandpose)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField("Bandpose Data : ", _bandpose,typeof(BandposeData));
                if (GUILayout.Button("Open",GUILayout.Width(100)))
                {
                    PropertyEditor.OpenPropertyEditor(_bandpose);
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("没有找到对应的 Bandpose 文件！", MessageType.Error);
            }
            GUILayout.EndScrollView();
        }

        public override void Destroy()
        {
            
        }
        
        public override string Name()
        {
            return this._name;
        }
    }

}

#endif

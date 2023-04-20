using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace XEditor
{
	class MapEditor : EditorWindow
	{
        private static GUIContent
            ButtonContent = new GUIContent("Generate", "generate map data");
        private static GUIContent
            AddButtonContent = new GUIContent("Add", "Add map data");
        private static GUIContent
           DeleteButtonContent = new GUIContent("Delete", "Delete map data");
        private static GUIContent
            LoadButtonContent = new GUIContent("Load", "load mapheight file");

        MapGenerator _map_generate = new MapGenerator();
        Object _terrianObj;
        List<Object> _terrianObjs = new List<Object>();
        [MenuItem(@"XEditor/Map Editor %k")]
        static void Init()
        {
            EditorWindow.GetWindow(typeof(MapEditor));
        }

        void OnDestroy()
        {
           _map_generate.Reset();
        }

        public void OnGUI()
        {
            EditorGUILayout.Space();

            _map_generate._grid_size = EditorGUILayout.Slider("Grid Size", _map_generate._grid_size, 0.1f, 1);
            _map_generate._inaccuracy = EditorGUILayout.Slider("Height Inaccuracy", _map_generate._inaccuracy, 0.01f, 0.1f);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(ButtonContent))
            {
                GenerateMapData();
            }

            if (GUILayout.Button(LoadButtonContent))
            {
                LoadMapHeight();
            }

            if (GUILayout.Button("Reset"))
            {
                _map_generate.Reset();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label("Physics Data");
            if (GUILayout.Button(AddButtonContent))
            {
                _terrianObjs.Add(null);
            }
            EditorGUILayout.EndHorizontal();
            for (int i=0;i< _terrianObjs.Count;++i)
            {
                EditorGUILayout.BeginHorizontal();
                //Object obj = _terrianObjs[i];
                _terrianObjs[i] = EditorGUILayout.ObjectField(_terrianObjs[i], typeof(GameObject), true);
                if (GUILayout.Button(DeleteButtonContent))
                {
                    _terrianObjs.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }
            //_terrianObj = EditorGUILayout.ObjectField(_terrianObj, typeof(GameObject), true);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(ButtonContent))
            {
                GeneratePhysicsData();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void GenerateMapData()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            //Application.loadedLevelName
            string path = EditorUtility.SaveFilePanel("Select a file to save", XEditorPath.Scb, scene.name + ".bytes", "bytes");

            if(path != null && path != "")
                _map_generate.Generate(path);
        }

        private void LoadMapHeight()
	    {
            string path = EditorUtility.OpenFilePanel("Select a file to load", XEditorPath.Scb, "bytes");

            if(path != null && path != "")
                _map_generate.LoadFromFile(path);
	    }

        private void GeneratePhysicsData()
        {
            //if (_terrianObj == null) return;
            if (_terrianObjs.Count == 0) return;

            string path = EditorUtility.SaveFilePanel("Select a file to save", XEditorPath.Lev, SceneManager.GetActiveScene().name + "_Terrain", "obj");
            StreamWriter sw = File.CreateText(path);
            StringBuilder vertexsb = new StringBuilder();
            StringBuilder indexsb = new StringBuilder();
            int index = 0;
            for (int i = 0; i < _terrianObjs.Count; ++i)
            {
                GameObject go = _terrianObjs[i] as GameObject;
                if (go != null)
                {
                    Transform t = go.transform;
                    Vector3 pos = t.position;
                    MeshFilter mf = go.GetComponent<MeshFilter>();

                    Mesh m = mf.sharedMesh;

                    Vector3[] v = m.vertices;
                    int[] triangles = m.triangles;

                    

                    // 服务器是右手坐标系
                    for (int j = 0; j < v.Length; j++)
                    {
                        string str = string.Format("v {0:#0.0000} {1:#0.0000} {2:#0.0000}", pos.x + v[j].x, pos.y + v[j].z, pos.z - v[j].y);
                        vertexsb.AppendLine(str.Replace("٫", "."));
                        //sw.WriteLine("v " + v[j].x.ToString("#0.0000") + " " + v[j].z.ToString("#0.0000") + " " + v[j].y.ToString("#0.0000"));
                    }

                    for (int j = 0; j < triangles.Length / 3; j++)
                    {
                        indexsb.AppendLine(string.Format("f {0} {1} {2}", triangles[j * 3]+ index, triangles[j * 3 + 1] + index, triangles[j * 3 + 2] + index));
                        
                        //sw.WriteLine("f " + triangles[i * 3] + " " + triangles[i * 3 + 1] + " " + triangles[i * 3 + 2]);
                    }
                    index += v.Length;

                }
            }
            sw.Write(vertexsb.ToString());
            sw.Write(indexsb.ToString());
            sw.Flush();
            sw.Close();

        }
	}
}

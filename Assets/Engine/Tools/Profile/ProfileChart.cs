#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.IO;
using UnityEngineEditor = UnityEditor.Editor;
//using AwesomeCharts;

namespace CFEngine
{
    [System.Serializable]
    public class RecorderCounter
    {
        public string name;
        public bool show;

        public SerializedProperty nameSp;
        public SerializedProperty showSp;
        public GUIContent title;

        public FieldInfo fi;

        public void Reset()
        {
            nameSp = null;
            showSp = null;

            title = null;
            fi = null;
        }
    }

    [DisallowMultipleComponent, ExecuteInEditMode]
    public class ProfileChart : MonoBehaviour
    {
        [System.NonSerialized]
        public Dictionary<string, RecordProfile> profileList = new Dictionary<string, RecordProfile>();

        [System.NonSerialized]
        public RecordProfile current;

        [System.NonSerialized]
        public bool recordFolder;

        [System.NonSerialized]
        public bool profileFolder;

        public List<RecorderCounter> counters = new List<RecorderCounter>();

        //public LineChart lineChart;

        public void Load(string filename)
        {
            if (!profileList.TryGetValue(filename, out var rp))
            {
                rp = EngineProfiler.Load(filename);
                if (rp != null)
                {
                    profileList.Add(filename, rp);
                }
            }

            current = rp;
            //if (current != null&& lineChart!=null)
            //{
            //    for (int i = 0; i < current.records.Count; ++i)
            //    {
            //        var r = current.records[i];
            //        if (r.sceneName == EngineContext.sceneName)
            //        {
            //            lineChart.GetChartData().DataSets.Clear();
            //            for (int k = 0; k < counters.Count; ++k)
            //            {
            //                var counter = counters[k];
            //                if (counter.fi != null)
            //                {
            //                    LineDataSet set = new LineDataSet();
            //                    for (int j = 0; j < r.records.Count; j += 2)
            //                    {
            //                        var min = r.records[j];
            //                        var max = r.records[j + 1];

            //                        float minV = System.Convert.ToSingle(counter.fi.GetValue(min));
            //                        float maxV = System.Convert.ToSingle(counter.fi.GetValue(max));
            //                        float v = (minV + maxV) * 0.5f;
            //                        set.AddEntry(new LineEntry(min.start, v));
                                    
            //                    }
            //                    lineChart.GetChartData().DataSets.Add(set);
            //                    break;
            //                }
            //            }
            //            lineChart.SetDirty();
            //        }
            //    }
            //}

        }


        private void DrawCube(Vector3 pos, bool useX, float offset, float s, float x, float z,float v,string name)
        {
            if (useX)
            {
                pos.x += offset;
            }
            else
            {
                pos.z += offset;
            }
            pos.y += v * 0.5f;
            Gizmos.color = new Color(0, 1, 0, 0.5f);
           
            Gizmos.DrawCube(pos, new Vector3(s, v, s));
            Handles.color = Color.black;
            pos.y += v * 0.5f + 1;
            Handles.Label(pos, name);
        }

        private void OnDrawGizmos()
        {
            if (current != null)
            {
                var c = Gizmos.color;
                
                for (int i = 0; i < current.records.Count; ++i)
                {
                    var r = current.records[i];
                    if (r.sceneName == EngineContext.sceneName)
                    {
                        for (int j = 0; j < r.records.Count; j+=2)
                        {
                            var min = r.records[j];
                            var max = r.records[j + 1];
                            Gizmos.color = Color.red;
                            Gizmos.DrawWireCube(min.cameraRange.center, min.cameraRange.size);
                            float x = min.cameraRange.size.x;
                            float z = min.cameraRange.size.z;
                            bool useX = x> z;
                            float s = useX ?
                                x : z;
                            s /= counters.Count + 1;
                            x *= 0.5f;
                            z *= 0.5f;
                            Vector3 pos = min.cameraRange.min;
                            pos.x = useX ? pos.x : (pos.x + min.cameraRange.max.x) * 0.5f;
                            pos.z = useX ? (pos.z + min.cameraRange.max.z) * 0.5f : pos.z;

                            for (int k = 0; k < counters.Count; ++k)
                            {
                                var counter = counters[k];
                                if (counter.fi != null && counter.show)
                                {
                                    float minV =  System.Convert.ToSingle(counter.fi.GetValue(min));
                                    float maxV = System.Convert.ToSingle(counter.fi.GetValue(max));
                                    float v = (minV + maxV) * 0.5f;
                                    DrawCube(pos, useX, s * (k + 1), s, x, z, v, counter.name);
                                }
                            }
                        }
                    }
                }
                Gizmos.color = c;
            }
        }
    }

    [CustomEditor(typeof(ProfileChart))]
    public class ProfileChartEditor : UnityEngineEditor
    {
        private List<FileInfo> profileList = new List<FileInfo>();
        SerializedProperty counters;
        SerializedProperty lineChart;
        private void OnEnable()
        {
            counters = serializedObject.FindProperty("counters");
            lineChart = serializedObject.FindProperty("lineChart");
            DirectoryInfo di = new DirectoryInfo("Assets/../Dump");
            var files = di.GetFiles("*.profile", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < files.Length; ++i)
            {
                profileList.Add(files[i]);
            }
        }
        private void OnDisable()
        {
            var pc = target as ProfileChart;
            for (int i = 0; i < counters.arraySize; ++i)
            {
                var counter = pc.counters[i];
                counter.Reset();
            }
        }
        public override void OnInspectorGUI ()
        {
            serializedObject.Update();
            var pc = target as ProfileChart;
            EditorGUILayout.PropertyField(lineChart);
            var scType = typeof(StatisticsContext);
            if (GUILayout.Button("Refresh", GUILayout.MaxWidth(80)))
            {
                pc.counters.Clear();
                
                var fields = scType.GetFields();
                foreach (var f in fields)
                {
                    var attrs = f.GetCustomAttributes(false);
                    if (attrs != null && attrs.Length > 0)
                    {
                        pc.counters.Add(new RecorderCounter() { name = f.Name, fi = f });
                    }
                }
                counters = serializedObject.FindProperty("counters");
            }
            for (int i = 0; i < counters.arraySize; ++i)
            {
                var counter = pc.counters[i];
                var counterSp = counters.GetArrayElementAtIndex(i);
                if (counter.showSp == null)
                {
                    counter.showSp = counterSp.FindPropertyRelative("show");
                }
                if (counter.nameSp == null)
                {
                    counter.nameSp = counterSp.FindPropertyRelative("name");
                    counter.title = new GUIContent(counter.nameSp.stringValue);
                }
                if (counter.fi == null)
                {
                    counter.fi = scType.GetField(counter.nameSp.stringValue);

                }
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(counter.showSp, counter.title);
                EditorGUILayout.EndHorizontal();
            }
            if (pc.current != null)
            {
                EditorGUILayout.BeginHorizontal();
                pc.recordFolder = EditorGUILayout.Foldout(pc.recordFolder, "Records");
                EditorGUILayout.EndHorizontal();
                if (pc.recordFolder)
                {
                    for (int i = 0; i < pc.current.records.Count; ++i)
                    {
                        var r = pc.current.records[i];
                        if (r != null)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(r.sceneName);
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
            }


            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            pc.profileFolder = EditorGUILayout.Foldout(pc.profileFolder, "Profiles");
            EditorGUILayout.EndHorizontal();
            if(pc.profileFolder)
            {
                for (int i = 0; i < profileList.Count; ++i)
                {
                    var p = profileList[i];
                    if (p != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(p.Name);
                        if (GUILayout.Button("Load", GUILayout.MaxWidth(80)))
                        {
                            pc.Load(p.FullName);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
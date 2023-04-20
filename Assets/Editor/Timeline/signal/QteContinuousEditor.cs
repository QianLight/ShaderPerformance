//using UnityEditor;
//using UnityEditor.Timeline;
//using UnityEngine;

////[CustomEditor (typeof (QteContinuousSignal))]
//public class QteContinuousEditor : Editor
//{

//    QteContinuousSignal signal;
//    int fx_cnt;
//    bool foldout;
//    GameObject[] arr;

//    SerializedObject so;
//    SerializedProperty m_time;
//    SerializedProperty m_retro;
//    SerializedProperty m_emitonce;
//    SerializedProperty m_btnpos;
//    SerializedProperty m_timeout;
//    SerializedProperty m_duration;
//    SerializedProperty fx_Path;

//    private void OnEnable ()
//    {
//        signal = target as QteContinuousSignal;
//        so = new SerializedObject (signal);
//        m_time = so.FindProperty ("m_Time");
//        m_retro = so.FindProperty ("m_Retroactive");
//        m_emitonce = so.FindProperty ("m_EmitOnce");
//        m_btnpos = so.FindProperty ("m_btnPos");
//        fx_Path = so.FindProperty ("m_fxPath");
//        m_timeout = so.FindProperty ("m_timeout");
//        m_duration = so.FindProperty ("m_duration");
//        if (signal.FxPath != null)
//            fx_cnt = signal.FxPath.Length;
//        foldout = true;
//    }

//    public override void OnInspectorGUI ()
//    {
//        EditorGUILayout.PropertyField (m_time, true);
//        if (m_retro != null)
//            EditorGUILayout.PropertyField (m_retro, true);
//        if (m_emitonce != null)
//            EditorGUILayout.PropertyField (m_emitonce, true);
//        if (m_btnpos != null)
//            EditorGUILayout.PropertyField (m_btnpos, true);
//        if (m_timeout != null)
//        {
//            EditorGUILayout.PropertyField (m_timeout, true);
//            CheckTimeout ();
//        }
//        if (m_duration != null)
//        {
//            EditorGUILayout.PropertyField (m_duration, true);
//            CheckDuration ();
//        }
//        so.ApplyModifiedProperties ();

//        GUILayout.Space (2);
//        DrawFx ();
//    }

//    private void DrawFx ()
//    {
//        GUILayout.Space (6);
//        foldout = EditorGUILayout.Foldout (foldout, "fx config");
//        if (foldout)
//        {
//            EditorGUILayout.BeginVertical ();
//            EditorGUILayout.BeginHorizontal ();
//            EditorGUILayout.LabelField ("fx cnt");
//            fx_cnt = EditorGUILayout.IntSlider (fx_cnt, 0, QteContinuousSignal.maxCount);
//            EditorGUILayout.EndHorizontal ();
//            signal.FxByCnt = MergeFrom (signal.FxByCnt, fx_cnt);
//            signal.FxPath = MergeFrom (signal.FxPath, fx_cnt);
//            arr = MergeFrom (arr, fx_cnt);
//            for (int i = 0; i < fx_cnt; i++)
//            {
//                signal.FxByCnt[i] = EditorGUILayout.IntField (" seq", signal.FxByCnt[i]);
//                arr[i] = EditorGUILayout.ObjectField (" fx", arr[i], typeof (GameObject), true) as GameObject;
//                if (arr[i] != null)
//                {
//                    string path = AssetDatabase.GetAssetPath (arr[i]);
//                    path = path.Replace ("Assets/BundleRes/", "").Replace (".prefab", "");
//                    EditorGUILayout.LabelField (path);
//                    signal.FxPath[i] = path;
//                }
//                else if (!string.IsNullOrEmpty (signal.FxPath[i]))
//                {
//                    string path = signal.FxPath[i];
//                    path = "Assets/BundleRes/" + path + ".prefab";
//                    arr[i] = AssetDatabase.LoadAssetAtPath<GameObject> (path);
//                }
//                if (arr[i] == null)
//                {
//                    EditorGUILayout.HelpBox ("fx can't be null", MessageType.Error);
//                }
//                GUILayout.Space (2);
//            }
//            EditorGUILayout.EndVertical ();
//        }
//    }

//    private void CheckDuration ()
//    {
//        var dir = TimelineEditor.inspectedDirector;
//        if (dir)
//        {
//            float max = (float) (dir.duration - signal.time);
//            EditorGUILayout.LabelField ("  duration range (0-" + max.ToString ("f3") + ")");
//            if (signal.Duration > max)
//            {
//                EditorGUILayout.HelpBox ("The duration overange", MessageType.Error);
//            }
//        }
//        if (signal.Duration < 1e-5)
//        {
//            EditorGUILayout.HelpBox ("The duration has to more than zero", MessageType.Error);
//        }
//    }

//    private void CheckTimeout ()
//    {
//        if (signal.TimeOut <= 1e-5)
//        {
//            EditorGUILayout.HelpBox ("The timeout has to more than zero", MessageType.Error);
//        }
//        if (signal.TimeOut > 10)
//        {
//            EditorGUILayout.HelpBox ("Are you sure to set timeout which is more than 10'", MessageType.Warning);
//        }
//    }

//    protected T[] MergeFrom<T> (T[] orig, int cnt)
//    {
//        if (orig == null)
//        {
//            return new T[cnt];
//        }
//        else
//        {
//            T[] ss = new T[cnt];
//            for (int i = 0; i < Mathf.Min (cnt, orig.Length); i++)
//            {
//                ss[i] = orig[i];
//            }
//            return ss;
//        }
//    }

//}
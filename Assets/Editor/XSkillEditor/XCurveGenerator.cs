using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CFUtilPoolLib;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace XEditor
{
    public class XCurveGenerator : EditorWindow
    {
        private GUIStyle _labelstyle = null;
        private string _data_source = null;
        private string _path = null;

        private string[] _files = null;
        private string _prefab = null;

        private bool _forward = true;
        private bool _side = false;
        private bool _up = false;
        private bool _rotate = false;

        private enum XCurveType
        {
            Forward,
            Side,
            Up,
            Rotation
        }

        void OnGUI ()
        {
            if (_labelstyle == null)
            {
                _labelstyle = new GUIStyle (EditorStyles.boldLabel);
                _labelstyle.fontSize = 13;
            }

            GUILayout.Label (@"Generate a Curve from:", _labelstyle);

            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.TextField ("*Data Source:", _data_source == null ? _path : _data_source);
            if (GUILayout.Button ("File", GUILayout.MaxWidth (80)))
            {
                _data_source = EditorUtility.OpenFilePanel ("Select Data Source", XEditorPath.Crv, "txt");
                if (_data_source.Length > 0)
                {
                    _files = null;

                    string s = Path.GetDirectoryName (_data_source);
                    _prefab = Path.GetFileNameWithoutExtension (s);
                }
                else
                    _data_source = null;
            }
            if (GUILayout.Button ("Folder", GUILayout.MaxWidth (80)))
            {
                _path = EditorUtility.OpenFolderPanel ("Select Data Source", XEditorPath.Crv, "");
                if (_path.Length > 0)
                {
                    _files = Directory.GetFiles (_path);
                    _data_source = null;

                    _prefab = _path.Substring (_path.LastIndexOf ('/') + 1);
                }
                else
                    _path = null;
            }
            EditorGUILayout.EndHorizontal ();

            if (_prefab == "Curve") _prefab = "None";
            EditorGUILayout.LabelField ("Prefab Name:", _prefab);
            EditorGUILayout.Space ();
            EditorGUILayout.BeginHorizontal ();
            _forward = EditorGUILayout.ToggleLeft ("Generate Forward", _forward);
            _side = EditorGUILayout.ToggleLeft ("Generate Side", _side);
            _up = EditorGUILayout.ToggleLeft ("Generate Up", _up);
            _rotate = EditorGUILayout.ToggleLeft ("Generate Rotation", _rotate);
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.Space ();
            EditorGUILayout.Space ();
            EditorGUILayout.Space ();

            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("", GUILayout.MaxWidth (120));
            if (GUILayout.Button ("Generate"))
            {
                if (_files == null)
                {
                    GenerateCurve (_data_source);
                }
                else
                {
                    foreach (string file in _files)
                    {
                        GenerateCurve (file);
                    }
                }

                AssetDatabase.Refresh ();
            }
            EditorGUILayout.LabelField ("", GUILayout.MaxWidth (120));
            EditorGUILayout.EndHorizontal ();
        }

        UnityEngine.Object GenerateCurve (string source)
        {
            UnityEngine.Object prefab = null;

            if (source == null || source.Length == 0) return prefab;

            if (Path.GetExtension (source).ToLower () == ".txt")
            {
                if (File.Exists (source))
                {
                    string name = Path.GetFileNameWithoutExtension (source);

                    using (StreamReader reader = new StreamReader (File.Open (source, FileMode.Open)))
                    {
                        AnimationCurve curve_forward = new AnimationCurve ();
                        AnimationCurve curve_side = new AnimationCurve ();
                        AnimationCurve curve_up = new AnimationCurve ();
                        AnimationCurve curve_rotation = new AnimationCurve ();

                        List<Keyframe> frames_forward = new List<Keyframe> ();
                        List<Keyframe> frames_side = new List<Keyframe> ();
                        List<Keyframe> frames_up = new List<Keyframe> ();
                        List<Keyframe> frames_rotation = new List<Keyframe> ();

                        float base_rotation = 0;
                        int count = 0;

                        while (true)
                        {
                            string data = reader.ReadLine ();
                            if (data == null) break;

                            string[] datas = data.Split ('\t');

                            if (datas.Length > 3)
                            {
                                Keyframe frame = new Keyframe (XParse.Parse (datas[0]), XParse.Parse (datas[1]), 0, 0);
                                frames_forward.Add (frame);
                                frame = new Keyframe (XParse.Parse (datas[0]), XParse.Parse (datas[2]), 0, 0);
                                frames_side.Add (frame);
                                frame = new Keyframe (XParse.Parse (datas[0]), XParse.Parse (datas[3]), 0, 0);
                                frames_up.Add (frame);

                                if (_rotate)
                                {
                                    if (datas.Length > 4)
                                    {
                                        float parse = XParse.Parse (datas[4]);
                                        if (count == 0) base_rotation = parse;

                                        frame = new Keyframe (XParse.Parse (datas[0]), parse, 0, 0);
                                        frames_rotation.Add (frame);
                                    }
                                    else
                                    {
                                        EditorUtility.DisplayDialog ("Confirm your data",
                                            "Please select raw-data file with new format!",
                                            "Ok");
                                        return null;
                                    }
                                }
                            }
                            else
                            {
                                EditorUtility.DisplayDialog ("Confirm your data",
                                    "Please select raw-data file with new format!",
                                    "Ok");
                                return null;
                            }

                            count++;
                        }

                        if (_forward)
                        {
                            float max = 0;
                            float firstland = 0;

                            //RefineData(frames_forward);

                            curve_forward.keys = frames_forward.ToArray ();
                            for (int i = 0; i < curve_forward.keys.Length; ++i)
                            {
                                curve_forward.SmoothTangents (i, 0); //zero weight means average
                            }

                            FindKeyPoint (curve_forward, out max, out firstland);
                            prefab = CreateCurvePrefab (name, _prefab, curve_forward, max, firstland, XCurveType.Forward);
                        }

                        if (_side)
                        {
                            float max = 0;
                            float firstland = 0;

                            //RefineData(frames_side);

                            curve_side.keys = frames_side.ToArray ();
                            for (int i = 0; i < curve_side.keys.Length; ++i)
                            {
                                curve_side.SmoothTangents (i, 0); //zero weight means average
                            }
                            FindKeyPoint (curve_side, out max, out firstland);
                            prefab = CreateCurvePrefab (name, _prefab, curve_side, max, firstland, XCurveType.Side);
                        }

                        if (_up)
                        {
                            float max = 0;
                            float firstland = 0;

                            //RefineData(frames_up);

                            curve_up.keys = frames_up.ToArray ();
                            for (int i = 0; i < curve_up.keys.Length; ++i)
                            {
                                curve_up.SmoothTangents (i, 0); //zero weight means average
                            }
                            FindKeyPoint (curve_up, out max, out firstland);
                            prefab = CreateCurvePrefab (name, _prefab, curve_up, max, firstland, XCurveType.Up);
                        }

                        if (_rotate)
                        {
                            float max = 0;
                            float firstland = 0;

                            RefineData (frames_rotation);

                            curve_rotation.keys = frames_rotation.ToArray ();
                            for (int i = 0; i < curve_rotation.keys.Length; ++i)
                            {
                                curve_rotation.SmoothTangents (i, 0); //zero weight means average
                            }
                            FindKeyPoint (curve_rotation, out max, out firstland);
                            prefab = CreateCurvePrefab (name, _prefab, curve_rotation, max, firstland, XCurveType.Rotation);
                        }
                    }
                }
            }

            return prefab;
        }

        UnityEngine.Object CreateCurvePrefab (string name, string prefab_name, AnimationCurve curve, float maxvalue, float firstland, XCurveType type)
        {
            string path = XEditorPath.GetPath ("Curve" + "/" + prefab_name);

            string server_curve_name = null;
            string fullname = null;

            

            switch (type)
            {
                case XCurveType.Forward:
                    {
                        server_curve_name = name + "_forward";
                        fullname = path + server_curve_name + ".prefab";
                    }
                    break;
                case XCurveType.Side:
                    {
                        server_curve_name = name + "_side";
                        fullname = path + server_curve_name + ".prefab";
                    }
                    break;
                case XCurveType.Up:
                    {
                        server_curve_name = name + "_up";
                        fullname = path + server_curve_name + ".prefab";
                    }
                    break;
                case XCurveType.Rotation:
                    {
                        server_curve_name = name + "_rotation";
                        fullname = path + server_curve_name + ".prefab";
                    }
                    break;
            }
            // GameObject go = new GameObject (server_curve_name);
            // prefab = PrefabUtility.SaveAsPrefabAsset (go, fullname);

            
            GameObject go = new GameObject (server_curve_name);
            XCurve xcurve = go.AddComponent<XCurve> ();
            xcurve.Curve = curve;
            xcurve.Max_Value = maxvalue;
            xcurve.Land_Value = firstland;
            UnityEngine.Object prefab = PrefabUtility.SaveAsPrefabAsset (go, fullname);
            EditorGUIUtility.PingObject (prefab);
            XServerCurveGenerator.GenerateCurve (go, fullname);
            DestroyImmediate (go);

            return prefab;
        }

        void RefineData (List<Keyframe> list)
        {
            int i = list.Count - 1;

            if (list[i].value == 0)
            {
                int j = i - 1;

                while (j >= 0)
                {
                    if (list[j].value != 0)
                        break;
                    j--;
                }

                if (j >= 0 && j + 2 < list.Count)
                {
                    list.RemoveRange (j + 2, i - j - 1);
                }
            }
        }

        void FindKeyPoint (AnimationCurve curve, out float max, out float firstland)
        {
            max = curve[0].value;
            for (int k = 1; k < curve.length; k++)
            {
                float t1 = curve[k - 1].time;
                float t2 = curve[k].time;
                float t3 = (t1 + t2) * 0.5f;

                max = Mathf.Max (max, curve.Evaluate (t1), curve.Evaluate (t2), curve.Evaluate (t3));
            }

            firstland = curve[curve.length - 1].time;
            for (int k = 1; k < curve.length; k++)
            {
                if (curve[k].value < 0.001f && curve[k].value >= 0)
                {
                    firstland = Mathf.Min (firstland, curve[k].time);
                }
            }
        }
    }
}
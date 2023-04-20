using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CFUtilPoolLib;
using ClientEcsData;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace XEditor
{
    class XCurveExtractor : EditorWindow
    {
        [MenuItem (@"XEditor/Generate Curve")]
        public static void CurveOpener ()
        {
            XCurveGenerator xw = EditorWindow.GetWindow<XCurveGenerator> (@"XCurve Generator", typeof (XCurveExtractor));
            xw.minSize = new Vector2(800, 360);
            EditorWindow.GetWindow<XCurveExtractor> (@"y-Rotation Extractor", typeof (XCurveGenerator));

            xw.Focus ();
        }

        private GUIStyle _labelstyle = null;
        private AnimationClip _clip = null;
        private EditorCurveBinding[] _data = null;

        void OnGUI ()
        {
            if (_labelstyle == null)
            {
                _labelstyle = new GUIStyle (EditorStyles.boldLabel);
                _labelstyle.fontSize = 13;
            }

            GUILayout.Label (@"Extract a Curve from:", _labelstyle);

            EditorGUILayout.BeginHorizontal ();
            EditorGUI.BeginChangeCheck ();
            _clip = EditorGUILayout.ObjectField ("Animation Clip", _clip, typeof (AnimationClip), true) as AnimationClip;
            if (EditorGUI.EndChangeCheck ())
            {
                if (_clip != null)
                {
                    // _data = AnimationUtility.GetAllCurves(_clip, true);
                    _data = AnimationUtility.GetCurveBindings (_clip);
                    //_checks = new bool[_data.Length];
                }
                else
                    _data = null;
            }
            if (_data == null) _clip = null;
            EditorGUILayout.EndHorizontal ();

            /*EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (_data != null && _data.Length > 0)
            {
                for (int i = 0; i < _data.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        if (i != 0) EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }
                    _checks[i] = EditorGUILayout.ToggleLeft(_data[i].propertyName, _checks[i]);
                }
                EditorGUILayout.EndHorizontal();
            }*/

            EditorGUILayout.Space ();
            EditorGUILayout.Space ();

            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("", GUILayout.MaxWidth (120));
            if (GUILayout.Button ("Generate"))
            {
                if (_data != null)
                {
                    AnimationCurve curve = new AnimationCurve ();
                    List<Keyframe> frames = new List<Keyframe> ();

                    AnimationCurve curve3 = AnimationUtility.GetEditorCurve (_clip, _data[3]);
                    AnimationCurve curve4 = AnimationUtility.GetEditorCurve (_clip, _data[4]);
                    AnimationCurve curve5 = AnimationUtility.GetEditorCurve (_clip, _data[5]);
                    AnimationCurve curve6 = AnimationUtility.GetEditorCurve (_clip, _data[6]);
                    for (int i = 0; i < curve3.length; i++)
                    {
                        Vector3 y = new Quaternion (
                            curve3.Evaluate (curve3.keys[i].time),
                            curve4.Evaluate (curve4.keys[i].time),
                            curve5.Evaluate (curve5.keys[i].time),
                            curve6.Evaluate (curve6.keys[i].time)).eulerAngles;

                        Keyframe frame = new Keyframe (curve3.keys[i].time, y.y, 0, 0);
                        frames.Add (frame);
                    }
                    // for (int i = 0; i < _data[3].curve.length; i++)
                    // {
                    //     Vector3 y = new Quaternion (
                    //         _data[3].curve.Evaluate (_data[3].curve.keys[i].time),
                    //         _data[4].curve.Evaluate (_data[4].curve.keys[i].time),
                    //         _data[5].curve.Evaluate (_data[5].curve.keys[i].time),
                    //         _data[6].curve.Evaluate (_data[6].curve.keys[i].time)).eulerAngles;

                    //     Keyframe frame = new Keyframe (_data[3].curve.keys[i].time, y.y, 0, 0);
                    //     frames.Add (frame);
                    // }

                    curve.keys = frames.ToArray ();

                    for (int i = 0; i < curve.keys.Length; ++i)
                    {
                        curve.SmoothTangents (i, 0); //zero weight means average
                    }

                    CreateCurvePrefab (_clip.name, curve);
                    AssetDatabase.Refresh ();
                }
            }
            EditorGUILayout.LabelField ("", GUILayout.MaxWidth (120));
            EditorGUILayout.EndHorizontal ();

            GenerateCameraCurve();
        }

        UnityEngine.Object CreateCurvePrefab (string name, AnimationCurve curve)
        {
            string path = XEditorPath.GetPath ("Curve/Auto_Camera");
            string fullname = null;

            // UnityEngine.Object prefab = null;

            fullname = path + name + ".prefab";

            // prefab = PrefabUtility.CreateEmptyPrefab(fullname);

            GameObject go = new GameObject (name);
            XCurve xcurve = go.AddComponent<XCurve> ();
            xcurve.Curve = curve;
            UnityEngine.Object prefab = PrefabUtility.SaveAsPrefabAsset (go, fullname);
            EditorGUIUtility.PingObject (prefab);
            XServerCurveGenerator.GenerateCurve (go, fullname);
            DestroyImmediate (go);

            return prefab;
        }

        private void LoadCurveFromAnim(AnimationClip clip, EditorCurveBinding data, ref AnimCurveData animData)
        {
            switch (data.propertyName)
            {
                case "m_LocalPosition.x":
                    animData.posX = AnimationUtility.GetEditorCurve(clip, data);
                    break;
                case "m_LocalPosition.y":
                    animData.posY = AnimationUtility.GetEditorCurve(clip, data);
                    break;
                case "m_LocalPosition.z":
                    animData.posZ = AnimationUtility.GetEditorCurve(clip, data);
                    break;
                case "m_LocalRotation.x":
                    animData.rotX = AnimationUtility.GetEditorCurve(clip, data);
                    break;
                case "m_LocalRotation.y":
                    animData.rotY = AnimationUtility.GetEditorCurve(clip, data);
                    break;
                case "m_LocalRotation.z":
                    animData.rotZ = AnimationUtility.GetEditorCurve(clip, data);
                    break;
                case "m_LocalRotation.w":
                    animData.rotW = AnimationUtility.GetEditorCurve(clip, data);
                    break;
            }
        }

        private void GenerateCameraCurve()
        {
            GUILayout.Space(10);
            GUILayout.Label(@"Extract CameraCurve", _labelstyle);

            if (GUILayout.Button("Generate CameraCurve"))
            {
                if (_data != null)
                {
                    AnimCurveData camData = new AnimCurveData();

                    for (int i = 0; i < _data.Length; ++i)
                    {
                        switch (_data[i].path)
                        {
                            case "Main Camera":
                                LoadCurveFromAnim(_clip, _data[i], ref camData);
                                break;
                        }
                    }

                    AssetDatabase.CreateAsset(camData, "Assets/BundleRes/Curve/Main_Camera/" + _clip.name + ".asset");
                    
                    AssetDatabase.Refresh();
                }
            }
        }
    }
}
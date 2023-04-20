// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Runtime.InteropServices;
// using CFEngine;
// using Unity.Collections;
// using Unity.Collections.LowLevel.Unsafe;
// using UnityEditor;
// using UnityEditor.SceneManagement;
// using UnityEngine;
// using UnityEngine.Rendering;
// using UnityEngine.SceneManagement;
// using UnityObject = UnityEngine.Object;
// namespace CFEngine.Editor
// {
//     public class SceneDebugTool : EditorWindow
//     {
//         [MenuItem ("Tools/Scene Debug %D", false, 20)]
//         public static void MenuInitEditorWindow ()
//         {
//             EditorWindow.GetWindow<SceneDebugTool> (false).Show ();
//         }

//         class DebugMaterial
//         {
//             public MatProperty mp;
//             public Material dummyMat;
//             public Material srcMat;
//             // public bool folder;
//         }
//         enum DebugCurveType
//         {
//             PosX,
//             PoxY,
//             PoxZ,
//             RotX,
//             RotY,
//             RotZ,
//         }

//         class DebugAnimCurve
//         {
//             public DrawCurveContext dcc = new DrawCurveContext ();
//             public AnimationCurve curve = new AnimationCurve ();
//             public DebugCurveType debugType = DebugCurveType.PosX;
//             // public bool folder;
//         }
//         private bool materialFolder = false;
//         private List<DebugMaterial> debugMaterials = new List<DebugMaterial> ();
//         private Vector2 matListScroll = Vector2.zero;
//         private int materialPreviewIndex = -1;
//         private ReflectFun matGetPropertyFun;
//         private UnityObject[] mats = new UnityObject[1];
//         private object[] paramters = new object[1];
//         private PBSShaderGUI shaderGui = null;
//         private MaterialProperty[] mp = null;
//         private bool animFolder = false;
//         private Vector2 curveListScroll = Vector2.zero;

//         private List<DebugAnimCurve> debugCurves = new List<DebugAnimCurve> ();
//         void OnEnable ()
//         {
//             EngineContext context = EngineContext.instance;
//             if (context != null)
//             {
//                 matGetPropertyFun = EditorCommon.GetMaterialPropertiesFun ();
//                 shaderGui = new PBSShaderGUI ();
//                 debugMaterials.Clear ();
//                 // for (int i = 0; i < context.matPropertyEnd; ++i)
//                 // {
//                 //     MatProperty matProperty = context.globalObjects.Get<MatProperty> (i);
//                 //     Material src = string.IsNullOrEmpty (matProperty.pathInEditor) ?
//                 //         null : AssetDatabase.LoadAssetAtPath<Material> (matProperty.pathInEditor);
//                 //     int matIndex = WorldSystem.GetSceneMatOffset (matProperty.matIndex);
//                 //     Material mat = WorldSystem.GetSceneMat (matIndex, 0);
//                 //     debugMaterials.Add (new DebugMaterial ()
//                 //     {
//                 //         mp = matProperty,
//                 //             dummyMat = mat,
//                 //             srcMat = src
//                 //     });
//                 // }
//                 debugCurves.Clear ();
//                 if (context.animCurve != null)
//                 {
//                     var controlIt = context.controlObjects.GetEnumerator ();
//                     while (controlIt.MoveNext ())
//                     {
//                         var ao = controlIt.Current.Value;
//                         var dc = new DebugAnimCurve ();
//                         dc.dcc.posIndex = ao.pos.curveIndex;
//                         dc.dcc.rotIndex = ao.rot.curveIndex;
//                         context.animCurve.RegisterDrawCurve (dc.dcc);
//                         debugCurves.Add (dc);
//                         ChangeCurveDebugType(dc);
//                     }
//                 }

//             }
//         }

//         void OnGUI ()
//         {
//             EngineContext context = EngineContext.instance;
//             if (context != null)
//             {
//                 OnMaterialGUI ("1.material info", context);
//                 OnAnimationGUI ("2.animation info", context);
//             }

//         }

//         private void OnMaterialGUI (string info, EngineContext context)
//         {
//             materialFolder = EditorGUILayout.Foldout (materialFolder, info);
//             if (!materialFolder)
//                 return;
//             EditorCommon.BeginGroup ("Materials");
//             EditorCommon.BeginScroll (ref matListScroll, debugMaterials.Count);
//             for (int i = 0; i < debugMaterials.Count; ++i)
//             {
//                 var debugMat = debugMaterials[i];
//                 EditorGUILayout.BeginHorizontal ();
//                 EditorGUILayout.BeginVertical ();
//                 EditorGUILayout.ObjectField (debugMat.dummyMat, typeof (Material), false);
//                 EditorGUILayout.EndVertical ();

//                 EditorGUILayout.BeginVertical ();
//                 EditorGUILayout.ObjectField (debugMat.srcMat, typeof (Material), false);
//                 EditorGUILayout.EndVertical ();
//                 if (GUILayout.Button (materialPreviewIndex == i? "UnPreview": "Preview", GUILayout.MaxWidth (100)))
//                 {
//                     if (materialPreviewIndex != i)
//                         materialPreviewIndex = i;
//                     else
//                         materialPreviewIndex = -1;
//                     mp = null;
//                     shaderGui.Reset ();
//                 }

//                 EditorGUILayout.EndHorizontal ();
//             }
//             EditorCommon.EndScroll ();
//             EditorCommon.EndGroup ();
//             EditorCommon.BeginGroup ("Preview");
//             if (materialPreviewIndex >= 0 && materialPreviewIndex < debugMaterials.Count)
//             {
//                 var debugMat = debugMaterials[materialPreviewIndex];
//                 if (matGetPropertyFun != null && mp == null)
//                 {
//                     mats[0] = debugMat.dummyMat;
//                     paramters[0] = mats;
//                     mp = matGetPropertyFun.Call (null, paramters) as MaterialProperty[];
//                     if (mp != null)
//                     {
//                         // for (int i = 0; i < mp.Length; ++i)
//                         // {
//                         //     mp[i].ReadFromMaterialPropertyBlock (debugMat.mp.mpb);
//                         // }
//                     }

//                 }
//                 if (GUILayout.Button ("Flush", GUILayout.MaxWidth (80)))
//                 {
//                     // if (mp != null)
//                     // {
//                     //     for (int i = 0; i < mp.Length; ++i)
//                     //     {
//                     //         mp[i].WriteToMaterialPropertyBlock (debugMat.mp.mpb, 1);
//                     //     }
//                     // }
//                 }
//                 if (mp != null)
//                     shaderGui.OnGUI (debugMat.dummyMat, mp, true);
//             }
//             EditorCommon.EndGroup ();
//         }
//         private void ChangeCurveDebugType (DebugAnimCurve dc)
//         {
//             dc.curve.keys = null;
//             if (dc.debugType < DebugCurveType.RotX)
//             {
//                 int startIndex = (int) dc.debugType;
//                 for (int i = 0; i < dc.dcc.posData.Count; ++i)
//                 {
//                     var pos = dc.dcc.posData[i];
//                     dc.curve.AddKey (pos.w, pos[startIndex]);
//                 }
//             }
//             else
//             {
//                 int startIndex = (int) dc.debugType - (int) DebugCurveType.RotX;
//                 for (int i = 0; i < dc.dcc.rotData.Count; ++i)
//                 {
//                     var rot = dc.dcc.rotData[i];
//                     dc.curve.AddKey (rot.w, rot[startIndex]);
//                 }
//             }

//         }
//         private void OnAnimationGUI (string info, EngineContext context)
//         {

//             animFolder = EditorGUILayout.Foldout (animFolder, info);
//             if (!animFolder)
//                 return;
//             EditorCommon.BeginGroup ("Curves");
//             EditorCommon.BeginScroll (ref curveListScroll, debugCurves.Count);
//             for (int i = 0; i < debugCurves.Count; ++i)
//             {
//                 var debugCurve = debugCurves[i];
//                 EditorGUILayout.BeginHorizontal ();
//                 EditorGUI.BeginChangeCheck ();
//                 debugCurve.debugType = (DebugCurveType) EditorGUILayout.EnumPopup ("CurveType", debugCurve.debugType);
//                 if (EditorGUI.EndChangeCheck ())
//                 {
//                     ChangeCurveDebugType(debugCurve);
//                 }
//                 EditorGUILayout.CurveField (debugCurve.curve);

//                 EditorGUILayout.EndHorizontal ();
//             }
//             EditorCommon.EndScroll ();
//             EditorCommon.EndGroup ();
//         }
//     }

// }
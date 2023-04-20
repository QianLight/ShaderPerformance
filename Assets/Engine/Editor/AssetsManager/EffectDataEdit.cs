// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using System.Reflection;
// using CFEngine;

// using UnityEditor;
// using UnityEngine;

// namespace CFEngine.Editor
// {

//     [CustomEditor (typeof (EditorEffectData))]
//     public class EffectDataEdit : BaseEditor<EditorEffectData>
//     {
//         private EffectData effectData;
//         private EditorEffectData editorEffectData;

//         private GameObject testGo = null;
//         private EffectPreviewInfo epi = null;
    
//         [MenuItem ("Assets/Tool/EffectData_Create")]
//         static void CreateEffectData ()
//         {
//             string path = "Assets/BundleRes/Config/EffectData.asset";
//             if (!File.Exists (path))
//             {
//                 EffectData ef = ScriptableObject.CreateInstance<EffectData> ();
//                 CommonAssets.CreateAsset<EffectData> ("Assets/BundleRes/Config", "EffectData", ".asset", ef);
//             }
//             path = "Assets/Engine/Editor/EditorResources/EditorEffectData.asset";
//             if (!File.Exists (path))
//             {
//                 EditorEffectData efd = ScriptableObject.CreateInstance<EditorEffectData> ();
//                 CommonAssets.CreateAsset<EditorEffectData> ("Assets/Engine/Editor/EditorResources", "EditorEffectData", ".asset", efd);
//             }
//         }

//         private void OnEnable ()
//         {
//             // sceneMats = FindProperty(x => x.sceneMats);
//             // customMats = FindProperty(x => x.customMats);
//             // charMats = FindProperty(x => x.charMats);
//             // effectMats = FindProperty(x => x.effectMats);
//             editorEffectData = AssetDatabase.LoadAssetAtPath<EditorEffectData> ("Assets/Engine/Editor/EditorResources/EditorEffectData.asset");
//             testGo = GameObject.Find ("EffectTestGo");
//             editorEffectData = target as EditorEffectData;
//             effectData = AssetDatabase.LoadAssetAtPath<EffectData> ("Assets/BundleRes/Config/EffectData.asset");

//         }

//         private void DrawMaterial (ref bool folder, Material[] materials, string name)
//         {
//             folder = EditorGUILayout.Foldout (folder, name);
//             if (folder)
//             {
//                 if (materials != null)
//                 {
//                     for (int i = 0; i < materials.Length; ++i)
//                     {
//                         EditorGUILayout.ObjectField (materials[i], typeof (Material), false);
//                     }
//                 }
//             }
//         }
//         public override void OnInspectorGUI ()
//         {
//             if (editorEffectData != null)
//             {
//                 GUILayout.Label ("Effects:", EditorStyles.boldLabel);
//                 for (EffectType effect = EffectType.RimLight; effect < EffectType.Num; ++effect)
//                 {
//                     int effectID = (int) effect;
//                     EditorRenderEffectList renderEffectList = editorEffectData.effects[effectID];
//                     if (renderEffectList == null)
//                     {
//                         renderEffectList = new EditorRenderEffectList ();
//                         editorEffectData.effects[effectID] = renderEffectList;
//                     }
//                     EditorGUILayout.BeginHorizontal ();
//                     renderEffectList.folder = EditorGUILayout.Foldout (renderEffectList.folder, string.Format ("{0}({1})", effect.ToString (), renderEffectList.effectList.Count));
//                     if (renderEffectList.folder)
//                     {
//                         if (GUILayout.Button ("Add", GUILayout.MaxWidth (60)))
//                         {
//                             EditorRenderEffect newRenderEffect = new EditorRenderEffect ();
//                             renderEffectList.effectList.Add (newRenderEffect);
//                         }
//                     }
//                     EditorGUILayout.EndHorizontal ();
//                     if (renderEffectList.folder)
//                     {
//                         EffectPreviewInfo.OnHeadInspectorGUI (epi, effect);
//                         int removeIndex = -1;
//                         for (int i = 0; i < renderEffectList.effectList.Count; ++i)
//                         {
//                             EditorRenderEffect renderEffect = renderEffectList.effectList[i];
//                             if (renderEffect != null)
//                             {
//                                 RenderEffect re = renderEffect.effect;
//                                 short uniqueID = (short) ((effectID + 1) * 100 + i);
//                                 EditorGUILayout.BeginHorizontal ();
//                                 if (GUILayout.Button ("Delete", GUILayout.MaxWidth (60)))
//                                 {
//                                     removeIndex = i;
//                                 }

//                                 if (EffectPreviewInfo.NeedTestButton (effect))
//                                 {
//                                     if (GUILayout.Button ("Test", GUILayout.MaxWidth (60)))
//                                     {
//                                         EffectPreviewInfo.BeginEffect (testGo, effect, uniqueID, re.data, re.fadeInOut);
//                                     }
//                                     if (GUILayout.Button ("EndTest", GUILayout.MaxWidth (60)))
//                                     {
//                                         EffectPreviewInfo.EndEffect (testGo, effect);
//                                     }
//                                 }

//                                 EditorGUILayout.EndHorizontal ();
//                                 EffectPreviewInfo.OnInspectorGUI (epi, effect, uniqueID, ref re.data, ref re.fadeInOut, renderEffect);
//                             }

//                         }
//                         if (removeIndex != -1)
//                         {
//                             renderEffectList.effectList.RemoveAt (removeIndex);
//                         }
//                     }
//                 }
//                 GUILayout.Space (5);
//                 int deleteIndex = -1;
//                 for (int i = 0; i < editorEffectData.predefinedIDNames.Count; ++i)
//                 {
//                     var predefinedIDName = editorEffectData.predefinedIDNames[i];
//                     EditorGUILayout.BeginHorizontal ();
//                     predefinedIDName.name = EditorGUILayout.TextField ("", predefinedIDName.name, GUILayout.MaxWidth (160));
//                     predefinedIDName.id = (short) EditorGUILayout.IntField ("", predefinedIDName.id, GUILayout.MaxWidth (60));
//                     if (GUILayout.Button ("Delete", GUILayout.MaxWidth (60)))
//                     {
//                         deleteIndex = i;
//                     }
//                     EditorGUILayout.EndHorizontal ();
//                 }
//                 if (deleteIndex >= 0)
//                 {
//                     editorEffectData.predefinedIDNames.RemoveAt (deleteIndex);
//                 }

//                 if (GUILayout.Button ("AddPreDefinedID", GUILayout.MaxWidth (160)))
//                 {
//                     editorEffectData.predefinedIDNames.Add (new PredefinedIDName ());
//                 }
//                 GUILayout.Space (5);
//                 deleteIndex = -1;
//                 for (int i = 0; i < editorEffectData.testGO.Count; ++i)
//                 {
//                     TestGameObject testGameObject = editorEffectData.testGO[i];
//                     EditorGUILayout.BeginHorizontal ();
//                     testGameObject.gameObject = EditorGUILayout.ObjectField ("", testGameObject.gameObject, typeof (GameObject), false, GUILayout.MaxWidth (160)) as GameObject;
//                     if (GUILayout.Button ("Delete", GUILayout.MaxWidth (60)))
//                     {
//                         deleteIndex = i;
//                     }
//                     if (GUILayout.Button ("Test", GUILayout.MaxWidth (60)))
//                     {
//                         if (testGameObject.gameObject != null)
//                         {
//                             if (testGo != null)
//                             {
//                                 GameObject.DestroyImmediate (testGo);
//                                 epi = null;
//                                 testGo = null;
//                             }
//                             testGo = PrefabUtility.InstantiatePrefab (testGameObject.gameObject) as GameObject;
//                             testGo.name = "EffectTestGo";
//                             EffectPreviewInfo.isPlayer = testGameObject.isPlayer;
//                             epi = testGo.GetComponent<EffectPreviewInfo> ();
//                             if (epi == null)
//                             {
//                                 epi = testGo.AddComponent<EffectPreviewInfo> ();
//                             }
//                             epi.Init ();
//                         }
//                     }
//                     testGameObject.isPlayer = EditorGUILayout.Toggle ("IsPlayer", testGameObject.isPlayer);
//                     EditorGUILayout.EndHorizontal ();
//                 }
//                 testGo = EditorGUILayout.ObjectField ("TestGo", testGo, typeof (GameObject), true) as GameObject;
//                 if (deleteIndex >= 0)
//                 {
//                     editorEffectData.testGO.RemoveAt (deleteIndex);
//                 }
//                 if (GUILayout.Button ("AddGameObject", GUILayout.MaxWidth (160)))
//                 {
//                     editorEffectData.testGO.Add (new TestGameObject ());
//                 }
//                 GUILayout.Space (5);
//                 if (effectData != null)
//                 {
//                     // DrawMaterial(ref editorEffectData.sceneMatFolder, effectData.sceneMats, "Scene Mats");
//                     // DrawMaterial(ref editorEffectData.customMatFolder, effectData.customMats, "Custom Mats");
//                     // DrawMaterial(ref editorEffectData.charMatFolder, effectData.charMats, "Char Mats");
//                     // DrawMaterial(ref editorEffectData.effectMatFolder, effectData.effectMats, "Effect Mats");
//                 }

//                 GUILayout.Space (5);
//                 // if (GUILayout.Button ("CopyFromEffectData", GUILayout.MaxWidth (100)))
//                 // {
//                 //     if (effectData != null)
//                 //     {
//                 //         for (EffectType i = EffectType.RimLight; i < EffectType.Num; ++i)
//                 //         {
//                 //             var editorEffect = editorEffectData.effects[(int) i];
//                 //             var effect = effectData.effects[(int) i];
//                 //             // editorEffect.effectList.Clear ();
//                 //             for (int j = 0; j < effect.effectList.Count; ++j)
//                 //             {
//                 //                 var e = effect.effectList[j];
//                 //                 EditorRenderEffect ee = null;
//                 //                 if (j >= editorEffect.effectList.Count)
//                 //                 {
//                 //                     ee = new EditorRenderEffect();
//                 //                     editorEffect.effectList.Add(ee);
//                 //                 }
//                 //                 else
//                 //                 {
//                 //                     ee = editorEffect.effectList[j];
//                 //                 }
//                 //                 ee.effect.data = e.data;
//                 //                 ee.effect.fadeInOut = e.fadeInOut;

//                 //             }
//                 //         }
//                 //     }

//                 // }

//                 if (GUILayout.Button ("Save", GUILayout.MaxWidth (100)))
//                 {
//                     if (effectData == null)
//                     {
//                         CreateEffectData ();
//                         effectData = AssetDatabase.LoadAssetAtPath<EffectData> ("Assets/BundleRes/Config/EffectData.asset");
//                     }
//                     if (effectData != null)
//                     {
//                         for (EffectType i = EffectType.RimLight; i < EffectType.Num; ++i)
//                         {
//                             var editorEffect = editorEffectData.effects[(int) i];
//                             var effect = effectData.effects[(int)i];
//                             effect.effectList.Clear();
//                             for (int j = 0; j < editorEffect.effectList.Count; ++j)
//                             {
//                                 var e = editorEffect.effectList[j];
//                                 effect.effectList.Add(new RenderEffect()
//                                 {
//                                     data = e.effect.data,
//                                     fadeInOut = e.effect.fadeInOut
//                                 });

//                             }
//                         }
//                         effectData.predefinedID = new short[editorEffectData.predefinedIDNames.Count];
//                         for (int i = 0; i < editorEffectData.predefinedIDNames.Count; ++i)
//                         {
//                             var pin = editorEffectData.predefinedIDNames[i];
//                             effectData.predefinedID[i] = pin.id;
//                         }

//                         CommonAssets.SaveAsset (effectData);
//                     }

//                     CommonAssets.SaveAsset (editorEffectData);
//                 }
//             }
//             serializedObject.ApplyModifiedProperties ();
//         }
//     }
// }

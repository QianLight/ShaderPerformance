using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace CFEngine.Editor
{
    public class LayerObject
    {
        public List<MeshRenderObject> objList = new List<MeshRenderObject> ();
        public Mesh addMesh = null;
        public bool folder = false;
    }
    public class LayerMesh
    {
        public Dictionary<Mesh, LayerObject> meshList = new Dictionary<Mesh, LayerObject> ();

        public bool folder = false;
    }

    public class LayerObjectTool : CommonToolTemplate
    {
        Dictionary<Material, LayerMesh> layerObjectList =
            new Dictionary<Material, LayerMesh> ();
        HashSet<Shader> layerShader = new HashSet<Shader> ();
        Vector2 m_Scroll = Vector2.zero;
        private int lineCount = 0;
        public override void OnInit ()
        {
            base.OnInit ();
            layerShader.Clear ();
            var layerShaders = AssetsConfig.instance.layerShader;
            if (layerShaders != null)
            {
                for (int i = 0; i < layerShaders.Length; ++i)
                {
                    layerShader.Add (layerShaders[i]);
                }
            }
            ScaneObject ();
        }

        public override void OnUninit ()
        {
            base.OnUninit ();
        }

        public override void DrawGUI (ref Rect rect)
        {
            bool showHide = false;
            bool isShow = false;
            EditorGUILayout.BeginHorizontal ();
            if (GUILayout.Button ("Scan", GUILayout.MaxWidth (80)))
            {
                ScaneObject ();
            }
            if (GUILayout.Button ("ShowAll", GUILayout.MaxWidth (100))) { showHide = true; isShow = true; }
            if (GUILayout.Button ("HideAll", GUILayout.MaxWidth (100))) { showHide = true; isShow = false; }
            EditorGUILayout.EndHorizontal ();

            float height = lineCount * 22;
            if (height > 0 && height > rect.height)
            {
                height = rect.height - 50;
            }
            EditorCommon.BeginScroll (ref m_Scroll, layerObjectList.Count, 10, height);
            lineCount = 0;
            var matIt = layerObjectList.GetEnumerator ();
            while (matIt.MoveNext ())
            {
                var matCurrent = matIt.Current;
                var layerMesh = matCurrent.Value;
                bool showHide2 = false;
                bool isShow2 = false;
                EditorGUILayout.BeginHorizontal ();
                EditorGUILayout.ObjectField ("", matCurrent.Key, typeof (Material), true, GUILayout.MaxWidth (200));
                if (GUILayout.Button (layerMesh.folder? "Hide": "Show", GUILayout.MaxWidth (80)) || showHide)
                {
                    layerMesh.folder = !layerMesh.folder;
                    if (showHide)
                    {
                        layerMesh.folder = isShow;
                    }
                }
                if (GUILayout.Button ("ShowAll", GUILayout.MaxWidth (100))) { showHide2 = true; isShow2 = true; }
                if (GUILayout.Button ("HideAll", GUILayout.MaxWidth (100))) { showHide2 = true; isShow2 = false; }
                EditorGUILayout.EndHorizontal ();
                lineCount++;
                if (showHide2)
                {
                    layerMesh.folder = isShow2;
                }
                if (layerMesh.folder)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.EndHorizontal ();
                    var meshIt = layerMesh.meshList.GetEnumerator ();
                    while (meshIt.MoveNext ())
                    {
                        var meshCurrent = meshIt.Current;
                        var layerObject = meshCurrent.Value;
                        bool applyAddMesh = false;
                        bool hasVC = false;
                        EditorGUILayout.BeginHorizontal ();
                        EditorGUILayout.LabelField ("SrcMesh", GUILayout.MaxWidth (80));
                        EditorGUILayout.ObjectField ("", meshCurrent.Key, typeof (Mesh), true, GUILayout.MaxWidth (200));
                        EditorGUILayout.LabelField ("AddMesh", GUILayout.MaxWidth (80));
                        EditorGUILayout.ObjectField ("", layerObject.addMesh, typeof (Mesh), true, GUILayout.MaxWidth (200));
                        if (GUILayout.Button (layerObject.folder? "Hide": "Show", GUILayout.MaxWidth (80)) || showHide2)
                        {
                            layerObject.folder = !layerObject.folder;
                            if (showHide2)
                            {
                                layerObject.folder = isShow2;
                            }
                        }
                        if (GUILayout.Button ("ApplyAll", GUILayout.MaxWidth (80)))
                        {
                            applyAddMesh = true;
                            hasVC = true;
                        }
                        if (GUILayout.Button ("ClearAll", GUILayout.MaxWidth (80)))
                        {
                            applyAddMesh = true;
                            hasVC = false;
                        }
                        EditorGUILayout.EndHorizontal ();
                        lineCount++;
                        if (layerObject.folder)
                        {
                            EditorGUI.indentLevel++;
                            for (int i = 0; i < layerObject.objList.Count; ++i)
                            {
                                var mro = layerObject.objList[i];
                                EditorGUILayout.BeginHorizontal ();
                                EditorGUILayout.ObjectField ("", mro, typeof (MeshRenderObject), true, GUILayout.MaxWidth (300));
                                EditorGUILayout.LabelField ("IsLayerMesh:", GUILayout.MaxWidth (160));
                                EditorGUI.BeginChangeCheck ();
                                bool hasAddmesh = EditorGUILayout.Toggle ("", mro.additionalVertexStreamMesh != null && mro.additionalVertexStreamMesh == layerObject.addMesh, GUILayout.MaxWidth (80));
                                if (EditorGUI.EndChangeCheck () || applyAddMesh)
                                {
                                    if (applyAddMesh)
                                    {
                                        hasAddmesh = hasVC;
                                    }
                                    if (hasAddmesh)
                                    {
                                        mro.additionalVertexStreamMesh = layerObject.addMesh;
                                    }
                                    else
                                    {
                                        mro.additionalVertexStreamMesh = null;
                                    }
                                    mro.UpdateAddMesh ();
                                }
                                EditorGUILayout.EndHorizontal ();
                                lineCount++;
                            }
                            EditorGUI.indentLevel--;
                        }
                    }

                    EditorGUI.indentLevel--;
                }
            }
            EditorCommon.EndScroll ();

        }

        private void ScaneObject ()
        {
            //EditorCommon.EnumTransform funPrefabs = null;
            //funPrefabs = (trans, param) =>
            //{
            //    if (EditorCommon.IsPrefabOrFbx (trans.gameObject))
            //    {
            //        var lot = param as LayerObjectTool;
            //        var monos = EditorCommon.GetScripts<MeshRenderObject> (trans.gameObject);
            //        for (int i = 0; i < monos.Count; ++i)
            //        {
            //            var mro = monos[i] as MeshRenderObject;
            //            var mat = mro.GetMat ();
            //            if (mat != null)
            //            {
            //                var shader = mat.shader;
            //                if (lot.layerShader.Contains (shader))
            //                {
            //                    LayerMesh meshList;
            //                    if (!lot.layerObjectList.TryGetValue (mat, out meshList))
            //                    {
            //                        meshList = new LayerMesh ();
            //                        lot.layerObjectList.Add (mat, meshList);
            //                    }
            //                    var mesh = mro.GetMesh ();
            //                    LayerObject layerObject;
            //                    if (!meshList.meshList.TryGetValue (mesh, out layerObject))
            //                    {
            //                        layerObject = new LayerObject ();
            //                        meshList.meshList.Add (mesh, layerObject);
            //                        string fbxPath;
            //                        string dir = AssetsPath.GetAssetDir (mesh, out fbxPath);
            //                        string fbxname;
            //                        if (AssetsPath.GetFileName (fbxPath, out fbxname))
            //                        {
            //                            string meshPath = string.Format ("{0}/{1}_{2}_vc.asset", dir, fbxname, mesh.name);
            //                            layerObject.addMesh = AssetDatabase.LoadAssetAtPath<Mesh> (meshPath);
            //                        }

            //                    }

            //                    layerObject.objList.Add (mro);
            //                }
            //            }
            //        }
            //    }
            //    else
            //    {
            //        EditorCommon.EnumChildObject (trans, param, funPrefabs);
            //    }

            //};

            //layerObjectList.Clear ();
            //EditorCommon.EnumPath (EditorSceneObjectType.StaticPrefab, funPrefabs, this);
            //EditorCommon.EnumPath (EditorSceneObjectType.Prefab, funPrefabs, this);
            //lineCount = -1;
        }
    }
}
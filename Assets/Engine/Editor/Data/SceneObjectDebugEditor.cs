using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine.Editor
{
    [CanEditMultipleObjects, CustomEditor (typeof (SceneObjectDebug))]
    public class SceneObjectDebugEditor : UnityEngineEditor
    {
        MaterialProperty[] srcProp = null;
        MaterialProperty[] runtimeProp = null;
        MaterialEditor srcMatEditor;
        MaterialEditor runtimeMatEditor;
        static PBSShaderGUI gui = null;
        static UnityEngine.Object[] mat = new UnityEngine.Object[1];
        private void OnEnable ()
        {
            if (gui == null)
                gui = new PBSShaderGUI ();

            SceneObjectDebug sod = target as SceneObjectDebug;
            var so = sod.so;
            if (so != null)
            {
                var mi = so.mi;
                if (mi != null && mi.mpRef != null)
                {
                    var srcMat = mi.mpRef.GetSrcMat ();
                    if (srcMat != null)
                    {
                        mat[0] = srcMat;
                        srcProp = MaterialEditor.GetMaterialProperties (mat);
                        srcMatEditor = MaterialEditor.CreateEditor (srcMat) as MaterialEditor;
                    }
                    if (mi.renderMat != null)
                    {
                        mat[0] = mi.renderMat;       
                        runtimeProp = MaterialEditor.GetMaterialProperties(mat);
                        runtimeProp = srcProp;
                        runtimeMatEditor = MaterialEditor.CreateEditor(mi.renderMat) as MaterialEditor;
                    }
                }
            }
        }

        private void LodGUI (ref LodDist ld, AssetHandler ah)
        {
            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.EnumPopup ("LodLevel", ld.lodLevel);
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.FloatField ("LodDist2", ld.lodDist2);
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.FloatField ("FadeDist2", ld.fadeDist2);
            EditorGUILayout.EndHorizontal ();

            if (ah != null)
            {
                EditorGUILayout.BeginHorizontal ();
                EditorGUILayout.LabelField ("LodPath", ah.path);
                EditorGUILayout.EndHorizontal ();
                EditorGUILayout.BeginHorizontal ();
                EditorGUILayout.ObjectField ("Lod", ah.obj as Mesh, typeof (Mesh), false);
                EditorGUILayout.EndHorizontal ();
            }

        }

        public override void OnInspectorGUI ()
        {
            SceneObjectDebug sod = target as SceneObjectDebug;
            var so = sod.so;
            if (so != null)
            {
                EditorGUILayout.BeginHorizontal ();
                EditorGUILayout.LabelField (so.id.ToString ());
                EditorGUILayout.EndHorizontal ();
                sod.resFolder = EditorGUILayout.Foldout (sod.resFolder, "Res");
                if (sod.resFolder)
                {
                    if (so.asset != null)
                    {
                        EditorGUILayout.BeginHorizontal ();
                        EditorGUILayout.LabelField ("MeshPath", so.asset.path);
                        EditorGUILayout.EndHorizontal ();
                        EditorGUILayout.BeginHorizontal ();
                        EditorGUILayout.ObjectField ("Mesh", so.asset.obj as Mesh, typeof (Mesh), false);
                        EditorGUILayout.EndHorizontal ();
                    }
                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.IntField ("Instance", so.instanceCount);
                    EditorGUILayout.EndHorizontal ();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Toggle("HideRender", so.HasFlag(SceneObject.HideRender));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.Toggle ("HasAnim", so.HasFlag (SceneObject.HasAnim));
                    EditorGUILayout.EndHorizontal ();
                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.IntField ("AreaMask", (int) so.areaMask);
                    EditorGUILayout.EndHorizontal ();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Toggle("PVSValid", so.pvsValid);
                    EditorGUILayout.EndHorizontal();
                    var mi = so.mi;
                    if (mi != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.IntField("MatLod", mi.matLod);
                        EditorGUILayout.EndHorizontal();
                        sod.srcMatFolder = EditorGUILayout.Foldout (sod.srcMatFolder, "SrcMat");
                        if (sod.srcMatFolder)
                        {
                            if (mi.mpRef != null)
                            {
                                var srcMat = mi.mpRef.GetSrcMat ();
                                EditorGUILayout.BeginHorizontal ();
                                EditorGUILayout.LabelField (mi.mpRef.matPath);
                                EditorGUILayout.EndHorizontal ();
                                EditorGUILayout.BeginHorizontal ();
                                EditorGUILayout.ObjectField ("srcMat", srcMat, typeof (Material), false);
                                EditorGUILayout.EndHorizontal ();
                                  

                                if (gui != null && srcMatEditor != null)
                                {
                                    gui.OnGUI (srcMatEditor, srcProp);
                                    
                                }
                            }
                        }

                        sod.runtimMatFolder = EditorGUILayout.Foldout (sod.runtimMatFolder, "RuntimeMat");
                        if (sod.runtimMatFolder)
                        {
                            EditorGUILayout.BeginHorizontal ();
                            EditorGUILayout.ObjectField ("mat", mi.renderMat, typeof (Material), false);
                            EditorGUILayout.EndHorizontal ();

                            if (gui != null && runtimeMatEditor != null)
                            {
                                gui.OnGUI(runtimeMatEditor, runtimeProp);
                            }

                            if (mi.mpRef != null)
                            {
                                var mp = so.mi.mpRef;
                                var context = EngineContext.instance;
                                EditorGUI.indentLevel++;
                                for (int i = mp.resIndexStart; i < mp.resIndexEnd; ++i)
                                {
                                    ResObject ro = context.matContext.resObjects.Get<ResObject>(i);
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField(ro.asset != null ? ro.asset.path : null);
                                    EditorGUILayout.EndHorizontal();
                                    if (ResObject.IsAssetValid(ro))
                                    {
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.ObjectField("Tex", ro.asset.obj as Texture, typeof(Texture), false);
                                        EditorGUILayout.EndHorizontal();
                                    }
                                }
                                EditorGUI.indentLevel--;
                            }
                        }
                        if (mi.lightmapRes != null)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("lightmapPath", mi.lightmapRes.path);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.Vector4Field("lightmapUVST", mi.lightmapUVST);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.ObjectField("lightmap", mi.lightmapRes.obj as Texture, typeof(Texture), false);
                            EditorGUILayout.EndHorizontal();
                        }
                    }

                }
                sod.renderFolder = EditorGUILayout.Foldout (sod.resFolder, "Render");
                if (sod.renderFolder)
                {
                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.Vector3Field ("Min", so.aabb.min);
                    EditorGUILayout.EndHorizontal ();
                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.Vector3Field ("Max", so.aabb.max);
                    EditorGUILayout.EndHorizontal ();

                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.Toggle ("Render", so.draw);
                    EditorGUILayout.EndHorizontal ();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Toggle("IsHide", so.HasFlag(SceneObject.HideRender));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Toggle("OnlyCastShadow", so.HasFlag(SceneObject.OnlyCastShadow));
                    EditorGUILayout.EndHorizontal();
                }
                sod.shadowFolder = EditorGUILayout.Foldout (sod.shadowFolder, "Shadow");
                if (sod.shadowFolder)
                {
                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.Toggle ("CastShadow", !so.HasFlag(SceneObject.IgnoreShadowCaster));
                    EditorGUILayout.EndHorizontal ();
                }

                sod.miscFolder = EditorGUILayout.Foldout (sod.miscFolder, "Misc");
                if (sod.miscFolder)
                {
                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.EnumPopup ("RayAABB", so.rayAABB);
                    EditorGUILayout.EndHorizontal ();

                    if (so.groupObjectRef != null)
                    {
                        EditorGUILayout.LabelField ("GroupLod");
                        LodGUI (ref so.groupObjectRef.lodData.lodDist,
                            so.groupObjectRef.lodData.hlod);
                    }
                    else
                    {
                        EditorGUILayout.LabelField ("SingleLod");
                        LodGUI (ref so.lodDist, so.tmpLodAsset);
                    }
                }
            }
        }
    }
}
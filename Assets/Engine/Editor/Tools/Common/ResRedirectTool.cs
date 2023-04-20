using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace CFEngine.Editor
{
    public enum EShaderPropertyType
    {
        Tex,
        Vector,
        Color
    }
    public partial class ResRedirectTool : BaseConfigTool<EditorResRedirectConfig>
    {
        public ResRedirectConfig rrc;
        private Vector2 resScroll;
        private Vector2 mpScroll;
        public override void OnInit ()
        {
            base.OnInit ();
            config = EditorResRedirectConfig.instance;
            string path = string.Format ("{0}EditorConfig/ResRedirectConfig.asset",
                LoadMgr.singleton.EngineResPath);
            if (File.Exists (path))
                rrc = AssetDatabase.LoadAssetAtPath<ResRedirectConfig> (path);
            if (rrc == null)
            {
                rrc = ScriptableObject.CreateInstance<ResRedirectConfig> ();
                rrc = CommonAssets.CreateAsset<ResRedirectConfig> (path, ".asset", rrc);
                // WorldSystem.miscConfig = qsd;
            }
        }
        protected override void OnSave ()
        {
            if (rrc != null)
            {
                EditorCommon.SaveAsset (rrc);
            }
        }

        protected override void OnConfigGui(ref Rect rect)
        {
            if (config.folder.Folder("ResRedirect", "ResRedirect"))
            {
                if (rrc != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Add", GUILayout.MaxWidth(100)))
                    {
                        rrc.resPath.Add(new ResPathRedirect());
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel++;

                    resScroll = EditorGUILayout.BeginScrollView(resScroll);
                    int removeIndex = -1;
                    for (int i = 0; i < rrc.resPath.Count; ++i)
                    {
                        var rp = rrc.resPath[i];
                        EditorGUILayout.BeginHorizontal();
                        rp.name = EditorGUILayout.TextField("Path", rp.name, GUILayout.MaxWidth(rect.width * 0.5f));
                        EditorGUILayout.LabelField(rp.ext, GUILayout.MaxWidth(100));
                        if (GUILayout.Button("Delete", GUILayout.MaxWidth(80)))
                        {
                            removeIndex = i;
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("PhysicPath", rp.physicPath, GUILayout.MaxWidth(rect.width - 20));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUI.BeginChangeCheck();
                        rp.asset = EditorGUILayout.ObjectField("Asset",
                            rp.asset, typeof(UnityEngine.Object), false, GUILayout.MaxWidth(rect.width * 0.5f));
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (rp.asset == null)
                            {
                                rp.name = "";
                                rp.physicPath = "";
                                rp.ext = "";
                                rp.targetPath = "";
                            }
                            else
                            {
                                rp.name = rp.asset.name.ToLower();
                                rp.physicPath = AssetsPath.GetAssetDir(rp.asset, out var path) + "/";
                                rp.ext = Path.GetExtension(path);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        rp.targetPath = EditorGUILayout.TextField("TargetPath", rp.targetPath, GUILayout.MaxWidth(300));
                        rp.buildBundle = EditorGUILayout.Toggle("BuildBundle", rp.buildBundle, GUILayout.MaxWidth(300));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space();
                    }
                    if (removeIndex >= 0)
                    {
                        rrc.resPath.RemoveAt(removeIndex);
                    }
                    EditorGUILayout.EndScrollView();
                    EditorGUI.indentLevel--;
                }
            }

            if (config.folder.Folder("MatPropertyCopy", "MatPropertyCopy"))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add", GUILayout.MaxWidth(100)))
                {
                    rrc.matProperty.Add(new MatPropertyCopy());
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel++;

                mpScroll = EditorGUILayout.BeginScrollView(mpScroll);
                int removeIndex = -1;
                for (int i = 0; i < rrc.matProperty.Count; ++i)
                {
                    var mp = rrc.matProperty[i];
                    EditorGUILayout.BeginHorizontal();
                    mp.mat = EditorGUILayout.ObjectField("Mat", mp.mat, typeof(Material),false,GUILayout.MaxWidth(300)) as Material;
                    if (GUILayout.Button("Delete", GUILayout.MaxWidth(80)))
                    {
                        removeIndex = i;
                    }
                    if (GUILayout.Button("AddProperty", GUILayout.MaxWidth(100)))
                    {
                        mp.propertys.Add(new MatPropertyInfo());
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel++;
                    int delete = -1;
                    for (int j = 0; j < mp.propertys.Count; ++j)
                    {
                        var mpi = mp.propertys[j];
                        EditorGUILayout.BeginHorizontal();
                        mpi.key = EditorGUILayout.TextField("", mpi.key, GUILayout.MaxWidth(300));
                        mpi.type = (int)(EShaderPropertyType)EditorGUILayout.EnumPopup((EShaderPropertyType)mpi.type, GUILayout.MaxWidth(200));
                        if (GUILayout.Button("D", GUILayout.MaxWidth(40)))
                        {
                            delete = j;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    if (delete >= 0)
                    {
                        mp.propertys.RemoveAt(delete);
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }
                if (removeIndex >= 0)
                {
                    rrc.matProperty.RemoveAt(removeIndex);
                }
                EditorGUILayout.EndScrollView();
                EditorGUI.indentLevel--;
            }
        }
    }
}
using System;
using UnityEditor;
using UnityEngine;

using MeshConfigListEdior = CFEngine.Editor.CommonListEditor<CFEngine.MeshConfigData>;
using MeshConfigListContext = CFEngine.Editor.AssetListContext<CFEngine.MeshConfigData>;

using TexConfigListEdior = CFEngine.Editor.CommonListEditor<CFEngine.TexConfigData>;
using TexConfigListContext = CFEngine.Editor.AssetListContext<CFEngine.TexConfigData>;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CFEngine.Editor
{
    public partial class AssetsConfigTool
    {
        private MeshConfigListContext meshConfigContext;
        private MeshConfigListEdior meshConfigEditor;

        private TexConfigListContext texConfigContext;
        private TexConfigListEdior texConfigEditor;
        private Vector2 unResizeScroll = Vector2.zero;
        private Vector2 totalConfigScroll;
            
        private void InitMeshTex ()
        {
            meshConfigContext.elementGUI = MatConfigGUI;
            meshConfigContext.needDelete = true;
            meshConfigContext.needAdd = true;

            config.meshConfig.name = "MeshConfig";
            meshConfigEditor = new MeshConfigListEdior (config.meshConfig, ref meshConfigContext);

            texConfigContext.elementGUI = TexConfigGUI;
            texConfigContext.needDelete = true;
            texConfigContext.needAdd = true;

            config.texConfig.name = "TexConfig";
            texConfigEditor = new TexConfigListEdior (config.texConfig, ref texConfigContext);

        }

        private void AddUnResizeTex(Texture tex, List<string> unResizeTexList)
        {

            if (tex != null)
            {
                string path = AssetDatabase.GetAssetPath(tex);
                unResizeTexList.Add(path);
            }
        }
        private void MeshTexGUI (ref Rect rect)
        {
            

            if (config.folder.FolderGroup ("MeshTex", "MeshTex", 10000))
            {
                meshConfigEditor.Draw (config.folder, ref rect);
                totalConfigScroll = EditorGUILayout.BeginScrollView(totalConfigScroll, false, true,GUILayout.MaxHeight(800),GUILayout.MinWidth(600), GUILayout.MinHeight(600));
                texConfigEditor.Draw (config.folder, ref rect);
                EditorGUILayout.EndScrollView();
                if (config.folder.Folder("unResizeTex", "unResizeTex"))
                {
                    Texture tex = null;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    tex = EditorGUILayout.ObjectField("Tex", tex, typeof(Texture),false,GUILayout.MaxWidth(300)) as Texture;
                    if(EditorGUI.EndChangeCheck())
                    {
                        AddUnResizeTex(tex, config.texConfig.unResizePath);
                    }
                    string folder = "";
                    ToolsUtility.FolderSelect(ref folder);
                    if(!string.IsNullOrEmpty(folder))
                    {
                        var guids = AssetDatabase.FindAssets("t:Texture", new string[] { folder });
                        foreach(var guid in guids)
                        {
                            string texPath = AssetDatabase.GUIDToAssetPath(guid);
                            if(!config.texConfig.unResizePath.Contains(texPath))
                            {
                                config.texConfig.unResizePath.Add(texPath);
                            }
                            
                        }
                        
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorCommon.BeginScroll(ref unResizeScroll, config.texConfig.unResizePath.Count, 20);

                    int deleteIndex = ToolsUtility.BeginDelete();
                    for (int i = 0; i < config.texConfig.unResizePath.Count; ++i)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(config.texConfig.unResizePath[i]);
                        ToolsUtility.DeleteButton(ref deleteIndex, i);
                        EditorGUILayout.EndHorizontal();
                    }
                    ToolsUtility.EndDelete(deleteIndex, config.texConfig.unResizePath);
                    EditorCommon.EndScroll();
                }
                EditorCommon.EndFolderGroup();
            }
            
        }

        private void MatConfigGUI (ref ListElementContext lec, ref MeshConfigListContext context, MeshConfigData data, int i)
        {
            var mc = data.meshConfigs[i];
            ToolsUtility.InitListContext (ref lec, context.defaultHeight);

            ToolsUtility.Label (ref lec, string.IsNullOrEmpty (mc.name) ? "empty" : mc.name, 100, true);
            string folderPath = mc.GetHash ();
            if (ToolsUtility.SHButton (ref lec, config.folder, folderPath))
            {
                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.TextField (ref lec, "Name", 120, ref mc.name, 400, true);
                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.Toggle  (ref lec, "IsExport", 120, ref mc.isExport, true);
                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.TextField (ref lec, "Export", 120, ref mc.exportDir, 400, true);
                // ToolsUtility.NewLine (ref lec, 20);
                // ToolsUtility.IntField(ref lec, "Max Triangular", 180, ref MeshOptimizeConfig.maxTriangular, 200, true);
                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.Toggle (ref lec, "IsReadable", 120, ref mc.isReadable, true);
                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.Toggle (ref lec, "Export Readable", 120, ref mc.exportReadable, true);
                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.Toggle (ref lec, "Remove UV2", 120, ref mc.removeUV2, true);
                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.Toggle (ref lec, "Remove Color", 120, ref mc.removeColor, true);
                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.Toggle (ref lec, "Rotate 90", 120, ref mc.rotate90, true);
                ToolsUtility.NewLine(ref lec, 20);
                ToolsUtility.Toggle(ref lec, "Import Camera", 120, ref mc.importCamera, true);
                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.Toggle(ref lec, "Override Import Normals", 250, ref mc.overrideImportNormals, true);
                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.EnumPopup(ref lec, "Import Normals", 180, ref mc.importNormal, 200, true);
                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.Toggle(ref lec, "Override Import Tangents", 250, ref mc.overrideImportTangents, true);
                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.EnumPopup(ref lec, "Import Tangents", 180, ref mc.importTangent, 200, true);
                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.Toggle(ref lec, "Override Import BlendShape Normals", 250, ref mc.overrideImportBlendShapeNormals, true);
                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.EnumPopup(ref lec, "Import BlendShape Normals", 180, ref mc.importBlendShapeNormals, 200, true);
                ToolsUtility.NewLine (ref lec, 20);
                if (ToolsUtility.Button (ref lec, "AddFilter", 120, true))
                {
                    mc.filters.Add ("");
                }
                ToolsUtility.NewLine (ref lec, 20);
                Enum type = mc.type;
                ToolsUtility.EnumPopup (ref lec, "FilterType", 100, ref type, 100, true);
                mc.type = (TexFilterType) type;
                
                int deleteIndex = ToolsUtility.BeginDelete ();
                for (int j = 0; j < mc.filters.Count; ++j)
                {
                    var str = mc.filters[j];
                    ToolsUtility.NewLine (ref lec, 30);
                    ToolsUtility.TextField (ref lec, "", 0, ref str, 400, true);
                    mc.filters[j] = str;
                    ToolsUtility.DeleteButton (ref deleteIndex, j, ref lec);
                }
                ToolsUtility.EndDelete (deleteIndex, mc.filters);
                
                ToolsUtility.NewLine (ref lec, 20);
                if (ToolsUtility.Button (ref lec, "重新导入相关模型", 120, true))
                {
                    string[] guids = AssetDatabase.FindAssets("t:model");
                    List<string> paths = new List<string>();
                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        MeshOptimizeConfig config = FBXAssets.FindMeshConfig(path);
                        if (config == mc)
                        {
                            paths.Add(path);
                        }
                    }

                    foreach (string path in paths)
                    {
                        Debug.Log(path);
                    }
                    if (paths.Count == 0)
                    {
                        EditorUtility.DisplayDialog("重新导入符合该设置的资源", $"找不到符合条件的资源", "确定");
                    }
                    else if (EditorUtility.DisplayDialog("重新导入符合该设置的资源", $"确定重新导入符合该设置的资源，总共{paths.Count}个？", "确定","取消"))
                    {
                        foreach (string path in paths)
                        {
                            ModelImporter mi = AssetImporter.GetAtPath(path) as ModelImporter; 
                            FBXAssets.ProcessModelImporterByConfig(mi, mc);
                            AssetDatabase.ImportAsset(path);
                        }
                    }
                    
                    // if (EditorUtility.DisplayDialog($"重新导入{paths.Count}个资源", $"确定重新导入{paths.Count}个模型资源吗？", "确定", "取消"))
                    // {
                    //     foreach (string path in paths)
                    //     {
                    //         ModelImporter mi = AssetImporter.GetAtPath(path) as ModelImporter; 
                    //         FBXAssets.ProcessModelImporterByConfig(mi, mc);
                    //         AssetDatabase.ImportAsset(path);
                    //     }
                    // }
                }
            }
        }
        private void TexturePlatformConfigGUI (ref ListElementContext lec, TexImportSetting tis, string name, string tcName)
        {
            ToolsUtility.NewLine (ref lec, 25);
            string folderPath = string.Format ("TexPlatform_{0}_{1}", tcName, name);
            if (ToolsUtility.Foldout (ref lec, config.folder, folderPath, name, 100, true))
            {
                ToolsUtility.NewLine (ref lec, 30);
                Enum maxTextureSize = tis.maxTextureSize;
                ToolsUtility.EnumPopup (ref lec, "Size", 120, ref maxTextureSize, 160, true);
                tis.maxTextureSize = (SpriteSize) maxTextureSize;

                ToolsUtility.NewLine (ref lec, 30);
                Enum format = tis.format;
                ToolsUtility.EnumPopup (ref lec, "Format", 120, ref format, 160, true);
                tis.format = (TextureImporterFormat) format;

                ToolsUtility.NewLine (ref lec, 30);
                Enum alphaFormat = tis.alphaFormat;
                ToolsUtility.EnumPopup (ref lec, "Format_A", 120, ref alphaFormat, 160, true);
                tis.alphaFormat = (TextureImporterFormat) alphaFormat;
                
                ToolsUtility.NewLine (ref lec, 30);
                Enum texCompressorQuality = tis.texCompressorQuality;
                ToolsUtility.EnumPopup (ref lec, "CompressorQuality", 120, ref texCompressorQuality, 160, true);
                tis.texCompressorQuality = (TextureCompressionQuality) texCompressorQuality;
            }
        }
        private void TexConfigGUI (ref ListElementContext lec, ref TexConfigListContext context, TexConfigData data, int i)
        {
            var tc = data.texConfigs[i];
            ToolsUtility.InitListContext (ref lec, context.defaultHeight);

            ToolsUtility.Label (ref lec, string.IsNullOrEmpty (tc.name) ? "empty" : tc.name, 100, true);

            string folderPath = tc.GetHash ();
            if (ToolsUtility.SHButton (ref lec, config.folder, folderPath))
            {
                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.Toggle (ref lec, "IsValid", 120, ref tc.vaild, true);
                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.Toggle (ref lec, "attrOverride", 120, ref tc.attrOverride, true);
                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.TextField (ref lec, "Name", 120, ref tc.name, 400, true);
                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.IntField (ref lec, "Priority", 120, ref tc.priority, 400, true);
                
                

                ToolsUtility.NewLine (ref lec, 20);
                Enum importType = tc.importType;
                ToolsUtility.EnumPopup (ref lec, "Type", 120, ref importType, 100, true);
                tc.importType = (TextureImporterType) importType;

                ToolsUtility.NewLine (ref lec, 20);
                Enum importShape = tc.importShape;
                ToolsUtility.EnumPopup (ref lec, "Shape", 120, ref importShape, 100, true);
                tc.importShape = (TextureImporterShape) importShape;

                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.Toggle (ref lec, "sRGB", 120, ref tc.sRGB, true);
                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.Toggle (ref lec, "Mipmap", 120, ref tc.mipMap, true);
                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.FloatField (ref lec, "MipMapBias", 120, ref tc.minBias, 400, true);
                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.Toggle (ref lec, "Readable", 120, ref tc.isReadable, true);

                ToolsUtility.NewLine (ref lec, 20);
                Enum filterMode = tc.filterMode;
                ToolsUtility.EnumPopup (ref lec, "Filter", 120, ref filterMode, 100, true);
                tc.filterMode = (FilterMode) filterMode;

                ToolsUtility.NewLine (ref lec, 20);

                Enum wrapMode = tc.wrapMode;
                ToolsUtility.EnumPopup (ref lec, "Wrap", 120, ref wrapMode, 100, true);
                tc.wrapMode = (TextureWrapMode) wrapMode;
                ToolsUtility.Toggle (ref lec, "Override", 120, ref tc.isOverride);

                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.IntSlider (ref lec, "AnisoLevel", 120, ref tc.anisoLevel, 0, 3, 300, true);

                TexturePlatformConfigGUI (ref lec, tc.iosSetting, "iOS", folderPath);
                TexturePlatformConfigGUI (ref lec, tc.androidSetting, "Android", folderPath);
                TexturePlatformConfigGUI (ref lec, tc.standaloneSetting, "Standalone", folderPath);

                ToolsUtility.NewLine (ref lec, 20);
                if (ToolsUtility.Button (ref lec, "AddFilter", 120, true))
                {
                    tc.compressFilters.Add (new TexCompressFilter ());
                }
                ToolsUtility.NewLine (ref lec, 20);
                Enum type = tc.type;
                ToolsUtility.EnumPopup (ref lec, "FilterType", 100, ref type, 100, true);
                tc.type = (TexFilterType) type;

                int deleteIndex = ToolsUtility.BeginDelete ();
                for (int j = 0; j < tc.compressFilters.Count; ++j)
                {
                    var mf = tc.compressFilters[j];
                    ToolsUtility.NewLine (ref lec, 30);
                    ToolsUtility.TextField (ref lec, "", 0, ref mf.str, 400, true);
                    ToolsUtility.DeleteButton (ref deleteIndex, j, ref lec);
                }
                ToolsUtility.EndDelete (deleteIndex, tc.compressFilters);

                ToolsUtility.NewLine (ref lec, 20);
                if (ToolsUtility.Button (ref lec, "重新导入符合条件的贴图", 160, true))
                {
                    string[] guids = AssetDatabase.FindAssets("t:texture");
                    List<string> reimportPaths = new List<string>();
                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        if (TextureAssets.GetTexureType(path) == tc)
                        {
                            reimportPaths.Add(path);
                        }
                    }

                    if (reimportPaths.Count == 0)
                    {
                        EditorUtility.DisplayDialog("重新导入符合该设置的资源", $"找不到符合条件的资源", "确定");
                    }
                    else if (EditorUtility.DisplayDialog("重新导入符合该设置的资源", $"确定重新导入符合该设置的资源，总共{reimportPaths.Count}个？", "确定", "取消"))
                    {
                        AssetDatabase.StartAssetEditing();
                        foreach (string path in reimportPaths)
                        {
                            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                            bool unResize = AssetsConfig.instance.texConfig.unResizePath.Contains(path);
                            TextureAssets.SetTextureConfig(tc, textureImporter, unResize);
                            textureImporter.SaveAndReimport();
                        }
                        AssetDatabase.StopAssetEditing();
                    }
                }
            }
        }
    }
}
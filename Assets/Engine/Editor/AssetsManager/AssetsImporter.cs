using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CFEngine.Editor
{
    public delegate void TableProcess(string tableNames);
    public delegate void AllTableProcess();
    public interface ITableCb { }

    public class AssetsImporter : AssetPostprocessor
    {
        public static bool skipAutoImport = false;
        /// <summary>
        /// Be called when some tables are imported and convert to bytes.
        /// </summary>
        public static event TableProcess tableCb;
        /// <summary>
        /// Be called when cvs is changed and all tables convert to bytes.
        /// Tables may not be changed.
        /// </summary>
        public static event AllTableProcess allTableCb;
        public static bool tableCbInited = false;
        public static bool checkAssetNames = false;
        private const int MaxTriangles = 200001; // MAX：66667
        // private static bool isSingleAimSetting = false;
        //
        // [MenuItem("Tools/引擎/开启单段动画设置")]
        // static void SingleAimSetting()
        // {
        //     isSingleAimSetting = !isSingleAimSetting;
        //     Menu.SetChecked("Tools/引擎/开启单段动画设置",isSingleAimSetting);
        // }
        

        [MenuItem("Tools/引擎/ToggleAutoAssetPostprocessor")]
        static void ToggleAutoAssetPostprocessor()
        {
            IsForbidAssetPostprocessor = !IsForbidAssetPostprocessor;

            UnityEngine.Debug.Log("AutoAssetPostprocessor:" + IsForbidAssetPostprocessor);
        }

        private static string IsForbidAssetPostprocessorName = "IsForbidAssetPostprocessorName";

        public static bool IsForbidAssetPostprocessor
        {
            get
            {
                if (EditorPrefs.HasKey(IsForbidAssetPostprocessorName))
                {
                    return EditorPrefs.GetBool(IsForbidAssetPostprocessorName);
                }

                return false;
            }

            set { EditorPrefs.SetBool(IsForbidAssetPostprocessorName, value); }
        }
        static Art_AnimationConfig art_AnimationConfig;
        void OnPostprocessModel(GameObject g)
        {
            if (g.TryGetComponent(out MeshFilter mf))
            {
                int a = (mf.sharedMesh.triangles.Length) / 3;
                //Debug.Log();
                if (a > MaxTriangles) //MeshOptimizeConfig.maxTriangular
                {
                    UnityEditor.EditorUtility.DisplayDialog("警告", g.name + ".fbx 模型面数过高！" + a, "删除");
                    AssetDatabase.DeleteAsset(assetPath);
                }
            }
            ModelImporter modelImporter = (ModelImporter) assetImporter;
            if (art_AnimationConfig == null)
            {
                art_AnimationConfig = Resources.Load<Art_AnimationConfig>("Art_AnimationConfig");
                Debug.Log("art_AnimationConfig");
            }
            
            if (modelImporter.importAnimation==true && art_AnimationConfig.isEnable)
            {

                
                // string metaPath = assetPath + ".meta";
                // string metaString = null;
                // string first = "first:";
                // int temp=0;
                // int count=0;
                // metaString = System.IO.File.ReadAllText(metaPath);
                // if (metaString == null)
                // {
                //     return;
                // }
                // for (int i = 0; i < metaString.Length; i++)
                // {
                //     if(metaString.IndexOf(first,temp)!=-1){
                //         temp=metaString.IndexOf(first,temp)+first.Length;
                //         count++;
                //     }
                // }

                if (modelImporter.clipAnimations.Length == 1)
                {
                    if (EditorUtility.DisplayDialog("单动画设置(默认取消)", $"重置动画({g.gameObject.name})长度", "重置", "取消"))
                    {
                        var clipAnimations = modelImporter.clipAnimations;
                        clipAnimations[0].lastFrame = modelImporter.defaultClipAnimations[0].lastFrame;
                        modelImporter.clipAnimations = clipAnimations;
                        //modelImporter.SaveAndReimport();
                    }
                    // int option = EditorUtility.DisplayDialogComplex("单动画设置(默认取消)",
                    //     $"重置动画({g.gameObject.name})长度",
                    //     "重置",
                    //     "取消",
                    //     "关闭导入设置");
                    // switch (option)
                    // {
                    //     // Save.
                    //     case 0:
                    //         modelImporter.clipAnimations = modelImporter.defaultClipAnimations;
                    //         break;
                    //     // Cancel.
                    //     case 1:
                    //         break;
                    //     // Don't Save.
                    //     case 2:
                    //         isSingleAimSetting = false;
                    //         break;
                    //
                    //     default:
                    //         Debug.LogError("Unrecognized option.");
                    //         break;
                    // }
                }

            }

        }



        
        public void OnPreprocessModel()
        {
            if (EngineUtility.IsBuildingGame || IsForbidAssetPostprocessor) return;

            // if (!EngineUtility.AutoAssetPostprocessor) return;

            if (IsIgnoredFile(assetPath))
                return;

            ModelImporter modelImporter = (ModelImporter) assetImporter;
            modelImporter.importCameras = false;
            modelImporter.importLights = false;
            if (modelImporter.animationType != ModelImporterAnimationType.None)
                modelImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            modelImporter.meshCompression = ModelImporterMeshCompression.Off;
            FBXAssets.ProcessFbx(assetPath, modelImporter, EditorMessageType.Dialog);
            DebugLog.AddEngineLog2("Reimport Model:{0}", assetPath);
        }

        public void OnPreprocessTexture()
        {
            if (EngineUtility.IsBuildingGame || IsForbidAssetPostprocessor) return;

            if (IsIgnoredFile(assetPath))
                return;

            TextureImporter textureImporter = assetImporter as TextureImporter;
            TextureAssets.SetTextureConfig(assetPath, textureImporter);
        }

        public static bool IsIgnoredFile(string assetPath)
        {
            return skipAutoImport
                   || assetPath.ToLower().Contains("/test")
                   || File.Exists("Assets/Engine/Test/IgnoreImportTex.txt")
                   || assetPath.ToLower().Contains("~~temp");
        }

        public void OnPreprocessAsset()
        {

            if (!EngineUtility.AutoAssetPostprocessor) return;

            if (IsIgnoredFile(assetPath))
                return;

            if (assetPath.EndsWith(AssetsConfig.instance.SpriteAtlasExt))
            {
                TextureAssets.SetSpriteAtlasConfig(assetPath);
            }
            else if (assetPath.EndsWith(AssetsConfig.instance.ReadableMeshSuffix))
            {
                MeshAssets.MakeMakeReadable(assetPath);
            }
        }

        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {

            if (EngineUtility.IsBuildingGame || IsForbidAssetPostprocessor) return;

            //if (!EngineUtility.AutoAssetPostprocessor) return;

            TableAssets.tableNames = "";
            bool deal = false;
            if (importedAssets != null)
            {
                bool allTables = false;
                for (int i = 0; i < importedAssets.Length; ++i)
                {
                    string path = importedAssets[i];
                    if (TableAssets.IsTable(path))
                    {
                        TableAssets.tableNames += " ";
                        string tablename = TableAssets.GetTableName(path);
                        TableAssets.tableNames += tablename;
                    }
                    else if (TableAssets.ShouldLoadAllTable(path))
                    {
                        allTables = true;
                        break;
                    }
                }

                if (!tableCbInited && (allTables || !string.IsNullOrEmpty(TableAssets.tableNames)))
                {
                    tableCbInited = true;
                    var types = EngineUtility.GetAssemblyType(typeof(ITableCb), "ITableCb");
                    foreach (var t in types)
                    {
                        EditorCommon.CallInternalFunction(t, "Init", true, false, false, null, null);
                        break;
                    }
                }

                if (allTables)
                {
                    Debug.Log("table2bytes all");
                    if (TableAssets.ExeTable2Bytes(string.Empty))
                    {
                        allTableCb?.Invoke();
                    }

                    deal = true;
                }
                else if (TableAssets.tableNames != "")
                {
                    Debug.Log("table2bytes: " + TableAssets.tableNames);
                    if (TableAssets.ExeTable2Bytes(TableAssets.tableNames))
                    {
                        tableCb?.Invoke(TableAssets.tableNames);
                    }

                    deal = true;
                }
            }

            for (int i = 0; i < deletedAssets.Length; ++i)
            {
                string path = deletedAssets[i];
                if (TableAssets.IsTable(path))
                {
                    deal |= TableAssets.DeleteTable(path);
                }
            }

            if (deal)
            {
                AssetDatabase.Refresh();
            }

            if (checkAssetNames && importedAssets != null)
            {
                foreach (string assetPath in importedAssets)
                {
                    if (assetPath.ToLower().EndsWith(".fbx"))
                    {
                        FBXAssets.CheckModelSubAssetNames(assetPath, EditorMessageType.Dialog);
                    }
                }

                EditorCommon.CheckInvalidAssetPaths(importedAssets, true);
            }

            ProcessLodFlag(importedAssets, deletedAssets, movedFromAssetPaths);
        }
        
        static void ProcessLodFlag(string[] importedAssets, string[] deletedAssets, string[] movedFromAssets)
        {
            void ProcessPrefabLodConfigFlag(PrefabLodConfig config, string asset, bool state)
            {
                bool isRuntimePrefabDirectory = asset.StartsWith("Assets/BundleRes/Runtime/Prefab/");
                bool isLod1 = isRuntimePrefabDirectory && asset.EndsWith("_lod1.prefab");
                bool isLod2 = isRuntimePrefabDirectory && asset.EndsWith("_lod2.prefab");
                bool isLod3 = isRuntimePrefabDirectory && asset.EndsWith("_lod3.prefab");
                bool isLod = isLod1 || isLod2 || isLod3;
                
                if (!isLod)
                    return;
                
                int startIndex = asset.LastIndexOf('/') + 1;
                int length = asset.Length - startIndex - "_lod1.prefab".Length;
                string prefabName = asset.Substring(startIndex, length).ToLower();

                
                uint targetFlag;
                if (isLod1)
                {
                    targetFlag = PrefabLodItem.Flag_Lod1;
                }
                else if (isLod2)
                {
                    targetFlag = PrefabLodItem.Flag_Lod2;
                }
                else
                {
                    targetFlag = PrefabLodItem.Flag_Lod3;
                }
                
                PrefabLodItem item;
                for (int i = 0; i < config.items.Count; i++)
                {
                    item = config.items[i];
                    if (string.Equals(item.name, prefabName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (item.lodFlag.HasFlag(targetFlag) != state)
                        {
                            item.lodFlag.SetFlag(targetFlag, state);
                            config.items[i] = item;
                            EditorUtility.SetDirty(config);
                            if (item.lodFlag.flag == 0u)
                                config.items.RemoveAt(i);
                        }
                        return;
                    }
                }

                // 不存在的新配置，当标记存在时才需要添加Item。
                if (state)
                {
                    item = new PrefabLodItem();
                    item.name = prefabName;
                    item.lodFlag.SetFlag(targetFlag, true);
                    config.items.Add(item);
                    EditorUtility.SetDirty(config);
                }
            }
            
            const string configPath = "Assets/BundleRes/Config/PrefabLodConfig.asset";
            PrefabLodConfig config = AssetDatabase.LoadAssetAtPath<PrefabLodConfig>(configPath);
            if (config)
            {
                foreach (string asset in deletedAssets)
                    ProcessPrefabLodConfigFlag(config, asset, false);
                foreach (string asset in movedFromAssets)
                    ProcessPrefabLodConfigFlag(config, asset, false);
                foreach (string asset in importedAssets)
                    ProcessPrefabLodConfigFlag(config, asset, true);
                if (EditorUtility.IsDirty(config))
                    config.items.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
                AssetDatabase.SaveAssetIfDirty(config);
            }
            else
            {
                Debug.LogError($"PrefabLodConfig at path {configPath} not find!");
            }
        }
    }
}
// public class Messagebox
// {
//     [DllImport("User32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
//     public static extern int MessageBox(IntPtr handle, String message, String title, int type);
// }

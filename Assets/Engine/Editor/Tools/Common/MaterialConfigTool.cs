using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MatListEdior = CFEngine.Editor.CommonListEditor<CFEngine.DummyMatData>;
using MatListContext = CFEngine.Editor.AssetListContext<CFEngine.DummyMatData>;

using ShaderKeywordEdior = CFEngine.Editor.CommonListEditor<CFEngine.ShaderKeywordData>;
using ShaderKeywordContext = CFEngine.Editor.AssetListContext<CFEngine.ShaderKeywordData>;
namespace CFEngine.Editor
{
    public partial class MaterialConfigTool : BaseConfigTool<EditorMaterialConfig>
    {
        public enum OpMatType
        {
            None,
            OpGenAllMat,
            OpRefreshAllMat,
            OpGenMat,
            OpRefreshMat,
            OpClearUnUsedMat,
        }
        private ShaderKeywordContext keywordContext;
        private ShaderKeywordEdior keywordEditor;

        private MatListContext sceneMatContext;
        private MatListEdior sceneMatEditor;
        private MatListContext dynamicMatContext;
        private MatListEdior roleMatEditor;
        private MatListContext effectMatContext;
        private MatListEdior effectMatEditor;
        private DummyMatData processMatData = null;
        private DummyMaterialInfo processMat = null;
        private DummyMaterialInfo copyMat = null;
        private int processIndex = -1;

        private ShaderKeywordConfig keywordConfig;
        private OpMatType opMatType = OpMatType.None;

        private string[] internalShaderProperty = null;

        private MaterialConfig matConfig = null;
        private MaterialConfig matConfiMediumg = null;
        private MaterialConfig matConfiggLow = null;

        public override void OnInit ()
        {
            base.OnInit ();
            config = EditorMaterialConfig.instance;

            string path = string.Format ("{0}Config/MaterialConfig.asset", LoadMgr.singleton.EngineResPath);
            matConfig = EditorCommon.LoadAsset<MaterialConfig> (path);

            path = string.Format ("{0}Config/MaterialConfigMediumg.asset", LoadMgr.singleton.EngineResPath);
            matConfiMediumg = EditorCommon.LoadAsset<MaterialConfig> (path);

            path = string.Format ("{0}Config/MaterialConfigLow.asset", LoadMgr.singleton.EngineResPath);
            matConfiggLow = EditorCommon.LoadAsset<MaterialConfig> (path);

            keywordContext.name = "ShaderKeywords";
            keywordContext.headGUI = KeywordHeadGUI;
            keywordContext.elementGUI = ShaderKeywordConfigGUI;
            keywordContext.needDelete = true;
            keywordContext.needAdd = true;

            keywordConfig = ShaderKeywordConfig.instance;
            keywordConfig.keywordData.name = "ShaderKeywords";

            keywordEditor = new ShaderKeywordEdior (keywordConfig.keywordData, ref keywordContext);
            ShaderKeywordConfig.instance.keywordData.InitKeyMap ();

            sceneMatContext.name = "SceneMat";
            sceneMatContext.preProcess = PreProcess;
            sceneMatContext.headGUI = MatHeadGUI;
            sceneMatContext.elementGUI = MatConfigGUI;
            sceneMatContext.needDelete = true;
            sceneMatContext.needAdd = true;

            config.sceneMat.name = "SceneMat";
            config.sceneMat.path = "Scene";
            sceneMatEditor = new MatListEdior (config.sceneMat, ref sceneMatContext);

            dynamicMatContext.name = "DynamicMat";
            dynamicMatContext.headGUI = MatHeadGUI;
            dynamicMatContext.elementGUI = MatConfigGUI;
            dynamicMatContext.needDelete = true;
            dynamicMatContext.needAdd = true;

            config.dynamicMat.name = "DynamicMat";
            config.dynamicMat.path = "Dynamic";
            roleMatEditor = new MatListEdior (config.dynamicMat, ref dynamicMatContext);

            effectMatContext.name = "EffectMat";
            effectMatContext.headGUI = EffectHeadGUI;
            effectMatContext.elementGUI = EffectMatConfigGUI;
            effectMatContext.needDelete = false;
            effectMatContext.needAdd = false;

            config.effectMat.name = "EffectMat";
            config.effectMat.path = "Effect";
            effectMatEditor = new MatListEdior (config.effectMat, ref effectMatContext);

            internalShaderProperty = Enum.GetNames (typeof (EShaderKeyID));
            internalShaderProperty[internalShaderProperty.Length - 1] = "";

        }

        protected override void OnConfigGui (ref Rect rect)
        {
            keywordEditor.Draw (config.folder, ref rect);
            if (config.folder.FolderGroup ("IgnoreBindShaderProperty", "IgnoreBindShaderProperty", rect.width - 10))
            {
                IgnoreBindShaderPropertyGUI ();
                EditorCommon.EndFolderGroup ();
            }

            if (config.folder.FolderGroup ("DefaultMatConfig", "DefaultMatLODConfig", rect.width - 10))
            {
                DefaultMatConfig (ref rect);
                EditorCommon.EndFolderGroup ();
            }
            if (config.folder.FolderGroup ("DummyMat", "DummyMat", rect.width - 10))
            {
                EditorGUILayout.BeginHorizontal ();
                EditorGUILayout.LabelField ("Suffix:S=Shadow;Ex=Extra;L=Light;B=Bake;D=Dynamic", GUILayout.MaxWidth (400));
                if (GUILayout.Button ("ClearUnUsedMat", GUILayout.MaxWidth (120)))
                {
                    opMatType = OpMatType.OpClearUnUsedMat;
                }
                EditorGUILayout.EndHorizontal ();
                sceneMatEditor.Draw (config.folder, ref rect);
                roleMatEditor.Draw (config.folder, ref rect);
                effectMatEditor.Draw (config.folder, ref rect);
                EditorGUILayout.LabelField ("RuntimeMats");
                RuntimeMatInfoGUI ("LodHighMat", matConfig);
                RuntimeMatInfoGUI ("LodMediumMat", matConfiMediumg);
                RuntimeMatInfoGUI ("LodLowMat", matConfiggLow);
                EditorCommon.EndFolderGroup ();
            }
        }

        #region process
        protected override void OnConfigUpdate ()
        {
            switch (opMatType)
            {
                case OpMatType.OpGenAllMat:
                    ProcessAllMat (false);
                    break;
                case OpMatType.OpRefreshAllMat:
                    ProcessAllMat (true);
                    break;
                case OpMatType.OpGenMat:
                    ProcessMat (false);
                    break;
                case OpMatType.OpRefreshMat:
                    ProcessMat (true);
                    break;
                case OpMatType.OpClearUnUsedMat:
                    ClearUnUsedMat ();
                    break;
            }
            opMatType = OpMatType.None;
        }
        private void AddMaterials (DummyMatData matData,
            List<Material> materials, List<MatConfigInfo> matConfigInfo, int lod)
        { 
            var dummyMats = matData.materialInfos;
            for (int i = 0; i < dummyMats.Count; ++i)
            {
                var dmi = dummyMats[i];
                matConfigInfo.Add(new MatConfigInfo()
                {
                    matHash = EngineUtility.XHashLowerRelpaceDot(0, dmi.hashID),
                    offset = (uint)materials.Count,
                    lodFunOffset = dmi.selectFunIndex,
                    keywordFlag = dmi.keywordFlag,
                });
                var lodGroup = dmi.lodGroup[lod];
                for (int j = 0; j < lodGroup.lodMat.Count; ++j)
                {
                    if (lodGroup.lodMat[j] != null)
                    {
                        materials.Add(lodGroup.lodMat[j]);
                    }
                }

            }
        }
        private void AddMaterials(DummyMatData matData,
            List<Material> materials, List<int> matOffset, int lod)
        {
            var dummyMats = matData.materialInfos;
            for (int i = 0; i < dummyMats.Count; ++i)
            {
                var dmi = dummyMats[i];
                matOffset.Add(materials.Count);
                matOffset.Add(dmi.selectFunIndex);
                var lodGroup = dmi.lodGroup[lod];
                for (int j = 0; j < lodGroup.lodMat.Count; ++j)
                {
                    if (lodGroup.lodMat[j] != null)
                    {
                        materials.Add(lodGroup.lodMat[j]);
                    }
                }

            }
        }
        private void SaveMatConfig (List<Material> materials, List<MatConfigInfo> matConfigInfo, string name, int lod)
        {
            string path = string.Format ("{0}Config/{1}.asset", LoadMgr.singleton.EngineResPath, name);
            var mc = EditorCommon.LoadAsset<MaterialConfig> (path);
            if (mc == null)
            {
                mc = MaterialConfig.CreateInstance<MaterialConfig> ();
            }
            mc.name = name;

            AddMaterials (config.sceneMat, materials, matConfigInfo, lod);
            AddMaterials(config.dynamicMat, materials, matConfigInfo, lod);
            mc.dummyMats = materials.ToArray();
            mc.matConfigInfo = matConfigInfo.ToArray();
            materials.Clear();
            matConfigInfo.Clear();

            var materialOffset = new List<int>();
            AddMaterials(config.effectMat, materials, materialOffset, lod);
            mc.effectMats = materials.ToArray ();
            mc.effectMatOffset = materialOffset.ToArray ();
            materials.Clear ();
            matConfigInfo.Clear ();
            EditorCommon.SaveAsset (path, mc);
        }

        protected override void OnSave ()
        {
            List<Material> materials = new List<Material> ();
            List<MatConfigInfo> matConfigInfo = new List<MatConfigInfo>();
            SaveMatConfig (materials, matConfigInfo, "MaterialConfig", 0);
            SaveMatConfig (materials, matConfigInfo, "MaterialConfigMedium", 1);
            SaveMatConfig (materials, matConfigInfo, "MaterialConfigLow", 2);
            keywordConfig?.Save ();
        }

        private void ProcessAllMat (bool refresh)
        {
            if (processMatData != null)
            {
                int matCount = processMatData.materialInfos.Count;
                for (int i = 0; i < matCount; ++i)
                {
                    var dmi = processMatData.materialInfos[i];
                    EditorUtility.DisplayProgressBar (string.Format ("{0}-{1}/{2}", "ProcessMat", i, matCount), dmi.name, (float) i / matCount);
                    if (refresh)
                    {
                        MaterialShaderAssets.DefaultRefeshMat (dmi, -1);
                    }
                    else
                    {
                        MaterialShaderAssets.DefaultMat (dmi, -1, processMatData.path);
                    }

                }
                EditorUtility.ClearProgressBar ();
                processMat = null;
            }
        }

        private void ProcessMat (bool refresh)
        {
            if (processMat != null)
            {
                if (refresh)
                {
                    MaterialShaderAssets.DefaultRefeshMat (processMat, processIndex);
                }
                else
                {
                    MaterialShaderAssets.DefaultMat (processMat, processIndex, processMatData.path);
                }
                processMatData = null;
                processMat = null;
                processIndex = -1;
            }
        }

        private void ClearUnUsedMat ()
        {
            CommonAssets.enumMat.cb = (mat, path, context) =>
            {
                var lst = context as List<Material>;
                lst.Add (mat);
            };

            var dummyMatList = new DummyMatData[]
            {
                config.sceneMat,
                config.dynamicMat,
                config.effectMat
            };

            for (int i = 0; i < dummyMatList.Length; ++i)
            {
                var dm = dummyMatList[i];
                string path = string.Format ("{0}{1}/{2}",
                    LoadMgr.singleton.EngineResPath,
                    AssetsConfig.instance.DummyMatFolder, dm.path);
                var matList = new List<Material> ();
                CommonAssets.EnumAsset<Material> (CommonAssets.enumMat, "ClearUnUsedMat",
                    path, matList, false);

                for (int j = 0; j < dm.materialInfos.Count; ++j)
                {
                    var dmi = dm.materialInfos[j];
                    for (int k = 0; k < dmi.matVariants.Count; ++k)
                    {
                        var mv = dmi.matVariants[k];
                        if (mv.mat != null)
                        {
                            matList.Remove (mv.mat);
                        }
                    }
                }
                var sb = new System.Text.StringBuilder ();
                sb.AppendLine (string.Format ("Delete Mat:{0}", path));
                for (int j = 0; j < matList.Count; ++j)
                {
                    var p = AssetDatabase.GetAssetPath (matList[j]);
                    sb.AppendLine (p);
                    AssetDatabase.DeleteAsset (p);
                }
                DebugLog.AddEngineLog (sb.ToString ());
            }

        }

        private void InitMat (DummyMatData data)
        {
            for (int i = 0; i < data.materialInfos.Count; ++i)
            {
                var dmi = data.materialInfos[i];

                if (dmi.shader != null)
                {
                    for (int j = 0; j < dmi.matVariants.Count; ++j)
                    {
                        var mv = dmi.matVariants[j];
                        string name = dmi.name + mv.suffix;
                        if (mv.mat == null || mv.mat.name != name)
                        {
                            mv.mat = MaterialShaderAssets.GetDummyMat (name, data.path);
                        }
                    }
                }
            }
        }

        private void LodMaterial (ref ListElementContext lec,
            MatLodConfigs configs, MatLodConfig mlc, int index,
            MaterialLodGroup lodGroup, List<MaterialVariant> matVariants)
        {
            Material mat = null;
            if (index >= lodGroup.lodMat.Count)
            {
                lodGroup.lodMat.Add (null);
            }
            else
            {
                mat = lodGroup.lodMat[index];
            }
            ToolsUtility.NewLine (ref lec, 25);
            ToolsUtility.Label (ref lec, mlc.matDesc, 120, true);
            ToolsUtility.ObjectField (ref lec, name, 100, ref mat, typeof (Material), 200);

            int selectIndex = -1;
            string[] names = configs.GetNames ();
            if (names != null)
            {
                EditorGUI.BeginChangeCheck ();
                ToolsUtility.Popup (ref lec, "", 0, ref selectIndex, names, 100);
                if (EditorGUI.EndChangeCheck ())
                {
                    if (selectIndex < matVariants.Count)
                        mat = matVariants[selectIndex].mat;
                }
                if (ToolsUtility.Button (ref lec, "Clear", 80))
                {
                    mat = null;
                }
            }
            lodGroup.lodMat[index] = mat;
        }

        private void InnerMatConfigGUI (
            ref ListElementContext lec,
            ref MatListContext context,
            DummyMatData data, int i, string matType, string name = "")
        {
            var dmi = data.materialInfos[i];
            ToolsUtility.InitListContext (ref lec, context.defaultHeight);

            //head line
            string label = name;
            if (string.IsNullOrEmpty (name))
            {
                label = string.Format ("{0}.{1}", i.ToString (), string.IsNullOrEmpty (dmi.name) ? "empty" : dmi.name);
            }
            else
            {
                dmi.name = name;
            }
            ToolsUtility.Label (ref lec, label, 160, true);

            string folderPath = dmi.GetHash ();
            bool dmiFolder = ToolsUtility.SHButton (ref lec, config.folder, folderPath);
            if (ToolsUtility.Button (ref lec, "Gen", 80))
            {
                processMat = dmi;
                processIndex = -1;
                processMatData = data;
                opMatType = OpMatType.OpGenMat;
            }
            // if (ToolsUtility.Button (ref lec, "Refresh", 80))
            // {
            //     processMat = dmi;
            //     processIndex = -1;
            //     opMatType = OpMatType.OpRefreshMat;
            // }
            ToolsUtility.CopyButton<DummyMaterialInfo> (ref lec, ref copyMat, dmi);
            ToolsUtility.Label (ref lec, dmi.hashID, 400);
            if (dmiFolder)
            {
                MatVariantGUI (ref lec, folderPath, data, dmi);
                MatLodGroupGUI (ref lec, folderPath, dmi);
                MatParamBindGUI (ref lec, folderPath, dmi);
            }
        }

        private void MatVariantGUI (ref ListElementContext lec, string folderPath,
            DummyMatData data, DummyMaterialInfo dmi)
        {
            ToolsUtility.NewLineWithOffset (ref lec);
            string mvfolderPath = string.Format ("MatVariant_{0}", folderPath);
            if (ToolsUtility.Foldout (ref lec, config.folder, mvfolderPath, "MatVariant", 80, true))
            {
                //new line
                ToolsUtility.NewLine (ref lec, 20);
                bool reset = true;
                if (string.IsNullOrEmpty (name))
                {
                    ToolsUtility.TextField (ref lec, "Name", 80, ref dmi.name, 160, true);
                    reset = false;
                }
                ToolsUtility.ObjectField (ref lec, "Shader", 70, ref dmi.shader, typeof (Shader), 300, reset);
                ToolsUtility.Popup (ref lec, "", 0, ref dmi.selectFunIndex, DummyMatData.BatchFun, 200);

                //new line
                ToolsUtility.NewLineWithOffset (ref lec);
                ToolsUtility.EnumPopup (ref lec, "", 0, ref dmi.blendType, 100, true);

                // Enum flags = (KeywordFlags) dmi.featureKeywordFlag;
                // ToolsUtility.EnumFlagsField (ref lec, "", 0, ref flags, 100);
                // dmi.featureKeywordFlag = (uint) (KeywordFlags) flags;

                // ToolsUtility.EnumPopup (ref lec, "", 0, ref dmi.blendType, 150, true);
                if (ToolsUtility.Button (ref lec, "Add", 80))
                {
                    dmi.matVariants.Add (new MaterialVariant ());
                }
                KeywordConfigGUI.OnGUI (ref lec, ref dmi.editKey, keywordConfig.keywordData.groups, dmi.keywords);

                if (ToolsUtility.Button (ref lec, "DefaultMat", 100))
                {
                    SetDefaultMat (dmi);
                }
                if (ToolsUtility.Button(ref lec, "Clear", 80))
                {
                    dmi.matVariants.Clear();
                }
                KeywordFlag f = (KeywordFlag)dmi.keywordFlag;
                ToolsUtility.EnumFlagsField(ref lec, "", 0, ref f, 100);
                dmi.keywordFlag = (uint)f;
                int deleteIndex = ToolsUtility.BeginDelete ();
                for (int j = 0; j < dmi.matVariants.Count; ++j)
                {
                    var mv = dmi.matVariants[j];

                    ToolsUtility.NewLine (ref lec, 25);
                    ToolsUtility.Label (ref lec, j.ToString () + ".",
                        20, true);
                    ToolsUtility.DrawLine (ref lec, lec.width - 50, 15, -5);
                    ToolsUtility.NewLineWithOffset (ref lec);
                    ToolsUtility.TextField (ref lec, "Suffix", 60, ref mv.suffix, 100, true);

                    // Enum e = (KeywordFlags) mv.keywordFlag;
                    // ToolsUtility.EnumFlagsField (ref lec, "", 0, ref e, 100);
                    // mv.keywordFlag = (uint) (KeywordFlags) e;

                    // ToolsUtility.NewLineWithOffset (ref lec);
                    ToolsUtility.ObjectField (ref lec, "", 0, ref mv.mat, typeof (Material), 150);

                    if (ToolsUtility.Button (ref lec, "Gen", 80))
                    {
                        opMatType = OpMatType.OpGenMat;
                        processMat = dmi;
                        processMatData = data;
                        processIndex = j;
                    }
                    // if (ToolsUtility.Button (ref lec, "Refresh", 80))
                    // {
                    //     opMatType = OpMatType.OpRefreshMat;
                    //     processMat = dmi;
                    //     processIndex = j;
                    // }
                    ToolsUtility.DeleteButton (ref deleteIndex, j, ref lec);
                    KeywordConfigGUI.OnGUI (ref lec, ref mv.editKey, keywordConfig.keywordData.groups, mv.keywords);

                    ToolsUtility.NewLineWithOffset (ref lec);
                    ToolsUtility.Label (ref lec, "SrcKeyword:", 80, true);
                    ToolsUtility.Label (ref lec, MaterialShaderAssets.GetKeyWords (mv.keywords), lec.width);
                    if (mv.mat != null)
                    {
                        string keywordStr = MaterialShaderAssets.GetKeyWords (mv.mat);
                        if (!string.IsNullOrEmpty (keywordStr))
                        {
                            ToolsUtility.NewLineWithOffset (ref lec);
                            ToolsUtility.Label (ref lec, "MatKeyword:", 80, true);
                            ToolsUtility.Label (ref lec, keywordStr, lec.width);

                        }
                    }
                    ToolsUtility.NewLineWithOffset (ref lec);
                    string mvPath = mv.GetHash ();
                    string paramfolderPath = string.Format ("{0}_Param", mvPath);
                    bool paramFolder = ToolsUtility.Foldout (ref lec, config.folder, paramfolderPath, string.Format ("Param({0})", mv.param.Count), 80, true);
                    if (ToolsUtility.Button (ref lec, "Add", 80))
                    {
                        mv.param.Add (new MaterialVariantParam ());
                    }
                    if (paramFolder)
                    {
                        int removeIndex = ToolsUtility.BeginDelete ();
                        for (int k = 0; k < mv.param.Count; ++k)
                        {
                            var mvp = mv.param[k];
                            ToolsUtility.NewLine (ref lec, 30);
                            Enum id = (EShaderKeyID) mvp.shaderID;
                            ToolsUtility.EnumPopup (ref lec, "", 0, ref id, 120, true);
                            mvp.shaderID = (int) (EShaderKeyID) id;
                            if (mvp.shaderID == (int) EShaderKeyID.Num)
                            {
                                ToolsUtility.TextField (ref lec, "", 0, ref mvp.shaderKey, 200);
                            }

                            ToolsUtility.EnumPopup<ShaderValueType> (ref lec, "", 0, ref mvp.vt, 120);
                            switch (mvp.vt)
                            {
                                case ShaderValueType.Vec:
                                    ToolsUtility.VectorField (ref lec, "", 0, ref mvp.value, 300);
                                    break;
                                case ShaderValueType.Float:
                                    ToolsUtility.FloatField (ref lec, "", 0, ref mvp.value.x, 300);
                                    break;
                                case ShaderValueType.Int:
                                    int v = (int) mvp.value.x;
                                    ToolsUtility.IntField (ref lec, "", 0, ref v, 300);
                                    mvp.value.x = v;
                                    break;
                            }

                            ToolsUtility.DeleteButton (ref removeIndex, k, ref lec);
                        }
                        ToolsUtility.EndDelete (removeIndex, mv.param);
                    }
                }
                ToolsUtility.EndDelete (deleteIndex, dmi.matVariants);
            }
        }
        private void MatLodGroupGUI (ref ListElementContext lec, string folderPath,
            DummyMaterialInfo dmi)
        {
            ToolsUtility.NewLine (ref lec, 0);
            string lodGroupfolderPath = string.Format ("MatLodGroup_{0}", folderPath);
            if (ToolsUtility.Foldout (ref lec, config.folder, lodGroupfolderPath, "MatLodGroup", 120, true))
            {
                if (ToolsUtility.Button (ref lec, "Default", 80))
                {
                    SetDefaultMatLod (dmi);
                }
                if (ToolsUtility.Button(ref lec, "Clear", 80))
                {
                    for (int j = 0; j < dmi.lodGroup.Length; ++j)
                    {
                        var lodGroup = dmi.lodGroup[j];
                        for (int k = 0; k < lodGroup.lodMat.Count; ++k)
                        {
                            lodGroup.lodMat[k] = null;
                        }
                    }
                }
                for (int j = 0; j < dmi.lodGroup.Length; ++j)
                {
                    var lodGroup = dmi.lodGroup[j];

                    ToolsUtility.NewLine (ref lec, 20);
                    string materialLodfolderPath = string.Format ("MatLodGroup_{0}_{1}", folderPath, lodGroup.groupName);
                    if (ToolsUtility.Foldout (ref lec, config.folder, materialLodfolderPath, lodGroup.groupName, 160, true))
                    {
                        var dmc = config.defaultMatConfig[dmi.selectFunIndex];
                        for (int k = 0; k < dmc.lodConfigs.configs.Count; ++k)
                        {
                            var mlc = dmc.lodConfigs.configs[k];
                            LodMaterial (ref lec, dmc.lodConfigs, mlc, k,
                                lodGroup, dmi.matVariants);
                        }
                    }
                }
            }
        }

        private void MatParamBindGUI (ref ListElementContext lec, string folderPath,
            DummyMaterialInfo dmi)
        {
            ToolsUtility.NewLine (ref lec, 0);
            string mcfolderPath = string.Format ("MatConvert_{0}", folderPath);
            if (ToolsUtility.Foldout (ref lec, config.folder, mcfolderPath, "MatConvert", 80, true))
            {
                ToolsUtility.NewLine (ref lec, 10);
                string mcifolderPath = string.Format ("MatConvertInclude_{0}", folderPath);
                string mciName = string.Format ("PropertyInclude({0})", dmi.includeParam.Count.ToString ());
                if (ToolsUtility.Foldout (ref lec, config.folder, mcifolderPath, mciName, 160, true))
                {
                    if (ToolsUtility.Button (ref lec, "Add", 100))
                    {
                        dmi.includeParam.Add (new IncludeProperty ());
                    }

                    int removeIndex = ToolsUtility.BeginDelete ();
                    for (int j = 0; j < dmi.includeParam.Count; ++j)
                    {
                        var ip = dmi.includeParam[j];
                        ToolsUtility.NewLine (ref lec, 25);
                        ToolsUtility.TextField (ref lec, "Property", 80, ref ip.property, 100, true);
                        ToolsUtility.Toggle (ref lec, "Indclude", 80, ref ip.include);
                        ToolsUtility.DeleteButton (ref removeIndex, j, ref lec);
                    }
                    ToolsUtility.EndDelete (removeIndex, dmi.includeParam);
                }

                ToolsUtility.NewLine (ref lec, 10);
                string mcdfolderPath = string.Format ("MatConvertDep_{0}", folderPath);
                string mcdName = string.Format ("PropertyDep({0})", dmi.depParam.Count.ToString ());
                if (ToolsUtility.Foldout (ref lec, config.folder, mcdfolderPath, mcdName, 160, true))
                {
                    if (ToolsUtility.Button (ref lec, "Add", 100))
                    {
                        dmi.depParam.Add (new DepPair ());
                    }

                    int removeIndex = ToolsUtility.BeginDelete ();
                    for (int j = 0; j < dmi.depParam.Count; ++j)
                    {
                        var dep = dmi.depParam[j];
                        ToolsUtility.NewLine (ref lec, 25);
                        ToolsUtility.TextField (ref lec, "Property", 80, ref dep.propertyName, 120, true);
                        ToolsUtility.TextField (ref lec, "Dep", 80, ref dep.dep, 200);
                        ToolsUtility.Toggle (ref lec, "Relative", 80, ref dep.relative);
                        ToolsUtility.DeleteButton (ref removeIndex, j, ref lec);
                    }
                    ToolsUtility.EndDelete (removeIndex, dmi.depParam);
                }

                ToolsUtility.NewLine (ref lec, 10);
                string mpfolderPath = string.Format ("MatProperty_{0}", folderPath);
                string mpName = string.Format ("MatProperty({0})", dmi.shaderPropertys.Count.ToString ());
                if (ToolsUtility.Foldout (ref lec, config.folder, mpfolderPath, mpName, 160, true))
                {
                    if (ToolsUtility.Button (ref lec, "AddProperty", 100))
                    {
                        dmi.shaderPropertys.Add (new ShaderProperty ());
                    }
                    if (ToolsUtility.Button(ref lec, "Clear", 100))
                    {
                        dmi.shaderPropertys.Clear();
                    }
                    if (dmi.shader != null)
                    {
                        if (ToolsUtility.Button (ref lec, "TryBind", 100))
                        {
                            dmi.shaderPropertys.Clear ();
                            int count = ShaderUtil.GetPropertyCount (dmi.shader);
                            for (int j = 0; j < count; ++j)
                            {
                                string shaderPropertyName = ShaderUtil.GetPropertyName (dmi.shader, j);
                                if (CanBindProperty (dmi, shaderPropertyName))
                                {
                                    ShaderProperty sp = new ShaderProperty ();

                                    ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType (dmi.shader, j);
                                    sp.dataType = type;

                                    int shaderKey = Shader.PropertyToID (shaderPropertyName);
                                    sp.shaderID = ShaderManager._ShaderKeyEffectKey.Length;
                                    sp.customName = shaderPropertyName;
                                    for (int k = 0; k < ShaderManager._ShaderKeyEffectKey.Length; ++k)
                                    {
                                        if (ShaderManager._ShaderKeyEffectKey[k] == shaderKey)
                                        {
                                            sp.shaderID = k;
                                            sp.customName = "";
                                            break;
                                        }
                                    }
                                    SetDep (dmi, sp, shaderPropertyName);
                                    dmi.shaderPropertys.Add (sp);
                                }

                            }
                        }
                    }

                    int removeIndex = ToolsUtility.BeginDelete ();
                    for (int j = 0; j < dmi.shaderPropertys.Count; ++j)
                    {
                        var sp = dmi.shaderPropertys[j];
                        ToolsUtility.NewLine (ref lec, 25);
                        ToolsUtility.EnumPopup (ref lec, "DataType", 60, ref sp.dataType, 100, true);
                        Enum id = (EShaderKeyID) sp.shaderID;
                        ToolsUtility.EnumPopup (ref lec, "", 0, ref id, 120);
                        sp.shaderID = (int) (EShaderKeyID) id;
                        // 
                        if (sp.shaderID == ShaderManager._ShaderKeyEffectKey.Length)
                        {
                            ToolsUtility.TextField (ref lec, "PropName", 80, ref sp.customName, 160);
                        }

                        ToolsUtility.DeleteButton (ref removeIndex, j, ref lec);
                        // ToolsUtility.Label (ref lec, "Dep:" + sp.depComp, 200);
                        //ToolsUtility.Label (ref lec, "RelativeStr:" + sp.relativeStr, 200);
                    }
                    ToolsUtility.EndDelete (removeIndex, dmi.shaderPropertys);
                }

            }
        }

        private bool CanBindProperty (DummyMaterialInfo dmi, string shaderPropertyName)
        {
            var ip = dmi.includeParam.Find ((x) => x.property == shaderPropertyName);
            if (ip != null)
            {
                return ip.include;
            }
            else
            {
                return !config.ignoreBindParam.Contains (shaderPropertyName);
            }
        }

        private void SetDep (DummyMaterialInfo dmi, ShaderProperty sp, string shaderPropertyName)
        {
            if (dmi.depParam.Count > 0)
            {
                var dp = dmi.depParam.Find ((x) => x.propertyName == shaderPropertyName);
                if (dp != null)
                {
                    if (dp.relative)
                    {
                        sp.relativeStr = dp.dep;
                    }
                    else
                    {

                        sp.depComp = dp.dep;
                    }
                }
            }
        }

        private void SetDefaultMat (DummyMaterialInfo dmi)
        {
            if (dmi.shader != null && !string.IsNullOrEmpty (dmi.name))
            {
                if (dmi.selectFunIndex < config.defaultMatConfig.Count)
                {
                    dmi.matVariants.Clear ();
                    var dmc = config.defaultMatConfig[dmi.selectFunIndex];
                    for (int i = 0; i < dmc.configs.configs.Count; ++i)
                    {
                        var mkc = dmc.configs.configs[i];
                        var mv = new MaterialVariant ()
                        {
                            suffix = mkc.suffix,
                        };
                        mv.keywords.AddRange (mkc.keywords);
                        dmi.matVariants.Add (mv);
                    }
                }

            }

        }

        private void SetDefaultMatLodCount (MaterialLodGroup lodGroup, int count)
        {
            lodGroup.lodMat.Clear ();
            for (int i = 0; i < count; ++i)
            {
                lodGroup.lodMat.Add (null);
            }
        }

        private void SetDefaultMatLod (DummyMaterialInfo dmi, MaterialLodGroup lodGroup, int index, string suffix)
        {
            lodGroup.lodMat[index] = dmi.FindMat (suffix);
        }

        private void SetDefaultMatLod (DummyMaterialInfo dmi)
        {
            if (dmi.selectFunIndex < config.defaultMatConfig.Count)
            {
                var dmc = config.defaultMatConfig[dmi.selectFunIndex];
                int count = dmc.lodConfigs.configs.Count;
                SetDefaultMatLodCount (dmi.lodGroup[0], count);
                SetDefaultMatLodCount (dmi.lodGroup[1], count);
                SetDefaultMatLodCount (dmi.lodGroup[2], count);
                for (int i = 0; i < count; ++i)
                {
                    var mlc = dmc.lodConfigs.configs[i];
                    SetDefaultMatLod (dmi, dmi.lodGroup[0], i, mlc.findSuffixLod0);
                    SetDefaultMatLod (dmi, dmi.lodGroup[1], i, mlc.findSuffixLod1);
                    SetDefaultMatLod (dmi, dmi.lodGroup[2], i, mlc.findSuffixLod2);
                }
            }
        }

        #endregion

        #region keyword

        private void KeywordHeadGUI (ref Rect rect, ref ShaderKeywordContext context, ShaderKeywordData data)
        {
            rect.width = 80;
            if (GUI.Button (rect, "Init"))
            {
                ShaderKeywordConfig.instance.keywordData.InitKeyMap ();
            }
        }
        private void ShaderKeywordConfigGUI (ref ListElementContext lec, ref ShaderKeywordContext context, ShaderKeywordData data, int i)
        {
            var kwg = data.groups[i];
            ToolsUtility.InitListContext (ref lec, context.defaultHeight);
            ToolsUtility.Label (ref lec, kwg.name, 160, true);

            string folderPath = kwg.GetHash ();
            bool kwgFolder = ToolsUtility.SHButton (ref lec, config.folder, folderPath);
            if (kwgFolder)
            {
                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.TextField (ref lec, "Name", 80, ref kwg.name, 160, true);
                if (ToolsUtility.Button (ref lec, "Add", 80))
                {
                    kwg.keywords.Add (new KeywordInfo ());
                }
                int deleteIndex = ToolsUtility.BeginDelete ();
                for (int j = 0; j < kwg.keywords.Count; ++j)
                {
                    var kwi = kwg.keywords[j];
                    ToolsUtility.NewLine (ref lec, 25);
                    ToolsUtility.TextField (ref lec, "Key", 80, ref kwi.str, 160, true);
                    ToolsUtility.Toggle (ref lec, "IsIdKey", 80, ref kwi.idKeyword);
                    ToolsUtility.DeleteButton (ref deleteIndex, j, ref lec);
                }
                ToolsUtility.EndDelete (deleteIndex, kwg.keywords);
            }
        }
        #endregion

        #region dummy mat
        private void PreProcess (ref MatListContext context, DummyMatData data)
        {
            int nameCount = data.matNames != null?data.matNames.Length : 0;
            if (data.materialInfos.Count != nameCount)
            {
                data.matNames = new string[data.materialInfos.Count];
                for (int i = 0; i < data.materialInfos.Count; ++i)
                {
                    data.matNames[i] = data.materialInfos[i].name;
                }
            }
        }
        private void MatHeadGUI (ref Rect rect, ref MatListContext context, DummyMatData data)
        {
            rect.width = 80;
            if (GUI.Button (rect, "GenAll"))
            {
                opMatType = OpMatType.OpGenAllMat;
                processMatData = data;
            }
            rect.x += 85;
            if (GUI.Button (rect, "Refresh"))
            {
                opMatType = OpMatType.OpRefreshAllMat;
                processMatData = data;
            }
            InitMat (data);
        }
        private void MatConfigGUI (ref ListElementContext lec, ref MatListContext context, DummyMatData data, int i)
        {
            InnerMatConfigGUI (ref lec, ref context, data, i, context.name);
        }

        #endregion

        #region runtimeData
        private void RuntimeMatInfoGUI (FolderConfig fc, Material[] mats, ref Vector2 scroll,string path, string name)
        {
            if (config.folder.Folder (path, name))
            {
                if (mats != null)
                {
                    EditorGUI.indentLevel++;
                    EditorCommon.BeginScroll(ref scroll, mats.Length, 30);
                    for (int j = 0; j < mats.Length; ++j)
                    {
                        var mat = mats[j];
                        EditorGUILayout.BeginHorizontal ();
                        EditorGUILayout.ObjectField (mat, typeof (Material), false);
                        EditorGUILayout.EndHorizontal ();
                    }
                    EditorCommon.EndScroll();
                    EditorGUI.indentLevel--;
                }
            }
        }
        private void RuntimeMatInfoGUI (string name, MaterialConfig mc)
        {
            if (config.folder.Folder ("RuntimeMat" + name, name))
            {
                EditorGUI.indentLevel++;
                RuntimeMatInfoGUI(config.folder, mc.dummyMats, ref mc.scroll, "DummyMat" + name, "DummyMat");
                //RuntimeMatInfoGUI (config.folder, mc.dynamicMats, "DynamicMat" + name, "DynamicMat");
                RuntimeMatInfoGUI(config.folder, mc.effectMats, ref mc.effectScroll, "EffectMat" + name, "EffectMat");
                EditorGUI.indentLevel--;
            }
        }
        private void IgnoreBindShaderPropertyGUI ()
        {
            EditorGUILayout.BeginHorizontal ();
            if (GUILayout.Button ("Add", GUILayout.MaxWidth (80)))
            {
                config.ignoreBindParam.Add ("");
            }
            EditorGUILayout.EndHorizontal ();
            int removeIndex = -1;
            for (int i = 0; i < config.ignoreBindParam.Count; ++i)
            {
                var param = config.ignoreBindParam[i];
                EditorGUILayout.BeginHorizontal ();
                EditorGUILayout.LabelField (string.Format ("{0}.", i.ToString ()), GUILayout.MaxWidth (80));
                EditorGUI.BeginChangeCheck ();
                param = EditorGUILayout.TextField ("", param, GUILayout.MaxWidth (300));
                if (EditorGUI.EndChangeCheck ())
                {
                    config.ignoreBindParam[i] = param;
                }
                int index = 0;
                EditorGUI.BeginChangeCheck ();
                index = EditorGUILayout.Popup (index, internalShaderProperty, GUILayout.MaxWidth (100));
                if (EditorGUI.EndChangeCheck ())
                {
                    config.ignoreBindParam[i] = internalShaderProperty[index];
                }
                if (GUILayout.Button ("Remove", GUILayout.MaxWidth (80)))
                {
                    removeIndex = i;
                }
                EditorGUILayout.EndHorizontal ();
            }
            if (removeIndex >= 0)
            {
                config.ignoreBindParam.RemoveAt (removeIndex);
            }
        }

        private void DefaultMatConfig (ref Rect rect)
        {
            if (config.defaultMatConfig.Count > DummyMatData.BatchFun.Length)
            {
                for (int i = config.defaultMatConfig.Count - 1; i > DummyMatData.BatchFun.Length - 1; --i)
                {
                    config.defaultMatConfig.RemoveAt (i);
                }
            }
            EditorGUI.indentLevel++;
            for (int i = 0; i < DummyMatData.BatchFun.Length; ++i)
            {
                var funName = DummyMatData.BatchFun[i];
                if (i >= config.defaultMatConfig.Count)
                {
                    config.defaultMatConfig.Add (new DefaultMatConfig ());
                }
                var dmc = config.defaultMatConfig[i];
                if (config.folder.FolderGroup (funName, funName, rect.width - 10))
                {
                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.LabelField ("KeywordConfg", GUILayout.MaxWidth (100));
                    if (GUILayout.Button ("AddConfig", GUILayout.MaxWidth (80)))
                    {
                        dmc.configs.configs.Add (new MatKeywordConfig ());
                    }
                    bool keyConfigFolder = config.folder.IsFolder (dmc.configs.GetHash ());
                    if (GUILayout.Button (keyConfigFolder? "Hide": "Edit", GUILayout.MaxWidth (60)))
                    {
                        keyConfigFolder = !keyConfigFolder;
                        config.folder.SetFolder (dmc.configs.GetHash (), keyConfigFolder);
                    }
                    EditorGUILayout.EndHorizontal ();
                    if (keyConfigFolder)
                    {
                        int removeIndex = ToolsUtility.BeginDelete ();
                        for (int j = 0; j < dmc.configs.configs.Count; ++j)
                        {
                            var c = dmc.configs.configs[j];
                            EditorGUILayout.BeginHorizontal ();
                            c.suffix = EditorGUILayout.TextField (
                                string.Format ("Suffix.{0}({1})", j.ToString (), c.keywords.Count.ToString ()),
                                c.suffix, GUILayout.MaxWidth (300));
                            if (GUILayout.Button ("Add", GUILayout.MaxWidth (60)))
                            {
                                c.keywords.Add ("");
                            }
                            bool folder = config.folder.IsFolder (c.GetHash ());
                            if (GUILayout.Button (folder? "Hide": "Edit", GUILayout.MaxWidth (60)))
                            {
                                folder = !folder;
                                config.folder.SetFolder (c.GetHash (), folder);
                            }
                            ToolsUtility.DeleteButton (ref removeIndex, j);
                            EditorGUILayout.EndHorizontal ();

                            if (folder)
                            {
                                EditorGUILayout.Space ();
                                EditorGUI.indentLevel++;
                                int deleteIndex = ToolsUtility.BeginDelete ();
                                for (int k = 0; k < c.keywords.Count; ++k)
                                {
                                    string key = c.keywords[k];
                                    EditorGUILayout.BeginHorizontal ();
                                    key = EditorGUILayout.TextField ("Keywords", key, GUILayout.MaxWidth (300));
                                    c.keywords[k] = key;
                                    ToolsUtility.DeleteButton (ref deleteIndex, k);
                                    EditorGUILayout.EndHorizontal ();
                                }
                                ToolsUtility.EndDelete (deleteIndex, c.keywords);
                                EditorGUI.indentLevel--;
                                EditorGUILayout.Space ();
                            }
                        }
                        ToolsUtility.EndDelete (removeIndex, dmc.configs.configs);
                    }

                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.LabelField ("LodConfg", GUILayout.MaxWidth (100));
                    if (GUILayout.Button ("AddConfig", GUILayout.MaxWidth (80)))
                    {
                        dmc.lodConfigs.configs.Add (new MatLodConfig ());
                    }
                    bool lodfolder = config.folder.IsFolder (dmc.GetHash ());
                    if (GUILayout.Button (lodfolder? "Hide": "Edit", GUILayout.MaxWidth (60)))
                    {
                        lodfolder = !lodfolder;
                        config.folder.SetFolder (dmc.GetHash (), lodfolder);
                    }
                    EditorGUILayout.EndHorizontal ();
                    if (lodfolder)
                    {
                        int removeIndex = ToolsUtility.BeginDelete ();
                        for (int j = 0; j < dmc.lodConfigs.configs.Count; ++j)
                        {
                            var c = dmc.lodConfigs.configs[j];

                            EditorGUILayout.BeginHorizontal ();
                            EditorGUILayout.LabelField (string.Format ("Dist.{0}", j.ToString ()), GUILayout.MaxWidth (100));
                            c.matDesc = EditorGUILayout.TextField ("", c.matDesc, GUILayout.MaxWidth (200));
                            ToolsUtility.DeleteButton (ref removeIndex, j);
                            EditorGUILayout.EndHorizontal ();

                            EditorGUI.indentLevel++;
                            EditorGUILayout.BeginHorizontal ();
                            EditorGUILayout.LabelField ("LodHigh", GUILayout.MaxWidth (100));
                            c.findSuffixLod0 = EditorGUILayout.TextField ("", c.findSuffixLod0, GUILayout.MaxWidth (200));
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField ("LodMedium", GUILayout.MaxWidth (100));
                            c.findSuffixLod1 = EditorGUILayout.TextField ("", c.findSuffixLod1, GUILayout.MaxWidth (200));
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField ("LodLow", GUILayout.MaxWidth (100));
                            c.findSuffixLod2 = EditorGUILayout.TextField ("", c.findSuffixLod2, GUILayout.MaxWidth (200));
                            EditorGUILayout.EndHorizontal();
                            EditorGUI.indentLevel--;
                        }
                        ToolsUtility.EndDelete (removeIndex, dmc.lodConfigs.configs);
                    }

                    EditorCommon.EndFolderGroup ();
                }
            }
            EditorGUI.indentLevel--;
        }
        #endregion

        #region effect mat
        private void EffectHeadGUI (ref Rect rect, ref MatListContext context, DummyMatData data)
        {
            rect.width = 80;
            if (GUI.Button (rect, "GenAll"))
            {
                opMatType = OpMatType.OpGenAllMat;
            }
            rect.x += 85;
            if (GUI.Button (rect, "Refresh"))
            {
                opMatType = OpMatType.OpRefreshAllMat;
            }
            int matCount = (int) EEffectMaterial.Num;
            if (data.materialInfos.Count > matCount)
                data.materialInfos.RemoveRange (matCount, data.materialInfos.Count - matCount);
            for (int i = data.materialInfos.Count; i < matCount; ++i)
            {
                data.materialInfos.Add (new DummyMaterialInfo ());
            }
            InitMat (data);
        }

        private void EffectMatConfigGUI (ref ListElementContext lec, ref MatListContext context, DummyMatData data, int i)
        {
            string name = ((EEffectMaterial) i).ToString ();
            InnerMatConfigGUI (ref lec, ref context, data, i, context.name, name);
        }
        #endregion
    }
}
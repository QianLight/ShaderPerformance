using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace CFEngine.Editor
{
    public delegate void OnProcessRawScene(out bool dirty, out bool error);
    public delegate void OnProcessCopiedScene(out bool error);
    
    public partial class SceneListTool : CommonToolTemplate
    {
        enum OpType
        {
            OpNone,
            OpRefreshSceneList,
            OpSaveSceneLoadList,
            OpSaveScene,
            OpRefreshUISceneList,
            OpLoadUIScene,
            OpSaveSceneRes,
            OpClearNoUseData,
            OpSaveAllScene2Bundle,
            OpConvertAllSceneLigtmap,
            OpRecoverAllSceneLighmap,
            OpFormatAllSceneLightmap,
            OpAddEnvColorVolumeToAllScene,
        }

        private SceneList sceneListConfig;
        private UISceneConfig uiSceneConfig;
        private UIScene currentUIScene;
        private bool saveScene = false;
        private bool clearallscene = false;
        private SceneAsset sceneAsset = null;
        private OpType opType = OpType.OpNone;
        private Vector2 sceneResListScroll = Vector2.zero;
        private Vector2 sceneListScroll = Vector2.zero;
        private Vector2 uiSceneListScroll = Vector2.zero;
        private bool sceneSystemFolder = true;
        private bool sceneListFolder = true;
        private bool uiSceneListFolder = true;

        private SceneBatchTool batchTool = new SceneBatchTool();
        
        private int switchIndex = -1;
        
        public override void OnInit ()
        {
            base.OnInit ();
            string path = string.Format ("{0}/SceneList.asset", AssetsConfig.instance.EngineResPath);
            sceneListConfig = AssetDatabase.LoadAssetAtPath<SceneList> (path);
            if (sceneListConfig == null)
            {
                sceneListConfig = ScriptableObject.CreateInstance<SceneList> ();
                sceneListConfig = CommonAssets.CreateAsset<SceneList> (path, ".asset", sceneListConfig);
            }

            path = string.Format ("{0}/UISceneList.asset", AssetsConfig.instance.EngineResPath);
            uiSceneConfig = AssetDatabase.LoadAssetAtPath<UISceneConfig> (path);
            if (uiSceneConfig == null)
            {
                uiSceneConfig = ScriptableObject.CreateInstance<UISceneConfig> ();
                uiSceneConfig = CommonAssets.CreateAsset<UISceneConfig> (path, ".asset", uiSceneConfig);
            }
            if (EngineContext.IsRunning)
            {
                var context = EngineContext.instance;
                for (int i = 0; i < sceneListConfig.sceneList.Count; ++i)
                {
                    var scene = sceneListConfig.sceneList[i];
                    scene.ids = "";
                    if (scene.sceneAsset != null)
                    {
                        string sceneName = scene.sceneAsset.name;
                        List<int> ids;
                        if (context.sceneIDs.TryGetValue (sceneName, out ids))
                        {
                            for (int j = 0; j < ids.Count; ++j)
                            {
                                scene.ids += string.Format ("{0};", ids[j].ToString ());
                            }
                        }
                    }
                }
            }
        }
        public override void DrawGUI (ref Rect rect)
        {
            if(EditorCommon.BeginFolderGroup("SceneRes",ref sceneSystemFolder, rect.width))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add", GUILayout.MaxWidth(80)))
                {
                    SceneResConfig.instance.configs.Add(new SceneResData());
                }
                if (GUILayout.Button("Save", GUILayout.MaxWidth(80)))
                {
                    opType = OpType.OpSaveSceneRes;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                var src = SceneResConfig.instance;
                src.bakeObjFolderName = EditorGUILayout.TextField("Static", src.bakeObjFolderName, GUILayout.MaxWidth(300));
                src.bakeTerrainFolderName = EditorGUILayout.TextField("Terrain", src.bakeTerrainFolderName, GUILayout.MaxWidth(300));
                EditorGUILayout.EndHorizontal();

                EditorCommon.BeginScroll(ref sceneResListScroll, SceneResConfig.instance.configs
                    .Count, 20, -1, rect.width - 20);
                int index = ToolsUtility.BeginDelete();
                int switchTarget = -1;
                for (int i = 0; i < SceneResConfig.instance.configs.Count; ++i)
                {
                    var srd = SceneResConfig.instance.configs[i];
                    EditorGUILayout.BeginHorizontal();
                    srd.resName = EditorGUILayout.TextField("", srd.resName, GUILayout.MaxWidth(300));
                    srd.systemName = EditorGUILayout.TextField("", srd.systemName, GUILayout.MaxWidth(300));
                    EditorGUILayout.LabelField("HasNode", GUILayout.MaxWidth(60));
                    srd.hasNode = EditorGUILayout.Toggle("", srd.hasNode, GUILayout.MaxWidth(100));
                    EditorGUILayout.LabelField("Tag", GUILayout.MaxWidth(60));
                    srd.hasSceneTag = EditorGUILayout.Toggle("", srd.hasSceneTag, GUILayout.MaxWidth(100));
                    srd.sceneType = (ESceneType)EditorGUILayout.EnumPopup(srd.sceneType, GUILayout.MaxWidth(150));
                    ToolsUtility.DeleteButton(ref index, i, true);
                    if (switchIndex == -1)
                    {
                        if (GUILayout.Button("Select", GUILayout.MaxWidth(80)))
                        {
                            switchIndex = i;
                        }
                    }
                    else if (switchIndex != i)
                    {
                        if (GUILayout.Button("Switch", GUILayout.MaxWidth(80)))
                        {
                            switchTarget = i;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                ToolsUtility.EndDelete(index, SceneResConfig.instance.configs);
                if (switchTarget != -1)
                {
                    var target = SceneResConfig.instance.configs[switchIndex];
                    SceneResConfig.instance.configs[switchIndex] = SceneResConfig.instance.configs[switchTarget];
                    SceneResConfig.instance.configs[switchTarget] = target;
                    switchIndex = -1;
                }
                EditorCommon.EndScroll();
                EditorCommon.EndFolderGroup();
            }
            

            if (sceneListConfig != null)
            {
                if (EditorCommon.BeginFolderGroup("Scenes", ref sceneListFolder, rect.width))
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Add", GUILayout.MaxWidth(80)))
                    {
                        sceneListConfig.sceneList.Add(new SceneInfo());
                    }
                    if (GUILayout.Button("Referesh", GUILayout.MaxWidth(80)))
                    {
                        opType = OpType.OpRefreshSceneList;
                    }

                    if (GUILayout.Button(string.Format("SaveSceneLoadList({0})", sceneListConfig.sceneList.Count.ToString()),
                            GUILayout.MaxWidth(160)))
                    {
                        opType = OpType.OpSaveSceneLoadList;
                    }

                    if(GUILayout.Button(string.Format("SaveAllScene2Bundle({0})", sceneListConfig.sceneList.Count.ToString()),
                            GUILayout.MaxWidth(160)))
                    {
                        if(EditorUtility.DisplayDialog("SaveAllScene2Bundle", "保存所有场景列表到 Bundle 资源下?", "OK", "No"))
                        {
                            opType = OpType.OpSaveAllScene2Bundle;
                        }
                    }

                    if (GUILayout.Button("ClearNoUseData",GUILayout.MaxWidth(180)))
                    {
                        opType = OpType.OpClearNoUseData;
                    }

                    saveScene = EditorGUILayout.Toggle("SaveScene", saveScene);

                    if(GUILayout.Button(clearallscene? "勾选所有" : "取消所有", GUILayout.MaxWidth(180)))
                    {
                        clearallscene = !clearallscene;
                        ClearAllSceneConvert();
                    }

                    GUILayout.EndHorizontal();
                    
                    GUILayout.BeginHorizontal();
                    if(GUILayout.Button(string.Format("FormatAllSceneLightmap({0})", sceneListConfig.sceneList.Count.ToString()),GUILayout.MaxWidth(180)))
                    {
                        if(EditorUtility.DisplayDialog("FormatAllSceneLightmap", "格式化所有场景的光照贴图命名?", "OK", "No"))
                        {
                            opType = OpType.OpFormatAllSceneLightmap;
                            Debug.Log("格式化所有场景的光照贴图命名成功！");
                        }
                    }
                    if(GUILayout.Button(string.Format("ConvertAllSceneLightmap({0})", sceneListConfig.sceneList.Count.ToString()),GUILayout.MaxWidth(180)))
                    {
                        if(EditorUtility.DisplayDialog("ConvertAllSceneLightmap", "将所有场景的Lightmap和Shadowmask贴图合并?", "OK", "No"))
                        {
                            opType = OpType.OpConvertAllSceneLigtmap;
                            Debug.Log("将所有场景的Lightmap和Shadowmask贴图合并成功！");
                        }
                    }
                    if(GUILayout.Button(string.Format("RecoverAllSceneLightmap({0})", sceneListConfig.sceneList.Count.ToString()),GUILayout.MaxWidth(180)))
                    {
                        if(EditorUtility.DisplayDialog("RecoverAllSceneLightmap", "将所有场景光照贴图还原?", "OK", "No"))
                        {
                            opType = OpType.OpRecoverAllSceneLighmap;
                            Debug.Log("将所有场景光照贴图还原成功！");
                        }
                    }
                    
                    if(GUILayout.Button(string.Format("AddEnvColorVolumeToAllScene({0})", sceneListConfig.sceneList.Count.ToString()),GUILayout.MaxWidth(180)))
                    {
                        if(EditorUtility.DisplayDialog("FormatAllSceneLightmap", "给所有使用的场景添加EnvColorAdjustVolume?", "OK", "No"))
                        {
                            opType = OpType.OpAddEnvColorVolumeToAllScene;
                            Debug.Log("所有使用的场景添加 EnvColorAdjustVolume 成功！");
                        }
                    }
                    
                    GUILayout.EndHorizontal();
                    
                    GUILayout.Space(10);
                    EditorCommon.BeginScroll(ref sceneListScroll, sceneListConfig.sceneList.Count, 20, -1, rect.width - 20);
                    int deleteIndex = -1;
                    for (int i = 0; i < sceneListConfig.sceneList.Count; ++i)
                    {
                        GUILayout.BeginHorizontal();
                        var scene = sceneListConfig.sceneList[i];
                        scene.sceneAsset = EditorGUILayout.ObjectField(scene.sceneAsset, typeof(SceneAsset), false, GUILayout.MaxWidth(260)) as SceneAsset;
                        EditorGUILayout.LabelField("NotBuild", GUILayout.MaxWidth(100));
                        scene.notBuild = EditorGUILayout.Toggle("", scene.notBuild, GUILayout.MaxWidth(100));
                        if (GUILayout.Button("Delete", GUILayout.MaxWidth(80)))
                        {
                            deleteIndex = i;
                        }
                        if (GUILayout.Button("Open", GUILayout.MaxWidth(80)))
                        {
                            if (scene.sceneAsset != null)
                            {
                                if (EditorUtility.DisplayDialog("Open", "Open without Scene?", "OK", "No"))
                                {
                                    //SceneAssets.SceneModify(true);
                                    string path = AssetDatabase.GetAssetPath(scene.sceneAsset);
                                    EditorSceneManager.OpenScene(path);
                                    NullComponentAndMissingPrefabSearchTool.Clear();
                                }             
                            }
                        }
                        if (GUILayout.Button("Save", GUILayout.MaxWidth(80)))
                        {
                            if (scene.sceneAsset != null && !scene.notBuild)
                            {
                                opType = OpType.OpSaveScene;
                                sceneAsset = scene.sceneAsset;
                            }
                        }
                        GUILayout.Space(25);

                        EditorGUILayout.LabelField("需要处理", GUILayout.MaxWidth(100));
                        GUILayout.Space(0);
                        scene.needconvert = EditorGUILayout.Toggle(scene.needconvert, GUILayout.MaxWidth(100));

                        GUILayout.EndHorizontal();
                        if (!string.IsNullOrEmpty(scene.ids))
                        {
                            EditorGUI.indentLevel++;
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("SceneIDs:", GUILayout.MaxWidth(100));
                            EditorGUILayout.LabelField(scene.ids, GUILayout.MaxWidth(600));
                            GUILayout.EndHorizontal();
                            EditorGUI.indentLevel--;
                        }

                    }
                    if (deleteIndex >= 0)
                    {
                        sceneListConfig.sceneList.RemoveAt(deleteIndex);
                    }

                    EditorGUILayout.BeginVertical(GUILayout.MaxWidth(180));
                    if (!batchTool.Initialized)
                        batchTool.Init();
                    batchTool.OnGUI();
                    EditorGUILayout.EndVertical();
                    
                    EditorCommon.EndScroll();

                    EditorCommon.EndFolderGroup();
                }                
            }

            if (uiSceneConfig != null)
            {
                if (EditorCommon.BeginFolderGroup("UIScenes", ref uiSceneListFolder, rect.width))
                {
                    if (GUILayout.Button("Referesh", GUILayout.MaxWidth(80)))
                    {
                        opType = OpType.OpRefreshUISceneList;
                    }
                    EditorCommon.BeginScroll(ref uiSceneListScroll, uiSceneConfig.uiSceneList.Count, 10, -1, rect.width - 20);
                    for (int i = 0; i < uiSceneConfig.uiSceneList.Count; ++i)
                    {
                        GUILayout.BeginHorizontal();
                        var uiScene = uiSceneConfig.uiSceneList[i];
                        EditorGUILayout.ObjectField(uiScene, typeof(UIScene), false, GUILayout.MaxWidth(400));
                        if (GUILayout.Button("Load", GUILayout.MaxWidth(80)))
                        {
                            currentUIScene = uiScene;
                            opType = OpType.OpLoadUIScene;
                        }
                        GUILayout.EndHorizontal();
                    }
                    EditorCommon.EndScroll();
                    EditorCommon.EndFolderGroup();
                }
            }
        }



        public override void Update ()
        {
            switch (opType)
            {
                case OpType.OpRefreshSceneList:
                    RefreshSceneList ();
                    break;
                case OpType.OpSaveSceneLoadList:
                    FastSaveSceneLoadList ();
                    break;
                case OpType.OpRefreshUISceneList:
                    RefreshUISceneList ();
                    break;
                case OpType.OpLoadUIScene:
                    LoadUIScene ();
                    break;
                case OpType.OpClearNoUseData:
                    ClearSceneNoUseData();
                    break;
                case OpType.OpSaveSceneRes:
                    SceneResConfig.instance.Save ();
                    break;
                case OpType.OpSaveScene:
                    SceneSerialize.FastSaveScene (sceneAsset, saveScene, null);
                    sceneAsset = null;
                    break;
                case OpType.OpSaveAllScene2Bundle:
                    SaveAllScene2Bundle();
                    break;
                case OpType.OpFormatAllSceneLightmap:
                    FormatAllSceneLightmap();
                    break;
                case OpType.OpConvertAllSceneLigtmap:
                    ConvertAllSceneLightmap();
                    break;
                case OpType.OpRecoverAllSceneLighmap:
                    RecoverAllSceneLightmap();
                    break;
                case OpType.OpAddEnvColorVolumeToAllScene:
                    AddEnvColorVolumeToAllScene();
                    break;
            }
            opType = OpType.OpNone;
        }
        void RefreshSceneList ()
        {
            if (sceneListConfig != null)
            {
                HashSet<string> notBuildScene = new HashSet<string> ();
                for (int i = 0; i < sceneListConfig.sceneList.Count; ++i)
                {
                    var s = sceneListConfig.sceneList[i];
                    if (s.notBuild && s.sceneAsset != null)
                    {
                        notBuildScene.Add (s.sceneAsset.name);
                    }
                }
                sceneListConfig.sceneList.Clear ();
                CommonAssets.enumSceneAsset.cb = (scene, path, context) =>
                {
                    string dirName = AssetsPath.GetParentFolderName (path);
                    if (scene.name == dirName)
                    {
                        sceneListConfig.sceneList.Add (new SceneInfo () { sceneAsset = scene, notBuild = notBuildScene.Contains (scene.name) });
                    }
                };
                CommonAssets.EnumAsset<SceneAsset> (CommonAssets.enumSceneAsset,
                    "RefreshSceneList",
                    AssetsConfig.instance.SceneLibPath, null, false, true);
                CommonAssets.SaveAsset (sceneListConfig);
            }
        }

        /// <summary>
        /// 保存所有未勾选NoBuild的场景到Bundle资源场景下
        /// </summary>
        void SaveAllScene2Bundle()
        {
            if(sceneListConfig == null || sceneListConfig.sceneList == null)
                return;

            Scene orignScene = EditorSceneManager.GetActiveScene();

            string orginScenePath = orignScene.path;
            SceneInfo tempSceneInfo;
            string tempScenePath;
            for(int i = 0; i < sceneListConfig.sceneList.Count; i++)
            {
                tempSceneInfo = sceneListConfig.sceneList[i];
                if(tempSceneInfo.sceneAsset == null || tempSceneInfo.notBuild)
                {
                    continue;
                }
                
                tempScenePath = AssetDatabase.GetAssetPath(tempSceneInfo.sceneAsset);
                EditorSceneManager.OpenScene(tempScenePath);

                string sceneDir = Path.GetDirectoryName(tempScenePath);
                string sceneConfigPath = string.Format("{0}/Config/{1}{2}.asset", sceneDir, tempSceneInfo.sceneAsset.name, SceneContext.SceneConfigSuffix);
                SceneConfig sc = AssetDatabase.LoadAssetAtPath<SceneConfig>(sceneConfigPath);
                if(sc == null)
                {
                    continue;
                }

                SceneContext context = new SceneContext();
                SceneAssets.GetSceneContext(ref context, tempSceneInfo.sceneAsset.name, tempScenePath);
                SceneSerialize.SaveScene2(sc, ref context, true, false);

                SceneEditTool.SaveScenetoBundleRes();
            }

            EditorSceneManager.OpenScene(orginScenePath);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("SaveAllScene2Bundle", "全部导出完成！", "好的");
        }

        private void FormatAllSceneLightmap()
        {
            if(sceneListConfig == null || sceneListConfig.sceneList == null)
                return;

            Scene orignScene = EditorSceneManager.GetActiveScene();

            string orginScenePath = orignScene.path;
            SceneInfo tempSceneInfo;
            string tempScenePath;
            LightmapVolumn[] lightmapVolumns;
            string currentSceneName;
            for(int i = 0; i < sceneListConfig.sceneList.Count; i++)
            {
                tempSceneInfo = sceneListConfig.sceneList[i];
                if(tempSceneInfo.sceneAsset == null || tempSceneInfo.notBuild)
                {
                    continue;
                }
                
                tempScenePath = AssetDatabase.GetAssetPath(tempSceneInfo.sceneAsset);
                EditorSceneManager.OpenScene(tempScenePath);
                
                lightmapVolumns = FindObjectsOfType<LightmapVolumn>(true);
                currentSceneName = EditorSceneManager.GetActiveScene().name;
                LightmapCombineManager.Instance.FormatLightmap(lightmapVolumns, currentSceneName);
            }

            EditorSceneManager.OpenScene(orginScenePath);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("FormatAllSceneLightmap", "全部场景的lightmap命名格式化完成！", "好的");
        }

        private void ConvertAllSceneLightmap()
        {
            if(sceneListConfig == null || sceneListConfig.sceneList == null)
                return;

            Scene orignScene = EditorSceneManager.GetActiveScene();

            string orginScenePath = orignScene.path;
            SceneInfo tempSceneInfo;
            string tempScenePath;
            LightmapVolumn[] lightmapVolumns;
            
            for(int i = 0; i < sceneListConfig.sceneList.Count; i++)
            {
                tempSceneInfo = sceneListConfig.sceneList[i];
                if(tempSceneInfo.sceneAsset == null || tempSceneInfo.notBuild)
                {
                    continue;
                }
                
                tempScenePath = AssetDatabase.GetAssetPath(tempSceneInfo.sceneAsset);

                //---------Test-------------
                // if (tempSceneInfo.sceneAsset.name != "OP_enieslobby_luqi2")
                // {
                //     continue;
                // }
                //--------------------------
                
                EditorSceneManager.OpenScene(tempScenePath);
                
                lightmapVolumns = FindObjectsOfType<LightmapVolumn>(true);
                LightmapCombineManager.Instance.ConvertLightmap(lightmapVolumns);
                
                
                string sceneDir = Path.GetDirectoryName(tempScenePath);
                string sceneConfigPath = string.Format("{0}/Config/{1}{2}.asset", sceneDir, tempSceneInfo.sceneAsset.name, SceneContext.SceneConfigSuffix);
                SceneConfig sc = AssetDatabase.LoadAssetAtPath<SceneConfig>(sceneConfigPath);
                if(sc == null)
                {
                    continue;
                }

                SceneContext context = new SceneContext();
                SceneAssets.GetSceneContext(ref context, tempSceneInfo.sceneAsset.name, tempScenePath);
                SceneSerialize.SaveScene2(sc, ref context, true, false);

                SceneEditTool.SaveScenetoBundleRes();
            }

            EditorSceneManager.OpenScene(orginScenePath);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("ConvertAllSceneLightmap", "全部场景的lightmap贴图合并完成！", "好的");
        }

        private void RecoverAllSceneLightmap()
        {
            if(sceneListConfig == null || sceneListConfig.sceneList == null)
                return;

            Scene orignScene = EditorSceneManager.GetActiveScene();

            string orginScenePath = orignScene.path;
            SceneInfo tempSceneInfo;
            string tempScenePath;
            LightmapVolumn[] lightmapVolumns;
            
            for(int i = 0; i < sceneListConfig.sceneList.Count; i++)
            {
                tempSceneInfo = sceneListConfig.sceneList[i];
                if(tempSceneInfo.sceneAsset == null || tempSceneInfo.notBuild)
                {
                    continue;
                }
                
                tempScenePath = AssetDatabase.GetAssetPath(tempSceneInfo.sceneAsset);

                //---------Test-------------
                // if (tempSceneInfo.sceneAsset.name != "OP_enieslobby_luqi2")
                // {
                //     continue;
                // }
                //--------------------------
                
                EditorSceneManager.OpenScene(tempScenePath);
                
                lightmapVolumns = FindObjectsOfType<LightmapVolumn>(true);
                LightmapCombineManager.Instance.RecoverLightmap(lightmapVolumns);
                
                
                string sceneDir = Path.GetDirectoryName(tempScenePath);
                string sceneConfigPath = string.Format("{0}/Config/{1}{2}.asset", sceneDir, tempSceneInfo.sceneAsset.name, SceneContext.SceneConfigSuffix);
                SceneConfig sc = AssetDatabase.LoadAssetAtPath<SceneConfig>(sceneConfigPath);
                if(sc == null)
                {
                    continue;
                }

                SceneContext context = new SceneContext();
                SceneAssets.GetSceneContext(ref context, tempSceneInfo.sceneAsset.name, tempScenePath);
                SceneSerialize.SaveScene2(sc, ref context, true, false);

                SceneEditTool.SaveScenetoBundleRes();
            }

            EditorSceneManager.OpenScene(orginScenePath);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("RecoverAllSceneLightmap", "全部场景的lightmap还原完成！", "好的");
        }

        private void AddEnvColorVolumeToAllScene()
        {
            if(sceneListConfig == null || sceneListConfig.sceneList == null)
                return;

            Scene orignScene = EditorSceneManager.GetActiveScene();

            string orginScenePath = orignScene.path;
            SceneInfo tempSceneInfo;
            string tempScenePath;
            
            for(int i = 0; i < sceneListConfig.sceneList.Count; i++)
            {
                tempSceneInfo = sceneListConfig.sceneList[i];
                if(tempSceneInfo.sceneAsset == null || tempSceneInfo.notBuild)
                {
                    continue;
                }
                
                tempScenePath = AssetDatabase.GetAssetPath(tempSceneInfo.sceneAsset);

                //---------Test-------------
                // if (tempSceneInfo.sceneAsset.name != "OP_Newvillage_Mogan")
                // {
                //     continue;
                // }
                //--------------------------
                
                EditorSceneManager.OpenScene(tempScenePath);
                
                Volume[] volumes = FindObjectsOfType<Volume>();
                for (int j = 0; j < volumes.Length; j++)
                {
                    Volume tempVolume = volumes[j];
                    VolumeProfile volumeProfile = tempVolume.sharedProfile;

                    if (volumeProfile.Has<EnvironmentVolume>())
                    {
                        volumeProfile.TryGet(out EnvironmentVolume environmentVolume);
                        if (environmentVolume != null && environmentVolume.CameraClearFlagsParam != null)
                        {
                            environmentVolume.CameraClearFlagsParam.overrideState = true;
                            environmentVolume.CameraClearFlagsParam.value = CameraClearSigns.Skybox;
                        }
                    }

                    if (!volumeProfile.Has<EnvironmentColorAdjust>())
                    {
                        EnvironmentColorAdjust environmentColorAdjust = volumeProfile.Add<EnvironmentColorAdjust>();
                        environmentColorAdjust.SetAllOverrideState(true);
                        AssetDatabase.AddObjectToAsset(environmentColorAdjust, volumeProfile);
                    }
                    
                    if (EditorUtility.IsPersistent(tempVolume.sharedProfile))
                    {
                        EditorUtility.SetDirty(tempVolume.sharedProfile);
                        AssetDatabase.SaveAssets();
                    }
                }
            }

            EditorSceneManager.OpenScene(orginScenePath);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("AddEnvColorVolumeToAllScene", "全部场景添加EnvColorVolume完成！", "好的");
        }

        private void FastSaveSceneLoadList ()
        {
            if (sceneListConfig != null && sceneListConfig.sceneList != null)
                CommonAssets.SaveAsset (sceneListConfig);
            SceneSerialize.FastSaveSceneLoadList (sceneListConfig, saveScene);
        }

        void ClearSceneNoUseData()
        {

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                string path = scene.path;
                UnityEngine.SceneManagement.Scene openedScene = EditorSceneManager.OpenScene(path);
                SceneEditTool.DeleteNoUseData();
                SceneAssets.SceneModify(true);
            }
        }

        void RefreshUISceneList ()
        {
            if (uiSceneConfig != null)
            {
                uiSceneConfig.uiSceneList.Clear ();
                CommonAssets.enumPrefab.cb = (prefab, path, context) =>
                {
                    var config = context as UISceneConfig;
                    UIScene uiScene;
                    if (prefab.TryGetComponent (out uiScene))
                    {
                        config.uiSceneList.Add (uiScene);
                    }
                };
                CommonAssets.EnumAsset<GameObject> (CommonAssets.enumPrefab,
                    "RefreshUISceneList",
                    string.Format ("{0}UIScene", LoadMgr.singleton.editorResPath), uiSceneConfig, false, true);
                CommonAssets.SaveAsset (uiSceneConfig);
            }
        }

        void LoadUIScene ()
        {
            if (currentUIScene != null)
            {
                var go = GameObject.Find (currentUIScene.name);
                if (go == null)
                {
                    go = PrefabUtility.InstantiatePrefab (currentUIScene.gameObject) as GameObject;
                }
                EditorGUIUtility.PingObject (go);

                currentUIScene = null;
            }
        }

        public static void ConvertScene(SceneList sceneListConfig, OnProcessRawScene rawSceneAction = null, OnProcessCopiedScene newSceneAction = null)
        {
            if (sceneListConfig == null || sceneListConfig.sceneList.Count == 0)
                return;

            string current = EditorSceneManager.GetActiveScene().path;
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            for (int i = 0; i < sceneListConfig.sceneList.Count; ++i)
            {
                SceneInfo _sceneinfo = sceneListConfig.sceneList[i];
                if (_sceneinfo.needconvert)
                {
                    string path = AssetDatabase.GetAssetPath(_sceneinfo.sceneAsset);
                    EditorSceneManager.OpenScene(path);
                    SceneEditTool.SaveScene_Urp(out bool error, rawSceneAction, newSceneAction);
                    if (error)
                        break;
                }
            }
            if (File.Exists(current))
                EditorSceneManager.OpenScene(current);
        }

        void ClearAllSceneConvert()
        {
            if (sceneListConfig != null && sceneListConfig.sceneList.Count > 0)
            {
                for (int i = 0; i < sceneListConfig.sceneList.Count; ++i)
                {
                    SceneInfo _sceneinfo = sceneListConfig.sceneList[i];
                    _sceneinfo.needconvert = !clearallscene;
                }
            }
        }
    }
}
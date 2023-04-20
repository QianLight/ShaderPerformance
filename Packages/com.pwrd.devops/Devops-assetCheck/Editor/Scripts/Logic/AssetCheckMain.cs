using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AssetCheck
{
    [InitializeOnLoad]
    public class AssetCheckMain
    {
        class CheckTaskInfos
        {
            public Type ruleT;
            public bool runtime;
            public RuleBase ruleInstance;
            public PublicMethod publicMethod;
            public MethodInfo checkMethod;
            public MethodInfo csvOutputMethod;
            public MethodInfo updateMethod;
            public List<PublicParam> rulePublicParams = new List<PublicParam>();
            public List<FieldInfo> rulePublicParamsFieldInfos = new List<FieldInfo>();
            public CheckRuleDescription checkRuleDescription;
            public List<FieldInfo> needClearParams = new List<FieldInfo>();
        }

        const string TitlePath = "路径";
        const string TitleName = "文件";
        const string TitleTags = "Tags";

        class ToExcelLine
        {
            public List<string> lineInfos = new List<string>();
            private List<string> tags = new List<string>();
            public ToExcelLine(string fileName)
            {
                bool bResult = AssetHelper.SplitFileRelativePathAndName(fileName, out string path, out string name);
                if (bResult)
                {
                    lineInfos.Add(name);
                    lineInfos.Add(path);
                    lineInfos.Add(string.Empty);
                }
                else
                {
                    lineInfos.Add(fileName);
                    lineInfos.Add(fileName);
                    lineInfos.Add(string.Empty);
                }
            }

            public void TryAddTag(string tag)
            {
                if (tag == string.Empty)
                    return;
                if (tags.Contains(tag))
                    return;
                tags.Add(tag);
            }

            public List<string> FinalLineInfo()
            {
                lineInfos[2] = string.Join("|", tags);
                return lineInfos;
            }
        }

        class ToExcelData
        {
            public Dictionary<string, ToExcelLine> excelDatas = new Dictionary<string, ToExcelLine>();
            public List<string> titleRules = new List<string>();
            public List<string> titleRuleNames = new List<string>();
            public List<string> collectLine = new List<string>();

            public ToExcelData()
            {
                titleRuleNames.Add(TitleName);
                titleRuleNames.Add(TitlePath);
                titleRuleNames.Add(TitleTags);

                titleRules.Add(string.Empty);
                titleRules.Add(string.Empty);
                titleRules.Add(string.Empty);

                collectLine.Add("总计");
                collectLine.Add(string.Empty);
                collectLine.Add(string.Empty);
            }

            public void AddRuleName(string rule, string ruleName, int passAssetCount, int failedAssetCount)
            {
                titleRules.Add(rule);
                titleRuleNames.Add(ruleName);
                collectLine.Add($"{failedAssetCount}/{passAssetCount + failedAssetCount}");
            }
            public List<List<string>> To2DArray()
            {
                List<List<string>> array = new List<List<string>>();
                array.Add(titleRuleNames);
                foreach (var line in excelDatas)
                {
                    array.Add(line.Value.FinalLineInfo());
                }
                array.Add(collectLine);
                return array;
            }
        }

        class ToAllExcelData
        {
            public Dictionary<string, ToExcelData> allExcelDatas = new Dictionary<string, ToExcelData>();
        }

        //static ToDevopsCheckResultData resultData;
        // 所有检查的任务
        static AssetCheckPathConfig needCheckData;
        // 当前正在检查的任务
        static RuleCheckAssetPath currentCheckTask;
        // 当前检查任务的index
        static int nRuleCheckIndex = 0;
        // 当前检查任务的规则
        static CheckTaskInfos currentCheckTaskInfos;
        // 当前检查文件和param组
        static CheckAssetWithParams currentAssetWithParamsGroup;
        // 当前检查组index
        static int nAssetWithParamsIndex = 0;
        // 所有检查的文件列表
        static List<string> needCheckAssetsPath;
        // 当前检查文件列表的Tags
        static string checkAssetsTags = string.Empty;
        // 当前检查的文件列表index
        static int nPathIndex = 0;
        // 当前正在检查的文件
        static string currentCheckAssetPath = null;
        // 当前task的output
        static RuleCheckResultData currentTaskOutput = null;
        static AssetPathGroupResultData currentAssetPathGroupOutput = null;
        static bool updateEnable = false;
        // 如果导出csv文件，就不输出json的log了
        static bool exportCSV = false;
        //static Dictionary<string, List<List<string>>> CSVDatas;
        static ToAllExcelData ExcelData = null;
        static Action<bool, string, string, string> actionAsyncMethod = new Action<bool, string, string, string>(SyncCheckEnd);
        public static bool CheckEndCloseApp = false;

        public static Action<int, int> EventCheckRuleProcessChanged;
        public static Action<bool> EventCheckRuleState;
        // runtime检查资源录制模式，如果是非录制模式，那就说明已经生成过结果了
        public static bool runtimeAssetRecordMode = true;
        public static AssetCheckRenderInfoConfig runtimeRenderInfoConfig = null;
        static void EnableUpdate(bool enable)
        {
            if (updateEnable == enable)
                return;
            updateEnable = enable;
            if (updateEnable)
            {
                EditorApplication.update += Update;
            }
            else
            {
                EditorApplication.update -= Update;
            }
        }

        public static object DefaultForType(Type targetType)
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }

        public static bool IsBusy()
        {
            return needCheckData != null;
        }

        public static void CSVEnable(bool enable)
        {
            exportCSV = enable;
            if (exportCSV)
            {
                //CSVDatas = new Dictionary<string, List<List<string>>>();
                ExcelData = new ToAllExcelData();
            }
            else
            {
                //CSVDatas = null;
                ExcelData = null;
            }
            if (runtimeRenderInfoConfig != null)
            {
                runtimeRenderInfoConfig.exportCSV = true;
            }
        }

        static void RenderRuntimeResultContent()
        {
            RuntimeRenderCheckResult.Init();
            runtimeAssetRecordMode = !RuntimeRenderCheckResult.HasGetResult();
            if (Directory.Exists(Defines.ResourceTempPath))
            {
                Directory.Delete(Defines.ResourceTempPath, true);
            }
        }

        static void RenderRuntimeRecordContent()
        {
            runtimeAssetRecordMode = !RuntimeRenderCheckResult.HasGetResult();
            if (!runtimeAssetRecordMode)
                return;
            runtimeRenderInfoConfig = new AssetCheckRenderInfoConfig();
            runtimeRenderInfoConfig.exportCSV = exportCSV;
            runtimeRenderInfoConfig.checkEndCloseApp = CheckEndCloseApp;
            runtimeRenderInfoConfig.assetCheckPathConfig = JsonUtility.ToJson(needCheckData);

            if (!Directory.Exists(Defines.ResourceTempPath))
            {
                Directory.CreateDirectory(Defines.ResourceTempPath);
            }
        }

        public static void OutputContent()
        {
            if (Directory.Exists(Defines.OutputDir))
            {
                string[] files = Directory.GetFiles(Defines.OutputDir);
                foreach (var file in files)
                {
                    File.Delete(file);
                }
            }
            else
            {
                Directory.CreateDirectory(Defines.OutputDir);
            }
        }

        static void SceneContent()
        {
            AssetHelper.BackLastScene();
            EditorApplication.ExecuteMenuItem("Window/General/Game");
        }

        [InitializeOnLoadMethod]
        static void EditorStateListen()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                CheckWithRuntimeRender();
            }
        }
        public static void CheckWithRuntimeRender()
        {
            if (!File.Exists(Defines.CheckPathRuntimeRenderResult))
                return;
            RenderRuntimeResultContent();
            string assetCheckPathConfigJson = RuntimeRenderCheckResult.GetAssetCheckPathConfigJson();
            CSVEnable(RuntimeRenderCheckResult.IsExportCSV());
            CheckEndCloseApp = RuntimeRenderCheckResult.IsCheckEndClose();
            AssetCheckPathConfig assetCheckPathConfig = AssetDatabase.LoadAssetAtPath<AssetCheckPathConfig>($"{Defines.CheckPathConfigPath}/{Defines.CheckPathConfigTempName}");
            if (!assetCheckPathConfig)
            {
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AssetCheckPathConfig>(), $"{Defines.CheckPathConfigPath}/{Defines.CheckPathConfigTempName}");
            }
            JsonUtility.FromJsonOverwrite(assetCheckPathConfigJson, assetCheckPathConfig);
            CheckWithConfig(assetCheckPathConfig);
        }

        public static void CheckWithConfig(AssetCheckPathConfig config)
        {
            needCheckData = config;
            RenderRuntimeRecordContent();
            OutputContent();
            SceneContent();
            EventCheckRuleState?.Invoke(true);
            EnableUpdate(true);
        }

        static int tryUpdateDeep = 0;
        static void TryUpdate()
        {
            if (tryUpdateDeep > 10)
            {
                tryUpdateDeep = 0;
                return;
            }
            tryUpdateDeep++;
            Update();
        }

        static void Update()
        {
            if(currentCheckTaskInfos != null && currentCheckTaskInfos.updateMethod != null)
            {
                currentCheckTaskInfos.updateMethod.Invoke(currentCheckTaskInfos.ruleInstance, new object[] { });
            }
            if (currentCheckTask == null)
            {
                if (NextTask())
                {
                    if (!GetCheckTaskInfos())
                    {
                        Debug.LogError($"errorMsg:{currentTaskOutput.ruleKey},{currentTaskOutput.errorMsg}");
                        EndCurrentTask();
                        return;
                    }
                    if (runtimeAssetRecordMode && !currentCheckTaskInfos.runtime)
                    {
                        EndCurrentTask();
                        return;
                    }
                }
                else
                {
                    //EndAllTask();
                    if (runtimeAssetRecordMode)
                        TryRuntimeCheck();
                    else
                        EndAllTask();
                    return;
                }
                if (currentCheckTask != null)
                    Debug.Log($"beginTask {currentCheckTask.ruleName}");
            }
            if (currentAssetWithParamsGroup == null)
            {
                if (NextFileAndParamGroup())
                {
                    if (SetParams())
                    {
                        currentAssetPathGroupOutput = new AssetPathGroupResultData();
                        currentAssetPathGroupOutput.checkAssetWithParams = currentAssetWithParamsGroup;
                        currentTaskOutput.assetPathGroups.Add(currentAssetPathGroupOutput);
                    }
                    else
                    {
                        EndFileAndParamGroup();
                        return;
                    }
                }
                else
                {
                    EndCurrentTask();
                    return;
                }
            }
            if (currentCheckAssetPath == null)
            {
                if (NextFile())
                {
                    CheckCurrentFile();
                }
                else
                {
                    EndFileAndParamGroup();
                    return;
                }
            }
        }

        static bool NextTask()
        {
            if (needCheckData.ruleCheckAssetPaths.Count > nRuleCheckIndex)
            {
                var checkTask = needCheckData.ruleCheckAssetPaths[nRuleCheckIndex++];
                EventCheckRuleProcessChanged?.Invoke(nRuleCheckIndex, needCheckData.ruleCheckAssetPaths.Count);
                if (checkTask.isOpen)
                {
                    currentCheckTask = checkTask;
                    currentTaskOutput = new RuleCheckResultData();
                    currentTaskOutput.ruleKey = checkTask.ruleName;
                    currentTaskOutput.ruleClassify = checkTask.ruleClassify;
                    currentTaskOutput.beginTime = DateTime.UtcNow.Ticks;
                    return true;
                }
                else
                {
                    return NextTask();
                }
            }
            return false;
        }

        static bool GetCheckTaskInfos()
        {
            Type ruleT = Type.GetType(currentCheckTask.ruleName);
            if (ruleT == null)
            {
                currentTaskOutput.errorMsg = $"AssetCheck Can not find rule type name {currentCheckTask.ruleName}";
                return false;
            }
            RuleBase ruleInstance = Activator.CreateInstance(ruleT) as RuleBase;
            if (ruleInstance == null)
            {
                currentTaskOutput.errorMsg = $"AssetCheck Can not find rule name {currentCheckTask.ruleName}";
                return false;
            }
            MethodInfo checkMethodInfo = ruleT.GetMethod("Check");
            if (checkMethodInfo == null)
            {
                currentTaskOutput.errorMsg = $"AssetCheck Can not find checkMethod {currentCheckTask.ruleName}";
                return false;
            }
            PublicMethod publicMethod = checkMethodInfo.GetCustomAttribute<PublicMethod>();
            if (publicMethod == null)
            {
                currentTaskOutput.errorMsg = $"AssetCheck Can not find PublicMethod {currentCheckTask.ruleName}.Check";
                return false;
            }
            var checkRuleDescription = ruleT.GetCustomAttribute<CheckRuleDescription>();
            if (checkRuleDescription == null)
            {
                currentTaskOutput.errorMsg = $"AssetCheck Can not find rule description name {currentCheckTask.ruleName}";
                return false;
            }
            MethodInfo csvOutputMethod = null;
            if ((typeof(CSVOutput)).IsAssignableFrom(ruleT))
            {
                csvOutputMethod = ruleT.GetMethod("ResultOutput");
            }
            MethodInfo updateMethod = ruleT.GetMethod("Update", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            currentTaskOutput.ruleName = checkRuleDescription.description;
            currentTaskOutput.description = checkRuleDescription.detailedDescription;
            List<PublicParam> rulePublicParams = new List<PublicParam>();
            List<FieldInfo> rulePublicParamsFieldInfos = new List<FieldInfo>();
            List<FieldInfo> needClearParams = new List<FieldInfo>();
            FieldInfo[] memberParamInfos = ruleT.GetFields();
            foreach (var memberParamInfo in memberParamInfos)
            {
                PublicParam pp = memberParamInfo.GetCustomAttribute<PublicParam>();
                if (pp == null)
                {
                    if (!memberParamInfo.IsStatic)
                        needClearParams.Add(memberParamInfo);
                }
                else
                {
                    rulePublicParams.Add(pp);
                    rulePublicParamsFieldInfos.Add(memberParamInfo);
                }
            }
            FieldInfo[] nonPublicMemberParamInfos = ruleT.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var memberParamInfo in nonPublicMemberParamInfos)
            {
                PublicParam pp = memberParamInfo.GetCustomAttribute<PublicParam>();
                if (pp == null)
                {
                    if (!memberParamInfo.IsStatic)
                        needClearParams.Add(memberParamInfo);
                }
                else
                {
                    rulePublicParams.Add(pp);
                    rulePublicParamsFieldInfos.Add(memberParamInfo);
                }
            }

            currentCheckTaskInfos = new CheckTaskInfos();
            currentCheckTaskInfos.ruleT = ruleT;
            currentCheckTaskInfos.runtime = checkRuleDescription.runtime;
            currentCheckTaskInfos.ruleInstance = ruleInstance;
            currentCheckTaskInfos.publicMethod = publicMethod;
            currentCheckTaskInfos.checkMethod = checkMethodInfo;
            currentCheckTaskInfos.csvOutputMethod = csvOutputMethod;
            currentCheckTaskInfos.updateMethod = updateMethod;
            currentCheckTaskInfos.rulePublicParams = rulePublicParams;
            currentCheckTaskInfos.rulePublicParamsFieldInfos = rulePublicParamsFieldInfos;
            currentCheckTaskInfos.checkRuleDescription = checkRuleDescription;
            currentCheckTaskInfos.needClearParams = needClearParams;
            return true;
        }

        static void EndCurrentTask()
        {
            if (!runtimeAssetRecordMode)
            {
                if (currentTaskOutput != null)
                {
                    currentTaskOutput.endTime = DateTime.UtcNow.Ticks;
                }
                if (currentCheckTask != null && currentCheckTaskInfos != null)
                {
                    if (currentCheckTaskInfos.csvOutputMethod != null)
                    {
                        object[] csvMethodParams = new object[] { null };
                        try
                        {
                            List<List<string>> outputArray = (List<List<string>>)currentCheckTaskInfos.csvOutputMethod.Invoke(currentCheckTaskInfos.ruleInstance, csvMethodParams);
                            AssetHelper.SaveCSV(Defines.OutputDir + "/" + csvMethodParams[0] + ".csv", outputArray);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"CSVOutputError ruleName:{currentCheckTaskInfos.ruleT.Name}");
                        }
                        currentTaskOutput.outputFile = csvMethodParams[0] + ".csv";
                    }
                    if (exportCSV)
                    {
                        if (currentCheckTaskInfos.csvOutputMethod == null)
                        {
                            if (!ExcelData.allExcelDatas.ContainsKey(currentTaskOutput.ruleClassify))
                            {
                                ExcelData.allExcelDatas.Add(currentTaskOutput.ruleClassify, new ToExcelData());
                            }
                            if (!ExcelData.allExcelDatas[currentTaskOutput.ruleClassify].titleRules.Contains(currentTaskOutput.ruleKey))
                            {
                                ExcelData.allExcelDatas[currentTaskOutput.ruleClassify].AddRuleName(
                                    currentTaskOutput.ruleKey,
                                    currentTaskOutput.ruleName,
                                    currentTaskOutput.passAssetCount,
                                    currentTaskOutput.failedAssetCount);
                            }
                            int nRuleIndex = ExcelData.allExcelDatas[currentTaskOutput.ruleClassify].titleRules.IndexOf(currentTaskOutput.ruleKey);
                            foreach (var pathGroup in currentTaskOutput.assetPathGroups)
                            {
                                foreach (var pathAndOutput in pathGroup.assetsPathAndOutputs)
                                {
                                    if (!ExcelData.allExcelDatas[currentTaskOutput.ruleClassify].excelDatas.ContainsKey(pathAndOutput.path))
                                    {
                                        ExcelData.allExcelDatas[currentTaskOutput.ruleClassify].excelDatas.Add(pathAndOutput.path, new ToExcelLine(pathAndOutput.path));
                                    }
                                    ExcelData.allExcelDatas[currentTaskOutput.ruleClassify].excelDatas[pathAndOutput.path].TryAddTag(pathAndOutput.tags);
                                    while (ExcelData.allExcelDatas[currentTaskOutput.ruleClassify].excelDatas[pathAndOutput.path].lineInfos.Count <= nRuleIndex)
                                    {
                                        ExcelData.allExcelDatas[currentTaskOutput.ruleClassify].excelDatas[pathAndOutput.path].lineInfos.Add(string.Empty);
                                    }
                                    ExcelData.allExcelDatas[currentTaskOutput.ruleClassify].excelDatas[pathAndOutput.path].lineInfos[nRuleIndex] = pathAndOutput.output;
                                }
                            }
                        }
                    }
                    else
                    {
                        File.WriteAllText($"{Defines.OutputDir}/{currentCheckTask.ruleName}.json", JsonUtility.ToJson(currentTaskOutput));
                    }
                }

            }
            currentCheckTask = null;
            currentCheckTaskInfos = null;
            currentTaskOutput = null;
            nAssetWithParamsIndex = 0;
            nPathIndex = 0;
        }

        static void TryRuntimeCheck()
        {
            CopyRuntimeAssetsToTempFolder();
            if (runtimeRenderInfoConfig != null)
            {
                string jsonData = JsonUtility.ToJson(runtimeRenderInfoConfig);
                File.WriteAllText($"{Defines.CheckPathRuntimeConfigPath}/{Defines.CheckPathRuntimeRenderConfig}", jsonData);
            }
            EditorApplication.isPlaying = true;
        }

        static void EndAllTask()
        {
            nRuleCheckIndex = 0;
            EnableUpdate(false);
            needCheckData = null;
            ExportRulesCheckResultCSV();
            Debug.Log("AllTaskCheckEnd");
            RuntimeRenderCheckResult.Release();
            EventCheckRuleState?.Invoke(false);
            if (CheckEndCloseApp)
            {
                CloseApp();
            }
        }

        static void CloseApp()
        {
#if UNITY_EDITOR_OSX            
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "bash";
            string shPath = Directory.GetParent(Application.dataPath).FullName + "/" + Defines.ShellPath + "closehub.sh";
            p.StartInfo.Arguments = shPath;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();

            p.WaitForExit();
            p.Close();
#endif

            EditorApplication.Exit(0);
        }

        public static bool IsRunning()
        {
            return needCheckData != null;
        }
        public static int GetCurrentCheckIndex()
        {
            return nRuleCheckIndex;
        }
        public static int GetAllCheckRuleCount()
        {
            if (needCheckData == null)
                return 0;
            return needCheckData.ruleCheckAssetPaths.Count;
        }

        static void ExportRulesCheckResultCSV()
        {
            if (!exportCSV)
                return;
            //foreach (var classify in CSVDatas)
            //{
            //    AssetHelper.SaveCSV($"{Defines.OutputDir}/{classify.Key}.csv", classify.Value);
            //}
            foreach (var csvSingleData in ExcelData.allExcelDatas)
            {
                AssetHelper.SaveCSV($"{Defines.OutputDir}/{csvSingleData.Key}.csv", csvSingleData.Value.To2DArray());
            }
            AssetHelper.OpenFolder(Defines.OutputDir);
        }

        public static void ForceEnd()
        {
            EndCurrentFile();
            EndCurrentTask();
            EndFileAndParamGroup();
            EndAllTask();
        }

        static bool NextFileAndParamGroup()
        {
            if (currentCheckTask.content.checkPathsWithParams.Count > nAssetWithParamsIndex)
            {
                currentAssetWithParamsGroup = currentCheckTask.content.checkPathsWithParams[nAssetWithParamsIndex++];
                return true;
            }
            return false;
        }

        static bool SetParams()
        {
            if (currentCheckTaskInfos.rulePublicParamsFieldInfos.Count != currentAssetWithParamsGroup.ruleParams.Count)
            {
                currentTaskOutput.errorMsg = $"ruleParam count error {currentCheckTask.ruleName}";
                return false;
            }
            for (int i = 0; i < currentCheckTaskInfos.rulePublicParamsFieldInfos.Count; i++)
            {
                if (!currentCheckTaskInfos.rulePublicParamsFieldInfos[i].FieldType.ToString().Equals(
                    currentAssetWithParamsGroup.ruleParams[i].paramType))
                {
                    currentTaskOutput.errorMsg = $"ruleParam type not same {currentCheckTask.ruleName}";
                    return false;
                }
                object paramValue;
                if (currentAssetWithParamsGroup.ruleParams[i].paramValue == string.Empty)
                {
                    paramValue = DefaultForType(currentCheckTaskInfos.rulePublicParamsFieldInfos[i].FieldType);
                }
                else
                {
                    if (currentCheckTaskInfos.rulePublicParamsFieldInfos[i].FieldType.IsEnum)
                    {
                        paramValue = Enum.Parse(currentCheckTaskInfos.rulePublicParamsFieldInfos[i].FieldType, currentAssetWithParamsGroup.ruleParams[i].paramValue);
                    }
                    else
                    {
                        paramValue = Convert.ChangeType(currentAssetWithParamsGroup.ruleParams[i].paramValue,
                            currentCheckTaskInfos.rulePublicParamsFieldInfos[i].FieldType);
                    }
                }
                currentCheckTaskInfos.rulePublicParamsFieldInfos[i].SetValue(currentCheckTaskInfos.ruleInstance, paramValue);
            }
            if (currentAssetWithParamsGroup.assetPath == null)
                return false;
            if (AssetHelper.IsCustomFilter(currentCheckTaskInfos.checkRuleDescription.filter, out string customFilter))
            {
                needCheckAssetsPath = AssetHelper.FindAssetCustom(customFilter, currentAssetWithParamsGroup.assetPath, currentAssetWithParamsGroup.excludePaths, currentAssetWithParamsGroup.includeKeyword);
            }
            else
            {
                needCheckAssetsPath = AssetHelper.FindAssets(currentCheckTaskInfos.checkRuleDescription.filter, currentAssetWithParamsGroup.assetPath, currentAssetWithParamsGroup.excludePaths, currentAssetWithParamsGroup.includeKeyword);
            }
            currentTaskOutput.checkAssetCount += needCheckAssetsPath.Count;
            checkAssetsTags = currentAssetWithParamsGroup.tags;
            return needCheckAssetsPath.Count > 0;
        }

        static void EndFileAndParamGroup()
        {
            currentAssetWithParamsGroup = null;
            nPathIndex = 0;
        }

        static bool NextFile()
        {
            if (needCheckAssetsPath.Count > nPathIndex)
            {
                currentCheckAssetPath = needCheckAssetsPath[nPathIndex++];
                return true;
            }
            return false;
        }

        static void CheckCurrentFile()
        {
            // 如果是录制模式，就记录下来，直接pass
            if (runtimeAssetRecordMode)
            {
                if (currentCheckTaskInfos.runtime)
                {
                    //bool bCopyToRes = false;
                    if (!runtimeRenderInfoConfig.assetList.Contains(currentCheckAssetPath))
                    {
                        runtimeRenderInfoConfig.assetList.Add(currentCheckAssetPath);
                        /*
                        FileInfo fileInfo = new FileInfo(currentCheckAssetPath);
                        if(fileInfo.Exists)
                        {
                            if (!currentCheckAssetPath.StartsWith("Assets/Resources"))
                            {
                                string resoucePath = Defines.ResourceTempPath + "/" + Path.GetDirectoryName(currentCheckAssetPath);
                                string tempResouce = resoucePath + "/" + Path.GetFileName(currentCheckAssetPath);
                                if (!currentCheckAssetPath.StartsWith("Assets/Resources"))
                                {
                                    if (!Directory.Exists(resoucePath))
                                    {
                                        Directory.CreateDirectory(resoucePath);
                                    }
                                    AssetDatabase.CopyAsset(currentCheckAssetPath, tempResouce);
                                    runtimeRenderInfoConfig.copyAssetList.Add(tempResouce);
                                    bCopyToRes = true;
                                }
                            }
                        }
                        */
                    }
                    //if(!bCopyToRes)
                    //    runtimeRenderInfoConfig.copyAssetList.Add(string.Empty);
                }
                EndCurrentFile();
                TryUpdate();
                return;
            }
            // 如果这里单个task是csvoutput的，就不要清理里面的参数
            if (currentCheckTaskInfos.csvOutputMethod == null)
            {
                foreach (var needClearParam in currentCheckTaskInfos.needClearParams)
                {
                    object value = null;
                    if (needClearParam.FieldType.IsConstructedGenericType && !needClearParam.FieldType.Name.StartsWith("Action"))
                    {
                        value = Activator.CreateInstance(needClearParam.FieldType);
                    }
                    needClearParam.SetValue(currentCheckTaskInfos.ruleInstance, value);
                }
            }

            if (currentCheckTaskInfos.publicMethod.isAsync)
            {
                try
                {
                    object[] checkMethodParams = new object[] { currentCheckAssetPath, actionAsyncMethod };
                    currentCheckTaskInfos.checkMethod.Invoke(currentCheckTaskInfos.ruleInstance, checkMethodParams);
                }
                catch (Exception e)
                {
                    Debug.LogError($"CheckError Rule:{currentCheckTaskInfos.ruleT}, Path:{currentCheckAssetPath}, Error:{e.Message}, StackTrace:{e.StackTrace}");
                }
            }
            else
            {
                bool bPass = false;
                object[] checkMethodParams = new object[] { currentCheckAssetPath, null };
                try
                {
                    bPass = (bool)currentCheckTaskInfos.checkMethod.Invoke(currentCheckTaskInfos.ruleInstance, checkMethodParams);
                }
                catch (Exception e)
                {
                    Debug.LogError($"CheckError Rule:{currentCheckTaskInfos.ruleT}, Path:{currentCheckAssetPath}, Error:{e.Message}, StackTrace:{e.StackTrace}");
                }
                if (!bPass)
                {
                    AssetPathAndOutput assetsPathAndOutput = new AssetPathAndOutput();
                    assetsPathAndOutput.path = currentCheckAssetPath;
                    assetsPathAndOutput.tags = checkAssetsTags;
                    assetsPathAndOutput.output = AssetHelper.GetFrontChar((string)checkMethodParams[1], 1000);
                    currentAssetPathGroupOutput.assetsPathAndOutputs.Add(assetsPathAndOutput);
                    currentTaskOutput.failedAssetCount++;
                }
                else
                {
                    currentTaskOutput.passAssetCount++;
                }
                EndCurrentFile();
                TryUpdate();
            }
        }

        static void SyncCheckEnd(bool bPass, string assetPath, string ruleName, string output)
        {
            if (!assetPath.Equals(assetPath))
            {
                Debug.LogError($"当前检查的和异步callback的文件为啥不一样? {assetPath} {currentCheckAssetPath}");
            }
            if (!bPass)
            {
                AssetPathAndOutput assetsPathAndOutput = new AssetPathAndOutput();
                assetsPathAndOutput.path = currentCheckAssetPath;
                assetsPathAndOutput.tags = checkAssetsTags;
                assetsPathAndOutput.output = AssetHelper.GetFrontChar(output, 1000);
                currentAssetPathGroupOutput.assetsPathAndOutputs.Add(assetsPathAndOutput);
                currentTaskOutput.failedAssetCount++;
            }
            else
            {
                currentTaskOutput.passAssetCount++;
            }
            EndCurrentFile();
        }

        static void EndCurrentFile()
        {
            currentCheckAssetPath = null;
        }

        static void CopyRuntimeAssetsToTempFolder()
        {
            foreach (var assetPath in runtimeRenderInfoConfig.assetList)
            {
                bool bCopyToRes = false;
                FileInfo fileInfo = new FileInfo(assetPath);
                if (fileInfo.Exists)
                {
                    if (!assetPath.StartsWith("Assets/Resources"))
                    {
                        string resoucePath = Defines.ResourceTempPath + "/" + Path.GetDirectoryName(assetPath);
                        string tempResouce = resoucePath + "/" + Path.GetFileName(assetPath);
                        if (!assetPath.StartsWith("Assets/Resources"))
                        {
                            if (!Directory.Exists(resoucePath))
                            {
                                Directory.CreateDirectory(resoucePath);
                            }
                            AssetDatabase.CopyAsset(assetPath, tempResouce);
                            runtimeRenderInfoConfig.copyAssetList.Add(tempResouce);
                            bCopyToRes = true;
                        }
                    }
                }
                if (!bCopyToRes)
                {
                    runtimeRenderInfoConfig.copyAssetList.Add(string.Empty);
                }
            }
        }
    }

}

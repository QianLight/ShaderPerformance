using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using GUIFrameWork;
using System.Threading.Tasks;
using Devops.Core;
using System.IO;

namespace AssetCheck
{
    public class AssetCheckMainGUI : EditorWindow
    {
        static AssetCheckMainGUI mainWindow;
        static float windowHeight = 800;
        static float windowWidth = 1200;
        [MenuItem("Devops/AssetCheck/GUI &G")]
        static void ShowWindow()
        {
            mainWindow = (AssetCheckMainGUI)EditorWindow.GetWindow(typeof(AssetCheckMainGUI));
            mainWindow.titleContent = new GUIContent($"{Defines.AssetCheckName} Setting {Defines.VersionId}");
            mainWindow.maxSize = new Vector2(1200, 800);
            mainWindow.minSize = new Vector2(1200, 800);
            mainWindow.Show();
            windowHeight = mainWindow.position.height;
            windowWidth = mainWindow.position.width;
        }

        public static void HideWindow()
        {
            if(mainWindow != null)
            {
                mainWindow.Close();
                mainWindow = null;
            }
        }

        // 当前类别选择
        int currentClassifySelect = 0;
        // 当前规则搜索框内容
        string currentRuleFind = string.Empty;
        // 规则是否折叠
        Dictionary<string, bool> dicRulesFold;

        // 所有类别名字
        string[] classifyNames;
        // 类别对应的所有规则
        SortedDictionary<string, SortedSet<string>> sorteddicAllclassifyRules;
        // 规则对应类别
        Dictionary<string, string> dicAllRulesClassify;
        // 规则说明
        Dictionary<string, string> dicRuleDescriptions;
        // 详细规则说明
        Dictionary<string, string> dicDetailedDescription;
        // 规则检查目录和排除目录
        Dictionary<string, List<CheckAssetWithParams>> dicAllRuleCheckAssetPaths;
        // 规则上的默认参数（临时，用来和存储的数据做对比)
        Dictionary<string, List<ParamInfo>> dicAllRuleDefaultParamInfos;
        // 规则是否启用
        Dictionary<string, bool> dicRuleOpen;
        // tags
        public List<string> FilePathTags = new List<string>();
        public string[] FilePathTagsArray;

        bool bCheckState = false;
        float currentCheckPer = 0.0f;
        private void OnEnable()
        {
            RefreshTags();
            AssetCheckMain.EventCheckRuleProcessChanged += OnCheckRuleProcessChanged;
            AssetCheckMain.EventCheckRuleState += OnCheckRuleState;
            AssetCheckTagsUI.EventAssetCheckTagsChange += RefreshTags;
            bCheckState = AssetCheckMain.IsRunning();
            if (bCheckState)
            {
                currentCheckPer = (float)AssetCheckMain.GetCurrentCheckIndex() / Mathf.Max((float)AssetCheckMain.GetAllCheckRuleCount(), 1.0f);
            }
            else
            {
                currentCheckPer = 0.0f;
            }

            sorteddicAllclassifyRules = new SortedDictionary<string, SortedSet<string>>();
            dicAllRulesClassify = new Dictionary<string, string>();
            dicRuleDescriptions = new Dictionary<string, string>();
            dicDetailedDescription = new Dictionary<string, string>();
            dicRuleOpen = new Dictionary<string, bool>();
            dicAllRuleCheckAssetPaths = new Dictionary<string, List<CheckAssetWithParams>>();
            dicAllRuleDefaultParamInfos = new Dictionary<string, List<ParamInfo>>();
            var ruleTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsSubclassOf(typeof(RuleBase)))
                    .Where(type => type.GetCustomAttribute<CheckRuleDescription>() != null);
            foreach (var ruleType in ruleTypes)
            {
                var checkRuleDescription = ruleType.GetCustomAttribute<CheckRuleDescription>();
                if (checkRuleDescription == null)
                {
                    continue;
                }
                if (!sorteddicAllclassifyRules.ContainsKey(checkRuleDescription.classify))
                    sorteddicAllclassifyRules.Add(checkRuleDescription.classify, new SortedSet<string>());
                sorteddicAllclassifyRules[checkRuleDescription.classify].Add(ruleType.FullName);
                dicAllRulesClassify.Add(ruleType.FullName, checkRuleDescription.classify);
                dicRuleDescriptions.Add(ruleType.FullName, checkRuleDescription.description);
                dicDetailedDescription.Add(ruleType.FullName, checkRuleDescription.detailedDescription);
                dicAllRuleDefaultParamInfos.Add(ruleType.FullName, new List<ParamInfo>());
                dicAllRuleCheckAssetPaths.Add(ruleType.FullName, new List<CheckAssetWithParams>());
                dicRuleOpen.Add(ruleType.FullName, false);
                FieldInfo[] memberParamInfos = ruleType.GetFields();
                foreach (var memberParamInfo in memberParamInfos)
                {
                    PublicParam pp = memberParamInfo.GetCustomAttribute<PublicParam>();
                    if (pp == null)
                        continue;
                    ParamInfo paramInfo = new ParamInfo();
                    paramInfo.description = pp.description;
                    paramInfo.uiType = pp.guiType.ToString();
                    paramInfo.uiParam = pp.uiParam;
                    paramInfo.paramType = memberParamInfo.FieldType.ToString();
                    dicAllRuleDefaultParamInfos[ruleType.FullName].Add(paramInfo);
                }
            }
            classifyNames = sorteddicAllclassifyRules.Keys.ToArray();
            dicRulesFold = new Dictionary<string, bool>();
            GetCheckAssetPaths();
        }

        private void OnDisable()
        {
            AssetCheckMain.EventCheckRuleProcessChanged -= OnCheckRuleProcessChanged;
            AssetCheckMain.EventCheckRuleState -= OnCheckRuleState;
        }

        void RefreshTags()
        {
            FilePathTags.Clear();
            FilePathTags.Add("UnDefined");
            AssetCheckTags config = AssetDatabase.LoadAssetAtPath<AssetCheckTags>($"{Defines.CheckPathConfigPath}/{Defines.CheckTagsConfigName}");
            FilePathTags.AddRange(config.tags);
            FilePathTagsArray = FilePathTags.ToArray();
        }

        private void GetCheckAssetPaths()
        {
            AssetDatabase.DeleteAsset($"{Defines.CheckPathConfigPath}/{Defines.CheckPathConfigTempName}");
            AssetDatabase.CopyAsset($"{Defines.CheckPathConfigPath}/{Defines.CheckPathConfigName}", $"{Defines.CheckPathConfigPath}/{Defines.CheckPathConfigTempName}");
            AssetCheckPathConfig checkPathConfig = AssetDatabase.LoadAssetAtPath<AssetCheckPathConfig>($"{Defines.CheckPathConfigPath}/{Defines.CheckPathConfigTempName}");
            foreach (var ruleCheckAssetPath in checkPathConfig.ruleCheckAssetPaths)
            {
                if (!dicAllRuleDefaultParamInfos.ContainsKey(ruleCheckAssetPath.ruleName))
                {
                    continue;
                }
                if (!dicAllRuleCheckAssetPaths.ContainsKey(ruleCheckAssetPath.ruleName))
                    continue;
                // 检查参数跟默认参数是否相似，如果不相似就刷掉上面的参数
                foreach (var checkPath in ruleCheckAssetPath.content.checkPathsWithParams)
                {
                    int paramCount = checkPath.ruleParams.Count;
                    if (paramCount != dicAllRuleDefaultParamInfos[ruleCheckAssetPath.ruleName].Count)
                    {
                        // 如果参数数量不相同，代表参数修改过了，所以刷掉原来的数据
                        checkPath.ruleParams.Clear();
                        for (int i = 0; i < dicAllRuleDefaultParamInfos[ruleCheckAssetPath.ruleName].Count; i++)
                        {
                            checkPath.ruleParams.Add(dicAllRuleDefaultParamInfos[ruleCheckAssetPath.ruleName][i].Clone());
                        }
                    }
                    else
                    {
                        // 如果数据类型变化过了，肯定也修改过了
                        for (int i = 0; i < paramCount; i++)
                        {
                            if (!ParamInfo.Similarity(checkPath.ruleParams[i], dicAllRuleDefaultParamInfos[ruleCheckAssetPath.ruleName][i]))
                            {
                                checkPath.ruleParams[i] = dicAllRuleDefaultParamInfos[ruleCheckAssetPath.ruleName][i].Clone();
                            }
                        }
                    }
                }

                dicAllRuleCheckAssetPaths[ruleCheckAssetPath.ruleName].AddRange(ruleCheckAssetPath.content.checkPathsWithParams);
                if (dicRuleOpen.ContainsKey(ruleCheckAssetPath.ruleName))
                {
                    dicRuleOpen[ruleCheckAssetPath.ruleName] = ruleCheckAssetPath.isOpen;
                }
            }
        }

        private void SaveCheckAssetPaths()
        {
            AssetCheckPathConfig checkPathConfig = AssetDatabase.LoadAssetAtPath<AssetCheckPathConfig>($"{Defines.CheckPathConfigPath}/{Defines.CheckPathConfigTempName}");
            checkPathConfig.ruleCheckAssetPaths.Clear();
            foreach (var ruleCheckAssetPath in dicAllRuleCheckAssetPaths)
            {
                RuleCheckAssetPath rcap = new RuleCheckAssetPath();
                rcap.ruleName = ruleCheckAssetPath.Key;
                rcap.ruleClassify = dicAllRulesClassify[ruleCheckAssetPath.Key];
                rcap.isOpen = dicRuleOpen[ruleCheckAssetPath.Key];
                rcap.content.checkPathsWithParams = ruleCheckAssetPath.Value;
                checkPathConfig.ruleCheckAssetPaths.Add(rcap);
            }
            EditorUtility.SetDirty(checkPathConfig);
            AssetDatabase.SaveAssets();
            AssetDatabase.CopyAsset($"{Defines.CheckPathConfigPath}/{Defines.CheckPathConfigTempName}", $"{Defines.CheckPathConfigPath}/{Defines.CheckPathConfigName}");
        }

        private void OnGUI()
        {
            DrawMainWindow();
        }

        private void DrawTitle()
        {
            GUILayout.Space(3);
            GUILayout.Label($"{Defines.AssetCheckName} Setting {Defines.VersionId}", GUIDefines.TitleNameStyle, GUILayout.Height(25));
            GUILayout.Space(2);
        }

        private void DrawMainWindow()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                {
                    Rect leftArea = EditorGUILayout.GetControlRect(GUILayout.Width(150), GUILayout.Height(800));
                    GUI.BeginGroup(leftArea);
                    DrawLeftMenu();
                    Rect customRuleButtonRect = leftArea.GetPart(0, 1, 0.85f, 0.9f);
                    if (GUI.Button(customRuleButtonRect, "+自定义规则"))
                    {
                        AssetHelper.OpenFolder(Defines.SamplesFolder);
                    }
                    GUI.EndGroup();
                    GUILayout.Space(4);
                    DrawRightPart();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.FlexibleSpace();
            DrawBottomStripe();
            GUILayout.EndVertical();
        }

        Rect rectLeftMenu = new Rect(10, 0, 150, 48);
        // 左侧列表
        private void DrawLeftMenu()
        {
            rectLeftMenu.height = 48 * classifyNames.Length;
            int curSelect = currentClassifySelect;
            curSelect = GUI.SelectionGrid(rectLeftMenu, currentClassifySelect, classifyNames, 1);
            if (curSelect != currentClassifySelect)
            {
                currentClassifySelect = curSelect;
            }
        }

        private void DrawRightPart()
        {
            GUILayout.BeginVertical(GUILayout.Width(1045));
            DrawRightPartTitle();
            DrawAllRules();
            DrawSlider();
            GUILayout.EndVertical();
        }
        private void DrawRightPartTitle()
        {
            GUILayout.BeginVertical();
            {
                DrawRulesTitle();
            }
            GUILayout.EndVertical();
        }

        Rect rulesTitleRect = new Rect(0, 10, 300, 30);
        Rect ruleFindFieldRect = new Rect(584, 10, 280, 30);
        Rect ruleOpenRect = new Rect(880, 10, 74, 30);
        Rect ruleCloseRect = new Rect(965, 10, 74, 30);
        private void DrawRulesTitle()
        {
            Rect rulesTitleArea = EditorGUILayout.GetControlRect(
                GUILayout.Width(1046), GUILayout.Height(48));
            GUI.BeginGroup(rulesTitleArea);
            GUI.Label(rulesTitleRect, $"{classifyNames[currentClassifySelect]}规则列表", GUIDefines.TitleRuleStyle);
            currentRuleFind = GUI.TextField(ruleFindFieldRect, currentRuleFind);
            if (currentRuleFind.Equals(string.Empty))
            {
                GUI.Label(ruleFindFieldRect, "请输入规则名称", GUIDefines.DisableText);
            }
            if (GUI.Button(ruleOpenRect, "全部启用"))
            {
                CurrentClassifyRulesOpen();
            }
            if (GUI.Button(ruleCloseRect, "全部禁用"))
            {
                CurrentClassifyRulesClose();
            }
            GUI.EndGroup();
        }

        void CurrentClassifyRulesOpen()
        {
            string selectClassify = classifyNames[currentClassifySelect];
            var selectRules = sorteddicAllclassifyRules[selectClassify];
            foreach (var ruleName in selectRules)
            {
                //if (currentRuleFind != string.Empty)
                //{
                //    if (!ruleName.Contains(currentRuleFind) && !dicRuleDescriptions[ruleName].Contains(currentRuleFind))
                //        continue;
                //}
                if (!dicRuleOpen.ContainsKey(ruleName))
                    continue;
                dicRuleOpen[ruleName] = true;
            }
        }

        void CurrentClassifyRulesClose()
        {
            string selectClassify = classifyNames[currentClassifySelect];
            var selectRules = sorteddicAllclassifyRules[selectClassify];
            foreach (var ruleName in selectRules)
            {
                if (!dicRuleOpen.ContainsKey(ruleName))
                    continue;
                dicRuleOpen[ruleName] = false;
            }
        }

        Vector2 ruleScrollPosition = Vector2.zero;

        private void DrawAllRules()
        {
            ruleScrollPosition = GUILayout.BeginScrollView(ruleScrollPosition, GUILayout.Width(1080), GUILayout.Height(windowHeight - 150));
            string selectClassify = classifyNames[currentClassifySelect];
            GUILayout.BeginVertical();
            foreach (var rule in sorteddicAllclassifyRules[selectClassify])
            {
                if (currentRuleFind != string.Empty)
                {
                    if (!rule.Contains(currentRuleFind) && !dicRuleDescriptions[rule].Contains(currentRuleFind))
                        continue;
                }
                bool bFold = true;
                if (dicRulesFold.ContainsKey(rule))
                    bFold = false;
                Rect rectLineTop = GUILayoutUtility.GetRect(900, 1);
                EditorGUI.DrawRect(rectLineTop, Color.black);
                GUILayout.BeginHorizontal();
                Rect foldOutRect = GUILayoutUtility.GetRect(850, 40);
                Rect togglePosition = GUILayoutUtility.GetRect(40, 40);
                bool b = GUIEx.AssetCheckBeginFoldoutHeaderGroup(foldOutRect, bFold, dicRuleDescriptions[rule], GUIDefines.FoldoutHeaderStyle);
                bool bOpen = GUIEx.AssetCheckToggle(togglePosition.GetCenterRect(36, 20), dicRuleOpen[rule]);
                if (bOpen != dicRuleOpen[rule])
                {
                    dicRuleOpen[rule] = bOpen;
                }
                GUILayout.EndHorizontal();
                Rect rectLineBottom = GUILayoutUtility.GetRect(1000, 1);
                EditorGUI.DrawRect(rectLineBottom, Color.black);
                GUILayout.Space(8);
                if (b != bFold)
                {
                    bFold = b;
                    if (bFold)
                    {
                        if (dicRulesFold.ContainsKey(rule))
                            dicRulesFold.Remove(rule);
                    }
                    else
                    {
                        dicRulesFold.Add(rule, true);
                    }
                }
                if (bFold)
                {
                    GUILayout.Label($"规则说明:{dicDetailedDescription[rule]}");
                    List<CheckAssetWithParams> checkPaths = dicAllRuleCheckAssetPaths[rule];
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("检查项路径", GUILayout.Width(80));

                    if (GUILayout.Button(GUIDefines.ContentAdd, GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        var checkAssetWithParams = new CheckAssetWithParams();
                        checkAssetWithParams.assetPath = string.Empty;
                        checkAssetWithParams.tags = string.Empty;
                        checkAssetWithParams.ruleParams = new List<ParamInfo>();
                        for (int i = 0; i < dicAllRuleDefaultParamInfos[rule].Count; i++)
                        {
                            checkAssetWithParams.ruleParams.Add(dicAllRuleDefaultParamInfos[rule][i].Clone());
                        }
                        checkPaths.Add(checkAssetWithParams);

                    }
                    GUILayout.EndHorizontal();
                    int nCheckPathIndex = 0;
                    int nRemoveIndex = -1;
                    foreach (var checkPath in checkPaths)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"检查路径{nCheckPathIndex + 1}", GUILayout.Width(100));
                        if (GUILayout.Button(GUIDefines.ContentTrash, GUILayout.Width(26), GUILayout.Height(26)))
                        {
                            nRemoveIndex = nCheckPathIndex;
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(10);
                        Rect getRect = GUILayoutUtility.GetRect(982, 26);
                        Rect rectOperateTextAssetPath = getRect.GetLeftPart(700);
                        string operateTextAssetPath = GUIEx.AssetCheckEditorAndDragPathTextField(rectOperateTextAssetPath, checkPath.assetPath);
                        if (!operateTextAssetPath.Equals(checkPath.assetPath))
                        {
                            checkPath.assetPath = operateTextAssetPath;
                        }
                        int nSelect = FilePathTags.IndexOf(checkPath.tags);
                        if(nSelect < 0)
                        {
                            checkPath.tags = string.Empty;
                            nSelect = 0;
                        }
                        bool bChange = GUIEx.Popup(getRect.GetHorizontalPart(720, 900), ref nSelect, FilePathTagsArray);
                        if(bChange)
                        {
                            checkPath.tags = FilePathTagsArray[nSelect];
                        }
                        if(GUI.Button(getRect.GetHorizontalPart(910, 930), GUIDefines.ContentAdd))
                        {
                            AssetCheckTagsUI.ShowWindow();
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginVertical();
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("文件必须包含的字符串(如果为空则没有限制)", GUILayout.Width(260));
                        checkPath.includeKeyword = GUILayout.TextField(checkPath.includeKeyword, GUILayout.Width(200));
                        //if(GUILayout.Button(GUIDefines.ContentAdd, GUILayout.Width(20), GUILayout.Height(20)))
                        //{
                        //    checkPath.includeKeyword.Add("");
                        //}
                        //int removeIncludeIndex = -1;
                        //for(int i = 0; i < checkPath.includeKeyword.Count; i++)
                        //{
                        //    GUILayout.BeginHorizontal();
                        //    GUILayout.Space(10);
                        //    Rect excludePathRect = GUILayoutUtility.GetRect(958, 26);
                        //    Rect[] splitRects = excludePathRect.SplitTwoHorizontalPercent(0.9f);
                        //    string operateTextExcludePath = GUIEx.AssetCheckEditorAndDragPathTextField(splitRects[0].GetLeftPart(700), checkPath.includeKeyword[i]);
                        //    if (!operateTextExcludePath.Equals(checkPath.includeKeyword[i]))
                        //    {
                        //        checkPath.includeKeyword[i] = operateTextExcludePath;
                        //    }
                        //    Rect removeIncludeBtnRect = splitRects[1].GetCenterRect(26, 26);
                        //    if (GUI.Button(removeIncludeBtnRect, GUIDefines.ContentTrash))
                        //    {
                        //        removeIncludeIndex = i;
                        //    }
                        //    GUILayout.EndHorizontal();
                        //}
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("资源白名单路径", GUILayout.Width(100));
                        if (GUILayout.Button(GUIDefines.ContentAdd, GUILayout.Width(20), GUILayout.Height(20)))
                        {
                            checkPath.excludePaths.Add("");
                        }
                        GUILayout.EndHorizontal();
                        int removeExcludeIndex = -1;
                        for (int i = 0; i < checkPath.excludePaths.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(10);
                            Rect excludePathRect = GUILayoutUtility.GetRect(958, 26);
                            Rect[] splitRects = excludePathRect.SplitTwoHorizontalPercent(0.9f);
                            string operateTextExcludePath = GUIEx.AssetCheckEditorAndDragPathTextField(splitRects[0].GetLeftPart(700), checkPath.excludePaths[i]);
                            if (!operateTextExcludePath.Equals(checkPath.excludePaths[i]))
                            {
                                checkPath.excludePaths[i] = operateTextExcludePath;
                            }
                            Rect removeExcludeBtnRect = splitRects[1].GetCenterRect(26, 26);
                            if (GUI.Button(removeExcludeBtnRect, GUIDefines.ContentTrash))
                            {
                                removeExcludeIndex = i;
                            }
                            GUILayout.EndHorizontal();
                        }
                        if (removeExcludeIndex >= 0)
                        {
                            checkPath.excludePaths.RemoveAt(removeExcludeIndex);
                        }
                        DrawParams(ref checkPath.ruleParams);
                        nCheckPathIndex++;
                        GUILayout.EndVertical();
                    }
                    if (nRemoveIndex >= 0)
                    {
                        checkPaths.RemoveAt(nRemoveIndex);
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void DrawParams(ref List<ParamInfo> ruleParams)
        {
            GUILayout.BeginVertical();

            if (ruleParams.Count > 0)
            {
                GUILayout.Space(10);
                GUILayout.Label("参数");
            }

            GUILayout.BeginHorizontal();
            {
                if (ruleParams.Count != 0)
                {
                    for (int i = 0; i < ruleParams.Count; i++)
                    {
                        int width = GUIhelper.GetGUIStringWidth($"{ruleParams[i].description}:");
                        GUILayout.Label($"{ruleParams[i].description}:", GUILayout.Width(width + 4));
                        eGUIType uiType = (eGUIType)Enum.Parse(typeof(eGUIType), ruleParams[i].uiType);
                        if (uiType == eGUIType.Input)
                        {
                            if (ruleParams[i].paramType.Equals(typeof(float).FullName))
                            {
                                if (!float.TryParse(ruleParams[i].paramValue, out float floatValue))
                                {
                                    floatValue = 0;
                                }
                                float newFloatValue = EditorGUILayout.FloatField(floatValue, GUILayout.Width(100));
                                if (floatValue != newFloatValue || ruleParams[i].paramValue.Equals(string.Empty))
                                {
                                    ruleParams[i].paramValue = newFloatValue.ToString();
                                }
                            }
                            else if (ruleParams[i].paramType.Equals(typeof(int).FullName))
                            {
                                if (!int.TryParse(ruleParams[i].paramValue, out int intValue))
                                {
                                    intValue = 0;
                                }
                                int newIntValue = EditorGUILayout.IntField(intValue, GUILayout.Width(100));
                                if (intValue != newIntValue || ruleParams[i].paramValue.Equals(string.Empty))
                                {
                                    ruleParams[i].paramValue = newIntValue.ToString();
                                }
                            }
                            else if (ruleParams[i].paramType.Equals(typeof(long).FullName))
                            {
                                if (!long.TryParse(ruleParams[i].paramValue, out long longValue))
                                {
                                    longValue = 0;
                                }
                                long newLongValue = EditorGUILayout.LongField(longValue, GUILayout.Width(100));
                                if (longValue != newLongValue || ruleParams[i].paramValue.Equals(string.Empty))
                                {
                                    ruleParams[i].paramValue = newLongValue.ToString();
                                }
                            }
                            else
                            {
                                string paramValue = EditorGUILayout.TextField(ruleParams[i].paramValue, GUILayout.Width(200));
                                if (!paramValue.Equals(ruleParams[i].paramValue) || ruleParams[i].paramValue.Equals(string.Empty))
                                {
                                    ruleParams[i].paramValue = paramValue;
                                }
                            }
                        }
                        else if (uiType == eGUIType.Enum)
                        {
                            Type enumType = GetEnumType(ruleParams[i].paramType);
                            object eValue;
                            if (ruleParams[i].paramValue == string.Empty)
                            {
                                eValue = Enum.ToObject(enumType, 0);
                            }
                            else
                            {
                                eValue = Enum.Parse(enumType, ruleParams[i].paramValue);
                            }
                            Enum newEValue = EditorGUILayout.EnumPopup((Enum)eValue, GUILayout.Width(GUIhelper.GetGUIStringWidth(eValue.ToString()) + 50));
                            if (newEValue != eValue)
                            {
                                ruleParams[i].paramValue = newEValue.ToString();
                            }
                        }
                        else if (uiType == eGUIType.Bool)
                        {
                            bool bValue = false;
                            bool.TryParse(ruleParams[i].paramValue, out bValue);
                            bool bNewValue = EditorGUILayout.Toggle(bValue);
                            if (bNewValue != bValue)
                            {
                                ruleParams[i].paramValue = bNewValue.ToString();
                            }
                        }
                        else if (uiType == eGUIType.StringMaskField)
                        {
                            string[] displayedOptions = AssetHelper.StringToMultiple(ruleParams[i].uiParam);
                            int mask = -1;
                            if (!ruleParams[i].paramValue.Equals(string.Empty))
                            {
                                if (!int.TryParse(ruleParams[i].paramValue, out mask))
                                {
                                    Debug.LogError($"Can not parse maskValue {ruleParams[i].paramValue} to int");
                                    continue;
                                }
                            }
                            int StringsMaxWidth = GUIhelper.GetGUIStringsMaxWidth(displayedOptions);
                            int newMask = EditorGUILayout.MaskField(mask, displayedOptions, GUILayout.Width(StringsMaxWidth + 50));
                            if (newMask != mask)
                            {
                                ruleParams[i].paramValue = newMask.ToString();
                            }
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        void OnCheckRuleProcessChanged(int index, int total)
        {
            if (total == 0)
                currentCheckPer = 1.0f;
            else
                currentCheckPer = (float)index / (float)total;
            Repaint();
        }
        void OnCheckRuleState(bool state)
        {
            bCheckState = state;
            if (!bCheckState)
            {
                currentCheckPer = 0.0f;
                Repaint();
            }
        }
        void DrawSlider()
        {
            Rect rect = GUILayoutUtility.GetRect(800, 18);
            EditorGUI.ProgressBar(rect, currentCheckPer, "");
        }

        private Type GetEnumType(string enumName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(enumName);
                if (type == null)
                    continue;
                if (type.IsEnum)
                    return type;
            }
            return null;
        }

        Rect bottomStripeRect = new Rect();
        private async void DrawBottomStripe()
        {
            if (UnityEditor.EditorApplication.isPlaying)
                return;
            bottomStripeRect.Set(0, windowHeight - 50.0f, windowWidth, 50.0f);
            GUILayout.BeginArea(bottomStripeRect);
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();

                    if (bCheckState)
                    {
                        if (GUILayout.Button("取消", GUIDefines.ButtonOrangeStyle))
                        {
                            AssetCheckMain.ForceEnd();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("本地检查", GUIDefines.ButtonOrangeStyle))
                        {
                            SaveCheckAssetPaths();
                            AssetCheckPathConfig checkPathConfig = AssetDatabase.LoadAssetAtPath<AssetCheckPathConfig>($"{Defines.CheckPathConfigPath}/{Defines.CheckPathConfigName}");
                            AssetCheckMain.CSVEnable(true);
                            AssetCheckMain.CheckWithConfig(checkPathConfig);
                        }
                    }

                    GUILayout.Space(10);
                    if (GUILayout.Button("保存到本地", GUIDefines.ButtonOrangeStyle))
                    {
                        SaveCheckAssetPaths();
                    }
                    GUILayout.Space(10);
                    if (GUILayout.Button("提交并覆盖", GUIDefines.ButtonOrangeStyle))
                    {
                        await CommitAndOverwrite();
                        return;
                    }
                    GUILayout.Space(10);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
        }

        async Task CommitAndOverwrite()
        {
            SaveCheckAssetPaths();
            LoginTokenInfo loginTokenInfo = await Login.GetTokenInfo();
            if (loginTokenInfo != null)
                RuleSyncGUI.ShowWindow();
        }
    }
}


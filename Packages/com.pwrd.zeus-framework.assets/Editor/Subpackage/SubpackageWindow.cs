/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Zeus.Framework.Asset
{
    public class GenerateSubpakcageUrlListFileWindow : EditorWindow
    {
        private List<string> _assetSettingUrlList;

        private void OnEnable()
        {
            _assetSettingUrlList = new List<string>(AssetManagerSetting.LoadSetting().bundleLoaderSetting.remoteURL);
        }

        private void OnGUI()
        {
            SubpackageWindow.Setting.SubpackageUrlListOutputPath = EditorGUILayout.TextField("Output Path", SubpackageWindow.Setting.SubpackageUrlListOutputPath);
            if (GUILayout.Button("Browse", EditorStyles.miniButton, GUILayout.Width(55)))
            {
                string temp = EditorUtility.OpenFolderPanel("Output Folder", SubpackageWindow.Setting.SubpackageUrlListOutputPath, string.Empty);
                if (!string.IsNullOrEmpty(temp))
                {
                    SubpackageWindow.Setting.SubpackageUrlListOutputPath = temp;
                }
            }
            EditorGUILayout.LabelField("Subpackage Server URL:");
            EditorGUI.BeginDisabledGroup(true);
            for (int i = 0; i < _assetSettingUrlList.Count; i++)
            {
                string url = _assetSettingUrlList[i];
                if (url == null) continue;
                EditorGUILayout.TextField("URL", _assetSettingUrlList[i]);
            }
            EditorGUI.EndDisabledGroup();
            for (int i = 0; i < SubpackageWindow.Setting.UrlList.Count; i++)
            {
                string url = SubpackageWindow.Setting.UrlList[i];
                if (url == null) continue;
                EditorGUILayout.BeginHorizontal();
                SubpackageWindow.Setting.UrlList[i] = EditorGUILayout.TextField("URL", SubpackageWindow.Setting.UrlList[i]);
                if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    SubpackageWindow.Setting.UrlList[i] = null;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Separator();
            if (GUILayout.Button("Add URL"))
            {
                string url = "";
                SubpackageWindow.Setting.UrlList.Add(url);
            }
            if (GUILayout.Button("生成Subpackage文件的URL列表文件"))
            {
                HashSet<string> tempSet = new HashSet<string>();
                foreach (var str in SubpackageWindow.Setting.UrlList)
                {
                    tempSet.Add(str);
                }
                foreach (var str in _assetSettingUrlList)
                {
                    tempSet.Add(str);
                }
                new SubpackageEditorUtil().GenerateURLListOfSubpackage(SubpackageWindow.Setting.SubpackageUrlListOutputPath, new List<string>(tempSet));
            }
            EditorGUILayout.Separator();
            if (GUILayout.Button("Save Setting"))
            {
                SubpackageWindow.Setting.Save();
            }
            EditorGUILayout.Separator();
        }
    }

    public class SubpackageWindow : EditorWindow
    {
        public const int IS_OBSERVING_INT = 1;
        public const int IS_NOT_OBSERVING_INT = 0;
        public const string OBSERVING_STORE_KEY = "IsObserving";

        public static SubpackageSetting Setting;
        public static TagAsset TagAsset;
        private bool _isObserving = false;
        private SubpackageMode _mode;

        [MenuItem("Zeus/Asset/分包工具", false, 2)]
        private static void Open()
        {
            SubpackageWindow subpackageWindow = GetWindowWithRect<SubpackageWindow>(new Rect(150, 35, 550, 700), false, "分包工具");
            TagDetailWindow tagDetailWindow = GetWindow<TagDetailWindow>("分包标签内容详情", typeof(SubpackageWindow));
            CdnSettingWindow cdnWindow = GetWindow<CdnSettingWindow>("上传工具", typeof(TagDetailWindow));
            GenerateSubpakcageUrlListFileWindow urlFileWindow = GetWindow<GenerateSubpakcageUrlListFileWindow>("二包文件Url列表生成", typeof(CdnSettingWindow));
            subpackageWindow.Show();
            subpackageWindow.Focus();
        }
        
        private void Reset()
        {
            _isObserving = false;
            EditorPrefs.SetInt(OBSERVING_STORE_KEY, IS_NOT_OBSERVING_INT);
        }

        private void OnEnable()
        {
            _isObserving = EditorPrefs.GetInt(OBSERVING_STORE_KEY, IS_NOT_OBSERVING_INT) == IS_OBSERVING_INT ? true : false;
            Setting = SubpackageSetting.LoadSetting();
            TagAsset = TagAsset.LoadTagAsset();
            if (Setting.currentTag >= TagAsset.TagListForRecord.Count)
            {
                Setting.currentTag = TagAsset.TagListForRecord.Count - 1;
            }
            _mode = AssetManagerSetting.LoadSetting().bundleLoaderSetting.mode;
        }

        Vector2 vec = Vector2.zero;

        private void OnGUI()
        {
            vec = GUILayout.BeginScrollView(vec);
            GUILayout.BeginHorizontal();
            Setting.assetListOutputPath = EditorGUILayout.TextField("AssetList Output Path", Setting.assetListOutputPath);
            if (GUILayout.Button("Browse", EditorStyles.miniButton, GUILayout.Width(55)))
            {
                string temp = EditorUtility.OpenFolderPanel("Output Folder", Setting.assetListOutputPath, string.Empty);
                if (!string.IsNullOrEmpty(temp))
                {
                    if (!temp.Replace("\\", "/").Contains(Directory.GetCurrentDirectory().Replace("\\", "/") + "/"))
                    {
                        EditorUtility.DisplayDialog("Error", string.Format("Please choose a path under workpath \"{0}\"", Directory.GetCurrentDirectory()), "Ok");
                    }
                    else
                    {
                        Setting.assetListOutputPath = temp.Replace("\\", "/").Replace(Directory.GetCurrentDirectory().Replace("\\", "/") + "/", "");
                    }
                }
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();
            var tempTag = EditorGUILayout.Popup("Current AssetList Tag", Setting.currentTag, TagAsset.TagListForRecord.ToArray());

            if (Setting.currentTag != tempTag)
            {
                if (_isObserving)
                    EditorUtility.DisplayDialog("Error", "切换到其他Tag前请先停止当前Tag的记录", "Ok");
                else
                    Setting.currentTag = tempTag;
            }

            if (!_isObserving)
            {
                if (GUILayout.Button("开始记录资源"))
                {
                    _isObserving = true;
                    EditorPrefs.SetInt(OBSERVING_STORE_KEY, IS_OBSERVING_INT);
                    AssetObserver.Start();
                }
            }
            else
            {
                if (GUILayout.Button("停止记录资源"))
                {
                    _isObserving = false;
                    EditorPrefs.SetInt(OBSERVING_STORE_KEY, IS_NOT_OBSERVING_INT);
                    AssetObserver.Stop();
                    AssetObserver.SaveObserver(TagAsset.TagListForRecord[Setting.currentTag]);
                }
            }
            if (GUILayout.Button("保存Asset Log"))
            {
                AssetObserver.SaveObserver(TagAsset.TagListForRecord[Setting.currentTag]);
            }
            EditorGUILayout.Separator();
            if (GUILayout.Button("Save Setting"))
            {
                if (TagAsset.TagListForRecord.Contains(AssetBundleLoaderSetting.OthersTag))
                {
                    EditorUtility.DisplayDialog("Warning", $"Please don't use tag named \"{AssetBundleLoaderSetting.OthersTag}\"", "OK");
                    TagAsset.TagListForRecord.Remove(AssetBundleLoaderSetting.OthersTag);
                }
                Setting.Save();
            }
            GUILayout.EndScrollView();
        }
    }

    public class TagDetailWindow : EditorWindow
    {
        int _currentTagIndex;
        List<string> _assetList = new List<string>();
        List<int> _filterIndex = new List<int>();
        Vector2 _vec = Vector2.zero;
        public const string STREAMING_ASSET_PATH = "StreamingAssets/";
        string filter = "";
        private bool isDirty = true;
        private bool removeFileExtension;
        private int currentPage = 1, maxPage = 1;
        private string jumpTo = "";
        private bool checkAll = true, checkTag, checkFilter;
        private byte currentCheck = 1;
        AddAssetToTagWindow addAssetToTagWindow;
        private GUIStyle normal = null;
        private int viewMood = 0;

        private void OnEnable()
        {
            AssetListHelper.Refresh();
            _currentTagIndex = 0;
            if (TagAsset.LoadTagAsset().TagListForRecord == null || TagAsset.LoadTagAsset().TagListForRecord.Count>0)
            {
                SetAssetList(TagAsset.LoadTagAsset().TagListForRecord[0]);
            }
            removeFileExtension = AssetManagerSetting.LoadSetting().removeFileExtension;
            addAssetToTagWindow = CreateInstance<AddAssetToTagWindow>();
        }

        private void SetStyle()
        {
            if (normal != null)
            {
                return;
            }
            normal = new GUIStyle(GUI.skin.FindStyle("TextField"));
            var texture1 = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            texture1.SetPixel(0, 0, Color.gray);
            texture1.wrapMode = TextureWrapMode.Repeat;
            texture1.Apply();
            // normal.normal.background = texture1;
        }

        private void Refresh()
        {
            AssetListHelper.Refresh();
            SetAssetList(SubpackageWindow.TagAsset.TagListForRecord[_currentTagIndex]);
            filter = "";
            currentPage = 1;
            _vec.y = 0;
            isDirty = true;
        }

        private void OnGUI()
        {
            SetStyle();
            var tagListWithOthers = SubpackageWindow.TagAsset.TagListForRecord;
            tagListWithOthers.Add(AssetBundleLoaderSetting.OthersTag);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Current Tag :",GUILayout.Width(80));
            int tempTag = EditorGUILayout.Popup(_currentTagIndex, tagListWithOthers.ToArray());
            EditorGUILayout.EndHorizontal();

            if (tempTag != _currentTagIndex)
            {
                if (IsTagChanged())
                {
                    int option = EditorUtility.DisplayDialogComplex("警告",
                              "是否保存当前Tag的数据",
                              "保存",
                              "不保存",
                              "取消");

                    switch (option)
                    {
                        case 0:
                            SaveCurrentTag();
                            ChangeTag(tempTag, tagListWithOthers);
                            break;
                        case 1:
                            ChangeTag(tempTag, tagListWithOthers);
                            break;
                    }
                }
                else
                {
                    ChangeTag(tempTag, tagListWithOthers);
                }
            }
            if (isDirty)
            {
                RefreshFilterIndex();
                isDirty = false;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Assets Filter :",GUILayout.Width(80));
            filter = EditorGUILayout.TextField(filter);
            if (GUILayout.Button("Filter",GUILayout.Width(100)))
            {
                isDirty = true;
                _vec.y = 0;
                currentPage = 1;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("选择模式 :",GUILayout.Width(80));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("浏览",GUILayout.Width(152)))
            {
                viewMood = 0;
            }
            GUI.enabled = tagListWithOthers[tempTag] != AssetBundleLoaderSetting.OthersTag;
            if (GUILayout.Button("调序",GUILayout.Width(152)))
            {
                viewMood = 1;
            }
            if (GUILayout.Button("编辑",GUILayout.Width(152)))
            {
                viewMood = 2;
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            if (_assetList.Count == 0)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (tagListWithOthers.IndexOf(AssetBundleLoaderSetting.OthersTag) == _currentTagIndex)
                {
                    EditorGUILayout.HelpBox("无法预览Other资源，需要先打Bundle", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox("该Tag没有录入资源", MessageType.Info);
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
            }
            else
            {
                _vec = GUILayout.BeginScrollView(_vec);
                for (int i = (currentPage - 1) * 100; i < currentPage * 100; ++i)
                {
                    try
                    {
                        if (_assetList[_filterIndex[i]] == null)
                        {
                            continue;
                        }
                    }
                    catch (Exception)
                    {
                        break;
                    }
                    EditorGUILayout.BeginHorizontal();
                    switch (viewMood)
                    {
                        case 0:
                            if (GUILayout.Button(_assetList[_filterIndex[i]], normal))
                            {
                                //TODO
                            }
                            break;
                        case 1:
                            GUILayout.Label(_assetList[_filterIndex[i]], normal);
                            if (GUILayout.Button(new GUIContent("▲", "全局置顶"), GUILayout.Width(22)))
                            {
                                for (int j = _filterIndex[i]; j > 0; j--)
                                {
                                    SwapListItem(_assetList, j, j-1);
                                }
                                isDirty = true;
                            }
                            if (GUILayout.Button(new GUIContent("∧", "在筛选结果中置顶"), GUILayout.Width(19)))
                            {
                                for (int j = i; j > 0; j--)
                                {
                                    SwapListItem(_assetList, _filterIndex[j], _filterIndex[j-1]);
                                }
                            }
                            if (GUILayout.Button(new GUIContent("↑", "上移"), GUILayout.Width(19)) && i >= 1)
                            {
                                SwapListItem(_assetList, _filterIndex[i], _filterIndex[i - 1]);
                            }
                            if (GUILayout.Button(new GUIContent("↓", "下移"), GUILayout.Width(19)) && _filterIndex.Count > i + 1)
                            {
                                SwapListItem(_assetList, _filterIndex[i], _filterIndex[i + 1]);
                            }
                            break;
                        default:
                    string temp = EditorGUILayout.TextField(_assetList[_filterIndex[i]]).Replace("\\", "/");
                    if (_assetList[_filterIndex[i]] != temp)
                    {
                        if (removeFileExtension && !temp.StartsWith(AssetListHelper.ExtraPrefix))
                        {
                            string extension = Path.GetExtension(temp);
                            if (!string.IsNullOrEmpty(extension))
                            {
                                temp = temp.Replace(extension, "");
                            }
                        }
                        _assetList[_filterIndex[i]] = temp;
                    }
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        _assetList[_filterIndex[i]] = null;
                    }
                            break;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }


            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("刷新列表(会丢失未保存信息)")))
            {
                Refresh();
            }
            EditorGUILayout.LabelField("", GUILayout.Width(80));
            if (GUILayout.Button("上一页") && currentPage > 1)
            {
                currentPage--;
                _vec.y = 0;
                isDirty = true;
            }
            EditorGUILayout.LabelField(currentPage + " / " + maxPage,GUILayout.Width(46));
            if (GUILayout.Button("下一页") && currentPage < maxPage)
            {
                currentPage++;
                _vec.y = 0;
                isDirty = true;
            }
            
            jumpTo = EditorGUILayout.TextField(jumpTo);
            if (GUILayout.Button("  Go  ") && jumpTo != "")
            {
                var jumpToNum = Convert.ToInt32(jumpTo);
                if (jumpToNum > 0 && jumpToNum <= maxPage)
                {
                    currentPage = jumpToNum;
                }
                isDirty = true;
            }
            EditorGUILayout.EndHorizontal();
            
            GUI.enabled = tagListWithOthers[tempTag] != AssetBundleLoaderSetting.OthersTag && viewMood != 0 && viewMood != 1;
            if (GUILayout.Button("手动添加"))
            {
                _assetList.Add(string.Empty);
                isDirty = true;
                currentPage = maxPage;
                filter = "";
                _vec.y = 999999;
            }

            GUI.enabled = tagListWithOthers[tempTag] != AssetBundleLoaderSetting.OthersTag;
            if (GUILayout.Button(new GUIContent("将选中的资源添加到当前Tag", "在Project视图中选择的资源文件或者文件夹，点击此按钮可添加为当前Tag的分包资源，需手动保存")))
            {
                CreateAssetLogFileWithSelections();
                isDirty = true;
                currentPage = maxPage;
                filter = "";
                _vec.y = 999999;
            }
            
            if (GUILayout.Button(new GUIContent("读取资源添加到当前Tag")))
            {
                var rect = new Rect(position.position, new Vector2(position.size.x, position.size.y*0.4f));
                addAssetToTagWindow = GetWindowWithRect<AddAssetToTagWindow>(rect, false, "读取资源添加到当前Tag");
                addAssetToTagWindow.parent = this;
                addAssetToTagWindow.Show();
                addAssetToTagWindow.Focus();
            }
            if (addAssetToTagWindow.isReady)
            {
                _assetList.AddRange(addAssetToTagWindow.assetList);
                addAssetToTagWindow.Close();
                addAssetToTagWindow.isReady = false;
            }
            GUI.enabled = true;

            if (GUILayout.Button("保存当前信息") && tagListWithOthers[_currentTagIndex] != AssetBundleLoaderSetting.OthersTag)
            {
                SaveCurrentTag();
            }
            EditorGUILayout.BeginHorizontal();
            if (checkAll != (checkAll = EditorGUILayout.ToggleLeft("全部Tag",checkAll,GUILayout.Width(80))))
            {
                currentCheck = 1;
                checkTag = checkFilter = false;
            }
            else if (checkTag != (checkTag = EditorGUILayout.ToggleLeft("当前Tag", checkTag, GUILayout.Width(80))))
            {
                currentCheck = 2;
                checkAll = checkFilter = false;
            }
            else if (checkFilter != (checkFilter = EditorGUILayout.ToggleLeft("当前筛选", checkFilter, GUILayout.Width(80))))
            {
                currentCheck = 3;
                checkAll = checkTag = false;
            }
            if (GUILayout.Button("计算资源大小"))
            {
                switch (currentCheck)
                {
                    case 1:
                        PrintTagSize();
                        break;
                    case 2:
                        var tags = new List<string>();
                        tags.Add(tagListWithOthers[_currentTagIndex]);
                        PrintTagSize(tags);
                        break;
                    case 3:
                        PrintCurrentAssetSize();
                        break;
                }
            }
            if (GUILayout.Button("更新分包数据"))
            {
                AssetBuildProcessor.CreateSubpackage(Path.GetFullPath("Temp/subpackage"), EditorUserBuildSettings.activeBuildTarget);
            }
            if (GUILayout.Button("导出资源依赖"))
            {
                PrintAssetDependence(tagListWithOthers[_currentTagIndex]);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private void ChangeTag(int newTag, List<string> tagList)
        {
            _currentTagIndex = newTag;
            SetAssetList(tagList[_currentTagIndex]);
            filter = "";
            isDirty = true;
        }

        private bool SwapListItem(List<string> list, int a, int b)
        {
            if (list.Count <= a || list.Count <= b)
            {
                return false;
            }
            (list[a], list[b]) = (list[b], list[a]);
            return true;
        }

        private bool IsTagChanged()
        {
            var tagList = SubpackageWindow.TagAsset.TagListForRecord;
            if (_currentTagIndex >= tagList.Count)
                return false;

            var tempList = AssetListHelper.LoadAssetListFromFiles(new string[1] { tagList[_currentTagIndex] }, false);
            bool isChanged = !new HashSet<string>(_assetList).SetEquals(tempList);
            return isChanged;
        }

        private void SaveCurrentTag()
        {
            _assetList.RemoveAll((string asset) => { return string.IsNullOrEmpty(asset); });

            RelatedAssetHelper.AddRelatedAsset(ref _assetList);

            try
            {
                AssetListHelper.SaveAssetList(_assetList, SubpackageWindow.TagAsset.TagListForRecord[_currentTagIndex], true);
            }
            catch (System.IndexOutOfRangeException e)
            {
                Debug.LogError(e.ToString() + " Please rechoose tag in \"Zeus->Asset->分包\".");
            }
            RefreshFilterIndex();
        }

        // private List<string> CombineLists(List<string>[] lists)
        // {
        //     HashSet<string> resultSet = new HashSet<string>(lists[0]);
        //     List<string> resultList = new List<string>(lists[0]);
        //     if (lists.Length == 1)
        //         return resultList;
        //     for (int i = 1; i < lists.Length; i++)
        //     {
        //         foreach (var asset in lists[i])
        //         {
        //             if (resultSet.Add(asset))
        //             {
        //                 resultList.Add(asset);
        //             }
        //         }
        //     }
        //     return resultList;
        // }

        private void SetAssetList(string tag)
        {
            try
            {
                _assetList.Clear();
                _assetList = AssetListHelper.LoadAssetListFromFiles(new string[1] { tag }, false);
                SimplifyAssetList(ref _assetList);
            }
            catch (Exception)
            {
                Debug.Log("Tag : "+tag+" 不存在,请确认AssetList路径");
            }
        }
        
        private void RefreshFilterIndex()
        {
            _filterIndex.Clear();
            var regex = new Regex("");
            try
            {
                regex = new Regex(filter, RegexOptions.IgnoreCase);
            }
            catch (ArgumentException)
            {
                Debug.LogError("Filter正则表达式格式错误!");
            }
            
            for (int i = 0; i < _assetList.Count; i++)
            {
                if (_assetList[i] != null && regex.IsMatch(_assetList[i]))
                {
                    _filterIndex.Add(i);
                }
            }
            maxPage = _filterIndex.Count / 100 + 1;
        }

        private void SimplifyAssetList(ref List<string> assetList)
        {
            List<string> tempAssetList = new List<string>();
            HashSet<string> assetSet = new HashSet<string>();
            foreach (string asset in assetList)
            {
                if (assetSet.Add(asset))
                {
                    tempAssetList.Add(asset);
                }
            }
            assetList = tempAssetList;
        }

        enum AssetLocation
        {
            StreamingAssets,
            ResourcesDirectory,
            Others
        }
        private void CreateAssetLogFileWithSelections()
        {
            AssetManagerSetting assetManagerSetting = AssetManagerSetting.LoadSetting();
            string resourcesDirectory = assetManagerSetting.bundleLoaderSetting.resourcesRootFolder.Replace("\\", "/");
            List<string> assetList = new List<string>();
            HashSet<string> assetSet = new HashSet<string>(_assetList);
            foreach (int id in Selection.instanceIDs)
            {
                AssetLocation location;
                string path = AssetDatabase.GetAssetPath(id);
                if (path.Contains(resourcesDirectory))
                {
                    location = AssetLocation.ResourcesDirectory;
                }
                else if (path.Contains("StreamingAssets"))
                {
                    location = AssetLocation.StreamingAssets;
                }
                else
                {
                    location = AssetLocation.Others;
                }
                
                switch (location)
                {
                    case AssetLocation.ResourcesDirectory:
                        if (File.Exists(path))
                        {
                            string assetPath = AssetPathHelper.GetShortPath(path);
                            AddAssetToAssetList(assetPath, assetList, assetSet);
                        }
                        if (Directory.Exists(path))
                        {
                            foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                            {
                                if (Path.GetExtension(file) == ".meta")
                                {
                                    continue;
                                }
                                string assetPath = AssetPathHelper.GetShortPath(file);
                                AddAssetToAssetList(assetPath, assetList, assetSet);
                            }
                        }
                        break;
                    case AssetLocation.StreamingAssets:
                        if (_currentTagIndex == 0)
                        {
                            Debug.Log("无需在首包中添加Streaming下的资源");
                            return;
                        }
                        if (File.Exists(path))
                        {
                            string assetPath = AssetPathHelper.GetShortPath(path);
                            AddAssetToAssetList(assetPath, assetList, assetSet);
                        }
                        if (Directory.Exists(path))
                        {
                            foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                            {
                                if (Path.GetExtension(file) == ".meta")
                                {
                                    continue;
                                }
                                string assetPath = AssetPathHelper.GetShortPath(file);
                                AddAssetToAssetList(assetPath, assetList, assetSet);
                            }
                        }
                        break;
                    default:
                        if (File.Exists(path))
                        {
                            string assetPath = AssetPathHelper.GetShortPath(path);
                            AddAssetToAssetList(assetPath, assetList, assetSet);
                        }
                        if (Directory.Exists(path))
                        {
                            foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                            {
                                if (Path.GetExtension(file) == ".meta" || Path.GetExtension(file) == ".cs")
                                {
                                    continue;
                                }
                                string assetPath = AssetPathHelper.GetShortPath(file);
                                AddAssetToAssetList(assetPath, assetList, assetSet);
                            }
                        }
                        break;
                }
            }
            if (SubpackageWindow.TagAsset.TagListForRecord.Count <= 0)
            {
                Debug.Log("没有节点,请先打开Zeus/Asset/TagNodeEditor面板添加");
            }
            else
            {
                if (assetList.Count <= 0) 
                    return;
                _assetList.AddRange(assetList);
                UnityEngine.Debug.Log($"Add {assetList.Count} assets into {SubpackageWindow.TagAsset.TagListForRecord[_currentTagIndex]}.");
            }
            
        }
        private void AddAssetToAssetList(string assetPath, List<string> assetList, HashSet<string> assetSet)
        {
            if (assetSet.Add(assetPath))
            {
                assetList.Add(assetPath);
            }
            else
            {
                UnityEngine.Debug.Log($"asset {assetPath} already includeed in {SubpackageWindow.TagAsset.TagListForRecord[_currentTagIndex]}.");
            }
        }

        SubPackageBundleInfoContainer _container;

        private void PrintCurrentAssetSize()
        {
            var _assetListFiltered = new List<string>();
            foreach (var i in _filterIndex)
            {
                _assetListFiltered.Add(_assetList[i]);
            }
            double totalSize = GetAllBundleSizeOfAssetList(_assetListFiltered);
            
            Debug.Log($"AssetBundles size:"  + $"{totalSize.ToString("F2")} MB");
        }

        /// <summary>
        /// 获取AssetList的bundle大小
        /// </summary>
        /// <param name="assetList"></param>
        /// <returns> size in MB</returns>
        private double GetAllBundleSizeOfAssetList(List<string> assetList)
        {
            AssetBundleUtils.Init();
            var bundleList = AssetListHelper.GetBundleListFromAssetList(assetList);
            long bundlesTotalSize = 0;
            string bundleDir = EditorAssetBundleUtils.GetPathRoot();
            foreach (string bundle in bundleList)
            {
                FileInfo info = new FileInfo(Path.Combine(bundleDir, bundle));
                if (info.Exists)
                {
                    bundlesTotalSize += info.Length;
                }
                else
                {
                    Debug.LogError($"Can't get info of bundle \"{bundle}\".");
                }
            }
            return (double)bundlesTotalSize / Zeus.Core.ZeusConstant.MB;
        }

        private void PrintTagSize(List<string> tags = null)
        {
            AssetBundleUtils.Init();
            _container = SubPackageBundleInfoContainer.LoadSubPackageInfo();
            var tag2Size = _container.GetTag2Size();
            System.Text.StringBuilder content = new System.Text.StringBuilder();
            content.Append("Tag Size:");
            foreach (var pair in tag2Size)
            {
                if(tags == null || tags.Contains(pair.Key))
                {
                    content.AppendLine();
                    content.Append(pair.Key);
                    content.Append(" chunkSize:");
                    content.Append((pair.Value / Zeus.Core.ZeusConstant.MB).ToString("F2"));
                    content.Append(" MB");

                    var tagAssetList = AssetListHelper.LoadAssetListFromFiles(new string[] { pair.Key }, false);
                    var bundleSize = GetAllBundleSizeOfAssetList(tagAssetList);
                    content.Append(" bundleSize:");
                    content.Append(bundleSize.ToString("F2"));
                    content.Append(" MB");
                }
            }
            Debug.Log(content);
        }
        
        private void PrintAssetDependence(string currentTag)
        {
            StreamWriter sw;
            Dictionary<string, bool> dic = new Dictionary<string, bool>();
            AssetBundleUtils.Init();
            var assetMapBundles = AssetBundleUtils.GetAssetMapBundles();
            _container = SubPackageBundleInfoContainer.LoadSubPackageInfo();
            var tags = _container.TagSequence;
            tags.Add(AssetBundleLoaderSetting.FirstPackageTag);
            tags.Add(AssetBundleLoaderSetting.OthersTag);
            foreach (var tag in tags)
            {
                if (currentTag != tag) continue;
                var tagAssetList = AssetListHelper.LoadAssetListFromFiles(new string[] { tag }, false);
                if (!Directory.Exists(Application.dataPath + "/../AssetsListDependence"))
                {
                    Directory.CreateDirectory(Application.dataPath + "/../AssetsListDependence");
                }
                sw = new StreamWriter(Application.dataPath + "/../AssetsListDependence/" + tag + "_bundle.txt");
                foreach (var assetPath in tagAssetList)
                {
                    if (!assetMapBundles.TryGetValue(assetPath, out string bundleName)) 
                        continue;
                    sw.Write(assetPath + "\t" + bundleName + "\t" + GetBundleSizeKb(bundleName) + "\n");
                    foreach (var dependency in AssetBundleUtils.GetAllDependencies(bundleName))
                    {
                        sw.Write("\t\t\t" + dependency + "\t" + GetBundleSizeKb(dependency) + "\n");
                    }
                }
                sw.Flush();
                sw.Close();
                sw = new StreamWriter(Application.dataPath + "/../AssetsListDependence/" + tag + "_asset.txt");
                
                
                for (int i = 0; i < tagAssetList.Count; i++)
                {
                    var assetPath = AssetExtHelper.GetFullPathByType(tagAssetList[i]);
                    WriteDependence(assetPath);
                }
                foreach (var pair in dic)
                {
                    if (!File.Exists(Application.dataPath + "/" + pair.Key))
                        continue;
                    int length = (int) ((float)File.Open(Application.dataPath + "/" + pair.Key, FileMode.Open).Length / Zeus.Core.ZeusConstant.KB * 100);
                    sw.Write(length / 100f + "\t");
                    if (!pair.Value)
                        sw.Write("\t");
                    sw.Write(pair.Key + "\n");
                }
                sw.Close();
            }

            void WriteDependence(string assetPath, int depth = 0)
            {
                
                if (string.IsNullOrEmpty(assetPath))
                    return;
                if (assetPath.EndsWith(".cs"))
                {
                    depth--;
                }
                else
                {
                    if (assetPath.StartsWith("Packages"))
                        return;
                    try
                    {
                        dic.Add(assetPath.Substring(7, assetPath.Length - 7), depth == 0);
                    }
                    catch (ArgumentException)
                    {
                    }
                }

                var dependencies = AssetDatabase.GetDependencies(assetPath, false);
                if (dependencies.Length > 0)
                {
                    foreach (var dependency in dependencies)
                    {
                        if (AssetExtHelper.IsUnityAsset(dependency) && depth < 10)
                        {
                            WriteDependence(dependency, depth + 1);
                        }
                    }
                }
            }
            
            void WriteDependenceAsset(string assetPath, int depth = 0)
            {
                if (string.IsNullOrEmpty(assetPath))
                    return;
                if (assetPath.EndsWith(".cs"))
                {
                    depth--;
                }
                else
                {
                    if (assetPath.StartsWith("Packages"))
                        return;
                    sw.Write(File.Open(Application.dataPath + "\\" + assetPath.Substring(7, assetPath.Length - 7), FileMode.Open).Length.ToString());
                    for (int i = 0; i < depth; i++)
                    {
                        sw.Write("\t");
                    }
                    sw.Write(assetPath.Substring(7, assetPath.Length - 7) + "\n");
                }

                var dependencies = AssetDatabase.GetDependencies(assetPath, false);
                if (dependencies.Length > 0)
                {
                    foreach (var dependency in dependencies)
                    {
                        if (AssetExtHelper.IsUnityAsset(dependency) && depth < 10)
                        {
                            WriteDependenceAsset(dependency, depth + 1);
                        }
                    }
                }
            }
            
            void WriteDependenceBundle(string assetPath)
            {
                if (string.IsNullOrEmpty(assetPath))
                    return;
                string assetName = assetPath.Substring(7, assetPath.Length - 7);
                string assetNameShort = AssetExtHelper.RemoveExt(assetPath.Split(new[] {"/"}, 4, StringSplitOptions.None)[3]);
                if (!assetMapBundles.TryGetValue(assetNameShort, out string bundleName))
                    return;
                sw.Write(assetName + "\t");
                sw.Write(bundleName + "\t");
                sw.Write(GetBundleSizeKb(bundleName) + "\n");
                
                var dependencies = AssetBundleUtils.GetAllDependencies(bundleName);
                foreach (var dependency in dependencies)
                {
                    sw.Write("\t\t\t" + dependency + "\t");
                    sw.Write(GetBundleSizeKb(dependency) + "\n");
                }
            }
        }

        private float GetBundleSizeKb(string bundleName)
        {
            string bundleDir = EditorAssetBundleUtils.GetPathRoot();
            FileInfo info = new FileInfo(Path.Combine(bundleDir, bundleName));
            if (info.Exists)
            {
                float result = (int) ((float)info.Length / Zeus.Core.ZeusConstant.KB * 100);
                return result / 100f;
            }
            return -1;
        }
    }

    public class AddAssetToTagWindow : EditorWindow
    {
        private string _filePath = "";
        public bool isReady;
        public List<string> assetList = new List<string>();
        public Rect rect;
        public EditorWindow parent;
        
        private void OnEnable()
        {
            position = rect;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("选择文件:",GUILayout.Width(60));
            _filePath = EditorGUILayout.TextField("", _filePath);
            if (GUILayout.Button("…", EditorStyles.toolbar, GUILayout.Width(30)))
            {
                string temp = EditorUtility.OpenFilePanel("选择资源列表文件:", _filePath, string.Empty);
                if (!string.IsNullOrEmpty(temp))
                {
                    _filePath = temp;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.LabelField("提示 : 请确保Bundle目录中有\"assetMapName.xml\"文件:");
            
            if (GUILayout.Button(new GUIContent("开始添加")))
            {
                AssetBundleUtils.Init();
                var bundleMapAssets = AssetBundleUtils.GetBundleMapAssets();
                using (var sr = new StreamReader(_filePath))
                {
                    string currentLine;
                    while ((currentLine = sr.ReadLine()) != null)
                    {
                        var current = currentLine.Split('=');
                        if (current[0] == "bundle")
                        {
                            if (bundleMapAssets.TryGetValue(current[1], out var assets))
                            {
                                assetList.AddRange(assets);
                                Debug.Log("已添加bundle: " + current[1] + " !");
                            }
                            else
                            {
                                Debug.LogError("没找到bundle: " + current[1] + " 的信息!");
                            }
                        }
                        else if (current[0] == "asset")
                        {
                            assetList.Add(current[1]);
                            Debug.Log("已添加asset: " + current[1] + " !");
                        }
                    }
                }
                isReady = true;
                parent.Focus();
            }

            if (GUILayout.Button(new GUIContent("取消")))
            {
                this.Close();
            }
            
        }
    }

    public class SubpackageUtilWindow : EditorWindow
    {
        [MenuItem("Zeus/Asset/BundleInfo查询工具", false, 8)]
        static void Open()
        {
            SubpackageUtilWindow window = GetWindow<SubpackageUtilWindow>("BundleInfo查询工具");
        }

        [MenuItem("Zeus/Asset/解析输出二包配置", false, 9)]
        static void SubpackageOuput()
        {
            var container = SubPackageBundleInfoContainer.LoadSubPackageInfo("Low");
            container.DumpContent(Zeus.Core.FileSystem.VFileSystem.GetRealZeusSettingPath("Low_SubpackageBundleInfo.fb.txt"));
            container = SubPackageBundleInfoContainer.LoadSubPackageInfo("Default");
            container.DumpContent(Zeus.Core.FileSystem.VFileSystem.GetRealZeusSettingPath("Default_SubpackageBundleInfo.fb.txt"));
        }

        SubPackageBundleInfoContainer _container;
        string _bundleName = string.Empty;
        string _bundleInfo = string.Empty;
        private void OnGUI()
        {
            _bundleName = EditorGUILayout.TextField("Assetbundle Name", _bundleName);
            if (GUILayout.Button("查询Bundle信息"))
            {
                _bundleInfo = GetInfoByBundleName(_bundleName);
            }
            if (!string.IsNullOrEmpty(_bundleInfo))
            {
                EditorGUILayout.TextArea(_bundleInfo);
            }
        }

        private string GetInfoByBundleName(string bundleName)
        {
            if (_container == null)
            {
                _container = SubPackageBundleInfoContainer.LoadSubPackageInfo();
            }
            SubPackageBundleInfo info;
            if (_container.TryGetBundleInfo(bundleName, out info))
            {
                System.Text.StringBuilder builder = new System.Text.StringBuilder(bundleName);
                builder.Append("\n");
                builder.Append("ChunkName: ");
                builder.AppendLine(info.ChunkFile);
                builder.Append("Crc32: ");
                builder.AppendLine(info.BundleCrc32.ToString());
                builder.Append("Size: ");
                builder.AppendLine(info.BundleSize.ToString());
                return builder.ToString();
            }
            else
            {
                Debug.LogError($"Can't get info of bundle \"{bundleName}\".");
                return string.Empty;
            }
        }
    }

    public class SubpackageCreateWindow : EditorWindow
    {
        [MenuItem("Zeus/Asset/手动生成二包资源",false, 4)]
        static void Open()
        {
            SubpackageCreateWindow window = GetWindow<SubpackageCreateWindow>("手动生成二包资源");
        }

        string _outputPath = string.Empty;
        BuildTarget _target;

        private void OnEnable()
        {
            _target = EditorUserBuildSettings.activeBuildTarget;
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            _outputPath = EditorGUILayout.TextField("Subpackag OutputPath", _outputPath);
            if (GUILayout.Button("…", EditorStyles.toolbar, GUILayout.Width(30)))
            {
                string temp = EditorUtility.OpenFolderPanel("Output Folder", _outputPath, string.Empty);
                if (!string.IsNullOrEmpty(temp))
                {
                    _outputPath = temp;
                }
            }
            GUILayout.EndHorizontal();
            _target = (BuildTarget)EditorGUILayout.EnumPopup("Target", _target);

            if (GUILayout.Button("生成"))
            {
                AssetBuildProcessor.CreateSubpackage(_outputPath, _target);
            }
        }
    }
    
    public class SubpackageCheckWindow : EditorWindow
    {
        [MenuItem("Zeus/Asset/资源校验工具", false, 6)]
        static void Open()
        {
            SubpackageCheckWindow window = GetWindow<SubpackageCheckWindow>("资源校验工具");
        }

        private string _subpackagePath = string.Empty;
        private string _md5VersionPath = string.Empty;
        private string _result;

        private void OnEnable()
        {
            _md5VersionPath = Directory.GetParent(Application.dataPath) + "/" + AssetBundleUtils._GetBundleRootPath() + "MD5Version.xml";
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            
            GUILayout.BeginHorizontal();
            _subpackagePath = EditorGUILayout.TextField("Subpackage文件路径", _subpackagePath);
            if (GUILayout.Button("…", EditorStyles.toolbar, GUILayout.Width(30)))
            {
                string temp = EditorUtility.OpenFilePanel("Subpackage文件路径", _subpackagePath, string.Empty);
                if (!string.IsNullOrEmpty(temp))
                {
                    _subpackagePath = temp;
                }
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            _md5VersionPath = EditorGUILayout.TextField("MD5Version文件路径", _md5VersionPath);
            if (GUILayout.Button("…", EditorStyles.toolbar, GUILayout.Width(30)))
            {
                string temp = EditorUtility.OpenFilePanel("MD5Version文件路径", _md5VersionPath, string.Empty);
                if (!string.IsNullOrEmpty(temp))
                {
                    _md5VersionPath = temp;
                }
            }
            GUILayout.EndHorizontal();
            

            if (GUILayout.Button("Check"))
            {
                _result = null;
                if (CheckSubpackageCRC32(_subpackagePath, _md5VersionPath, out _result))
                {
                    EditorUtility.DisplayDialog("", "校验成功!", "确认");
                }
                else
                {
                    EditorUtility.DisplayDialog("", "校验失败! 详见校验面板", "确认");
                }
            }
            
            if (!string.IsNullOrEmpty(_result))
            {
                EditorGUILayout.TextArea(_result);
            }

            GUILayout.EndVertical();
        }

        private void OnDestroy()
        {
            Directory.Delete(Application.temporaryCachePath + "/CheckTemp", true);
        }

        public static bool CheckSubpackageCRC32(string subpackagePath, string md5VersionPath, out string resultStr)
        {
            bool _checked = false;
            if (!File.Exists(subpackagePath))
            {
                resultStr = subpackagePath + " 文件未找到,请确认路径";
                return false;
            }

            var result = new System.Text.StringBuilder();
            SubPackageBundleInfoContainer container = SubPackageBundleInfoContainer.LoadSubPackageInfo();
            var chunk2Bundles = container.Chunk2Bundles;
            var bundleInfoDic = container.GetBundleInfoDic();

            string destPath = Application.temporaryCachePath + "/CheckTemp";
            EditorUtility.DisplayProgressBar("资源校验工具", "SubPackage文件解压中...", 0);
            Zeus.Core.ZipUtil.ExtractFileFormZipArchive(subpackagePath, destPath);
            EditorUtility.DisplayProgressBar("资源校验工具", "SubPackage文件解压中...", 1);
            
            var md5FileInfos = MD5Util.DecodeMD5Version(md5VersionPath);
            
            byte[] buffer = new byte[1024];
            var chunkDirectory = new DirectoryInfo(destPath + "/Chunk");
            Directory.CreateDirectory(destPath + "/UnpackedBundles");
            var infos = chunkDirectory.GetFiles();
            for (var i = 0; i < infos.Length; i++)
            {
                EditorUtility.DisplayProgressBar("资源校验工具", "Chunk文件校验中...", (i+0f) / infos.Length);
                FileInfo fileInfo = infos[i];
                List<string> chunk2Bundle;
                if (chunk2Bundles.TryGetValue(fileInfo.Name, out chunk2Bundle))
                {
                    foreach (var bundleName in chunk2Bundle)
                    {
                        long chunkFrom = bundleInfoDic[bundleName].ChunkFrom;
                        long bundleSize = bundleInfoDic[bundleName].BundleSize;
                        var tempFilePath = Path.Combine(destPath, "UnpackedBundles", bundleName);
                        using (FileStream fsWrite = File.Open(tempFilePath, FileMode.Create, FileAccess.Write))
                        {
                            using (FileStream fsRead = fileInfo.OpenRead())
                            {
                                fsRead.Seek(chunkFrom, SeekOrigin.Begin);
                                long numToRead = bundleSize;
                                while (numToRead > 0)
                                {
                                    int readCount = fsRead.Read(buffer, 0, (int) Mathf.Min(buffer.Length, numToRead));
                                    if (readCount == 0)
                                    {
                                        break;
                                    }
                                    fsWrite.Write(buffer, 0, readCount);
                                    numToRead -= readCount;
                                }
                            }
                        }

                        var md5FromFile = MD5Util.GetMD5FromFile(tempFilePath);

                        MD5FileInfo md5FileInfo;
                        if (md5FileInfos.TryGetValue(bundleName, out md5FileInfo))
                        {
                            _checked = true;
                            if (md5FromFile == md5FileInfo.MD5.Replace("-", string.Empty)) //兼容旧版
                                continue;
                            result.Append("Chunk文件: " + fileInfo.Name + " MD5校验失败\n");
                            goto breakLoop; //校验失败时跳出多层循环和Origin校验
                        }
                        result.Append("Chunk文件: " + fileInfo.Name + " 中Bundle" + bundleName + " 未找到原MD5码\n");
                    }
                }
            }

            var bundleDirectory = new DirectoryInfo(destPath + "/Origin");
            var files = bundleDirectory.GetFiles("*", SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; i++)
            {
                EditorUtility.DisplayProgressBar("资源校验工具", "Origin文件校验中...", (i+0f) / files.Length);
                FileInfo fileInfo = files[i];
                var md5FromFile = MD5Util.GetMD5FromFile(fileInfo.FullName);
                MD5FileInfo md5FileInfo;
                if (md5FileInfos.TryGetValue(fileInfo.Name, out md5FileInfo))
                {
                    if (md5FromFile == md5FileInfo.MD5.Replace("-", string.Empty)) //兼容旧版
                        continue;
                    result.Append("Origin文件: " + fileInfo.Name + " MD5校验失败\n");
                    break;
                }
                result.Append("Origin文件: " + fileInfo.Name + " 未找到原MD5码\n");
            }
            
            breakLoop:
            EditorUtility.ClearProgressBar();

            if (result.Length == 0)
            {
                resultStr = _checked ? "校验成功~" : "校验未进行!  需在本机进行过完整打包流程后才可以进行校验";
                return _checked;
            }

            resultStr = result.ToString();
            return false;
        }
    }
}
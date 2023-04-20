/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Zeus.Build;
using Object = UnityEngine.Object;


namespace Zeus.Framework.Asset
{
    public class InvalidBundleProcessor : EditorWindow
    {
        private Vector2 vec;
        private Rect rect;

        private bool _isManuallySetCachePath;
        private string _cachePath;
        private List<string> _cachePaths;
        private List<string> _invalidAssetPath;
        private Dictionary<string, Dictionary<string, List<string>>> _bundleToDelete;
        private string _result;
        
        [MenuItem("Zeus/Asset/损坏Bundle清理工具", false, 6)]
        static void Open()
        {
            InvalidBundleProcessor window = GetWindow<InvalidBundleProcessor>("损坏Bundle清理工具");
        }
        
        private void OnEnable()
        {
            _cachePath = string.Empty;
            _cachePaths = new List<string>();
            _invalidAssetPath = new List<string>();
            vec = Vector2.zero;
            _bundleToDelete = new Dictionary<string, Dictionary<string, List<string>>>();
        }

        
        private void OnGUI()
        {
            GUILayout.BeginVertical();
            if (_isManuallySetCachePath)
            {
                if (GUILayout.Button("改为自动选择缓存路径"))
                {
                    _isManuallySetCachePath = false;
                }
                GUILayout.BeginHorizontal();
                _cachePath = EditorGUILayout.TextField("cache文件路径", _cachePath);
                if (GUILayout.Button("…", EditorStyles.toolbar, GUILayout.Width(30)))
                {
                    string temp = EditorUtility.OpenFolderPanel("cache文件路径", _cachePath, string.Empty);
                    if (!string.IsNullOrEmpty(temp))
                    {
                        _cachePath = temp;
                        Repaint();
                    }
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                if (GUILayout.Button("改为手动选择缓存路径"))
                {
                    _isManuallySetCachePath = true;
                }
            }
            
            GUILayout.BeginHorizontal();
            GUILayout.Box("拖拽到此处以记录资源",GUILayout.Width(rect.width*0.5f),GUILayout.Height(20));
            if (GUILayout.Button("手动添加资源"))
            {
                _invalidAssetPath.Add(string.Empty);
            }
            GUILayout.EndHorizontal();

            vec = GUILayout.BeginScrollView(vec);
            for (int i = 0; i < _invalidAssetPath.Count; ++i)
            {
                string scenePath = _invalidAssetPath[i];
                if (scenePath == null)
                {
                    continue;
                }
                EditorGUILayout.BeginHorizontal();
                _invalidAssetPath[i] = EditorGUILayout.TextField(scenePath, GUILayout.Height(20));
                var sceneAsset = EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath<Object>(_invalidAssetPath[i]), typeof(Object), true, GUILayout.Width(200));
                if (sceneAsset != null)
                {
                    _invalidAssetPath[i] = AssetDatabase.GetAssetPath(sceneAsset);
                }
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    _invalidAssetPath[i] = null;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (!string.IsNullOrEmpty(_result))
            {
                EditorGUILayout.TextArea(_result);
            }
            GUILayout.EndScrollView();
            
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("查询Bundle"))
            {
                _cachePaths.Clear();
                _result = "未找到任何相关Bundle!";
                if (_isManuallySetCachePath)
                {
                    _cachePaths.Add(_cachePath);
                }
                else
                {
                    var parentFolders = Directory.GetDirectories("AssetBundleCache");
                    foreach (var parent in parentFolders)
                    {
                        foreach (var folder in Directory.GetDirectories(parent))
                        {
                            _cachePaths.Add(folder);
                        }
                    }
                }

                var str = new StringBuilder();
                foreach (var cachePath in _cachePaths)
                {
                    _bundleToDelete[cachePath] = new Dictionary<string, List<string>>();
                    if (!GetCacheInfo(cachePath, out var assetMapBundles, out var bundleIsDependedBy, out var bundleMapDepData))
                        continue;
                    
                    foreach (var path in _invalidAssetPath)
                    {
                        if (path == null)
                            continue;
                        var pathL = path.Replace('\\', '/');
                        _bundleToDelete[cachePath].Add(pathL, GetInvalidBundles(pathL, assetMapBundles, bundleIsDependedBy, bundleMapDepData));
                    }
                    
                    str.Append("[");
                    str.Append(cachePath);
                    str.AppendLine("]");
                    foreach (var pair in _bundleToDelete[cachePath])
                    {
                        str.Append("nAsset: ");
                        str.AppendLine(pair.Key);
                        if (pair.Value != null)
                        {
                            foreach (var bundle in pair.Value)
                            {
                                str.Append(" -");
                                str.AppendLine(bundle);
                            }
                        }
                        else
                        {
                            str.AppendLine("没找到这个资源的Bundle, 可能已被删除");
                        }
                        str.AppendLine();
                    }
                    str.AppendLine();
                }
                _result = str.ToString();
            }

            if (GUILayout.Button("删除Bundle"))
            {
                var str = new StringBuilder();
                foreach (var directory in _bundleToDelete)
                {
                    foreach (var pair in directory.Value)
                    {
                        if (pair.Value == null)
                            continue;
                        foreach (var bundle in pair.Value)
                        {
                            var filePath = Path.Combine(directory.Key, bundle);
                            if (File.Exists(filePath))
                            {
                                File.Delete(filePath);
                                str.AppendLine("成功删除了:\t" + filePath);
                            }
                            else
                            {
                                str.AppendLine("没找到:\t" + filePath);
                            }

                            if (File.Exists(filePath + ".manifest"))
                            {
                                File.Delete(filePath + ".manifest");
                                str.AppendLine("成功删除了:\t" + filePath + ".manifest");
                            }
                            else
                            {
                                str.AppendLine("没找到:\t" + filePath + ".manifest");
                            }
                        }
                    }
                }

                Debug.Log(str);
            }
            GUILayout.EndHorizontal();

            
            GUILayout.EndVertical();
            
            rect = new Rect(0, 0, position.width, 100);
            if (Event.current.type == EventType.DragUpdated)
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            if (Event.current.type == EventType.DragExited && rect.Contains(Event.current.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                {
                    foreach (var path in DragAndDrop.paths)
                    {
                        if (Directory.Exists(path))
                        {
                            foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                            {
                                if (!file.ToLower().EndsWith(".meta") && !_invalidAssetPath.Contains(file))
                                    _invalidAssetPath.Add(file);
                            }
                        }
                        else
                        {
                            if (!path.ToLower().EndsWith(".meta") && !_invalidAssetPath.Contains(path))
                                _invalidAssetPath.Add(path);
                        }
                    }
                }
            }
        }
        

        private bool GetCacheInfo(string cacheDir, out Dictionary<string, List<string>> assetMapBundles, 
            out Dictionary<string, List<string>> bundleIsDependedBy, out Dictionary<string, List<string>> bundleMapDepData)
        {
            assetMapBundles = new Dictionary<string, List<string>>();
            bundleIsDependedBy = new Dictionary<string, List<string>>();
            bundleMapDepData = new Dictionary<string, List<string>>();
            
            //获取 bundleMapAssets bundleMapDepData
            string mainManifest = cacheDir.Substring(cacheDir.LastIndexOf('/') + 1) + ".manifest";
            DirectoryInfo bundleDirectory = new DirectoryInfo(cacheDir);
            var bundleFiles = bundleDirectory.GetFiles("*.manifest");

            if (bundleFiles.Length <= 0)
                return false;
            
            var bundleMapAssets = new Dictionary<string, List<string>>();

            foreach (var file in bundleFiles)
            {
                if (file.Name == mainManifest)
                    continue;

                bool isReadingAsset = false;
                bool isReadingDependencies = false;
                var currentBundle = file.Name.Substring(0, file.Name.Length - 9);
                using (var sr = file.OpenText())
                {
                    string currentLine;
                    List<string> assetsList = new List<string>();
                    while ((currentLine = sr.ReadLine()) != null)
                    {
                        if (isReadingDependencies) //读依赖
                        {
                            var bundleName = currentLine.Substring(currentLine.LastIndexOf("/", StringComparison.Ordinal) + 1);
                            bundleMapDepData[currentBundle].Add(bundleName);
                        }
                        else if (isReadingAsset && !currentLine.Contains("Dependencies:")) //读资源
                        {
                            var assetPath = currentLine.Substring(2);
                            assetsList.Add(assetPath);
                        }
                        else if (currentLine == "Assets:") //下一行开始读资源
                        {
                            isReadingAsset = true;
                        }
                        else if (isReadingAsset) //资源读完了 下一行开始读依赖
                        {
                            isReadingAsset = false;
                            isReadingDependencies = true;
                            bundleMapAssets.Add(currentBundle, assetsList);
                            bundleMapDepData.Add(currentBundle, new List<string>());
                        }
                    }
                }
            }

            //反转获取获取 assetMapBundles bundleIsDependedBy
            foreach (var pair in bundleMapAssets)
            {
                var bundle = pair.Key;
                foreach (var asset in pair.Value)
                {
                    if (!assetMapBundles.ContainsKey(asset))
                    {
                        var bundleList = new List<string> {bundle};
                        assetMapBundles.Add(asset, bundleList);
                    }
                    else
                    {
                        assetMapBundles[asset].Add(bundle);
                    }
                }
            }

            foreach (var pair in bundleMapDepData)
            {
                var valueBundle = pair.Key;
                foreach (var keyBundle in pair.Value)
                {
                    if (!bundleIsDependedBy.ContainsKey(keyBundle))
                    {
                        var bundleList = new List<string> {valueBundle};
                        bundleIsDependedBy.Add(keyBundle, bundleList);
                    }
                    else
                    {
                        bundleIsDependedBy[keyBundle].Add(valueBundle);
                    }
                }
            }
            return true;
        }

        private List<string> GetInvalidBundles(string invalidAsset, Dictionary<string, List<string>> assetMapBundles, 
            Dictionary<string, List<string>> bundleIsDependedBy, Dictionary<string, List<string>> bundleMapDepData)
        {
            if (!assetMapBundles.TryGetValue(invalidAsset, out var targetBundles))
                return null;
            
            var result = new HashSet<string>();
            var stack = new Stack<string>();
            foreach (var targetBundle in targetBundles)
            {
                result.Add(targetBundle);
                stack.Push(targetBundle);
                while (stack.Any())
                {
                    stack.Pop();
                    if (bundleMapDepData.TryGetValue(targetBundle, out var deps))
                    {
                        foreach (var dep in deps)
                        {
                            if (result.Add(dep))
                            {
                                stack.Push(dep);
                            }
                        }
                    }
                }
                if (bundleIsDependedBy.TryGetValue(targetBundle, out var depBys))
                {
                    foreach (var depBy in depBys)
                    {
                        result.Add(depBy);
                    }
                }
            }
            return result.ToList();
        }

        private void DeleteInvalidBundleInternal(string pathString)
        {
            var invalidAssetPath = pathString.Split(';').ToList();
            OnEnable();
            _cachePaths.Clear();

            var parentFolders = Directory.GetDirectories("AssetBundleCache");
            foreach (var parent in parentFolders)
            {
                foreach (var folder in Directory.GetDirectories(parent))
                {
                    _cachePaths.Add(folder);
                }
            }
            
            foreach (var cachePath in _cachePaths)
            {
                _bundleToDelete[cachePath] = new Dictionary<string, List<string>>();
                if (!GetCacheInfo(cachePath, out var assetMapBundles, out var bundleIsDependedBy, out var bundleMapDepData))
                    continue;

                foreach (var path in invalidAssetPath)
                {
                    if (path == null)
                        continue;
                    var pathL = path.Replace('\\', '/');
                    _bundleToDelete[cachePath].Add(pathL, GetInvalidBundles(pathL, assetMapBundles, bundleIsDependedBy, bundleMapDepData));
                }
            }
            
            var str = new StringBuilder();
            foreach (var directory in _bundleToDelete)
            {
                foreach (var pair in directory.Value)
                {
                    if (pair.Value == null)
                        continue;
                    foreach (var bundle in pair.Value)
                    {
                        var filePath = Path.Combine(directory.Key, bundle);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                            str.AppendLine("成功删除了:\t" + filePath);
                        }
                        else
                        {
                            str.AppendLine("没找到:\t" + filePath);
                        }

                        if (File.Exists(filePath + ".manifest"))
                        {
                            File.Delete(filePath + ".manifest");
                            str.AppendLine("成功删除了:\t" + filePath + ".manifest");
                        }
                        else
                        {
                            str.AppendLine("没找到:\t" + filePath + ".manifest");
                        }
                    }
                }
            }
            Debug.Log(str);
        }

        public static void DeleteInvalidBundle(string pathString)
        {
            var processor = new InvalidBundleProcessor();
            processor.DeleteInvalidBundleInternal(pathString);
        }

        public static void DeleteInvalidBundleInBatchMode()
        {
            CommandLineArgs.Initialize();
            string clearInvalidAb = string.Empty;
            if (CommandLineArgs.TryGetString("CLEAR_INVALID_BUNDLE_BY_ASSET_PATH", ref clearInvalidAb))
            {
                DeleteInvalidBundle(clearInvalidAb);
            }
            else
            {
                Debug.LogError("DeleteInvalidBundleInBatchMode() 参数读取出错");
            }
        }
    }
}
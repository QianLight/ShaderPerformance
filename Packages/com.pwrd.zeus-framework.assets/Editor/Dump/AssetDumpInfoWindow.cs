/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Zeus.Framework.Asset
{
    public class AssetDumpInfoWindow : EditorWindow
    {
        [MenuItem("Zeus/Asset/AssetProfiler", false, 3)]
        public static void ShowExample()
        {
            AssetDumpInfoWindow wnd = GetWindow<AssetDumpInfoWindow>();
            wnd.titleContent = new GUIContent("AssetProfiler");
        }

        public void OnEnable()
        {
            RefreshSnapFile();
            InitScreenShot();
        }

        public void Awake()
        {
            InitTextAreaStyle();
        }

        private List<string> _snapshotsFileList;
        private bool _isSnapRefreshed = false;
        private GUIStyle _textAreaStyle;
        private Texture2D _snapPic1;
        private Texture2D _snapPic2;
        int _firstSnapshotIndex = 0;
        int _firstSnapshotIndexLast = -1;
        int _secondSnapshotIndex = 1;
        int _secondSnapshotIndexLast = -1;
        float _screenShotAspectRatio;

        private void RefreshSnapFile()
        {
            if (_isSnapRefreshed)
            {
                return;
            }
            _snapshotsFileList = new List<string>();
            if (Directory.Exists(AssetDumpHelper.EditorSnapshotDirectory))
            {
                foreach (var file in Directory.GetFiles(AssetDumpHelper.EditorSnapshotDirectory, "*" + AssetDumpHelper.SnapshotExtension, SearchOption.TopDirectoryOnly))
                {
                    _snapshotsFileList.Add(Path.GetFileNameWithoutExtension(file));
                }
            }
        }

        private void InitScreenShot()
        {
            _isSnapRefreshed = false;
            _firstSnapshotIndexLast = -1;
            _secondSnapshotIndexLast = -1;
            _firstSnapshotIndex = -1;
            _secondSnapshotIndex = -1;
            _snapPic1 = new Texture2D(1600, 900);
            _snapPic2 = new Texture2D(1600, 900);
            if (_snapshotsFileList.Count > 0 && File.Exists(Path.Combine(AssetDumpHelper.EditorSnapshotDirectory, _snapshotsFileList[0] + ".png")))
            {
                byte[] picBytes = File.ReadAllBytes(Path.Combine(AssetDumpHelper.EditorSnapshotDirectory, _snapshotsFileList[0] + ".png"));
                _snapPic1.LoadImage(picBytes);
                _firstSnapshotIndex = 0;
            }
            if (_snapshotsFileList.Count >1 && File.Exists(Path.Combine(AssetDumpHelper.EditorSnapshotDirectory, _snapshotsFileList[1] + ".png")))
            {
                byte[] picBytes = File.ReadAllBytes(Path.Combine(AssetDumpHelper.EditorSnapshotDirectory, _snapshotsFileList[1] + ".png"));
                _snapPic2.LoadImage(picBytes);
                _secondSnapshotIndex = 1;
            }
            _screenShotAspectRatio = (float)_snapPic1.width / _snapPic1.height;
            _screenShotAspectRatio = (float)_snapPic2.width / _snapPic2.height;
        }

        private void OnProjectChange()
        {
            OnEnable();
        }

        private void InitTextAreaStyle()
        {
            _textAreaStyle = new GUIStyle(EditorStyles.textArea);
        }

        Vector2 scrollVec;
        Vector2 scrollVec1;
        Vector2 scrollVec2;
        Vector2 scrollVecCompare;
        string _snapshotText1 = "";
        string _snapshotText2 = "";
        string _snapshotCompareText = "";
        bool _refeshSnapshot = false;
        private bool _changeImage = false;

        private void OnGUI()
        {
            var windowWidth = this.position.width;
            var windowHeight = this.position.height;
            if(GUILayout.Button("Dump"))
            {
                AssetManager.Dump();
                RefreshSnapFile();
                _changeImage = true;
            }
            EditorGUILayout.Space();
            scrollVec = EditorGUILayout.BeginScrollView(scrollVec);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Box(_snapPic1, GUILayout.Width(100 * _screenShotAspectRatio), GUILayout.Height(100));
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Snapshot1", GUILayout.Width(70));
            _firstSnapshotIndex = EditorGUILayout.Popup(_firstSnapshotIndex, _snapshotsFileList.ToArray());
            if(_firstSnapshotIndexLast != _firstSnapshotIndex)
            {
                LoadSnapshotScreenshot(_snapPic1, _firstSnapshotIndex);
                _snapshotText1 = OpenSnapshot(_firstSnapshotIndex);
            }
            _firstSnapshotIndexLast = _firstSnapshotIndex;
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Box(_snapPic2, GUILayout.Width(100 * _screenShotAspectRatio), GUILayout.Height(100));
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Snapshot2", GUILayout.Width(70));
            _secondSnapshotIndex = EditorGUILayout.Popup(_secondSnapshotIndex, _snapshotsFileList.ToArray());
            if (_secondSnapshotIndexLast != _secondSnapshotIndex)
            {
                LoadSnapshotScreenshot(_snapPic2, _secondSnapshotIndex);
                _snapshotText2 = OpenSnapshot(_secondSnapshotIndex);
            }
            _secondSnapshotIndexLast = _secondSnapshotIndex;
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            if (_changeImage)
            {
                if (_snapshotsFileList.Count > 0)
                {
                    _firstSnapshotIndex = _snapshotsFileList.Count - 1;
                }
                if (_snapshotsFileList.Count > 1)
                {
                    _secondSnapshotIndex = _snapshotsFileList.Count - 2;
                }
                _changeImage = false;
            }
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear", GUILayout.Width(130)))
            {
                AssetDumpHelper.ClearAllSnapshotFiles();
                _snapshotCompareText = _snapshotText1 = _snapshotText2 = "";
                OnEnable();
            }
            if (GUILayout.Button("Compare"))
            {
                if (_firstSnapshotIndex == _secondSnapshotIndex)
                {
                    _snapshotCompareText = "选择了相同的Snapshot";
                }
                else
                {
                    _snapshotCompareText = Compare(_firstSnapshotIndex, _secondSnapshotIndex);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            GUILayout.BeginArea(new Rect(0,230,windowWidth,(windowHeight-270)/2));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("SnapshotInfo1:");
            scrollVec1 = EditorGUILayout.BeginScrollView(scrollVec1);
            if (_snapshotText1.Length > 100)
                EditorGUILayout.TextArea(_snapshotText1, _textAreaStyle);
            else
                EditorGUILayout.TextArea(_snapshotText1, _textAreaStyle,
                    GUILayout.MinWidth(windowWidth/2 -15), GUILayout.MinHeight((windowHeight - 310) / 2.1f -4));
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("SnapshotInfo2:");
            scrollVec2 = EditorGUILayout.BeginScrollView(scrollVec2);
            if (_snapshotText2.Length > 100)
                EditorGUILayout.TextArea(_snapshotText2, _textAreaStyle);
            else
                EditorGUILayout.TextArea(_snapshotText2, _textAreaStyle,
                    GUILayout.MinWidth(windowWidth/2 -15), GUILayout.MinHeight((windowHeight - 310) / 2.1f -4));
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
            
            GUILayout.BeginArea(new Rect(0,windowHeight/2+100,windowWidth,(windowHeight-290)/2));
            
            EditorGUILayout.LabelField("Snapshot Difference:");
            scrollVecCompare = EditorGUILayout.BeginScrollView(scrollVecCompare);
            if (_snapshotCompareText.Length > 157)
                EditorGUILayout.TextArea(_snapshotCompareText, _textAreaStyle);
            else
                EditorGUILayout.TextArea(_snapshotCompareText, _textAreaStyle, GUILayout.MinHeight((windowHeight - 310) / 2.1f -7));
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private bool LoadSnapshotScreenshot(Texture2D texture, int index)
        {
            if (_snapshotsFileList.Count <= index || index < 0 || 
                !File.Exists(Path.Combine(AssetDumpHelper.EditorSnapshotDirectory, _snapshotsFileList[index] + ".png"))) 
                return false;
            texture.LoadImage(File.ReadAllBytes(Path.Combine(AssetDumpHelper.EditorSnapshotDirectory, _snapshotsFileList[index] + ".png")));
            return true;
        }

        private string OpenSnapshot(int index)
        {
            if(_snapshotsFileList.Count == 0)
            {
                return string.Empty;
            }
            return GetSnapinfo(GetSnapPathByIndex(index));
        }

        private string GetSnapinfo(string snapPath)
        {
            DumpInfo info = AssetDumpHelper.LoadDumpInfoFromFile(snapPath);

            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
            strBuilder.Append(Path.GetFileName(snapPath));
            strBuilder.Append("\n\n");
            strBuilder.Append("缓存的Asset数目:");
            strBuilder.Append(info.cachedAssetList.Count);
            strBuilder.AppendLine();
            foreach (var asset in info.cachedAssetList)
            {
                strBuilder.Append(asset.path);
                strBuilder.Append(" : ");
                strBuilder.Append(asset.refCount);
                strBuilder.AppendLine();
            }
            if (info.assetLoaderType == AssetLoaderType.AssetBundle)
            {
            strBuilder.AppendLine();
            strBuilder.Append("缓存的Bundle数目:");
            strBuilder.Append(info.cachedBundleList.Count);
            strBuilder.AppendLine();
            foreach (var bundle in info.cachedBundleList)
            {
                strBuilder.Append(bundle.name);
                strBuilder.Append(" : ");
                strBuilder.Append(bundle.refCount);
                strBuilder.AppendLine();
            }
            }
            return strBuilder.ToString();
        }

        private string Compare(int index1, int index2)
        {
            if (_snapshotsFileList.Count > index1 && _snapshotsFileList.Count > index2)
            {
                return GetCompareInfo(GetSnapPathByIndex(index1), GetSnapPathByIndex(index2));
            }
            return "";
        }

        private string GetCompareInfo(string snapPath1, string snapPath2)
        {
            Dictionary<string, CachedAsset> assetDic1;
            Dictionary<string, CachedBundle> bundleDic1;
            Dictionary<string, CachedAsset> assetDic2;
            Dictionary<string, CachedBundle> bundleDic2;
            DumpInfo info1 = AssetDumpHelper.LoadDumpInfoFromFile(snapPath1);
            DumpInfo info2 = AssetDumpHelper.LoadDumpInfoFromFile(snapPath2);
            if (info1.assetLoaderType != info2.assetLoaderType)
            {
                Debug.LogError("Please choose snaps with same type.");
                return string.Empty;
            }
            LoadDicFromDumpInfo(AssetDumpHelper.LoadDumpInfoFromFile(snapPath1), out assetDic1, out bundleDic1);
            LoadDicFromDumpInfo(AssetDumpHelper.LoadDumpInfoFromFile(snapPath2), out assetDic2, out bundleDic2);

            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
            System.Text.StringBuilder strBuilderAdd = new System.Text.StringBuilder();
            System.Text.StringBuilder strBuilderReduce = new System.Text.StringBuilder();

            int newCount = 0;
            int releaseCount = 0;

            foreach (var pair in assetDic1)
            {
                CachedAsset asset;
                if(assetDic2.TryGetValue(pair.Key, out asset))
                {
                    if (asset.refCount > pair.Value.refCount)
                    {
                        strBuilderAdd.Append("[add] ");
                        strBuilderAdd.Append(pair.Key);
                        strBuilderAdd.Append(" : ");
                        strBuilderAdd.Append(asset.refCount - pair.Value.refCount);
                        strBuilderAdd.AppendLine();
                    }
                    else if (asset.refCount < pair.Value.refCount)
                    {
                        strBuilderReduce.Append("[reduce] ");
                        strBuilderReduce.Append(pair.Key);
                        strBuilderReduce.Append(" : ");
                        strBuilderReduce.Append(pair.Value.refCount - asset.refCount);
                        strBuilderReduce.AppendLine();
                    }
                }
                else
                {
                    strBuilderReduce.Append("[release] ");
                    strBuilderReduce.Append(pair.Key);
                    strBuilderReduce.Append(" : ");
                    strBuilderReduce.Append(pair.Value.refCount);
                    strBuilderReduce.AppendLine();
                    releaseCount++;
                }
            }
            foreach (var pair in assetDic2)
            {
                if (!assetDic1.ContainsKey(pair.Key))
                {
                    strBuilderAdd.Append("[new] ");
                    strBuilderAdd.Append(pair.Key);
                    strBuilderAdd.Append(" : ");
                    strBuilderAdd.Append(pair.Value.refCount);
                    strBuilderAdd.AppendLine();
                    newCount++;
                }

            }

            strBuilder.Append("##缓存的Asset\nNew count: ");
            strBuilder.Append(newCount);
            strBuilder.AppendLine();
            strBuilder.Append(strBuilderAdd);
            strBuilder.Append("\nRelease count: ");
            strBuilder.Append(releaseCount);
            strBuilder.AppendLine();
            strBuilder.Append(strBuilderReduce);
            strBuilder.Append("\n\n");
            strBuilderAdd.Clear();
            strBuilderReduce.Clear();
            newCount = 0;
            releaseCount = 0;

            if (info1.assetLoaderType == AssetLoaderType.AssetBundle)
            {
            foreach (var pair in bundleDic1)
            {
                CachedBundle bundle;
                if (bundleDic2.TryGetValue(pair.Key, out bundle))
                {
                    if (bundle.refCount > pair.Value.refCount)
                    {
                        strBuilderAdd.Append("[add] ");
                        strBuilderAdd.Append(pair.Key);
                        strBuilderAdd.Append(" : ");
                        strBuilderAdd.Append(bundle.refCount - pair.Value.refCount);
                        strBuilderAdd.AppendLine();
                    }
                    else if (bundle.refCount < pair.Value.refCount)
                    {
                        strBuilderReduce.Append("[reduce] ");
                        strBuilderReduce.Append(pair.Key);
                        strBuilderReduce.Append(" : ");
                        strBuilderReduce.Append(pair.Value.refCount - bundle.refCount);
                        strBuilderReduce.AppendLine();
                    }
                }
                else
                {
                    strBuilderReduce.Append("[release] ");
                    strBuilderReduce.Append(pair.Key);
                    strBuilderReduce.Append(" : ");
                    strBuilderReduce.Append(pair.Value.refCount);
                    strBuilderReduce.AppendLine();
                    releaseCount++;
                }
            }
            foreach (var pair in bundleDic2)
            {
                if (!bundleDic1.ContainsKey(pair.Key))
                {
                    strBuilderAdd.Append("[new] ");
                    strBuilderAdd.Append(pair.Key);
                    strBuilderAdd.Append(" : ");
                    strBuilderAdd.Append(pair.Value.refCount);
                    strBuilderAdd.AppendLine();
                    newCount++;
                }

            }

            strBuilder.Append("##缓存的Bundle\nNew count: ");
            strBuilder.Append(newCount);
            strBuilder.AppendLine();
            strBuilder.Append(strBuilderAdd);
            strBuilder.Append("\nRelease count: ");
            strBuilder.Append(releaseCount);
            strBuilder.AppendLine();
            strBuilder.Append(strBuilderReduce);
            }

            return strBuilder.ToString();
        }

        private void LoadDicFromDumpInfo(DumpInfo info, out Dictionary<string, CachedAsset> assetDic, out Dictionary<string, CachedBundle> bundleDic)
        {
            assetDic = new Dictionary<string, CachedAsset>();
            bundleDic = new Dictionary<string, CachedBundle>();
            foreach (var asset in info.cachedAssetList)
            {
                assetDic.Add(asset.path, asset);
            }
            foreach (var bundle in info.cachedBundleList)
            {
                bundleDic.Add(bundle.name, bundle);
            }
        }

        private string GetSnapPathByIndex(int index)
        {
            return Path.Combine(AssetDumpHelper.EditorSnapshotDirectory, _snapshotsFileList[index] + AssetDumpHelper.SnapshotExtension);
        }
    }
}
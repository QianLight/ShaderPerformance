/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Zeus.Framework.Asset
{
    public class AssetObserverRuntime
    {
        [Serializable]
        private class ListClass
        {
            public List<string> list;
            public ListClass(List<string> list)
            {
                this.list = list;
            }
        }

        private static AssetObserverRuntime _assetObserver;

        private HashSet<string> _assetHashSet;
        private List<string> _assetList;
        private bool _isSaved;
        private static bool _isObserving = false;
        public static bool IsObserving
        {
            get
            {
                return _isObserving;
            }
        }

        public static void Start()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (_assetObserver == null)
            {
                _assetObserver = new AssetObserverRuntime();
                _assetObserver.Init();
            }
            if (!_isObserving)
            {
                _isObserving = true;
            }
#endif
        }

        public static void Stop()
        {
            if (_isObserving)
            {
                _isObserving = false;
            }
        }

        public static void SaveObserver(string tag)
        {
            if (_assetObserver != null)
            {
                _assetObserver.Save(tag);
            }
        }

        public void Save(string tag)
        {
            if (!_isSaved)
            {
                if (SaveAssetList(_assetList, tag))
                {
                    _assetList.Clear();
                    _isSaved = true;
                }
            }
        }

        private bool SaveAssetList(List<string> list, string tag)
        {
            string _parentDirectory = Zeus.Core.FileSystem.OuterPackage.GetRealPath("AssetListLog");
            string path = Path.Combine(_parentDirectory, tag + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffff") + ".json");
            Zeus.Core.FileUtil.EnsureFolder(path);
            string content = JsonUtility.ToJson(new ListClass(list), true);
            File.WriteAllText(path, content);
            Debug.Log("Save successfully, file path: " + path);
            time = 0;
            return true;
        }

        private void _OnApplicationPause(bool pause)
        {
            Save("_Temp_");
            time = 0;
            Debug.Log("Autosave asset list.");
        }

        float time = 0;
        const float AutoSaveTimeSpanInMinutes = 5;
        private void _Update()
        {
            if (!_isObserving)
            {
                return;
            }
            time += Time.deltaTime;
            if (time > AutoSaveTimeSpanInMinutes * 60)
            {
                Save("_Temp_");
                time = 0;
            }
        }

        public void Init()
        {
            _assetList = new List<string>();
            _assetHashSet = new HashSet<string>();
            _isSaved = true;
            AssetManager.OnLoadEvent += _OnLoadAsset;
            Zeus.Core.ZeusCore.Instance.RegisterOnApplicationPause(_OnApplicationPause);
            Zeus.Core.ZeusCore.Instance.RegisterUpdate(_Update);
        }

        protected void _OnLoadAsset(string assetPath)
        {
            if (_isObserving)
            {
                if (_assetHashSet.Add(assetPath))
                {
                    _assetList.Add(assetPath);
                    _isSaved = false;
                }
            }
        }
    }
}
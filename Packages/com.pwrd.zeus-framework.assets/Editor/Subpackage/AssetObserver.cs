/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Zeus.Framework.Asset
{
    public class AssetObserver
    {
        private static AssetObserver _assetObserver;

        private HashSet<string> _assetHashSet;
        private List<string> _assetList;
        private bool _isSaved;
        private static bool _isObserving = true;
        public static bool IsObserving
        {
            get
            {
                return _isObserving;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitAssetObserver()
        {
            if (EditorPrefs.GetInt(SubpackageWindow.OBSERVING_STORE_KEY) == SubpackageWindow.IS_OBSERVING_INT)
            {
                _assetObserver = new AssetObserver();
                _assetObserver.Init();
                Start();
            }
        }

        public static void Start()
        {
            if (Application.isPlaying)
            {
                if (_assetObserver == null)
                {
                    _assetObserver = new AssetObserver();
                    _assetObserver.Init();
                }
            }
            if (!_isObserving)
            {
                _isObserving = true;
            }
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
                RelatedAssetHelper.AddRelatedAsset(ref _assetList);
                if (AssetListHelper.SaveAssetList(_assetList, tag))
                {
                    _assetList.Clear();
                    _isSaved = true;
                }
            }
        }

        private void _OnApplicationQuit()
        {
            if (!_isSaved)
            {
                int index;
                if (SubpackageWindow.Setting != null)
                {
                    index = SubpackageWindow.Setting.currentTag;
                }
                else
                {
                    SubpackageSetting setting = SubpackageSetting.LoadSetting();
                    index = setting.currentTag;
                }
                try
                {
                    Save(TagAsset.LoadTagAsset().TagListWithoutChild[index]);
                }
                catch(IndexOutOfRangeException e)
                {
                    Debug.LogError(e.ToString() + " Please rechoose tag in \"Zeus->Asset->分包\". And the log file will be saved with Tag \"_Temp_\", " +
                        "please rename it with correct name or delete it later.");
                    Save("_Temp_");
                }
                Debug.Log("Autosave asset list.");
            }
            Zeus.Core.ZeusCore.Instance.UnRegisterOnApplicationQuit(_OnApplicationQuit);
        }

        public void Init()
        {
            _assetList = new List<string>();
            _assetHashSet = new HashSet<string>();
            _isSaved = true;
            AssetManager.OnLoadEvent += _OnLoadAsset;
            Zeus.Core.ZeusCore.Instance.RegisterOnApplicationQuit(_OnApplicationQuit);
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
/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Object = UnityEngine.Object;

namespace Zeus.Framework.Asset
{
    public class ResourceAssetLoadTask
    {
        private string _assetPath;
        private Type _type;
        private List<AssetLoadCallbackData> _callbackList;
        private ResourceRequest _request;

        public ResourceAssetLoadTask(string assetPath, Type type)
        {
            _assetPath = assetPath;
            _type = type;
            _callbackList = new List<AssetLoadCallbackData>();
        }

        public void Start()
        {
            _request = Resources.LoadAsync(_assetPath, _type);
        }

        public bool IsDone()
        {
            if (_request != null &&  _request.isDone)
            {
                if(_request.asset == null)
                {
                    Debug.LogError("Asset not found! Please check asset path: " + _assetPath);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public Object AssetObject
        {
            get { return _request.asset; }
        }

        public string AssetPath { get { return _assetPath; } }

        public void AddCallback(Action<IAssetRef, object> callback, object param)
        {
            _callbackList.Add(new AssetLoadCallbackData(callback, param));
        }

        public void ExecuteCallBack(IAssetRef assetRef)
        {
            for (int i = 0; i < _callbackList.Count; i++)
            {
                _callbackList[i].Execute(assetRef);
            }
            _callbackList.Clear();
        }
    }
}


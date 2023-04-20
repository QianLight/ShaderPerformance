/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Zeus.Framework.Asset
{
	public class AssetGroupLoader
	{
		private Action<AssetGroupLoader, object> _callback;
		private bool _isStarted;
		Dictionary<string, IAssetRef> _assetDict = new Dictionary<string, IAssetRef>();
		private int _assetCount;
		private object _param;
        public object Param
        {
            set { _param = value; }
        }

		public AssetGroupLoader(Action<AssetGroupLoader, object> callback, object param)
		{
			_isStarted = false;
			_assetCount = 0;
			_callback = callback;
			_param = param;
		}

		public void AddAssetReq(string path, Type type, string tag)
		{
			if(_assetDict.ContainsKey(tag))
			{
				Debug.LogError("GroupLoader duplicated tag");
				return;
			}
			_assetDict.Add(tag, null);
			AssetManager.LoadAssetAsync(path, type, this.LoadAssetAction, tag);
		}

		public IAssetRef GetAsset(string tag)
		{
			IAssetRef assetRef = null;
			_assetDict.TryGetValue(tag, out assetRef);
			return assetRef;
		}

		public void Load()
		{
			Debug.Assert(!_isStarted, "GroupLoader has started");
			_isStarted = true;
			CheckStatus();
		}

		public void Clear()
		{
			foreach(var pair in _assetDict)
			{
				if(pair.Value != null)
				{
					pair.Value.Release();
				}
			}
			_assetDict.Clear();
		}

		private void LoadAssetAction(IAssetRef assetRef, object param)
		{
			_assetCount++;
			var tag = param as string;
			_assetDict[tag] = assetRef;
			if(assetRef != null && assetRef.AssetObject != null)
			{
				assetRef.Retain();
			}
			CheckStatus();
		}

		private void CheckStatus()
		{
			if(_isStarted && _assetCount == _assetDict.Count)
			{
				_callback(this, _param);
			}
		}

        public Dictionary<string,IAssetRef> GetDic()
        {
            return _assetDict;
        }
	}
}


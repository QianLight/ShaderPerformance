/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
//#define ZEUS_LUA_SUPPORT
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus.Framework.Asset
{
    public class AssetGroup
    {
        Dictionary<string, IAssetRef> _assetRefDict = new Dictionary<string, IAssetRef>();

        private class CallbackParam
        {
            public Action<UnityEngine.Object, object> callback;
            public object param;
        }

        private class CallbackParam2
        {
            public int total;
            public int count;
            public Action<bool, int> callback;
        }

        public void Release()
        {
            foreach(KeyValuePair<string, IAssetRef> pair in _assetRefDict)
            {
                pair.Value.Release();
            }
            _assetRefDict.Clear();
        }

        public UnityEngine.Object LoadAsset(string assetPath, Type type)
        {
            Debug.Assert(assetPath != null);
            IAssetRef assetRef = null;
            if (_assetRefDict.TryGetValue(assetPath, out assetRef))
            {
                return assetRef.AssetObject;
            }
            assetRef = AssetManager.LoadAsset(assetPath, type==null ? typeof(UnityEngine.Object): type);
            if(assetRef == null)
            {
                return null;
            }
            if (!_assetRefDict.ContainsKey(assetPath))
            {
                _assetRefDict.Add(assetPath, assetRef);
                assetRef.Retain();
            }
            return assetRef.AssetObject;
        }

        public void LoadAssetAsync(string assetPath, Type type, Action<UnityEngine.Object, object> callback, object param)
        {
            LoadAssetAsyncInternal(assetPath, type, callback, param, false);
        }

#if ZEUS_LUA_SUPPORT
        public void LoadPrefabTableAsync(LuaInterface.LuaTable assetTable, Action<bool, int> callback)
        {
            List<string> assetList = new List<string>(assetTable.Length);
            List<Type> assetTypeList = new List<Type>(assetTable.Length);
            for (int i = 1; i <= assetTable.Length; i++)
            {
                assetList.Add(assetTable[i] as string);
                assetTypeList.Add(typeof(UnityEngine.GameObject));
            }
            this.LoadAssetListAsync(assetList, assetTypeList, callback);
        }

        public void LoadAssetTableAsync(LuaInterface.LuaTable assetTable, Action<bool, int> callback)
        {
            List<string> assetList = new List<string>(assetTable.Length);
            List<Type> assetTypeList = new List<Type>(assetTable.Length);
            for (int i = 1; i <= assetTable.Length; i++)
            {
                if(assetTable[i] is string)
                {
                    assetList.Add(assetTable[i] as string);
                    assetTypeList.Add(typeof(UnityEngine.Object));
                }
                else
                {
                    LuaInterface.LuaTable table = assetTable[i] as LuaInterface.LuaTable;
                    string assetPath = table["assetPath"] as string;
                    Type assetType = table["assetType"] as Type;
                    Debug.Assert(assetPath != null, "assetPath is null");
                    Debug.Assert(assetType != null, "assetType is null");
                    assetList.Add(assetPath);
                    assetTypeList.Add(assetType);
                }
            }
            this.LoadAssetListAsync(assetList, assetTypeList, callback);
        }
#endif
        public void LoadAssetListAsync(List<string> assetList, List<Type> typeList, Action<bool, int> callback)
        {
            CallbackParam2 callbackParam = new CallbackParam2();
            callbackParam.total = assetList.Count;
            callbackParam.count = 0;
            callbackParam.callback = callback;
            for(int i = 0; i < assetList.Count; i++)
            {
                AssetManager.LoadAssetAsync(assetList[i], typeList[i], this.AssetReadyAction2, callbackParam);
            }
        }

        public void LoadAssetAsyncUrgent(string assetPath, Type type, Action<UnityEngine.Object, object> callback, object param)
        {
            LoadAssetAsyncInternal(assetPath, type, callback, param, true);
        }

        private void LoadAssetAsyncInternal(string assetPath, Type type, Action<UnityEngine.Object, object> callback, object param, bool isUngent)
        {
            IAssetRef assetRef = null;
            if (_assetRefDict.TryGetValue(assetPath, out assetRef))
            {
                callback(assetRef.AssetObject, param);
            }
            else
            {
                CallbackParam callbackParam = new CallbackParam();
                callbackParam.param = param;
                callbackParam.callback = callback;
                if (isUngent)
                {
                    AssetManager.LoadAssetUrgent(assetPath, type, this.AssetReadyAction, callbackParam);
                }
                else
                {
                    AssetManager.LoadAssetAsync(assetPath, type, this.AssetReadyAction, callbackParam);
                }
            }
        }

        private void AssetReadyAction(IAssetRef assetRef, object param)
        {
            if (!_assetRefDict.ContainsKey(assetRef.AssetPath))
            {
                assetRef.Retain();
                _assetRefDict.Add(assetRef.AssetPath, assetRef);
            }
            CallbackParam callbackParam = param as CallbackParam;
            callbackParam.callback(assetRef.AssetObject, callbackParam.param);
        }

        private void AssetReadyAction2(IAssetRef assetRef, object param)
        {
            if (!_assetRefDict.ContainsKey(assetRef.AssetPath))
            {
                assetRef.Retain();
                _assetRefDict.Add(assetRef.AssetPath, assetRef);
            }
            CallbackParam2 callbackParam = param as CallbackParam2;
            callbackParam.count++;
            callbackParam.callback(callbackParam.count == callbackParam.total, callbackParam.count);
        }
    }
}

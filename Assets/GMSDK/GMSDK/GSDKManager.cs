using System.Collections.Generic;
using UnityEngine;

namespace GMSDK
{
    public class GMSDKManager : MonoBehaviour
    {
        private static bool _init = false;
        private static GameObject _container = null;
        private static GMSDKManager _instance = null;
        private Dictionary<int,GMSDKObject> _gmsdkObjects = new Dictionary<int, GMSDKObject>();
        public static GMSDKManager Instance
        {
            get
            {
                if (_init) return _instance;
                lock (typeof(GMSDKManager))
                {
                    if (_init) return _instance;
                    _init = true;
                    if (_container == null)
                    {
                        _container = new GameObject();
                        DontDestroyOnLoad(_container);
                    }
                    _instance = _container.AddComponent(typeof(GMSDKManager)) as GMSDKManager;
                }
                return _instance;
            }
        }

        #region Operate GMSDK Object
        
        // 添加一个GSDKObject
        public void AddObject(GMSDKObject obj)
        {
            _gmsdkObjects.Add(obj.ID,obj);
        }

        // 移除一个GSDKObject
        public void RemoveObject(GMSDKObject obj)
        {
            if (obj == null)
            {
                return;
            }

            if (_gmsdkObjects.ContainsKey(obj.ID))
            {
                _gmsdkObjects.Remove(obj.ID);
            }
        }
        
        // 清除所有GSDKObjects
        public void ClearObjects()
        {
            _gmsdkObjects.Clear();
        }
        
        #endregion

        #region Mono Events
        
        public void Update()
        {
            var enumerator = _gmsdkObjects.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.Value.Update();
            }
        }

        public void OnApplicationPause(bool pauseStatus)
        {
            var enumerator = _gmsdkObjects.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.Value.OnApplicationPause(pauseStatus);
            }
        }

        public void OnDisable()
        {
            var enumerator = _gmsdkObjects.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.Value.OnDisable();
            }
        }

        public void OnApplicationQuit()
        {
            var enumerator = _gmsdkObjects.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.Value.OnApplicationQuit();
            }
            ClearObjects();
        }
        
        #endregion
    }
}
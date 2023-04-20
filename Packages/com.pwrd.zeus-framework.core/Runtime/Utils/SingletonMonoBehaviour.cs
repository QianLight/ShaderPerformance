/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Zeus.Framework
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object syslock = new object();

        public static T GetInstance()
        {
            if (_instance == null)
            {
                lock (syslock)
                {
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).ToString();
                        GameObject.DontDestroyOnLoad(obj);
                        _instance = obj.AddComponent<T>();
                    }
                }
            }
            return _instance;
        }

        public abstract void Init();

        public abstract void Final();
    }

}

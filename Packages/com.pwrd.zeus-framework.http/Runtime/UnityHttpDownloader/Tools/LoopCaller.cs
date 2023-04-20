/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#define OpenHttpDownloadLog
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Net;
using System.Threading;
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace Zeus.Framework.Http.UnityHttpDownloader.Tool
{
    class LoopCaller : MonoBehaviour
    {
        public Action OnUpdate;
        public Action<bool> OnFocus;
        public Action OnQuit;
        public bool WaitDestroy = false;
        private bool _destroyed = false;
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (WaitDestroy && !_destroyed)
            {
                _destroyed = true;
                GameObject.Destroy(gameObject);
            }
            if (!WaitDestroy && OnUpdate != null)
            {
                OnUpdate();
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!WaitDestroy && OnFocus != null)
            {
                OnFocus(focus);
            }
        }

        private void OnApplicationQuit()
        {
            if (!WaitDestroy && OnQuit != null)
            {
                OnQuit();
            }
        }
    }
}

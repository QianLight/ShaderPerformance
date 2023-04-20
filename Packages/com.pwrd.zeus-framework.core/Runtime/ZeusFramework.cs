/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeus.Core;
using Zeus.Core.FileSystem;

namespace Zeus.Framework
{
    public static class ZeusFramework
    {
        private static bool _IsStart = false;
        private static List<ILauncher> _ListLauncher = new List<ILauncher>();
        private static Action _OnComplete = null;


        static ZeusFramework()
        {
            _ListLauncher.Add(new FileSystemLauncher());
        }

        public static ZeusLaunchErrorFlag Start()
        {
            if (_IsStart)
            {
                throw new ZeusException("ZeusFramework already started.");
            }

            _IsStart = true;
            AndroidJNIUtil.InitMainThreadID();
            return ExecuteLauncher();
        }

        public static ZeusLaunchErrorFlag Start(Action onComplete)
        {
            if (_IsStart)
            {
                throw new ZeusException("ZeusFramework already started.");
            }

            _IsStart = true;
            _OnComplete = onComplete;

            return ExecuteLauncher();
        }

        private static ZeusLaunchErrorFlag ExecuteLauncher()
        {
            ZeusLaunchErrorFlag flag = ZeusLaunchErrorFlag.None;
            for (int i = 0; i < _ListLauncher.Count; i++)
            {
                ZeusCore.Log("ZeusFramework", string.Format("{0} executing.", _ListLauncher[i].GetType().Name));
                flag |= _ListLauncher[i].Execute();
                ZeusCore.Log("ZeusFramework", string.Format("{0} done.", _ListLauncher[i].GetType().Name));
            }

            ZeusCore.Log("ZeusFramework", "All launcher has been completed.");

            // 触发完成。
            if (_OnComplete != null)
            {
                _OnComplete();
                _OnComplete = null;
            }
            return flag;
        }
    }
}

/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;


namespace Zeus.Framework.Asset
{
    internal class SceneLoadCommand : SceneCommandBase
    {
        LoadSceneMode _loadMode;
        Action<bool, float, object> _callback;
        object _param;

        public SceneLoadCommand(string scenePath, LoadSceneMode loadMode, Action<bool, float, object> callback, object param):base(scenePath, CommandType.AsyncLoad)
        {
            _loadMode = loadMode;
            _callback = callback;
            _param = param;
        }

        public SceneLoadCommand(string scenePath, LoadSceneMode loadMode) : base(scenePath, CommandType.CoroutineLoad)
        {
            _loadMode = loadMode;


        }
        public LoadSceneMode LoadMode { get { return _loadMode; } }

        public override void FinishCommand()
        {
            base.FinishCommand();
            if(_callback != null)
            {
                try
                {
                    _callback.Invoke(true, 1.0f, _param);
                }
                catch
                {
                    //do nothing
                }
            }
        }
    }
}


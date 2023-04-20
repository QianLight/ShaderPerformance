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
    internal class SceneUnloadCommand : SceneCommandBase
    {
        Action<bool, float, object> _callback;
        object _param;
        public SceneUnloadCommand(string scenePath, Action<bool, float, object> callback, object param) : base(scenePath, CommandType.AsyncUnload)
        {
            _callback = callback;
            _param = param;
        }

        public SceneUnloadCommand(string scenePath) : base(scenePath, CommandType.CoroutineUnload)
        {

        }

        public override void FinishCommand()
        {
            base.FinishCommand();
            if (_callback != null)
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


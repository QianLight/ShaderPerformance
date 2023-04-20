/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

namespace Zeus.Framework.Asset
{
    abstract class SceneLoadTaskBase
    {
        public abstract void UpdateLoadProgress();
        public abstract bool IsDone();
        public abstract void ExecuteCallBack();
    }
}


/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using UnityEngine;

namespace Zeus.Core
{
    public interface ILauncher
    {
        ZeusLaunchErrorFlag Execute();
    }
}

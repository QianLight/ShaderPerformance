using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Zeus.Core
{
    [Flags]
    public enum ZeusLaunchErrorFlag
    {
        None = 0,
        //一键修复功能清理包外文件时发生异常
        CLEAR_OUTPACKAGE_ERROR = 1,
        //覆盖安装时清理冗余文件出错
        COVERLY_INSTALLATION_ERROR = 1 << 1,

    }
}



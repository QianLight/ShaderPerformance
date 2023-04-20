/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;

namespace Zeus.Framework.Http.UnityHttpDownloader
{
    [Flags]
    public enum ErrorType
    {
        None = 0,

        NetError = 1 << 0,//网络异常　　　　　　　　　　

        IOException = 1 << 1,

        Exception = 1 << 2,

        CombineFile = 1 << 3,//合并文件异常

        MissingFile = 1 << 4,//找不到文件404

        HardDiskFull = 1 << 5,//存储空间不足

        CheckFail = 1 << 6//下载下来的文件校验失败
    }
}
/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Zeus.Framework.Http.UnityHttpDownloader;

namespace Zeus.Framework.Asset
{
    [Flags]
    public enum AssetLoadErrorType
    {
        None = ErrorType.None,
       
        //远程加载bundle会产生的错误类型
        //网络错误，无法下载资源
        NetError = ErrorType.NetError,　　　　　　　　
        IOException = ErrorType.IOException,
        Exception = ErrorType.Exception,
        //多线程下载之后合并文件出错
        CombineFile = ErrorType.CombineFile,
        //资源在CDN上不存在
        MissingFile = ErrorType.MissingFile,
        //磁盘已满，无法下载资源
        HardDiskFull = ErrorType.HardDiskFull,
        //下载完成后校验失败
        MD5Error = ErrorType.CheckFail,
        //资源不存在，常见大小写拼写错误
        AssetNotExist = 1 << 12,
        //bundle加载失败
        BundleLoadFailed = 1 << 13,
        //从bundle中加载Asset失败
        AssetLoadFailed = 1 << 14,

        //同步加载资源，bundle不存在
        AssetSyncLoadBundleNotReady = 1 << 15,
        //其他错误
        Others = 1 << 16
    }
}

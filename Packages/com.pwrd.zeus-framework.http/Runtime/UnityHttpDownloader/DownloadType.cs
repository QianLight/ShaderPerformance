/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus.Framework.Http.UnityHttpDownloader
{
    public enum DownloadType
    {
        WholeFile = 0,//下载完整文件
        PartialFile = 1,//下载部分文件
    }
}
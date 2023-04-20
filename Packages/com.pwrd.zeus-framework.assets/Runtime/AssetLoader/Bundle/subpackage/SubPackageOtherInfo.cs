/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;

namespace Zeus.Framework
{
    [Serializable]
    public class SubPackageOtherInfo
    {
        public readonly string Path;
        public readonly string ChunkFile;

        public SubPackageOtherInfo(string path, string chunk)
        {
            this.Path = path;
            this.ChunkFile = chunk;
        }
    }
}
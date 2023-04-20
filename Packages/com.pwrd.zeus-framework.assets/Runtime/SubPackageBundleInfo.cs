/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;

namespace Zeus.Framework
{
    [Serializable]
    public class SubPackageBundleInfo
    {
        public readonly string BundleName;
        public readonly int BundleCrc32;
        public readonly string ChunkFile;
        public readonly uint BundleSize;
        public readonly uint ChunkFrom;

        public SubPackageBundleInfo(string name, int crc32, uint size, string chunk, uint from)
        {
            this.BundleName = name;
            this.BundleCrc32 = crc32;
            this.BundleSize = size;
            this.ChunkFile = chunk;
            this.ChunkFrom = from;
        }
    }
}
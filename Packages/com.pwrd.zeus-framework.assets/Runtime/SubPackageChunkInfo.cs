/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
namespace Zeus.Framework.Asset
{
    [System.Serializable]
    public class SubPackageChunkInfo
    {
        string fileName;
        public string FileName
        {
            get
            {
                return fileName;
            }
        }
        int crc32;
        public int Crc32
        {
            get
            {
                return crc32;
            }
        }
        uint fileSize;
        public uint FileSize
        {
            get
            {
                return fileSize;
            }
        }
        //压缩算法
        BundleCompressMethod compressMethod;
        public BundleCompressMethod CompressMethod
        { 
            get
            {
                return compressMethod;
            }
        }

        public SubPackageChunkInfo(string fileName, int crc32, uint fileSize, BundleCompressMethod method)
        {
            this.fileName = fileName;
            this.crc32 = crc32;
            this.fileSize = fileSize;
            compressMethod = method;
        }
    }
}


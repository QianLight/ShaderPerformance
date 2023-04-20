/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;

namespace Zeus.Framework.Http.UnityHttpDownloader
{
    [Serializable]
    public class CheckAlgorithm
    {
        [UnityEngine.SerializeField]
        public CheckAlgorithmType checkAlgorithmType;
        public string md5;
        public int crc32;

        public CheckAlgorithm(CheckAlgorithmType type,string value)
        {
            checkAlgorithmType = type;
            switch (checkAlgorithmType)
            {
                case CheckAlgorithmType.Md5:
                    md5 = value;
                    break;
            }
        }

        public CheckAlgorithm(CheckAlgorithmType type, int value)
        {
            checkAlgorithmType = type;
            switch (checkAlgorithmType)
            {
                case CheckAlgorithmType.Crc32:
                    crc32 = value;
                    break;
            }
        }



        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                CheckAlgorithm other = obj as CheckAlgorithm;
                if (other != null && checkAlgorithmType == other.checkAlgorithmType)
                {
                    if (checkAlgorithmType == CheckAlgorithmType.Md5)
                    {
                        return md5.Equals(other.md5);
                    }
                    else if(checkAlgorithmType == CheckAlgorithmType.Crc32)
                    {
                        return crc32 == other.crc32;
                    }
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            switch (checkAlgorithmType)
            {
                case CheckAlgorithmType.Md5:
                    return md5.GetHashCode();
                case CheckAlgorithmType.Crc32:
                    return crc32.GetHashCode();
            }
            return base.GetHashCode();
        }
    }
}

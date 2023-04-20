/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
namespace Zeus.Core
{
    public static class ZeusConstant
    {
        public const int KB = 1024;
        public const int MB = 1024 * 1024;
        public const int GB = 1024 * 1024 * 1024;

#if UNITY_ANDROID && !UNITY_EDITOR
        public const string ASSETBUNDLE_ROOT_DIR = "Bundles/Android";
#elif UNITY_IOS && !UNITY_EDITOR
        public const string ASSETBUNDLE_ROOT_DIR = "Bundles/iOS";
#else
        public const string ASSETBUNDLE_ROOT_DIR = "Bundles/Windows";
#endif

        //lua文件64位字节码文件后缀名
        public static readonly string LuaByte64Extension = ".64bytes";
        //lua文件32位字节码文件后缀名
        public static readonly string LuaByte32Extension = ".32bytes";
    }
}

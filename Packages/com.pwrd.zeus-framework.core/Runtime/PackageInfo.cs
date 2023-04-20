/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_EDITOR
namespace Zeus.Framework.Core
{
    public static class PackageInfo
    {
        private static string packageName = "com.pwrd.zeus-framework.core";
        public static string PackageName { get {return packageName;}  }
        public static string PackageFullPath
        {
            get
            {
                return PackageUtility.GetPackageFullPath(PackageName);
            }
        }

        public static string PackageRelativePath
        {
            get
            {
                return PackageUtility.GetPackageRelativePath(PackageName);
            }
        }
    }
}
#endif
/*******************************************************************
* Copyright © 2017—2022 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.IO;

namespace Zeus.Framework.Asset
{
    public static class AssetPathHelper
    {
        private static readonly string resourcesDirectory;
        private static readonly int resourcesDirectoryLength;
        private static readonly bool removeFileExtension;

        static AssetPathHelper()
        {
            AssetManagerSetting assetManagerSetting = AssetManagerSetting.LoadSetting();
            resourcesDirectory = assetManagerSetting.bundleLoaderSetting.resourcesRootFolder.Replace("\\", "/");
            resourcesDirectoryLength = resourcesDirectory.EndsWith("/") ? resourcesDirectory.Length : resourcesDirectory.Length + 1;
            removeFileExtension = assetManagerSetting.removeFileExtension;
        }

        public static string GetShortPath(string path)
        {
            string shortPath;
            if (path.Contains(resourcesDirectory))
            {
                shortPath = path.Substring(path.IndexOf(resourcesDirectory, StringComparison.Ordinal) + resourcesDirectoryLength).Replace("\\", "/");
                if (removeFileExtension)
                {
                    string extension = Path.GetExtension(shortPath);
                    if (!string.IsNullOrEmpty(extension))
                        shortPath = shortPath.Replace(extension, "");
                }
            }
            else if (path.Contains("StreamingAssets"))
            {
                shortPath = path.Substring(path.IndexOf("StreamingAssets", StringComparison.Ordinal)).Replace("\\", "/");
            }
            else
            {
                shortPath = path.Replace("\\", "/");
                if (removeFileExtension)
                {
                    string extension = Path.GetExtension(shortPath);
                    if (!string.IsNullOrEmpty(extension))
                        shortPath = shortPath.Replace(extension, "");
                }
            }

            return shortPath;
        }
    }
}
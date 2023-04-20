/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEditor;

namespace Zeus.Framework
{
    public static class EditorAssetBundleUtils
    {
        public static string GetPathRoot(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    return "Bundles/Android";
                case BuildTarget.iOS:
                    return "Bundles/iOS";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Bundles/Windows";
#if UNITY_5
                case BuildTarget.StandaloneOSXUniversal:
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
#else
                case BuildTarget.StandaloneOSX:
#endif
                    return "Bundles/OSX";
                default:
                    throw new ZeusException(string.Format("'{0}' don't have path root", buildTarget.ToString()));
            }
        }

        public static string GetPathRoot()
        {
#if UNITY_ANDROID
            BuildTarget target = BuildTarget.Android;
#elif UNITY_IOS
            BuildTarget target = BuildTarget.iOS;
#elif UNITY_STANDALONE_WIN
            BuildTarget target = BuildTarget.StandaloneWindows64;
#else
#if UNITY_5
            BuildTarget target = BuildTarget.StandaloneOSXUniversal:
#else
            BuildTarget target = BuildTarget.StandaloneOSX;
#endif
#endif

            return GetPathRoot(target);
        }
    }
}

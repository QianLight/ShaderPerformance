/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_EDITOR && UNITY_ANDROID

using UnityEditor;
using UnityEngine;
using Zeus.Build;

namespace Zeus.BuildAndroidProject
{
    /// <summary>
    /// build Android 打包前操作
    /// </summary>
    public class BeforeBuild_Android : IBeforeBuild
    {
        //[BeforeBuild]
        public void OnBeforeBuild(BuildTarget target, string outputPath)
        {
            if (target != BuildTarget.Android)
                return;

            Debug.Log("BeforeBuild_Android");

            //EditorUserBuildSettings.androidETC2Fallback = AndroidETC2Fallback.Quality32Bit;
            //EditorUserBuildSettings.buildAppBundle
            //EditorUserBuildSettings.wsaBuildAndRunDeployTarget
            //EditorUserBuildSettings.compressFilesInPackage
            //EditorUserBuildSettings.wsaMinUWPSDK
            EditorUserBuildSettings.androidBuildSystem = GetAndroidBuildSystemSetting();
        }

        private static AndroidBuildSystem GetAndroidBuildSystemSetting()
        {
            ZeusObbBuild obbBuild = GlobalBuild.Default.AndroidBuildObb;
            string obbBuildStr = string.Empty;
            if (CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.ANDROID_BUILD_OBB, ref obbBuildStr))
            {
                obbBuild = (ZeusObbBuild)System.Enum.Parse(typeof(ZeusObbBuild), obbBuildStr);
            }
            if (obbBuild == ZeusObbBuild.Unity)
            {
                PlayerSettings.Android.useAPKExpansionFiles = true;
            }
            else
            {
                PlayerSettings.Android.useAPKExpansionFiles = false;
            }
#if UNITY_2019_1_OR_NEWER
            return AndroidBuildSystem.Gradle;
#else
            var buildSystemSetting = CommandLineArgs.GetString(GlobalBuild.CmdArgsKey.ANDROID_BUILD_SYSTEMSETTING, GlobalBuild.Default.AndroidBuildSystemSetting);

            switch (buildSystemSetting)
            {
                case "Internal":
                    return AndroidBuildSystem.Internal;
                case "Gradle":
                    return AndroidBuildSystem.Gradle;
                default:
                    Debug.LogError("AndroidBuildSystem not set");
                    return AndroidBuildSystem.Internal;
            }
#endif
        }
    }
}
#endif
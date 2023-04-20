/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Zeus.Build.AAB
{
    internal static class AABGlobal
    {
        /// <summary>
        /// 白名单配置的路径
        /// </summary>
        public static string AABWhiteListPath
        {
            get
            {
                return Path.Combine(Application.dataPath, "ZeusSetting/EditorSetting", "AABWhiteList.json");
            }
        }

        /// <summary>
        /// 不放到InstallTime的文件的临时存储路径
        /// </summary>
        public const string InstallTimeTmpFolder = "DonNotMoveToInstalTime";
        public const string ZeusCorePath = "Packages/com.pwrd.zeus-framework.core";

#if UNITY_2019_3_OR_NEWER
        public const string StreamingAssetsPathInAPK = "unityLibrary/src/main/assets";
#else
        public const string StreamingAssetsPathInAPK = "src/main/assets";
#endif
        public const string InstallTimeAssetpackPathInAPK = "install_time/src/main/assets";
        public const string InstallTimeBuildGradlePathInAPK = "install_time/build.gradle";
        public const string InstallTimeManifestPathInAPK = "install_time/src/main/AndroidManifest.xml";
        public const string InstallTimePresetBuildGradlePath = ZeusCorePath + "/Editor/AAB/res/install_time_build.gradle.txt";
        public const string InstallTimePresetBuildGradle3Path = ZeusCorePath + "/Editor/AAB/res/install_time_build.gradle.3.txt";
        public const string InstallTimePresetManifestPath = ZeusCorePath + "/Editor/AAB/res/install_time_AndroidManifest.txt";
        public const string LauncherBuildGradlePathInAPK = "launcher/build.gradle";
        public const string ProjectBuildGradlePathInAPK = "build.gradle";
        public const string ProjectSettingGradlePathInAPK = "settings.gradle";
        public const string ProjectGradlePropertiesPathInAPK = "gradle.properties";

        /// <summary>
        /// 为了防止被打到包的临时的StreamingAssets路径
        /// </summary>
        public const string TmpStreamingAssetsFolder = "~StreamingAssets";
        public const string StreamingAssetsFolder = "StreamingAssets";
        /// <summary>
        /// InstallTime的assetPack的名称，需要和热更后台约定好保持一致
        /// </summary>
        public const string InstallTimeAssetpackName = "install_time";
        public const string APKExtension = ".apk";
        public const string APKSExtension = ".apks";
        public const string AABExtension = ".aab";


        /// <summary>
        /// bin是程序文件，ZeusSetting的读取配置可能会很提前，本身不大，防止意外错误也不拷贝到外面
        /// </summary>
        public static List<string> ExludeFoldersForInstallTime = new List<string>{"bin", "ZeusSetting" };

        public const string LastAABPathSaveKey = "Zeus.Build.AAB.AABGlobal.LastAABPathSaveKey";
    }
}

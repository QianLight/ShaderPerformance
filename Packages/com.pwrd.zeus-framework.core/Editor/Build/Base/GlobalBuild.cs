/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Zeus.Framework;

namespace Zeus.Build
{
    public class GlobalBuild
    {
        /// <summary>
        /// Command Line Args keys
        /// </summary>
        public class CmdArgsKey
        {
            /// <summary>
            /// 此值只能在ModifyPlayerSettings修改。
            /// </summary>
            public const string PLATFORM = "PLATFORM";
            public const string MODIFY_DEFINE_SYMBOL = "MODIFY_DEFINE_SYMBOL";
            /// <summary>
            /// 此值只能在ModifyPlayerSettings修改。
            /// </summary>
            public const string PACKAGE_NAME = "PACKAGE_NAME";
            public const string SUBPACKAGE_NAME = "SUBPACKAGE_NAME";
            /// <summary>
            /// 此值只能在ModifyPlayerSettings修改。
            /// </summary>
            public const string OUTPUT_PATH = "OUTPUT_PATH";
            
            public const string IS_BUILD_AB = "IS_BUILD_AB";
            public const string IS_CLEAR_AB_CACHE = "IS_CLEAR_AB_CACHE";
            public const string IS_DEVELOPMENT_BUILD = "IS_DEVELOPMENT_BUILD";
            public const string SCRIPTING_BACKEND = "SCRIPTING_BACKEND";
            public const string ANDROID_TARGET_ARCHITECTURES = "ANDROID_TARGET_ARCHITECTURES";
            public const string IS_AUTOCONNECT_PROFILER = "IS_AUTOCONNECT_PROFILER";
            public const string IS_ALLOW_DEBUGGING = "IS_ALLOW_DEBUGGING";
            public const string IS_DEEP_PROFILING = "IS_DEEP_PROFILING";
            public const string IS_BUILD_ANDROID_PROJECT = "IS_BUILD_ANDROID_PROJECT";
            public const string LUA_ENCRYPT_BUILD = "LUA_ENCRYPT_BUILD";
            public const string IS_COMBINE_FILE = "IS_COMBINE_FILE";
            public const string IS_PACKAGE_PATCH = "IS_PACKAGE_PATCH";
            public const string GENERATE_SYMBOL_FILE = "GENERATE_SYMBOL_FILE";
            public const string IS_GENERATE_SYMBOL_FILE = "IS_GENERATE_SYMBOL_FILE";

            /// <summary>
            /// 当输出Android工程或者iOS工程的时候，清除(True)还是覆盖(False)输出目录上已经存在的工程
            /// </summary>
            public const string IS_DELETE_PROJECT_OUTPUT_FOLDER = "IS_DELETE_PROJECT_OUTPUT_FOLDER";
            public const string IS_OPEN_OUTPUT_FOLDER = "IS_OPEN_OUTPUT_FOLDER";

            //Asset
            public const string SUBPACKAGE_SERVER_URL = "ASSET_SUBPACKAGE_SERVER_URL";
            public const string USE_BUNDLELOADER = "ASSET_USE_BUNDLELOADER";
            public const string USE_SUBPACKAGE = "ASSET_USE_SUBPACKAGE";
            public const string SKIP_EXPORT_SUBPACKAGE = "ASSET_SKIP_EXPORT_SUBPACKAGE";
            public const string UPLOAD_BUNDLE = "ASSET_UPLOAD_BUNDLE";
            public const string GENERATE_BUNDLE_SEQUENCE = "ASSET_GENERATE_BUNDLE_SEQUENCE";
            public const string BUNDLE_COMPRESS_METHOD = "ASSET_BUNDLE_COMPRESS_METHOD";
            public const string MARK_TAG_TO_FIRST_PACKAGE = "MARK_TAG_TO_FIRST_PACKAGE";

            //首包填充
            public const string IS_FILL_FIRSTPACKAGE_ANDROID = "IS_FILL_FIRSTPACKAGE_ANDROID";
            public const string FILL_FIRSTPACKAGE_SIZE_ANDROID = "FILL_FIRSTPACKAGE_SIZE_ANDROID";
            public const string IS_FILL_FIRSTPACKAGE_IOS = "IS_FILL_FIRSTPACKAGE_IOS";
            public const string FILL_FIRSTPACKAGE_SIZE_IOS = "FILL_FIRSTPACKAGE_SIZE_IOS";

            /// <summary>
            /// 是否支持后台下载，false会在打包的时候剔除保活模块，主要指的是构建阶段
            /// </summary>
            public const string SUPPORT_BACKGROUND_DOWNLOAD = "SUPPORT_BACKGROUND_DOWNLOAD";
            /// <summary>
            /// 是否允许后台下载，主要指运行时，需要在构建阶段开启 SUPPORT_BACKGROUND_DOWNLOAD 才能生效，否则无意义
            /// </summary>
            public const string SUBPACKAGE_BACKGROUND_DOWNLOAD = "ASSET_SUBPACKAGE_BACKGROUND_DOWNLOAD";

            public const string APPLICATION_IDENTIFIER = "APPLICATION_IDENTIFIER";
            public const string BUNDLE_VERSION = "BUNDLE_VERSION";
            /// <summary>
            /// Android中的bundleVersionCode，IOS中的bundleNumber.
            /// </summary>
            public const string BUNDLE_NUMBER = "BUNDLE_NUMBER";
            public const string PRODUCT_NAME = "PRODUCT_NAME";
            public const string COMPANY_NAME = "COMPANY_NAME";
            //XCode
            public const string APPLE_ENABLE_AUTOMATIC_SIGNING = "XCODE_APPLE_ENABLE_AUTOMATIC_SIGNING";
            public const string PROVISIONING_PROFILE_SPECIFIER = "XCODE_PROVISIONING_PROFILE_SPECIFIER";
            public const string CODE_SIGN_IDENTITY = "XCODE_CODE_SIGN_IDENTITY";
            public const string PROVISIONING_STYLE = "XCODE_PROVISIONING_STYLE";
            public const string DEVELOPMENT_TEAM = "XCODE_DEVELOPMENT_TEAM";

            //Android
            public const string ANDROID_BUILD_SYSTEMSETTING = "ANDROID_BUILD_SYSTEMSETTING";
            public const string KEYSTORE_PASSWORD = "KEYSTORE_PASSWORD";
            public const string ANDROID_BUILD_OBB = "ANDROID_BUILD_OBB";
            public const string EXPORT_AAB = "EXPORT_AAB";

            //Hotfix
            public const string HOTFIX_OPEN = "HOTFIX_OPEN";
            public const string HOTFIX_SERVER_URL = "HOTFIX_SERVER_URL";
            public const string HOTFIX_INDEPENDENT_CONTROL_DATA_URL = "HOTFIX_INDEPENDENT_CONTROL_DATA_URL";
            public const string HOTFIX_CONTROL_DATA_URL = "HOTFIX_CONTROL_DATA_URL";
            public const string HOTFIX_CHANNEl = "HOTFIX_CHANNEl";
            public const string HOTFIX_VERSION = "HOTFIX_VERSION";
            public const string HOTFIX_TEST_MODE = "HOTFIX_TEST_MODE";
        }

        public class Platform
        {
            public const string ANDROID = "android";
            public const string IOS = "ios";
            public const string WINDOWS = "windows";
            public const string WINDOWS64 = "windows64";
        }
        
        public class BuildConst
        {
            public const string ZEUS_BUILD_PATH = "/../_Build";
            public static readonly string ZEUS_BUILD_PATH_CONFIG = Application.dataPath + ZEUS_BUILD_PATH + "/Config";
            public static readonly string ZEUS_BUILD_PATH_CONFIG_SLASH = ZEUS_BUILD_PATH_CONFIG + "/";
            public static readonly string ZEUS_BUILD_PATH_STREAMING_TEMP = Application.dataPath + ZEUS_BUILD_PATH + "/StreamingAssets_temp";
        }

        public class Default
        {
            private static string s_PackageName;
            public static string packageName
            {
                get
                {
                    if (string.IsNullOrEmpty(s_PackageName))
                    {
                        if (string.IsNullOrEmpty(PlayerSettings.productName) || PlayerSettings.productName.ToLower() == "productname")
                        {
                            return "ZeusDemo";
                        }
                        else
                        {
                            return PlayerSettings.productName;
                        }
                    }
                    return s_PackageName;
                }
                set
                {
                    s_PackageName = value;
                }
            }
            public static string productName
            {
                get
                {
                    if (string.IsNullOrEmpty(PlayerSettings.productName) || PlayerSettings.productName.ToLower() == "productname")
                    {
                        return "Zeus";
                    }
                    return PlayerSettings.productName;
                }
                set
                {
                    PlayerSettings.productName = value;
                }
            }
            public static string companyName
            {
                get
                {
                    if (string.IsNullOrEmpty(PlayerSettings.companyName) || PlayerSettings.companyName.ToLower() == "companyname")
                    {
                        return "PerfectWrold";
                    }
                    return PlayerSettings.companyName;
                }
                set
                {
                    PlayerSettings.companyName = value;
                }
            }
            public static string applicationIdentifier
            {
                get
                {
                    if (string.IsNullOrEmpty(PlayerSettings.applicationIdentifier) ||
                        PlayerSettings.applicationIdentifier.ToLower() == "com.companyname.productname" ||
                        PlayerSettings.applicationIdentifier.IndexOf(".") == PlayerSettings.applicationIdentifier.LastIndexOf(".")
                        )
                    {
                        return string.Join(".", new string[] { "com", companyName, productName });
                    }
                    return PlayerSettings.applicationIdentifier;
                }
                set
                {
                    PlayerSettings.applicationIdentifier = value;
                }
            }
            public static string bundleVersion
            {
                get
                {
                    if (string.IsNullOrEmpty(PlayerSettings.bundleVersion) /*|| PlayerSettings.bundleVersion.ToLower() == "1.0"*/)
                    {
                        return "0.0.1";
                    }
                    return PlayerSettings.bundleVersion;
                }
                set
                {
                    PlayerSettings.bundleVersion = value;
                }
            }

            public static bool appleEnableAutomaticSigning
            {
                get
                {
                    return PlayerSettings.iOS.appleEnableAutomaticSigning;
                }
            }

            public static string bundleNumber(BuildTarget target)
            {
                string result = string.Empty;
                switch (target)
                {
                    case BuildTarget.iOS:
                        result = PlayerSettings.iOS.buildNumber;
                        break;
                    case BuildTarget.Android:
                        result = PlayerSettings.Android.bundleVersionCode.ToString();
                        break;
                }
                if (string.IsNullOrEmpty(result))
                    return "0";
                else
                    return result;
            }
            public static string Platform
            {
                get
                {
                    switch (EditorUserBuildSettings.activeBuildTarget)
                    {
                        case BuildTarget.Android:
                            return GlobalBuild.Platform.ANDROID;
                        case BuildTarget.iOS:
                            return GlobalBuild.Platform.IOS;
                        case BuildTarget.StandaloneWindows:
                            return GlobalBuild.Platform.WINDOWS;
                        case BuildTarget.StandaloneWindows64:
                            return GlobalBuild.Platform.WINDOWS64;
#if UNITY_5
                        case BuildTarget.StandaloneOSXUniversal:
                        case BuildTarget.StandaloneOSXIntel:
                        case BuildTarget.StandaloneOSXIntel64:
#else
                        case BuildTarget.StandaloneOSX:
#endif
                            return GlobalBuild.Platform.WINDOWS;
                        default:
                            throw new System.Exception(string.Format("Cann't Support {0}", EditorUserBuildSettings.selectedStandaloneTarget));
                    }
                }
            }

            public static string AndroidBuildSystemSetting
            {
                get
                {
                    return EditorUserBuildSettings.androidBuildSystem.ToString();
                }
            }

            public static ZeusObbBuild AndroidBuildObb
            {
                get
                {
                    if (PlayerSettings.Android.useAPKExpansionFiles)
                    {
                        return ZeusObbBuild.Unity;
                    }
                    else
                    {
                        return ZeusObbBuild.None;
                    }
                }
            }

            public static string keystorePass
            {
                get
                {
                    if (string.IsNullOrEmpty(PlayerSettings.Android.keystorePass))
                    {
                        return "0";
                    }
                    return PlayerSettings.Android.keystorePass;
                }
            }

            private static string _OutputPath = string.Empty;
            public static string OutputPath { get { return string.IsNullOrEmpty(_OutputPath) ? PathUtil.FormatPathSeparator(System.Environment.CurrentDirectory) : _OutputPath; } set { _OutputPath = PathUtil.FormatPathSeparator(value); } }
            public const string ENABLE_BITCODE = "NO";
            public static string CODE_SIGN_IDENTITY = "iPhone Distribution: Chongqing Perfect World Interactive Technology Co., Ltd. (EL4HUQZ2MY)";

            public static string PROVISIONING_PROFILE_SPECIFIER
            {
                get
                {
                    return PlayerSettings.iOS.iOSManualProvisioningProfileID;
                }
            }

            public static string DEVELOPMENT_TEAM
            {
                get
                {
                    return PlayerSettings.iOS.appleDeveloperTeamID;
                }
            }

        }
    }
}
#endif

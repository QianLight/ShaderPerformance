#if UNITY_IOS

using UnityEngine;
using UnityEditor.iOS.Xcode;


namespace UnityEditor.XBuild
{
    public class ProjectSettingIOS_Internal : ProjectSettingIOS
    {
        public override void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            base.OnPostProcessBuild(target, pathToBuiltProject);
            EditProj(pathToBuiltProject);
        }

        static void EditProj(string pathToBuiltProject)
        {
            // string projPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
            string projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            Debug.Log("SDK-PostProcessBuild :: " + projPath);

            PBXProject pbxProj = new PBXProject();
            pbxProj.ReadFromFile(projPath);

            // Build Settings
            string targetGuid = pbxProj.GetUnityFrameworkTargetGuid();

            // 添加预编译宏
            string[] macros = {"SYS_IOS", "CODE_PFM_U3D"};
            foreach (string item in macros)
            {
                pbxProj.AddBuildProperty(targetGuid, "GCC_PREPROCESSOR_DEFINITIONS[arch=*]", item);
            }

            // 关闭BitCode
            pbxProj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");

            targetGuid = pbxProj.GetUnityMainTargetGuid();
            // 添加预编译宏
            foreach (string item in macros)
            {
                pbxProj.AddBuildProperty(targetGuid, "GCC_PREPROCESSOR_DEFINITIONS[arch=*]", item);
            }
            // 关闭BitCode
            pbxProj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
            pbxProj.SetBuildProperty(targetGuid, "PROVISIONING_PROFILE_SPECIFIER", "cfgame");
            
            string debugConfig = pbxProj.BuildConfigByName(targetGuid, "Debug");
            string releaseConfig = pbxProj.BuildConfigByName(targetGuid, "Release");
            string releaseForProfilingConfig = pbxProj.BuildConfigByName(targetGuid, "ReleaseForProfiling");
            string releaseForRunningConfig = pbxProj.BuildConfigByName(targetGuid, "ReleaseForRunning");
            pbxProj.SetBuildPropertyForConfig(debugConfig, "DEBUG_INFORMATION_FORMAT", "dwarf-with-dsym");
            pbxProj.SetBuildPropertyForConfig(releaseConfig, "DEBUG_INFORMATION_FORMAT", "dwarf-with-dsym");
            pbxProj.SetBuildPropertyForConfig(releaseForProfilingConfig, "DEBUG_INFORMATION_FORMAT", "dwarf-with-dsym");
            pbxProj.SetBuildPropertyForConfig(releaseForRunningConfig, "DEBUG_INFORMATION_FORMAT", "dwarf-with-dsym");
            
            pbxProj.WriteToFile(projPath);
        }


        public override void WritePlistFile(string path)
        {
            base.WritePlistFile(path);
            XCPlist list = new XCPlist(path);
            // 允许HTTP通信协议
            AddATSSection(ref list);
            
            // 适配 iOS10
            // 添加麦克风访问权限
            AddStringKVPair(ref list, "NSMicrophoneUsageDescription", "是否允许此App使用你的麦克风？");
            // 添加相机访问权限
            AddStringKVPair(ref list, "NSCameraUsageDescription", "是否允许此App使用你的相机？");
            // 添加相册访问权限
            AddStringKVPair(ref list, "NSPhotoLibraryUsageDescription", "是否允许此App使用你的媒体资料库？");
            // 添加蓝牙访问权限
            AddStringKVPair(ref list, "NSBluetoothPeripheralUsageDescription", "是否允许此App使用你的蓝牙？");
            // 访问位置
            AddStringKVPair(ref list, "NSLocationWhenInUseUsageDescription", "是否允许此App访问位置？");
            // 游戏不支持竖屏，需要勾选全屏选项
            AddBooleanKVPair(ref list, "UIRequiresFullScreen", true);

            // 是否启用自动刷新票据
            // Yes-启用；No（或不配置）-禁用
            AddBooleanKVPair(ref list, "AutoRefreshToken", true);

            // iOS系统渠道编号
            AddStringKVPair(ref list, "CHANNEL_DENGTA", "1001");

            // 是否启用公告功能
            AddBooleanKVPair(ref list, "NeedNotice", true);

            // 公告自动拉取的时间间隔（秒）
            AddIntegerNumberKVPair(ref list, "NoticeTime", 600);

            // 白名单
            string[] query = {"mqq", "mqqapi", "mqqwpa", "mqqbrowser", "mttbrowser", "weixin", "wechat"};
            AddAppQueriesSchemes(ref list, query);

            list.Save();
        }


        public override void AddExtCode(string pathToBuiltProject)
        {
            base.AddExtCode(pathToBuiltProject);

            AddExtCode_HandleURL_Modify(pathToBuiltProject);
            AddExtCode_HandleURL_Extend(pathToBuiltProject);
            AddExtCode_supportOrientation(pathToBuiltProject);
            AdaptHomeIndicator(pathToBuiltProject);
            AddUnityFrameWork(pathToBuiltProject);
            AddExtCode_UIView(pathToBuiltProject);
        }
    }
}

#endif

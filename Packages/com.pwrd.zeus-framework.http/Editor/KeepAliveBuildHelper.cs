/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_EDITOR
using UnityEditor;
using Zeus.Build;
using System.Collections.Generic;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
#endif

namespace Zeus.Framework.Http
{
    public class KeepAliveBuildHelper
    {
#if UNITY_IOS
        private iOSAppInBackgroundBehavior _oldiOSAppInBackgroundBehavior;
        private iOSBackgroundMode _oldiOSBackgroundMode;
#endif
        /// <summary>
        /// 此函数对应IModifyPlayerSettings接口，应该在其对应的阶段调用
        /// </summary>
        /// <param name="target"></param>
        public void OnModifyPlayerSettings(BuildTarget target)
        {
#if UNITY_IOS
            if (target == BuildTarget.iOS)
            {
                _oldiOSAppInBackgroundBehavior = PlayerSettings.iOS.appInBackgroundBehavior;
                _oldiOSBackgroundMode = PlayerSettings.iOS.backgroundModes;
                PlayerSettings.iOS.appInBackgroundBehavior = iOSAppInBackgroundBehavior.Custom;
                PlayerSettings.iOS.backgroundModes = iOSBackgroundMode.Audio;
            }
#endif
        }

#if TEST_KEEPALIVE
        [UnityEditor.MenuItem("test/test_keepalive")]
        public static void Test()
        {
            KeepAliveBuildHelper helper = new KeepAliveBuildHelper();
            helper.OnBeforeBuild(BuildTarget.Android);
            helper.OnBeforeBuild(BuildTarget.iOS);
        }
#endif

        /// <summary>
        /// 此函数对应IBeforeBuild接口，应该在其对应的阶段调用
        /// </summary>
        /// <param name="target"></param>
        public void OnBeforeBuild(BuildTarget target)
        {
            try
            {
                string packagePath = PackageUtility.GetPackageFullPath("com.pwrd.zeus-framework.http");
                string pluginsDir = packagePath + "/Runtime/UnityMobilePlatformKeepAlive/Plugins";
                string[] libraryPaths = null;
                if (target == BuildTarget.Android)
                {
                    string dir = pluginsDir + "/Android";
                    libraryPaths = System.IO.Directory.GetFiles(dir, "*.aar");
                }
                if (libraryPaths != null && libraryPaths.Length > 1)
                {
                    Dictionary<string, System.DateTime> dic = new Dictionary<string, System.DateTime>();
                    for (int i = 0; i < libraryPaths.Length; i++)
                    {
                        string[] temp = System.IO.Path.GetFileName(libraryPaths[i]).Split('-');
                        System.DateTime buildDate;
                        if (temp.Length >= 2 && System.DateTime.TryParse(temp[1], out buildDate))
                        {
                            dic.Add(libraryPaths[i], buildDate);
                        }
                        else
                        {
                            dic.Add(libraryPaths[i], System.DateTime.MinValue);
                        }
                    }
                    string maxPath = null;
                    System.DateTime maxDate = System.DateTime.MinValue;
                    foreach (var item in dic)
                    {
                        if (item.Value >= maxDate)
                        {
                            if (!string.IsNullOrEmpty(maxPath))
                            {
                                System.IO.File.Delete(maxPath);
                            }
                            maxPath = item.Key;
                            maxDate = item.Value;
                        }
                        else
                        {
                            UnityEngine.Debug.LogError("Delete Duplicate Plugin:" + item.Key);
                            System.IO.File.Delete(item.Key);
                        }
                    }
                    AssetDatabase.Refresh();
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
        }

        /// <summary>
        /// 此函数对应IAfterBuild接口，应该在其对应的阶段调用
        /// </summary>
        /// <param name="target"></param>
        public void OnAfterBuild(BuildTarget target, string locationPathName)
        {
#if UNITY_IOS
            if (target == BuildTarget.iOS)
            {
                //iOSKeepAlive.framework 保活模块内包含音乐资源，需要设置为 Embed&sign 才能拷贝到app中
                string path = PBXProject.GetPBXProjectPath(locationPathName);
                PBXProject project = new PBXProject();
                project.ReadFromFile(path);
#if UNITY_2019_3_OR_NEWER
                string targetGuid = project.GetUnityMainTargetGuid();
                string unityFrameworkTargetGuid = project.GetUnityFrameworkTargetGuid();
#else
                string targetGuid = project.TargetGuidByName(PBXProject.GetUnityTargetName());
                string unityFrameworkTargetGuid = project.TargetGuidByName("UnityFramework");
#endif
                string[] guids = AssetDatabase.FindAssets("iOSKeepAlive.framework");
                //获取保活模块在Xcode工程里的相对路径
                string keepalivePath = AssetDatabase.GUIDToAssetPath(guids[0]).Replace("Assets", "Frameworks").Replace("Packages", "Frameworks");
                //获取保活模块在Xcode工程里的Guid
                string keepaliveGuid = project.FindFileGuidByProjectPath(keepalivePath);
                //设置为 Embed&sign
                PBXProjectExtensions.AddFileToEmbedFrameworks(project, targetGuid, keepaliveGuid);
                project.WriteToFile(path);
            }
#endif
        }

        /// <summary>
        /// 此函数对应IFinallyBuild接口，应该在其对应的阶段调用
        /// </summary>
        /// <param name="target"></param>
        public void OnFinallyBuild(BuildTarget target, string outputPath)
        {
#if UNITY_IOS
            if (target == BuildTarget.iOS)
            {
                PlayerSettings.iOS.appInBackgroundBehavior = _oldiOSAppInBackgroundBehavior;
                PlayerSettings.iOS.backgroundModes = _oldiOSBackgroundMode;
            }
#endif
        }
    }
}
#endif
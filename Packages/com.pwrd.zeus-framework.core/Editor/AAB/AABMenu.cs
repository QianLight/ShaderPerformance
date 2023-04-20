/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Zeus.Build.AAB
{
    public class AABWhiteList
    {
        [SimpleJsonWindowPath(SimpleJsonWindowPathAttribute.RootType.StreamingAssets, SimpleJsonWindowPathAttribute.ContentType.Folder)
            , SimpleJsonWindowDesc("这里填相对于StreamingAssets的路径\n添加白名单，生成AAB的时候，资源不放入InstallTime，主要是为了处理一些Unity的接口调用，需要资源放到StreamingAssets下的情况")]
        public List<string> FolderDoNotMoveToInstallTime = new List<string>();
        [SimpleJsonWindowPath(SimpleJsonWindowPathAttribute.RootType.StreamingAssets, SimpleJsonWindowPathAttribute.ContentType.File)
            , SimpleJsonWindowDesc("这里填相对于StreamingAssets的路径\n添加白名单，生成AAB的时候，资源不放入InstallTime，主要是为了处理一些Unity的接口调用，需要资源放到StreamingAssets下的情况")]
        public List<string> FileDoNotMoveToInstallTime = new List<string>();

        public static AABWhiteList Load()
        {
            if (!File.Exists(AABGlobal.AABWhiteListPath))
            {
                Debug.LogFormat("no whitelist file at {0}", AABGlobal.AABWhiteListPath);
                return null;
            }
            var json = File.ReadAllText(AABGlobal.AABWhiteListPath);
            var aabWhiteList = JsonUtility.FromJson<AABWhiteList>(json);
            return aabWhiteList;
        }

    }

    internal static class AABMenu
    {
        [MenuItem("Zeus/Core/AAB/编辑AAB白名单")]
        private static void EditorAABStreamingAssetsConfig()
        {
            SimpleJsonWindow.GetWindow(AABGlobal.AABWhiteListPath, typeof(AABWhiteList), "编辑AAB白名单");
        }

        [MenuItem("Zeus/Core/AAB/从AAB导出Apks以检查包内容")]
        private static void ExportApks()
        {
            var aabPath = PlayerPrefs.GetString(AABGlobal.LastAABPathSaveKey, Application.dataPath);
            aabPath = EditorUtility.OpenFilePanel("选择AAB文件", aabPath, "aab");
            if(string.IsNullOrEmpty(aabPath))
            {
                return;
            }
            if(!File.Exists(aabPath))
            {
                EditorUtility.DisplayDialog("错误", "请选择一个合法的aab文件", "好");
                return;
            }
            var apksPath = aabPath.Replace(AABGlobal.AABExtension, AABGlobal.APKSExtension);
            AABUtility.ExportAPKS(apksPath, aabPath);
            EditorUtility.DisplayDialog("提示", "生成成功", "好");
#if UNITY_EDITOR_WIN
            Application.OpenURL(Path.GetDirectoryName(aabPath));
#endif
        }

        [MenuItem("Zeus/Core/AAB/安装AAB到设备")]
        private static void InstallAABToDevices()
        {
            EditorUtility.DisplayDialog("注意", "请连接手机，或模拟器，使用adb devices命令，确认有且仅有一台设备", "好");
            var aabPath = PlayerPrefs.GetString(AABGlobal.LastAABPathSaveKey, Application.dataPath);
            aabPath = EditorUtility.OpenFilePanel("选择AAB文件", aabPath, "aab");
            if(string.IsNullOrEmpty(aabPath))
            {
                return;
            }
            if(!File.Exists(aabPath))
            {
                EditorUtility.DisplayDialog("错误", "请选择一个合法的aab文件", "好");
                return;
            }
            var apksPath = aabPath.Replace(AABGlobal.AABExtension, AABGlobal.APKSExtension);
            EditorUtility.DisplayProgressBar("运行中", "正在预处理AAB，请稍后", 0.3f);
            AABUtility.ExportAPKS(apksPath, aabPath);
            EditorUtility.DisplayProgressBar("运行中", "正在安装AAB，请稍后", 0.7f);
            AABUtility.InstallToDevice(apksPath, aabPath);
            if(File.Exists(apksPath))
            {
                File.Delete(apksPath);
            }
            PlayerPrefs.SetString(AABGlobal.LastAABPathSaveKey, aabPath);
            PlayerPrefs.Save();
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("提示", "安装成功", "好");
        }


        [MenuItem("Zeus/Core/AAB/获取apk大小")]
        private static void GetAPKSizeFromAPKS()
        {
            var aabPath = PlayerPrefs.GetString(AABGlobal.LastAABPathSaveKey, Application.dataPath);
            aabPath = EditorUtility.OpenFilePanel("选择AAB文件", aabPath, "aab");
            if (string.IsNullOrEmpty(aabPath))
            {
                return;
            }
            if (!File.Exists(aabPath))
            {
                EditorUtility.DisplayDialog("错误", "请选择一个合法的aab文件", "好");
                return;
            }

            var apksPath = aabPath.Replace(AABGlobal.AABExtension, AABGlobal.APKSExtension);
            var result = AABUtility.GetAPKSize(apksPath, aabPath);
            if (string.IsNullOrEmpty(result))
            {
                EditorUtility.DisplayDialog("错误", "获取size失败", "好");
            }
            else
            {
                var lines = result.Replace("\r", "").Trim().Split('\n');
                var sizeLine = lines[lines.Length - 1];
                var sizes = sizeLine.Trim().Split(',');
                EditorUtility.DisplayDialog("提示", string.Format("{0} to {1}", BeautySize(sizes[0]), BeautySize(sizes[1])), "好");
            }
        }

        private static string BeautySize(string size)
        {
            var result = "";
            var sizeNum = int.Parse(size);
            var m = 1024 * 1024;
            var k = 1024;
            var mNum = sizeNum / m;
            if(mNum > 0)
            {
                result += mNum.ToString() + "m";
            }
            var last = sizeNum % m;
            var kNum = last / k;
            if(kNum  > 0)
            {
                result += kNum.ToString() + "k";
            }
            return result;
        }

        [MenuItem("Zeus/Core/AAB/从AAB导出APK")]
        private static void ExportAPKFromAAB()
        {
            try
            {
                var aabPath = PlayerPrefs.GetString(AABGlobal.LastAABPathSaveKey, Application.dataPath);
                aabPath = EditorUtility.OpenFilePanel("选择AAB文件", aabPath, "aab");
                if(string.IsNullOrEmpty(aabPath))
                {
                    return;
                }
                if(!File.Exists(aabPath))
                {
                    EditorUtility.DisplayDialog("错误", "请选择一个合法的aab文件", "好");
                    return;
                }
                var apksPath = aabPath.Replace(AABGlobal.AABExtension, AABGlobal.APKSExtension);
                EditorUtility.DisplayProgressBar("运行中", "正在预处理AAB，请稍后", 0.3f);
                AABUtility.ExportUniversalAPKS(apksPath, aabPath);
                EditorUtility.DisplayProgressBar("运行中", "正在导出APK文件，请稍后", 0.7f);
                AABUtility.ExportUniversalAPK(apksPath);
                if(File.Exists(apksPath))
                {
                    File.Delete(apksPath);
                }
                EditorUtility.DisplayDialog("提示", "导出APK成功", "好");
#if UNITY_EDITOR_WIN
                Application.OpenURL(Path.GetDirectoryName(aabPath));
#endif
            }
            catch(System.Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}

/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
//#define TEST
using ICSharpCode.ZeusSharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 不同的Unity版本导出的Android工程的结构不一样，所以需要针对2019.3之前和之后的版本做不同的处理 
/// https://docs.unity3d.com/2019.3/Documentation/Manual/android-gradle-overview.html
/// 不同的Gradle版本对于aab的支持方式也不一样，所以需要区分下Gradle版本做不同处理，
/// Unity自带的Gradle版本最高为5.6.4 对应最高的Gradle插件版本为3.6.4，而4.0.0以上的插件版本，api有变化
/// https://developer.android.com/studio/releases/gradle-plugin
/// https://docs.unity3d.com/2021.2/Documentation/Manual/android-gradle-overview.html
/// 由于高版本的Gradle也可能会被使用，（比如在接sdk的时候用），所以分别做下支持
/// 由于低版本的GradlePlugins 有bug，会导致不合理的压缩，且不能控制见链接
/// https://forum.unity.com/threads/streamingassets-files-are-compressed-in-apk-when-build-app-bundle-google-play-option-is-used.739967/
/// 因此不再支持低版本的GradlePlugins, 相关接口保留以备有特殊需求的时候可以有个基础
/// </summary>
namespace Zeus.Build.AAB
{
    internal static class AABUtility
    {
        /// <summary>
        /// 根据路径创建所需的文件夹
        /// </summary>
        /// <param name="path"></param>
        private static void CreateDirectoryForPath(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        public static bool ExportAndroidProject(BuildPlayerOptions buildPlayerOptions)
        {
            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            return report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded;
        }

        /// <summary>
        /// 导出配好的可以生成所需aab(即streamingAssets下的资源作为installTime类型的资源包进行配置) 的Android工程
        /// </summary>
        /// <param name="location"></param>
        public static bool ExportAndroidProjectForAAB(BuildPlayerOptions buildPlayerOptions)
        {
            var succeed = ExportAndroidProject(buildPlayerOptions);
            if(!succeed)
            {
                return false;
            }
            try
            {
#if UNITY_2019_3_OR_NEWER
                CreateAssetPacks(buildPlayerOptions.locationPathName);
#else
                CreateAssetPacks(Path.Combine(buildPlayerOptions.locationPathName, PlayerSettings.productName));
#endif
                return succeed;
            }
            catch (System.Exception e)
            {
                Debug.LogError("CreateAssetPacks failed \n" + e.ToString());
                return false;
            }
        }


        /// <summary>
        /// 这里提供了使用低版本gradleplugins的选项，但是如果使用低版本，需要对资源做约束，名字全部小写，否则会导致不合理的压缩问题
        /// </summary>
        /// <param name="location"></param>
        /// <param name="isGradle4OrHigher"></param>
        private static void CreateAssetPacks(string location, bool isGradle4OrHigher = true)
        {
            var aabWhiteList = AABWhiteList.Load();
            ModifyBaseGradleSettings(location);
            DisableNoCompressionLibirary(location);
#if UNITY_2019_3_OR_NEWER
            if (isGradle4OrHigher)
            {
                ModifyBaseGradeVersion(location);
                ModifyLauncherGradleBuildInGradle4(location);
                GenerateAssetpackBuildGradleInGradle4(location);
            }
            else
            {
                ModifyLauncherGradleBuildInGradle3(location);
                GenerateAssetpackBuildGradleInGradle3(location);
            }
            GenerateAssetpackManifest(location);
#else
            GenerateAssetpackManifest(location);
            if(isGradle4OrHigher)
            {
                ModifyBaseGradeVersion(location);
                ModifyBaseGradleBuildInGradle4(location);
                GenerateAssetpackBuildGradleInGradle4(location);
            }
            else
            {
                GenerateAssetpackBuildGradleInGradle3(location);
                AddAssetpackToBuildGradleInGradle3(location);
                ModifyDependenciesSearchPathInGradle3(location);
            }
#endif
            CopyStreamingAssetsToAssetPack(location, aabWhiteList);
        }

#if UNITY_2019_3_OR_NEWER
        /// <summary>
        /// 修改base的build.gradle文件，将install_time作为base的分包，GradlePlugins 4.0+
        /// </summary>
        /// <param name="location"></param>
        /// <param name="aabWhiteList"></param>
        private static void ModifyLauncherGradleBuildInGradle4(string location)
        {
            var appBuildGradlePath = Path.Combine(location, AABGlobal.LauncherBuildGradlePathInAPK);
            ModifyAppGradleBuildInGradle4(appBuildGradlePath);
        }
#else
        private static void ModifyDependenciesSearchPathInGradle3(string location)
        {
            var projectBuildGradlePath = Path.Combine(location, AABGlobal.ProjectBuildGradlePathInAPK);
            var projectBuildGradleLines = File.ReadAllLines(projectBuildGradlePath);
            //添加搜索路径，搜索父工程的libs，这里赤金的项目不添加会报错，找不到依赖
            AddToNextLine(projectBuildGradleLines, line => line.Contains("flatDir"), "\t\t\tdirs '../libs'");
        }

        /// <summary>
        /// 添加dynamicFeatures属性，将install_time作为base的分包，GradlePlugins 3.0
        /// </summary>
        /// <param name="location"></param>
        private static void AddAssetpackToBuildGradleInGradle3(string location)
        {
            var projectBuildGradlePath = Path.Combine(location, AABGlobal.ProjectBuildGradlePathInAPK);
            AddDynamicFeaturesToBaseInGradle3(projectBuildGradlePath);
        }

        /// <summary>
        /// 修改base的build.gradle文件，将install_time作为base的分包，GradlePlugins 4.0+
        /// </summary>
        /// <param name="location"></param>
        /// <param name="aabWhiteList"></param>
        private static void ModifyBaseGradleBuildInGradle4(string location)
        {
            var appBuildGradlePath = Path.Combine(location, AABGlobal.ProjectBuildGradlePathInAPK);
            ModifyAppGradleBuildInGradle4(appBuildGradlePath);
        }



#endif

        private static string[] AddToNextLine(string[] lines, Func<string, bool> isPrevLine, string content)
        {
            var lineList = new List<string>();
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] == content)
                {
                    return lines;
                }
            }
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                lineList.Add(line);
                if (isPrevLine(line))
                {
                    lineList.Add(content);
                }
            }
            return lineList.ToArray();
        }

        /// <summary>
        /// 根据预设，生成installTime的build.gradle
        /// </summary>
        /// <param name="location"></param>
        private static void GenerateAssetpackBuildGradleInGradle4(string location)
        {
            var buildGradlePath = Path.Combine(location, AABGlobal.InstallTimeBuildGradlePathInAPK);
            ReplaceFile(AABGlobal.InstallTimePresetBuildGradlePath, buildGradlePath);
        }

        /// <summary>
        /// 生成InstallTime的build.gradle文件
        /// </summary>
        /// <param name="location"></param>
        private static void GenerateAssetpackBuildGradleInGradle3(string location)
        {
            var buildGradlePath = Path.Combine(location, AABGlobal.InstallTimeBuildGradlePathInAPK);
            if (File.Exists(buildGradlePath))
            {
                File.Delete(buildGradlePath);
            }
            var gradleContent = AssetDatabase.LoadAssetAtPath<TextAsset>(AABGlobal.InstallTimePresetBuildGradle3Path);
            CreateDirectoryForPath(buildGradlePath);
            var compileSdkVer = "";
            var minSdkVer = "";
            var targetSdkVer = "";
            var buildToolsVer = "";
            var noCompress = "";
#if UNITY_2019_3_OR_NEWER
            var projectBuildGradlePath = Path.Combine(location, AABGlobal.LauncherBuildGradlePathInAPK);
#else
            var projectBuildGradlePath = Path.Combine(location, AABGlobal.ProjectBuildGradlePathInAPK);
#endif
            GetBaseApkSdkVersion(projectBuildGradlePath, ref compileSdkVer, ref minSdkVer, ref targetSdkVer, ref buildToolsVer, ref noCompress);
            var content = gradleContent.text.Replace("{compileSdkVersion}", compileSdkVer)
                .Replace("{minSdkVersion}", minSdkVer)
                .Replace("{targetSdkVersion}", targetSdkVer)
                .Replace("{buildToolsVersion}", buildToolsVer)
                .Replace("{noCompress}", noCompress)
#if UNITY_2019_3_OR_NEWER
                .Replace("{project}", ":launcher");
#else
                .Replace("{project}", ":");
#endif
            //File.WriteAllText(buildGradlePath, string.Format(gradleContent.text, compileSdkVer, minSdkVer, targetSdkVer));
            File.WriteAllText(buildGradlePath, content);
        }

        private static void GetBaseApkSdkVersion(string projectBuildGradlePath,
            ref string compileSdkVer,
            ref string minSdkVer,
            ref string targetSdkVer,
            ref string buildToolsVer,
            ref string nocompress)
        {
            var projectBuildGradleLines = File.ReadAllLines(projectBuildGradlePath);
            foreach (var line in projectBuildGradleLines)
            {
                var compileSdkVersionKey = "compileSdkVersion";
                TrySetGradleValInLine(line, compileSdkVersionKey, ref compileSdkVer);
                var minSdkVersionKey = "minSdkVersion";
                TrySetGradleValInLine(line, minSdkVersionKey, ref minSdkVer);
                var targetSdkVersionKey = "targetSdkVersion";
                TrySetGradleValInLine(line, targetSdkVersionKey, ref targetSdkVer);
                var buildToolsVersionKey = "buildToolsVersion";
                TrySetGradleValInLine(line, buildToolsVersionKey, ref buildToolsVer);
                var nocompressKey = "noCompress";
                TrySetGradleValInLine(line, nocompressKey, ref nocompress);
            }
        }

        private static void TrySetGradleValInLine(string line, string key, ref string val)
        {
            var keyIndex = line.IndexOf(key);
            if (keyIndex >= 0)
            {
                var start = keyIndex + key.Length;
                val = line.Substring(start, line.Length - start);
            }
        }


        /// <summary>
        /// 生成Install_time的Manifest
        /// </summary>
        /// <param name="location"></param>
        private static void GenerateAssetpackManifest(string location)
        {
            var targetFile = Path.Combine(location, AABGlobal.InstallTimeManifestPathInAPK);
            ReplaceFile(AABGlobal.InstallTimePresetManifestPath, targetFile);
        }

        /// <summary>
        /// 禁用库压缩的优化，虽然会让安装速度提升，但是会导致包体变大，留给base的空间变少
        /// </summary>
        /// <param name="location"></param>
        private static void DisableNoCompressionLibirary(string location)
        {
            var gradleProperities = Path.Combine(location, AABGlobal.ProjectGradlePropertiesPathInAPK);
            //这里的字符串头加了个换行符，防止出现上一行没有换行符，配置错误的情况
            File.AppendAllLines(gradleProperities, new string[]{ "\nandroid.bundle.enableUncompressedNativeLibs=false" });
        }

        private static void ReplaceFile(string sourceFile, string targetFile)
        {
            var dir = Path.GetDirectoryName(targetFile);
            if (Directory.Exists(dir) && File.Exists(targetFile))
            {
                File.Delete(targetFile);
            }
            CreateDirectoryForPath(targetFile);
            File.Copy(sourceFile, targetFile);
        }

        private static void ModifyAppGradleBuildInGradle4(string appBuildGradlePath)
        {
            var appBuildGradleLines = File.ReadAllLines(appBuildGradlePath);
            var insertLine = "\tassetPacks = [':install_time']";
            var newLines = AddToNextLine(appBuildGradleLines, line => line.IndexOf("android {") >= 0, insertLine);
            File.WriteAllLines(appBuildGradlePath, newLines);

        }

        /// <summary>
        /// 修改Launcher的build.gradle，添加对installTime的引用
        /// </summary>
        /// <param name="location"></param>
        private static void ModifyLauncherGradleBuildInGradle3(string location)
        {
            var projectBuildGradlePath = Path.Combine(location, AABGlobal.LauncherBuildGradlePathInAPK);
            AddDynamicFeaturesToBaseInGradle3(projectBuildGradlePath);
        }

        private static void AddDynamicFeaturesToBaseInGradle3(string baseGradlePath)
        {
            var projectBuildGradleLines = File.ReadAllLines(baseGradlePath);
            var newLines = AddToNextLine(projectBuildGradleLines, line => line.StartsWith("android {"), "\tdynamicFeatures = [':install_time']");
            File.WriteAllLines(baseGradlePath, newLines);
        }

        /// <summary>
        /// 修改GradlePlugins的版本，Unity默认的版本是3，3和4的API不一样
        /// </summary>
        /// <param name="location"></param>
        private static void ModifyBaseGradeVersion(string location)
        {
            var projectBuildGradlePath = Path.Combine(location, AABGlobal.ProjectBuildGradlePathInAPK);
            var projectBuildGradleLines = File.ReadAllLines(projectBuildGradlePath);
            for (var i = 0; i < projectBuildGradleLines.Length; i++)
            {
                var line = projectBuildGradleLines[i];
                if (line.IndexOf("classpath") >= 0)
                {
                    var start = line.IndexOf("gradle:");
                    var newLine = line.Substring(0, start);
                    //Unity默认的gradle版本是6.1.1，为了通用，这里的gradlePlugin用兼容的版本
                    projectBuildGradleLines[i] = newLine + "gradle:4.0.1'";
                    break;
                }
            }
            File.WriteAllLines(projectBuildGradlePath, projectBuildGradleLines);
        }

        /// <summary>
        /// 修改settings.gradle文件，添加assetpack的引用
        /// </summary>
        /// <param name="location"></param>
        private static void ModifyBaseGradleSettings(string location)
        {
            var projectSettingsGradlePath = Path.Combine(location, AABGlobal.ProjectSettingGradlePathInAPK);
            if (File.Exists(projectSettingsGradlePath))
            {
                var projectSettingsGradleLines = File.ReadAllLines(projectSettingsGradlePath);
                if (projectSettingsGradleLines.Length > 0)
                {
                    for (var i = 0; i < projectSettingsGradleLines.Length; i++)
                    {
                        var line = projectSettingsGradleLines[i];
                        if (line.IndexOf("include") >= 0)
                        {
                            projectSettingsGradleLines[i] = line + ",':install_time'";
                            break;
                        }
                    }
                    File.WriteAllLines(projectSettingsGradlePath, projectSettingsGradleLines);
                    return;
                }
            }
            var content = "include ':install_time'";
            CreateDirectoryForPath(projectSettingsGradlePath);
            File.WriteAllText(projectSettingsGradlePath, content);
        }

        /// <summary>
        /// 把StreamingAssets下的文件拷贝到assetpack路径下
        /// </summary>
        /// <param name="location"></param>
        private static void CopyStreamingAssetsToAssetPack(string location, AABWhiteList aabWhiteList)
        {
            var streamingAssetPath = Path.Combine(location, AABGlobal.StreamingAssetsPathInAPK);
            var installTimeAssetPackPath = Path.Combine(location, AABGlobal.InstallTimeAssetpackPathInAPK);
            RemakeDir(installTimeAssetPackPath);
            var formatStreamingAssetPath = FormatPath(streamingAssetPath);
            CopyFiles(streamingAssetPath, installTimeAssetPackPath, file => IsFileCopyToInstallTime(file, aabWhiteList));
            CleanUpEmptyFolders(streamingAssetPath);
        }

        /// <summary>
        /// 判断是否需要拷贝到assetpack里
        /// </summary>
        /// <param name="file"></param>
        /// <param name="aabWhiteList"></param>
        /// <returns></returns>
        private static bool IsFileCopyToInstallTime(string file, AABWhiteList aabWhiteList)
        {
            if(null == aabWhiteList)
            {
                return true;
            }
            foreach (var exclude in aabWhiteList.FileDoNotMoveToInstallTime)
            {
                if (file.StartsWith(exclude))
                {
                    return false;
                }
            }
            foreach (var exclude in aabWhiteList.FolderDoNotMoveToInstallTime)
            {
                if (file.StartsWith(exclude))
                {
                    return false;
                }
            }
            foreach (var exclude in AABGlobal.ExludeFoldersForInstallTime)
            {
                if (file.StartsWith(exclude))
                {
                    return false;
                }
            }


            return true;
        }

        private static void RemakeDir(string dir)
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
            Directory.CreateDirectory(dir);
        }

        private static string GetRelativePath(string path, string rootPath)
        {
            return FormatPath(path).Replace(FormatPath(rootPath) + "/", "");
        }

        private static string FormatPath(string path)
        {
            if (null == path)
            {
                return null;
            }
            return path.Replace("\\", "/");
        }

        private static void CopyFiles(string sourceFolder, string targetFolder, Func<string, bool> isValid)
        {
            var files = Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var formatFile = GetRelativePath(file, sourceFolder);
                if (!isValid(formatFile))
                {
                    continue;
                }
                var target = Path.Combine(targetFolder, formatFile);
                CreateDirectoryForPath(target);
                File.Move(file, target);
            }

        }

        private static void CleanUpEmptyFolders(string path)
        {
            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                CleanUpEmptyFolders(dir);
                if (!Directory.EnumerateFileSystemEntries(dir).Any() && !Directory.EnumerateFileSystemEntries(dir).Any())
                {
                    Directory.Delete(dir, false);
                }
            }
        }

        public static void ExportUniversalAPKS(string apksPath, string aabPath)
        {
            AABCMDUtility.RunToolCMD("生成可解压出universal_apk的apks", apksPath, aabPath);
        }

        public static void ExportAPKS(string apksPath, string aabPath)
        {
            AABCMDUtility.RunToolCMD("生成apks", apksPath, aabPath);
        }

        public static string GetAPKSize(string apksPath, string aabPath)
        {
            ExportAPKS(apksPath, aabPath);
            return AABCMDUtility.RunToolCMD("获取apk大小", apksPath, aabPath);
        }


        public static void InstallToDevice(string apksPath, string aabPath)
        {
            AABCMDUtility.RunToolCMD("安装apks", apksPath, aabPath);
        }

        public static bool ExportUniversalAPK(string apksPath)
        {
            using (var zipFile = new ZipFile(apksPath))
            {
                var entry = zipFile.GetEntry("universal.apk");
                if (null != entry)
                {
                    using (var inputStream = zipFile.GetInputStream(entry))
                    {
                        using (var writeStream = File.Create(apksPath.Replace(AABGlobal.APKSExtension, AABGlobal.APKExtension)))
                        {
                            byte[] buffer = new byte[1024];
                            while (true)
                            {
                                int readCount = inputStream.Read(buffer, 0, buffer.Length);
                                if (readCount <= 0)
                                {
                                    break;
                                }
                                writeStream.Write(buffer, 0, readCount);
                            }
                            writeStream.Flush();
                        }
                    }
                    return true;
                }
                return false;
            }
        }

#region TESTS
#if TEST
        [MenuItem("Test/CleanupEmptyFoldes")]
        private static void TestCleanUpEmptyFolders()
        {
            var dir = Path.Combine(Application.dataPath, "..", "tmp");
            var width = 4;
            var depth = 4;
            CreateDirTree(dir, width, depth);
            var file = Path.Combine(dir, "1/0/a.txt");
            File.WriteAllText(file, "a");
            file = Path.Combine(dir, "1/1/0/a.txt");
            File.WriteAllText(file, "a");
            CleanUpEmptyFolders(dir);
        }

        private static void CreateDirTree(string rootDir, int width, int depth)
        {
            var queue = new Queue<string>();
            var nextQueue = new Queue<string>();
            queue.Enqueue(rootDir);
            while (true)
            {
                if (queue.Count == 0)
                {
                    depth -= 1;
                    if (depth == 0)
                    {
                        break;
                    }
                    var tmp = queue;
                    queue = nextQueue;
                    nextQueue = tmp;
                }
                var dir = queue.Dequeue();
                for (var i = 0; i < width; i++)
                {
                    var subDir = Path.Combine(dir, i.ToString());
                    Directory.CreateDirectory(subDir);
                    nextQueue.Enqueue(subDir);
                }
            }
        }

        [MenuItem("Build/ModifyAndroidProjectGradle")]
        private static void ModifyAndroidProject()
        {
            var location = EditorUtility.OpenFolderPanel("SelectDir", Application.dataPath, "Build");
            if (string.IsNullOrEmpty(location))
            {
                return;
            }
            CreateAssetPacks(location, true);
        }

#endif
#endregion
    }
}

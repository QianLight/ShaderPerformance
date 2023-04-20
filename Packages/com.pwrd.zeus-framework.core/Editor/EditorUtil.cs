/********************************************************************
	created:	2:6:2017   16:46
	author:		crimson
	
	purpose:	各种Editor下帮助函数，能用就行不在乎效率。
*********************************************************************/
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Zeus.Framework;

namespace Zeus
{
    [InitializeOnLoad]
    public class EditorUtil
	{
        static EditorUtil()
        {
            //EditorApplication.playmodeStateChanged += OnPlaymodeChanged;
        }


        public static Type[] GetAllTypeByAttribute<T>(bool inherit) 
            where T : Attribute
        {
            List<Type> listType = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    
                    T attr = type.GetCustomAttribute<T>(inherit);
                    if (attr != null)
                    {
                        listType.Add(type);
                    }
                }
            }

            return listType.ToArray();
        }

        private static void OnPlaymodeChanged()
        {
            if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
            {// 编辑器运行前
                try
                {
                    var assems = EditorReflectionUtilly.GetCustomAssemblies().Where(a => a.FullName.StartsWith("Assembly-CSharp", StringComparison.CurrentCultureIgnoreCase)).ToArray();
                    foreach (var assem in assems)
                    {
                        foreach (var t in assem.GetTypes())
                        {
                            if (t != null && t.GetCustomAttribute<OnBeforePlayAttribute>() != null)
                            {
                                    var mi = t.GetMethod("_OnBeforePlay", BindingFlags.Static | BindingFlags.NonPublic);
                                    mi.Invoke(null, null);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    EditorApplication.isPlaying = false;
                    throw e;
                }
            }
        }        

        public static void RunBat(string batfile, string args, string workingDir = "")
        {
            string path = FormatPath(workingDir);
            System.Diagnostics.Process p = CreateShellExProcess(batfile, args, path);
            p.WaitForExit();
            p.Close();
        }

        /// <summary>
        /// 运行exe文件
        /// </summary>
        /// <param name="exeName">该参数请传递exe文件完整路径</param>
        /// <param name="args">运行参数</param>
        /// <param name="processOutputMsg">输出信息，没有为空字符串</param>
        /// <param name="processErrorMsg">错误信息，没有为空字符串</param>
        /// <param name="workingDir"></param>
        public static void RunExe(string exeName, string args, out string processOutputMsg, out string processErrorMsg, string workingDir = "")
        {
            string path = FormatPath(workingDir);
            System.Diagnostics.Process p = CreateNoShellProcess(exeName, args, path);
            
            processOutputMsg = p.StandardOutput.ReadToEnd();
            processErrorMsg = p.StandardError.ReadToEnd();
            
            p.WaitForExit();
            p.Close();
        }

        private static System.Diagnostics.Process CreateShellExProcess(string cmd, string args, string workingDir = "")
        {
            var pStartInfo = new System.Diagnostics.ProcessStartInfo(cmd);
            pStartInfo.Arguments = args;
            pStartInfo.CreateNoWindow = false;
            pStartInfo.UseShellExecute = true;
            pStartInfo.RedirectStandardError = false;
            pStartInfo.RedirectStandardInput = false;
            pStartInfo.RedirectStandardOutput = false;
            if (!string.IsNullOrEmpty(workingDir))
                pStartInfo.WorkingDirectory = workingDir;
            return System.Diagnostics.Process.Start(pStartInfo);
        }

        private static System.Diagnostics.Process CreateNoShellProcess(string fileName, string args, string workingDir = "")
        {
            var pStartInfo = new System.Diagnostics.ProcessStartInfo(fileName);
            pStartInfo.Arguments = args;
            pStartInfo.CreateNoWindow = false;
            pStartInfo.UseShellExecute = false;
            pStartInfo.RedirectStandardOutput = true;
            pStartInfo.RedirectStandardError = true;
            if (!string.IsNullOrEmpty(workingDir))
                pStartInfo.WorkingDirectory = workingDir;
            return System.Diagnostics.Process.Start(pStartInfo);
        }

        public static string FormatPath(string path)
        {
            path = path.Replace("/", "\\");
            if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.OSXEditor)
                path = path.Replace("\\", "/");
            return path;
        }

        /// <summary>
        /// 获取Svn版本号
        /// </summary>
        /// <param name="svnpath">svn 可执行文件的路径，如果已经加入了环境变量，可以传空</param>
        /// <param name="workingPath">获取获取svn版本号的工作路径，一般是Application.dataPath</param>
        /// <param name="error">错误信息</param>
        /// <returns>如果出错会返回-1，并附带错误信息，否则返回int类型的svn版本号</returns>
        public static int GetSvnRevision(string svnpath, string workingPath, out string error)
        {
            try
            {
                var p = new System.Diagnostics.Process();
                p.StartInfo.FileName = string.IsNullOrEmpty(svnpath) ? "svn" : svnpath;
                p.StartInfo.Arguments = "info " + workingPath;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.UseShellExecute = false;
                
                p.Start();
                var result = -1;
                var output = p.StandardOutput.ReadToEnd();
                error = p.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(error))
                {
                    p.WaitForExit();
                    p.Close();
                    return result;
                }
                p.WaitForExit();
                p.Close();
                System.Console.WriteLine("output: " + output);

                output = output.Replace("\r", "");
                var lines = output.Split('\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("Last Changed Rev:"))
                    {
                        var revisionStr = line.Substring("Last Changed Rev:".Length);
                        if (!int.TryParse(revisionStr, out result))
                        {
                            error = $"invilid revision {revisionStr}";
                            return -1;
                        }
                        break;
                    }
                }
                return result; ;
            }
            catch (Exception e)
            {
                error = e.ToString();
                return -1;
            }
        }

        /// <summary>
        /// 快速检查有没有被Editor宏隐藏起来的编译错误
        /// </summary>
        [MenuItem("Zeus/FastCheckRuntimeError")]
        public static void FastCheckRuntimeError()
        {
            var outputPath = UnityEngine.Application.dataPath + "/../ZeusFastBuild/";
            if(!System.IO.Directory.Exists(outputPath))
            {
                System.IO.Directory.CreateDirectory(outputPath);
            }
            AssetBundleBuild[] abs = new AssetBundleBuild[]
            {
                new AssetBundleBuild
                {
                    assetBundleName = "fast_check_runtime_error",
                    assetNames = new string[]{"Packages/com.pwrd.zeus-framework.core/fast_check_runtime_error.txt" },
                }
            };
            BuildPipeline.BuildAssetBundles(outputPath, abs, BuildAssetBundleOptions.ForceRebuildAssetBundle, EditorUserBuildSettings.activeBuildTarget);
        }
    }
}

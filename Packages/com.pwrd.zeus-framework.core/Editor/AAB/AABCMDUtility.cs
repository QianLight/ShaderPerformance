/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.IO;
using UnityEngine;

namespace Zeus.Build.AAB
{
    internal static class AABCMDUtility
    {
        private const string BatchFilePath = "Packages/com.pwrd.zeus-framework.core/Editor/AAB/res";
        /// <summary>
        /// 这个是赤金项目的地址
        /// </summary>
        //private const string BatchFilePath = "Assets/Zeus/Core/Editor/AAB/res";
        private static string RunBat(string dir, string fileName)
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                WorkingDirectory = dir,
#if UNITY_EDITOR_OSX
                FileName = "sh",
                Arguments = fileName,
#else
                FileName = fileName,
#endif
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            var p = System.Diagnostics.Process.Start(startInfo);
            p.WaitForExit();
            var output = p.StandardOutput.ReadToEnd();
            if (!string.IsNullOrEmpty(output))
            {
                Debug.Log(output);
            }
            var error = p.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
            }
            p.Close();
            return output;
        }

        public static string RunToolCMD(string batFileName, string apksPath, string aabPath)
        {
#if UNITY_EDITOR_OSX
            var batFileNameWithExtension = batFileName + ".sh";
#else
            var batFileNameWithExtension = batFileName + ".bat";
#endif
            Debug.LogFormat("RunToolCMD {0}, {1}, {2}", batFileNameWithExtension, apksPath, aabPath);
            var dir = Path.Combine(Application.dataPath, "..", BatchFilePath);
            if(!Directory.Exists(dir))
            {
                Debug.LogErrorFormat("{0} dones't exist", dir);
            }
            if(!Directory.Exists(dir))
            {
                Debug.LogErrorFormat("dir {0} dones't exist", dir);
                return null; 
            }
            var file = Path.Combine(dir, batFileNameWithExtension);
            if(!File.Exists(file))
            {
                Debug.LogErrorFormat("file {0} dones't exist", file);
                return null;
            }
            if(string.IsNullOrEmpty(apksPath))
            {
                LogFail(file, "apkPath is null or empty, you should pass a valid path");
                return null;
            }
            if(string.IsNullOrEmpty(aabPath))
            {
                LogFail(file, "aabPath is null or empty, you should pass a valid path");
                return null;
            }
            var cmd = File.ReadAllText(file);
            cmd = cmd.Replace("AAB_PATH=", string.Format("AAB_PATH=\"{0}\"", aabPath.Replace("/", "\\"))).Replace("APKS_PATH=", string.Format("APKS_PATH=\"{0}\"", apksPath.Replace("/", "\\")));
#if UNITY_EDITOR_OSX
            cmd = cmd.Replace("\\", "/");
            var tmpCmd = Path.Combine(dir, "tmp.sh");
#else
            var tmpCmd = Path.Combine(dir, "tmp.cmd");
#endif
            Debug.LogFormat("{0}:{1}", tmpCmd, cmd);
            File.WriteAllText(tmpCmd, cmd);
            var output = RunBat(dir, tmpCmd);
            File.Delete(tmpCmd);
            return output;
        }

        private static void LogFail(string filePah, string tip)
        {
            Debug.LogErrorFormat("Run batchFile {0} failed with error : {1}", filePah, tip);
        }
    }
}

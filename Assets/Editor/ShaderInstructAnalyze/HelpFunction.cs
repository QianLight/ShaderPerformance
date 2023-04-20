using UnityEngine;
using System.IO;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace ShaderInstructAnalyze
{
    public class HelpFunction
    {   
        public static byte[] LoadFile(string filePath)
        {
            FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            byte[] thebytes = new byte[fs.Length];
            fs.Read(thebytes, 0, (int)fs.Length);
            fs.Close();
            return thebytes;
        }

        public static string[] RunCmd(string cmd, string args, string workingDir = "")
        {
            string[] res = new string[2];
            var p = CreateCmdProcess(cmd, args, workingDir);
            res[0] = p.StandardOutput.ReadToEnd();
            res[1] = p.StandardError.ReadToEnd();
#if !UNITY_IOS
            // res[2] = p.ExitCode.ToString();
#endif
            p.Close();
            return res;
        }
        private static System.Diagnostics.Process CreateCmdProcess(string cmd, string args, string workingDir = "")
        {
            var en = System.Text.UTF8Encoding.UTF8;
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                en = System.Text.Encoding.GetEncoding("gb2312");
            }
            var pStartInfo = new System.Diagnostics.ProcessStartInfo(cmd);
            pStartInfo.Arguments = args;
            pStartInfo.CreateNoWindow = true;
            pStartInfo.UseShellExecute = false;
            pStartInfo.RedirectStandardError = true;
            pStartInfo.RedirectStandardInput = true;
            pStartInfo.RedirectStandardOutput = true;
            pStartInfo.StandardErrorEncoding = en;
            pStartInfo.StandardOutputEncoding = en;
            if (!string.IsNullOrEmpty(workingDir))
            {
                pStartInfo.WorkingDirectory = workingDir;
            }
            return System.Diagnostics.Process.Start(pStartInfo);
        }
        public static void SaveToFile(string text, string fileDirectory, string fileName)
        {
            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            var streamWriter = new StreamWriter(fileDirectory + fileName);
            streamWriter.Write(text);
            streamWriter.Close();
        }

        public static List<FileInfo> GetAllFiles(ref List<FileInfo> FileList, DirectoryInfo dir, in string pattern)
        {
            if(FileList == null)
            {
                FileList = new List<FileInfo>();
            }
            FileInfo[] allFile = dir.GetFiles(pattern);
            foreach (FileInfo fi in allFile)
            {
                FileList.Add(fi);
            }
            DirectoryInfo[] allDir = dir.GetDirectories();
            foreach (DirectoryInfo d in allDir)
            {
                GetAllFiles(ref FileList, d, pattern);
            }
            return FileList;
        }
    }
}

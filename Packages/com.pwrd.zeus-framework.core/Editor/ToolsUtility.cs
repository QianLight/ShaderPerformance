/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.IO;

#if UNITY_EDITOR
public class ToolsUtility {

    public static void RunCmd(string cmdArguments)
    {
        Console.InputEncoding = System.Text.Encoding.UTF8;
        Process process = new Process();
        {
            process.StartInfo.FileName = "cmd";
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardError = true;
            //process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            process.StandardInput.WriteLine(cmdArguments);
            process.StandardInput.AutoFlush = true;
            process.WaitForExit();
        }
    }

    private static string s_MSBuild_EXE_Directory;
    internal static void RunMSbuild(string argments)
    {
        if (string.IsNullOrEmpty(s_MSBuild_EXE_Directory))
        {
            string path = Path.Combine(System.Environment.GetEnvironmentVariable("SystemRoot"), "Microsoft.NET/Framework");
            if (Directory.Exists(path + "64"))
            {
                path += "64";
            }
            var files = Directory.GetFiles(path, "MSBuild.exe", SearchOption.AllDirectories);

            if (files.Length == 0)
            {
                UnityEngine.Debug.LogError("未找到 MSBuild.exe");
                return;
            }

            List<Version> list = new List<Version>();
            foreach (var file in files)
            {
                list.Add(new Version(Path.GetFileName(Path.GetDirectoryName(file)).Substring(1)));
            }
            list.Sort();
            path += "/v" + list[list.Count - 1];
            s_MSBuild_EXE_Directory = path;
        }
        ///<![CDATA[
        /// 注：  cmd.exe 单个参数带空格时需要用双引号包起来， PowerShell.exe 则需要用单引号包起来。
        /// ]]>
        string cmdArguments = string.Format("{0}/MSBuild.exe \"{1}\"", s_MSBuild_EXE_Directory, argments);
        RunCmd(cmdArguments);
    }

    #region Run exe Function
    public static bool IsWindows { get { return Environment.OSVersion.Platform < PlatformID.Unix; } }

    public static Process RunEXE(string fileName,string args,bool useShellExecute) {
        if (!IsWindows) { Debug.Log("Not Windows platform, can't run exe!"); return null; }

        Process process = new Process();
        string fullName = System.IO.Path.GetFullPath(fileName);
        process.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(fullName);
        process.StartInfo.FileName = fullName;
        if (string.IsNullOrEmpty(args))
            process.StartInfo.Arguments = args;
        if (useShellExecute)
        {
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.CreateNoWindow = false;
        }
        process.Start();
        return process;
    }
    public static Process RunEXE(string fileName, string args) {
        return RunEXE(fileName, args, false);
    }

    public static Process RunEXE(string fileName) {
        return RunEXE(fileName, string.Empty);
    }

    public static bool ShowWarmingWindow(string message)
    {
        return EditorUtility.DisplayDialog("警告，请认真阅读提示后选择", message, "OK", "Cancel");
    }

    public static bool DetermindFirstUpdate()
    {
        return !System.IO.Directory.Exists("Assets/Zeus");
    }
    #endregion

    public static void ExecuteCommand(string command, string cmdArguments)
    {
        try
        {
            using(Process process = new Process())
            {
                process.StartInfo.FileName = command;
                process.StartInfo.Arguments = cmdArguments;
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                if (!string.IsNullOrEmpty(output))
                {
                    Debug.Log(command + " " + cmdArguments + " : " + output);
                }
                process.WaitForExit();
            }
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }
    }

    public static void ExecuteCommands(string command, List<string> cmdArguments)
    {
        string commandFileName;
        ProcessStartInfo processStartInfo = new ProcessStartInfo();
        processStartInfo.UseShellExecute = false;
        processStartInfo.CreateNoWindow = true;
        processStartInfo.RedirectStandardOutput = true;
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            commandFileName = "temp" + ".bat";
            processStartInfo.FileName = commandFileName;

        }
        else if(Application.platform == RuntimePlatform.OSXEditor)
        {
            commandFileName = "temp" + ".sh";
            processStartInfo.FileName = "/bin/bash";
            processStartInfo.Arguments = "./" + commandFileName;
        }
        else
        {
            throw new Exception(string.Format("Wrong platform of \"{0}\"", Application.platform));
        }
        using (StreamWriter batFile = new StreamWriter(commandFileName))
        {
            foreach (var str in cmdArguments)
            {
                batFile.WriteLine(command + " " + str);
            }
        }
        using (Process p = new Process())
        {
            p.StartInfo = processStartInfo;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            if (!string.IsNullOrEmpty(output))
            {
                Debug.Log(command + " " + cmdArguments + " : " + output);
            }
            p.WaitForExit();
        }    
        File.Delete(commandFileName);
    }

    //[MenuItem("Zeus/Zeus Framework Update(First Update)",false)]
    public static void FirstUpdate()
    {
        if (ShowWarmingWindow("该操作会覆盖/删除原有的文件，属破坏性操作，是否继续？"))
        {
            //Application.OpenURL(Environment.CurrentDirectory + "/Tools/ScriptsUpdate/ScriptsUpdate.exe");
            //RunEXE("Tools/ScriptsUpdate/ScriptsUpdate.exe");
            if (IsWindows)
            {
                RunEXE("Tools/ZeusDownloader/DownAndExtractFU");
            }
            else
            {
                //RunEXE("pythonw.exe", "Tools/ZeusDownloader/DownAndExtractFrameWorkFU.py");    
            }
        }
    }

    //[MenuItem("Zeus/Zeus Framework Update(First Update)",true)]
    public static bool RunScriptsUpdateEXEValidate() {
        return DetermindFirstUpdate();
    }

    //[MenuItem("Zeus/Zeus Framework Update(Increase Update)",false)]
    public static void IncreaseUpdate()
    {
        if (ShowWarmingWindow("该操作会覆盖/删除原有的文件，属破坏性操作，是否继续？"))
        {
            if (IsWindows)
            {
                RunEXE("Tools/ZeusDownloader/DownAndExtractIU");
            }
            else
            {
                //RunEXE("pythonw.exe", "Tools/ZeusDownloader/DownAndExtractFrameWorkFU.py");    
            }
        }
    }

    //[MenuItem("Zeus/Zeus Engine Local Update", false)]
    public static void ZeusEngineLocalUpdate()
    {
        if (ShowWarmingWindow("该操作会覆盖/删除原有的文件，属破坏性操作，是否继续？"))
        {
            if (IsWindows)
            {
                RunEXE("Tools/ZeusLocalUpdate/LocalUpdate_EngineToFramework");
            }
        }
    }

    //    [MenuItem("Zeus/Zeus Framework Update", true)]
    //    public static bool RunScriptsUpdateEXEValidate() {
    //        return IsWindows;
    //    }
    public static bool IsBatchMode
    {
        get
        {
#if UNITY_2018_2_OR_NEWER
            return Application.isBatchMode;
#else
            return Environment.CommandLine.Contains("-batchmode");
#endif
        }
    }

}
#endif
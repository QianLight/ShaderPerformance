using CFUtilPoolLib;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using XEcsGamePlay;


/**
 *  运行时热加载
 **/ 
public class RuntimeLoad
{
    private static string _result = null;


    [MenuItem("Tools/Table/RuntimeLoad")]
    public static void ReimportTable()
    {
        Process p = new Process();
        p.StartInfo.FileName = "sh";
        p.StartInfo.Arguments = Application.dataPath + "/Editor/Patch/gittable.sh ";

        p.StartInfo.WorkingDirectory = Application.dataPath;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.Start();

        _result = p.StandardOutput.ReadToEnd().Trim();
        p.WaitForExit();
        //UnityEngine.Debug.Log(_result);
        var mc = _result.Split('\n');
        List<string> importedAssets = new List<string>();
        string[] empty = new string[0];
        foreach (var m in mc)
        {
            var m2 = m.Trim();

            if (m2.StartsWith("M") || m2.StartsWith("A"))
            {
                var file = m2.Substring(2).Trim();
                importedAssets.Add("Assets/Table/" + file);
            }
        }
        if (importedAssets.Count > 0)
        {
            CFEngine.Editor.AssetsImporter.OnPostprocessAllAssets(importedAssets.ToArray(), empty, empty, empty);
            CopyTable2Server(importedAssets);
        }
        ReimportSkill();
    }


    private static void CopyTable2Server(List<string> assets)
    {
        string target = "Z://table/";
        foreach (var it in assets)
        {
            FileInfo file = new FileInfo(it);
            var dest = target + file.Name;
            if (File.Exists(dest))
            {
                File.Delete(dest);
            }
            File.Copy(file.FullName, dest);
        }
    }

    public static void ReimportSkill()
    {
        Process p = new Process();
        p.StartInfo.FileName = "sh";
        p.StartInfo.Arguments = Application.dataPath + "/Editor/Patch/gitskill.sh ";

        p.StartInfo.WorkingDirectory = Application.dataPath;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.Start();

        _result = p.StandardOutput.ReadToEnd().Trim();
        p.WaitForExit();
        //UnityEngine.Debug.Log(_result);
        var mc = _result.Split('\n');
        string[] empty = new string[0];
        foreach (var m in mc)
        {
            var m2 = m.Trim();
            if (m2.StartsWith("M") || m2.StartsWith("A"))
            {
                var file = m2.Substring(2).Trim();
                CopySkill2Server(file);
                int idx1 = file.LastIndexOf('/');
                int idx2 = file.LastIndexOf('.');
                file = file.Substring(idx1 + 1, idx2 - idx1 - 1);
                uint hash = XCommon.singleton.XHash(file);
                XDebug.singleton.AddLog("ecs skill: " + file);
                XEcs.singleton.Reload(hash);
            }
        }
    }

    private static void CopySkill2Server(string it)
    {
        string target = "Z://SkillPackage/";
        string path = "Assets/BundleRes/SkillPackage/" + it;
        FileInfo file = new FileInfo(path);
        target = target + it;
        if (File.Exists(target))
        {
            File.Delete(target);
        }
        File.Copy(file.FullName, target);
    }
}

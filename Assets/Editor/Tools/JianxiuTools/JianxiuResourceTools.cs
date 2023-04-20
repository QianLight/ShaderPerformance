using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class JianxiuResourceTools
{
    private static bool IsReplaceJianxiuDir;

    static void SetJianxiuReplaceValue()
    {
        IsReplaceJianxiuDir = false;

        if (Application.isBatchMode)
        {
            IsReplaceJianxiuDir = bool.TryParse(Environment.GetEnvironmentVariable("JianxiuReplace")?.ToLower(), out bool value) && value;
            AppContext.SetSwitch("JianxiuReplace", IsReplaceJianxiuDir);
            Debug.Log("======ReplaceJianxiuDirectory IsReplaceJianxiuDir:" + IsReplaceJianxiuDir);
            if (AppContext.TryGetSwitch("JianxiuReplace", out bool value1) && value1 == false)
            {
                Debug.Log("======ReplaceJianxiuDirectory JianxiuReplace:" + value1);
            }
        }
        else
        {
#if UNITY_EDITOR
            IsReplaceJianxiuDir = true;
#endif
        }
    }

    [MenuItem("Tools/替换监修文件夹")]
    public static void ReplaceJianxiuDirectory()
    {
        SetJianxiuReplaceValue();
        if (IsReplaceJianxiuDir)
        {
            bool fileExits = ReplaceJianxiuResource(false);
            if (fileExits)
            {
                ReplaceJianxiuResource(true);
            }
            else
            {
                if (Application.isBatchMode)
                    throw new Exception("Replace Jianxiu Directory failed.");
            }
        }
    }

    private static bool ReplaceJianxiuResource(bool isReplace, bool overwrite = true)
    {
        bool isExits = true;
        string JianxiuRootPath = Application.dataPath + Path.DirectorySeparatorChar + "Jianxiu";
        JianxiuRootPath = Zeus.Framework.PathUtil.FormatPathSeparator(JianxiuRootPath);
        System.Text.StringBuilder contentBuilder = new System.Text.StringBuilder();
        var allFiles = Directory.GetFiles(JianxiuRootPath, "*", SearchOption.AllDirectories);
        int replaceCount = 0;
        foreach (string file in allFiles)
        {
            if (Path.GetExtension(file) == ".meta")
            {
                continue;
            }
            string desPath = file.Replace(Path.DirectorySeparatorChar + "Jianxiu", "");
            if (!File.Exists(desPath))
            {
                string fileObjectPath = file.Replace(Zeus.Framework.PathUtil.FormatPathSeparator(Application.dataPath), "Assets");
                var fileObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fileObjectPath);
                Debug.LogError("没有找到对应文件替换(单击选中):" + file, fileObject);
                isExits = false;
            }
            if (isReplace)
            {
                replaceCount += 1;
                Zeus.Core.FileUtil.EnsureFolder(desPath);
                UnityEngine.Debug.Log(file + " " + desPath);
                contentBuilder.AppendLine(string.Format(" {0}", file));
                File.Copy(file, desPath, overwrite);
            }
        }
        if (isReplace)
        {
            contentBuilder.AppendLine(string.Format("文件替换数量：{0}", replaceCount));
            WriteReplaceTxt(contentBuilder);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        return isExits;
    }

    private static void WriteReplaceTxt(System.Text.StringBuilder contentBuilder)
    {
        string txtName = string.Format("JianxiuReplace.log");
        string outPath = Application.dataPath.Replace("Assets", "") + "shell/" + txtName;
        string log = contentBuilder.ToString();
        if (File.Exists(outPath))
        {
            File.Delete(outPath);
        }
        File.WriteAllText(outPath, log);
    }
}

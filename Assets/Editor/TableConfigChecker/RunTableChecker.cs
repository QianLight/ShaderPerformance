using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;


public class RunTableChecker
{
    static readonly string workDir = Directory.GetParent(Application.dataPath).Parent.ToString();
    static readonly string version = "1.0";
    static readonly string iniPath = $"{workDir}/tools/TableConfigCheck/userConfig.ini";
    static readonly string venvCfgPath = $"{workDir}/tools/TableConfigCheck/pythonProject/venv/pyvenv.cfg";
    static readonly string venvContent = $"home = {workDir}/tools/TableConfigCheck/Python/Python39\n" +
        $"include-system-site-packages = false\nversion = 3.9.2";
    static readonly string[] primaryTables = new string[] {
        "XEntityStatistics",
        "SkillListForEnemy",
        "CutScene",
        "UnitAITable",
        "SceneList"
    };
    private static void CheckInit()
    {
        if (!File.Exists(iniPath))
        {
            InitPythonEnv(iniPath);
        }
        else
        {
            var info = File.ReadAllText(iniPath);
            if(info != $"TableConfigCheck={version}")
            {
                InitPythonEnv(iniPath);
            }
        }
    }

    private static void InitPythonEnv(string iniPath)
    {
        string resultDir = $"{Application.dataPath}/Editor/TableConfigChecker/CheckResult";
        if (!Directory.Exists(resultDir))
        {
            Directory.CreateDirectory(resultDir);
        }

        using(var cfgFile = File.Open(venvCfgPath, FileMode.Create))
        {
            var content = Encoding.UTF8.GetBytes(venvContent);
            cfgFile.Write(content, 0, content.Length);
        }

        using (var file = File.Open(iniPath, FileMode.Create))
        {
            var info = Encoding.UTF8.GetBytes($"TableConfigCheck={version}");
            file.Write(info, 0, info.Length);
        }
    }

    [MenuItem("Tools/Table/TableChecker/CheckAll")]
    private static void DoCheckTable()
    {
        CheckInit();
        Process p = new Process();
        p.StartInfo.FileName = $"{Application.dataPath}/Editor/TableConfigChecker/do_table_check.bat";
        p.StartInfo.Arguments = $"ALL {Application.dataPath} {Application.dataPath}/Editor/TableConfigChecker/CheckResult";

        p.StartInfo.WorkingDirectory = $"{workDir}";
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.Start();
        UnityEngine.Debug.Log("");
    }

    public static void DoCheckIfPrimary(string tableNames)
    {
        List<string> tables = tableNames.Split(' ').ToList();
        var primaryList = primaryTables.ToList();
        var result = tables.FindAll(name => primaryList.Contains(name));
        if (result.Count > 0)
        {
            string checkTables = string.Join("_", result);
            DoCheckTableByNames(checkTables);
        }
    }

    [MenuItem("Tools/Table/TableChecker/CheckPrimary")]
    private static void DoCheckTablePrimary()
    {
        string tableNames = string.Join("_", primaryTables);
        DoCheckTableByNames(tableNames);
    }
    private static void DoCheckTableByNames(string tableNames)
    {
        CheckInit();
        UnityEngine.Debug.Log($"Do Check Table {tableNames}");
        Process p = new Process();
        p.StartInfo.FileName = $"{Application.dataPath}/Editor/TableConfigChecker/do_table_check.bat";
        p.StartInfo.Arguments = $"SINGLE_{tableNames} {Application.dataPath} {Application.dataPath}/Editor/TableConfigChecker/CheckResult";
        p.StartInfo.WorkingDirectory = $"{workDir}";
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.StandardOutputEncoding = Encoding.GetEncoding("GB2312");
        p.Start();
        var result = p.StandardOutput.ReadToEnd().Trim().Split('\n');
        bool isErr = false;
        StringBuilder output = new StringBuilder();
        foreach(string res in result)
        {
            if (res.StartsWith("[ERROR]"))
                isErr = true;
            if (res.Trim() == "[ERRFINISH]")
                isErr = false;
            if (isErr)
            {
                output.AppendLine(res);
            }
        }
        if(output.Length > 0)
        {
            EditorUtility.DisplayDialog("配表错误", output.ToString(), "确定");
            UnityEngine.Debug.LogError($"策划配表错误\n{output}");
        }
    }
}


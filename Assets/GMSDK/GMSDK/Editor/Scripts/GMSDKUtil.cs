using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using GSDK;

public class GMSDKUtil
{
    #region 工具接口

    // 检查Unity3D IDE版本是否低于 version
    public static bool isUnityEarlierThan(string version)
    {
        string unityVersion = Application.unityVersion;
        if (unityVersion.Length > version.Length)
        {
            unityVersion = unityVersion.Substring(0, version.Length);
        }
        else
        {
            version = version.Substring(0, unityVersion.Length);
        }

        if (unityVersion.CompareTo(version) < 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion

    #region 文本操作

    // 读取 key = vaule 类的配置
    public static Dictionary<string, string> ReadConfigs(string filePath)
    {
        Debug.Log("filePath:" + filePath);
        Dictionary<string, string> configs = new Dictionary<string, string>();
        string[] lines = File.ReadAllLines(filePath);
        string pattern = @"^([(^;)\w\?\.\\:/]+)\s*=\s*([\w\?\.\\:=/]+)\s*";
        foreach (var line in lines)
        {
            foreach (Match m in Regex.Matches(line, pattern))
            {
                if (!m.Groups[0].Success)
                {
                    continue;
                }

                string key = m.Groups[1].Value;
                string value = m.Groups[2].Value;
                Debug.Log("key:" + key + ",value:" + value);
                configs.Add(key, value);
            }
        }

        return configs;
    }

    // 将 below 的下一行替换为 text
    public static void ReplaceBelow(string fileFullPath, string below, string text)
    {
        StreamReader streamReader = new StreamReader(fileFullPath);
        string text_all = streamReader.ReadToEnd();
        streamReader.Close();

        text_all = System.Text.RegularExpressions.Regex.Replace(text_all, "\r\n", "\n");

        int firstIndex = text_all.IndexOf(below);
        if (firstIndex == -1)
        {
            Debug.LogError(fileFullPath + "中没有找到标志" + below);
            return;
        }

        int beginIndex = text_all.IndexOf("\n", firstIndex + below.Length);
        int endIndex = text_all.IndexOf("\n", beginIndex + 1);

        text_all = text_all.Substring(0, beginIndex) + "\n" + text + "\n" + text_all.Substring(endIndex + 1);

        StreamWriter streamWriter = new StreamWriter(fileFullPath);
        streamWriter.Write(text_all);
        streamWriter.Close();
    }

    // 在 below 下行添加 text
    public static void WriteBelow(string filePath, string below, string text)
    {
        WriteBelowWithAlterBelow(filePath, below, "", text);
    }

    // 在 below 下行添加 text
    public static void WriteBelowWithAlterBelow(string filePath, string below, string alterBelow, string text)
    {
        StreamReader streamReader = new StreamReader(filePath);
        string text_all = streamReader.ReadToEnd();
        streamReader.Close();

        text_all = System.Text.RegularExpressions.Regex.Replace(text_all, "\r\n", "\n");

        int beginIndex = text_all.IndexOf(below);
        if (beginIndex == -1)
        {
            if (alterBelow.Length > 0)
            {
                beginIndex = text_all.IndexOf(alterBelow);
                if (beginIndex == -1)
                {
                    Debug.LogError(filePath + "中没有找到标志" + alterBelow);
                    return;
                }

                below = alterBelow;
            }
            else
            {
                Debug.LogError(filePath + "中没有找到标志" + below);
                return;
            }
        }

        int endIndex = text_all.LastIndexOf("\n", beginIndex + below.Length);

        if (!text_all.Substring(endIndex, text.Length + 2).Contains(text))
        {
            text_all = text_all.Substring(0, endIndex) + "\n" + text + "\n" + text_all.Substring(endIndex);
        }

        StreamWriter streamWriter = new StreamWriter(filePath);
        streamWriter.Write(text_all);
        streamWriter.Close();
    }

    // 在 uplow 上行添加 text
    public static void WriteUplow(string filePath, string uplow, string text)
    {
        StreamReader streamReader = new StreamReader(filePath);
        string text_all = streamReader.ReadToEnd();
        streamReader.Close();

        text_all = System.Text.RegularExpressions.Regex.Replace(text_all, "\r\n", "\n");

        int beginIndex = text_all.IndexOf(uplow);
        if (beginIndex == -1)
        {
            Debug.LogError(filePath + "中没有找到标志" + uplow);
            return;
        }

        int endIndex = beginIndex;
        beginIndex -= 1;


        //int endIndex = text_all.LastIndexOf("\n", beginIndex + uplow.Length);

        if (!text_all.Substring(0, endIndex).Contains(text))
        {
            text_all = text_all.Substring(0, beginIndex) + "\n" + text + "\n" + text_all.Substring(endIndex);
        }

        StreamWriter streamWriter = new StreamWriter(filePath);
        streamWriter.Write(text_all);
        streamWriter.Close();
    }

    //查找某个文件中是否存在某一行string
    public static bool ExistLine(string filePath, string str)
    {
        StreamReader streamReader = new StreamReader(filePath);
        string text_all = streamReader.ReadToEnd();
        streamReader.Close();

        text_all = System.Text.RegularExpressions.Regex.Replace(text_all, "\r\n", "\n");

        int beginIndex = text_all.IndexOf(str);
        if (beginIndex == -1)
        {
            return false;
        }

        return true;
    }

    // 在 below 下将 第一个oldString行 替换为 newString行
    public static void ReplaceLineBelow(string filePath, string below, string oldString, string newString)
    {
        ReplaceLineBelowWithAlterBelow(filePath, below, "", oldString, newString);
    }

    public static void ReplaceLineBelowWithAlterBelow(string filePath, string below, string alterBelow,
        string oldString, string newString)
    {
        StreamReader streamReader = new StreamReader(filePath);
        string text_all = streamReader.ReadToEnd();
        streamReader.Close();

        text_all = System.Text.RegularExpressions.Regex.Replace(text_all, "\r\n", "\n");

        int beginIndex = text_all.IndexOf(below);
        if (beginIndex == -1)
        {
            if (alterBelow.Length > 0)
            {
                beginIndex = text_all.IndexOf(alterBelow);
                if (beginIndex == -1)
                {
                    Debug.LogError(filePath + "中没有找到标志" + alterBelow);
                    return;
                }

                below = alterBelow;
            }
            else
            {
                Debug.LogError(filePath + "中没有找到标志" + below);
                return;
            }
        }

        int endIndex = beginIndex + below.Length;
        int n = 0;
        string[] lines = File.ReadAllLines(filePath);
        for (int i = 0; i < lines.Length; i++)
        {
            n += lines[i].Length;
            n++;

            if (n >= endIndex)
            {
                for (int j = i; j < lines.Length; j++)
                {
                    if (lines[j].IndexOf(oldString) != -1)
                    {
                        lines[j] = newString;

                        break;
                    }

                    if (j == lines.Length - 1)
                    {
                        Debug.LogError(filePath + "中没有找到标志" + oldString);
                    }
                }

                break;
            }

            if (i == lines.Length - 1)
            {
                Debug.LogError(filePath + "中没有找到标志" + below);
            }
        }

        File.WriteAllLines(filePath, lines);
    }

    // 替换文件的匹配的文本
    public static void ReplaceText(string fileFullPath, string oldText, string newText)
    {
        Debug.Log(fileFullPath);
        string[] lines = File.ReadAllLines(fileFullPath);
        for (int i = 0; i < lines.Length; i++)
        {
            if (Regex.IsMatch(lines[i], oldText))
            {
                Debug.Log(oldText);
                lines[i] = lines[i].Replace(oldText, newText);
            }
        }

        File.WriteAllLines(fileFullPath, lines);
    }

    // 正则表达式替换匹配的文本
    public static void ReplaceTextStringWithRegex(string fileFullPath, string regexString, string replaceString)
    {
        string[] lines = File.ReadAllLines(fileFullPath);
        for (int i = 0; i < lines.Length; i++)
        {
            if (Regex.IsMatch(lines[i], regexString))
            {
                Match match = Regex.Match(lines[i], regexString);
                lines[i] = lines[i].Replace(match.Value, replaceString);
            }
        }

        File.WriteAllLines(fileFullPath, lines);
    }

    // 正则表达式替换文件的行
    public static void ReplaceTextWithRegex(string fileFullPath, string regexString, string replaceString)
    {
        string[] lines = File.ReadAllLines(fileFullPath);
        for (int i = 0; i < lines.Length; i++)
        {
            if (Regex.IsMatch(lines[i], regexString))
            {
                Debug.Log("match:" + lines[i]);
                lines[i] = replaceString;
            }
        }

        File.WriteAllLines(fileFullPath, lines);
    }

    // 正则表达式批量替换文件的行
    public static void ReplaceTextWithRegex(string fileFullPath, Dictionary<string, string> regexRules)
    {
        if (!System.IO.File.Exists(fileFullPath))
        {
            return;
        }

        string[] lines = File.ReadAllLines(fileFullPath);
        for (int i = 0; i < lines.Length; i++)
        {
            foreach (KeyValuePair<string, string> rule in regexRules)
            {
                //Debug.LogError(i+" , lines = "+ lines[i] + " , key = " + rule.Key);
                if (Regex.IsMatch(lines[i], rule.Key))
                {
                    lines[i] = rule.Value;
                }
            }
        }

        File.WriteAllLines(fileFullPath, lines);
    }

    #endregion

    #region 文件操作

    // 替换文件夹，若目标目录已存在此文件夹则删除后复制
    public static void ReplaceDir(string srcPath, string destPath)
    {
        Debug.Log("=========copy file from:" + srcPath + " to:" + destPath);
        if (Directory.Exists(destPath))
        {
            try
            {
                Directory.Delete(destPath, true);
            }
            catch (IOException e)
            {
                Debug.LogException(e);
                return;
            }
        }

        CopyDir(srcPath, destPath, true);
    }

    // 复制文件夹，包括子目录和子文件，cover为true时覆盖已有文件(不包括meta文件)
    public static void CopyDir(string srcPath, string destPath, bool cover)
    {
        if (Directory.Exists(srcPath))
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            string[] files = Directory.GetFiles(srcPath);
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destPath, fileName);
                string extention = Path.GetExtension(file);
                if (extention != ".meta")
                {
                    try
                    {
                        File.Copy(file, destFile, cover);
                    }
                    catch (IOException e)
                    {
                        Debug.Log(e.Message);
                    }
                }
            }

            string[] dirs = Directory.GetDirectories(srcPath);
            foreach (string dir in dirs)
            {
                string dirName = Path.GetFileName(dir);
                if (".svn".Equals(dirName))
                {
                    continue;
                }

                string destDir = Path.Combine(destPath, dirName);
                CopyDir(dir, destDir, cover);
            }
        }
    }

    //复制文件. 若目标已存在, duplicate为true时则创建副本, 否则不进行复制. 返回目标文件路径.
    public static string CopyFile(string srcFile, string destFile, bool duplicate)
    {
        string fileName = Path.GetFileName(destFile);
        if (File.Exists(destFile))
        {
            if (duplicate)
            {
                string dirPath = Path.GetDirectoryName(destFile);
                fileName = "Copy_" + fileName;
                destFile = Path.Combine(dirPath, fileName);
            }
            else
            {
                return destFile;
            }
        }

        File.Copy(srcFile, destFile, true);
        return destFile;
    }

    //复制文件. 若目标已存在, duplicate为true时则创建副本, 否则不进行复制. 返回目标文件路径.
    public static string CopyFileWithOverwrite(string srcFile, string destFile, bool overwrite = true)
    {
        if (File.Exists(destFile))
        {
            if (overwrite)
            {
                if (File.Exists(destFile))
                {
                    File.Delete(destFile);
                }
            }
            else
            {
                return destFile;
            }
        }

        File.Copy(srcFile, destFile, true);
        return destFile;
    }

    // 计算文件MD5
    public static string GetFileMd5(string filePath)
    {
        try
        {
            FileStream file = new FileStream(filePath, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }

            return sb.ToString();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return null;
        }
    }

    #endregion

    #region Xml操作

    // 节点或其子节点是否包含属性
    public static bool XmlInclue(XmlNode parent, string attributes, string value = "", bool recursivity = false)
    {
        if (parent == null || parent.Attributes == null)
        {
            return false;
        }

        XmlAttribute attr = parent.Attributes[attributes];
        if (attr != null)
        {
            if (String.IsNullOrEmpty(value) || attr.Value.Equals(value))
            {
                return true;
            }
        }

        if (!recursivity)
        {
            return false;
        }

        if (!parent.HasChildNodes)
        {
            return false;
        }

        XmlNodeList childs = parent.ChildNodes;
        foreach (XmlNode child in childs)
        {
            if (XmlInclue(child, attributes, value, recursivity))
            {
                return true;
            }
        }

        return false;
    }


    // 字点中包含子节点 <keyType>key</keyType>，且此子节点下一节点为 <valueType>value</valueType>
    public static bool XmlMath(XmlNode parent, string keyType, string key, string valueType, string value = "")
    {
        if (parent == null)
        {
            return false;
        }

        XmlNode keyChild = parent.SelectSingleNode(keyType + "[. = '" + key + "']");
        if (keyChild == null || keyChild.NextSibling == null)
        {
            return false;
        }

        if (!keyChild.NextSibling.Name.Equals(valueType))
        {
            return false;
        }

        if (String.IsNullOrEmpty(value))
        {
            return true;
        }

        if (keyChild.NextSibling.InnerText.Equals(value))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // 读取父节点某子节点下一节点的节点名
    public static string XmlNextName(XmlNode parent, string keyType, string key)
    {
        if (parent == null)
        {
            return "";
        }

        XmlNode keyChild = parent.SelectSingleNode(keyType + "[. = '" + key + "']");
        if (keyChild == null || keyChild.NextSibling == null)
        {
            return "";
        }

        return keyChild.NextSibling.Name;
    }

    // 读取父节点某子节点下一节点的值
    public static string XmlNextValue(XmlNode parent, string keyType, string key, string valueType)
    {
        if (parent == null)
        {
            return null;
        }

        XmlNode keyChild = parent.SelectSingleNode(keyType + "[. = '" + key + "']");
        if (keyChild == null || keyChild.NextSibling == null)
        {
            return null;
        }

        if (!keyChild.NextSibling.Name.Equals(valueType))
        {
            return null;
        }

        return keyChild.NextSibling.InnerText;
    }

    #endregion

    public static string RunCmdSync(string cmd, string args, string workingDirectory, bool returnResult)
    {
        var pStartInfo = new System.Diagnostics.ProcessStartInfo(cmd);
        pStartInfo.Arguments = args;
        pStartInfo.CreateNoWindow = false;
        pStartInfo.UseShellExecute = false;
        pStartInfo.RedirectStandardError = false;
        pStartInfo.RedirectStandardInput = false;
        pStartInfo.RedirectStandardOutput = returnResult;
        pStartInfo.ErrorDialog = false;
        pStartInfo.WorkingDirectory = workingDirectory;
        var process = new System.Diagnostics.Process
        {
            StartInfo = pStartInfo,
        };
        Debug.Log("working dir : " + workingDirectory);
        process.Start();
        process.WaitForExit();
        if (returnResult) {
            var output = process.StandardOutput;
            string result = output.ReadToEnd();
            process.Close();
            return result;
        } else {
            return "";
        }
    }

    private static string ParseOption(string option, string prefix)
    {
        if (option.StartsWith(prefix))
        {
            return option.Substring(prefix.Length);
        }
        return null;
    }

    public static Dictionary<string, string> ParseCommandLineArgs()
    {
        Dictionary<string, string> dic = new Dictionary<string, string>();
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            var option = args[i];
            if (option.StartsWith("--"))
            {
                if (option.StartsWith("--enable-") || option.StartsWith("--disable-"))
                {
                    continue;
                }
                var key = ParseOption(option, "--");
                var next = i <= args.Length ? args[i + 1] : null;
                if (next != null && !next.StartsWith("-"))
                {
                    dic[key] = next;
                }
                else
                {
                    dic[key] = "true";
                }
            }
        }

        return dic;
    }

    public static Dictionary<string, bool> ParseCommandLineMacros()
    {
        Dictionary<string, bool> macros = new Dictionary<string, bool>();
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            var option = args[i];
            if (option.StartsWith("--"))
            {
                var key = ParseOption(option, "--enable-");
                if (key != null)
                {
                    macros[key] = true;
                    continue;
                }
                key = ParseOption(option, "--disable-");
                if (key != null)
                {
                    macros[key] = false;
                }
            }
        }
        return macros;
    }

    public static void UpdateDefineSymbols(Dictionary<string, bool> SymbolMap)
    {
        if (SymbolMap.Count <= 0)
        {
            return;
        }
        var buildTargets = new UnityEditor.BuildTargetGroup[] {
            UnityEditor.BuildTargetGroup.iOS,
            UnityEditor.BuildTargetGroup.Android,
            UnityEditor.BuildTargetGroup.Standalone,
        };
        foreach (var buildTarget in buildTargets)
        {
            var updater = new SymbolUpdater(buildTarget);
            foreach (var pair in SymbolMap)
            {
                if (pair.Value)
                {
                    updater.AddSymbol(pair.Key);
                }
                else
                {
                    updater.RemoveSymbol(pair.Key);
                }
            }
            updater.Save();
        }
    }

    public class SymbolUpdater
    {
        private UnityEditor.BuildTargetGroup TargetGroup;
        private List<string> SymbolList;

        public SymbolUpdater(UnityEditor.BuildTargetGroup targetGroup)
        {
            TargetGroup = targetGroup;
            var symbolsContent = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            if (symbolsContent == null)
            {
                symbolsContent = "";
            }
            var symbols = symbolsContent.Split(';');
            SymbolList = new List<string>(symbols);
        }

        public bool HasSymbol(string symbol)
        {
            return SymbolList.Contains(symbol);
        }

        public SymbolUpdater RemoveSymbol(string symbol)
        {
            SymbolList.RemoveAll(item => item == symbol);
            return this;
        }

        public SymbolUpdater RemoveSymbols(string[] symbols)
        {
            foreach (var symbol in symbols)
            {
                RemoveSymbol(symbol);
            }
            return this;
        }

        public SymbolUpdater AddSymbol(string symbol)
        {
            if (!SymbolList.Contains(symbol))
            {
                SymbolList.Add(symbol);
            }
            return this;
        }

        public SymbolUpdater AddSymbols(string[] symbols)
        {
            foreach (var symbol in symbols)
            {
                AddSymbol(symbol);
            }
            return this;
        }

        public void Save()
        {
            string symbolsContent = string.Join(";", SymbolList.ToArray());

            UnityEngine.Debug.Log("Applying symbols for " + TargetGroup.ToString() + ": " + symbolsContent);
            UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(TargetGroup, symbolsContent);
        }
    }
}
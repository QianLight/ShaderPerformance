using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System;

namespace AssetCheck
{
    public static class AssetHelper
    {
        const string CustomFilterPrefix = "custom:";
        public static bool IsCustomFilter(string filter, out string customFilter)
        {
            if (filter.StartsWith(CustomFilterPrefix))
            {
                customFilter = filter.Substring(CustomFilterPrefix.Length);
                return true;
            }
            else
            {
                customFilter = filter;
                return false;
            }
        }
        public static List<string> FindAssets(string filter, string path, List<string> excludePaths, string includeKeyword)
        {
            List<string> result = new List<string>();
            if (path == null || path.Equals(string.Empty))
                return result;
            string[] assetsGUIDs = AssetDatabase.FindAssets(filter, new string[] { path });
            foreach (var assetsGUID in assetsGUIDs)
            {
                string assetsPath = AssetDatabase.GUIDToAssetPath(assetsGUID);
                bool bInExclude = false;
                foreach (var excludePath in excludePaths)
                {
                    if (assetsPath.Contains(excludePath))
                    {
                        bInExclude = true;
                        break;
                    }
                }
                if (!bInExclude && (string.IsNullOrEmpty(includeKeyword) || assetsPath.Contains(includeKeyword)))
                    result.Add(assetsPath);
            }
            return result;
        }

        // 这里的filter的函数参数，必须前三个参数是查找的path，排除的excludePaths和包含的includePaths
        public static List<string> FindAssetCustom(string filter, string path, List<string> excludePaths, string includeKeyword)
        {
            string[] splitsFunc = filter.Split(',');
            string function = splitsFunc[0];
            string[] splits = function.Split('.');
            string typeName = string.Empty;
            string methodName = string.Empty;
            if (splits.Length == 2)
            {
                typeName = splits[0];
                methodName = splits[1];
            }
            else if (splits.Length == 3)
            {
                typeName = $"{splits[0]}.{splits[1]}";
                methodName = splits[2];
            }
            else
            {
                UnityEngine.Debug.LogError($"Can not parse FindAssetCustom filter = {filter}");
                return new List<string>();
            }

            Assembly[] assemblys = AppDomain.CurrentDomain.GetAssemblies();
            Type t = null;
            foreach (var assembly in assemblys)
            {
                t = assembly.GetType(typeName);
                if (t != null)
                    break;
            }
            if (t == null)
            {
                UnityEngine.Debug.LogError($"Can not find type FindAssetCustom filter = {filter}");
                return new List<string>();
            }
            MethodInfo method = t.GetMethod(methodName);
            if (method == null)
            {
                UnityEngine.Debug.LogError($"Can not find method FindAssetCustom filter = {filter}");
                return new List<string>();
            }
            int paramLength = method.GetParameters().Length;
            object[] functionParams = new object[paramLength];
            if (paramLength == 0)
            {
                functionParams = new object[] { };
            }
            else
            {
                functionParams = new object[paramLength];
            }
            functionParams[0] = path;
            functionParams[1] = excludePaths;
            functionParams[2] = includeKeyword;
            for (int i = 2; i < paramLength; i++)
            {
                if (splitsFunc.Length > i - 1)
                {
                    functionParams[i] = splitsFunc[i - 1];
                }
                else
                {
                    functionParams[i] = null;
                }
            }
            return (List<string>)method.Invoke(null, functionParams);
        }

        public static List<string> FindAllFiles(string path, List<string> excludePaths, string includeKeyword, string filter)
        {
            List<string> result = new List<string>();
            if (path == null || path.Equals(string.Empty))
                return result;
            string dir = Directory.GetParent(Application.dataPath).FullName + "/" + path;
            if (!Directory.Exists(dir))
                return result;
            var files = Directory.GetFiles(dir, filter != null ? filter : "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                bool bInExclude = false;
                string filePath = file.Replace("\\", "/");
                foreach (var excludePath in excludePaths)
                {
                    if (filePath.Contains(excludePath))
                    {
                        bInExclude = true;
                        break;
                    }
                }

                if (filePath.EndsWith(".meta"))
                {
                    bInExclude = true;
                }
                if (!bInExclude && (string.IsNullOrEmpty(includeKeyword) || filePath.Contains(includeKeyword)))
                {
                    result.Add(filePath.Substring(Directory.GetParent(Application.dataPath).FullName.Length + 1));
                }
            }
            return result;
        }

        public static string[] StringToMultiple(string multipleString)
        {
            return multipleString.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
        }

        public static string MultipleToString(string[] strs)
        {
            return string.Join(",", strs);
        }

        public static bool SplitFilePathAndName(string fileAndPath, out string filePath, out string fileName)
        {
            if (!File.Exists(fileAndPath))
            {
                filePath = string.Empty;
                fileName = string.Empty;
                return false;
            }
            FileInfo fileInfo = new FileInfo(fileAndPath);
            filePath = fileInfo.DirectoryName;
            fileName = fileInfo.Name;
            return true;
        }

        public static bool SplitFileRelativePathAndName(string fileAndPath, out string filePath, out string fileName)
        {
            if (!File.Exists(fileAndPath))
            {
                filePath = string.Empty;
                fileName = string.Empty;
                return false;
            }
            FileInfo fileInfo = new FileInfo(fileAndPath);
            filePath = fileInfo.DirectoryName.Substring(Directory.GetParent(Application.dataPath).FullName.Length + 1);
            fileName = fileInfo.Name;
            return true;
        }

        public static string[] SelectMultipleStrings(string[] strs, int mask)
        {
            List<string> listResult = new List<string>();
            for (int i = 0; i < strs.Length; i++)
            {
                int flag = mask & (1 << i);
                if (flag != 0)
                {
                    listResult.Add(strs[i]);
                }
            }
            return listResult.ToArray();
        }

        const string CameraName = "AssetCheckCamera";
        public static Camera GetOrCreateCamera()
        {
            Camera camera = null;
            GameObject gObject = GameObject.Find(CameraName);
            if (gObject != null)
            {
                camera = gObject.GetComponent<Camera>();
            }
            else
            {
                gObject = new GameObject(CameraName);
            }
            if (camera == null)
            {
                camera = gObject.AddComponent<Camera>();
            }
            return camera;
        }

        static string LastScene = string.Empty;
        public static void OpenScene(string scenePath)
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Single);
        }

        public static void BackLastScene()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(Defines.ScenePath);
        }

        public static void WriteAllText(string path, string content)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, content);
        }

        public static void SaveCSV(string filePath, List<List<string>> excelData)
        {
            FileInfo fi = new FileInfo(filePath);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
            if (fi.Exists)
                fi.Delete();
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                {
                    for (int x = 0; x < excelData.Count; x++)
                    {
                        if (x != 0)
                            sw.Write("\n");
                        for (int y = 0; y < excelData[x].Count; y++)
                        {
                            if (y != 0)
                                sw.Write(",");
                            sw.Write(excelData[x][y]);
                        }
                    }
                }
            }
        }

        public static void SaveCSV(string filePath, string[,] excelData)
        {
            FileInfo fi = new FileInfo(filePath);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
            if (fi.Exists)
                fi.Delete();
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                {
                    for (int x = 0; x < excelData.GetLength(0); x++)
                    {
                        if (x != 0)
                            sw.Write("\n");
                        for (int y = 0; y < excelData.GetLength(1); y++)
                        {
                            if (y != 0)
                                sw.Write(",");
                            sw.Write(excelData[x, y]);
                        }
                    }
                }
            }
        }

        public static void OpenFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder)) return;
            if (!Directory.Exists(folder))
            {
                UnityEngine.Debug.LogError($"No Directory: {folder}");
                return;
            }
            //int lastIndex = Application.dataPath.LastIndexOf("/");
            //Thread newThread = new Thread(new ParameterizedThreadStart(CmdOpenDirectory));
            //newThread.Start(folder);
            //if (Directory.Exists(folder))
            //    System.Diagnostics.Process.Start("explorer.exe", folder);
            CmdOpenDirectory(Directory.GetParent(Application.dataPath).FullName + "/" + folder);
        }

        private static void CmdOpenDirectory(object obj)
        {
            Process p = new Process();
#if UNITY_EDITOR_WIN
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/c start " + obj.ToString();
#elif UNITY_EDITOR_OSX
            p.StartInfo.FileName = "bash";
            string shPath = Directory.GetParent(Application.dataPath).FullName + "/" + Defines.ShellPath + "openDir.sh";
            p.StartInfo.Arguments = shPath + " " + obj.ToString();
#endif
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();

            p.WaitForExit();
            p.Close();
        }

        public static string GetFrontChar(string str, int length)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            if (str.Length <= length)
                return str;
            return str.Substring(0, length);
        }
    }
}
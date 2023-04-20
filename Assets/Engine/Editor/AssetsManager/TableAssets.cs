using CFUtilPoolLib;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    internal class TableAssets
    {
        public static string tableNames = "";

        public static bool IsTable(string path)
        {
            return path.StartsWith(AssetsConfig.instance.Table_Path) && path.EndsWith(".txt") && !path.EndsWith("*.lua.txt");
        }

        static string s_WriteTimePath = "Shell/cvswritetime";
        static long cvsWriteTimeTicks = 0;
        public static bool ShouldLoadAllTable(string path)
        {
            if (!path.EndsWith("CFUtilPoolLib.dll"))
                return false;

            var oldtime = cvsWriteTimeTicks;
            try
            {
                cvsWriteTimeTicks = File.GetLastWriteTime("Shell/cvs.xml").Ticks;
            }
            catch
            {
                return false;
            }

            if (oldtime == 0)
            {
                
                if (!File.Exists(s_WriteTimePath))
                {
                    var fs = File.Create(s_WriteTimePath);
                    fs.Close();
                }
                else
                {
                    oldtime = File.GetLastWriteTime(s_WriteTimePath).Ticks;
                }
            }

            if (cvsWriteTimeTicks == oldtime)
                return false;

            try
            {
                File.SetLastWriteTime(s_WriteTimePath, new System.DateTime(cvsWriteTimeTicks));
            }
            catch
            {
                return false;
            }
            return true;
        }
        public static string GetTableName(string tablePath)
        {
            tablePath = tablePath.Replace("\\", "/");
            int lastIdxOfSlash = tablePath.LastIndexOf('/');
            if (lastIdxOfSlash >= 0)
                tablePath = tablePath.Substring(lastIdxOfSlash + 1);
            //string tableName = tablePath.Replace(AssetsConfig.instance.Table_Path, "");
            string tableName = tablePath;
            tableName = tableName.Replace(".txt", "");
            return tableName.Replace("\\", "/");
        }

        public static bool ExeTable2Bytes(string tables, string arg0 = "-q -tables")
        {
            bool success = true;
#if UNITY_EDITOR_WIN
            DirectoryInfo dir = new DirectoryInfo("Assets");
            System.Diagnostics.Process exep = new System.Diagnostics.Process();
            exep.StartInfo.FileName = dir.FullName + "/../Shell/Table2Bytes.exe";
            exep.StartInfo.Arguments = arg0 + tables;
            exep.StartInfo.CreateNoWindow = true;
            exep.StartInfo.UseShellExecute = false;
            exep.StartInfo.RedirectStandardOutput = true;
            exep.StartInfo.StandardOutputEncoding = System.Text.Encoding.Default;
            exep.Start();
            string output = exep.StandardOutput.ReadToEnd();
            exep.WaitForExit();
            if (output != "")
            {
                int errorIndex = output.IndexOf("error:");
                if (errorIndex >= 0)
                {
                    string errorStr = output.Substring(errorIndex);
                    Debug.LogError(errorStr);
                    Debug.Log(output.Substring(0, errorIndex));
                    EditorUtility.DisplayDialog("表格生成Bytes失败", errorStr, "截图到bug反馈大群");
                    success = false;
                }
                else
                {
                    Debug.Log(output);
                }
            }
            AssetDatabase.Refresh();

            if (Application.isPlaying)
            {
                var arr = tables.Trim().Split(' ');
                foreach (var it in arr)
                {
                    string table = "Table/" + it.Trim();
                    if (!string.IsNullOrEmpty(table) && XTableAsyncLoader.dic_map.ContainsKey(table))
                    {
                        Debug.Log("runtime reimport: " + it);
                        var csv = XTableAsyncLoader.dic_map[table];
                        csv.Reload(table);
                    }
                    else
                    {
                        Debug.LogError(string.Format("runtime reimport failed, {0} is not registed", it));
                    }
                }
            }
#endif
            return success;
        }
        

        [MenuItem("Tools/Table/CheckNotUsedTable")]
        public static void CheckNotUsed()
        {
            var dir = "Assets/Table";
            DirectoryInfo dirIn = new DirectoryInfo(dir);
            var files = dirIn.GetFiles("*.txt");

            HashSet<string> set = new HashSet<string>();
            foreach (var it in XTableAsyncLoader.dic_map)
            {
                set.Add(it.Key.ToLower());
            }
            foreach (var file in files)
            {
                string table = "Table/" + file.Name.Trim().Replace(".txt", "");
                if (!string.IsNullOrEmpty(table) && !set.Contains(table.ToLower()))
                {
                    if (table != "Table/Updater")
                    {
                        Debug.LogError("table not ued: " + table);
                        AssetDatabase.DeleteAsset("Assets/" + table + ".txt");
                        var bytes = "Assets/BundleRes/" + table + ".bytes";
                        AssetDatabase.DeleteAsset(bytes);
                    }
                }
            }
            set.Clear();
            AssetDatabase.Refresh();
        }

        public static bool DeleteTable(string path)
        {
            string tableName = GetTableName(path);
            string des = AssetsConfig.instance.Table_Bytes_Path + tableName + ".bytes";
            if (File.Exists(des))
            {
                File.Delete(des);
                return true;
            }
            return false;
        }
    }
}
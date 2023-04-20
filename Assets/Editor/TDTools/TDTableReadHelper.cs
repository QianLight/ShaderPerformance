using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using TableEditor;
using UnityEngine;

namespace TDTools
{
    public class TDTableReadHelper
    {
        private static Dictionary<string, TableEditor.TableFullData> pool = new Dictionary<string, TableEditor.TableFullData>();
        private static Dictionary<string, string> Name2Path = new Dictionary<string, string>();

        public static void LoadOrReload(string path, string encode = "Unicode")
        {
            TableEditor.TableFullData fullData = new TableEditor.TableFullData();
            TableEditor.TableReader.ReadTableByFileStream(path, ref fullData, encode);
            pool[path] = fullData;
            string name = Path.GetFileNameWithoutExtension(path);
            Name2Path[name] = path;
        }


        public static void ReloadAll()
        {
            foreach (var item in pool.Keys)
            {
                LoadOrReload(item);
            }
        }
        public static TableData GetTableData(string path, string keyTitle, string value, bool needReload = false,string encode= "Unicode")
        {
            if (!pool.ContainsKey(path) || needReload) LoadOrReload(path, encode);
            TableEditor.TableFullData fullData = pool[path];
            int index = fullData.nameList.IndexOf(keyTitle);
            try
            {
                var result = fullData.dataList.First(item => item.valueList[index] == value);
                return result;
            }
            catch
            {
                Debug.Log($"{path} not has {value} in [{keyTitle}] column");
                return null;
            }
        }

        public static TableData GetTableData(string path, string[] keyTitles, string[] values, bool[] allowEmpty = null, bool needReload = false, string encode = "Unicode")
        {
            if (!pool.ContainsKey(path) || needReload) LoadOrReload(path, encode);
            TableFullData fullData = pool[path];
            try
            {
                List<TableData> resultList = fullData.dataList;
                for (int i = 0; i < keyTitles.Length; ++i)
                {
                    int index = fullData.nameList.IndexOf(keyTitles[i]);
                    resultList = resultList.Where(item => item.valueList[index] == values[i]).ToList();
                }
                if (resultList.Count > 0)
                    return resultList[0];
                resultList = fullData.dataList;
                for (int i = 0; i < keyTitles.Length; ++i)
                {
                    int index = fullData.nameList.IndexOf(keyTitles[i]);
                    resultList = resultList.Where(item => item.valueList[index] == values[i] ||
                        (allowEmpty != null && allowEmpty[i] && item.valueList[index] == string.Empty)).ToList();
                }
                return resultList[0];
            }
            catch
            {
                Debug.Log($"{path} not has {string.Join(",", values)} in [{string.Join(",", keyTitles)}] column");
                return null;
            }
        }

        public static TableFullData GetTableFullData(string path, bool needReload = false, string encode = "Unicode")
        {
            if (!pool.ContainsKey(path) || needReload) LoadOrReload(path, encode);
                return pool[path];
        }

        public static List<string> GetTitleData(string path, string encode = "Unicode")
        {
            if (!pool.ContainsKey(path)) LoadOrReload(path, encode);
            return pool[path]?.nameList;
        }

        public static bool WriteTable(string tablePath, TableFullData data)
        {
            return TableEditor.TableReader.WriteTable(tablePath, data, false);
        }
    }
}

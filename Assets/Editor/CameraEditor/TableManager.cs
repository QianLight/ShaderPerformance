using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CFUtilPoolLib;
using UnityEditor;

namespace TableUtils
{
    static public class TableManager
    {
        static public TableData ReadTable(string tablePath)
        {
            try
            {
                using (StreamReader sr = new StreamReader(tablePath))
                {
                    List<List<string>> content = new List<List<string>>();
                    while (true)
                    {
                        string str = sr.ReadLine();
                        if (str == null || str.Equals(string.Empty)) break;
                        List<string> lineStr = DecodeConfigTxt(str);
                        if (lineStr == null) break;
                        content.Add(lineStr);
                    }
                    TableData td = new TableData(content);
                    content.Clear();
                    return td;
                }
            }
            catch (Exception e)
            {
                XDebug.singleton.AddErrorLog("The file could not be read:");
                XDebug.singleton.AddErrorLog(e.Message);
                return null;
            }
        }

        static public void WriteTable(string tablePath, TableData tableData)
        {
            if (tableData == null) 
            {
                XDebug.singleton.AddWarningLog(string.Format("tableData 为空"));
                return; 
            }
            try
            {
                FileStream fs = null;
                if (!File.Exists(tablePath))
                {
                    XDebug.singleton.AddWarningLog(string.Format("不存在文件：{0}", tablePath));
                    fs = File.Create(tablePath);
                }
                else
                    fs = File.Open(tablePath, FileMode.Truncate);
                if (fs == null) 
                {
                    XDebug.singleton.AddWarningLog(string.Format("文件打开失败：{0}", tablePath));
                    return;
                }
                List<string> configStr = tableData.ToConfigString();
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode))
                {
                    for (int i = 0; i < configStr.Count; ++i)
                        sw.WriteLine(configStr[i]);
                }
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                XDebug.singleton.AddErrorLog("The file could not be write:");
                XDebug.singleton.AddErrorLog(e.Message);
            }
        }
        /// <summary>
        /// 编码 文本组合 将一系列字符串拼接成.txt 需要的形式（一行）
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        static public string EncodeConfigTxt(List<string> content)
        {
            if (content == null || content.Count <= 0) return string.Empty;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < content.Count - 1; ++i)
                sb.Append(content[i]).Append("\t");
            sb.Append(content[content.Count - 1]);
            return sb.ToString();
        }

        /// <summary>
        /// 解码 文本分割 将.txt存储的一行文本拆解成一系列有效字符串
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        static public List<string> DecodeConfigTxt(string content)
        {
            if (content == null || content.Equals(String.Empty)) return null;
            string[] contentSplit = content.Split('\t');
            return new List<string>(contentSplit);
        }

        /// <summary>
        /// 将一系列参数组合成字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="param"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        static public string CompactParam<T>(IEnumerable<T> param, string separator)
        {
            if (param == null) return string.Empty;
            StringBuilder sb = new StringBuilder(20);
            IEnumerator<T> iterator = param.GetEnumerator();
            if(iterator.MoveNext())
                sb.Append(iterator.Current.ToString());
            while(iterator.MoveNext())
                sb.Append(separator).Append(iterator.Current.ToString());
            return sb.ToString();
        }
    }

    public class TableData
    {
        public List<string> title;//标题
        public Dictionary<string, int> titleIndex;//标题对应的编号
        public List<string> comment;//注释
        public List<List<string>> dataStoreByRow;//配置内容
        public TableData(List<List<string>> content)
        {
            if (content == null || content.Count <= 2)
            {
                XDebug.singleton.AddErrorLog("配置内容行数 < 3,是否缺少内容");
                return;
            }
            title = content[0];
            titleIndex = new Dictionary<string, int>();
            for (int i = 0; i < title.Count; ++i)
                titleIndex.Add(title[i], i);
            comment = content[1];
            dataStoreByRow = new List<List<string>>(content.Count - 2);
            for (int i = 2; i < content.Count; ++i)
                dataStoreByRow.Add(content[i]);
        }
        /// <summary>
        /// 查表 找第一个
        /// </summary>
        /// <param name="key">按照这个title查询</param>
        /// <param name="value">title == value</param>
        /// <param name="searchKey">查询这一行title为searchKey 的</param>
        /// <returns></returns>
        public string GetconfigByKey(string key, string value, string searchKey)
        {
            if (titleIndex.ContainsKey(key) && titleIndex.ContainsKey(searchKey))
            {
                int index = titleIndex[key];
                int searchIndex = titleIndex[searchKey];
                for (int i = 0; i < dataStoreByRow.Count; ++i)
                    if (dataStoreByRow[i][index].Equals(value))
                        return dataStoreByRow[i][searchIndex];
                return null;
            }
            else
            {
                XDebug.singleton.AddErrorLog(string.Format("没有指定的title{0} or {1}", key, searchKey));
                return null;
            }
        }

        public bool SetconfigByKey(string key, string value, string searchKey, string setValue)
        {
            if (titleIndex.ContainsKey(key) && titleIndex.ContainsKey(searchKey))
            {
                int index = titleIndex[key];
                int searchIndex = titleIndex[searchKey];
                for (int i = 0; i < dataStoreByRow.Count; ++i)
                    if (dataStoreByRow[i][index].Equals(value))
                    {
                        dataStoreByRow[i][searchIndex] = setValue;
                        return true; 
                    }
                return false;
            }
            else
            {
                XDebug.singleton.AddErrorLog(string.Format("没有指定的title{0} or {1}", key, searchKey));
                return false;
            }
        }
        public List<string> ToConfigString()
        {
            List<string> res = new List<string>(2 + dataStoreByRow.Count);
            res.Add(TableManager.EncodeConfigTxt(title));//加入title
            res.Add(TableManager.EncodeConfigTxt(comment));//加入comment
            for (int i = 0; i < dataStoreByRow.Count; ++i)
                res.Add(TableManager.EncodeConfigTxt(dataStoreByRow[i]));//加入正文
            return res;
        }

    } 
}



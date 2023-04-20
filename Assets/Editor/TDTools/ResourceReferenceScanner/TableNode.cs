using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TDTools.ResourceScanner {
    public class TableNode {
        public HashSet<string> Set;
        public Dictionary<string, int> Dic;
        public List<Dictionary<string, string>> Table;
        public Dictionary<string, string> Comment;

        public string DisplayName;
        public string TableName;
        public string LastModifiedTime;
        public string IDName;

        string[] Header;

        public Dictionary<string, string> GetRowByID(string id) {
            if (Dic.ContainsKey(id))
                return Table[Dic[id]];
            else
                return null;
        }

        public List<string> GetIDList() {
            string[] ids = IDName.Split('|');
            List<string> result = new List<string>();
            for (int i = 0; i < Table.Count; i++) {
                string s = "";
                for (int j = 0; j < ids.Length; j++) {
                    if (j > 0)
                        s += "=";
                    s += Table[i][ids[j]];
                }
                result.Add(s);
            }
            return result;
        }

        public List<string> GetNameList() {
            if (DisplayName == "")
                return GetIDList();

            List<string> result = new List<string>();

            for (int i = 0; i < Table.Count; i++) {
                string s = DisplayName;
                int start = DisplayName.IndexOf('{');
                int end = DisplayName.IndexOf('}');

                while (start >= 0 && end >= 0 && start < end) {
                    string sub = s.Substring(start + 1, end - start - 1);
                    string value;
                    if (sub.Equals("row")) {
                        value = (i + 3).ToString();
                    } else if (!Table[i].ContainsKey(sub)) {
                        break;
                    } else {
                        value = Table[i][sub];
                    }

                    s = $"{s.Substring(0, start)}{value}{s.Substring(end + 1)}";

                    start = s.IndexOf('{');
                    end = s.IndexOf('}');
                }
                result.Add(s);
            }
            return result;
        }

        /// <summary>
        /// 读取一张表格，存入Dic中
        /// </summary>
        /// <param name="table">表格名, 在/table/中</param>
        /// <returns></returns>
        void ReadTable(string table, bool fullpath = false) {
            Table = new List<Dictionary<string, string>>();
            try {
                FileStream fs;
                if (fullpath) {
                    fs = new FileStream(table, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    LastModifiedTime = File.GetLastWriteTime(table).ToString();
                } else {
                    fs = new FileStream($"{Application.dataPath}/table/{table}.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    LastModifiedTime = File.GetLastWriteTime($"{Application.dataPath}/table/{table}.txt").ToString();
                }
                using StreamReader sr = new StreamReader(fs);

                Header = sr.ReadLine().Split('\t');

                Comment = new Dictionary<string, string>();
                string[] comments = sr.ReadLine().Split('\t');
                for (int i = 0; i < Header.Length; i++) {
                    Comment[Header[i]] = comments[i];
                }

                while (!sr.EndOfStream) {
                    string[] row = sr.ReadLine().Split('\t');
                    Dictionary<string, string> rowDic = new Dictionary<string, string>();

                    for (int i = 0; i < Header.Length; i++)
                        rowDic[Header[i]] = row[i];

                    Table.Add(rowDic);
                }
            } catch (Exception e) {

                //弹一个窗警告sharing violation
                Debug.Log("表格读取失败！请确保表格没有被其他程序占用");
                Debug.Log(e.Message);
            }

            Set = new HashSet<string>();
            Dic = new Dictionary<string, int>();
            string[] ids = IDName.Split('|');

            for (int i = 0; i < Table.Count; i++) {
                string s = "";
                for (int j = 0; j < ids.Length; j++) {
                    if (j > 0)
                        s += "=";
                    s += Table[i][ids[j]];
                }
                Dic[s] = i;
            }
        }

        public void Save() {
            try {
                using FileStream fs  = new FileStream($"{Application.dataPath}/table/{TableName}.txt", FileMode.Create, FileAccess.Write, FileShare.Read); ;
                using StreamWriter sr = new StreamWriter(fs, System.Text.Encoding.Unicode);

                for (int i = 0; i < Header.Length; i++) {
                    if (i > 0)
                        sr.Write('\t');
                    sr.Write(Header[i]);
                }
                sr.Write("\r\n");

                for (int i = 0; i < Header.Length; i++) {
                    if (i > 0)
                        sr.Write('\t');
                    sr.Write(Comment[Header[i]]);
                }
                sr.Write("\r\n");

                for (int i = 0; i < Table.Count; i++) {
                    for (int j = 0; j < Header.Length; j++) {
                        if (j > 0)
                            sr.Write('\t');
                        sr.Write(Table[i][Header[j]]);
                    }
                    sr.Write("\r\n");
                }
                sr.Close();
            } catch (Exception e) {
                //弹一个窗警告sharing violation
                Debug.Log("表格读取失败！请确保表格没有被其他程序占用");
                Debug.Log(e.Message);
            }
        }

        public void Reload() {
            ReadTable(TableName);
        }

        public void Modify(string ID, Dictionary<string, string> changes) {
            Reload();

            if (Dic.ContainsKey(ID)) {
                int index = Dic[ID];
                Table[index] = changes;

                Save();
            } else { 
            }
        }

        public TableNode(string path, string idName, string displayName = "") {
            TableName = path;
            DisplayName = displayName;
            IDName = idName;
            ReadTable(path);
        }
    }
}

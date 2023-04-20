using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace TDTools {

    /// <summary>
    /// 读表工具组，用来读取表格
    /// </summary>
    public class TableReader{

        public static readonly string DATAPATH = Application.dataPath;

        /// <summary>
        /// Tables[表格名][行][列] = 数据
        /// </summary>
        public static Dictionary<string, List<Dictionary<string, string>>> Tables {
            get {
                if (_tables == null)
                    _tables = new Dictionary<string, List<Dictionary<string, string>>>();
                return _tables;
            }

        }
        private static Dictionary<string, List<Dictionary<string, string>>> _tables;



        /// <summary>
        /// 以某一列中的数据进行筛选，获得字典
        /// 返回一个字典，以目标列的数据为Key
        /// </summary>
        /// <param name="table">目标的表格</param>
        /// <param name="columnToFilter"></param>
        /// <returns>Result[筛选的列数据][行][列] = 数据</returns>
        public static Dictionary<string, List<Dictionary<string, string>>> FilterTableByColumn(string table, string columnToFilter) {
            Dictionary<string, List<Dictionary<string, string>>> result = new Dictionary<string, List<Dictionary<string, string>>>();
            if (!Tables.ContainsKey(table))
                ReadTable(table);

            for (int i = 0; i < Tables[table].Count; i++) {
                if (!result.ContainsKey(Tables[table][i][columnToFilter]))
                    result[Tables[table][i][columnToFilter]] = new List<Dictionary<string, string>>();
                result[Tables[table][i][columnToFilter]].Add(Tables[table][i]);
            }

            return result;
        }

        /// <summary>
        /// 读取一张表格，存入Dic中
        /// </summary>
        /// <param name="table">表格名, 在/table/中</param>
        /// <returns></returns>
        public static List<Dictionary<string, string>> ReadTable(string table, bool loadNew = false) {
            if (!loadNew && Tables.ContainsKey(table))
                return Tables[table];

            var result = new List<Dictionary<string, string>>();

            try {
                FileStream fs = new FileStream($@"{DATAPATH}\table\{table}.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader sr = new StreamReader(fs);

                string[] header = sr.ReadLine().Split('\t');
                sr.ReadLine();

                while (!sr.EndOfStream) {
                    string[] row = sr.ReadLine().Split('\t');
                    Dictionary<string, string> rowDic = new Dictionary<string, string>();

                    for (int i = 0; i < header.Length; i++)
                        rowDic[header[i]] = row[i];

                    result.Add(rowDic);
                }
                sr.Close();
            } catch (Exception e) {
                Debug.Log(e.Message);
            }

            Tables[table] = result;
            return result;
        }

        /// <summary>
        /// 读取一张表格，存入Dic中
        /// </summary>
        /// <param name="table">表格名, 在/table/中</param>
        /// <returns></returns>
        public static List<Dictionary<string, string>> ReadTable(string fullpath, string dummyFullPath) {
            var result = new List<Dictionary<string, string>>();

            try {
                FileStream fs = new FileStream(fullpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader sr = new StreamReader(fs);

                string[] header = sr.ReadLine().Split('\t');
                sr.ReadLine();

                while (!sr.EndOfStream) {
                    string[] row = sr.ReadLine().Split('\t');
                    Dictionary<string, string> rowDic = new Dictionary<string, string>();

                    for (int i = 0; i < header.Length; i++)
                        rowDic[header[i]] = row[i];

                    result.Add(rowDic);
                }
                sr.Close();
            } catch (Exception e) {

                //弹一个窗警告sharing violation
                Debug.Log("表格读取失败！请确保表格没有被其他程序占用");
                Debug.Log(e.Message);
            }

            return result;
        }

        /// <summary>
        /// 读取一个表格，并且用reflection读入List<T>中
        /// 默认第一行为抬头，第二行为注释
        /// </summary>
        /// <typeparam name="T">目标的类</typeparam>
        /// <param name="path">表格路径</param>
        /// <param name="target">目标类实例</param>
        /// <param name="delimiter">使用的分隔符，默认为制表分隔符</param>
        public static void LoadTable<T>(string path, ref List<T> target, char delimiter = '\t') where T : new() {
            try {
                target.Clear();
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader sr = new StreamReader(fs, Encoding.Unicode);
                string line = sr.ReadLine();
                string[] argList = line.Split(delimiter);
                sr.ReadLine(); //跳过注释

                while (!sr.EndOfStream) {
                    line = sr.ReadLine();
                    string[] args = line.Split(delimiter);
                    T t = new T();

                    for (int i = 0; i < args.Length; i++)
                        t.SetPublicField(argList[i], args[i]);

                    target.Add(t);
                }
                sr.Close();
            } catch (Exception e) {
                Debug.Log(e.Message);
            }
        }
    }
}
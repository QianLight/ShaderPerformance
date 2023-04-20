using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace TDTools {

    /// <summary>
    /// �������飬������ȡ���
    /// </summary>
    public class TableReader{

        public static readonly string DATAPATH = Application.dataPath;

        /// <summary>
        /// Tables[�����][��][��] = ����
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
        /// ��ĳһ���е����ݽ���ɸѡ������ֵ�
        /// ����һ���ֵ䣬��Ŀ���е�����ΪKey
        /// </summary>
        /// <param name="table">Ŀ��ı��</param>
        /// <param name="columnToFilter"></param>
        /// <returns>Result[ɸѡ��������][��][��] = ����</returns>
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
        /// ��ȡһ�ű�񣬴���Dic��
        /// </summary>
        /// <param name="table">�����, ��/table/��</param>
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
        /// ��ȡһ�ű�񣬴���Dic��
        /// </summary>
        /// <param name="table">�����, ��/table/��</param>
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

                //��һ��������sharing violation
                Debug.Log("����ȡʧ�ܣ���ȷ�����û�б���������ռ��");
                Debug.Log(e.Message);
            }

            return result;
        }

        /// <summary>
        /// ��ȡһ����񣬲�����reflection����List<T>��
        /// Ĭ�ϵ�һ��Ϊ̧ͷ���ڶ���Ϊע��
        /// </summary>
        /// <typeparam name="T">Ŀ�����</typeparam>
        /// <param name="path">���·��</param>
        /// <param name="target">Ŀ����ʵ��</param>
        /// <param name="delimiter">ʹ�õķָ�����Ĭ��Ϊ�Ʊ�ָ���</param>
        public static void LoadTable<T>(string path, ref List<T> target, char delimiter = '\t') where T : new() {
            try {
                target.Clear();
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader sr = new StreamReader(fs, Encoding.Unicode);
                string line = sr.ReadLine();
                string[] argList = line.Split(delimiter);
                sr.ReadLine(); //����ע��

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
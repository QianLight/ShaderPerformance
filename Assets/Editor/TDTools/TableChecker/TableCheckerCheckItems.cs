using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace TDTools.TableChecker {

    /// <summary>
    /// 具体的检查项目，对应TSV表中的某一行
    /// </summary>
    public class TableCheckerCheckItems
    {
        /// <summary>
        /// 是否启用这个测试
        /// </summary>
        public bool enabled = false;

        /// <summary>
        /// 被测试的表
        /// </summary>
        public string SourceTable;

        /// <summary>
        /// 被测试的列
        /// </summary>
        public string SourceColumn;

        /// <summary>
        /// 测试所使用的method
        /// </summary>
        public string CheckType;

        /// <summary>
        /// 关联的表
        /// </summary>
        public string LinkedTable;

        /// <summary>
        /// 关联的列
        /// </summary>
        public string LinkedColumn;

        /// <summary>
        /// 重要度
        /// </summary>
        public string Importance;

        /// <summary>
        /// 负责组别
        /// </summary>
        public string Responsibility;

        public string MinRange;

        public string MaxRange;

        public string Path;

        public string FileName;

        public List<Dictionary<string, string>> Data;
        public int ProgressId;

        public int parentID = -1;

        public string CaseSensitivity;

        public string SearchAll;

        /// <summary>
        /// 运行这项检查
        /// </summary>
        public List<TableCheckerResult> Run() {
            if (!enabled)
                return null;
            ProgressId = Progress.Start("检查表格", $"{SourceTable} {CheckType}", Progress.Options.None, parentID);
            //return new TableCheckerResult("ExampleTable" + UnityEngine.Random.Range(1, 10), UnityEngine.Random.Range(2, 1024).ToString(), "Example Column", UnityEngine.Random.Range(1, 10), "Example Des", "负责组" + UnityEngine.Random.Range(1, 3), "Assets/Table/XEntityStatistics.txt");

            object[] args = new object[1];
            args[0] = this;
            List<TableCheckerResult> results = new List<TableCheckerResult>();
            Type type = typeof(TableChecker);
            try {
                var rrr = type.GetMethod(CheckType).Invoke(null, args);
                List<TableCheckerResult> rr;
                if (rrr is List<TableCheckerResult>)
                    rr = (List<TableCheckerResult>)rrr;
                else
                    rr = ((Task<List<TableCheckerResult>>)rrr).Result;

                for (int i = 0; i < rr.Count; i++) {
                    //if (rr[i].Importantce.CompareTo("0") == 0)
                    results.Add(rr[i]);
                }
            } catch (Exception e){
                Progress.Remove(ProgressId);
                throw e;
            }
            Progress.Remove(ProgressId);
            return results;
        }

        public override string ToString() {
            return SourceTable + " " + SourceColumn + " " + CheckType + " " + LinkedTable + " " + LinkedColumn + " " + Importance + " " + Responsibility + " " + MinRange + " " + MaxRange;
        }
     }

    /// <summary>
    /// 读取一个包含了所有检查项目的TSV表
    /// </summary>
    public class TableCheckerCheckList {

        public List<TableCheckerCheckItems> List;

        const string PATH = "Assets/Editor/TDTools/TableChecker/TableCheckerWorkingTable.txt";

        public void Load() {
            List = new List<TableCheckerCheckItems>();
            TableReader.LoadTable<TableCheckerCheckItems>(PATH, ref List);
        }
    }
}

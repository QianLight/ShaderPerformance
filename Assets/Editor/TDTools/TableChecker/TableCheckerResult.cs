using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TDTools.TableChecker
{

    public class TableCheckerResultSortByImportance : IComparer<TableCheckerResult> {
        public int Compare(TableCheckerResult x, TableCheckerResult y) {
            if (x.Importantce.CompareTo(y.Importantce) != 0)
                return -x.Importantce.CompareTo(y.Importantce);
            if (x.Table.CompareTo(y.Table) != 0)
                return x.Table.CompareTo(y.Table);
            else return x.Row.CompareTo(y.Row);
        }
    }

    /// <summary>
    /// 表格检测器的结果项
    /// </summary>
    public class TableCheckerResult: IComparable<TableCheckerResult>
    {

        /// <summary>
        /// 问题所在表格名
        /// </summary>
        public string Table;

        /// <summary>
        /// 问题所在行
        /// </summary>
        public string Column;

        /// <summary>
        /// 问题所在列
        /// </summary>
        public string Row;

        /// <summary>
        /// 问题的重要程度
        /// </summary>
        public string Importantce;

        /// <summary>
        /// 对于问题的描述
        /// </summary>
        public string Description;

        /// <summary>
        /// 问题的所属组别
        /// </summary>
        public string Responsibility;

        /// <summary>
        /// 问题对应的表格路径，双击可以在unity中显示
        /// </summary>
        public string Path;

        public Dictionary<string, string> RowContent;

        public TableCheckerResult(string table, string row, string column, string importance, string description, string responsibility, string path, Dictionary<string, string> rowContent)
        {
            Table = table;
            Column = column;
            Row = row;
            Importantce = importance;
            Description = description;
            Responsibility = responsibility;
            Path = path;
            RowContent = rowContent;
        }

        /// <summary>
        /// 在读取TSV文件时使用的构造函数
        /// </summary>
        /// <param name="args">分割好的参数</param>
        public TableCheckerResult(string[] args) {
            Importantce = args[0] ;
            Table = args[1];
            Row = args[2];
            Column = args[3];
            Responsibility = args[4];
            Description = args[5];
        }

        /// <summary>
        /// 优先以表名排序，然后以行排序
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(TableCheckerResult other) {
            if (Table.CompareTo(other.Table) != 0)
                return Table.CompareTo(other.Table);
            else
                try
                {
                    return int.Parse(Row).CompareTo(int.Parse(other.Row));
                }
                catch {
                    return Row.CompareTo(other.Row);
                }
        }
    }
}
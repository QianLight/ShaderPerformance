using CFEngine;
using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace TDTools.TableChecker {

    /// <summary>
    /// 这里包括了表格检查工具的大部分检查项
    /// 到时候用反射进行调用
    /// </summary>
    class TableChecker {

        #region Variables

        public static Dictionary<string, List<Dictionary<string, string>>> Tables {
            get {
                if (_tables == null)
                    _tables = new Dictionary<string, List<Dictionary<string, string>>>();
                return _tables;
            }

        }
        private static Dictionary<string, List<Dictionary<string, string>>> _tables;

        public static string DATAPATH;
        #endregion

        ///一些主要的重复使用的检查项，比方说查空和查重
        #region Universal

        /// <summary>
        /// 查空
        /// 确保某一列中不能又空值，比如ID
        /// </summary>
        public static List<TableCheckerResult> EmptyCheck(TableCheckerCheckItems data) {
            List<TableCheckerResult> results = new List<TableCheckerResult>();

            List<Dictionary<string, string>> table;
            if (!Tables.ContainsKey(data.SourceTable)) {
                table = ReadTable(data.SourceTable);
            } else
                table = Tables[data.SourceTable];

            for (int i = 0; i < table.Count; i++) {
                if (table[i][data.SourceColumn] == "") {
                    TableCheckerResult result = new TableCheckerResult(
                        data.SourceTable,
                        (i + 3).ToString(),
                        data.SourceColumn,
                        data.Importance, $"表{data.SourceTable}的{data.SourceColumn}列的第{i + 3}行为空",
                        data.Responsibility,
                        "Assets/table/" + data.SourceTable + ".txt",
                        table[i]);
                    results.Add(result);
                }
                Progress.Report(data.ProgressId, i / (float)table.Count);
            }
            if (results.Count == 0)
                results.Add(new TableCheckerResult(
                    data.SourceTable,
                    "NA",
                    data.SourceColumn,
                    "0",
                    $"表{data.SourceTable}的{data.SourceColumn}列中不存在空值",
                    data.Responsibility,
                    "Assets/table/" + data.SourceTable + ".txt",
                    null));
            return results;
        }

        /// <summary>
        /// 新版的查重，允许多列组合查重,在列中用 | 分开
        /// </summary>
        /// <returns></returns>
        public static List<TableCheckerResult> RepetitionCheck(TableCheckerCheckItems data) {
            List<TableCheckerResult> results = new List<TableCheckerResult>();

            List<Dictionary<string, string>> table;
            if (!Tables.ContainsKey(data.SourceTable)) {
                table = ReadTable(data.SourceTable);
            } else
                table = Tables[data.SourceTable];

            Dictionary<string, int> names = new Dictionary<string, int>();

            string[] columnsToCheck = data.SourceColumn.Split('|');

            for (int i = 0; i < table.Count; i++) {
                string combinedName = "";
                Progress.Report(data.ProgressId, i / (float)table.Count);
                for (int j = 0; j < columnsToCheck.Length; j++)
                    combinedName += table[i][columnsToCheck[j]] + "\t";
                if (names.ContainsKey(combinedName))
                    results.Add(new TableCheckerResult(
                        data.SourceTable,
                        (i + 3).ToString(),
                        data.SourceColumn,
                        data.Importance,
                        $"第{i + 3}行的技能名与第{names[combinedName]}行的技能名相同",
                        data.Responsibility,
                        $"Assets/Table/{data.SourceTable}.txt",
                        table[i]));
                else
                    names[combinedName] = i;
            }

            if (results.Count == 0)
                results.Add(new TableCheckerResult(
                    data.SourceTable,
                    "NA",
                    data.SourceColumn,
                    "0",
                    $"表{data.SourceTable}的{data.SourceColumn}列中不存在重复内容",
                    data.Responsibility,
                    $"Assets/Table/{data.SourceTable}.txt",
                    null));
            return results;
        }


        /// <summary>
        /// 新版的查外键，同样，支持多列组合查
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<TableCheckerResult> ForeignkeyCheck(TableCheckerCheckItems data) {
            List<TableCheckerResult> results = new List<TableCheckerResult>();

            List<Dictionary<string, string>> table;
            if (!Tables.ContainsKey(data.SourceTable)) {
                table = ReadTable(data.SourceTable);
                Tables[data.SourceTable] = table;
            } else
                table = Tables[data.SourceTable];

            List<Dictionary<string, string>> linkedTable;

            if (data.LinkedTable.Contains("|")) {
                linkedTable = new List<Dictionary<string, string>>();
                string[] tables = data.LinkedTable.Split('|');
                for (int i = 0; i < tables.Length; i++) {
                    var t = ReadTable(tables[i]);
                    linkedTable.AddRange(t);
                }
            } else {
                linkedTable = ReadTable(data.LinkedTable);
            }

            HashSet<string> linkedTableSet = new HashSet<string>();
            for (int i = 0; i < linkedTable.Count; i++) {
                linkedTableSet.Add(linkedTable[i][data.LinkedColumn]);
            }

            string[] columnsToCheck = data.SourceColumn.Split('|');

            for (int i = 0; i < table.Count; i++) {
                Progress.Report(data.ProgressId, i / (float)table.Count);
                string combinedName = "";
                for (int j = 0; j < columnsToCheck.Length; j++)
                    combinedName += table[i][columnsToCheck[j]];

                string[] names = combinedName.Split('|');
                for (int j = 0; j < names.Length; j++) {
                    if (names[j] != "" && !linkedTableSet.Contains(names[j])) {
                        results.Add(new TableCheckerResult(
                            data.SourceTable,
                            (i + 3).ToString(),
                            data.SourceColumn,
                            data.Importance,
                            $"表{data.SourceTable}的{data.SourceColumn}列的第{i + 3}行内容{names[j]}无法在表{data.LinkedTable}的{data.LinkedColumn}列中找到",
                            data.Responsibility,
                            "Assets/table/" + data.SourceTable + ".txt",
                            table[i]));
                    }
                }
            }

            if (results.Count == 0)
                results.Add(new TableCheckerResult(
                    data.SourceTable,
                    "NA",
                    data.SourceColumn,
                    "0",
                    $"表{data.SourceTable}的{data.SourceColumn}列的所有内容都能在表{data.LinkedTable}的{data.LinkedColumn}列中找到",
                    data.Responsibility,
                    "Assets/table/" + data.SourceTable + ".txt",
                    null));
            return results;
        }

        /// <summary>
        /// 检查互斥
        /// </summary>
        /// <returns></returns>
        public static List<TableCheckerResult> ConflictCheck(TableCheckerCheckItems data) {
            List<TableCheckerResult> results = new List<TableCheckerResult>();


            List<Dictionary<string, string>> table;
            if (!Tables.ContainsKey(data.SourceTable)) {
                table = ReadTable(data.SourceTable);
            } else
                table = Tables[data.SourceTable];

            for (int i = 0; i < table.Count; i++) {
                Progress.Report(data.ProgressId, i / (float)table.Count);
                if (table[i][data.SourceColumn] != "" && table[i][data.LinkedColumn] != "") {
                    results.Add(new TableCheckerResult(
                        data.SourceTable,
                        (i + 3).ToString(),
                        data.SourceColumn,
                        data.Importance,
                        $"表{data.SourceTable}的{data.SourceColumn}列的第{i + 3}行的互斥内容在表{data.LinkedTable}的{data.LinkedColumn}列中找到",
                        data.Responsibility,
                        "Assets/table/" + data.SourceTable + ".txt",
                        table[i]));
                }
            }
            if (results.Count == 0) {
                results.Add(new TableCheckerResult(
                    data.SourceTable,
                    "NA",
                    data.SourceColumn,
                    "0",
                    $"表{data.SourceTable}的{data.SourceColumn}列的所有内容和表{data.LinkedTable}的{data.LinkedColumn}列没有相斥",
                    data.Responsibility,
                    "Assets/table/" + data.SourceTable + ".txt",
                    null));
            }

            return results;
        }

        /// <summary>
        /// 数值范围检测
        /// </summary>
        /// <returns></returns>
        public static List<TableCheckerResult> RangeCheckInt(TableCheckerCheckItems data) {
            List<TableCheckerResult> results = new List<TableCheckerResult>();

            List<Dictionary<string, string>> table;
            if (!Tables.ContainsKey(data.SourceTable)) {
                table = ReadTable(data.SourceTable);
            } else
                table = Tables[data.SourceTable];

            for (int i = 0; i < table.Count; i++) {
                Progress.Report(data.ProgressId, i / (float)table.Count);
                if (table[i][data.SourceColumn].CompareTo("") != 0)
                    if (int.Parse(table[i][data.SourceColumn]) < int.Parse(data.MinRange) || int.Parse(table[i][data.SourceColumn]) > int.Parse(data.MaxRange)) {
                        TableCheckerResult result = new TableCheckerResult(
                            data.SourceTable,
                            (i + 3).ToString(),
                            data.SourceColumn,
                            data.Importance,
                            $"表{data.SourceTable}的{data.SourceColumn}列的第{i + 3}行不在允许的范围内",
                            data.Responsibility,
                            "Assets/Table/" + data.SourceTable + ".txt",
                            table[i]);
                        results.Add(result);
                    }
            }
            if (results.Count == 0)
                results.Add(new TableCheckerResult(
                    data.SourceTable,
                    "NA",
                    data.SourceColumn,
                    "0",
                    $"表{data.SourceTable}的{data.SourceColumn}列中所有值均在允许范围内",
                    data.Responsibility,
                    "Assets/Table/" + data.SourceTable + ".txt",
                    null));

            return results;
        }

        /// <summary>
        /// 数值范围检测
        /// </summary>
        /// <returns></returns>
        public static List<TableCheckerResult> RangeCheckFloat(TableCheckerCheckItems data) {
            List<TableCheckerResult> results = new List<TableCheckerResult>();

            List<Dictionary<string, string>> table;
            if (!Tables.ContainsKey(data.SourceTable)) {
                table = ReadTable(data.SourceTable);
            } else
                table = Tables[data.SourceTable];

            for (int i = 0; i < table.Count; i++) {
                Progress.Report(data.ProgressId, i / (float)table.Count);
                if (table[i][data.SourceColumn].CompareTo("") != 0)
                    if (float.Parse(table[i][data.SourceColumn]) < float.Parse(data.MinRange) || float.Parse(table[i][data.SourceColumn]) > float.Parse(data.MaxRange)) {
                        TableCheckerResult result = new TableCheckerResult(
                            data.SourceTable,
                            (i + 3).ToString(),
                            data.SourceColumn,
                            data.Importance,
                            $"表{data.SourceTable}的{data.SourceColumn}列的第{i + 3}行不在允许的范围内",
                            data.Responsibility,
                            "Assets/Table/" + data.SourceTable + ".txt",
                            table[i]);
                        results.Add(result);
                    }
            }
            if (results.Count == 0)
                results.Add(new TableCheckerResult(
                    data.SourceTable,
                    "NA", data.SourceColumn,
                    "0",
                    $"表{data.SourceTable}的{data.SourceColumn}列中所有值均在允许范围内",
                    data.Responsibility,
                    "Assets/Table/" + data.SourceTable + ".txt",
                    null));

            return results;
        }

        static List<FileInfo> GetAllFiles(DirectoryInfo rootDir, string searchPattern) {
            List<FileInfo> result = new List<FileInfo>();
            result.AddRange(rootDir.GetFiles(searchPattern));
            var dirs = rootDir.GetDirectories();
            for (int i = 0; i < dirs.Length; i++) {
                result.AddRange(GetAllFiles(dirs[i], searchPattern));
            }

            return result;
        }

        /// <summary>
        /// 检测指定文件是否存在
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<TableCheckerResult> FileExistCheck(TableCheckerCheckItems data) {
            List<TableCheckerResult> results = new List<TableCheckerResult>();
            string lastPath = "";
            List<FileInfo> files = new List<FileInfo>();

            List<Dictionary<string, string>> table;
            if (!Tables.ContainsKey(data.SourceTable)) {
                table = ReadTable(data.SourceTable);
            } else
                table = Tables[data.SourceTable];

            for (int i = 0; i < table.Count; i++) {
                Progress.Report(data.ProgressId, i / (float)table.Count);
                string path = data.Path;
                string fileName = data.FileName;

                bool hasNull = false;
                while (path.IndexOf("\\{") != -1) {
                    int start = path.IndexOf("\\{");
                    int end = path.IndexOf("\\}");

                    string s = path.Substring(start + 2, end - start - 3);
                    path = path.Remove(start, end - start + 2);
                    path = path.Insert(start, table[i][s]);
                    if (table[i][s] == "") {
                        hasNull = true;
                        break;
                    }
                }
                if (hasNull)
                    continue;

                hasNull = false;
                while (fileName.IndexOf("\\{") != -1) {
                    int start = fileName.IndexOf("\\{");
                    int end = fileName.IndexOf("\\}");

                    string s = fileName.Substring(start + 2, end - start - 3);
                    fileName = fileName.Remove(start - 1, end - start + 3);
                    if (table[i][s] == "") {
                        hasNull = true;
                        break;
                    }
                    fileName = fileName.Insert(start - 1, table[i][s]);
                }
                if (hasNull)
                    continue;

                if (!data.CaseSensitivity.Equals("1"))
                    fileName = fileName.ToLower();

                bool exist = false;
                if (path.IndexOf('/') != 0 && path.IndexOf('\\') != 0)
                    path = "/" + path;
                path = DATAPATH + path;
                if (File.Exists(path + fileName)) {
                    exist = true;
                } else if (data.SearchAll.Equals("1")) {
                    if (!lastPath.Equals(path)) {
                        DirectoryInfo d = new DirectoryInfo(path);
                        files = GetAllFiles(d, $"*{fileName.Substring(fileName.LastIndexOf('.'))}");
                        lastPath = path;
                    }
                    for (int j = 0; j < files.Count; j++) {
                        string otherName = files[j].Name;
                        if (!data.CaseSensitivity.Equals("1"))
                            otherName = otherName.ToLower();

                        if (files[j].Name.Equals(otherName)) {
                            exist = true;
                            break;
                        }
                    }
                } else {
                    try {
                        DirectoryInfo d = new DirectoryInfo(path);
                        FileInfo[] fi = d.GetFiles();
                        for (int j = 0; j < fi.Length; j++) {
                            if (Regex.IsMatch(fi[j].Name, fileName)) {
                                exist = true;
                                break;
                            }
                        }
                    } catch { 
                    }
                }

                if (!exist) {
                    results.Add(new TableCheckerResult(
                        data.SourceTable,
                        $"{i + 3}",
                        data.SourceColumn,
                        data.Importance,
                        $"表{data.SourceTable}的{data.SourceColumn}列中的对应文件{path}{fileName}不存在",
                        data.Responsibility,
                        "Assets/Table/" + data.SourceTable + ".txt",
                        table[i]));
                }
            }

            if (results.Count == 0)
                results.Add(new TableCheckerResult(
                    data.SourceTable,
                    "NA",
                    data.SourceColumn,
                    "0",
                    $"表{data.SourceTable}的{data.SourceColumn}列中的对应文件均存在",
                    data.Responsibility,
                    "Assets/Table/" + data.SourceTable + ".txt",
                    null));

            return results;
        }

        #endregion

        public static T DeserializeEcsData<T>(string path) {
            string json = "";
            byte[] bytes;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                bytes = new byte[fs.Length + 1];
                fs.Read(bytes, 0, bytes.Length);
                //bytes[fs.Length] = 0;
            }
            SimpleTools.Unlock(ref bytes, 0, bytes.Length - 1);
            json = System.Text.Encoding.UTF8.GetString(bytes);
            return JsonUtility.FromJson<T>(json);
        }

        #region SkillListForRole

        /// <summary>
        /// CD,霸体不可为空，除非是被动
        /// </summary>
        /// <param name="data"></param>
        public static List<TableCheckerResult> SkillListForRole_EmptyCheckUnlessPassive(TableCheckerCheckItems data) {
            List<TableCheckerResult> results = new List<TableCheckerResult>();

            List<Dictionary<string, string>> table;
            if (!Tables.ContainsKey(data.SourceTable)) {
                table = ReadTable(data.SourceTable);
            } else
                table = Tables[data.SourceTable];

            for (int i = 0; i < table.Count; i++) {
                try {
                    Progress.Report(data.ProgressId, i / (float)table.Count);
                    if (table[i]["SkillType"].CompareTo("0") != 0 && table[i][data.SourceColumn] == "") {
                        results.Add(new TableCheckerResult(
                            data.SourceTable,
                            $"{i + 3}",
                            data.SourceColumn,
                            data.Importance,
                            $"表{data.SourceTable}的{i + 3}行中，非被动技能没有填写{data.SourceColumn}",
                            data.Responsibility,
                            "Assets/Table/" + data.SourceTable + ".txt",
                            table[i]));
                    }
                } catch (Exception e) {
                    Debug.Log($"{e.Message}\n{e.StackTrace}");
                }
            }

            if (results.Count == 0)
                results.Add(new TableCheckerResult(
                    data.SourceTable,
                    "NA",
                    data.SourceColumn,
                    "0",
                    $"表{data.SourceTable}的非被动技能均有填写{data.SourceColumn}列",
                    data.Responsibility,
                    "Assets/Table/" + data.SourceTable + ".txt",
                    null));
            return results;
        }

        /// <summary>
        /// 一大堆列数量 (用|分隔)必须一直
        /// </summary>
        public static List<TableCheckerResult> SkillListForRole_DamageCountCheck(TableCheckerCheckItems data) {
            List<TableCheckerResult> results = new List<TableCheckerResult>();

            List<Dictionary<string, string>> table;
            if (!Tables.ContainsKey(data.SourceTable)) {
                table = ReadTable(data.SourceTable);
            } else
                table = Tables[data.SourceTable];

            string[] mustMatch = { "MagicFixed",
                "PvPMagicRatio",
                "PvPMagicFixed",
                "ModeBKRatio",
                "ModeBKValue",
                "ModeBKPresent",
                "AttackLevel"
            };

            string[] nullOrMatch = { 
                "ModeBKTransfer",
                "DecreaseSuperArmor",
                "PvPDecreaseSuperArmor",
                "MagicDecreaseSuperArmor",
                "RecoverEnergyPVE",
                "RecoverEnergyPVP",
                "RecoverEnergyPVPForOpponent",
                "PvPMagicDecreaseSuperArmor"
            };

            for (int i = 0; i < table.Count; i++) {
                Progress.Report(data.ProgressId, i / (float)table.Count);
                int count = table[i]["MagicFixed"].Split('|').Length;

                for (int j = 0; j < mustMatch.Length; j++)
                    if (table[i][mustMatch[j]].Split('|').Length != count)
                        results.Add(new TableCheckerResult(
                            data.SourceTable,
                            $"{i + 3}",
                            mustMatch[j],
                            data.Importance,
                            $"表{data.SourceTable}的{i + 3}行{mustMatch[j]}列中的数量不符",
                            data.Responsibility,
                            "Assets/Table/" + data.SourceTable + ".txt",
                            table[i]));

                for (int j = 0; j < nullOrMatch.Length; j++)
                    if (table[i][nullOrMatch[j]] != "" && table[i][nullOrMatch[j]].Split('|').Length != count)
                        results.Add(new TableCheckerResult(
                            data.SourceTable,
                            $"{i + 3}",
                            nullOrMatch[j],
                            data.Importance,
                            $"表{data.SourceTable}的{i + 3}行{nullOrMatch[j]}列中的数量不符",
                            data.Responsibility,
                            "Assets/Table/" + data.SourceTable + ".txt",
                            table[i]));
            }

            if (results.Count == 0)
                results.Add(new TableCheckerResult(
                    data.SourceTable,
                    "NA",
                    data.SourceColumn,
                    "0",
                    $"表{data.SourceTable}的技能数量均相符",
                    data.Responsibility,
                    "Assets/Table/" + data.SourceTable + ".txt",
                    null));
            return results;
        }

        /// <summary>
        /// 检查buffID和等级，以及-1的情况
        /// SkillListForRole 对应 BuffID 列，=分开的第2、3个数字
        /// BuffID表对应 BuffID列 和BuffLevel列
        /// </summary>
        /// <returns></returns>
        public static List<TableCheckerResult> SkillListForRole_BuffIDCheck(TableCheckerCheckItems data) {
            List<TableCheckerResult> results = new List<TableCheckerResult>();

            List<Dictionary<string, string>> table;
            if (!Tables.ContainsKey(data.SourceTable)) {
                table = ReadTable(data.SourceTable);
            } else
                table = Tables[data.SourceTable];

            List<Dictionary<string, string>> linkedTable;
            if (!Tables.ContainsKey(data.LinkedTable)) {
                linkedTable = ReadTable(data.LinkedTable);
            } else
                linkedTable = Tables[data.LinkedTable];

            List<Dictionary<string, string>> buffIDListTable;
            if (!Tables.ContainsKey("BuffIDList")) {
                buffIDListTable = ReadTable("BuffIDList");
            } else
                buffIDListTable = Tables["BuffIDList"];

            HashSet<string> set = new HashSet<string>();
            for (int i = 0; i < linkedTable.Count; i++)
                set.Add(linkedTable[i]["BuffID"] + "\t" + linkedTable[i]["BuffLevel"]);

            HashSet<string> set2 = new HashSet<string>();
            for (int i = 0; i < buffIDListTable.Count; i++)
                set2.Add(buffIDListTable[i]["ID"]);

            for (int i = 0; i < table.Count; i++) {
                Progress.Report(data.ProgressId, i / (float)table.Count);
                if (table[i][data.SourceColumn] == "")
                    continue;

                string[] buffs = table[i][data.SourceColumn].Split('|');
                for (int j = 0; j < buffs.Length; j++) {
                    try {
                        if (buffs[j] == "")
                            continue;
                        string[] line = buffs[j].Split('=');
                        if (line[1].CompareTo("-1") == 0 || (line.Length > 2 && line[2].CompareTo("-1") == 0)) {
                            if ((line[1].CompareTo("-1") == 0 && !set2.Contains(line[0])) || (line.Length > 2 && line[2].CompareTo("-1") == 0 && !set2.Contains(line[1])))
                                results.Add(new TableCheckerResult(
                                    data.SourceTable,
                                    $"{i + 3}",
                                    data.SourceColumn,
                                    data.Importance,
                                    $"表{data.SourceTable}的{i + 3}行为-1但是，BuffID:{buffs[j]}不能够在BuffIDList表中找到",
                                    data.Responsibility,
                                    "Assets/Table/" + data.SourceTable + ".txt",
                                    table[i]));
                            continue;
                        }
                        bool found = false;
                        if (line.Length > 2)
                            found = set.Contains(line[1] + '\t' + line[2]) || set.Contains(line[0] + '\t' + line[1]);
                        else
                            found = set.Contains(line[0] + '\t' + line[1]);
                        if (!found) {
                            results.Add(new TableCheckerResult(
                                data.SourceTable,
                                $"{i + 3}",
                                data.SourceColumn,
                                data.Importance,
                                $"表{data.SourceTable}的{i + 3}行的BuffID + 等级 不能够在Buff表中找到",
                                data.Responsibility,
                                "Assets/Table/" + data.SourceTable + ".txt",
                                table[i]));
                        }
                    } catch (Exception e) {
                        Debug.Log($"{e.Message} {table[i][data.SourceColumn]}\n{e.StackTrace}");
                    }
                }
            }

            if (results.Count == 0)
                results.Add(new TableCheckerResult(
                    data.SourceTable,
                    "NA",
                    data.SourceColumn,
                    "0",
                    $"表{data.SourceTable}的所有buffID均能在buff表中找到",
                    data.Responsibility,
                    "Assets/Table/" + data.SourceTable + ".txt",
                    null));

            return results;
        }


        /// <summary>
        /// partnerID和 partnerInfo表对应
        /// 同一行的presnet ID和 Xpresentation 表对应
        /// 然后去Xpresentation找技能目录+技能名
        /// 对应最大值不能超过上海数量
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<TableCheckerResult> SkillListForRole_PartnerID(TableCheckerCheckItems data) {
            List<TableCheckerResult> results = new List<TableCheckerResult>();

            List<Dictionary<string, string>> table;
            if (!Tables.ContainsKey(data.SourceTable)) {
                table = ReadTable(data.SourceTable);
            } else
                table = Tables[data.SourceTable];

            List<Dictionary<string, string>> partnerInfoTable;
            if (!Tables.ContainsKey("PartnerInfo")) {
                partnerInfoTable = ReadTable("PartnerInfo");
            } else
                partnerInfoTable = Tables["PartnerInfo"];

            Dictionary<string, string> partnerInfoDic = new Dictionary<string, string>();
            for (int i = 0; i < partnerInfoTable.Count; i++) {
                partnerInfoDic[partnerInfoTable[i]["ID"]] = partnerInfoTable[i]["PresentId"];
            }

            List<Dictionary<string, string>> xPresentationTable;
            if (!Tables.ContainsKey("XEntityPresentation")) {
                xPresentationTable = ReadTable("XEntityPresentation");
            } else
                xPresentationTable = Tables["XEntityPresentation"];
            Dictionary<string, string> xPresentationDic = new Dictionary<string, string>();
            for (int i = 0; i < xPresentationTable.Count; i++) {
                xPresentationDic[xPresentationTable[i]["PresentID"]] = xPresentationTable[i]["SkillLocation"];
            }


            for (int i = 0; i < table.Count; i++) {
                if (table[i]["SkillPartnerID"] == "" || table[i]["SkillType"].CompareTo("0") == 0 || table[i]["SkillType"].CompareTo("6") == 0)
                    continue;
                Progress.Report(data.ProgressId, i / (float)table.Count);

                if (!partnerInfoDic.ContainsKey(table[i]["SkillPartnerID"])) {
                    //找不到 partnerID
                    results.Add(new TableCheckerResult(
                        data.SourceTable,
                        $"{ i + 3}",
                        data.SourceColumn,
                        data.Importance,
                        $"表{data.SourceTable}的第{i + 3}行PartnerID {table[i]["SkillPartnerID"]} 无法在PartnerInfo表中找到",
                        data.Responsibility,
                        "Assets/Table/" + data.SourceTable + ".txt", 
                        table[i]));
                    continue;
                }

                string presentID = partnerInfoDic[table[i]["SkillPartnerID"]];
                if (!xPresentationDic.ContainsKey(presentID)) {
                    //找不到 Present ID
                    results.Add(new TableCheckerResult(
                        data.SourceTable,
                        $"{i + 3}",
                        data.SourceColumn,
                        data.Importance,
                        $"表{data.SourceTable}的第{i + 3}行PartnerID在PartnerInfo表中对应的PresentID {presentID} 无法在Xpresentation表中找到",
                        data.Responsibility,
                        "Assets/Table/" + data.SourceTable + ".txt",
                        table[i]));
                    continue;
                }

                string skillPath;
                //XSkillData config;
                int countResult = 0;
                int countBullet = 0;
                try {
                    skillPath = $"{DATAPATH}/BundleRes/SkillPackage/{xPresentationDic[presentID]}{table[i]["SkillScript"]}.bytes";

                    if (!File.Exists(skillPath)) {
                        results.Add(new TableCheckerResult(
                            data.SourceTable,
                            $"{i + 3}",
                            data.SourceColumn,
                            data.Importance,
                            $"表{data.SourceTable}的第{i + 3}行找不到技能文件{table[i]["SkillScript"]}",
                            data.Responsibility,
                            "Assets/Table/" + data.SourceTable + ".txt",
                            table[i]));
                        continue;
                    }
                    //graph = SkillGraphDataManager.GetSkillGraph(skillPath);
                    XSkillData configData = DeserializeEcsData<XSkillData>(skillPath);
                    if (configData.ResultData.Count > 0)
                        countResult = configData.ResultData[0].TableIndex;

                    if (configData.BulletData.Count > 0)
                        countBullet = configData.BulletData[0].TableIndex;
                } catch (Exception e){
                    results.Add(new TableCheckerResult(
                        data.SourceTable,
                        $"{i + 3}",
                        data.SourceColumn,
                        data.Importance,
                        $"表{data.SourceTable}的第{i + 3}行{table[i]["SkillType"]}配置错误{e.Message}",
                        data.Responsibility,
                        "Assets/Table/" + data.SourceTable + ".txt",
                        table[i]));
                    continue;
                }

                int maxLength = table[i]["MagicFixed"].Split('|').Length;
                //Todo: 检查伤害数量是否一致

                if (Math.Max(countResult, countBullet) + 1 > maxLength) {
                    results.Add(new TableCheckerResult(
                        data.SourceTable,
                        $"{i + 3}",
                        data.SourceColumn,
                        data.Importance,
                        $"表{data.SourceTable}的第{i + 3}行技能文件{xPresentationDic[presentID]}{table[i]["SkillScript"]}打击点不匹配, Result:{countResult} Bullet:{countBullet}, Damage:{maxLength}",
                        data.Responsibility,
                        "Assets/Table/" + data.SourceTable + ".txt",
                        table[i]));
                }
            }

            if (results.Count == 0)
                results.Add(new TableCheckerResult(
                    data.SourceTable,
                    "NA",
                    data.SourceColumn,
                    "0",
                    $"表{data.SourceTable}的所有PartnerID全部配置正确",
                    data.Responsibility,
                    "Assets/Table/" + data.SourceTable + ".txt",
                    null));

            return results;
        }


        /// <summary>
        /// 同一个角色同一个技能层级不能有两个技能
        /// </summary>
        public static List<TableCheckerResult> SkillListForRole_LayerPositionCheck(TableCheckerCheckItems data) {
            Dictionary<string, Dictionary<string, int>> characters = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, HashSet<string>> skills = new Dictionary<string, HashSet<string>>();
            List<TableCheckerResult> results = new List<TableCheckerResult>();

            List<Dictionary<string, string>> table;
            if (!Tables.ContainsKey(data.SourceTable)) {
                table = ReadTable(data.SourceTable);
            } else
                table = Tables[data.SourceTable];

            for (int i = 0; i < table.Count; i++) {
                if (table[i]["SkillLayer"] == "")
                    continue;
                Progress.Report(data.ProgressId, i / (float)table.Count);
                if (!characters.ContainsKey(table[i]["SkillPartnerID"])) {
                    characters[table[i]["SkillPartnerID"]] = new Dictionary<string, int>();
                    skills[table[i]["SkillPartnerID"]] = new HashSet<string>();
                }

                if (skills[table[i]["SkillPartnerID"]].Contains(table[i]["SkillScript"]))
                    continue;

                if (characters[table[i]["SkillPartnerID"]].ContainsKey(table[i]["SkillLayer"])) {
                    results.Add(new TableCheckerResult(
                        data.SourceTable,
                        $"{i + 3}",
                        data.SourceColumn,
                        data.Importance,
                        $"表{data.SourceTable}的第{i + 3}行SkillLayer的位置和{characters[table[i]["SkillPartnerID"]][table[i]["SkillLayer"]]}行重复",
                        data.Responsibility,
                        "Assets/Table/" + data.SourceTable + ".txt",
                        table[i]));
                } else {
                    characters[table[i]["SkillPartnerID"]][table[i]["SkillLayer"]] = i + 3;
                    skills[table[i]["SkillPartnerID"]].Add(table[i]["SkillScript"]);
                }
            }

            if (results.Count == 0)
                results.Add(new TableCheckerResult(
                    data.SourceTable,
                    "NA",
                    data.SourceColumn,
                    "0",
                    $"表{data.SourceTable}的所有技能位置全部配置正确",
                    data.Responsibility,
                    "Assets/Table/" + data.SourceTable + ".txt",
                    null));

            return results;
        }

        /// <summary>
        /// 所需技能点应该等于Layer * 5
        /// </summary>
        public static List<TableCheckerResult> SkillListForRole_LayerPointSpentRequirement(TableCheckerCheckItems data) {
            List<TableCheckerResult> results = new List<TableCheckerResult>();
            List<Dictionary<string, string>> table;
            if (!Tables.ContainsKey(data.SourceTable)) {
                table = ReadTable(data.SourceTable);
            } else
                table = Tables[data.SourceTable];

            for (int i = 0; i < table.Count; i++) {
                if (table[i]["SkillLayer"] == "")
                    continue;
                Progress.Report(data.ProgressId, i / (float)table.Count);
                int layer = int.Parse(table[i]["SkillLayer"].Split('|')[0]);


                if (table[i]["LevelupAlreadyCost"] == "") {
                    if (layer != 0) {
                        results.Add(new TableCheckerResult(
                            data.SourceTable,
                            $"{i + 3}",
                            data.SourceColumn,
                            data.Importance,
                            $"表{data.SourceTable}的第{i + 3}行LevelupAlreadyCost不是技能层级的5倍",
                            data.Responsibility,
                            "Assets/Table/" + data.SourceTable + ".txt",
                            table[i]));
                    }
                    continue;
                }

                int pointSpent = int.Parse(table[i]["LevelupAlreadyCost"]);

                if (pointSpent != layer * 5) {
                    results.Add(new TableCheckerResult(
                        data.SourceTable,
                        $"{i + 3}",
                        data.SourceColumn,
                        data.Importance,
                        $"表{data.SourceTable}的第{i + 3}行LevelupAlreadyCost不是技能层级的5倍",
                        data.Responsibility,
                        "Assets/Table/" + data.SourceTable + ".txt",
                        table[i]));
                }
            }

            if (results.Count == 0)
                results.Add(new TableCheckerResult(
                    data.SourceTable,
                    "NA",
                    data.SourceColumn,
                    "0",
                    $"表{data.SourceTable}的所有LevelupAlreadyCost全部配置正确",
                    data.Responsibility,
                    "Assets/Table/" + data.SourceTable + ".txt",
                    null));
            return results;
        }

        public static List<TableCheckerResult> SkillListForRole_SuperArmorCheck(TableCheckerCheckItems data) {
            List<TableCheckerResult> results = new List<TableCheckerResult>();
            List<Dictionary<string, string>> table = ReadTable(data.SourceTable);

            for (int i = 0; i < table.Count; i++) {
                var row = table[i];

                if (row["SkillType"].Equals("0"))
                    continue;

                if (row["SuperArmorID"] != "") {
                } else if (row["IncreaseSuperArmor"] != "" && row["PvPIncreaseSuperArmor"] != "" && row["MagicIncreaseSuperArmor"] != "" && row["PvPMagicIncreaseSuperArmor"] != "") {
                } else {
                    if (results.Count == 0)
                        results.Add(new TableCheckerResult(
                            data.SourceTable,
                            $"{i + 3}",
                            data.SourceColumn,
                            data.Importance,
                            $"表{data.SourceTable}的{i + 3}行中，既没有填写霸体ID，也没有填写霸体值",
                            data.Responsibility,
                            "Assets/Table/" + data.SourceTable + ".txt",
                            row));
                }

            }

            if (results.Count == 0)
                results.Add(new TableCheckerResult(
                    data.SourceTable,
                    "NA",
                    data.SourceColumn,
                    "0",
                    $"表{data.SourceTable}的所有霸体全部配置正确",
                    data.Responsibility,
                    "Assets/Table/" + data.SourceTable + ".txt",
                    null));
            return results;
        }

        #endregion

        #region XEntityPresentation

        public static List<TableCheckerResult> XEntityPresentation_HitReactionCheck(TableCheckerCheckItems data) {
            List<TableCheckerResult> results = new List<TableCheckerResult>();

            List<Dictionary<string, string>> table;
            if (!Tables.ContainsKey("XEntityPresentation")) {
                table = ReadTable("XEntityPresentation");
            } else
                table = Tables["XEntityPresentation"];

            for (int i = 0; i < table.Count; i++) {
                try {
                    var row = table[i];
                    if (row["BeHit"] == "")
                        continue;

                    string[] s = row["BeHit"].Split('|');
                    for (int j = 0; j < s.Length; j++) {
                        string[] ss = s[j].Split('=');

                        if (ss.Length < 2) {
                            results.Add(new TableCheckerResult(
                                data.SourceTable,
                                $"{i + 3}",
                                data.SourceColumn,
                                data.Importance,
                                $"表{data.SourceTable}的{i + 3}行{data.SourceColumn}列 ID{row["PresentID"]} {row["Name"]} 的受击脚本{ss[0]}无法被找到",
                                data.Responsibility,
                                "Assets/Table/" + data.SourceTable + ".txt",
                                table[i]));
                            continue;
                        }

                        string path = $@"{DATAPATH}\BundleRes\HitPackage\{row["BehitLocation"]}{ss[1]}.bytes";

                        if (!File.Exists(path)) {
                            results.Add(new TableCheckerResult(
                                data.SourceTable,
                                $"{i + 3}",
                                data.SourceColumn,
                                data.Importance,
                                $"表{data.SourceTable}的{i + 3}行{data.SourceColumn}列 ID{row["PresentID"]} {row["Name"]} 的受击脚本{ss[1]}无法被找到",
                                data.Responsibility,
                                "Assets/Table/" + data.SourceTable + ".txt",
                                table[i]));
                        }
                    }
                } catch {
                    Debug.Log(table[i]["BeHit"]);
                }
            }

            if (results.Count == 0)
                results.Add(new TableCheckerResult(
                    data.SourceTable,
                    "NA",
                    data.SourceColumn,
                    "0",
                    $"表{data.SourceTable}的受击反应配置没有检查到问题",
                    data.Responsibility,
                    "Assets/table/" + data.SourceTable + ".txt",
                    null));
            return results;
        }

        #endregion

        #region SkillListForEnemy

        public static List<TableCheckerResult> BuffList_BuffID_Level_Check(TableCheckerCheckItems data) {
            List<TableCheckerResult> results = new List<TableCheckerResult>();

            List<Dictionary<string, string>> table = ReadTable(data.SourceTable);

            List<Dictionary<string, string>> linkedTable = ReadTable(data.LinkedTable);

            List<Dictionary<string, string>> buffIDListTable = ReadTable("BuffIDList");

            HashSet<string> set = new HashSet<string>();
            for (int i = 0; i < linkedTable.Count; i++)
                set.Add(linkedTable[i]["BuffID"] + "\t" + linkedTable[i]["BuffLevel"]);

            HashSet<string> set2 = new HashSet<string>();
            for (int i = 0; i < buffIDListTable.Count; i++)
                set2.Add(buffIDListTable[i]["ID"]);

            for (int i = 0; i < table.Count; i++) {
                if (table[i][data.SourceColumn] == "")
                    continue;
                Progress.Report(data.ProgressId, i / (float)table.Count);
                string[] buffs = table[i][data.SourceColumn].Split('|');
                for (int j = 0; j < buffs.Length; j++) {
                    if (buffs[j] == "")
                        continue;
                    string[] line = buffs[j].Split('=');
                    try {
                        if (line[0].CompareTo("-1") == 0 || line[1].CompareTo("-1") == 0) {
                            if ((line[1].CompareTo("-1") == 0 && !set2.Contains(line[0])))
                                results.Add(new TableCheckerResult(
                                    data.SourceTable,
                                    $"{i + 3}",
                                    data.SourceColumn,
                                    data.Importance,
                                    $"表{data.SourceTable}的{i + 3}行{data.SourceColumn}列为buff等级为-1但是，BuffID不能够在BuffIDList表中找到",
                                    data.Responsibility,
                                    "Assets/Table/" + data.SourceTable + ".txt",
                                    table[i]));
                            continue;
                        }

                        if (!set.Contains(line[0] + '\t' + line[1])) {
                            results.Add(new TableCheckerResult(
                                data.SourceTable,
                                $"{i + 3}",
                                data.SourceColumn,
                                data.Importance,
                                $"表{data.SourceTable}的{i + 3}行{data.SourceColumn}列的BuffID + 等级 不能够在Buff表中找到",
                                data.Responsibility,
                                "Assets/Table/" + data.SourceTable + ".txt",
                                table[i]));
                        }
                    } catch (Exception e) {
                        results.Add(new TableCheckerResult(
                            data.SourceTable,
                            $"{i + 3}",
                            data.SourceColumn,
                            data.Importance,
                            $"表{data.SourceTable}的{i + 3}行{data.SourceColumn}列配置错误",
                            data.Responsibility,
                            "Assets/Table/" + data.SourceTable + ".txt",
                            table[i]));
                    }
                }
            }

            if (results.Count == 0)
                results.Add(new TableCheckerResult(
                    data.SourceTable,
                    "NA",
                    data.SourceColumn,
                    "0",
                    $"表{data.SourceTable}的所有buffID均能在buff表中找到",
                    data.Responsibility,
                    "Assets/Table/" + data.SourceTable + ".txt",
                    null));

            return results;
        }

        #endregion

        #region UnitAITable

        /// <summary>
        /// 检查指定技能是否在三张技能表中的任意一张
        /// </summary>
        /// <returns></returns>
        public static List<TableCheckerResult> SkillCheck(TableCheckerCheckItems data) {
            List<TableCheckerResult> results = new List<TableCheckerResult>();

            var table = ReadTable(data.SourceTable);
            var skillListForRole = ReadTable("SkillListForRole");
            var skillListForEnemy = ReadTable("SkillListForEnemy");
            var skillListForPet = ReadTable("SkillListForPet");

            var partnerInfoTable = ReadTable("PartnerInfo");
            var xpresentationTable = ReadTable("XEntityPresentation");
            var xentityStatisticsTable = ReadTable("XEntityStatistics");

            Dictionary<string, string> skillListForRoleDic = new Dictionary<string, string>();
            Dictionary<string, string> skillListForEnemySet = new Dictionary<string, string>();
            Dictionary<string, string> skillListForPetSet = new Dictionary<string, string>();

            Dictionary<string, string> partnerInfoSet = new Dictionary<string, string>();
            Dictionary<string, string> xentityStatisticsDic = new Dictionary<string, string>();
            Dictionary<string, string> xpresentationDic = new Dictionary<string, string>();

            for (int i = 0; i < skillListForEnemy.Count; i++)
                skillListForEnemySet[skillListForEnemy[i]["SkillScript"]] = skillListForEnemy[i]["XEntityStatisticsID"];

            for (int i = 0; i < skillListForRole.Count; i++)
                skillListForRoleDic[skillListForRole[i]["SkillScript"]] = skillListForRole[i]["SkillPartnerID"];

            for (int i = 0; i < skillListForPet.Count; i++)
                skillListForPetSet[skillListForPet[i]["SkillScript"]] = skillListForPet[i]["XEntityStatisticsID"];

            for (int i = 0; i < partnerInfoTable.Count; i++)
                partnerInfoSet[partnerInfoTable[i]["SkillPartnerID"]] = partnerInfoTable[i]["PresentId"];

            for (int i = 0; i < xpresentationTable.Count; i++)
                xpresentationDic[xpresentationTable[i]["PresentID"]] = xpresentationTable[i]["SkillLocation"];

            for (int i = 0; i < xentityStatisticsTable.Count; i++)
                xentityStatisticsDic[xentityStatisticsTable[i]["ID"]] = xentityStatisticsTable[i]["PresentID"];

            DirectoryInfo skillPackageDirectory = new DirectoryInfo($"{DATAPATH}/BundleRes/SkillPackage/");
            DirectoryInfo[] dds = skillPackageDirectory.GetDirectories();

            for (int i = 0; i < table.Count; i++) {
                if (table[i][data.SourceColumn] == "")
                    continue;
                Progress.Report(data.ProgressId, i / (float)table.Count);
                string[] s;

                Progress.Report(data.ProgressId, i / (float)table.Count);

                if (table[i][data.SourceColumn].Contains("=") && table[i][data.SourceColumn].Contains("|")) {
                    List<string> list = new List<string>(); 
                    string[] s1 = table[i][data.SourceColumn].Split('=');
                    for (int j = 0; j < s1.Length; j++) {
                        string[] s2 = s1[j].Split('|');
                        for (int k = 0; k < s2.Length; k++)
                            list.Add(s2[k]);
                    }
                    s = list.ToArray();
                } else if (table[i][data.SourceColumn].Contains("=")) {
                    s = table[i][data.SourceColumn].Split('=');
                } else {
                    s = table[i][data.SourceColumn].Split('|');
                }

                for (int j = 0; j < s.Length; j++) {
                    if (s[j] == "")
                        continue;
                    if (!(skillListForEnemySet.ContainsKey(s[j]) || skillListForPetSet.ContainsKey(s[j]) || skillListForRoleDic.ContainsKey(s[j]))) {
                        results.Add(new TableCheckerResult(
                            data.SourceTable,
                            $"{i + 3}",
                            data.SourceColumn,
                            data.Importance,
                            $"表{data.SourceTable}的{i + 3}行{data.SourceColumn}列技能{s[j]}无法在任何技能表中找到",
                            data.Responsibility,
                            "Assets/Table/" + data.SourceTable + ".txt",
                            table[i]));
                    } else if (skillListForRoleDic.ContainsKey(s[j])){
                        //先从SkillListForRole表中获得 PartnerID
                        //然后从PartnerInfo表中获得PresentID
                        //再从Xpresentation表中获得技能目录
                        //skillListForEnemySet的value已经记录了PartnerID

                        string partnerID = skillListForRoleDic[s[j]];
                        string presentID = "";
                        string path = "";
                        if (partnerInfoSet.ContainsKey(partnerID))
                            presentID = partnerInfoSet[partnerID];
                        if (xpresentationDic.ContainsKey(presentID))
                            path = xpresentationDic[presentID];

                        if (File.Exists($"{DATAPATH}/BundleRes/SkillPackage/{path}{s[j]}.bytes")) {
                            continue;
                        }

                        results.Add(new TableCheckerResult(
                            data.SourceTable,
                            $"{i + 3}",
                            data.SourceColumn,
                            data.Importance,
                            $"表{data.SourceTable}的{i + 3}行{data.SourceColumn}列技能{s[j]}可以在SkillListForRole表中找到，但是对应技能文件{path}{s[j]}.bytes不存在",
                            data.Responsibility,
                            "Assets/Table/" + data.SourceTable + ".txt",
                            table[i]));
                    } else {
                        string XEntityStatisticsID;
                        string presentID = "";
                        string path = "";
                        if (skillListForPetSet.ContainsKey(s[j]))
                            XEntityStatisticsID = skillListForPetSet[s[j]];
                        else
                            XEntityStatisticsID = skillListForEnemySet[s[j]];

                        //if (XEntityStatisticsID == "") {
                        //    //大部分敌人技能都没有填敌人ID，没有办法对应到presentID, 所以放弃治疗了
                        //    continue;
                        //}   这部分改从XEntitisty表里面查

                        if (xentityStatisticsDic.ContainsKey(XEntityStatisticsID))
                            presentID = xentityStatisticsDic[XEntityStatisticsID];

                        if (xpresentationDic.ContainsKey(presentID))
                            path = xpresentationDic[presentID];

                        if (File.Exists($"{DATAPATH}/BundleRes/SkillPackage/{path}{s[j]}.bytes")) {
                            continue;
                        } else {
                            bool found = false;
                            for (int k = 0; k < dds.Length; k++) {
                                if (File.Exists($"{DATAPATH}/BundleRes/SkillPackage/{dds[k].Name}/{s[j]}.bytes")) {
                                    found = true;
                                    break;
                                }
                            }
                            if (found)
                                continue;
                        }

                        results.Add(new TableCheckerResult(
                            data.SourceTable,
                            $"{i + 3}",
                            data.SourceColumn,
                            data.Importance,
                            $"表{data.SourceTable}的{i + 3}行{data.SourceColumn}列技能{s[j]}可以在SkillListForPet或SkillListForEnemy表中找到，但是对应SID{XEntityStatisticsID}和PID{presentID}技能文件{path}{s[j]}.bytes不存在",
                            data.Responsibility,
                            "Assets/Table/" + data.SourceTable + ".txt",
                            table[i]));
                    }
                }
            }


            if (results.Count == 0)
                results.Add(new TableCheckerResult(
                    data.SourceTable,
                    "NA",
                    data.SourceColumn,
                    "0",
                    $"表{data.SourceTable}表的{data.SourceColumn}所有技能配置正确",
                    data.Responsibility,
                    "Assets/Table/" + data.SourceTable + ".txt",
                    null));
            return results;
        }

        #endregion

        #region BuffList

        public static List<TableCheckerResult> BuffDuration_OvertTwoDigits(TableCheckerCheckItems data) {
            List<TableCheckerResult> results = new List<TableCheckerResult>();
            var table = ReadTable(data.SourceTable);
            for (int i = 0; i < table.Count; i++) { 
                int pos = table[i]["BuffDuration"].LastIndexOf('.');
                if (pos == -1)
                    continue;

                string dec = table[i]["BuffDuration"].Substring(pos + 1);

                if (dec.Length > 1)
                    results.Add(new TableCheckerResult(
                        data.SourceTable,
                        $"{i + 3}",
                        data.SourceColumn,
                        data.Importance,
                        $"表{data.SourceTable}的{i + 3}行{data.SourceColumn}列的buff持续时间小数点2位以上, {table[i]["BuffDuration"]}",
                        data.Responsibility,
                        "Assets/Table/" + data.SourceTable + ".txt",
                        table[i]));
            }
            if (results.Count == 0)
                results.Add(new TableCheckerResult(
                    data.SourceTable,
                    "NA",
                    data.SourceColumn,
                    "0",
                    $"表{data.SourceTable}表的{data.SourceColumn}所有技能配置正确",
                    data.Responsibility,
                    "Assets/Table/" + data.SourceTable + ".txt",
                    null));
            return results;
        }

        #endregion

            ///读表工具
        #region TableReader

            /// <summary>
            /// 读取一张表格，存入Dic中
            /// </summary>
            /// <param name="table">表格名, 在/table/中</param>
            /// <returns></returns>
            public static List<Dictionary<string, string>> ReadTable(string table, bool fullpath = false) {
            var result = new List<Dictionary<string, string>>();

            try{
                FileStream fs;
                if (fullpath)
                    fs = new FileStream(table, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                else
                    fs = new FileStream($"{DATAPATH}/table/{table}.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using StreamReader sr = new StreamReader(fs);

                string[] header = sr.ReadLine().Split('\t');
                sr.ReadLine();

                while (!sr.EndOfStream) {
                    string[] row = sr.ReadLine().Split('\t');
                    Dictionary<string, string> rowDic = new Dictionary<string, string>();

                    for (int i = 0; i < header.Length; i++)
                        rowDic[header[i]] = row[i];

                    result.Add(rowDic);
                }
            } catch (Exception e) {

                //弹一个窗警告sharing violation
                Debug.Log("表格读取失败！请确保表格没有被其他程序占用");
                Debug.Log(e.Message);
            }

            return result;
        }

        //public static List<Dictionary<string, string>> NewReadTable(string table) {
        //    var result = new List<Dictionary<string, string>>();
        //    FileStream fs = new FileStream($"{DATAPATH}/table/{table}.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        //    byte[] buffer = new byte[1024];
        //    int c;
        //    string s = "";

        //    int line = 0;
        //    List<string> header = new List<string>();

        //    void ReadLine(string ss) {
        //        string[] rows = ss.Split('\t');
        //        if (line == 0) {
        //            for (int j = 0; j < rows.Length; j++) {
        //                header.Add(rows[j]);
        //            }
        //        } else if (line != 1){
        //            result.Add(new Dictionary<string, string>());
        //            for (int j = 0; j < rows.Length; j++)
        //                result[line - 2][header[j]] = rows[j];
        //        }
        //        line++;
        //    }

        //    while ((c = fs.Read(buffer, 0, buffer.Length)) > 0) {
        //        s = $"{s}{Encoding.Unicode.GetString(buffer, 0, c)}";
        //        string[] lines = s.Split('\n');
        //        s = lines[lines.Length - 1];
        //        for (int i = 0; i < lines.Length - 1; i++) {
        //            ReadLine(lines[i]);
        //        }
        //    }
        //    if (s.Length > 0) {
        //        ReadLine(s);
        //    }

        //    //for (int i = 0; i < header.Count; i++)
        //    //    Debug.Log(header[i]);

        //    fs.Close();
        //    return result;
        //}

        #endregion

        #region Batch Check

        /// <summary>
        /// Used for Jenkins automation
        /// </summary>
        //[MenuItem("Tools/TDTools/表格检查工具/batch表格检查工具")]
        public static void BathCheckTables() {
            TableChecker.DATAPATH = Application.dataPath;
            var list = new TableCheckerCheckList();
            var results = new List<TableCheckerResult>();
            list.Load();
            for (int i = 0; i < list.List.Count; i++) {
                list.List[i].enabled = true;
                var r = list.List[i].Run();
                results.AddRange(r);
            }

            results.Sort(new TableCheckerResultSortByImportance());

            string targetDir = "~/Desktop";
            string[] args = System.Environment.GetCommandLineArgs();
            // find: -executeMethod
            //   +1: JenkinsBuild.BuildMacOS
            //   +2: VRDungeons
            //   +3: /Users/Shared/Jenkins/Home/jobs/VRDungeons/builds/47/output
            for (int i = 0; i < args.Length; i++) {
                if (args[i] == "-executeMethod") {
                    if (i + 2 < args.Length) {
                        targetDir = args[i + 2];
                    } else {
                        System.Console.WriteLine("[JenkinsBuild] Incorrect Parameters");
                        return;
                    }
                }
            }
            ///不知道为什么Jenkins传进来地址会带个"
            targetDir = targetDir.Remove(targetDir.Length - 1);
            //targetDir = System.Environment.GetEnvironmentVariable("JENKINS_HOME")
            //    + "/jobs/${JOB_NAME}/builds/"
            //    + System.Environment.GetEnvironmentVariable("BUILD_NUMBER")
            //    + "/output/";
            System.Console.WriteLine("Target Path: " + targetDir);
            Directory.CreateDirectory(targetDir);
            string path = targetDir + "TableCheckerResult.txt";
            System.Console.WriteLine("Write to Path: " + path);
            //string path = Application.dataPath + "/Editor/TableConfigChecker/Results/表格检查结果存档" + DateTime.Now.ToString("yyyy-MM-dd HH-mm") + ".txt";

            StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Unicode);
            bool pass = true;
            foreach (var result in results) {
                try {
                    int imp = int.Parse(result.Importantce);
                    if (imp > 4) {
                        pass = false;
                    }
                    if (imp > 0)
                        sw.Write(result.Importantce.ToString() + "\t" + result.Table + "\t" + result.Row + "\t" + result.Column + "\t" + result.Responsibility + "\t" + result.Description + "\n");
                } catch (Exception e) {
                    sw.Write($"{e.Message}\t{e.StackTrace}\n");
                    pass = false;
                }
            }


            if (pass) {
                sw.WriteLine("全部检测通过(重要度3或以下被忽略)");

                //try {
                //    HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://qyapi.weixin.qq.com/cgi-bin/webhook/send?key=7f5201db-8ced-4381-bc99-29f2147f05fb&debug=1");
                //    req.Method = "POST";
                //    req.ContentType = "application/json";
                //    req.Referer = null;
                //    req.AllowAutoRedirect = true;
                //    req.Accept = "*/*";

                //    Stream stream = req.GetRequestStream();
                //    string json = "{\"msgtype\":\"text\"," +
                //        "\"text\":{\"content\":\"" + $"{System.DateTime.Now}" + "表格配置检查全部通过 (测试消息，以后通过不会有消息) http://10.253.48.187:9090/job/TableChecker/lastBuild/artifact/TableCheckerResult.txt\"}}";
                //    var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                //    stream.Write(bytes, 0, bytes.Length);
                //    stream.Close();

                //    HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                //    using (StreamReader sr = new StreamReader(response.GetResponseStream())) {
                //        Console.WriteLine(sr.ReadToEnd());
                //    }
                //} catch {
                //}
            }

            sw.Close();

            //Build failed
            if (!pass) {
                try {
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://qyapi.weixin.qq.com/cgi-bin/webhook/send?key=7f5201db-8ced-4381-bc99-29f2147f05fb");
                    req.Method = "POST";
                    req.ContentType = "application/json";
                    req.Referer = null;
                    req.AllowAutoRedirect = true;
                    req.Accept = "*/*";

                    Stream stream = req.GetRequestStream();
                    string json = "{\"msgtype\":\"text\"," +
                       "\"text\":{\"content\":\"" + $"{System.DateTime.Now}" + "表格配置检查没有通过 http://10.253.48.187:9090/job/TableChecker/lastBuild/artifact/TableCheckerResult.txt\"}}";
                    var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Close();

                    HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                    using (StreamReader sr = new StreamReader(response.GetResponseStream())) {
                        Console.WriteLine(sr.ReadToEnd());
                    }
                } catch { 
                }
                    
                throw new Exception("表格检查没有通过!");
            }
        }

        //[MenuItem("Tools/TDTools/配置检查工具/表格检查工具 测试")]
        //static void RunChecker() {
        //    TableChecker.DATAPATH = Application.dataPath;
        //    var list = new TableCheckerCheckList();
        //    var results = new List<TableCheckerResult>();
        //    list.Load();
        //    for (int i = 0; i < list.List.Count; i++) {
        //        list.List[i].enabled = true;
        //        var r = list.List[i].Run();
        //        results.AddRange(r);
        //    }

        //    results.Sort(new TableCheckerResultSortByImportance());

        //    bool pass = true;
        //    foreach (var result in results) {
        //        try {
        //            int imp = int.Parse(result.Importantce);
        //            if (imp > 4) {
        //                pass = false;
        //            }
        //            if (imp > 0)
        //                Debug.Log(result.Importantce.ToString() + "\t" + result.Table + "\t" + result.Row + "\t" + result.Column + "\t" + result.Responsibility + "\t" + result.Description + "\n");
        //        } catch (Exception e) {
        //            Debug.Log($"{e.Message}\t{e.StackTrace}\n");
        //            pass = false;
        //        }
        //    }
        //}

        #endregion
    }
}
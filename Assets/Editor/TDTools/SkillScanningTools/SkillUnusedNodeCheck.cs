using EcsData;
using EditorNode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TDTools {
    public class SkillUnusedNodeCheck : MonoBehaviour {
        /// <summary>
        /// 检查指定技能是否在三张技能表中的任意一张
        /// </summary>
        /// <returns></returns>
        //public static void SkillCheck() {

        //    var skillListForRole = TableChecker.TableChecker.ReadTable("SkillListForRole");
        //    var skillListForEnemy = TableChecker.TableChecker.ReadTable("SkillListForEnemy");
        //    var skillListForPet = TableChecker.TableChecker.ReadTable("SkillListForPet");

        //    var partnerInfoTable = TableChecker.TableChecker.ReadTable("PartnerInfo");
        //    var xpresentationTable = TableChecker.TableChecker.ReadTable("XEntityPresentation");
        //    var xentityStatisticsTable = TableChecker.TableChecker.ReadTable("XEntityStatistics");

        //    Dictionary<string, string> skillListForRoleDic = new Dictionary<string, string>();
        //    Dictionary<string, string> skillListForEnemySet = new Dictionary<string, string>();
        //    Dictionary<string, string> skillListForPetSet = new Dictionary<string, string>();

        //    Dictionary<string, string> partnerInfoSet = new Dictionary<string, string>();
        //    Dictionary<string, string> xentityStatisticsDic = new Dictionary<string, string>();
        //    Dictionary<string, string> xpresentationDic = new Dictionary<string, string>();

        //    for (int i = 0; i < skillListForEnemy.Count; i++)
        //        skillListForEnemySet[skillListForEnemy[i]["SkillScript"]] = skillListForEnemy[i]["XEntityStatisticsID"];

        //    for (int i = 0; i < skillListForRole.Count; i++)
        //        skillListForRoleDic[skillListForRole[i]["SkillScript"]] = skillListForRole[i]["SkillPartnerID"];

        //    for (int i = 0; i < skillListForPet.Count; i++)
        //        skillListForPetSet[skillListForPet[i]["SkillScript"]] = skillListForPet[i]["XEntityStatisticsID"];

        //    for (int i = 0; i < partnerInfoTable.Count; i++)
        //        partnerInfoSet[partnerInfoTable[i]["SkillPartnerID"]] = partnerInfoTable[i]["PresentId"];

        //    for (int i = 0; i < xpresentationTable.Count; i++)
        //        xpresentationDic[xpresentationTable[i]["PresentID"]] = xpresentationTable[i]["SkillLocation"];

        //    for (int i = 0; i < xentityStatisticsTable.Count; i++)
        //        xentityStatisticsDic[xentityStatisticsTable[i]["ID"]] = xentityStatisticsTable[i]["PresentID"];

        //    DirectoryInfo skillPackageDirectory = new DirectoryInfo($"{Application.dataPath}/BundleRes/SkillPackage/");
        //    DirectoryInfo[] dds = skillPackageDirectory.GetDirectories();

        //    for (int i = 0; i < table.Count; i++) {
        //        if (table[i][data.SourceColumn] == "")
        //            continue;

        //        string[] s;

        //        if (table[i][data.SourceColumn].Contains("=") && table[i][data.SourceColumn].Contains("|")) {
        //            List<string> list = new List<string>();
        //            string[] s1 = table[i][data.SourceColumn].Split('=');
        //            for (int j = 0; j < s1.Length; j++) {
        //                string[] s2 = s1[j].Split('|');
        //                for (int k = 0; k < s2.Length; k++)
        //                    list.Add(s2[k]);
        //            }
        //            s = list.ToArray();
        //        } else if (table[i][data.SourceColumn].Contains("=")) {
        //            s = table[i][data.SourceColumn].Split('=');
        //        } else {
        //            s = table[i][data.SourceColumn].Split('|');
        //        }

        //        for (int j = 0; j < s.Length; j++) {
        //            if (!(skillListForEnemySet.ContainsKey(s[j]) || skillListForPetSet.ContainsKey(s[j]) || skillListForRoleDic.ContainsKey(s[j]))) {

        //                //无法在任何技能表中找到

        //            } else if (skillListForRoleDic.ContainsKey(s[j])) {
        //                //先从SkillListForRole表中获得 PartnerID
        //                //然后从PartnerInfo表中获得PresentID
        //                //再从Xpresentation表中获得技能目录
        //                //skillListForEnemySet的value已经记录了PartnerID

        //                string partnerID = skillListForRoleDic[s[j]];
        //                string presentID = "";
        //                string path = "";
        //                if (partnerInfoSet.ContainsKey(partnerID))
        //                    presentID = partnerInfoSet[partnerID];
        //                if (xpresentationDic.ContainsKey(presentID))
        //                    path = xpresentationDic[presentID];

        //                if (File.Exists($"{Application.dataPath}/BundleRes/SkillPackage/{path}{s[j]}.bytes")) {
        //                    continue;
        //                }


        //                //技能不存在
        //            } else {
        //                string XEntityStatisticsID;
        //                string presentID = "";
        //                string path = "";
        //                if (skillListForPetSet.ContainsKey(s[j]))
        //                    XEntityStatisticsID = skillListForPetSet[s[j]];
        //                else
        //                    XEntityStatisticsID = skillListForEnemySet[s[j]];

        //                //if (XEntityStatisticsID == "") {
        //                //    //大部分敌人技能都没有填敌人ID，没有办法对应到presentID, 所以放弃治疗了
        //                //    continue;
        //                //}

        //                if (xentityStatisticsDic.ContainsKey(XEntityStatisticsID))
        //                    presentID = xentityStatisticsDic[XEntityStatisticsID];

        //                if (xpresentationDic.ContainsKey(presentID))
        //                    path = xpresentationDic[presentID];

        //                if (File.Exists($"{Application.dataPath}/BundleRes/SkillPackage/{path}{s[j]}.bytes")) {
        //                    continue;
        //                } else if (path == "") {
        //                    bool found = false;
        //                    for (int k = 0; k < dds.Length; k++)
        //                        if (File.Exists($"{Application.dataPath}/BundleRes/SkillPackage/{dds[k].Name}/{s[j]}.bytes")) {
        //                            found = true;
        //                            break;
        //                        }
        //                    if (found)
        //                        continue;
        //                }

        //                Debug.Log("技能文件不存在");
        //            }
        //        }
        //    }
        //}


        /// <summary>
        /// 检查指定技能是否在三张技能表中的任意一张
        /// </summary>
        [MenuItem("Tools/TDTools/废弃工具/技能有效无效节点检测")]
        public static void SkillCheck() {
            List<FileInfo> files = new List<FileInfo>();
            DirectoryInfo d = new DirectoryInfo($@"{Application.dataPath}\BundleRes\SkillPackage");
            List<FileInfo> GetAllSkillInDirectory(DirectoryInfo d) {
                List<FileInfo> result = new List<FileInfo>(d.GetFiles("*.bytes"));
                var dd = d.GetDirectories();
                for (int i = 0; i < dd.Length; i++) {
                    result.AddRange(GetAllSkillInDirectory(dd[i]));
                }
                return result;
            }

            files = GetAllSkillInDirectory(d);

            FileStream stream = new FileStream($@"{Application.dataPath}\Editor\TDTools\SkillUnusedNodeCheck\SkillCheck{DateTime.Now.ToString("yyyy-MM-dd")}.txt", FileMode.Create);
            FileStream tsv = new FileStream($@"{Application.dataPath}\Editor\TDTools\SkillUnusedNodeCheck\SkillCheck{DateTime.Now.ToString("yyyy-MM-dd")}TSV.txt", FileMode.Create);
            byte[] ba = Encoding.UTF8.GetBytes($"技能名\t总节点数\tTimebased无输出节点数\t无效节点数\n");
            tsv.Write(ba, 0, ba.Length);
            for (int z = 0; z < files.Count; z ++) {
                try {
                    SkillGraph graph = new SkillGraph();
                    graph.NeedInitRes = false;
                    graph.OpenData(files[z].FullName);
                    int nodeCount = graph.GetNodeCount();
                    int nodeTimeBasedNoOutputCount = 0;
                    int inativeCount = 0;
                    MemoryStream ms = new MemoryStream();
                    for (int i = 0; i < nodeCount; i++) {
                        BaseSkillNode node = graph.GetNodeByIndex(i);
                        var data = node.GetHosterData<XBaseData>();
                        //是timebase,没有输出的节点计数
                        //节点总数，去掉无效节点

                        bool hasInput = false;
                        bool hasOutput = false;

                        for (int j = 0; j < node.pinList.Count; j++) {
                            var pin = node.pinList[j];
                            if (pin.connections.Count > 0 || pin.reverseConnections.Count > 0) {
                                if (pin.pinStream == BluePrint.PinStream.In)
                                    hasInput = true;
                                else if (pin.pinStream == BluePrint.PinStream.Out)
                                    hasOutput = true;
                            }
                        }
                        if (data.TimeBased && !hasOutput) {
                            nodeTimeBasedNoOutputCount++;
                            byte[] b = Encoding.UTF8.GetBytes($"    {node.NodeName}为TimeBased无输出节点\n");
                            ms.Write(b, 0, b.Length);
                        } else if (!data.TimeBased && !hasInput) {
                            if (!(node is PreConditionNode)) {
                                inativeCount++;
                                byte[] b = Encoding.UTF8.GetBytes($"    {node.NodeName}为无效节点\n");
                                ms.Write(b, 0, b.Length);
                            }
                        }

                    }
                    ba = Encoding.UTF8.GetBytes($"{files[z].Name}\t{nodeCount}\t{nodeTimeBasedNoOutputCount}\t{inativeCount}\n");
                    tsv.Write(ba, 0, ba.Length);

                    if (nodeTimeBasedNoOutputCount > 0 || inativeCount > 0) {
                        byte[] bytes = Encoding.UTF8.GetBytes($"{files[z].Name} 共有{nodeCount}个节点， {nodeTimeBasedNoOutputCount}为TimeBased无输出节点， {inativeCount}个为无效节点 \n");
                        stream.Write(bytes, 0, bytes.Length);
                        ms.Position = 0;
                        ms.CopyTo(stream);
                    }
                } catch (Exception e){
                    Debug.Log(e.Message);
                }
            }
            tsv.Close();
            stream.Close();
        }
     }

}

using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TDTools {

    public class SkillTalkIDListing : MonoBehaviour {
        public static Dictionary<int, List<string>> GetTables() {
            List<Dictionary<string, string>> unitAITable = TableReader.ReadTable("UnitAITable");
            List<Dictionary<string, string>> enemyModeStateTable = TableReader.ReadTable("EnemyModeState");
            List<Dictionary<string, string>> enemyJZTable = TableReader.ReadTable("EnemyJZ");
            List<Dictionary<string, string>> enemyStage = TableReader.ReadTable("EnemyStage");

            Dictionary<int, List<string>> monsterSkill = new Dictionary<int, List<string>>();

            for (int i = 0; i < enemyStage.Count; i ++) {
                int monsterID = int.Parse(enemyStage[i]["StaticsID"]);
                if (!monsterSkill.ContainsKey(monsterID))
                    monsterSkill[monsterID] = new List<string>();
                if (enemyStage[i]["SkillID"] != "")
                    monsterSkill[monsterID].Add(enemyStage[i]["SkillID"]);
            }

            for (int i = 0; i < enemyJZTable.Count; i++) {
                int monsterID = int.Parse(enemyJZTable[i]["StatictiscID"]);
                if (!monsterSkill.ContainsKey(monsterID))
                    monsterSkill[monsterID] = new List<string>();
                if (enemyJZTable[i]["LeaveSkillID"] != null)
                    monsterSkill[monsterID].Add(enemyJZTable[i]["LeaveSkillID"]);
            }

            for (int i = 0; i < enemyModeStateTable.Count; i++) {
                int monsterID = int.Parse(enemyModeStateTable[i]["EnemyID"]);
                if (!monsterSkill.ContainsKey(monsterID))
                    monsterSkill[monsterID] = new List<string>();
                if (enemyModeStateTable[i]["NormalEnterSkillID"] != "")
                    monsterSkill[monsterID].Add(enemyModeStateTable[i]["NormalEnterSkillID"]);
                if (enemyModeStateTable[i]["BindSkillList"] != null)
                    monsterSkill[monsterID].Add(enemyModeStateTable[i]["BindSkillList"]);
            }

            for (int i = 0; i < unitAITable.Count; i++) {
                int monsterID = int.Parse(unitAITable[i]["ID"]);
                if (!monsterSkill.ContainsKey(monsterID))
                    monsterSkill[monsterID] = new List<string>();

                string[] t;
                string[] column = { "MainSkillName",
                    "LeftSkillName",
                    "RightSkillName",
                    "BackSkillName",
                    "CheckingSkillName",
                    "CheckingSkillAndStopName",
                    "DashSkillName",
                    "FarSkillName",
                    "SelectSkillName",
                    "UnusedSkillName",
                    "TurnSkillName",
                    "MoveSkillName"
                };

                for (int k = 0; k < column.Length; k++) {
                    t = unitAITable[i][column[k]].Split('|');
                    for (int j = 0; j < t.Length; j++) {
                        if (t[j] != "")
                            monsterSkill[monsterID].Add(t[j]);
                    }
                }
            }


            return monsterSkill;
        }

        public static Dictionary<string, string> GetAllSkilLFiles(string path) {
            Dictionary<string, string> result = new Dictionary<string, string>();
            DirectoryInfo d = new DirectoryInfo(path);
            var ds = d.GetDirectories();
            foreach (var dd in ds) {
                var t = GetAllSkilLFiles(path + "/" + dd.Name);
                foreach (var pair in t) {
                    result[pair.Key] = pair.Value;
                }
            }
            var df = d.GetFiles();
            foreach (var ff in df) {
                result[ff.Name] = path + "/" + ff.Name + ".byte";
            }

            return result;
        }

        /// <summary>
        /// 根据talk ID列一遍
        /// </summary>
        [MenuItem("Tools/TDTools/废弃工具/TalkID列表扫描")]
        public static void Run() {     
            List<Dictionary<string, string>> table = TableReader.ReadTable("XEntityStatistics");
            var skills = GetTables();

            var files = GetAllSkilLFiles($"{Application.dataPath}/BundleRes/SkillPackage");
            Dictionary<string, int> filesChecked = new Dictionary<string, int>();

            for (int i = 0; i < table.Count; i++) {
                int id = int.Parse(table[i]["ID"]);
                if (!skills.ContainsKey(id))
                    continue;
                ///怪物表的ID
                List<string> names = skills[id];
                for (int j = 0; j < names.Count; j++) {
                    if (filesChecked.ContainsKey(names[j])) {
                        try {
                            SkillGraph graph = new SkillGraph();
                            graph.OpenData(files[names[j]]);
                            int count = graph.GetNodeCount();
                            for (int k = 0; k < count; k++) {
                                var t = graph.GetNodeByIndex(k);
                                var z = t.GetHosterData<XSpecialActionData>();
                                filesChecked[names[j]] = z.Type;
                            }
                        } catch (Exception e) {
                            Debug.Log(e.Message);
                        }
                    }
                    if (filesChecked[names[j]] == 15)
                        Debug.Log(id + " " + names[j]);
                }
            }
        }
    }
}
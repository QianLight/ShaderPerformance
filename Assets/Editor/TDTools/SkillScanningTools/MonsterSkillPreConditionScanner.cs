using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using CFEngine;
using EcsData;
using System;

namespace TDTools {
    public class MonsterSkillPreConditionScanner : EditorWindow {

        class TableNode {
            public HashSet<string> Set;
            public Dictionary<string, int> Dic;
            public List<Dictionary<string, string>> Table;

            public TableNode(string path, string IDName) {
                Table = TableChecker.TableChecker.ReadTable(path);
                Set = new HashSet<string>();
                Dic = new Dictionary<string, int>();
                for (int i = 0; i < Table.Count; i++) {
                    Dic[Table[i][IDName]] = i;
                }
            }
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

        static T DeserializeEcsData<T>(string path) {
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

        [MenuItem("Tools/TDTools/废弃工具/怪物技能PreCondition节点扫描")]
        public async static void Run() {
            string DATAPATH = Application.dataPath;
            TableChecker.TableChecker.DATAPATH = Application.dataPath;
            DirectoryInfo dir = new DirectoryInfo($@"{DATAPATH}\BundleRes\SkillPackage\");
            List<string> resuts = new List<string>();
            await Task.Run(() => {
                TableNode monsterTable = new TableNode("SkillListForEnemy", "SkillScript");
                var allFiles = GetAllFiles(dir, "*.bytes");
                Dictionary<string, string> fileDic = new Dictionary<string, string>();
                for (int i = 0; i < allFiles.Count; i++) {
                    fileDic[allFiles[i].Name.Substring(0, allFiles[i].Name.IndexOf('.')).ToLower()] = allFiles[i].FullName;
                }
                for (int i = 0; i < monsterTable.Table.Count; i++) {
                    try {
                        var row = monsterTable.Table[i];

                        if (!fileDic.ContainsKey(row["SkillScript"].ToLower()))
                            continue;

                        string path = fileDic[row["SkillScript"].ToLower()];

                        XSkillData data = DeserializeEcsData<XSkillData>(path);

                        if (data.PreConditionData.Count > 0) {
                            Debug.Log($"怪物技能<color=green>{row["SkillScript"]}</color>有PreCondition节点");
                            resuts.Add($"{row["SkillScript"]}\t{path}");
                        }
                    } catch (Exception e){
                        Debug.LogError(e.Message + "\n" + e.StackTrace);
                    }
                }


            });

            using StreamWriter sw = new StreamWriter(EditorUtility.SaveFilePanel("保存", "", "怪物PreCon结果", "txt"));
            for (int i = 0; i < resuts.Count; i++)
                sw.WriteLine(resuts[i]);
        }

    }
}
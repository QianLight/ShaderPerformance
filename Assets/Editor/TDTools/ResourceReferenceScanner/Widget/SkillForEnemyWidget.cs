using CFClient;
using CFEngine;
using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {
    public class SkillForEnemyWidget : InspectorElement {
        public SkillForEnemyWidget(ResourceReferenceInspector owner, Vector2 position, string skillScript, string linkedID = "") : base(owner, position, skillScript, linkedID) {
            InspectorNodeType = NodeType.XEntitySkill;
        }

        public override void BuildContent() {
            base.BuildContent();
            var table = TableDatabase.Instance.GetTable("SkillListForEnemy", "SkillScript");
            currentRow = table.Dic[ID].ToString();
            var row = table.Table[table.Dic[ID]];
            if (LinkedID == "")
                TitleLabel.text = $"技能: {row["SkillScript"]} {row["ScriptName"]}";
            else
                TitleLabel.text = $"技能: {row["SkillScript"]} {row["ScriptName"]} StatisticsID:{LinkedID}";
            Label labelRow = new Label($"第{table.Dic[ID] + 3}行");
            Container.Add(labelRow);

            if (LinkedID != "") {
                var gmButton = new GMButton(GMCommandType.Server, "链接的怪物GM指令", $"mob {LinkedID}");
                Container.Add(gmButton.Root);
            }

            foreach (var pair in row) {
                string key = pair.Key;

                if (!(Match(key) || Match(pair.Value)))
                    continue;

                if (pair.Value == "" || key.Equals("SkillScript") || key.Equals("ScriptName"))
                    continue;

                if (key.Equals("BeginBuff") || key.Equals("EndBuff") || key.Equals("ResistBuff")) {
                    var buffs = pair.Value.Split('|');
                    for (int i = 0; i < buffs.Length; i++) {
                        string[] s = buffs[i].Split('=');
                        if (s.Length < 2)
                            continue;
                        var ele = new ResourceHyperlink(this, NodeType.Buff, key, buffs[i], table.Comment[key], $"{s[0]}={s[1]}").Root;
                        Container.Add(ele);
                    }
                } else if (key.Equals("BuffID")) {
                    string[] buffs = pair.Value.Split('|');
                    for (int i = 0; i < buffs.Length; i++) {
                        string[] s = buffs[i].Split('=');
                        if (s.Length < 3)
                            continue;
                        var ele = new ResourceHyperlink(this, NodeType.Buff, key, buffs[i], table.Comment[key], $"{s[1]}={s[2]}").Root;
                        Container.Add(ele);
                    }
                } else if (key.Equals("XEntityStatisticsID")) {
                    var ele = new ResourceHyperlink(this, NodeType.Enemy, key, row[key], table.Comment[key]);
                    Container.Add(ele.Root);
                } else {
                    InspectorLabel label = new InspectorLabel(this, pair.Key, pair.Value, table.Comment[pair.Key]);
                    Container.Add(label.Root);
                }
            }
        }

        protected override GenericMenu ContextClick(ContextClickEvent evt)
        {
            var menu = base.ContextClick(evt);
#if USE_GM
            if (LinkedID != "") {
                menu.AddItem(new GUIContent($"GM指令: mob {LinkedID}"), false, () => {
                    CFCommand.singleton.ProcessServerCommand($"mob {LinkedID}");
                });
                menu.AddItem(new GUIContent($"GM指令: monstercastskill {LinkedID} {ID}"), false, () => {
                    CFCommand.singleton.ProcessServerCommand($"monstercastskill {LinkedID} {ID}");
                });
            }
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("用Excel打开"), false, () => {
                OpenExcel.OpenOffice($@"{Application.dataPath}\Table".Replace('/', '\\'), "SkillListForEnemy.txt", "F3", currentRow);
            });
            menu.AddItem(new GUIContent("用WPS打开"), false, () => {
                OpenExcel.OpenWps($@"{Application.dataPath}\Table".Replace('/', '\\'), "SkillListForEnemy.txt", "F3", currentRow);
            });

            menu.AddItem(new GUIContent("打开技能编辑器"), false, () => {
                string filePath = "";
                DirectoryInfo skillD = new DirectoryInfo($@"{Application.dataPath}\BundleRes\SkillPackage");
                DirectoryInfo[] ds = skillD.GetDirectories();
                for (int i = 0; i < ds.Length; i++) {
                    var files = ds[i].GetFiles($"{ID}.bytes");
                    if (files.Length > 0) {
                        filePath = files[0].FullName;
                        break;
                    }
                }

                if (filePath == "") {
                    Debug.Log("找不到技能文件！");
                    return;
                }

                var editor = EditorWindow.GetWindow<SkillEditor>();
                editor.OpenFile(filePath);
            });
#endif
            return menu;
        }
    }
}
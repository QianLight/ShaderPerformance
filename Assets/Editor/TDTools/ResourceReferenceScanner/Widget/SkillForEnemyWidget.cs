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
                TitleLabel.text = $"����: {row["SkillScript"]} {row["ScriptName"]}";
            else
                TitleLabel.text = $"����: {row["SkillScript"]} {row["ScriptName"]} StatisticsID:{LinkedID}";
            Label labelRow = new Label($"��{table.Dic[ID] + 3}��");
            Container.Add(labelRow);

            if (LinkedID != "") {
                var gmButton = new GMButton(GMCommandType.Server, "���ӵĹ���GMָ��", $"mob {LinkedID}");
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
                menu.AddItem(new GUIContent($"GMָ��: mob {LinkedID}"), false, () => {
                    CFCommand.singleton.ProcessServerCommand($"mob {LinkedID}");
                });
                menu.AddItem(new GUIContent($"GMָ��: monstercastskill {LinkedID} {ID}"), false, () => {
                    CFCommand.singleton.ProcessServerCommand($"monstercastskill {LinkedID} {ID}");
                });
            }
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("��Excel��"), false, () => {
                OpenExcel.OpenOffice($@"{Application.dataPath}\Table".Replace('/', '\\'), "SkillListForEnemy.txt", "F3", currentRow);
            });
            menu.AddItem(new GUIContent("��WPS��"), false, () => {
                OpenExcel.OpenWps($@"{Application.dataPath}\Table".Replace('/', '\\'), "SkillListForEnemy.txt", "F3", currentRow);
            });

            menu.AddItem(new GUIContent("�򿪼��ܱ༭��"), false, () => {
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
                    Debug.Log("�Ҳ��������ļ���");
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
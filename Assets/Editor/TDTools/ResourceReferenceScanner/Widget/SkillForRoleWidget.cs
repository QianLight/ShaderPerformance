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
    public class SkillForRoleWidget : InspectorElement {

        public SkillForRoleWidget(ResourceReferenceInspector owner, Vector2 position, string skillScript) : base(owner, position, skillScript) {
            InspectorNodeType = NodeType.PartnerSkill;
        }

        public override void BuildContent() {
            base.BuildContent();
            var table = TableDatabase.Instance.GetTable("SkillListForRole", "SkillScript");
            currentRow = table.Dic[ID].ToString();
            var row = table.Table[table.Dic[ID]];
            TitleLabel.text = $"主角技能: {row["SkillScript"]} {row["ScriptName"]}";

            HashSet<string> buffSet = new HashSet<string>();

            Label labelRow = new Label($"第{table.Dic[ID] + 3}行");
            Container.Add(labelRow);

            foreach (var pair in row) {
                string key = pair.Key;
                if (!(Match(key) || Match(pair.Value)))
                    continue;
                if (pair.Value == "" || key.Equals("SkillScript") || key.Equals("ScriptName"))
                    continue;

                if (key.Equals("Icon")) {
                    var ele = new ResourceHyperlink(this, NodeType.Texture2D, key, pair.Value, table.Comment[key], $@"BundleRes/UI/UISource/ui_skill/{row["Atlas"]}/{pair.Value}.png").Root;
                    Container.Add(ele);
                } else if (key.Equals("BindSkill") || key.Equals("PreSkill")) {
                    var skills = pair.Value.Split('|');
                    for (int i = 0; i < skills.Length; i++) {
                        var ele = new ResourceHyperlink(this, NodeType.PartnerSkill, key, skills[i], table.Comment[key]).Root;
                        Container.Add(ele);
                    }
                } else if (key.Equals("BeginBuff") || key.Equals("EndBuff") || key.Equals("ResistBuff") || key.Equals("AllBuff") || key.Equals("OnBuff") || key.Equals("OffBuff") || key.Equals("OnBuffRemove") || key.Equals("OffBuffRemove")) {
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
                } else if (key.Equals("SkillPartnerID")) {
                    var ele = new ResourceHyperlink(this, NodeType.Partner, key, row[key], table.Comment[key]);
                    Container.Add(ele.Root);
                } else {
                    InspectorLabel label = new InspectorLabel(this, pair.Key, pair.Value, table.Comment[pair.Key]);
                    Container.Add(label.Root);
                }
            }
        }

        protected override GenericMenu ContextClick(ContextClickEvent evt) {
            var menu = base.ContextClick(evt);
            menu.AddItem(new GUIContent("用Excel打开"), false, () => {
                OpenExcel.OpenOffice($@"{Application.dataPath}\Table".Replace('/', '\\'), "SkillListForRole.txt", "H3", currentRow);
            });
            menu.AddItem(new GUIContent("用WPS打开"), false, () => {
                OpenExcel.OpenWps($@"{Application.dataPath}\Table".Replace('/', '\\'), "SkillListForRole.txt", "H3", currentRow);
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
            return menu;
        }
    }
}
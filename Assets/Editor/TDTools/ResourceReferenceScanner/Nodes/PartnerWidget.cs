using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {
    public class PartnerWidget : InspectorElement {
        public PartnerWidget(ResourceReferenceInspector owner, Vector2 position, string id) : base(owner, position, id) {
            InspectorNodeType = NodeType.Partner;
        }

        public override void BuildContent() {
            base.BuildContent();
            var table = TableDatabase.Instance.GetTable("PartnerInfo", "ID");
            var row = table.Table[table.Dic[ID]];
            TitleLabel.text = $"主角: {ID} {row["Name"]}";
            Label labelRow = new Label($"第{table.Dic[ID] + 3}行");
            Container.Add(labelRow);

            foreach (var pair in row) {
                string key = pair.Key;
                if (!(Match(key) || Match(pair.Value)))
                    continue;
                if (pair.Value == "" || pair.Key.Equals("ID") || pair.Key.Equals("Name") || pair.Key.Equals("UIkeyword"))
                    continue;

                if (key.Equals("PresentId")) {
                    var ele = new ResourceHyperlink(this, NodeType.XEntityPresentation, key, pair.Value, table.Comment[key]).Root;
                    Container.Add(ele);
                } else if (key.Equals("Switch") || key.Equals("Assist") || key.Equals("AssistEnd")) {
                    var skills = pair.Value.Split('|');
                    for (int i = 0; i < skills.Length; i++) {
                        string[] s = skills[i].Split('=');
                        var ele = new ResourceHyperlink(this, NodeType.PartnerSkill, key, skills[i], table.Comment[key], s[1]).Root;
                        Container.Add(ele);
                    }
                } else if (key.Equals("StarAttr") || key.Equals("PartnerScoreRate")) {
                    var Attrs = pair.Value.Split('|');
                    InspectorLabel ele;
                    for (int i = 0; i < Attrs.Length; i++) {
                        ele = new InspectorLabel(this, key, Attrs[i], table.Comment[pair.Key]);
                        Container.Add(ele.Root);
                    }
                } else if (key.Equals("WinSkill") || key.Equals("ExtremeDodgeSkills")) {
                    var skills = pair.Value.Split('|');
                    for (int i = 0; i < skills.Length; i++) {
                        var ele = new ResourceHyperlink(this, NodeType.PartnerSkill, key, skills[i], table.Comment[key], skills[i]).Root;
                        Container.Add(ele);
                    }
                } else if (key.Equals("InBornBuff")) {
                    var buffs = pair.Value.Split('|');
                    for (int i = 0; i < buffs.Length; i++) {
                        string[] s = buffs[i].Split('=');
                        if (s.Length < 2)
                            continue;
                        var ele = new ResourceHyperlink(this, NodeType.Buff, key, buffs[i], table.Comment[key], $"{s[0]}={s[1]}").Root;
                        Container.Add(ele);
                    }
                } else if (key.Equals("SwitchBuff")) {
                    var buffs = pair.Value.Split('|');
                    for (int i = 0; i < buffs.Length; i++) {
                        string[] s = buffs[i].Split('=');
                        if (s.Length < 3)
                            continue;
                        var ele = new ResourceHyperlink(this, NodeType.Buff, key, buffs[i], table.Comment[key], $"{s[1]}={s[2]}").Root;
                        Container.Add(ele);
                    }
                } else if (pair.Value != "") {
                    InspectorLabel label = new InspectorLabel(this, key, pair.Value, table.Comment[pair.Key]);
                    Container.Add(label.Root);
                }
            }
        }

        protected override GenericMenu ContextClick(ContextClickEvent evt) {
            var menu = base.ContextClick(evt);
            menu.AddItem(new GUIContent("用Excel打开"), false, () => {
                OpenExcel.OpenOffice($@"{Application.dataPath}\Table".Replace('/', '\\'), "PartnerInfo.txt", "D3", currentRow);
            });
            menu.AddItem(new GUIContent("用WPS打开"), false, () => {
                OpenExcel.OpenWps($@"{Application.dataPath}\Table".Replace('/', '\\'), "PartnerInfo.txt", "D3", currentRow);
            });
            return menu;
        }
    }
}
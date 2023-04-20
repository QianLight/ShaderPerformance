using CFClient;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {
    public class EnemyWidget : TableWidget {
        public EnemyWidget(ResourceReferenceInspector owner, Vector2 position, string entityID) : base(owner, position, entityID) {
            InspectorNodeType = NodeType.Enemy;
        }

        protected override string TableName => "XEntityStatistics";
        protected override string TableID => "ID";


        protected override void BuildEditContent(TableNode table, Dictionary<string, string> row) {
            TitleLabel.text = $"Entity: {ID} {row["Note"]}";
            _editors = new Dictionary<string, BaseInspectorField>();

            foreach (var pair in row) {
                string key = pair.Key;
                if (!(Match(key) || Match(pair.Value)))
                    continue;

                if (key.Equals("InBornBuff")) {
                    InspectorTextfield MakeBuffInput(string value) {
                        return new InspectorTextfield(this, key, value, table.Comment[pair.Key], VerifyBuffID, HintBuffID); 
                    }
                    var ele = new InspectorMultifield<InspectorTextfield>('|', key, pair.Value, MakeBuffInput);
                    _editors[key] = ele;
                    Container.Add(ele.Root);
                } else if (key.Equals("EnableHeadBar")) {
                    var ele = new InspectorPopup(this, key, pair.Value, new List<string> { "不填", "1. 红色", "2. 蓝色" }, new List<string> { "", "1", "2" }, table.Comment[pair.Key]);
                    _editors[key] = ele;
                    Container.Add(ele.Root);
                } else if (key.Equals("Type")) {
                    List<string> options = new List<string>() {
                        "1.boss",
                        "2.opposer",
                        "3.puppet",
                        "4.switch",
                        "5.substance",
                        "6.elite",
                        "7. npc,隐形怪,交互怪",
                        "11,战斗召唤物",
                        "14.doodad掉落物",
                        "15.战场计数怪",
                        "18.表现召唤物",
                        "19.可移动平台型"
                    };
                    List<string> values = new List<string>() {"1", "2", "3", "4", "5", "6", "7","11", "14","15","18","19"};
                    var ele = new InspectorPopup(this, key, pair.Value, options, values, table.Comment[pair.Key]);
                    _editors[key] = ele;
                    Container.Add(ele.Root);
                } else if (key.Equals("AIID")) {
                    InspectorTextfield ele = new InspectorTextfield(this, key, pair.Value, table.Comment[pair.Key], VerifyAIID, HintAIID);
                    _editors[key] = ele;
                    Container.Add(ele.Root);
                } else if (key.Equals("PresentID")) {
                    InspectorTextfield ele = new InspectorTextfield(this, key, pair.Value, table.Comment[pair.Key], VerifyPresentID, HintPresentID);
                    _editors[key] = ele;
                    Container.Add(ele.Root);
                } else if (key.Equals("ShowArrow") || key.Equals("AlwaysHpHide") || key.Equals("HideName") || key.Equals("UsingGeneralCutscene") || key.Equals("SoloShow") || key.Equals("EndShow") || key.Equals("Block") || key.Equals("IsWander") || key.Equals("IsFixedInCD") || key.Equals("HideInMiniMap") || key.Equals("BlockAlways")) {
                    var ele = new InspectorPopup(this, key, pair.Value, new List<string> { "不填", "TRUE", "FALSE" }, new List<string> { "", "TRUE", "FALSE" }, table.Comment[pair.Key]);
                    _editors[key] = ele;
                    Container.Add(ele.Root);
                } else if (key.Equals("CallerAttrList")) {
                    InspectorTextfield MakeAttrInput(string value) {
                        return new InspectorTextfield(this, key, value, table.Comment[pair.Key], VerifyAttribute, HintAttribute);
                    }
                    var ele = new InspectorMultifield<InspectorTextfield>('|', key, pair.Value, MakeAttrInput);
                    _editors[key] = ele;
                    Container.Add(ele.Root);
                } else {
                    var textField = new InspectorTextfield(this, key, pair.Value, table.Comment[pair.Key]);
                    _editors[key] = textField;
                    Container.Add(textField.Root);
                }
            }
        }

        protected override void BuildTableContent(TableNode table, Dictionary<string, string> row) {
            TitleLabel.text = $"Entity: {ID} {row["Note"]}";

            var gmButton = new GMButton(GMCommandType.Server, "ID", $"mob {ID}");
            Container.Add(gmButton.Root);

            TableNode enemyStageTable = TableDatabase.Instance.GetTable("EnemyStage", "StaticsID");
            if (enemyStageTable.Dic.ContainsKey(ID)) {
                var ele = new ResourceHyperlink(this, NodeType.EmemyStage, "EnemyStage", ID, "boss阶段").Root;
                Container.Add(ele);
            }

            foreach (var pair in row) {
                string key = pair.Key;
                if (!(Match(key) || Match(pair.Value)))
                    continue;
                if (pair.Value == "" || pair.Key.Equals("ID") || pair.Key.Equals("Note"))
                    continue;

                if (key.Equals("CallerAttrList")) {
                    string[] s = pair.Value.Split('|');
                    for (int i = 0; i < s.Length; i++) {
                        InspectorLabel label = new InspectorLabel(this, key, s[i], table.Comment[pair.Key]);
                        Container.Add(label.Root);
                    }
                } else if (key.Equals("PresentID")) {
                    var ele = new ResourceHyperlink(this, NodeType.XEntityPresentation, key, pair.Value, table.Comment[key]).Root;
                    Container.Add(ele);
                } else if (key.Equals("AIID")) {
                    var ele = new ResourceHyperlink(this, NodeType.AI, key, pair.Value, table.Comment[key], pair.Value, row["ID"]).Root;
                    Container.Add(ele);
                } else if (key.Equals("InBornBuff")) {
                    var buffs = pair.Value.Split('|');
                    for (int i = 0; i < buffs.Length; i++) {
                        string[] s = buffs[i].Split('=');
                        if (s.Length < 2)
                            continue;
                        var ele = new ResourceHyperlink(this, NodeType.Buff, key, buffs[i], table.Comment[key], $"{s[0]}={s[1]}").Root;
                        Container.Add(ele);
                    }
                } else if (pair.Value != "") {
                    InspectorLabel label = new InspectorLabel(this, key, pair.Value, table.Comment[pair.Key]);
                    Container.Add(label.Root);
                }
            }
        }

        protected async override void FindAllReference() {
            var r = new ReferenceRecord(NodeType.Enemy, ID);
            await ScanReference(r);
        }

        protected override GenericMenu ContextClick(ContextClickEvent evt) {
            var menu = base.ContextClick(evt);
            menu.AddItem(new GUIContent("用Excel打开"), false, () => {
                OpenExcel.OpenOffice($@"{Application.dataPath}\Table".Replace('/', '\\'), "XEntityStatistics.txt", "I3", currentRow);
            });
            menu.AddItem(new GUIContent("用WPS打开"), false, () => {
                OpenExcel.OpenWps($@"{Application.dataPath}\Table".Replace('/', '\\'), "XEntityStatistics.txt", "I3", currentRow);
            });
            return menu;
        }

        string VerifyPresentID(string value) {
            var table = TableDatabase.Instance.GetTable("XEntityPresentation", "PresentID");
            if (!table.Dic.ContainsKey(value))
                return "PresentID不存在";
            return "";
        }

        List<string> HintPresentID(string value) {
            List<string> results = new List<string>();
            var table = TableDatabase.Instance.GetTable("XEntityPresentation", "PresentID");

            for (int i = 0; i < table.Table.Count; i++) {
                if (table.Table[i]["PresentID"].Contains(value)) {
                    results.Add($"{table.Table[i]["PresentID"]} {table.Table[i]["Name"]}");
                    if (results.Count > 50)
                        return results;
                }
            }

            return results;
        }

        string VerifyAttribute(string value) {
            string[] s = value.Split('=');
            if (s.Length < 2)
                return "格式错误";

            var table = TableDatabase.Instance.GetTable("AttrDefine", "AttrID");
            if (!table.Dic.ContainsKey(s[0]))
                return "AttrID不存在";

            try {
                int t = int.Parse(s[1]);
            } catch {
                return "格式错误";
            }
            return "";
        }

        List<string> HintAttribute(string value) {
            List<string> results = new List<string>();
            var table = TableDatabase.Instance.GetTable("AttrDefine", "AttrID");
            string[] s = value.Split('=');
            for (int i = 0; i < table.Table.Count; i++) {
                if (table.Table[i]["AttrID"].Contains(s[0])) {
                    results.Add($"{table.Table[i]["AttrID"]} {table.Table[i]["Name"]} {table.Table[i]["Comment"]}");
                    if (results.Count > 50)
                        return results;
                }
            }

            return results;
        }

        string VerifyAIID(string value) {
            var table = TableDatabase.Instance.GetTable("UnitAITable", "ID");
            if (!table.Dic.ContainsKey(value))
                return "AIID不存在";
            return "";
        }

        List<string> HintAIID(string value) {
            List<string> results = new List<string>();
            var table = TableDatabase.Instance.GetTable("UnitAITable", "ID");

            for (int i = 0; i < table.Table.Count; i++) {
                if (table.Table[i]["ID"].Contains(value)) {
                    results.Add($"{table.Table[i]["ID"]} {table.Table[i]["Comment"]}");
                    if (results.Count > 50)
                        return results;
                }
            }

            return results;
        }

        string VerifyBuffID(string value) {
            var table = TableDatabase.Instance.GetTable("BuffList", "BuffID|BuffLevel");
            if (!table.Dic.ContainsKey(value))
                return "BuffID=BuffLevel不存在";
            return "";
        }

        List<string> HintBuffID(string value) {
            List<string> results = new List<string>();
            var table = TableDatabase.Instance.GetTable("BuffList", "BuffID|BuffLevel");

            for (int i = 0; i < table.Table.Count; i++) {
                string s = $"{table.Table[i]["BuffID"]}={ table.Table[i]["BuffLevel"]}";
                if (s.Contains(value)) {
                    results.Add($"{table.Table[i]["BuffID"]}={table.Table[i]["BuffLevel"]} {table.Table[i]["BuffName"]}");
                    if (results.Count > 50)
                        return results;
                }
            }

            return results;
        }

    }
}
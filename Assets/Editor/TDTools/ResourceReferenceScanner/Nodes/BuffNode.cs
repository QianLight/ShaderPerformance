using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {

    [System.Serializable]
    public class BuffNode : InspectorElement {

        public BuffNode(ResourceReferenceInspector owner, Vector2 position, string buffIDLevel) : base(owner, position, buffIDLevel) {
            InspectorNodeType = NodeType.Buff;
        }

        public override void BuildContent() {
            base.BuildContent();
            var table = TableDatabase.Instance.GetTable("BuffList", "BuffID|BuffLevel", "{BuffID}={BuffLevel}\t{BuffName}");
            string _buffIDLevel = ID;
            string[] ss = ID.Split('=');
            if (ss[1].Equals("-1"))
                _buffIDLevel = $"{ss[0]}=1";
            var row = table.Table[table.Dic[_buffIDLevel]];
            TitleLabel.text = $"Buff: {row["BuffID"]}={row["BuffLevel"]}    {row["BuffName"]}";
            Label labelRow = new Label($"第{table.Dic[_buffIDLevel] + 3}行");
            currentRow = (table.Dic[_buffIDLevel] + 3).ToString();
            Container.Add(labelRow);

            var gmButton = new GMButton(GMCommandType.Server, "GM指令", $"buff {row["BuffID"]} {row["BuffLevel"]}");
            Container.Add(gmButton.Root);

            foreach (var pair in row) {
                string key = pair.Key;
                if (!(Match(key) || Match(pair.Value)))
                    continue;
                if (pair.Value == "" || key.Equals("BuffID") || key.Equals("BuffLevel") || key.Equals("BuffName"))
                    continue;

                //if (key.Equals("ChangeAnim")) {
                //    string[] s = pair.Value.Split('=');
                //    for (int i = 0; i < s.Length; i++) {
                //        if (s[i].Equals("Origin"))
                //            continue;
                //        var ele = new ResourceHyperlink(this, NodeType.Asset, key, pair.Value, table.Comment[key], $"BundleRes/{s[0]}.prefab").Root;
                //        Container.Add(ele);
                //    }
                //} else 
                if (key.Equals("Transform")) {
                    string[] s = pair.Value.Split('=');
                    var ele = new ResourceHyperlink(this, NodeType.Partner, key, pair.Value, table.Comment[key], s[0]).Root;
                    Container.Add(ele);
                } else if (key.Equals("BuffEffectFx")) {
                    string[] s = pair.Value.Split('='); 
                    var ele = new ResourceHyperlink(this, NodeType.Asset, key, pair.Value, table.Comment[key], $"BundleRes/{s[0]}.prefab").Root;
                    Container.Add(ele);
                } else if (key.Equals("RelevantSkills")) {
                    string[] s = pair.Value.Split('|');
                    for (int i = 0; i < s.Length; i++) {
                        var ele = new ResourceHyperlink(this, NodeType.PartnerSkill, key, s[i], table.Comment[key]).Root;
                        Container.Add(ele);
                    }
                } else if (key.Equals("BuffIcon")) {
                    string[] s = pair.Value.Split('=');
                    var ele = new ResourceHyperlink(this, NodeType.Texture2D, key, pair.Value, table.Comment[key], $@"BundleRes/UI/UISource/ui_bufficon/{s[0]}.png").Root;
                    Container.Add(ele);
                } else if (key.Equals("BuffTriggerBuff")) {
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

        protected override GenericMenu ContextClick(ContextClickEvent evt) {
            var menu = base.ContextClick(evt);
            menu.AddItem(new GUIContent("用Excel打开BuffList表"), false, () => {
                OpenExcel.OpenOffice($@"{Application.dataPath}\Table".Replace('/', '\\'), "BuffList.txt", "E3", currentRow);
            });
            menu.AddItem(new GUIContent("用WPS打开BuffList表"), false, () => {
                OpenExcel.OpenWps($@"{Application.dataPath}\Table".Replace('/', '\\'), "BuffList.txt", "E3", currentRow);
            });

            return menu;
        }

        protected async override void FindAllReference() {
            var key = new ReferenceRecord(NodeType.Buff, ID);
            await ScanReference(key);
        }
    }

    public partial class ResourceReferenceScanner{
        ScannerResult ScanBuff(string IDLevel, ReferenceRecord from, string presentID = "", bool suppressedLog = false) {
            ScannerResult results = new ScannerResult();
            if (IDLevel.Contains("|")) {
                string[] ss = IDLevel.Split('|');
                for (int i = 0; i < ss.Length; i++)
                    results.AddRange(ScanBuff(ss[i], from, presentID));
                return results;
            }

            if (IDLevel.Equals("0") || IDLevel == "")
                return results;

            string[] s = IDLevel.Split('=');
            if (s.Length < 2)
                return results;

            if (s[1].Equals("-1")) {
                for (int i = 0; i < 9; i++) {
                    results.AddRange(ScanBuff($"{s[0]}={i}", from, presentID, true));
                }
                return results;
            }

            string id = $"{s[0]}={s[1]}";
            //string id = IDLevel;


            if (s[0].Equals("0") || s[1].Equals("0"))
                return results;

            if (!_buffTable.Dic.ContainsKey(id)) {
                if (!suppressedLog) {
                    results.MissingFiles.Add($"BuffID: {id} 不存在");
                    //Debug.Log($"找不到BuffID: {id}");
                }
                return results;
            }

            var recordKey = new ReferenceRecord(NodeType.Buff, id);
            AddReference(recordKey, from);

            if (_buffTable.Set.Contains(id))
                return results;
            _buffTable.Set.Add(id);

            Dictionary<string, string> row = _buffTable.Table[_buffTable.Dic[id]];

            //Debug.Log($"扫描Buff: {id}{row["BuffName"]}");

            if (row["BuffEffectFx"] != "") {
                string[] fxs = row["BuffEffectFx"].Split('=');
                string FXPath = $@"{DATAPATH}\BundleRes\{fxs[0]}.prefab".Replace('/', '\\').ToLower();
                results.FxFiles.Add(FXPath);
            }

            //if (row["BuffIcon"] != "")
            //Debug.Log(row["BuffIcon"]);

            if (row["Mob"] != "")
                results.AddRange(ScanMonster(row["Mob"].Split('=')[0], new ReferenceRecord(NodeType.Buff, id)));

            if (row["ChangeAnim"] != "" && presentID != "" && _presentationTable.Dic.ContainsKey(presentID)) {
                var pRow = _presentationTable.Table[_presentationTable.Dic[presentID]];
                string[] anims = row["ChangeAnim"].Split('=');
                for (int i = 0; i < anims.Length; i++) {
                    if (!anims.Equals("Origin")) {
                        results.animationFiles.Add($@"{DATAPATH}\BundleRes\Animation\{pRow["AnimLocation"]}{anims[i]}.anim".Replace('/', '\\').ToLower());
                    }
                }
            }

            if (row["Command"] != "") {
                string[] commands = row["Command"].Split('|');
                for (int i = 0; i < commands.Length; i++) {
                    if (commands[i].Contains("castskill")) {
                        string skillName = commands[i].Substring(commands[i].IndexOf(" ") + 1);
                        results.AddRange(ScanSkill(skillName, presentID));
                    }
                }
            }

            if (row["Transform"] != "") {
                string transformID = row["Transform"].Split('=')[0];

                if (_roleTable.Dic.ContainsKey(transformID)) {

                } else {
                    results.AddRange(ScanMonster(transformID, new ReferenceRecord(NodeType.Buff, id)));
                    //if (_monsterTable.Dic.ContainsKey(transformID))
                    //    Debug.LogWarning(_monsterTable.Table[_monsterTable.Dic[transformID]]["Note"]);
                }
            }

            return results;
        }
    }
}
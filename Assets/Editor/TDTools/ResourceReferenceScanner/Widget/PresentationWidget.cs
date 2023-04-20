using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {
    public class PresentationWidget : InspectorElement {

        public PresentationWidget(ResourceReferenceInspector owner, Vector2 position, string presentID) : base(owner, position, presentID) {
            InspectorNodeType = NodeType.XEntityPresentation;
        }

        public override void BuildContent() {
            base.BuildContent();
            var table = TableDatabase.Instance.GetTable("XEntityPresentation", "PresentID");
            var row = table.Table[table.Dic[ID]];
            TitleLabel.text = $"Present: {row["PresentID"]} {row["Name"]}";
            Label labelRow = new Label($"第{table.Dic[ID] + 3}行");
            Container.Add(labelRow);
            List<string> animations = new List<string> { "Idle", "AttackIdle", "FishingIdle", "Walk", "AttackWalk", "Run", "AttackRun", "Brake", "RunLeft", "RunRight", "AttackRunRight", "Sheath", "AvatarPos" };
            foreach (var pair in row) {
                string key = pair.Key;
                if (!(Match(key) || Match(pair.Value)))
                    continue;
                if (pair.Value == "" || key.Equals("PresentID") || key.Equals("Name"))
                    continue;

                if (key.Equals("MoveFx")) {
                    var ele = new ResourceHyperlink(this, NodeType.Asset, key, pair.Value, table.Comment[key], $"BundleRes/{pair.Value}.prefab").Root;
                    Container.Add(ele);
                } else if (key.Equals("OtherSkills")) {
                    var skills = pair.Value.Split('|', '=');
                    for (int i = 0; i < skills.Length; i++) {
                        var ele = new ResourceHyperlink(this, NodeType.XEntitySkill, key, skills[i], table.Comment[key]).Root;
                        Container.Add(ele);
                    }
                } else if (animations.Contains(key)) {
                    string animationPath = $"BundleRes/Animation/{row["AnimLocation"]}{pair.Value}.anim";
                    string prefabPath = $"BundleRes/Prefabs/{row["Prefab"]}.prefab";
                    var ele = new ResourceHyperlink(this, NodeType.Animation, key, pair.Value, table.Comment[key], $"{animationPath}={prefabPath}").Root;
                    Container.Add(ele);
                } else if (key.Equals("Prefab") || key.Equals("PrefabShow")) {
                    var ele = new ResourceHyperlink(this, NodeType.Asset, key, pair.Value, table.Comment[key], $"BundleRes/Prefabs/{pair.Value}.prefab").Root;
                    Container.Add(ele);
                } else if (key.Equals("CurveLocation")) {
                    var ele = new ResourceHyperlink(this, NodeType.Folder, key, pair.Value, table.Comment[key], $"{Application.dataPath}/BundleRes/Curve/{row["CurveLocation"]}".Replace("/", "\\")).Root;
                    Container.Add(ele);
                } else if (key.Equals("AnimLocation")) {
                    var ele = new ResourceHyperlink(this, NodeType.Folder, key, pair.Value, table.Comment[key], $"{Application.dataPath}/BundleRes/Animation/{row["AnimLocation"]}".Replace("/", "\\")).Root;
                    Container.Add(ele);
                } else if (key.Equals("SkillLocation")) {
                    var ele = new ResourceHyperlink(this, NodeType.Folder, key, pair.Value, table.Comment[key], $"{Application.dataPath}/BundleRes/SkillPackage/{row["SkillLocation"]}".Replace("/", "\\")).Root;
                    Container.Add(ele);
                } else if (key.Equals("BehitLocation")) {
                    var ele = new ResourceHyperlink(this, NodeType.Folder, key, pair.Value, table.Comment[key], $"{Application.dataPath}/BundleRes/HitPackage/{row["BehitLocation"]}".Replace("/", "\\")).Root;
                    Container.Add(ele);
                } else if (key.Equals("BeHit")) {
                    var behits = pair.Value.Split('|');
                    for (int i = 0; i < behits.Length; i++) {
                        string[] s = behits[i].Split('=');
                        if (s.Length < 2)
                            continue;
                        var ele = new ResourceHyperlink(this, NodeType.BeHit, key, behits[i], table.Comment[key], $"BundleRes/HitPackage/{row["BehitLocation"]}{s[1]}").Root;
                        Container.Add(ele);
                    }
                } else {
                    InspectorLabel label = new InspectorLabel(this, pair.Key, pair.Value, table.Comment[pair.Key]);
                    Container.Add(label.Root);
                }

            }
        }

        protected override GenericMenu ContextClick(ContextClickEvent evt) {
            var menu = base.ContextClick(evt);
            menu.AddItem(new GUIContent("用Excel打开"), false, () => {
                OpenExcel.OpenOffice($@"{Application.dataPath}\Table".Replace('/', '\\'), "XEntityPresentation.txt", "E3", currentRow);
            });
            menu.AddItem(new GUIContent("用WPS打开"), false, () => {
                OpenExcel.OpenWps($@"{Application.dataPath}\Table".Replace('/', '\\'), "XEntityPresentation.txt", "E3", currentRow);
            });
            return menu;
        }
    }
}
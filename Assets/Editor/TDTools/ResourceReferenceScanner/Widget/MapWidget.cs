using CFClient;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {
    public class MapWidget : InspectorElement {

        public MapWidget(ResourceReferenceInspector owner, Vector2 position, string presentID) : base(owner, position, presentID) {
            InspectorNodeType = NodeType.Map;
        }

        public override void BuildContent() {
            base.BuildContent();
            var table = TableDatabase.Instance.GetTable("MapList", "MapID");
            var row = table.Table[table.Dic[ID]];
            TitleLabel.text = $"Map: {row["MapID"]} {row["Comment"]}";
            Label labelRow = new Label($"第{table.Dic[ID] + 3}行");
            Container.Add(labelRow);

            var gmButton = new GMButton(GMCommandType.Client, "MapID", $"mm {ID}");
            Container.Add(gmButton.Root);

            foreach (var pair in row) {
                string key = pair.Key;
                if (!(Match(key) || Match(pair.Value)))
                    continue;
                if (pair.Value == "" || key.Equals("MapID") || key.Equals("Comment"))
                    continue;

                //if (key.Equals("AIID")) {
                //    var ele = new ResourceHyperlink(this, NodeType.AI, key, row[key], table.Comment[key]).Root;
                //    Container.Add(ele);
                //} else 
                if (key.Equals("Buff")) {
                    var buffs = pair.Value.Split('|');
                    for (int i = 0; i < buffs.Length; i++) {
                        string[] s = buffs[i].Split('=');
                        if (s.Length < 2)
                            continue;
                        var ele = new ResourceHyperlink(this, NodeType.Buff, key, buffs[i], table.Comment[key], $"{s[0]}={s[1]}").Root;
                        Container.Add(ele);
                    }
                } else if (key.Equals("BGM") || key.Equals("EnvBGM")) {
                    var ele = new ResourceHyperlink(this, NodeType.FMOD, key, pair.Value, table.Comment[key], $"event:/{pair.Value}").Root;
                    Container.Add(ele);
                } else if (key.Equals("LoadingPic")) {
                    var ele = new ResourceHyperlink(this, NodeType.Texture2D, key, pair.Value, table.Comment[key], $"BundleRes/{pair.Value}.png").Root;
                    Container.Add(ele);
                } else if (key.Equals("WayPointFile")) {
                    var ele = new ResourceHyperlink(this, NodeType.Asset, key, pair.Value, table.Comment[key], $"BundleRes/Table/WayPoint/{pair.Value}.xml").Root;
                    Container.Add(ele);
                } else if (key.Equals("LevelConfigFile")) {
                    var ele = new ResourceHyperlink(this, NodeType.Asset, key, pair.Value, table.Comment[key], $"BundleRes/Table/{pair.Value}.cfg").Root;
                    Container.Add(ele);
                } else if (key.Equals("ScenePath")) {
                    var ele = new ResourceHyperlink(this, NodeType.Folder, key, pair.Value, table.Comment[key], $"{Application.dataPath}/{row["ScenePath"]}/".Replace("/", "\\")).Root;
                    Container.Add(ele);
                } else if (key.Equals("BlockFilePath")) {
                    var ele = new ResourceHyperlink(this, NodeType.Asset, key, pair.Value, table.Comment[key], $"BundleRes/{pair.Value}.mapheight").Root;
                    Container.Add(ele);
                } else if(key.Equals("UnitySceneFile")) {
                    var ele = new ResourceHyperlink(this, NodeType.Asset, key, pair.Value, table.Comment[key], $"{row["ScenePath"]}/{pair.Value}.unity").Root;
                    Container.Add(ele);
                } else {
                    InspectorLabel label = new InspectorLabel(this, pair.Key, pair.Value, table.Comment[pair.Key]);
                    Container.Add(label.Root);
                }

            }
        }

        protected override GenericMenu ContextClick(ContextClickEvent evt) {
            var menu = base.ContextClick(evt);
            menu.AddItem(new GUIContent("用Excel打开"), false, () => {
                OpenExcel.OpenOffice($@"{Application.dataPath}\Table".Replace('/', '\\'), "MapList.txt", "C3", currentRow);
            });
            menu.AddItem(new GUIContent("用WPS打开"), false, () => {
                OpenExcel.OpenWps($@"{Application.dataPath}\Table".Replace('/', '\\'), "MapList.txt", "C3", currentRow);
            });
            return menu;
        }
    }
}
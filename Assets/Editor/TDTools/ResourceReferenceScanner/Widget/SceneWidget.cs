using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {
    public class SceneWidget : InspectorElement {

        public SceneWidget(ResourceReferenceInspector owner, Vector2 position, string presentID) : base(owner, position, presentID) {
            InspectorNodeType = NodeType.Scene;
        }

        public override void BuildContent() {
            base.BuildContent();
            var table = TableDatabase.Instance.GetTable("SceneList", "SceneID");
            var row = table.Table[table.Dic[ID]];
            TitleLabel.text = $"Scene: {row["SceneID"]} {row["SceneTitle"]}";
            Label labelRow = new Label($"第{table.Dic[ID] + 3}行");
            Container.Add(labelRow);
            foreach (var pair in row) {
                string key = pair.Key;
                if (!(Match(key) || Match(pair.Value)))
                    continue;
                if (pair.Value == "" || key.Equals("SceneID") || key.Equals("SceneTitle"))
                    continue;

                if (key.Equals("LoadingPic") || key.Equals("BackLoadingPic")) {
                    var ele = new ResourceHyperlink(this, NodeType.Texture2D, key, pair.Value, table.Comment[key], $"BundleRes/{pair.Value}.png").Root;
                    Container.Add(ele);
                } else if (key.Equals("BGM")) {
                    var ele = new ResourceHyperlink(this, NodeType.FMOD, key, pair.Value, table.Comment[key], $"event:/{pair.Value}").Root;
                    Container.Add(ele);
                } else if (key.Equals("MapID")) {
                    var ele = new ResourceHyperlink(this, NodeType.Map, key, pair.Value, table.Comment[key]).Root;
                    Container.Add(ele);
                    var gmButton = new GMButton(GMCommandType.Client, "MapID", $"mm {pair.Value}");
                    Container.Add(gmButton.Root);
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
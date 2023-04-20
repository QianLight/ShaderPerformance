using CFClient;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {
    public class UnitAITaableWidget : InspectorElement
    {
        bool _showSkillButton = false;
        static List<string> skillCols = new List<string>(){ "MainSkillName", "LeftSkillName", "RightSkillName", "BackSkillName", "CheckingSkillName", "CheckingSkillAndStopName",
        "DashSkillName", "FarSkillName", "SelectSkillName", "UnusedSkillName", "TurnSkillName", "MoveSkillName"};

        public UnitAITaableWidget(ResourceReferenceInspector owner, Vector2 position, string AIID, string linkedID = "") : base(owner, position, AIID, linkedID)
        {
            InspectorNodeType = NodeType.AI;
        }

        public override void BuildContent()
        {
            base.BuildContent();
            var table = TableDatabase.Instance.GetTable("UnitAITable", "ID");
            currentRow = table.Dic[ID].ToString();
            var row = table.Table[table.Dic[ID]];
            if (LinkedID == "")
                TitleLabel.text = $"AI: {ID} {row["Comment"]}";
            else
                TitleLabel.text = $"AI: {ID} {row["Comment"]} StatisticsID:{LinkedID}";
            Label labelRow = new Label($"第{table.Dic[ID] + 3}行");

            if (LinkedID != "")
            {
                Toggle toggle = new Toggle();
                toggle.text = "显示GM按钮";
                toggle.value = _showSkillButton;
                toggle.RegisterValueChangedCallback(obj =>
                {
                    _showSkillButton = obj.newValue;
                    BuildContent();
                });
                Container.Add(toggle);

                if (_showSkillButton)
                {
                    var gmButton = new GMButton(GMCommandType.Server, "链接的怪物GM指令", $"mob {LinkedID}");
                    Container.Add(gmButton.Root);
                }
            }

            Container.Add(labelRow);

            foreach (var pair in row)
            {
                string key = pair.Key;
                if (!(Match(key) || Match(pair.Value)))
                    continue;

                if (pair.Value == "" || key.Equals("ID") || key.Equals("Comment"))
                    continue;

                if (key.Equals("Tree") || key.Equals("Combat_SubTree") || key.Equals("Combat_PreCombatSubTree"))
                {
                    var ele = new ResourceHyperlink(this, NodeType.BehaviourTree, key, pair.Value, table.Comment[key]).Root;
                    Container.Add(ele);
                }
                else if (key.Equals("Events") || key.Equals("CustomVariables"))
                {
                    var events = pair.Value.Split('|');
                    for (int i = 0; i < events.Length; i++)
                    {
                        var ele = new InspectorLabel(this, key, events[i], table.Comment[key]).Root;
                        Container.Add(ele);
                    }
                }
                else if (skillCols.Contains(key))
                {
                    var skills = pair.Value.Split('|', '=');
                    for (int i = 0; i < skills.Length; i++)
                    {
                        if (_showSkillButton && LinkedID != "")
                        {
                            var gmButton = new GMButton(GMCommandType.Server, key, $"monstercastskill {LinkedID} {skills[i]}");
                            Container.Add(gmButton.Root);
                        }
                        else
                        {
                            var ele = new ResourceHyperlink(this, NodeType.XEntitySkill, key, skills[i], table.Comment[key], skills[i], LinkedID).Root;
                            Container.Add(ele);
                        }
                    }
                }
                else if (pair.Value != "")
                {
                    InspectorLabel label = new InspectorLabel(this, key, pair.Value, table.Comment[pair.Key]);
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
            }
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("用Excel打开表"), false, () => {
                OpenExcel.OpenOffice($@"{Application.dataPath}\Table".Replace('/', '\\'), "UnitAITable.txt", "C3", currentRow);
            });
            menu.AddItem(new GUIContent("用WPS打开表"), false, () => {
                OpenExcel.OpenWps($@"{Application.dataPath}\Table".Replace('/', '\\'), "UnitAITable.txt", "C3", currentRow);
            });
#endif
            return menu;
        }
    }

}
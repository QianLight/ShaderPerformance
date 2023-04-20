//using System.Collections;
//using System.Collections.Generic;
//using UnityEditor;
//using UnityEngine;
//using UnityEngine.UIElements;

//namespace TDTools.SkillTreeSimulator {
//    public class SkillVE {

//        SkillTreeSimulator owner;

//        public int initialLevel;
//        public int Level;
//        public int MaxLevel;

//        public int layer;

//        public string PreReq;

//        public int LevelUpAlreadyCost;

//        //public Image Icon;

//        //public Label skillLevel;

//        public Texture Icon;

//        public float x, y;

//        Dictionary<string, string> row;

//        public string Tooltip;

//        public string SkillLabel;

//        public void AddLevel(bool history = true, bool ignoreLimits = false) {
//            if (ignoreLimits || Level < MaxLevel && owner.PointSpent >=  LevelUpAlreadyCost && (PreReq == "" || owner._skills[PreReq].Level == owner._skills[PreReq].MaxLevel)) {
//                Level++;
//                owner.PointSpent++;
//                //skillLevel.text = Level + "/" + MaxLevel;
//                if (history) {
//                    owner.History.Add("+ " + row["SkillScript"]);
//                    owner.veList.Refresh();
//                }
//                if (!ignoreLimits)
//                    owner.ChangePointSpent();
//            }
//        }

//        public void RemoveLevel(bool history = true) {
//            if (Level <= initialLevel)
//                return;
//            Level--;
//            owner.PointSpent--;
//            //skillLevel.text = Level + "/" + MaxLevel;
//            owner.ChangePointSpent();
//            if (history) {
//                owner.History.Add("- " + row["SkillScript"]);
//                owner.veList.Refresh();
//            }
//        }

//        public SkillVE(SkillTreeSimulator owner, Dictionary<string, string> row, VisualElement ve) {
//            this.owner = owner;
//            MaxLevel = int.Parse(row["SkillTotalLevel"]);
//            PreReq = row["PreSkill"];
//            if (row["InitLevel"] != "") {
//                Level = int.Parse(row["InitLevel"]);
//                initialLevel = Level;
//            }

//            this.row = row;

//            //VisualElement skillVE = new VisualElement();
//            //skillVE.style.position = Position.Absolute;
//            //skillVE.style.height = 128;
//            //skillVE.style.width = 64;
//            //skillVE.style.flexShrink = 0;
//            if (row["LevelupAlreadyCost"] != "")
//                LevelUpAlreadyCost = int.Parse(row["LevelupAlreadyCost"]);
//            else
//                LevelUpAlreadyCost = 0;
//            var layerString = row["SkillLayer"].Split('|');
//            layer = int.Parse(layerString[0]);

//            float p = 90f - float.Parse(layerString[1]) * 360f / 12f;
//            //skillVE.style.top = -(layer + 1) * 128 * Mathf.Sin(p * Mathf.Deg2Rad) - 48;
//            //skillVE.style.right = -(layer + 1) * 128 * Mathf.Cos(p * Mathf.Deg2Rad) + 96;

//            x = -(layer + 1) * 128 * Mathf.Cos(p * Mathf.Deg2Rad);
//            y = -(layer + 1) * 128 * Mathf.Sin(p * Mathf.Deg2Rad) - 48;

//            //Icon = new Image();

//            //Icon.image = AssetDatabase.LoadAssetAtPath<Texture>("Assets/BundleRes/UI/UISource/ui_skill/" + row["Atlas"] + "/" + row["Icon"] + ".png");
//            Icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/BundleRes/UI/UISource/ui_skill/" + row["Atlas"] + "/" + row["Icon"] + ".png");
//            //Icon.tooltip = $"√Ë ˆ\n{row["CurrentLevelDescription"]}\n\n\nPVP√Ë ˆ\n{row["CurrentLevelDescriptionPVP"]}".Replace("&c3&", "<color=green>").Replace("&c0&", "</color>").Replace("&c1&", "<color=blue>").Replace("&c2&", "<color=red>").Replace("\\n", "\n");
//            Tooltip = $"√Ë ˆ\n{row["CurrentLevelDescription"]}\n\n\nPVP√Ë ˆ\n{row["CurrentLevelDescriptionPVP"]}".Replace("&c3&", "<color=green>").Replace("&c0&", "</color>").Replace("&c1&", "<color=blue>").Replace("&c2&", "<color=red>").Replace("\\n", "\n");
//            //Icon.style.height = 64;
//            //Icon.style.width = 64;

//            //Icon.RegisterCallback<TooltipEvent>(obj =>{
//            //    Debug.Log(tooltip);
//            //    VisualElement root = (VisualElement)obj.target;
//            //    var tooltipRect = new Rect(root.worldBound);
//            //    tooltipRect.x += 10;
//            //    tooltipRect.y += 10;

//            //    VisualElement element = new VisualElement();
//            //    element.style.position = Position.Absolute;
//            //    element.pickingMode = PickingMode.Ignore;
//            //    root.Add(element);

//            //    Label label = new Label();
//            //    label.text = tooltip;
//            //    element.Add(label);
//            //});

//            //Image skillTierIcon = new Image();
//            //skillTierIcon.image = owner.SkillTierIcon[layer];
//            //skillTierIcon.style.position = Position.Absolute;
//            //skillTierIcon.style.height = 16;
//            //skillTierIcon.style.width = 16;
//            //skillTierIcon.style.top = 0;
//            //skillTierIcon.style.left = 0;
//            //Icon.Add(skillTierIcon);

//            //if (LevelUpAlreadyCost > owner.PointSpent)
//            //    Icon.tintColor = Color.gray;

//            //skillVE.Add(Icon);
//            SkillLabel = row["ScriptName"];
//            //Label skillName = new Label();
//            //skillName.text = row["ScriptName"] + '\n' + row["SkillScript"];
//            //skillName.style.unityTextAlign = TextAnchor.MiddleCenter;
//            //skillName.style.fontSize = 9;
//            //skillVE.Add(skillName);

//            //VisualElement ButtonsVE = new VisualElement();
//            //ButtonsVE.style.flexGrow = 0;
//            //ButtonsVE.style.height = 16;
//            //ButtonsVE.style.width = 64;
//            //ButtonsVE.style.alignContent = Align.Center;
//            //ButtonsVE.style.alignItems = Align.Center;
//            //ButtonsVE.style.flexDirection = FlexDirection.Row;
//            //skillVE.Add(ButtonsVE);

//            //skillLevel = new Label();
//            //skillLevel.text = Level + "/" + MaxLevel;
//            //skillLevel.style.unityTextAlign = TextAnchor.MiddleCenter;


//            //Button buttonMinus = new Button();
//            //buttonMinus.text = "-";
//            //buttonMinus.style.height = 16;
//            //buttonMinus.style.width = 16;


//            //buttonMinus.clicked += () => RemoveLevel(true);

//            //Icon.RegisterCallback<MouseUpEvent>(obj => {
//            //    if (obj.button == 0)
//            //        AddLevel();
//            //    else if (obj.button == 1)
//            //        RemoveLevel();
//            //});

//            //Button buttonPlus = new Button();
//            //buttonPlus.text = "+";
//            //buttonPlus.style.height = 16;
//            //buttonPlus.style.width = 16;
//            //buttonPlus.clicked += () => AddLevel();

//            //ButtonsVE.Add(buttonMinus);
//            //ButtonsVE.Add(skillLevel);
//            //ButtonsVE.Add(buttonPlus);

//            //ve.Add(skillVE);
//        }
//    }
//}
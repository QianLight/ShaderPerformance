using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace TDTools.SkillTreeSimulator {
    /// <summary>
    /// 加点模拟器窗口
    /// </summary>
    public class SkillTreeSimulator : EditorWindow {

        #region 内部类
        struct Character {
            public int ID;
            public string Name;

            public override string ToString() => $"{ID}\t{Name}";
        }

        class Skill {
            SkillTreeSimulator _owner;

            public int InitialLevel;
            public int Level;
            public int MaxLevel;

            public int layer;
            public string PreReq;
            public int LevelUpAlreadyCost;


            public Texture Icon;

            public float x, y;

            Dictionary<string, string> row;

            public string Tooltip {
                get {
                    if (Level == 0) {
                        return _tooltips["1"];
                    } else {
                        return _tooltips[Level.ToString()];
                    }
                }
            }
            private Dictionary<string, string> _tooltips;

            public GUIContent SkillName;

            public void AddLevel(bool history = true, bool ignoreLimits = false) {
                if (ignoreLimits || Level < MaxLevel && _owner.PointSpent >= LevelUpAlreadyCost && (PreReq == "" || _owner._skills[PreReq].Level == _owner._skills[PreReq].MaxLevel)) {
                    Level++;
                    _owner.PointSpent++;
                    if (history) {
                        _owner.History.Add("+ " + row["SkillScript"]);
                        _owner.veList.Rebuild();
                    }
                    if (!ignoreLimits)
                        _owner.ChangePointSpent();
                }
            }

            public bool CanAddLevel() {
                return Level < MaxLevel && _owner.PointSpent >= LevelUpAlreadyCost && (PreReq == "" || _owner._skills[PreReq].Level == _owner._skills[PreReq].MaxLevel);
            }

            public void RemoveLevel(bool history = true) {
                if (Level <= InitialLevel)
                    return;
                Level--;
                _owner.PointSpent--;
                _owner.ChangePointSpent();
                if (history) {
                    _owner.History.Add("- " + row["SkillScript"]);
                    _owner.veList.Rebuild();
                }
            }

            public void AddTooltip(Dictionary<string, string> row) {
                string s = $"描述\n{row["CurrentLevelDescription"]}\n\n\nPVP描述\n{row["CurrentLevelDescriptionPVP"]}";
                s = s.Replace("&c1&", "<b><color=orange>");
                s = s.Replace("&c2&", "<b><color=red>");
                s = s.Replace("&c3&", "<b><color=cyan>");
                s = s.Replace("&c0&", "</color></b>");
                s = s.Replace("&c4&", "<i><color=silver>[");
                s = s.Replace("&c5&", "]</color></i>");
                s = s.Replace("\\n", "\n");
                _tooltips[row["SkillLevel"]] = s;
            }

            public Skill(SkillTreeSimulator owner, Dictionary<string, string> row) {
                _owner = owner;
                MaxLevel = int.Parse(row["SkillTotalLevel"]);
                PreReq = row["PreSkill"];
                if (row["InitLevel"] != "") {
                    Level = int.Parse(row["InitLevel"]);
                    InitialLevel = Level;
                }

                this.row = row;

                if (row["LevelupAlreadyCost"] != "")
                    LevelUpAlreadyCost = int.Parse(row["LevelupAlreadyCost"]);
                else
                    LevelUpAlreadyCost = 0;
                var layerStrings = row["SkillLayer"].Split('|');
                layer = int.Parse(layerStrings[0]);

                float p = 90f - float.Parse(layerStrings[1]) * 360f / 12f;
                x = -(layer + 1) * 128 * Mathf.Cos(p * Mathf.Deg2Rad);
                y = -(layer + 1) * 128 * Mathf.Sin(p * Mathf.Deg2Rad) - 48;

                Icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/BundleRes/UI/UISource/ui_skill/" + row["Atlas"] + "/" + row["Icon"] + ".png");
                _tooltips = new Dictionary<string, string>();
                AddTooltip(row);
                SkillName = new GUIContent(row["ScriptName"]);
            }
        }
        #endregion

        #region 变量
        public Texture[] SkillTierIcon;

        public Texture CheckMarkIcon;

        private List<Dictionary<string, string>> _tableSkillListForRole;
        private List<Dictionary<string, string>> _tablePartnerInfo;
        private Dictionary<int, string> _dicPartnerName;
        private Dictionary<string, Skill> _skills;

        private List<Character> _characters;
        public List<string> History;
        private int _characterID = 1;
        public int PointSpent = 0;
        private Label _labelPointSpent;

        public ListView veList;

        IMGUIContainer _skillContainer;

        Vector2 _treeOffset;
        #endregion

        #region 初始化

        [MenuItem("Tools/TDTools/技能工具/技能加点模拟器")]
        public static void ShowWindow() {
            SkillTreeSimulator wnd = GetWindow<SkillTreeSimulator>();
            wnd.titleContent = new GUIContent("技能加点模拟器");

        }

        void OnEnable() {
            wantsMouseMove = true;
            wantsLessLayoutEvents = false;

            SkillTierIcon = new Texture[5];
            for (int i = 0; i < 5; i++) {
                SkillTierIcon[i] = AssetDatabase.LoadAssetAtPath<Texture>($"Assets/BundleRes/UI/UISource/ui_skillsystem/ui_skillsystem_skilltree_num_{i + 1}.png");
            }

            CheckMarkIcon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Tools/AmplifyShaderEditor/Plugins/EditorResources/UI/Buttons/Checkmark.png");

            _characters = new List<Character>();
            PointSpent = 0;

            TableChecker.TableChecker.DATAPATH = Application.dataPath;
            _tableSkillListForRole = TableChecker.TableChecker.ReadTable("SkillListForRole");
            _tablePartnerInfo = TableChecker.TableChecker.ReadTable("PartnerInfo");
            _dicPartnerName = new Dictionary<int, string>();
            for (int i = 0; i < _tablePartnerInfo.Count; i++) {
                _dicPartnerName[int.Parse(_tablePartnerInfo[i]["ID"])] = _tablePartnerInfo[i]["Name"];
            }


            for (int i = 0; i < _tableSkillListForRole.Count; i++) {
                var row = _tableSkillListForRole[i];
                if (row["SkillLayer"] != "" && row["SkillLevel"].CompareTo("1") == 0) {
                    int id = int.Parse(row["SkillPartnerID"]);
                    _characters.Add(new Character {
                        ID = id,
                        Name = _dicPartnerName[id]
                    });
                }
            }

            _skills = new Dictionary<string, Skill>();
            History = new List<string>();
        }

        #endregion

        #region 按钮功能

        /// <summary>
        /// 更改已经消耗的总技能点
        /// </summary>
        public void ChangePointSpent() {
            _labelPointSpent.text = "已使用技能点: " + PointSpent.ToString();
            foreach (var pair in _skills) {
                if (pair.Value.LevelUpAlreadyCost > PointSpent || (pair.Value.PreReq != "" && _skills[pair.Value.PreReq].Level != _skills[pair.Value.PreReq].MaxLevel)) {
                    if (pair.Value.Level > pair.Value.InitialLevel) {
                        PointSpent--;
                        pair.Value.Level--;
                        ChangePointSpent();
                        History.Add("- " + pair.Key);
                        veList.Rebuild();
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 撤销上一个加点
        /// </summary>
        public void Revert() {
            if (History.Count == 0)
                return;
            var last = History[History.Count - 1].Split(' ');
            History.RemoveAt(History.Count - 1);

            if (last[0].CompareTo("+") == 0) {
                _skills[last[1]].RemoveLevel(false);
            } else {
                _skills[last[1]].AddLevel(false);
            }
            veList.Rebuild();
        }

        #endregion

        #region 画技能树

        #region 来自 Unity Wiki
        public void DrawLine(Vector2 P1, Vector2 P2, Color color, float width) {
            Vector2 offset = new Vector2(_skillContainer.resolvedStyle.width / 2 + 32, _skillContainer.resolvedStyle.height / 2 + 32);

            P1 += offset;
            P2 += offset;

            if (!_skillContainer.contentRect.Contains(P1) && !_skillContainer.contentRect.Contains(P2))
                return;

            float ratio1 = P1.x / P1.y;
            float ratio2 = P2.x / P2.y;

            P1.x = Mathf.Clamp(_skillContainer.contentRect.xMin, P1.x, _skillContainer.contentRect.xMax);
            //P1.y = P1.x / ratio1;
            P2.x = Mathf.Clamp(_skillContainer.contentRect.xMin, P2.x, _skillContainer.contentRect.xMax);
            //P2.y = P2.x / ratio2;


            P1.y = Mathf.Clamp(_skillContainer.contentRect.yMin, P1.y, _skillContainer.contentRect.yMax);
            //P1.x = P1.y * ratio1;
            P2.y = Mathf.Clamp(_skillContainer.contentRect.yMin, P2.y, _skillContainer.contentRect.yMax);
            //P2.x = P2.y * ratio2;

            Matrix4x4 matrix = GUI.matrix;
            if ((P2 - P1).magnitude < 0.001f) 
                return;

            var savedColor = GUI.color;
            GUI.color = color;
            P2.y += width / 2; 

            float angle = Vector3.Angle(P2 - P1, Vector2.right);

            if (P1.y > P2.y) { angle = -angle; }


            Vector3 pivot = new Vector2(P1.x, P1.y + width / 2);
            GUIUtility.RotateAroundPivot(angle, pivot);
            GUI.DrawTexture(new Rect(P1.x, P1.y, (P2 - P1).magnitude, width), new Texture2D(64, 64));

            GUI.matrix = matrix;
            GUI.color = savedColor;
        }

        private Vector2 CubeBezier(Vector2 s, Vector2 st, Vector2 e, Vector2 et, float t) {
            float rt = 1 - t;
            return rt * rt * rt * s + 3 * rt * rt * t * st + 3 * rt * t * t * et + t * t * t * e;
        }

        public void DrawBezierLine(Vector2 start, Vector2 startTangent, Vector2 end, Vector2 endTangent, Color color, float width, int segments) {
            Vector2 lastV = CubeBezier(start, startTangent, end, endTangent, 0);
            for (int i = 1; i < segments + 1; ++i) {
                Vector2 v = CubeBezier(start, startTangent, end, endTangent, i / (float)segments);
                DrawLine(lastV, v, color, width);
                lastV = v;
            }
        }

        public void DrawCircle(Vector2 center, int radius, Color color, float width, int segmentsPerQuarter) {
            float rh = (float)radius / 2;

            Vector2 p1 = new Vector2(center.x, center.y - radius);
            Vector2 p1_tan_a = new Vector2(center.x - rh, center.y - radius);
            Vector2 p1_tan_b = new Vector2(center.x + rh, center.y - radius);

            Vector2 p2 = new Vector2(center.x + radius, center.y);
            Vector2 p2_tan_a = new Vector2(center.x + radius, center.y - rh);
            Vector2 p2_tan_b = new Vector2(center.x + radius, center.y + rh);

            Vector2 p3 = new Vector2(center.x, center.y + radius);
            Vector2 p3_tan_a = new Vector2(center.x - rh, center.y + radius);
            Vector2 p3_tan_b = new Vector2(center.x + rh, center.y + radius);

            Vector2 p4 = new Vector2(center.x - radius, center.y);
            Vector2 p4_tan_a = new Vector2(center.x - radius, center.y - rh);
            Vector2 p4_tan_b = new Vector2(center.x - radius, center.y + rh);

            DrawBezierLine(p1, p1_tan_b, p2, p2_tan_a, color, width, segmentsPerQuarter);
            DrawBezierLine(p2, p2_tan_b, p3, p3_tan_b, color, width, segmentsPerQuarter);
            DrawBezierLine(p3, p3_tan_a, p4, p4_tan_b, color, width, segmentsPerQuarter);
            DrawBezierLine(p4, p4_tan_a, p1, p1_tan_a, color, width, segmentsPerQuarter);
        }
        #endregion

        void DrawTree() {
            Event e = Event.current;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 10;
            style.alignment = TextAnchor.MiddleCenter;

            GUIStyle levelStyle = new GUIStyle(GUI.skin.label);
            levelStyle.alignment = TextAnchor.MiddleCenter;

            GUIStyle tooltip = new GUIStyle(GUI.skin.window);
            tooltip.richText = true;
            tooltip.wordWrap = true;

            if (e.type == EventType.Repaint) {

                try {
                    for (int i = 0; i < 5; i++) {
                    var center = new Vector2(-16, -32) + _treeOffset;
                    DrawCircle(center, (i + 1) * 128 + 64, Color.gray, 2f, 32);
                    }

                    foreach (var pair in _skills) {
                        if (pair.Value.PreReq != "") {
                            DrawLine(new Vector2(-pair.Value.x, pair.Value.y) + _treeOffset, new Vector2(-_skills[pair.Value.PreReq].x, _skills[pair.Value.PreReq].y) + _treeOffset, Color.red, 3f);
                        }
                    }
                } catch {
                    Debug.Log($"技能前置错误");
                }

                foreach (var pair in _skills) {
                    float x = -pair.Value.x + _skillContainer.resolvedStyle.width / 2 + _treeOffset.x;
                    float y = pair.Value.y + _skillContainer.resolvedStyle.height / 2 + _treeOffset.y;

                    //if (x < -64 || y < - 128 || x > _skillContainer.contentRect.xMax || y > _skillContainer.contentRect.yMax)
                    //    continue;

                    Rect imageRect = new Rect(x, y, 64, 64);

                    if (pair.Value.Level == pair.Value.MaxLevel || pair.Value.CanAddLevel()) {
                        GUI.DrawTexture(imageRect, pair.Value.Icon, ScaleMode.ScaleToFit, true, 1.0f, Color.white, 0f, 0f);
                    } else {
                        GUI.DrawTexture(imageRect, pair.Value.Icon, ScaleMode.ScaleToFit, true, 1.0f, new Color(0.3f, 0.3f, 0.3f, 1f), 0f, 0f);
                    }
                    GUI.DrawTexture(new Rect(x - 16, y - 16, 32, 32), SkillTierIcon[pair.Value.layer]);

                    if (pair.Value.Level == pair.Value.MaxLevel)
                        GUI.DrawTexture(new Rect(x + 48, y, 16, 16), CheckMarkIcon, ScaleMode.ScaleToFit, true, 1.0f, Color.green, 0f, 0f);

                    GUIContent skillLevel = new GUIContent($"{pair.Value.Level}/{pair.Value.MaxLevel}");
                    GUIContent skillScript = new GUIContent(pair.Key);
                    GUIContent skillName = pair.Value.SkillName;

                    var nameSize = levelStyle.CalcSize(skillName);
                    var scriptSize = style.CalcSize(skillScript);
                    var levelSize = levelStyle.CalcSize(skillLevel);

                    GUI.Label(new Rect(new Vector2(x - nameSize.x / 2 + 32, y + 64), nameSize), skillName, levelStyle);
                    GUI.Label(new Rect(new Vector2(x - scriptSize.x / 2 + 32, y + 64 + nameSize.y), scriptSize), skillScript, style);
                    GUI.Label(new Rect(new Vector2(x - levelSize.x / 2 + 32, y + 64 + nameSize.y + scriptSize.y), levelSize), skillLevel, levelStyle);

                }
            }

            bool onSkill = false;
            foreach (var pair in _skills) {
                float x = -pair.Value.x + _skillContainer.resolvedStyle.width / 2 + _treeOffset.x;
                float y = pair.Value.y + _skillContainer.resolvedStyle.height / 2 + _treeOffset.y;
                Rect imageRect = new Rect(x, y, 64, 64);
                if (imageRect.Contains(e.mousePosition)) {
                    onSkill = true;
                    float height = tooltip.CalcHeight(new GUIContent(pair.Value.Tooltip), 300);
                    GUI.Box(new Rect(_skillContainer.resolvedStyle.width - 300, 0, 300, height), pair.Value.Tooltip, tooltip);
                    if (e.type == EventType.MouseUp) {
                        if (e.button == 0) {
                            pair.Value.AddLevel();
                        } else {
                            pair.Value.RemoveLevel();
                        }
                        break;
                    }
                }
            }

            if (!onSkill && _skillContainer.contentRect.Contains(e.mousePosition) && e.type == EventType.MouseDrag && e.button == 0) {
                _treeOffset += e.delta;
                _skillContainer.MarkDirtyRepaint();
            }

            if (e.type == EventType.MouseMove)
                _skillContainer.MarkDirtyRepaint();
        }

        #endregion

        #region 读取剪切板
        /// <summary>
        ///从剪切板中读取技能数据
        ///如果有=技能等级分开，则没有加点顺序历史
        /// </summary>
        public void LoadFromPasteBin() {
            int current;
            string pastebin = GUIUtility.systemCopyBuffer;
            string[] skills = pastebin.Split('|');
            try {
                for (int i = 0; i < skills.Length; i++) {
                    current = i;
                    if (skills[i].Contains("=")) {
                        string[] s = skills[i].Split('=');
                        if (_skills.ContainsKey(s[0]))
                            for (int j = _skills[s[0]].Level; j < int.Parse(s[1]) && j < _skills[s[0]].MaxLevel; j++)
                                _skills[s[0]].AddLevel(false, true);
                        else
                            Debug.Log($"没有找到技能 {s[0]} 已跳过");
                    } else {
                        if (_skills.ContainsKey(skills[i]))
                            _skills[skills[i]].AddLevel(true, false);
                        else
                            Debug.Log($"没有找到技能 {skills[i]} 已跳过");
                    }
                }
                ChangePointSpent();
            } catch (Exception e) {
                Debug.Log(e.Message);
            }
        }
        #endregion

        #region UI

        public void CreateGUI() {
            //工具栏
            Toolbar bar = new Toolbar();
            bar.style.flexShrink = 0;
            rootVisualElement.Add(bar);

            //角色选择
            int index = 0;
            for (int i = 0; i < _characters.Count; i++)
                if (_characters[i].ID == _characterID) {
                    index = i;
                    break;
                }
            PopupField<Character> popup = new PopupField<Character>(_characters, index);
            bar.Add(popup);

            //新技能树按钮
            ToolbarButton newTree = new ToolbarButton();
            newTree.text = "新技能树";
            newTree.clicked += RepaintWindow;
            newTree.style.unityTextAlign = TextAnchor.MiddleCenter;
            newTree.style.marginRight = 128;
            bar.Add(newTree);


            //已消耗技能点
            _labelPointSpent = new Label();
            _labelPointSpent.text = "已使用技能点: " + PointSpent;
            _labelPointSpent.style.marginRight = 128;
            _labelPointSpent.style.unityTextAlign = TextAnchor.MiddleCenter;
            bar.Add(_labelPointSpent);

            //撤销按钮
            ToolbarButton revertButton = new ToolbarButton();
            revertButton.text = "撤销";
            revertButton.clicked += Revert;
            bar.Add(revertButton);

            //保存加点顺序按钮
            ToolbarButton saveHistory = new ToolbarButton();
            saveHistory.text = "输出加点顺序";
            saveHistory.clicked += () => {
                string result = "";
                for (int i = 0; i < History.Count; i++) {
                    string[] s = History[i].Split(' ');
                    if (s[0].CompareTo("-") == 0) {
                        SkillTreeSimulatorPasteWindow.Show("无法生成加点顺序！目前不支持删点！");
                        return;
                    } else {
                        if (i > 0)
                            result += "|";
                        result += s[1];
                    }
                }
                SkillTreeSimulatorPasteWindow.Show(result);
            };
            bar.Add(saveHistory);

            //保存技能列表按钮
            ToolbarButton saveSkillList = new ToolbarButton();
            saveSkillList.text = "输出技能列表";
            saveSkillList.clicked += () => {
                int count = 0;
                string result = "";
                foreach (var pair in _skills) {
                    if (pair.Value.Level > pair.Value.InitialLevel) {
                        if (count > 0)
                            result += "|";
                        result += $"{pair.Key}={pair.Value.Level}";
                        count++;
                    }
                }
                SkillTreeSimulatorPasteWindow.Show(result);
            };
            bar.Add(saveSkillList);

            ToolbarButton loadFromPasteBinButton = new ToolbarButton();
            loadFromPasteBinButton.text = "从剪切板读取技能树(请先切换到指定技能树)";
            loadFromPasteBinButton.clicked += LoadFromPasteBin;
            bar.Add(loadFromPasteBinButton);

            _skillContainer = new IMGUIContainer();
            _skillContainer.onGUIHandler += DrawTree;
            _skillContainer.style.flexGrow = 100;
            _skillContainer.style.overflow = Overflow.Hidden;

            VisualElement skilltreeContainer = new VisualElement();
            skilltreeContainer.style.flexGrow = 100;
            skilltreeContainer.style.flexDirection = FlexDirection.Row;
            rootVisualElement.Add(skilltreeContainer);

            VisualElement makeItem() => new Label();
            void bindItem(VisualElement e, int i) => (e as Label).text = History[i];

            veList = new ListView(History, 16, makeItem, bindItem);
            veList.style.alignContent = Align.Stretch;
            veList.style.borderRightWidth = 1;
            veList.style.borderRightColor = Color.black;
            veList.style.width = 256;

            skilltreeContainer.Add(veList);
            skilltreeContainer.Add(_skillContainer);

            RepaintWindow();

            /// <summary>
            /// 重绘界面
            /// </summary>
            void RepaintWindow() {
                _treeOffset = new Vector2();
                _characterID = popup.value.ID;
                _skills.Clear();
                History.Clear();
                PointSpent = 0;
                _labelPointSpent.text = "已使用技能点: " + PointSpent;

                for (int i = 0; i < _tableSkillListForRole.Count; i++) {
                    var row = _tableSkillListForRole[i];
                    if (row["SkillPartnerID"].CompareTo(_characterID.ToString()) == 0 && row["SkillLayer"] != "" && row["SkillLevel"].CompareTo("1") == 0) {
                        _skills[row["SkillScript"]] = new Skill(this, row);
                    }
                }

                for (int i = 0; i < _tableSkillListForRole.Count; i++) {
                    var row = _tableSkillListForRole[i];
                    if (row["SkillPartnerID"].CompareTo(_characterID.ToString()) == 0 && row["SkillLayer"] != "" && row["SkillLevel"].CompareTo("1") != 0) {
                        _skills[row["SkillScript"]].AddTooltip(row);
                    }
                }

                Repaint();
                veList.Rebuild();
            }
        }


    }

    #endregion
}
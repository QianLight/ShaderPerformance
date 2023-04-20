using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace TDTools.ResourceScanner {
    public class MergeWindow : EditorWindow {
        
        [System.Serializable]
        class ChangeStruc {
            public string TableName;
            public string TableID;
            public string ID;

            public Dictionary<string, string> Changes;
        }

        class MergeScrollView {
            public ScrollView Root;
            public List<Toggle> Toggles;

            public string Title;
            public string[] Values;

            public MergeScrollView(string title, string[] text, bool[] changed, bool beToggle = true) {
                ScrollView scroll;
                Title = title;
                Values = text;
                Toggles = new List<Toggle>();
                if (beToggle) {
                    scroll = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
                    scroll.style.flexGrow = 100;
                } else {
                    scroll = new ScrollView(ScrollViewMode.Vertical);
                    scroll.style.flexShrink = 0;
                    scroll.style.marginBottom = 16;
                }

                scroll.contentContainer.style.overflow = Overflow.Visible;
                scroll.style.marginLeft = 1;
                scroll.style.marginRight = 1;
                scroll.style.borderLeftWidth = 2;
                scroll.style.borderLeftColor = Color.gray;

                if (beToggle) {
                    Button titleToggle = new Button();
                    titleToggle.text = title;
                    titleToggle.style.unityTextAlign = TextAnchor.MiddleLeft;
                    scroll.Add(titleToggle);
                    titleToggle.clicked += ()=>{
                        for (int i = 0; i < Toggles.Count; i++) {
                            Toggles[i].value = true;
                        }
                    };
                } else {
                    Label titleLabel = new Label();
                    titleLabel.text = title;
                    scroll.Add(titleLabel);
                }

                for (int i = 0; i < text.Length; i++) {
                    if (beToggle) {
                        if (!ShowDifference || changed[i]) {
                            VisualElement toggleContainer = new VisualElement();
                            toggleContainer.style.flexDirection = FlexDirection.Row;
                            scroll.Add(toggleContainer);

                            Toggle toggle = new Toggle();
                            toggle.RegisterValueChangedCallback(obj => {
                            });
                            toggleContainer.Add(toggle);
                            Toggles.Add(toggle);

                            int index = i;
                            toggle.RegisterValueChangedCallback(obj => {

                            });
                            var content = new GUIContent(text[i]);
                            IMGUIContainer textC = new IMGUIContainer();
                            textC.onGUIHandler += () => {
                                GUIStyle style = new GUIStyle(GUI.skin.label);
                                style.richText = true;
                                GUILayout.Label(content, style);
                            };
                            toggleContainer.Add(textC);

                            if (changed[i]) {
                                toggleContainer.style.backgroundColor = Color.yellow;
                                //toggle.labelElement.style.color = Color.red;
                                content.text = $"<color=red>{text[i]}</color>";
                            }
                        }
                    } else {
                        if (!ShowDifference || changed[i]) {
                            Label label = new Label(text[i]);
                            scroll.Add(label);
                        }
                    }
                }

                foreach (var v in scroll.Children()) {
                    v.style.overflow = Overflow.Visible;
                    v.style.flexGrow = 100;
                    v.style.maxHeight = 22;
                    v.style.minHeight = 22;
                    v.style.flexShrink = 0;
                    v.style.paddingTop = 3;
                    v.style.paddingBottom = 0;
                    v.style.marginTop = 3;
                    v.style.marginBottom = 0;
                    v.style.borderTopColor = Color.gray;
                    v.style.borderTopWidth = 1;
                }


                Root = scroll;
            }

            public void ResetToggle(int index) {
                if (index < Toggles.Count)
                    Toggles[index].SetValueWithoutNotify(false);
            }
        }

        [SerializeField]
        List<TableChangeRecord> _records;
        [SerializeField]
        int _currentIndex = 0;

        static bool ShowDifference = false;

        [SerializeField]
        List<ChangeStruc> _changes;

        [SerializeField]
        Dictionary<string, string> _currentValue;

        public void SetRecords(List<TableChangeRecord> records) {
            if (records == null || records.Count == 0) {
                Close();
                return;
            }
            _records = records;
            _currentIndex = 0;
            _changes = new List<ChangeStruc>();
            CreateGUI();
        }

        public void CreateGUI() {
            rootVisualElement.Clear();
            VisualElement rootVE = new VisualElement();
            titleContent = new GUIContent("表格合并");
            rootVisualElement.Add(rootVE);
            rootVE.style.flexGrow = 100;

            Toolbar bar = new Toolbar();
            bar.style.flexShrink = 0;
            bar.style.justifyContent = Justify.SpaceBetween;
            rootVE.Add(bar);

            Toggle toggleShowOnlyDifference = new Toggle();
            toggleShowOnlyDifference.text = "只显示不同项";
            toggleShowOnlyDifference.value = ShowDifference;
            toggleShowOnlyDifference.RegisterValueChangedCallback(obj => {
                ShowDifference = obj.newValue;
                CreateGUI();
            });
            bar.Add(toggleShowOnlyDifference);

            VisualElement btnNextVE = new VisualElement();
            btnNextVE.style.flexDirection = FlexDirection.Row;
            bar.Add(btnNextVE);

            Button btnPre = new Button();
            btnPre.text = "上一个";
            btnPre.clicked += () => {
                _currentIndex--;
                if (_currentIndex < 0)
                    _currentIndex = 0;
                CreateGUI();
            };
            btnNextVE.Add(btnPre);

            Label labelCount = new Label();
            if (_records != null)
                labelCount.text = $"({_currentIndex + 1} / {_records.Count})";
            labelCount.style.flexShrink = 0;
            labelCount.style.width = 100;
            labelCount.style.fontSize = 20;
            labelCount.style.unityTextAlign = TextAnchor.MiddleCenter;
            btnNextVE.Add(labelCount);

            void Next() {
                bool found = false;
                for (int i = 0; i < _changes.Count; i++) {
                    if (_changes[i].ID.Equals(_records[_currentIndex].ID) && _changes[i].TableName.Equals(_records[_currentIndex].TableName)) {
                        found = true;
                        string[] s = _changes[i].TableID.Split('|');
                        string id = "";
                        for (int j = 0; j < s.Length; j++) {
                            if (j > 0)
                                id += "=";
                            id += _currentValue[s[j]];
                        }
                        _changes[i].ID = id;
                        _changes[i].Changes = _currentValue;
                        break;
                    }
                }
                if (!found) {
                    ChangeStruc c = new ChangeStruc();
                    c.TableID = _records[_currentIndex].IDColumn;
                    c.TableName = _records[_currentIndex].TableName;
                    string[] s = c.TableID.Split('|');
                    string id = "";
                    for (int i = 0; i < s.Length; i++) {
                        if (i > 0)
                            id += "=";
                        id += _currentValue[s[i]];
                    }
                    c.ID = id;
                    c.Changes = _currentValue;
                    _changes.Add(c);
                }
            }

            Button btnNext = new Button();
            if (_records != null && _currentIndex == _records.Count - 1) {
                btnNext.text = "保存";
            } else {
                btnNext.text = "下一个";
            }

            btnNext.clicked += () => {
                Next();
                if (_currentIndex == _records.Count - 1) {
                    for (int i = 0; i < _changes.Count; i++) {
                        var table = TableDatabase.Instance.GetTable(_changes[i].TableName, _changes[i].TableID);
                        table.Modify(_changes[i].ID, _changes[i].Changes);
                    }
                    _changes.Clear();
                    ResourceReferenceInspector.Instance.RefreshTables();
                    Close();
                } else {
                    _currentIndex++;
                    if (_currentIndex >= _records.Count)
                        _currentIndex = _records.Count - 1;
                }
                CreateGUI();
            };
            btnNextVE.Add(btnNext);

            VisualElement btnSLVE = new VisualElement();
            btnSLVE.style.flexDirection = FlexDirection.Row;
            bar.Add(btnSLVE);

            Button btnCancel = new Button();
            btnCancel.text = "取消";
            btnCancel.clicked += () => {
                _changes.Clear();
                Close();
            };
            btnSLVE.Add(btnCancel);

            VisualElement ListVe = new VisualElement();
            rootVE.Add(ListVe);

            if (_records != null && _records.Count > 0) {
                if (_currentIndex >= _records.Count)
                    _currentIndex = 0;

                var r = _records[_currentIndex];

                var table = TableDatabase.Instance.GetTable(r.TableName, r.IDColumn);
                Dictionary<string, string> row = null;
                for (int i = _changes.Count - 1; i >= 0; i --) {
                    if (_changes[i].TableName.Equals(r.TableName) && _changes[i].ID.Equals(r.ID)) {
                        row = _changes[i].Changes;
                    }
                }

                if (row == null && table.Dic.ContainsKey(r.ID)) {
                    row = table.GetRowByID(r.ID);
                }

                if (row != null) {

                    if (r.oldKeys.Length != row.Count || r.newKeys.Length != row.Count) {

                    } else {
                        bool same = true;

                        for (int i = 0; i < r.oldKeys.Length; i++) {
                            if (!row[r.oldKeys[i]].Equals(r.oldVlaues[i]) || !row[r.oldKeys[i]].Equals(r.newValues[i])) {
                                same = false;
                                break;
                            }
                        }

                        bool[] changesOld = new bool[r.oldVlaues.Length];
                        bool[] changesCur = new bool[r.oldVlaues.Length];
                        bool[] changesNew = new bool[r.oldVlaues.Length];
                        for (int i = 0; i < r.newKeys.Length; i++) {
                            if (!r.newValues[i].Equals(r.oldVlaues[i])) {
                                changesNew[i] = true;
                                changesOld[i] = true;
                            }

                            if (!r.newValues[i].Equals(row[r.newKeys[i]])) {
                                changesCur[i] = true;
                                changesNew[i] = true;
                            }

                            if (!r.oldVlaues[i].Equals(row[r.oldKeys[i]])) {
                                changesCur[i] = true;
                                changesOld[i] = true;
                            }
                        }
                        string[] curKey = new string[row.Count];
                        row.Keys.CopyTo(curKey, 0);

                        string[] curVal = new string[row.Count];
                        row.Values.CopyTo(curVal, 0);
                        if (!same) {
                            ListVe.Add(MakeListView(curKey, r.oldVlaues, r.newValues, changesOld, curVal));
                        } else {
                            ListVe.Add(MakeListView(curKey, r.oldVlaues, r.newValues, changesOld));
                        }
                    }
                } else {
                    bool[] changesOld = new bool[r.oldVlaues.Length];
                    for (int i = 0; i < r.oldKeys.Length; i++) {
                        changesOld[i] = !r.oldVlaues.Equals(r.newValues);
                    }
                    ListVe.Add(MakeListView(r.oldKeys, r.oldVlaues, r.newValues, changesOld));
                }
            }
        }

        VisualElement MakeListView(string[] keys, string[]old, string[] newValue, bool[] changed, string[] current = null) {
            VisualElement ve = new VisualElement();
            ve.style.flexDirection = FlexDirection.Row;
            List<MergeScrollView> scrolls = new List<MergeScrollView>();

            _currentValue = new Dictionary<string, string>();
            for (int i = 0; i < keys.Length; i++) {
                _currentValue[keys[i]] = newValue[i];
            }

            if (current != null) {
                scrolls.Add(new MergeScrollView("键: ", keys, changed, false));
                scrolls.Add(new MergeScrollView("全选目前表中的: ", current, changed));
                scrolls.Add(new MergeScrollView("全选修改前的: ", old, changed));
                scrolls.Add(new MergeScrollView("全选修改后的: ", newValue, changed));
            } else {
                scrolls.Add(new MergeScrollView("键: ", keys, changed, false));
                scrolls.Add(new MergeScrollView("全选修改前的: ", old, changed));
                scrolls.Add(new MergeScrollView("全选修改后的: ", newValue, changed));
            }
            for (int i = 0; i < scrolls.Count; i++) {

                scrolls[i].Root.verticalScroller.valueChanged += obj => {
                    for (int i = 0; i < scrolls.Count; i++) {
                        if (scrolls[i].Root.verticalScroller.value != obj)
                            scrolls[i].Root.verticalScroller.value = obj;
                    }
                };

                void SetToggle(int index, int toggleIndex) {
                    scrolls[index].Toggles[toggleIndex].RegisterValueChangedCallback(obj => {
                        if (obj.newValue) {
                            for (int k = 0; k < scrolls.Count; k++) {
                                if (k != index) {
                                    scrolls[k].ResetToggle(toggleIndex);
                                }
                            }
                            _currentValue[keys[toggleIndex]] = scrolls[index].Values[toggleIndex];
                        } else {
                            scrolls[index].Toggles[toggleIndex].SetValueWithoutNotify(true);
                        }
                    });
                    if (index == scrolls.Count - 1) {
                        scrolls[index].Toggles[toggleIndex].value = true;
                        _currentValue[keys[toggleIndex]] = scrolls[index].Values[toggleIndex];
                    }
                }

                for (int j = 0; j < scrolls[i].Toggles.Count; j++) {
                    SetToggle(i, j);
                }

                if (i != scrolls.Count - 1)
                    scrolls[i].Root.verticalScroller.style.display = DisplayStyle.None;
                ve.Add(scrolls[i].Root);
            }

            ve.style.flexGrow = 100;
            return ve;
        }
    }
}
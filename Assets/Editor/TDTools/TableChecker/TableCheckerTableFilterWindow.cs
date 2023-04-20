using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;

namespace TDTools.TableChecker {

    public class FilterPopupWindow : EditorWindow {

        List<string> columnList;
        string table;
        string column;
        string value;
        int index;
        Dictionary<string, string> row;

        [MenuItem("Tools/TDTools/白名单")]
        public static void GetWindow(string table, string column, string value, Dictionary<string, string> row) {
            var wnd = ScriptableObject.CreateInstance(typeof(FilterPopupWindow)) as FilterPopupWindow;
            wnd.titleContent = new GUIContent("编辑白名单");
            wnd.SetValue(table, column, value, row);
            wnd.ShowUtility();
        }

        public void SetValue(string table, string column, string value, Dictionary<string, string> row) {
            columnList = new List<string>();
            string[] t = new string[row.Keys.Count];
            row.Keys.CopyTo(t, 0);
            for (int i = 0; i < t.Length; i++) {
                columnList.Add(t[i]);
                if (t[i].Equals(column))
                    index = i;
            }
            this.table = table;
            this.column = column;
            this.value = value;
            this.row = row;
        }

        void CreateGUI() {
            var root = rootVisualElement;

            Label labelTableName = new Label($"{table}表");
            root.Add(labelTableName);

            TextField content = new TextField();
            content.value = value;

            PopupField<string> popupField = new PopupField<string>(columnList, index);
            popupField.label = "列";
            popupField.RegisterValueChangedCallback(obj => {
                content.value =  row[obj.newValue];
                column = obj.newValue;
            });
            root.Add(popupField);
            root.Add(content);

            VisualElement buttonVE = new VisualElement();
            buttonVE.style.flexDirection = FlexDirection.Row;
            root.Add(buttonVE);


            Button buttonSaveLocal = new Button();
            buttonSaveLocal.text = "保存到本地";
            buttonSaveLocal.clicked += () => {
                var filter = new ColumnFilter() {
                    Column = column,
                    Value = content.value
                };
                var wnd = GetWindow<TableCheckerManualCheckUI>();
                wnd.AddToLocalColumn(table, filter);
                Close();
            };
            buttonVE.Add(buttonSaveLocal);

            Button buttonSaveProject = new Button();
            buttonSaveProject.text = "保存到项目";
            buttonSaveProject.clicked += () => {
                var filter = new ColumnFilter() {
                    Column = column,
                    Value = content.value
                };
                var wnd = GetWindow<TableCheckerManualCheckUI>();
                wnd.AddToProjectColumn(table, filter);
                Close();
            };
            buttonVE.Add(buttonSaveProject);

            Button buttonCancel = new Button();
            buttonCancel.text = "取消";
            buttonCancel.clicked += Close;
            buttonVE.Add(buttonCancel);
        }

        void OnLostFocus() {
            Focus();
        }

    }
    /// <summary>
    /// 列筛选
    /// </summary>
    public class ColumnFilter {
        public string Column;
        public string Value;

        public bool Enabled = true;
    }

    /// <summary>
    /// 行筛选
    /// </summary>
    public class RowFilter {
        public int Min;
        public int Max;

        public bool Enabled = true;
    }

    /// <summary>
    /// 筛选器合集，白名单和黑名单用
    /// </summary>
    public class FilterSet{
        public Dictionary<string, List<RowFilter>> RowFilters = new Dictionary<string, List<RowFilter>>();
        public Dictionary<string, List<ColumnFilter>> ColumnFilters = new Dictionary<string, List<ColumnFilter>>();

        public bool Reversed = false;

        private string _path;
        string _comment;
        string _headerLine;

        public FilterSet(string path) {
            _path = path;
            RowFilters = new Dictionary<string, List<RowFilter>>();
            ColumnFilters = new Dictionary<string, List<ColumnFilter>>();
            var whiteList = TableChecker.ReadTable(path, true);
            using StreamReader sr = new StreamReader(path, Encoding.UTF8);
            _headerLine = sr.ReadLine();
            _comment = sr.ReadLine();
            for (int i = 0; i < whiteList.Count; i++) {
                if (whiteList[i]["Type"].CompareTo("0") == 0) {
                    if (!RowFilters.ContainsKey(whiteList[i]["Table"]))
                        RowFilters[whiteList[i]["Table"]] = new List<RowFilter>();
                    RowFilters[whiteList[i]["Table"]].Add(new RowFilter { 
                        Min = int.Parse(whiteList[i]["RowMin"]),
                        Max = int.Parse(whiteList[i]["RowMax"])
                    });
                } else {
                    if (!ColumnFilters.ContainsKey(whiteList[i]["Table"]))
                        ColumnFilters[whiteList[i]["Table"]] = new List<ColumnFilter>();
                    ColumnFilters[whiteList[i]["Table"]].Add(new ColumnFilter { 
                        Column = whiteList[i]["Column"],
                        Value = whiteList[i]["Value"]
                    });
                }
            }
        }

        public void Save() {
            using StreamWriter sw = new StreamWriter(_path, false, Encoding.UTF8);
            sw.WriteLine(_headerLine);
            sw.WriteLine(_comment);
            foreach(var pair in RowFilters)
                for (int i = 0; i < pair.Value.Count; i++) {
                    RowFilter row = pair.Value[i];
                    sw.WriteLine($"{pair.Key}\t0\t{row.Min}\t{row.Max}\t\t");
                }

            foreach (var pair in ColumnFilters)
                for (int i = 0; i < pair.Value.Count; i++) {
                    ColumnFilter col = pair.Value[i];
                    sw.WriteLine($"{pair.Key}\t1\t\t\t{col.Column}\t{col.Value}");
                }
        }

        /// <summary>
        /// 指定是否符合筛选器的要求
        /// </summary>
        /// <param name="table">目标项所属的表</param>
        /// <param name="row">目标的行</param>
        /// <param name="content">目标行的内容</param>
        /// <returns>是否符合筛选器</returns>
        public bool Match(TableCheckerResult r) {
            string table = r.Table;
            int row = int.Parse(r.Row);
            var content = r.RowContent;

            if (RowFilters.ContainsKey(table)) {
                for (int i = 0; i < RowFilters[table].Count; i ++) {
                    RowFilter filter = RowFilters[table][i];
                    if (filter.Enabled && row >= filter.Min && row <= filter.Max)
                        return true != Reversed;
                }
            }

            if (ColumnFilters.ContainsKey(table)) {
                for (int i = 0; i < ColumnFilters[table].Count; i++) {
                    string[] columns = ColumnFilters[table][i].Column.Split(';');
                    string[] values = ColumnFilters[table][i].Value.Split(';');
                    for (int k = 0; k < columns.Length; k++) {
                        if (ColumnFilters[table][i].Enabled && (content[columns[k]].Contains(values[k]))) {  //|| Regex.IsMatch(content[columns[k]], values[k]))) {
                            return true != Reversed;
                        }
                    }
                }
            }

            return false != Reversed;
        }
    }
}
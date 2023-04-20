using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {
    public abstract class TableWidget : InspectorElement {
        protected bool _isEditing = false;
        protected bool _rawEditMode = false;

        protected abstract string TableName {
            get;
        }

        protected abstract string TableID {
            get;
        }

        protected Dictionary<string, BaseInspectorField> _editors;

        public TableWidget(ResourceReferenceInspector owner, Vector2 position, string id, string linkedID = "") : base(owner, position, id, linkedID) {
        }

        public override void BuildContent() {
            base.BuildContent();

            var table = TableDatabase.Instance.GetTable(TableName, TableID);
            var row = table.Table[table.Dic[ID]];
            Label labelRow = new Label($"第{table.Dic[ID] + 3}行");
            Container.Add(labelRow);

            if (_isEditing) {
                VisualElement ve = new VisualElement();
                ve.style.flexDirection = FlexDirection.Row;

                Button buttonSave = new Button();
                buttonSave.text = "保存编辑";
                buttonSave.clicked += () => {
                    if (_editors != null) {
                        Dictionary<string, string> row = new Dictionary<string, string>();
                        foreach (var pair in _editors) {
                            row[pair.Key] = pair.Value.GetValue();
                        }

                        var table = TableDatabase.Instance.GetTable(TableName, TableID);
                        var oldRow = table.Table[table.Dic[ID]];

                        bool changed = false;
                        foreach (var key in row.Keys){
                            if (!row[key].Equals(oldRow[key])) {
                                changed = true;
                                break;
                            }
                        }
                        if (changed) {
                            var record = new TableChangeRecord(row, TableName, TableID);
                            ResourceReferenceInspector.Instance.AddRecord(record);
                            var wnd = EditorWindow.GetWindow<MergeWindow>(true);
                            wnd.position = ResourceReferenceInspector.Instance.position;
                            wnd.SetRecords(new List<TableChangeRecord>() { record });
                        }
                        _editors = null;
                    }

                    _isEditing = false;
                    BuildContent();
                };
                ve.Add(buttonSave);

                Button buttonCancel = new Button();
                buttonCancel.text = "取消编辑";
                buttonCancel.clicked += () => {
                    _isEditing = false;
                    BuildContent();
                };
                ve.Add(buttonCancel);

                Button buttonRawEddit = new Button();
                buttonRawEddit.text = "Raw编辑模式";
                buttonRawEddit.clicked += () => {
                    _rawEditMode = !_rawEditMode;
                    BuildContent();
                };
                ve.Add(buttonRawEddit);

                Container.Add(ve);
                if (_rawEditMode)
                    BuildContentRaw(table, row);
                else
                    BuildEditContent(table, row);
            } else {
                Button buttonEdit = new Button();
                buttonEdit.text = "编辑";
                buttonEdit.clicked += () => {
                    _isEditing = true;
                    BuildContent();
                };
                Container.Add(buttonEdit);

                BuildTableContent(table, row);
            }
        }

        protected virtual void BuildTableContent(TableNode table, Dictionary<string, string> row) {
            foreach (var pair in row) {
                string key = pair.Key;
                if (!(Match(key) || Match(pair.Value)))
                    continue;

                if (pair.Value != "") {
                    InspectorLabel label = new InspectorLabel(this, key, pair.Value, table.Comment[pair.Key]);
                    Container.Add(label.Root);
                }
            }
        }

        protected virtual void BuildEditContent(TableNode table, Dictionary<string, string> row) {
            BuildContentRaw(table,row);
        }

        protected void BuildContentRaw(TableNode table, Dictionary<string, string> row) {
            foreach (var pair in row) {
                string key = pair.Key;
                if (!(Match(key) || Match(pair.Value)))
                    continue;

                var textField = new InspectorTextfield(this, key, pair.Value, table.Comment[pair.Key]);
                Container.Add(textField.Root);
            }
        }
    }
}

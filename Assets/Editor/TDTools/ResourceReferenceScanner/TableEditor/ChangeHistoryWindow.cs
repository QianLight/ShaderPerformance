using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace TDTools.ResourceScanner {
    public class ChangeHistoryWindow : EditorWindow {
        ListView _listView;

        public static ChangeHistoryWindow ShowWindow(Rect rect) {
            ChangeHistoryWindow wnd = ScriptableObject.CreateInstance<ChangeHistoryWindow>();
            wnd.position = rect;
            wnd.ShowPopup();
            return wnd;
        }

        public void CreateGUI() {
            Toolbar bar = new Toolbar();
            bar.style.flexShrink = 0;
            rootVisualElement.Add(bar);

            Button btnClose = new Button();
            btnClose.text = "X";
            btnClose.clicked += () => {
                Close();
            };
            bar.Add(btnClose);

            Button btnClear = new Button();
            btnClear.text = "删除所选";
            btnClear.clicked += () => {
                if (!EditorUtility.DisplayDialog("确认清除历史记录", "确认要清除所选的历史记录吗？此操作不可逆", "确认", "取消"))
                    return;

                var r = ResourceReferenceInspector.Instance.Records;
                for (int i = 0; i < r.Count; i++)
                    if (r[i].Selected) {
                        r.RemoveAt(i);
                        i--;
                    }
                _listView.Refresh();
            };
            bar.Add(btnClear);

            Button btnApply = new Button();
            btnApply.text = "再次应用或撤回";
            btnApply.clicked += () => {
                var l = ResourceReferenceInspector.Instance.Records;
                List<TableChangeRecord> list = new List<TableChangeRecord>();
                for (int i = 0; i < l.Count; i++) {
                    if (l[i].Selected)
                        list.Add(l[i]);
                }
                if (list.Count == 0)
                    return;
                GetWindow<MergeWindow>().SetRecords(list);
                Close();
            };
            bar.Add(btnApply);

            bool selectAll = true;
            Button btnSelectAll = new Button();
            btnSelectAll.text = "全选";
            btnSelectAll.clicked += () => {
                var r = ResourceReferenceInspector.Instance.Records;
                for (int i = 0; i < r.Count; i++)
                    r[i].Selected = selectAll;
                selectAll = !selectAll;
                _listView.Refresh();
            };
            bar.Add(btnSelectAll);

            VisualElement MakeItem() {
                VisualElement root = new VisualElement();
                root.style.flexDirection = FlexDirection.Row;
                Toggle toggle = new Toggle();
                root.Add(toggle);
                toggle.RegisterValueChangedCallback(obj => {
                    try {
                        int index = int.Parse(toggle.name);
                        ResourceReferenceInspector.Instance.Records[index].Selected = obj.newValue;
                    } catch { 
                    }
                });
                root.RegisterCallback<PointerUpEvent>(obj => {
                    toggle.value = !toggle.value;
                });
                Label label = new Label();
                root.Add(label);
                return root;
            }

            void BindItem(VisualElement ve, int index) {
                var i = ResourceReferenceInspector.Instance.Records.Count - index - 1;
                if (i >= ResourceReferenceInspector.Instance.Records.Count)
                    return;
                var r = ResourceReferenceInspector.Instance.Records[i];
                ve.Q<Toggle>().value = r.Selected;
                ve.Q<Toggle>().name = i.ToString();
                ve.Q<Label>().text = $"{r.DateTime} {r.TableName} {r.ID}";
            }

            _listView = new ListView(ResourceReferenceInspector.Instance.Records, 16, MakeItem, BindItem);
            _listView.style.flexGrow = 100;
            rootVisualElement.Add(_listView);
        }

        private void OnLostFocus() {
            Close();
        }

        public void Refresh() {
            _listView.Refresh();
        }


    }
}
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System;

namespace TDTools.ResourceScanner {

    /// <summary>
    /// 用来图形化显示数据资源等关联关系的工具
    /// </summary>
    public class ResourceReferenceInspector : EditorWindow {

        #region Utility Class

        struct TableNodeType {
            public TableNode node;
            public NodeType type;

            public TableNodeType(TableNode node, NodeType type) {
                this.type = type;
                this.node = node;
            }
        }

        public struct SearchOption {
            public string DisplayedName;
            public string ID;
            public int TableIndex;
            public NodeType type;
            public bool IsFavorite;

            public SearchOption(int index, string name, string id, NodeType type, bool isFavorite = false) {
                TableIndex = index;
                DisplayedName = name;
                ID = id;
                this.type = type;
                IsFavorite = isFavorite;
            }
        }

        #endregion

        #region Variables

        public static ResourceReferenceInspector Instance {
            get {
                if (_instance == null)
                    _instance = GetWindow<ResourceReferenceInspector>();
                return _instance;
            }
        }
        private static ResourceReferenceInspector _instance;

        public Vector2 Offsets;

        public bool isDraggingElement;
        private bool _isDragging;

        private Vector2 _targetStartPosition;
        private Vector3 _pointerStartPosition;

        public float Zoom = 1.0f;
        private readonly float _zoomMin = 0.5f;
        private readonly float _zoomMax = 2.0f;

        public ContextToolTip Tooltip;
        public Action OnDrag;

        VisualElement _container;

        InspectorElement _selected;
        IMGUIContainer _board;

        [SerializeField]
        List<Connection> _connections;
        [SerializeField]
        List<InspectorElement> list;

        VisualElement _listContainer;

        FavoriatedItems favoriatedItems;

        public List<TableChangeRecord> Records;

        ChangeHistoryWindow _changeHistoryWindow;

        VisualElement ReferenceRecordContainer;
        ListView RefereceRecordListView;

        [SerializeField]
        public Dictionary<ReferenceRecord, Dictionary<ReferenceRecord, int>> ReferenceRecords;

        #endregion

        #region UI

        void OnEnable() {
            //_scanner = GetWindow<ResourceReferenceScanner>(typeof(ResourceReferenceInspector));

            if (list == null)
                list = new List<InspectorElement>();
            if (_connections == null)
                _connections = new List<Connection>();
            if (Records == null)
                Records = new List<TableChangeRecord>();

            favoriatedItems = new FavoriatedItems();
        }

        private void OnDisable() {
            _changeHistoryWindow?.Close();
        }

        [MenuItem("Tools/TDTools/配置检查工具/引用可视化工具")]
        public static void ShowWindow() {
            ResourceReferenceInspector wnd = GetWindow<ResourceReferenceInspector>();
        }

        void CreateGUI() {
            rootVisualElement.Clear();
            titleContent = new GUIContent("引用检测工具");

            
            CreateListView();
            CreateBoard();

            _selected = null;
            _connections.Clear();
            for (int i = 0; i < list.Count; i++) {
                list[i].CreateUI();
                list[i] = InspectorElement.CreateElement(this, list[i].InspectorNodeType, list[i].Position, list[i].ID, list[i].LinkedID);
                _container.Add(list[i].Root);
            }
        }

        void CreateBoard() {
            VisualElement boardContainer = new VisualElement();
            boardContainer.style.flexGrow = 100;
            boardContainer.style.flexShrink = 0;

            Toolbar bar = new Toolbar();
            bar.style.flexShrink = 0;
            boardContainer.Add(bar);

            IMGUIContainer board = new IMGUIContainer();
            _board = board;
            board.onGUIHandler += DrawConnections;
            board.style.flexGrow = 100;
            board.style.flexShrink = 0;

            board.RegisterCallback<PointerDownEvent>(PointerDownHandler);
            board.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
            board.RegisterCallback<PointerUpEvent>(PointerUpHandler);
            board.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
            board.RegisterCallback<PointerLeaveEvent>(obj => {
                _isDragging = false;
            });

            _container = new VisualElement();
            board.style.overflow = Overflow.Hidden;
            board.Add(_container);
            boardContainer.Add(board);
            Tooltip = new ContextToolTip(board);
            var hint = new InputHint(board);

            rootVisualElement.Add(boardContainer);
            CreateScannerResult(boardContainer);

            VisualElement leftToolBar = new VisualElement();
            VisualElement rightToolBar = new VisualElement();
            leftToolBar.style.flexDirection = FlexDirection.Row;
            bar.Add(leftToolBar);
            bar.Add(rightToolBar);
            bar.style.justifyContent = Justify.SpaceBetween;

            Button buttonClearAll = new Button();
            buttonClearAll.text = "清除所有";
            buttonClearAll.clicked += ClearBoard;
            rightToolBar.Add(buttonClearAll);

            Button buttonRefresh = new Button();
            buttonRefresh.text = "刷新表格";
            buttonRefresh.clicked += RefreshTables;
            rightToolBar.Add(buttonRefresh);

            Slider zoomSlider = new Slider(_zoomMin, _zoomMax);
            Zoom = 1.0f;

            Button resetZoomButton = new Button();
            resetZoomButton.text = "重置缩放";
            resetZoomButton.clicked += () => {
                zoomSlider.value = 1.0f;
            };
            rightToolBar.Add(resetZoomButton
                );
            Label zoomLabel = new Label($"{Zoom.ToString("0.0")}X");
            rightToolBar.Add(zoomLabel);

            zoomSlider.style.flexShrink = 0;
            zoomSlider.style.width = 160;
            zoomSlider.value = Zoom;
            zoomSlider.RegisterValueChangedCallback(obj => {
                Zoom = zoomSlider.value;
                _container.transform.scale = new Vector3(Zoom, Zoom, 1f);
                zoomSlider.SetValueWithoutNotify(Zoom);
                zoomLabel.text = $"{Zoom.ToString("0.0")}X";
            });

            board.RegisterCallback<WheelEvent>((evt) => {
                Zoom += evt.delta.y * 0.05f;
                Zoom = Mathf.Clamp(Zoom, _zoomMin, _zoomMax);
                _container.transform.scale = new Vector3(Zoom, Zoom, 1f);
                zoomSlider.SetValueWithoutNotify(Zoom);
                zoomLabel.text = $"{Zoom:0.0}X";
            });

            rightToolBar.Add(zoomSlider);
            rightToolBar.style.flexDirection = FlexDirection.Row;
            bool hiden = false;
            Button hideLeft = new Button();
            hideLeft.text = "隐藏边栏";
            hideLeft.style.flexShrink = 0;
            hideLeft.clicked +=()=>{
                hiden = !hiden;
                if (hiden) {
                    _listContainer.style.display = DisplayStyle.None;
                } else {
                    _listContainer.style.display = DisplayStyle.Flex;
                }
            };
            leftToolBar.Add(hideLeft);

            Button buttonHistory = new Button();
            buttonHistory.text = "修改历史(合表工具)";
            buttonHistory.style.flexShrink = 0;
            buttonHistory.clicked += () => {
                if (_changeHistoryWindow == null) {
                    _changeHistoryWindow = ChangeHistoryWindow.ShowWindow(new Rect(position.position + buttonHistory.worldBound.position + new Vector2(0, 15), new Vector2(256, 512)));
                } else {
                    _changeHistoryWindow.Close();
                    _changeHistoryWindow = null;
                }
            };
            leftToolBar.Add(buttonHistory);

            Label labelSearchBar = new Label("筛选");
            labelSearchBar.style.unityTextAlign = TextAnchor.MiddleCenter;
            leftToolBar.Add(labelSearchBar);

            ToolbarPopupSearchField filter = new ToolbarPopupSearchField();
            filter.RegisterValueChangedCallback(obj => {
                for (int i = 0; i < list.Count; i++)
                    list[i].SetFilterText(obj.newValue);
            });
            leftToolBar.Add(filter);

        }

        void CreateScannerResult(VisualElement boardContainer) {
            VisualElement container = new VisualElement();
            boardContainer.Add(container);
            container.style.flexGrow = 1;
            container.style.flexShrink = 1;
            container.style.maxHeight = 256;
            container.style.minHeight = 256;
            container.style.display = DisplayStyle.None;
            ReferenceRecordContainer = container;

            Toolbar bar = new Toolbar();
            bar.style.flexShrink = 0;
            container.Add(bar);

            Button btnClose = new Button();
            btnClose.text = "X";
            bar.Add(btnClose);
            btnClose.clicked += () => {
                container.style.display = DisplayStyle.None;
            };

            int currentIndex = -1;



            ListView listview = new ListView();
            listview.style.flexGrow = 100;
            listview.makeItem = MakeItem;
            listview.bindItem = BindItem;
            listview.itemHeight = 16;
            container.Add(listview);
            RefereceRecordListView = listview;

            void BindItem(VisualElement v, int index) {
                var pair = (KeyValuePair<ReferenceRecord, int>)listview.itemsSource[index];
                if (pair.Value > 1) {
                    v.Q<Label>().text = $"{pair.Key} X {pair.Value}";
                } else {
                    v.Q<Label>().text = pair.Key.ToString();
                }

                v.Q<Label>().name = index.ToString();
            }

            VisualElement MakeItem() {
                VisualElement root = new VisualElement();
                Label label = new Label();
                label.RegisterCallback<PointerOverEvent>(evt => {
                    int index = int.Parse(label.name);
                    if (index == -1 || index == currentIndex)
                        return;
                    currentIndex = index;
                    var ele = InspectorElement.CreateElement(this, ((KeyValuePair<ReferenceRecord, int>)listview.itemsSource[index]).Key.Type, -Offsets, ((KeyValuePair<ReferenceRecord, int>)listview.itemsSource[index]).Key.ID);
                    ele.SetBorderColor(Color.red);
                    ContextToolTip.Instance.SetFixed(ele.Root);
                });

                label.RegisterCallback<PointerOutEvent>(e => {
                    currentIndex = -1;
                    ContextToolTip.Instance.Hide();
                    ContextToolTip.Instance.ClearFixed();
                });
                root.Add(label);
                return root;
            }

            listview.RegisterCallback<PointerDownEvent>(obj => {
                if (obj.clickCount >= 2) {
                    var o = ((KeyValuePair<ReferenceRecord, int>)listview.selectedItem).Key;
                    Vector2 position = new Vector2(10, 5) - Offsets;
                    InspectorElement node = InspectorElement.CreateElement(this, o.Type, position, o.ID);
                    AddNode(node);
                }
            });
        }

        void CreateListView() {
            VisualElement listContainer = new VisualElement();
            _listContainer = listContainer;
            listContainer.style.flexGrow = 100;
            listContainer.style.flexShrink = 0;
            listContainer.style.maxWidth = 400;
            listContainer.style.borderRightColor = Color.black;
            listContainer.style.borderRightWidth = 1;

            Toolbar listBar = new Toolbar();
            listBar.style.flexShrink = 0;
            listContainer.Add(listBar);

            ToolbarPopupSearchField searchField = new ToolbarPopupSearchField();
            searchField.style.maxWidth = 230;

            TableDatabase data = TableDatabase.Instance;
            List<SearchOption> options = new List<SearchOption>();
            List<SearchOption> filtered = new List<SearchOption>();

            List<TableNodeType> tableList = new List<TableNodeType>();
            TableChecker.TableChecker.DATAPATH = Application.dataPath;
            tableList.Add(new TableNodeType(data.GetTable("BuffList", "BuffID|BuffLevel", "BuffList {row}:\t{BuffID}={BuffLevel}    {BuffName}"), NodeType.Buff));
            tableList.Add(new TableNodeType(data.GetTable("PartnerInfo", "ID", "PartnerInfo {row}:\t{ID} {Name} {UIkeyword}"), NodeType.Partner));
            tableList.Add(new TableNodeType(data.GetTable("SkillListForEnemy", "SkillScript", "SkEnemy {row}:\t{SkillScript} {ScriptName}"), NodeType.XEntitySkill));
            tableList.Add(new TableNodeType(data.GetTable("SkillListForRole", "SkillScript", "SkRole {row}:\t{SkillScript} {ScriptName}"), NodeType.PartnerSkill));
            tableList.Add(new TableNodeType(data.GetTable("UnitAITable", "ID", "UnitAITable {row}:\t{ID} {Comment}"), NodeType.AI));
            tableList.Add(new TableNodeType(data.GetTable("XEntityStatistics", "ID", "Enemy {row}:\t{ID} {Note}"), NodeType.Enemy));
            tableList.Add(new TableNodeType(data.GetTable("XEntityPresentation", "PresentID", "Presnet {row}:\t{PresentID} {Prefab} {Name}"), NodeType.XEntityPresentation));
            tableList.Add(new TableNodeType(data.GetTable("MapList", "MapID", "MapList {row}:\t{MapID} {Comment}"), NodeType.Map));
            tableList.Add(new TableNodeType(data.GetTable("SceneList", "SceneID", "SceneList {row}:\t{SceneID} {SceneTitle}"), NodeType.Scene));

            List<string> tableNames = new List<string>();
            tableNames.Add("全部表格");
            tableNames.Add("收藏的");
            for (int i = 0; i < tableList.Count; i++) {
                tableNames.Add(tableList[i].node.TableName);
                var nameList = tableList[i].node.GetNameList();
                var idList = tableList[i].node.GetIDList();
                try {
                    for (int j = 0; j < nameList.Count; j++) {
                        options.Add(new SearchOption(i + 2, nameList[j], idList[j], tableList[i].type));
                    }
                } catch {
                }
            }
            int currentIndex = -1;
            PopupField<string> popup = new PopupField<string>(tableNames, 0);

            VisualElement MakeItem(){ 
                var label = new Label();
                label.name = "-1";
                label.RegisterCallback<PointerOverEvent>(evt => {
                    int index = int.Parse(label.name);
                    if (index == -1 || index == currentIndex)
                        return;
                    currentIndex = index;
                    var ele = InspectorElement.CreateElement(this, filtered[index].type, -Offsets, filtered[index].ID);
                    ele.SetBorderColor(Color.red);
                    ContextToolTip.Instance.SetFixed(ele.Root);
                });

                label.RegisterCallback<PointerOutEvent>(e => {
                    currentIndex = -1;
                    ContextToolTip.Instance.Hide();
                    ContextToolTip.Instance.ClearFixed();
                });

                label.RegisterCallback<ContextClickEvent>(obj => {
                    int index = int.Parse(label.name);
                    if (index == -1)

                        return;
                    GenericMenu menu = new GenericMenu();
                    if (!filtered[index].IsFavorite) {
                        menu.AddItem(new GUIContent($"收藏: {filtered[index].ID}"), false, () => {
                            favoriatedItems.Add(filtered[index]);
                            OnSearchChange(searchField.value);
                        });
                    } else {
                        menu.AddItem(new GUIContent($"删除收藏: {filtered[index].ID}"), false, () => {
                            favoriatedItems.Remove(filtered[index].type, filtered[index].ID);
                            OnSearchChange(searchField.value);
                        });
                    }
                    menu.AddItem(new GUIContent($"复制: {filtered[index].ID}"), false, () => { GUIUtility.systemCopyBuffer = filtered[index].ID; });
                    InspectorElement.AddGMMenu(menu, filtered[index].type, filtered[index].ID);
                    menu.ShowAsContext();
                });
                return label;
            };

            void BindItem(VisualElement v, int index){
                (v as Label).text = filtered[index].DisplayedName;
                v.name = index.ToString();
                if (filtered[index].IsFavorite)
                    v.style.color = Color.yellow;
                else
                    v.style.color = new StyleColor(StyleKeyword.Null);
            };

            ListView listView = new ListView();
            listView.bindItem = BindItem;
            listView.makeItem = MakeItem;
            listView.itemsSource = options;
            listView.itemHeight = 16;

            OnSearchChange("");
            listView.style.flexGrow = 100;
            listView.horizontalScrollingEnabled = true;
            listContainer.Add(listView);

            listView.RegisterCallback<PointerDownEvent>(obj => {
                if (obj.clickCount >= 2) {
                    SearchOption o = (SearchOption)listView.selectedItem;
                    Vector2 position = new Vector2(10, 5) - Offsets;
                    InspectorElement node = InspectorElement.CreateElement(this, o.type, position, o.ID);
                    AddNode(node);
                }
            });

            Button buttonAdd = new Button();
            buttonAdd.text = "添加";
            buttonAdd.style.flexShrink = 0;
            buttonAdd.clicked += () => {
                SearchOption o = (SearchOption)listView.selectedItem;
                Vector2 position = new Vector2(10, 5) - Offsets;
                InspectorElement node = InspectorElement.CreateElement(this, o.type, position, o.ID);
                AddNode(node);
            };

            searchField.RegisterValueChangedCallback(obj => OnSearchChange(obj.newValue));
            popup.RegisterValueChangedCallback(obj => OnSearchChange(searchField.value));

            listBar.Add(buttonAdd);
            listBar.Add(popup);
            listBar.Add(searchField);
            rootVisualElement.style.flexDirection = FlexDirection.Row;
            rootVisualElement.Add(listContainer);

            void OnSearchChange(string newValue) {
                if (newValue == "") {
                    filtered.Clear();
                    if (favoriatedItems == null)
                        favoriatedItems = new FavoriatedItems();
                    for (int i = 0; i < favoriatedItems.List.Count; i++) {
                        if (popup.index == 0 || popup.index == 1 || favoriatedItems.List[i].type == tableList[popup.index - 2].type) {
                            filtered.Add(favoriatedItems.List[i]);
                        }
                    }

                    int count = options.Count;
                    if (popup.index != 0) {
                        for (int i = 0; i < count; i++) {
                            if (options[i].TableIndex == popup.index)
                                filtered.Add(options[i]);
                        }
                    } else
                        filtered.AddRange(options);
                } else {
                    filtered.Clear();
                    string[] searches = newValue.ToLower().Split(' ');
                    int count = options.Count;
                    int len = searches.Length;

                    for (int i = 0; i < favoriatedItems.List.Count; i++) {
                        if (popup.index == 0 || popup.index == 1 || favoriatedItems.List[i].type == tableList[popup.index - 2].type) {
                            string s = favoriatedItems.List[i].DisplayedName.ToLower();
                            for (int j = 0; j < len; j++) {
                                if (searches[j] == "")
                                    continue;
                                if (s.Contains(searches[j])) {
                                    filtered.Add(favoriatedItems.List[i]);
                                    break;
                                }
                            }
                        }
                    }

                    for (int i = 0; i < count; i++) {
                        if (popup.index != 0 && options[i].TableIndex != popup.index)
                            continue;
                        string s = options[i].DisplayedName.ToLower();
                        for (int j = 0; j < len; j++) {
                            if (searches[j] == "")
                                continue;
                            if (s.Contains(searches[j])) {
                                filtered.Add(options[i]);
                                break;
                            }
                        }
                    }
                }

                listView.itemsSource = filtered;
                listView.Refresh();
            }

        }

        #endregion

        #region 节点操作

        void ClearBoard() {
            list.Clear();
            _container.Clear();
            _connections.Clear();
        }

        public void RefreshTables() {
            TableDatabase.Instance.Reload();
            CreateGUI();
        }

        public void AddNode(InspectorElement node) {
            _container.Add(node.Root);
            list.Add(node);
        }

        public void RemoveNode(InspectorElement node) {
            RemoveConnection(node.ConnectionFrom);
            for (int i = 0; i < node.Connections.Count; i++)
                RemoveConnection(node.Connections[i]);
            _container.Remove(node.Root);
            list.Remove(node);
        }

        public void ClearAllNodes() {
            _container.Clear();
            list.Clear();
        }

        public void SelectNode(InspectorElement e) {
            if (e == _selected)
                return;

            _selected?.UnSelect();
            _selected = e;
        }

        #endregion

        #region 引用反向扫描

        public void SetScanResult(Dictionary<ReferenceRecord, int> results) {
            List<KeyValuePair<ReferenceRecord, int>> list = new List<KeyValuePair<ReferenceRecord, int>>();
            foreach (var pair in results) {
                list.Add(pair);
            }
            RefereceRecordListView.itemsSource = list;
            RefereceRecordListView.Refresh();
            ReferenceRecordContainer.style.display = DisplayStyle.Flex;
        }

        #endregion

        #region 拖拽
        void PointerDownHandler(PointerDownEvent evt) {
            if (evt.button == 0) {
                _pointerStartPosition = evt.position;
                _targetStartPosition = Offsets;
                _isDragging = true;
            }
        }
        void PointerMoveHandler(PointerMoveEvent evt) {
            if (_isDragging) {
                Vector3 pointerDelta = evt.position - _pointerStartPosition;
                Offsets = new Vector2(
                    _targetStartPosition.x + pointerDelta.x,
                    _targetStartPosition.y + pointerDelta.y);
                OnDrag?.Invoke();
            }
        }
        void PointerUpHandler(PointerUpEvent evt) {
            rootVisualElement.ReleasePointer(evt.pointerId);
            //_selected?.UnSelect();
            _isDragging = false;
        }
        void PointerCaptureOutHandler(PointerCaptureOutEvent evt) {
            _isDragging = false;
        }


        #endregion

        #region Connection

        public void DrawLine(Vector2 P1, Vector2 P2, Color color, float width) {

            P1 += new Vector2(6, 6);
            P2 += new Vector2(6, 6);

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

        void DrawConnections() {
            List<Connection> ListRemove = new List<Connection>();
            for (int i = 0; i < _connections.Count; i++) { 
                var connection = _connections[i];
                if (connection.From == null || connection.To == null) {
                    ListRemove.Add(connection);
                    continue;
                }
                //DrawLine(connection.From.Pin.worldBound.position - _board.worldBound.position, connection.To.Pin.worldBound.position - _board.worldBound.position, Color.white, 2);

                var P1 = connection.From.Pin.worldBound.position - _board.worldBound.position;
                var P2 = connection.To.Pin.worldBound.position - _board.worldBound.position;

                var P3 = P1 + new Vector2(50, 0);
                var P4 = P2 - new Vector2(50, 0);

                DrawLine(P1, P3, Color.white, 2);
                DrawLine(P2, P4, Color.white, 2);

                if (P3.x > P4.x) {
                    Vector2 P5;
                    if (P4.y - 50 < P3.y)
                        P5 = new Vector2(P3.x, P4.y + Mathf.Max(P4.y - P3.y, 50));
                    else
                        P5 = new Vector2(P3.x, P4.y - Mathf.Min(P4.y - P3.y, 50));
                    var P6 = new Vector2(P4.x, P5.y);

                    DrawLine(P3, P5, Color.white, 2);
                    DrawLine(P4, P6, Color.white, 2);
                    DrawLine(P5, P6, Color.white, 2);
                } else {
                    var P5 = new Vector2(P3.x, P4.y);
                    var P6 = new Vector2(P4.x, P5.y);

                    DrawLine(P3, P5, Color.white, 2);
                    DrawLine(P4, P6, Color.white, 2);
                    DrawLine(P5, P6, Color.white, 2);
                }
            }
            for (int i = 0; i < ListRemove.Count; i++)
                RemoveConnection(ListRemove[i]);
        }

        public void AddConnection(Connection c) {
            _connections.Add(c);
        }

        public void RemoveConnection(Connection c) {
            if (c == null)
                return;

            if (c.From != null) {
                c.From.Connection = null;
                if (c.From.Pin != null)
                    (c.From.Pin as Image).tintColor = Color.white;
                c.From.LinkedTo = null;
            }

            if (c.To != null) {
                c.To.ConnectionFrom = null;
                if (c.To.Pin != null)
                    (c.To.Pin as Image).tintColor = Color.white;
                c.To.LinkedFrom = null;
            }

            _connections.Remove(c);
        }

        #endregion

        #region 编辑历史

        public void AddRecord(TableChangeRecord record) {
            Records.Add(record);
            if (_changeHistoryWindow != null)
                _changeHistoryWindow.Refresh();
        }

        #endregion

    }
}
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.IO;

namespace TDTools.TableChecker
{

    /// <summary>
    /// 选择需要手动检查的表格项目
    /// </summary>
    public class TableCheckerManualCheckUI : EditorWindow {

        #region Display Mode

        /// <summary>
        /// 选择检查项目窗口的显示模式
        /// </summary>
        enum CheckListDisplayMode {
            TableOnly,
            TableAndColumn,
            DetailedMode
        }

        /// <summary>
        /// 结果显示模式
        /// </summary>
        enum ResultDisplayMode {

            /// <summary>
            /// 以表为单位显示
            /// </summary>
            GroupByTable,

            /// <summary>
            /// 以重要度为单位显示
            /// </summary>
            GroupByImportance,

            /// <summary>
            /// 以负责组为单位显示
            /// </summary>
            GroupByResponsbility
        }

        #endregion

        #region Variables

        private Dictionary<string, List<TableCheckerCheckItems>> _optionList;
        private Dictionary<string, bool> _tableToCheck;
        private Dictionary<string, Dictionary<string, bool>> _columnToCheck;

        /// <summary>
        /// 需要检查的项目列表
        /// </summary>
        private TableCheckerCheckList _checkList;

        /// <summary>
        /// 显示模式
        /// </summary>
        private CheckListDisplayMode _checkListDisplayMode = CheckListDisplayMode.TableAndColumn;
        private ResultDisplayMode _resultDisplayMode = ResultDisplayMode.GroupByTable;

        ScrollView _checkListScrollView;
        /// <summary>
        /// 用来显示结果的listView
        /// </summary>
        private ScrollView _resultScrollView;

        const int LIST_HEIGHT = 16;

        private List<TableCheckerResult> _results;
        private Dictionary<string, List<TableCheckerResult>> _groupedResult;

        private bool _showImportance0 = true;
        private bool _showLowImportance = true;

        VisualElement _progressContainer;

        string _searchString;

        FilterSet _localFilterSet;
        FilterSet _projectFilterSet;

        ScrollView _filterScrollView;

        Dictionary<string, bool> _foldoutValues;

        Label _labelFilteredCount;
        Label _labelTotalResult;

        ProgressBar progressBar;

        #endregion

        #region Constructor

        [MenuItem("Tools/TDTools/配置检查工具/表格检查工具")]
        public static void ShowWindow() {
            TableCheckerManualCheckUI wnd = GetWindow<TableCheckerManualCheckUI>();
            wnd.titleContent = new GUIContent("表格检查工具");
        }

        public void CreateGUI() {
            rootVisualElement.style.flexDirection = FlexDirection.Row;
            CreateCheckListUI();
            CreateResultUI();
            CreateFilterUI();
        }

        void OnEnable() {
            _foldoutValues = new Dictionary<string, bool>();
            _searchString = "";
            _checkList = new TableCheckerCheckList();
            _checkList.Load();

            _localFilterSet = new FilterSet($@"{Application.dataPath}\Editor\TDTools\TableChecker\TableCheckerLocalWhiteList.txt");
            _projectFilterSet = new FilterSet($@"{Application.dataPath}\Editor\TDTools\TableChecker\TableCheckerProjectWhiteList.txt");

            _optionList = new Dictionary<string, List<TableCheckerCheckItems>>();
            _tableToCheck = new Dictionary<string, bool>();
            _columnToCheck = new Dictionary<string, Dictionary<string, bool>>();

            for (int i = 0; i < _checkList.List.Count; i++) {
                _tableToCheck[_checkList.List[i].SourceTable] = false;
                if (!_columnToCheck.ContainsKey(_checkList.List[i].SourceTable))
                    _columnToCheck[_checkList.List[i].SourceTable] = new Dictionary<string, bool>();
                _columnToCheck[_checkList.List[i].SourceTable][_checkList.List[i].SourceColumn] = false;

                string s = $"{_checkList.List[i].SourceTable}\t{_checkList.List[i].SourceColumn}";

                if (!_optionList.ContainsKey(s))
                    _optionList[s] = new List<TableCheckerCheckItems>();
                _optionList[s].Add(_checkList.List[i]);
            }

            if (_results == null)
                _results = new List<TableCheckerResult>();
            _groupedResult = new Dictionary<string, List<TableCheckerResult>>();
        }

        #endregion

        #region Filter

        void CreateFilterUI() {
            VisualElement container = new VisualElement();
            rootVisualElement.Add(container);

            container.style.flexGrow = 0;
            container.style.flexShrink = 0;
            container.style.width = 256;

            Toolbar bar = new Toolbar();
            container.Add(bar);

            ToolbarButton buttonOpenLocal = new ToolbarButton();
            buttonOpenLocal.text = "打开本地";
            buttonOpenLocal.clicked += () => System.Diagnostics.Process.Start($@"{Application.dataPath}\Editor\TDTools\TableChecker\TableCheckerLocalWhiteList.txt");
            bar.Add(buttonOpenLocal);

            ToolbarButton buttonOpenProject = new ToolbarButton();
            buttonOpenProject.text = "打开项目";
            buttonOpenProject.clicked += () => System.Diagnostics.Process.Start($@"{Application.dataPath}\Editor\TDTools\TableChecker\TableCheckerProjectWhiteList.txt");
            bar.Add(buttonOpenProject);

            ToolbarButton buttonReloadFilter = new ToolbarButton();
            buttonReloadFilter.text = "重载";
            buttonReloadFilter.clicked += ()=>{
                _localFilterSet = new FilterSet($@"{Application.dataPath}\Editor\TDTools\TableChecker\TableCheckerLocalWhiteList.txt");
                _projectFilterSet = new FilterSet($@"{Application.dataPath}\Editor\TDTools\TableChecker\TableCheckerProjectWhiteList.txt");
                RepaintFilter();
            };
            bar.Add(buttonReloadFilter);

            ToolbarToggle buttonReverseLocal = new ToolbarToggle();
            buttonReverseLocal.text = "反转本地";
            buttonReverseLocal.RegisterValueChangedCallback(obj => { 
                _localFilterSet.Reversed = obj.newValue;
                RepaintResult();
            });
            bar.Add(buttonReverseLocal);

            ToolbarToggle buttonReverseProject = new ToolbarToggle();
            buttonReverseProject.text = "反转项目";
            buttonReverseProject.RegisterValueChangedCallback(obj => {
                _projectFilterSet.Reversed = obj.newValue;
                RepaintResult();
            });
            bar.Add(buttonReverseProject);

            VisualElement filterContainer = new VisualElement();
            filterContainer.style.flexShrink = 0;
            filterContainer.style.flexGrow = 100;
            container.Add(filterContainer);

            _filterScrollView = new ScrollView();

            filterContainer.Add(_filterScrollView);

            RepaintFilter();
        }

        void SaveFilters() {
            _localFilterSet.Save();
            _projectFilterSet.Save();
        }

        void RepaintFilter() {
            _filterScrollView.Clear();

            void PaintFilter(FilterSet filter, VisualElement root) {
                foreach (var pair in filter.RowFilters) {

                    Foldout foldout = new Foldout();
                    foldout.text = pair.Key;
                    foldout.style.marginLeft = 8;
                    //foldout.style.fontSize = 16;
                    root.Add(foldout);

                    for (int i = 0; i < pair.Value.Count; i++) {
                        RowFilter f = pair.Value[i];

                        VisualElement container = new VisualElement();
                        container.style.flexDirection = FlexDirection.Row;
                        foldout.Add(container);

                        container.RegisterCallback<ContextClickEvent>(obj => {
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("删除项"), false, Delete);

                            void Delete() {

                                //Delete 项目
                                pair.Value.Remove(f);

                                SaveFilters();
                                RepaintFilter();
                                RepaintResult();
                            }

                            menu.ShowAsContext();
                        });

                        Toggle toggle = new Toggle();
                        toggle.SetValueWithoutNotify(pair.Value[i].Enabled);
                        toggle.RegisterValueChangedCallback(obj => {
                            f.Enabled = obj.newValue;
                            RepaintResult();
                        });
                        container.Add(toggle);

                        Label table = new Label();
                        table.text = pair.Key;
                        table.style.width = 64;
                        table.style.flexGrow = 0;
                        table.style.flexShrink = 0;
                        table.style.overflow = Overflow.Hidden;
                        container.Add(table);

                        Label rowMin = new Label();
                        rowMin.text = pair.Value[i].Min.ToString();
                        rowMin.style.width = 64;
                        rowMin.style.flexGrow = 0;
                        rowMin.style.flexShrink = 0;
                        rowMin.style.marginLeft = 4;
                        rowMin.style.unityTextAlign = TextAnchor.MiddleCenter;
                        container.Add(rowMin);

                        Label rowMax = new Label();
                        rowMax.text = pair.Value[i].Max.ToString();
                        rowMax.style.width = 64;
                        rowMax.style.flexGrow = 0;
                        rowMax.style.flexShrink = 0;
                        rowMax.style.unityTextAlign = TextAnchor.MiddleCenter;
                        container.Add(rowMax);
                    }
                }
            }

            void PaintColumnFilter(FilterSet filter, VisualElement root) {
                foreach (var pair in filter.ColumnFilters) {

                    Foldout foldout = new Foldout();
                    foldout.text = pair.Key;
                    foldout.style.marginLeft = 8;
                    //foldout.style.fontSize = 16;
                    root.Add(foldout);

                    for (int i = 0; i < pair.Value.Count; i++) {
                        ColumnFilter f = pair.Value[i];

                        VisualElement container = new VisualElement();
                        container.style.flexDirection = FlexDirection.Row;
                        foldout.Add(container);

                        container.RegisterCallback<ContextClickEvent>(obj => {
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("删除项"), false, Delete);

                            void Delete() {

                                //Delete 项目
                                pair.Value.Remove(f);

                                SaveFilters();
                                RepaintFilter();
                                RepaintResult();
                            }

                            menu.ShowAsContext();
                        });

                        Toggle toggle = new Toggle();
                        toggle.SetValueWithoutNotify(pair.Value[i].Enabled);
                        toggle.RegisterValueChangedCallback(obj => {
                            f.Enabled = obj.newValue;
                            RepaintResult();
                        });
                        container.Add(toggle);

                        Label table = new Label();
                        table.text = pair.Key;
                        table.style.width = 64;
                        table.style.flexGrow = 0;
                        table.style.flexShrink = 0;
                        table.style.overflow = Overflow.Hidden;
                        container.Add(table);

                        Label rowMin = new Label();
                        rowMin.text = f.Column;
                        rowMin.style.width = 64;
                        rowMin.style.flexGrow = 0;
                        rowMin.style.flexShrink = 0;
                        rowMin.style.marginLeft = 4;
                        rowMin.style.unityTextAlign = TextAnchor.MiddleCenter;
                        container.Add(rowMin);

                        Label rowMax = new Label();
                        rowMax.text = f.Value;
                        rowMax.style.width = 64;
                        rowMax.style.flexGrow = 0;
                        rowMax.style.flexShrink = 0;
                        rowMax.style.unityTextAlign = TextAnchor.MiddleCenter;
                        container.Add(rowMax);
                    }
                }
            }

            Foldout localFoldout = new Foldout();
            Foldout projectFoldout = new Foldout();
            localFoldout.text = "本地白名单";
            //localFoldout.style.fontSize = 16;
            projectFoldout.text = "项目白名单";
            //projectFoldout.style.fontSize = 16;

            _filterScrollView.Add(localFoldout);
            _filterScrollView.Add(projectFoldout);


            PaintFilter(_localFilterSet, localFoldout);
            PaintColumnFilter(_localFilterSet, localFoldout);
            PaintFilter(_projectFilterSet, projectFoldout);
            PaintColumnFilter(_projectFilterSet, projectFoldout);

        }

        #endregion

        #region Result

        void CreateResultUI() {
            VisualElement resultContainer = new VisualElement();
            resultContainer.style.flexGrow = 100;
            rootVisualElement.Add(resultContainer);

            Toolbar bar = new Toolbar();
            bar.style.flexShrink = 0;
            resultContainer.Add(bar);

            ToolbarButton openButton = new ToolbarButton();
            openButton.text = "打开结果";
            openButton.clicked += LoadFromTSV;
            bar.Add(openButton);

            ToolbarButton saveButton = new ToolbarButton();
            saveButton.text = "保存结果";
            saveButton.clicked += SaveToTSV;
            bar.Add(saveButton);

            ToolbarButton groupByTableButton = new ToolbarButton();
            groupByTableButton.text = "以表分组显示";
            groupByTableButton.clicked += () => ChangeResultMode(ResultDisplayMode.GroupByTable);
            bar.Add(groupByTableButton);

            ToolbarButton groupByImportanceButton = new ToolbarButton();
            groupByImportanceButton.text = "以重要度分组显示";
            groupByImportanceButton.clicked += () => ChangeResultMode(ResultDisplayMode.GroupByImportance);
            bar.Add(groupByImportanceButton);

            ToolbarButton groupByResponsibility = new ToolbarButton();
            groupByResponsibility.text = "以职责分组显示";
            groupByResponsibility.clicked += () => ChangeResultMode(ResultDisplayMode.GroupByResponsbility);
            bar.Add(groupByResponsibility);

            Toggle toggleShowSuccess = new Toggle();
            toggleShowSuccess.text = "不显示通过项";
            toggleShowSuccess.value = !_showImportance0;
            toggleShowSuccess.RegisterValueChangedCallback(obj => {
                _showImportance0 = !obj.newValue;
                RepaintResult();

            });
            bar.Add(toggleShowSuccess);

            Toggle toggleShowLowPro = new Toggle();
            toggleShowLowPro.text = "不显示低优先级";
            toggleShowLowPro.value = !_showLowImportance;
            toggleShowLowPro.RegisterValueChangedCallback(obj => {
                _showLowImportance = !obj.newValue;
                RepaintResult();

            });
            bar.Add(toggleShowLowPro);

            ToolbarPopupSearchField searchField = new ToolbarPopupSearchField();
            searchField.RegisterValueChangedCallback(SetSearch);
            bar.Add(searchField);

            _labelTotalResult = new Label();
            _labelTotalResult.style.display = DisplayStyle.None;
            bar.Add(_labelTotalResult);

            _labelFilteredCount = new Label();
            _labelFilteredCount.style.display = DisplayStyle.None;
            bar.Add(_labelFilteredCount);

            CreateProgressBar(resultContainer);
            _resultScrollView = new ScrollView();
            resultContainer.Add(_resultScrollView);

            RepaintResult();
        }

        void SetSearch(ChangeEvent<string> e) {
            if (_results.Count == 0)
                return;
            _searchString = e.newValue.ToLower();
            RepaintResult();
        }
        async void ShowProgress(int id) {
            _progressContainer.style.display = DisplayStyle.Flex;
            try {
                var p = Progress.GetProgressById(id);
                while (p != null && !p.finished && p.running) {
                    progressBar.value = p.progress * 100;
                    await Task.Yield();
                }
            } catch { 
            }
            _progressContainer.style.display = DisplayStyle.None;
        }

        void CreateProgressBar(VisualElement root) {
            _progressContainer = new VisualElement();
            root.Add(_progressContainer);
            _progressContainer.style.flexShrink = 0;
            _progressContainer.style.flexGrow = 0;
            _progressContainer.style.height = 16;
            _progressContainer.style.display = DisplayStyle.None;

            progressBar = new ProgressBar();
            progressBar.title = "进度";
            _progressContainer.Add(progressBar);


            //Progress.added += ShowProgress;

            //async void ShowProgress(Progress.Item[] items) {
            //    _progressContainer.visible = true;
            //    while (Progress.running) {
            //        progressBar.value = Progress.globalProgress * 100;
            //        await Task.Yield();
            //    }
            //    _progressContainer.visible = false;
            //}

        }

        /// <summary>
        /// 更换显示模式并且重新显示
        /// </summary>
        /// <param name="mode"></param>
        private void ChangeResultMode(ResultDisplayMode mode) {
            _resultDisplayMode = mode;
            _foldoutValues.Clear();
            RepaintResult();
        }

        public void AddToLocalColumn(string table, ColumnFilter filter) {
            if (!_localFilterSet.ColumnFilters.ContainsKey(table))
                _localFilterSet.ColumnFilters[table] = new List<ColumnFilter>();
            _localFilterSet.ColumnFilters[table].Add(filter);
            RepaintFilter();
            SaveFilters();
            RepaintResult();
        }

        public void AddToProjectColumn(string table, ColumnFilter filter) {
            if (!_projectFilterSet.ColumnFilters.ContainsKey(table))
                _projectFilterSet.ColumnFilters[table] = new List<ColumnFilter>();
            _projectFilterSet.ColumnFilters[table].Add(filter);
            RepaintFilter();
            SaveFilters();
            RepaintResult();
        }

        /// <summary>
        /// 清除目前现实的结果，并且重新将Results里面的结果显示
        /// </summary>
        private void RepaintResult() {
            _resultScrollView.Clear();
            _groupedResult.Clear();

            int filteredCount = 0;

            ///分组
            for (int i = 0; i < _results.Count; i++) {
                var result = _results[i];
                if (_searchString != "") {
                    if (!result.Importantce.ToLower().Contains(_searchString)
                        && (result.Responsibility == null || !result.Responsibility.ToLower().Contains(_searchString))
                        && (result.Table == null || !result.Table.ToLower().Contains(_searchString))
                        && (result.Column == null || !result.Column.ToLower().Contains(_searchString))
                        && (result.Description == null || !result.Description.ToLower().Contains(_searchString))
                        && (result.RowContent == null || !result.RowContent.ContainsKey(_searchString))
                        && (result.RowContent == null || !result.RowContent.ContainsValue(_searchString))) {
                        filteredCount++;
                        continue;
                    }
                }

                if (result.Importantce.CompareTo("0") != 0 && _localFilterSet.Match(result)) {
                    filteredCount++;
                    continue;
                }

                if (result.Importantce.CompareTo("0") != 0 && _projectFilterSet.Match(result)) {
                    filteredCount++;
                    continue;
                }

                if (!_showLowImportance) {
                    int imp = int.Parse(result.Importantce);
                    if (imp < 4)
                        continue;
                }

                if (result.Importantce.CompareTo("0") != 0 || _showImportance0) {
                    switch (_resultDisplayMode) {
                        case ResultDisplayMode.GroupByTable:
                            if (!_groupedResult.ContainsKey(result.Table))
                                _groupedResult[result.Table] = new List<TableCheckerResult>();
                            _groupedResult[result.Table].Add(result);
                            break;

                        case ResultDisplayMode.GroupByImportance:
                            if (!_groupedResult.ContainsKey(result.Importantce.ToString()))
                                _groupedResult[result.Importantce] = new List<TableCheckerResult>();
                            _groupedResult[result.Importantce.ToString()].Add(result);
                            break;

                        case ResultDisplayMode.GroupByResponsbility:
                            if (!_groupedResult.ContainsKey(result.Responsibility))
                                _groupedResult[result.Responsibility] = new List<TableCheckerResult>();
                            _groupedResult[result.Responsibility].Add(result);
                            break;
                    }
                }
            }

            int totalCount = 0;

            ///将dictionary 以key排序，主要是应对 以重要度为组分类 的情况
            foreach (var pair in _groupedResult.OrderBy(x => x.Key)) {
                string key = pair.Key;
                totalCount += _groupedResult[pair.Key].Count;

                Foldout foldout = new Foldout {
                    text = key,
                    value = _searchString != ""
                };
                if (_foldoutValues.ContainsKey(key)) {
                    foldout.value = foldout.value || _foldoutValues[key];
                } else {
                    _foldoutValues[key] = foldout.value;
                }
                _resultScrollView.Add(foldout);

                foldout.RegisterValueChangedCallback(obj => {
                    _foldoutValues[key] = obj.newValue;
                });
                pair.Value.Sort();

                VisualElement makeItem() => AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/TableConfigChecker/TableCheckerUI/TableCheckerResultListItem.uxml").Instantiate();

                void bindItem(VisualElement e, int i) {
                    TableCheckerResult result = pair.Value[i];

                    e.Q<Label>("Importance").text = result.Importantce.ToString();
                    e.Q<Label>("Table").text = result.Table;
                    e.Q<Label>("Column").text = result.Column;
                    e.Q<Label>("Row").text = result.Row;
                    e.Q<Label>("Responsibility").text = result.Responsibility;
                    e.Q<Label>("Des").text = result.Description;
                    e.RegisterCallback<ContextClickEvent>(obj => {
                        if (e.Q<Label>("Importance").text.Equals("0"))
                            return;
                        try {
                            GenericMenu menu = new GenericMenu();
                            menu.AddDisabledItem(new GUIContent($"{result.Table}表第{result.Row}行"));
                            if (result.RowContent != null) {
                                menu.AddDisabledItem(new GUIContent($"{result.Column}列值{result.RowContent[result.Column]}"));
                                menu.AddItem(new GUIContent($"显示行内容"), false, PrintResult);
                            }
                            menu.AddSeparator("");
                            //menu.AddItem(new GUIContent($"将{result.Row}行加入本地白名单"), false, AddToLocalRow);
                            //menu.AddItem(new GUIContent($"将{result.Row}行加入项目白名单"), false, AddToProjectRow);
                            menu.AddItem(new GUIContent($"将{result.Row}行加入白名单"), false, () => { FilterPopupWindow.GetWindow(result.Table, result.Column, result.RowContent[result.Column], result.RowContent); });
                            menu.AddSeparator("");
                            if (result.RowContent != null) {
                                //menu.AddItem(new GUIContent($"将{result.Column}列值为{result.RowContent[result.Column]}的行加入本地白名单"), false, AddToLocalColumn);
                                //menu.AddItem(new GUIContent($"将{result.Column}列值为{result.RowContent[result.Column]}的行加入项目白名单"), false, AddToProjectColumn);
                            }
                            menu.ShowAsContext();
                        } catch (Exception e) {
                            //Debug.Log(e.Message);
                        }

                        void PrintResult() {
                            string s = "";
                            int length = 0;
                            foreach (var pair in result.RowContent) {
                                length += pair.Key.Length + pair.Value.Length;
                                if (length > 200) {
                                    length = 0;
                                    s = $"{s}\n\n";
                                }
                                s = $"{s}{pair.Key}:\"{pair.Value}\"    ";

                            }
                            EditorUtility.DisplayDialog($"{result.Table}表{result.Row}行", s, "确认");
                        }

                        //void AddToLocalRow() {
                        //    if (!_localFilterSet.RowFilters.ContainsKey(result.Table))
                        //        _localFilterSet.RowFilters[result.Table] = new List<RowFilter>();
                        //    _localFilterSet.RowFilters[result.Table].Add(new RowFilter() {
                        //        Min = int.Parse(result.Row),
                        //        Max = int.Parse(result.Row)
                        //    });
                        //    RepaintFilter();
                        //    SaveFilters();
                        //    RepaintResult();
                        //}

                        //void AddToProjectRow() {
                        //    if (!_projectFilterSet.RowFilters.ContainsKey(result.Table))
                        //        _projectFilterSet.RowFilters[result.Table] = new List<RowFilter>();
                        //    _projectFilterSet.RowFilters[result.Table].Add(new RowFilter() {
                        //        Min = int.Parse(result.Row),
                        //        Max = int.Parse(result.Row)
                        //    });
                        //    RepaintFilter();
                        //    SaveFilters();
                        //    RepaintResult();
                        //}

                        //void AddToLocalColumn() {
                        //    if (!_localFilterSet.ColumnFilters.ContainsKey(result.Table))
                        //        _localFilterSet.ColumnFilters[result.Table] = new List<ColumnFilter>();
                        //    _localFilterSet.ColumnFilters[result.Table].Add(new ColumnFilter() {
                        //        Column = result.Column,
                        //        Value = result.RowContent[result.Column]
                        //    });
                        //    RepaintFilter();
                        //    SaveFilters();
                        //    RepaintResult();
                        //}

                        //void AddToProjectColumn() {
                        //    if (!_projectFilterSet.ColumnFilters.ContainsKey(result.Table))
                        //        _projectFilterSet.ColumnFilters[result.Table] = new List<ColumnFilter>();
                        //    _projectFilterSet.ColumnFilters[result.Table].Add(new ColumnFilter() {
                        //        Column = result.Column,
                        //        Value = result.RowContent[result.Column]
                        //    });
                        //    RepaintFilter();
                        //    SaveFilters();
                        //    RepaintResult();
                        //}


                    });
                }

                ListView listview = new ListView(_groupedResult[pair.Key], LIST_HEIGHT, makeItem, bindItem);
                listview.style.flexGrow = 1.0f;
                listview.style.height = Mathf.Clamp(LIST_HEIGHT * 2, (pair.Value.Count + 1) * LIST_HEIGHT, LIST_HEIGHT * 32);
                ///双击跳转到指定表格
                listview.onItemsChosen += (IEnumerable<object> target) => {
                    EditorUtility.FocusProjectWindow();
                    foreach (var t in target) {
                        UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(((TableCheckerResult)t).Path);
                        Selection.activeObject = obj;
                        EditorGUIUtility.PingObject(obj);
                    }
                };

                listview.horizontalScrollingEnabled = true;

                foldout.Add(listview);
            }

            if (totalCount > 0) {
                _labelTotalResult.style.display = DisplayStyle.Flex;
                _labelTotalResult.text = $"结果数: {totalCount}";
            } else {
                _labelTotalResult.style.display = DisplayStyle.None;
            }

            if (filteredCount > 0) {
                _labelFilteredCount.style.display = DisplayStyle.Flex;
                _labelFilteredCount.text = $"被隐藏的项: {filteredCount}";
            } else {
                _labelFilteredCount.style.display = DisplayStyle.None;
            }
        }

        /// <summary>
        /// 清除目前显示的结果
        /// </summary>
        public void ClearResult() {
            _resultScrollView.Clear();
            _results.Clear();
            _groupedResult.Clear();
        }

        #endregion

        #region CheckList

        void CreateCheckListUI() {
            VisualElement checkListContainer = new VisualElement();
            checkListContainer.style.flexGrow = 1;
            checkListContainer.style.minWidth = 128;
            rootVisualElement.Add(checkListContainer);

            Toolbar bar = new Toolbar();
            bar.style.flexShrink = 0;
            checkListContainer.Add(bar);

            ToolbarButton buttonStart = new ToolbarButton();
            buttonStart.text = "开始检查";
            buttonStart.clicked += StartCheck;
            bar.Add(buttonStart);

            ToolbarButton buttonShowOnlyTables = new ToolbarButton();
            buttonShowOnlyTables.text = "显示表格";
            buttonShowOnlyTables.clicked += () => ChangeDisplayMode(CheckListDisplayMode.TableOnly);
            bar.Add(buttonShowOnlyTables);

            ToolbarButton buttonShowColunmn = new ToolbarButton();
            buttonShowColunmn.text = "显示列";
            buttonShowColunmn.clicked += () => ChangeDisplayMode(CheckListDisplayMode.TableAndColumn);
            bar.Add(buttonShowColunmn);

            ToolbarButton buttonShowDetail = new ToolbarButton();
            buttonShowDetail.text = "显示检查项";
            buttonShowDetail.clicked += () => ChangeDisplayMode(CheckListDisplayMode.DetailedMode);
            bar.Add(buttonShowDetail);

            Toggle toggleSelectAll = new Toggle();
            checkListContainer.Add(toggleSelectAll);
            toggleSelectAll.text = "全选";
            toggleSelectAll.RegisterValueChangedCallback(ctx => {
                List<string> tables = new List<string>(_tableToCheck.Keys);
                for (int i = 0; i < tables.Count; i++) {
                    ToggleTable(ctx.newValue, tables[i]);
                }
            });

            _checkListScrollView = new ScrollView();
            checkListContainer.Add(_checkListScrollView);

            RepaintList();
        }

        async void StartCheck() {
            _results.Clear();
            List<Task<List<TableCheckerResult>>> t = new List<Task<List<TableCheckerResult>>>();
            TableChecker.DATAPATH = Application.dataPath;
            int progressID = Progress.Start("表格检查...");
            ShowProgress(progressID);
            for (int i = 0; i < _checkList.List.Count; i++) {
                if (!_checkList.List[i].enabled)
                    continue;
                _checkList.List[i].parentID = progressID;
                t.Add(Task.Run(_checkList.List[i].Run));
            }

            for (int i = 0; i < t.Count; i++) {
                Progress.Report(progressID, i / (float)t.Count);
                try {
                    await t[i];
                    if (t[i].Result != null)
                        _results.AddRange(t[i].Result);
                } catch (Exception e) {
                    Debug.Log($"{_checkList.List[i].CheckType} {e.Message}");
                    var r = _checkList.List[i].Run();
                    if (r != null)
                        _results.AddRange(r);
                }
            }
            Progress.Finish(progressID);
            Progress.Remove(progressID);
            RepaintResult();
        }

        private void ChangeDisplayMode(CheckListDisplayMode mode) {
            _checkListDisplayMode = mode;
            RepaintList();
        }


        /// <summary>
        /// 点选整个表格
        /// </summary>
        /// <param name="value">启用与否</param>
        /// <param name="table">所属表格</param>
        public void ToggleTable(bool value, string table)
        {
            _tableToCheck[table] = value;
            List<string> list = new List<string>();
            foreach (var column in _columnToCheck[table])
            {
                list.Add(column.Key);
                foreach (var check in _optionList[$"{table}\t{column.Key}"])
                    check.enabled = value;
            }
            for (int i = 0; i < list.Count; i++)
                _columnToCheck[table][list[i]] = value;
            RepaintList();
        }

        /// <summary>
        /// 点选整列的所有检查
        /// </summary>
        /// <param name="value">启用与否</param>
        /// <param name="table">所属表格</param>
        /// <param name="column">所属列</param>
        public void ToggleColumn(bool value, string table, string column) {
            _columnToCheck[table][column] = value;
            foreach (var check in _optionList[$"{table}\t{column}"])
                check.enabled = value;
            RepaintList();
        }

        /// <summary>
        /// 重新绘制列表
        /// </summary>
        void RepaintList() {
            _checkListScrollView.Clear();
            foreach (var table in _tableToCheck)
            {
                Toggle toggle = new Toggle {
                    text = table.Key,
                    value = table.Value
                };
                toggle.RegisterValueChangedCallback(obj => ToggleTable(obj.newValue, table.Key));
                toggle.style.marginLeft = 16;
                _checkListScrollView.Add(toggle);
                if (_checkListDisplayMode == CheckListDisplayMode.TableAndColumn || _checkListDisplayMode == CheckListDisplayMode.DetailedMode) {
                    foreach (var column in _columnToCheck[table.Key])
                    {
                        toggle = new Toggle {
                            text = column.Key,
                            value = column.Value
                        };
                        toggle.RegisterValueChangedCallback(obj => ToggleColumn(obj.newValue, table.Key, column.Key));
                        toggle.style.marginLeft = 32;
                        _checkListScrollView.Add(toggle);
                        if (_checkListDisplayMode == CheckListDisplayMode.DetailedMode) {
                            foreach (var check in _optionList[table.Key + '\t' + column.Key])
                            {
                                toggle = new Toggle {
                                    text = check.CheckType,
                                    value = check.enabled
                                };
                                toggle.RegisterValueChangedCallback(obj => check.enabled = obj.newValue);
                                toggle.style.marginLeft = 48;
                                _checkListScrollView.Add(toggle);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Save Load

        /// <summary>
        /// 将结果保存成一个TSV方便以后查看或者在读表工具中查看
        /// </summary>
        public void SaveToTSV() {
            string path = EditorUtility.SaveFilePanel("选择保存位置", "/Assets/Editor/TableConfigChecker/Results", "表格检查结果存档" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt", "txt");
            try {
                StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Unicode);

                foreach (var result in _results) {
                    if (_showImportance0 || !result.Importantce.Equals("0"))
                        sw.Write(result.Importantce.ToString() + "\t" + result.Table + "\t" + result.Row + "\t" + result.Column + "\t" + result.Responsibility + "\t" + result.Description + "\n");
                }

                sw.Close();
            } catch (Exception e) {
                Debug.Log(e.Message);
            }
        }

        /// <summary>
        /// 从一个TSV中读取以前保存的结果
        /// </summary>
        public void LoadFromTSV() {
            string path = EditorUtility.OpenFilePanel("选择过去的结果文件", "/Assets/Editor/TableConfigChecker/Results", "txt");
            ClearResult();

            try {
                StreamReader sr = new StreamReader(path);
                while (!sr.EndOfStream) {
                    string line = sr.ReadLine();
                    string[] args = line.Split('\t');
                    TableCheckerResult result = new TableCheckerResult(args);
                    _results.Add(result);
                }
            } catch (Exception e) {
                Debug.Log(e.Message);
            }

            RepaintResult();
        }

        #endregion
    }
}
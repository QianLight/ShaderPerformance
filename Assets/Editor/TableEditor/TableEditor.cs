using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.Xml;
using System.IO;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TableEditor
{
    [System.Serializable]
    class TableEditor : EditorWindow
    {
        public static TableEditor Instance { get; set; }
        public static string XmlPath = "/Editor/TableEditor/TableConfig/";
        public static string TableConfigPath = "/Editor/TableEditor/TableConfig/TableConfig.xml";
        public static string TableKeyConfigPath = "/Editor/TableEditor/TableConfig/TableKey.xml";
        public static string TablePath = "Assets/Table/";

        private struct Drawing
        {
            public Rect Rect;
            public Action Draw;
        }
        public string curFilePath;
        public TableFullData curFullData = new TableFullData();
        public TableFullData curShowData = new TableFullData();
        public Dictionary<string, int> curFlagDic = new Dictionary<string, int>(); //这个是进行逻辑操作时查找的临时变量,tempFullData中的fullflagdic只在写回去时用到
        public Dictionary<string, int> tempFlagDic = new Dictionary<string, int>(); //这个是进行逻辑操作时查找的临时变量
        public TableConfig fullConfig = new TableConfig();
        public TableConfigData curConfig;
        public List<ActiveTable> activeTable = new List<ActiveTable>();
        public int activeTableIndex;
        public List<int> curKeyIndex;
        private string curTableName = "";
        private int tagListNum = 0;
        public string curSearchWord = string.Empty;
        private bool isShowModifiedLine;
        public Action InvokeAction;
        public float delayTime = 0;
        public float timer = 0;
        public bool isStartTimer = false;

        //public TableFullData tempData=new TableFullData();//此数据保存至清空缓存之前所有修改 每次保存单条数据之后重新写入temp文件
        public TableTempFullData tempFullData = new TableTempFullData();
        public TableData cacheData;

        VisualElement RootContainer;
        VisualElement ToolbarContainer;
        VisualElement TagBoxContainer;
        ListView TableListView;

        [MenuItem("Window/Table")]
        public static void ShowExample()
        {
            TableEditor window = GetWindow<TableEditor>();
            window.titleContent = new GUIContent("TableEditor");
            window.Show();
            Instance = window;
        }
        public void OnEnable()
        {
            // Each editor window contains a root VisualElement object
            rootVisualElement.style.flexDirection = FlexDirection.Column;

            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/TableEditor/UI/TableEditor_UXML.uxml");
            rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/TableEditor/UI/TableEditor_USS.uss"));

            asset.CloneTree(rootVisualElement);

            rootVisualElement.Q<ToolbarSearchField>("serachField").RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                curSearchWord = evt.newValue;
                Search(curSearchWord);
            });
            BindButton("Load-button", () => { LoadTableDataWindow(); } );
            BindButton("Save-button", SaveValidate);
            BindButton("CreateConfig-button", CreateConfig);
            BindButton("AddLine-button", () => { AddLineData(); }); //addlinedata有一个参数，所以用lambda包一层才能传入BindButton
            BindButton("ShowModified-button", ShowModifiedLine);
            BindButton("ClearAllTemp-button", ClearAllTemp);
            BindButton("ClearTemp-button", ClearSingleTemp); ;
            BindButton("CheckConflict-button", CheckConflict); ;

            ToolbarContainer = new VisualElement();
            rootVisualElement.Add(ToolbarContainer);

            var toolbar = new Toolbar() { name= "toolbar1" };
            ToolbarContainer.Add(toolbar);
            toolbar.AddToClassList("toolbar1");

            RootContainer = new VisualElement();
            RootContainer.AddToClassList("RootContainer");
            rootVisualElement.Add(RootContainer);
            ReadConfig();
            if(!string.IsNullOrEmpty(curTableName))
            {
                LoadTableData(GetTableFilePath(curTableName));
            }
        }
        private void Update()
        {
            Instance = this;
            if(isStartTimer)
            {
                timer += Time.deltaTime;
                if (timer > delayTime)
                {
                    rootVisualElement.schedule.Execute(InvokeAction);
                    isStartTimer = false;
                }
            }
        }

        #region DrawUI
        public void DrawGUI()
        {
            DrawTableNameBar();
            DrawTitle();
            DrawTableData();
        }

        #region TableToolbar

        private void DrawTableNameBar()
        {
            
            Toolbar toolbar= ToolbarContainer.Query<Toolbar>("toolbar1").First();
            toolbar.Clear();

            foreach (var i in activeTable)
            {
                TextElement text = new TextElement() { };
                if(!i.isSaveChange)
                    text.text = i.tableName+"(*)";
                else
                    text.text = i.tableName;
                text.name = "text";
                text.AddToClassList("toolbar_text");

                string filePath = GetTableFilePath(i.tableName);

                var btn1 = new ToolbarButton(() => { 
                    LoadTableData(filePath);
                    if (isShowModifiedLine)
                        ShowModifiedLine();
                }) { name = i.tableName };

                btn1.Add(text);
                btn1.RegisterCallback<ContextClickEvent>(ToolbarButtonMenuOption);
                toolbar.Add(btn1);
            }

            ChangeTablebarButtonStyle();
        }
        private void ChangeTablebarButtonStyle()
        {
            foreach(var i in activeTable)
            {
                ToolbarButton toolbarButton = ToolbarContainer.Query<ToolbarButton>(i.tableName).First();
                if (i.tableName != curTableName)
                {
                    toolbarButton.RemoveFromClassList("toolbarButton_active");
                    toolbarButton.AddToClassList("toolbarButton_inactive");
                }
                else
                {
                    toolbarButton.RemoveFromClassList("toolbarButton_inactive");
                    toolbarButton.AddToClassList("toolbarButton_active");
                }
            }
        }

        public void SetTablebarButtonChanged(string tableName,bool isSaveChanged)
        {
            ToolbarButton toolbarButton = ToolbarContainer.Query<ToolbarButton>(tableName).First();
            TextElement text = toolbarButton.Query<TextElement>("text").First();

            foreach(var i in activeTable)
            {
                if(i.tableName==tableName)
                {
                    i.isSaveChange = isSaveChanged;
                    if (!i.isSaveChange)
                        text.text = i.tableName + "(*)";
                    else
                        text.text = i.tableName;
                }
            }
        }
        #endregion
        private void DrawTitle()
        {
            //rootVisualElement.Query<Label>("openFileName").First().text = "当前打开文件：" + curTableName;
            if (TagBoxContainer == null)
            {
                TagBoxContainer = new VisualElement();
                TagBoxContainer.AddToClassList("TagBoxContainer");
                RootContainer.Add(TagBoxContainer);
            }
            else
            {
                TagBoxContainer.Clear();
            }
            VisualElement TagContainer = new VisualElement();
            TagContainer.AddToClassList("TagContainer");
            for (int i = 0; i < tagListNum; i++)
            {
                string tagName = curConfig.tagList[i];
                if (curFullData.nameList.Contains(tagName))
                {
                    var box1 = new VisualElement();
                    box1.AddToClassList("TagBoxContainer");
                    var label = new Label(tagName) { name = $"tag{i}" };
                    label.tooltip = curFullData.noteList[FindColIndexByColName(tagName)];
                    box1.Add(label);
                    TagContainer.Add(box1);
                }
            }
            var box2 = new VisualElement();
            box2.AddToClassList("TagBoxContainer");
            var label1 = new Label("");
            box2.Add(label1);
            TagContainer.Add(box2);

            TagBoxContainer.Add(TagContainer);
            
        }

        private void DrawTableData()
        {
            if (curShowData == null)
                return;

            Func<VisualElement> makeItem = () =>
            {
                var box = new VisualElement() { name = "box" };
                box.AddToClassList("rowContainer");
                for (int i = 0; i < tagListNum; i++)
                {
                    var box1 = new VisualElement();
                    box1.AddToClassList("itemsContainer");
                    Label label = new Label() { name = $"Label{i}" };
                    label.RegisterCallback<ContextClickEvent>(LabelMenuOption);
                    box1.Add(label);
                    //box1.Add(new TextField() { name = $"TextField{i}" });
                    box.Add(box1);
                }

                var box2 = new VisualElement();
                box2.AddToClassList("itemsContainer");
                box2.Add(new Button() { text = "Copy", name = "copy_Button" });
                if(!isShowModifiedLine)
                    box2.Add(new Button() { text = "Insert Behind", name = "insert_Button" });
                //box2.Add(new Button() { text = "Delete", name = "delete_Button" });
                box.Add(box2);
                return box;
            };

            Action<VisualElement, int> bindItem = (e, i) => {
                if(i<curShowData.dataList.Count)
                {
                    string keyName = curShowData.dataList[i].keyName;
                    int index = FindCurDataIndex(keyName);
                    //if(index<0)
                    //{
                    //    e.Query<Button>("delete_Button").First().RemoveFromHierarchy();
                    //}
                    for (int j = 0; j < tagListNum; j++)
                    {
                        string tagName = curConfig.tagList[j];
                        if (curFullData.nameList.Contains(tagName))
                        {
                            var index1 = curFullData.nameList.IndexOf(tagName);
                            e.Query<Label>($"Label{j}").First().text = curShowData.dataList[i].valueList[index1];
                        }
                    }
                    //Debug.Log(i);

                    e.Query<Button>("copy_Button").First().clickable.clicked += () => {
                        if (index >= 0)
                            cacheData = curFullData.dataList[index];
                        else cacheData = tempDataToCurData(tempFullData.tempDataList[i]); //支持复制已删除的数据
                        //Debug.Log(index);

                        ShowNotice("复制成功！");
                    };
                    //if(index>=0)
                    //{
                    //    e.Query<Button>("delete_Button").First().clickable.clicked += () => {
                    //        DeleteData(FindCurDataIndex(keyName));
                    //        Debug.Log(keyName);
                    //    };
                    //}
                    if(!isShowModifiedLine)
                        e.Query<Button>("insert_Button").First().clickable.clicked += () => { AddLineData(FindCurDataIndex(keyName) + 1); }; //Debug.Log(keyName); 
                    rootVisualElement.schedule.Execute(() => {
                        ChangeLineStyle(e, i);
                        //e.Query<ListView>().First().Refresh();
                    });
                }
            };

            if(TableListView ==null)
            {
                TableListView = new ListView(curShowData.dataList, 30, makeItem, bindItem);
                RootContainer.Add(TableListView);
            }
            else
            {
                TableListView.Rebuild();
                if (activeTable[activeTableIndex].scrollerVale > 0)
                {
                    Scroller scroller = TableListView.Query<Scroller>().Last();
                    scroller.value = activeTable[activeTableIndex].scrollerVale;

                    //TableListView.Refresh();需要一定时间去响应，此时scroller的highVale还是之前那张表格的，
                    //当赋给scroller.value比他highValue大的值的时候会截取到highValue，所以此时的跳转等于失效了。
                    //要等TableListView.Refresh();响应完后才能生效，所以我这里延迟了0.0000001赋值highValue
                    //并且用popupWindow遮挡一下listView以免显得很奇怪
                    if (scroller.highValue< activeTable[activeTableIndex].scrollerVale)
                    {
                        UnityEngine.UIElements.PopupWindow popupWindow = new UnityEngine.UIElements.PopupWindow();
                        rootVisualElement.Add(popupWindow);
                        float height = TableListView.contentRect.height;
                        popupWindow.style.height = height+100;
                        setInvokeAction(() => {
                            rootVisualElement.Remove(popupWindow);
                            scroller.value = activeTable[activeTableIndex].scrollerVale;
                        }, 0.0000001f);
                    }      
                }
                else
                {
                    Scroller scroller = TableListView.Query<Scroller>().Last();
                    scroller.value = 0;
                }  
            }

            TableListView.name = curTableName;
            TableListView.AddToClassList("ListView");

            InitListView(TableListView);

            //if (activeTable[curTableName].lineInfo > 0)
            //{
            //    int i = activeTable[curTableName].lineInfo;
            //
            //    setInvokeAction(() => { TableListView.ScrollToItem(i); }, 0.00001f);
            //}
        }

        public void RefreshListView()
        {
            TableListView.Rebuild();
        }
        private void InitListView(ListView listView)
        {
            listView.selectionType = SelectionType.Single;

            listView.onSelectionChange += (objects) =>
            {
                int index = listView.selectedIndex;
                string keyName = curShowData.dataList[index].keyName;
                EditSelectData(curShowData.dataList[index], FindCurDataIndex(keyName));
                //Debug.Log(listView.selectedIndex);
            };
        }     
        private void ChangeLineStyle(VisualElement e, int lineInfo)
        {
            bool isModified = false;
            //一定要先remove，因为listview的原理是复用元素，所以会导致有些还保留着原来的样式
            e.Q<VisualElement>("box").RemoveFromClassList("rowContainer_Green");
            e.Q<VisualElement>("box").RemoveFromClassList("rowContainer_Red");
            e.Q<VisualElement>("box").RemoveFromClassList("rowContainer_Grey");

            if(lineInfo<curShowData.dataList.Count)
            {
                string keyName = curShowData.dataList[lineInfo].keyName;
                if (tempFlagDic.ContainsKey(keyName))
                {
                    int tempIndex = tempFlagDic[keyName];
                    if (tempFullData.tempDataList[tempIndex].type == TableTempFullData.TempType[0])
                        e.Q<VisualElement>("box").AddToClassList("rowContainer_Green");
                    else if (tempFullData.tempDataList[tempIndex].type == TableTempFullData.TempType[1])
                    {
                        e.Q<VisualElement>("box").AddToClassList("rowContainer_Red");
                        isModified = true;
                    }
                    else if (tempFullData.tempDataList[tempIndex].type == TableTempFullData.TempType[2])
                    {
                        e.Q<VisualElement>("box").AddToClassList("rowContainer_Green");
                    }
                    else if (tempFullData.tempDataList[tempIndex].type == TableTempFullData.TempType[3])
                    {
                        e.Q<VisualElement>("box").AddToClassList("rowContainer_Grey");
                    }
                }
                else
                {
                    e.Q<VisualElement>("box").AddToClassList("rowContainer");
                }
                for (int j = 0; j < tagListNum; j++)
                {
                    string tagName = curConfig.tagList[j];
                    if (curFullData.nameList.Contains(tagName))
                    {
                        var index = curFullData.nameList.IndexOf(tagName);
                        if (isModified)
                        {
                            int tempIndex = tempFlagDic[keyName];
                            for (int k = 0; k < tempFullData.tempDataList[tempIndex].ModifyCol.Count; k++)
                            {
                                if (tagName == tempFullData.tempDataList[tempIndex].ModifyCol[k])
                                    e.Q<Label>($"Label{j}").AddToClassList("redFont");
                            }
                        }
                        else e.Q<Label>($"Label{j}").RemoveFromClassList("redFont");
                    }
                }
            }
        }
            
        #endregion

        #region Load Table
        public void LoadTableDataWindow()
        {
            TableDataLoader.InitWindow();
        }
        public void LoadTableData(string filePath)
        {
            activeTableIndex = FindActiveTableIndex(curTableName);

            if(!string.IsNullOrEmpty(curTableName)&&TableListView !=null)
            {
                Scroller scroller = TableListView.Query<Scroller>().Last();
                activeTable[activeTableIndex].scrollerVale = scroller.value;
            }
            curFilePath = filePath;
           

            if (!string.IsNullOrEmpty(curFilePath))
            {
                curFullData.Reset();//防止切表时报错
                curShowData.Reset();//防止切表时报错
                TableReader.ReadTableByFileStream(curFilePath, ref curFullData);
                cacheData = new TableData();
                LoadConfig();
                LoadCurFullData();
                LoadTempFullData();

                //加载后把现有的tempdata放到curdata里
                MergeTempToCur();

                ChangeCurShowData (curSearchWord);

                TableDataEditor.Instance?.Close();

                //RefreshGUI();
                DrawGUI();
            }
        }
        private void LoadConfig()
        {
            curConfig = fullConfig.configList.Find((c) => c.tableName == curFullData.tableName.Replace(".txt", string.Empty));
            if (curConfig == null)
            {
                ShowNotification(new GUIContent("无相关配置 请先添加配置"), 3);
                curFullData.Reset();
                return;
            }
        }
        private void LoadCurFullData()
        {
            curTableName = curConfig.tableName;
            curKeyIndex = new List<int>();
            activeTableIndex = FindActiveTableIndex(curTableName);
            if (activeTableIndex < 0)
            {
                ActiveTable at = new ActiveTable(curTableName);
                at.tableName = curTableName;
                activeTable.Add(at);
                activeTableIndex = FindActiveTableIndex(curTableName);
                activeTable[activeTableIndex].isSaveChange = true;
            }

            if (curConfig.keyName.Contains("+"))
            {
                activeTable[activeTableIndex].isKeyOnly = false;
                string[] key = curConfig.keyName.Split('+');
                for(int i=0;i<key.Length;i++)
                {
                    curKeyIndex.Add(curFullData.nameList.IndexOf(key[i]));
                }
            }
            else
                curKeyIndex.Add(curFullData.nameList.IndexOf(curConfig.keyName));

            tagListNum = curConfig.tagList.Count;

           
            keyGenerator();
            //Debug.Log(activeTable[activeTableIndex].isKeyOnly);

            CurFlagDicRest();
        }
        private void LoadTempFullData()
        {
            if (tempFullData.tableName != curFullData.tableName)
            {
                tempFullData.Reset();
                tempFullData.tableName = curFullData.tableName;
            }
            if (!File.Exists(Application.dataPath + XmlPath + tempFullData.tableName))
            {
                DataIO.SerializeData<TableTempFullData>(Application.dataPath + XmlPath + tempFullData.tableName, tempFullData);
                return;
            }
            tempFullData = DataIO.DeserializeData<TableTempFullData>(Application.dataPath + XmlPath + tempFullData.tableName);
            tempFlagDicReset();
        }

        #endregion

        #region Save To Excel
        private void SaveValidate()
        {
            if (curFullData == null || string.IsNullOrEmpty(curFullData.tableName))
                ShowNotification(new GUIContent("未设置数据"), 3);
            else
            {
                //UpdataCurTempData();
                ImportToExcel();
            }
            SetTablebarButtonChanged(curTableName,true);
        }
        private void ImportToExcel()
        {
            if(TableReader.WriteTable(curFilePath, curFullData))
                ShowNotification(new GUIContent("成功写入表格"), 3);
        }
        #endregion

        #region CreateConfig
        private void CreateConfig()
        {
            TableConfigEditor.InitWindow();
        }
        #endregion

        #region EditData
        private void EditSelectData(TableData selectData, int lineInfo)
        {
            TableDataEditor.InitWindow();
            TableDataEditor.Instance.SetEditData(selectData, lineInfo);
        }
        public void SaveEditData(TableTempData data, int lineInfo)
        {
            //存到curFullData
            curFullData.dataList[lineInfo] = tempDataToCurData(data);
            keyGenerator();
            string keyName = curFullData.dataList[lineInfo].keyName;

            //存到orignData
            SaveOrignData(lineInfo);

            //存到tempFullData
            data.keyName= keyName;
            int tempIndex = FindTempDataIndex(keyName);

            if (tempIndex >= 0)
            {
                tempFullData.tempDataList[tempIndex] = data;
            }
            else
            {
                tempFullData.tempDataList.Add(data);
                tempFlagDic.Add(keyName, tempFlagDic.Count);
            }
            CurFlagDicRest();

        }

        // 修改、新增保存到一个temp文件
        public void WriteTemp()
        {
            FullFlagDicReset();
            if (tempFullData.tableName != curFullData.tableName)
            {
                tempFullData.Reset();
                tempFullData.tableName = curFullData.tableName;
            }
            if (!File.Exists(Application.dataPath + XmlPath + tempFullData.tableName))
            {
                DataIO.SerializeData<TableTempFullData>(Application.dataPath + XmlPath + tempFullData.tableName, tempFullData);
                return;
            }

            DataIO.SerializeData<TableTempFullData>(Application.dataPath + XmlPath + tempFullData.tableName, tempFullData);

            SetTablebarButtonChanged(curTableName,false);
        }
        //保存修改前的原数据，是为了作为merge以及检查冲突的依据
        private void SaveOrignData(int lineInfo)
        {
            string keyName = curFullData.dataList[lineInfo].keyName;
        }
        #endregion

        #region AddLine

        // index<0在最后增加数据 index>0为插入数据
        private void AddLineData(int index = -1)
        {
            if (curFullData == null || string.IsNullOrEmpty(curFullData.tableName))
            {
                ShowNotification(new GUIContent("未设置数据"), 3);
                return;
            }

            TableData temp = new TableData();
            for (var i = 0; i < curFullData.dataList[0].valueList.Count; i++)
            {
                temp.valueList.Add(string.Empty);
            }
            if (index < 0)
                index = curFullData.dataList.Count;

            TableDataEditor.InitWindow();
            TableDataEditor.Instance.SetEditData(temp, index);
        }
        public void AddData(int index)
        {
            int count = curFullData.dataList.Count;
            if (index >= count)
            {
                curFullData.dataList.Add(new TableData());
            }
            else curFullData.dataList.Insert(index, new TableData());

        }

        #endregion

        #region Search
        private void Search(string value)
        {
            ChangeCurShowData(value);
            DrawGUI();
        }
        public void ChangeCurShowData(string value)
        {
            //ShowModifiedLine时屏蔽搜索功能
            if (!isShowModifiedLine)
            {
                if (value == "")
                    curShowData = curFullData;
                else
                {
                    curShowData = new TableFullData();
                    for (int i = 0; i < curFullData.dataList.Count; i++)
                    {
                        for (int j = 0; j < tagListNum; j++)
                        {
                            string tagName = curConfig.tagList[j];
                            if (curFullData.nameList.Contains(tagName))
                            {
                                var index = curFullData.nameList.IndexOf(tagName);
                                if (curFullData.dataList[i].valueList[index].IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    curShowData.dataList.Add(curFullData.dataList[i]);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region ShowModified
        private void ShowModifiedLine()
        {
            if (curFullData == null || string.IsNullOrEmpty(curFullData.tableName))
            {
                ShowNotification(new GUIContent("未设置数据"), 3);
                return;
            }

            isShowModifiedLine = (isShowModifiedLine == true) ? false : true;

            if (isShowModifiedLine)
            {
                LoadTempFullData();
                curShowData = new TableFullData();

                for (int i = 0; i < tempFullData.tempDataList.Count; i++)
                {
                    curShowData.dataList.Add(tempDataToCurData(tempFullData.tempDataList[i]));
                }

            }
            else
            {
                curShowData = curFullData;
            }

            RootContainer.Remove(TableListView);
            TableListView = null;

            DrawGUI();
        }
        #endregion

        #region ClearTemp
        private void ClearAllTemp()
        {
            tempFullData.Reset();
            DirectoryInfo di = new DirectoryInfo(Application.dataPath + XmlPath);
            var files = di.GetFiles();
            foreach (var file in files)
            {
                if (file.Name.Contains(".meta") || file.Name.Contains(".xml"))
                    continue;
                tempFullData.tableName = file.Name;
                DataIO.SerializeData<TableTempFullData>(Application.dataPath + XmlPath + tempFullData.tableName, tempFullData);
            }
            LoadTableData(curFilePath);

            foreach (var i in activeTable)
            {
                i.isSaveChange = true;
            }
            DrawTableNameBar();
            ShowNotification(new GUIContent("清除成功！"), 3);
        }
        private void ClearSingleTemp()
        {
            TableTempDataWindow.InitWindow();
        }
        #endregion

        #region DeleteData
        public void DeleteData(int index)
        {
            TableTempData deleteData = new TableTempData();
            deleteData.type = TableTempFullData.TempType[3];
            deleteData.valueList = curFullData.dataList[index].valueList;
            deleteData.keyName = curFullData.dataList[index].keyName;
            deleteData.index = index;
            if (tempFlagDic.ContainsKey(deleteData.keyName))
                tempFullData.tempDataList[tempFlagDic[deleteData.keyName]] = deleteData;
            else
                tempFullData.tempDataList.Add(deleteData);

            SaveOrignData(index);
            curFullData.dataList.RemoveAt(index);

            tempFlagDicReset();
            CurFlagDicRest();
            FullFlagDicReset();
            WriteTemp();
            ShowNotice("删除成功！该数据可以从\"显示修改\"中恢复 ");
            RefreshListView();
        }
        //Clear已经删除过的temp数据
        public void ClearDeletedData(string key)
        {
            int index = FindTempDataIndex(key);
            tempFullData.tempDataList.RemoveAt(index);
            curShowData = new TableFullData();

            for (int i = 0; i < tempFullData.tempDataList.Count; i++)
            {
                curShowData.dataList.Add(tempDataToCurData(tempFullData.tempDataList[i]));
            }
            tempFlagDicReset();
            WriteTemp();

            RefreshListView();
        }
        #endregion

        #region CheckConflict
        private void CheckConflict()
        {

        }

        
        #endregion

        #region Merge
        //主顺序还是按照读进来的curFullData,fullFlagDic作为合并temp和cur时行号的依据，最后更新fullFlagDic
        private void MergeTempToCur()
        {
            if (tempFullData.tempDataList.Count > 0)
            {
                foreach (var temp in tempFullData.tempDataList)
                {
                    string keyName = temp.keyName;
                    int indexInXml = FindFullDataIndex(keyName);

                    if (indexInXml >= 0)
                    {

                        //改动类型是修改、新增、新增修改
                        ////curFullData中有该数据，且行号不变
                        if (indexInXml < curFullData.dataList.Count && curFullData.dataList[indexInXml].keyName == keyName)
                        {
                                curFullData.dataList[indexInXml] = tempDataToCurData(temp);
                        }
                        else
                        {
                            if (curFlagDic.ContainsKey(keyName)) //curFullData中该数据行号改变
                            {
                                curFullData.dataList[curFlagDic[keyName]] = tempDataToCurData(temp);
                            }
                            else
                            {
                                //curFullData中没有该数据,找到他 邻近 的数据插入
                                List<KeyValuePair<string, int>> lst = DictionarySort(tempFullData.fullFlagDic); //按值排序

                                bool isInserted = false;
                                for (int i = 0; i < lst.Count; i++)
                                {
                                    if (lst[i].Key == keyName)
                                    {
                                        int lineInfo = i - 1;
                                        //先向前查找
                                        while (lineInfo >= 0)
                                        {
                                            string nearKeyName = lst[lineInfo].Key;
                                            if (curFlagDic.ContainsKey(nearKeyName))
                                            {
                                                curFullData.dataList.Insert(curFlagDic[nearKeyName] + 1, tempDataToCurData(temp));
                                                CurFlagDicRest();
                                                isInserted = true;
                                                break;
                                            }
                                            if (!isInserted)
                                                lineInfo--;
                                        }
                                        if (!isInserted)
                                        {
                                            lineInfo = i + 1;
                                            //再向后查找
                                            while (lineInfo <= lst.Count)
                                            {
                                                string nearKeyName = lst[lineInfo].Key;
                                                if (curFlagDic.ContainsKey(nearKeyName))
                                                {
                                                    curFullData.dataList.Insert(curFlagDic[nearKeyName], tempDataToCurData(temp));
                                                    CurFlagDicRest();
                                                    isInserted = true;
                                                    break;
                                                }
                                                if (!isInserted)
                                                    lineInfo++;
                                            }
                                            break;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                                if (isInserted == false)
                                {
                                //待补充
                                }
                            }
                        }
                    }
                    //如果改动类型是删除
                    else if(temp.type == TableTempFullData.TempType[3])
                    {
                        int deleteIndex = temp.index;
                        ////curFullData中有该数据，且行号不变
                        if (deleteIndex < curFullData.dataList.Count && curFullData.dataList[deleteIndex].keyName == keyName)
                        {
                            curFullData.dataList.RemoveAt(deleteIndex);
                            CurFlagDicRest();
                        }
                        else
                        {
                            if (curFlagDic.ContainsKey(keyName)) //curFullData中该数据行号改变
                            {
                                curFullData.dataList.RemoveAt(curFlagDic[keyName]);
                                CurFlagDicRest();
                            }
                        }
                    }
                }
            }
            keyGenerator();
            CurFlagDicRest();
            FullFlagDicReset();
        }

        #endregion

        private void OnDisable()
        {
        }
        private void OnDestroy()
        {
            TableDataEditor.Instance?.Close();
            TableConfigEditor.Instance?.Close();
            TableDataLoader.Instance?.Close();
            TableTempDataWindow.Instance?.Close();
        }
        #region UIElements Event
        void LabelMenuOption(ContextClickEvent evt)
        {
            Label target = (Label)evt.target;
            Vector2 mousePos = evt.mousePosition;
            // Now create the menu, add items and show it
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("copy"), false, CopyToWindowsClipboard, target.text);

            menu.ShowAsContext();
        }
        void ToolbarButtonMenuOption(ContextClickEvent evt)
        {
            ToolbarButton target = (ToolbarButton)evt.currentTarget;
            Vector2 mousePos = evt.mousePosition;
            // Now create the menu, add items and show it
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Close"), false, CloseTable, target.name);
            menu.AddItem(new GUIContent("Change Config"), false, ChangeConfig, target.name);

            menu.ShowAsContext();
        }

        public void CopyToWindowsClipboard(object obj)
        {
            GUIUtility.systemCopyBuffer = obj.ToString();
        }
        public void CloseTable(object obj)
        {
            string tableName = obj.ToString();
            int index = FindActiveTableIndex(tableName);

            activeTable.RemoveAt(index);

            if (activeTable.Count == 0)
            {
                RootContainer.Clear();
                TableListView = null;
                curTableName = "";
                return;
            }
            if (tableName == curTableName)
            {
                if(index>=activeTable.Count)
                {
                    curTableName = activeTable[activeTable.Count - 1].tableName;
                    LoadTableData(GetTableFilePath(curTableName));
                    
                }    
                else
                {
                    curTableName = activeTable[index].tableName;
                    LoadTableData(GetTableFilePath( curTableName));
                }
                return;
            }
            else DrawTableNameBar();
           
        }

        public void ChangeConfig(object obj)
        {
            string tableName = obj.ToString();
            TableConfigEditor.InitWindow(GetTableFilePath(tableName));
        }

        #endregion

        #region 工具函数
        public void ReadConfig()
        {
            fullConfig = DataIO.DeserializeData<TableConfig>(Application.dataPath + TableConfigPath);
        }

        private List<KeyValuePair<string, int>> DictionarySort(SerizlizerDictionary<string, int> dic)
        {
            List<KeyValuePair<string, int>> lst = new List<KeyValuePair<string, int>>(dic);
            if (dic.Count > 0)
            {
                lst.Sort(delegate (KeyValuePair<string, int> s1, KeyValuePair<string, int> s2)
                {
                    return s1.Value.CompareTo(s2.Value);
                });
            }
            return lst;
        }
        public string GetTableFilePath(string tableName)
        {
            string filePath = Application.dataPath + "/Table/" + tableName + ".txt";
            return filePath;
        }
        public TableData tempDataToCurData(TableTempData tempData)
        {
            TableData tableData = new TableData();
            tableData.valueList = tempData.valueList;
            if (tempData.keyName != null)
                tableData.keyName = tempData.keyName;

            return tableData;
        }

        public int FindTempDataIndex(string key)
        {
            if (tempFlagDic.ContainsKey(key))
                return tempFlagDic[key];
            else return -1;
        }
        public int FindFullDataIndex(string key)
        {
            if (tempFullData.fullFlagDic.ContainsKey(key))
                return tempFullData.fullFlagDic[key];
            else return -1;
        }
        public int FindCurDataIndex(string key)
        {
            if (curFlagDic.ContainsKey(key))
                return curFlagDic[key];
            else return -1;
        }
        public int FindColIndexByColName(string ColName)
        {
            for(int i=0;i<curFullData.nameList.Count;i++)
            {
                if (ColName == curFullData.nameList[i])
                    return i;
            }
            return -1;
        }

        public int FindActiveTableIndex(string tableName)
        {
            for(int i=0;i<activeTable.Count;i++)
            {
                if (tableName == activeTable[i].tableName)
                    return i;
            }
            return -1;
        }
        //为处理键值重复、键值不存在的表格，需要再生成一个末尾有编号的键值
        public void keyGenerator()
        {
            Dictionary<string, int> keyCounter = new Dictionary<string, int>();
            if(activeTable[activeTableIndex].isKeyOnly)
            {
                for (int i = 0; i < curFullData.dataList.Count; i++)
                {
                    string keyName = curFullData.dataList[i].valueList[curKeyIndex[0]];
                    if (!keyCounter.ContainsKey(keyName))
                    {
                        keyCounter.Add(keyName, 1);
                        curFullData.dataList[i].keyName = keyName + "_0";
                    }
                    else
                    {
                        keyCounter[keyName]++;
                        curFullData.dataList[i].keyName = keyName + $"_{keyCounter[keyName] - 1}";
                    }
                }
            }
            else
            {
                for (int i = 0; i < curFullData.dataList.Count; i++)
                {
                    StringBuilder key=new StringBuilder();
                    for(int j=0;j<curKeyIndex.Count;j++)
                    {
                        if (key.Length>=1)
                            key.Append("+");
                        key.Append(curFullData.dataList[i].valueList[curKeyIndex[j]]);
                    }
                    string keyName = key.ToString();
                    if (!keyCounter.ContainsKey(keyName))
                    {
                        keyCounter.Add(keyName, 1);
                        curFullData.dataList[i].keyName = keyName + "_0";
                    }
                    else
                    {
                        keyCounter[keyName]++;
                        curFullData.dataList[i].keyName = keyName + $"_{keyCounter[keyName] - 1}";
                    }
                }

            }
            
        }

        //查找键值相同的有多少个
        public int TempFullDataKeyCount(string key)
        {
            string keyOrign = key.Substring(0, key.Length - 2);
            int num = -1;
            if (tempFullData.fullFlagDic.ContainsKey($"{keyOrign}_0"))
            {
                num = 1;
                while (num >= 1)
                {
                    if (tempFullData.fullFlagDic.ContainsKey($"{keyOrign}_{num}"))
                        num++;
                    else break;
                }
            }
            return num;
        }
        public int CurFullDataKeyCount(string key)
        {
            string keyOrign = key.Substring(0, key.Length - 2);
            int num = -1;
            if (curFlagDic.ContainsKey($"{keyOrign}_0"))
            {
                num = 1;
                while (num >= 1)
                {
                    if (curFlagDic.ContainsKey($"{keyOrign}_{num}"))
                        num++;
                    else break;
                }
            }
            return num;
        }
        public void CurFlagDicRest()
        {
            if (curFullData.dataList.Count >= 0)
            {
                curFlagDic.Clear();
                for (int i = 0; i < curFullData.dataList.Count; i++)
                {
                    string key = curFullData.dataList[i].keyName;
                    curFlagDic.Add(key, i);
                }
            }
        }
        public void tempFlagDicReset()
        {
            if (tempFullData.tempDataList.Count >= 0)
            {
                tempFlagDic.Clear();
                for (int i = 0; i < tempFullData.tempDataList.Count; i++)
                {
                    string key = tempFullData.tempDataList[i].keyName;
                    tempFlagDic.Add(key, i);
                }
            }
        }
        //进行这个逻辑之前，确保tempdata merge进了curdata
        public void FullFlagDicReset()
        {
            if (curFullData.dataList.Count >= 0)
            {
                tempFullData.fullFlagDic.Clear();
                for (int i = 0; i < curFullData.dataList.Count; i++)
                {
                    string key = curFullData.dataList[i].keyName;
                    tempFullData.fullFlagDic.Add(key, i);
                }
            }
        }
        void BindButton(string name, Action clickEvent)
        {
            var button = rootVisualElement.Q<Button>(name);

            if (button != null)
            {
                button.clickable.clicked += clickEvent;
            }
        }
        public void ShowNotice(string value,int time=3)
        {
            ShowNotification(new GUIContent(value), time);
        }

        public void setInvokeAction(Action action,float time)
        {
            InvokeAction = action;
            isStartTimer = true;
            delayTime = time;
            timer = 0;
        }
        public int Jaccard(string str1, string str2)
        {
            var tmp1 = Regex.Replace(str1, "\t", "");
            var tmp2 = Regex.Replace(str2, "\t", "");
            var intersect = tmp1.Intersect(tmp2).Count();
            var union = tmp1.Union(tmp2).Count();
            var abs = Math.Abs(tmp2.Length - tmp1.Length);
            return Convert.ToInt32((double)intersect / (union + abs) * 100);
        }
        #endregion
    }
}

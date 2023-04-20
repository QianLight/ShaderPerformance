using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.Xml;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace TableEditor
{
    [System.Serializable]
    class TableDataEditor : EditorWindow
    {
        public static TableDataEditor Instance { get; set; }
        public static void InitWindow()
        {
            var window = GetWindow<TableDataEditor>();
            window.titleContent = new GUIContent("TableDataEditor");
            window.Show();
            Instance = window;
            Instance.originData = new TableData();
            Instance.editData = null;
            Instance.tempData = null;
        }

        private TableData originData;
        private TableData editData;
        private TableTempData tempData;
        private List<int> modifiedCol;
        private int keyIndex;
        private int lineInfo;
        private int tempState;            //0新增  1修改 2新增修改 3 temp不存在
        private bool isChanged = false; //该数据是否改动
        private bool isAdd = false; //该数据是新增的
        private string searchValue;

        private ScrollView ScrollContainer;

        public void OnEnable()
        {
            // Each editor window contains a root VisualElement object
            rootVisualElement.style.flexDirection = FlexDirection.Column;

            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/TableEditor/UI/TableDataEditor_UXML.uxml");
            rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/TableEditor/UI/TableDataEditor_USS.uss"));

            asset.CloneTree(rootVisualElement);
            rootVisualElement.Q<ToolbarSearchField>("serachField").RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                Search(evt.newValue);
            });
            BindButton("Save-button", SetDataBack);
            BindButton("Delete-button", DeleteCurData);
            BindButton("Paste-button", PasteData);
        }

        public void Update()
        {
        }
        #region DrawUI
        private void DrawDataArea()
        {
            if (editData == null)
                return;
            if (rootVisualElement.Contains(ScrollContainer))
            {
                rootVisualElement.Remove(ScrollContainer);
            }
            ScrollContainer = new ScrollView();
            ScrollContainer.AddToClassList("ScrollContainer");
            for (var i = 0; i < TableEditor.Instance.curFullData.nameList.Count; i++)
            {
                //if (TableEditor.Instance.curConfig.contentList.Contains(TableEditor.Instance.curData.nameList[i]))
                //    continue;
                string name = TableEditor.Instance.curFullData.nameList[i];
                //  StringComparison.OrdinalIgnoreCase 忽略大小写
                if (searchValue == "" || name.IndexOf(searchValue, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    TextField text = new TextField(name) { name = $"{i}", value = editData.valueList[i] };
                    text.labelElement.tooltip = TableEditor.Instance.curFullData.noteList[i];
                    text.labelElement.style.width = 250;
                    if (tempState == 1)
                    {
                        if (tempData.ModifyCol.Contains(name))
                            text.labelElement.AddToClassList("blueFont");
                    }
                    if (modifiedCol.Contains(i))
                        text.labelElement.AddToClassList("redFont");
                    text.RegisterCallback<ChangeEvent<string>>((evt) =>
                    {
                        int index = int.Parse(text.name);
                        editData.valueList[index] = evt.newValue;
                        ChangeStyle();
                    });
                    ScrollContainer.Add(text);
                }
            }
            rootVisualElement.Add(ScrollContainer);
        }
        private void ChangeStyle()
        {
            for (var i = 0; i < TableEditor.Instance.curFullData.nameList.Count; i++)
            {
                string name = TableEditor.Instance.curFullData.nameList[i];
                if (searchValue == "" || name.IndexOf(searchValue, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (editData.valueList[i] != originData.valueList[i])
                    {
                        rootVisualElement.Q<TextField>($"{i}").labelElement.RemoveFromClassList("blueFont");
                        rootVisualElement.Q<TextField>($"{i}").labelElement.AddToClassList("redFont");
                        modifiedCol.Add(i);
                        modifiedCol.Distinct();
                    }
                }
            }
        }
        #endregion
     
        public void SetEditData(TableData data, int lineIndex)
        {
            originData = TableData.DeepCopyByXml(data);
            editData = TableData.DeepCopyByXml(data); //这边必须深拷贝，不然即便不save,curData的值也会随着TextArea输入框改变
            originData.keyName = data.keyName;
            editData.keyName = data.keyName;

            keyIndex = TableEditor.Instance.curKeyIndex[0];
            lineInfo = lineIndex;
            modifiedCol = new List<int>();
            searchValue = "";
            if (data.valueList[keyIndex] == string.Empty)
            {
                isAdd = true;
            }
            InitialTemp();
            CheckTempFlag();
            DrawDataArea();
        }
        private void DeleteCurData()
        {
            TableEditor.Instance.DeleteData(lineInfo);
            Instance.Close();
        }
        private void SetDataBack()
        {
            int flag = CheckCurFlag(editData);
            if (flag == 0)
            {
                ShowNotification(new GUIContent(string.Format("新增行{0}不能为空", TableEditor.Instance.curConfig.keyName)), 3);
                return;
            }
            CheckIsChanged();
            if (!isChanged)
            {
                ShowNotification(new GUIContent("未改动任何数据，不会保存"), 3);
                return;
            }
            if (flag == 2)
            {
                ShowNotification(new GUIContent(string.Format("键值{0}不可修改", TableEditor.Instance.curConfig.keyName)), 3);
            }
            else if (flag == 3)
            {
                SetTempData();
                if (isAdd) 
                    TableEditor.Instance.AddData(lineInfo);
                    
                TableEditor.Instance.SaveEditData(tempData, lineInfo);
                TableEditor.Instance.WriteTemp();
                TableEditor.Instance.ChangeCurShowData(TableEditor.Instance.curSearchWord);
                TableEditor.Instance.RefreshListView();
                ShowNotification(new GUIContent("数据保存成功"), 3);
                if (isAdd)
                    Instance.Close();
            }
        }
        public void PasteData()
        {
            var cacheData = TableEditor.Instance.cacheData;
            if (cacheData == null || cacheData.valueList.Count <= 0)
            {
                ShowNotification(new GUIContent("请先复制数据"), 3);
                return;
            }
            editData.valueList.Clear();
            for (var i = 0; i < cacheData.valueList.Count; i++)
            {
                editData.valueList.Add(cacheData.valueList[i]);
            }
            DrawDataArea();
        }

        #region Edit Temp
        private void InitialTemp()
        {
            tempData = new TableTempData();
            tempData.valueList = editData.valueList;
            string key = originData.keyName;
            int index=-1;
            if (!string.IsNullOrEmpty(key))
                index = TableEditor.Instance.FindTempDataIndex(key);

            if (index>=0)
            {
                tempData = TableTempData.DeepCopyByXml(TableEditor.Instance.tempFullData.tempDataList[index]);
                if (tempData.type==TableTempFullData.TempType[3])
                {
                    TableEditor.Instance.ShowNotice("已删除数据不可修改");
                    Close();
                }
            }
        }
        private void SetTempData()
        {
            //0新增  1修改 2新增修改 3 temp不存在
            if (isChanged)
            {
                MergeModifyCol();
                if (isAdd)
                {
                    tempData.type = TableTempFullData.TempType[0];
                }
                else
                {
                    if (tempState == 0)
                    {
                        tempData.type = TableTempFullData.TempType[2];
                    }
                    else
                        tempData.type = TableTempFullData.TempType[1];
                }
            }
        }
        private void MergeModifyCol()
        {
            for (int i = 0; i < originData.valueList.Count; i++)
            {
                if (originData.valueList[i] != editData.valueList[i])
                {
                    tempData.ModifyCol.Add(TableEditor.Instance.curFullData.nameList[i]);
                }
            }
            tempData.ModifyCol.Distinct().ToList(); //去重
        }
        #endregion

        #region Check
        //0空值不可填入 2键值修改不可填入 3可填入
        public int CheckCurFlag(TableData check)
        {
            if (TableEditor.Instance.curKeyIndex[0] < 0)
                return 0;

            var flag = check.valueList[TableEditor.Instance.curKeyIndex[0]];
            if (flag == "")
                return 0;
            if (isAdd) return 3;
            if (flag != originData.valueList[TableEditor.Instance.curKeyIndex[0]])
                return 2;
            return 3;
        }

        //0新增  1修改 2新增修改 3 temp不存在(指针对temp文件不存在,但有可能excel中已有)
        public void CheckTempFlag()
        {
            string key = originData.keyName;
            if(key!=null)
            {
                if (TableEditor.Instance.tempFlagDic.ContainsKey(key))
                {
                    int index = TableEditor.Instance.tempFlagDic[key];
                    string tempType = TableEditor.Instance.tempFullData.tempDataList[index].type;
                    for (int i = 0; i < TableTempFullData.TempType.Length; i++)
                    {
                        if (tempType == TableTempFullData.TempType[i])
                            tempState = i;
                    }
                    return;
                }
            }
            tempState = 3;
        }
        public void CheckIsChanged()
        {
            for (int i = 0; i < editData.valueList.Count; i++)
            {
                if (editData.valueList[i] != originData.valueList[i])
                {
                    isChanged = true;
                    break;
                }
            }
        }
        #endregion

        private void Search(string value)
        {
            searchValue = value;
            DrawDataArea();
        }
        void BindButton(string name, Action clickEvent)
        {
            var button = rootVisualElement.Q<Button>(name);

            if (button != null)
            {
                button.clickable.clicked += clickEvent;
            }
        }

        private void OnDestroy()
        {
            //if (CheckCurFlag( editData) == 0)
            //{
            //    TableEditor.Instance.DeleteData(lineInfo);
            //}
        }
    }
}

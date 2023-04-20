using System;
using System.Collections.Generic;
using CFEngine;
using UnityEditor;
using UnityEngine;
using BluePrint;
using CFUtilPoolLib;
using XLevel;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.IO;
using System.Text;


namespace LevelEditor
{
    class LevelCustomEditor:EditorWindow
    {
        enum CustomDataType
        {
            STRING,
            FLOAT,
            VECTOR,
            BOOL,
            ENUMSTRING,
            ENUMFLAG
        }

        public static LevelCustomEditor Instance { get; set; }
        private LevelCustomNodeDefineData editData;
        private List<string> enumNameList = new List<string>();
        private bool foldDataArea;
        private bool foldPinArea;
        private GUILayoutOption[] ContentLayout = new GUILayoutOption[] { GUILayout.Width(150f) };
        private GUILayoutOption[] LabelContent = new GUILayoutOption[] { GUILayout.Width(60f) };

        public static void Init()
        {
            Instance = GetWindowWithRect<LevelCustomEditor>(new Rect(0, 0, 1200, 600));
            Instance.titleContent = new GUIContent("CustomNodeDefine");
            Instance.Show();
            Instance.editData = new LevelCustomNodeDefineData();
        }

        private void OnGUI()
        {
            DrawToolbarArea();
            DrawDataArea();
        }

        private void DrawDataArea()
        {
            editData.name = EditorGUILayout.TextField(new GUIContent("节点名字"),editData.name);

            editData.useDefaultPin = EditorGUILayout.Toggle("useDefaultPin", editData.useDefaultPin, ContentLayout);

            foldDataArea = EditorGUILayout.BeginFoldoutHeaderGroup(foldDataArea, "数据区域");
            if(foldDataArea)
            {
                for (var i = 0; i < editData.desc.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("DataName", LabelContent);
                    editData.desc[i] = EditorGUILayout.TextField(editData.desc[i], ContentLayout);

                    EditorGUILayout.LabelField("DataType", LabelContent);
                    CustomDataType ev = (CustomDataType)Enum.Parse(typeof(CustomDataType), editData.type[i].Split('|')[0].ToUpper());
                    editData.type[i] = EditorGUILayout.EnumPopup(ev, ContentLayout).ToString().ToLower();

                    if(ev==CustomDataType.ENUMSTRING||ev==CustomDataType.ENUMFLAG)
                    {
                        EditorGUILayout.LabelField("EnumName", LabelContent);
                        enumNameList[i] = EditorGUILayout.TextField(enumNameList[i], ContentLayout);
                    }                  

                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            foldPinArea = EditorGUILayout.BeginFoldoutHeaderGroup(foldPinArea, "pin区域");
            if(foldPinArea)
            {
                for(var i=0;i<editData.pinList.Count;i++)
                {
                    var pin = editData.pinList[i];
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField("PinName", LabelContent);
                    pin.pinName = EditorGUILayout.TextArea(pin.pinName,ContentLayout);

                    EditorGUILayout.LabelField("PinID", LabelContent);
                    pin.pinID = EditorGUILayout.IntField(pin.pinID, ContentLayout);

                    EditorGUILayout.LabelField("PinStream", LabelContent);
                    pin.pinStream = (PinStream)EditorGUILayout.EnumPopup(pin.pinStream, ContentLayout);

                    EditorGUILayout.LabelField("PinType", LabelContent);
                    pin.pinType = (PinType)EditorGUILayout.EnumPopup(pin.pinType, ContentLayout);

                    if (pin.pinType == PinType.Data)
                    {
                        EditorGUILayout.LabelField("PinDataType", LabelContent);
                        pin.pinDatatType = (VariantType)EditorGUILayout.EnumPopup(pin.pinDatatType, ContentLayout);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawToolbarArea()
        {
            EditorGUILayout.BeginHorizontal(BluePrint.BlueprintStyles.Toolbar());
            if (GUILayout.Button("AddPin"))
                AddPin();
            if (GUILayout.Button("AddData"))
                AddData();
            if (GUILayout.Button("Save"))
                SaveCustomData();
            EditorGUILayout.EndHorizontal();
        }

        private void AddPin()
        {
            editData.pinList.Add(new LevelCustomPinData()
            {
                pinID = 1,
                pinName = "",
                pinStream = PinStream.In,
                pinType = PinType.Main
            });
        }

        private void AddData()
        {
            editData.desc.Add(string.Empty);
            editData.type.Add("STRING");
            editData.layout.Add("short");
            enumNameList.Add(string.Empty);
        }

        private void SaveCustomData()
        {
            LevelHelper.ReadLevelCustomConfig();
            if(string.IsNullOrEmpty(editData.name))
            {
                ShowNotification(new GUIContent("节点名字不能为空"), 3);
                return;
            }
            foreach(var pin in editData.pinList)
            {
                if(editData.pinList.FindAll(p=>p.pinID==pin.pinID).Count>1)
                {
                    ShowNotification(new GUIContent("存在重复的PinID"), 3);
                    return;
                }
            }
            if (!LevelHelper.dList.CustomDefineNodes.Exists(cn => cn.name == editData.name))
            {
                for(var i=0;i<editData.type.Count;i++)
                {
                    CustomDataType ev = (CustomDataType)Enum.Parse(typeof(CustomDataType), editData.type[i].ToUpper());
                    if (ev==CustomDataType.ENUMFLAG||ev==CustomDataType.ENUMSTRING)
                    {
                        editData.type[i] = string.Format("{0}|{1}", editData.type[i], enumNameList[i]);
                    }
                }
                LevelHelper.dList.CustomDefineNodes.Add(editData);
                DataIO.SerializeData<LevelCustomDefineData>(Application.dataPath + "/Editor/Blueprint/LevelEditor/Data/CustomNode.cfg", LevelHelper.dList);
                ShowNotification(new GUIContent("保存成功 请重开关卡编辑器应用保存"), 3);
            }
            else
                ShowNotification(new GUIContent("已存在该名称的自定义节点"),3);
        }
    }
}

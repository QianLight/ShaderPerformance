using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace LevelEditor
{
    class LevelVarientNode:LevelBaseNode<LevelVarientData>
    {

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, false);

            nodeEditorData.Tag = "Varient";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;

            BluePrintPin pinOut = new BluePrintValuedPin(this, 1, "", PinType.Data, PinStream.Out, VariantType.Var_Custom);
            AddPin(pinOut);
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HostData.varType = (VariantType)EditorGUILayout.EnumPopup(HostData.varType, new GUILayoutOption[] { GUILayout.Width(100) });
            switch(HostData.varType)
            {
                case VariantType.Var_Vector3:
                    if (HostData.varType == VariantType.Var_Vector3)
                    {
                        EditorGUILayout.LabelField("Vector3:", new GUILayoutOption[] { GUILayout.Width(100) });
                        HostData.vec.x = EditorGUILayout.FloatField(HostData.vec.x, new GUILayoutOption[] { GUILayout.Width(100) });
                        HostData.vec.y = EditorGUILayout.FloatField(HostData.vec.y, new GUILayoutOption[] { GUILayout.Width(100) });
                        HostData.vec.z = EditorGUILayout.FloatField(HostData.vec.z, new GUILayoutOption[] { GUILayout.Width(100) });
                    }
                    break;
                case VariantType.Var_Bool:
                    EditorGUILayout.LabelField("开关:", new GUILayoutOption[] { GUILayout.Width(100) });
                    HostData.boolParam = EditorGUILayout.Toggle(HostData.boolParam, new GUILayoutOption[] { GUILayout.Width(100) });
                    break;
                default:
                    HostData.value = EditorGUILayout.TextField("paramValue", HostData.value, new GUILayoutOption[] { GUILayout.Width(270f) });
                    break;
            }                       
        }

        public override List<LevelVarientData> GetDataList(LevelGraphData data)
        {
            return data.VarientData;
        }

        public override void BeforeSave()
        {
            base.BeforeSave();
            switch(HostData.varType)
            {
                case VariantType.Var_Float:
                    HostData.floatParam = float.Parse(HostData.value);
                    break;
                case VariantType.Var_UINT64:
                    HostData.intParam = int.Parse(HostData.value);
                    break;
                case VariantType.Var_Bool:
                case VariantType.Var_String:
                case VariantType.Var_Vector3:
                    break;
                default:
                    BlueprintEditor.Editor.ShowNotification(new GUIContent("暂不支持的数据类型"),5);
                    break;
            }
        }

        public override void AfterLoad()
        {
            base.AfterLoad();
            switch (HostData.varType)
            {
                case VariantType.Var_Float:
                    HostData.value = HostData.floatParam.ToString();
                    break;
                case VariantType.Var_UINT64:
                    HostData.value = HostData.intParam.ToString();
                    break;
                case VariantType.Var_Bool:
                    HostData.value = HostData.boolParam ? "true" : "false";
                    break;
                default:
                    break;
            }
        }

        public override VariantType GetOutputDataType(BluePrintValuedPin pin)
        {
            return HostData.varType;
        }
    }
}

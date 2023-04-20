using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace EditorNode
{
    public enum ScriptParam
    {
        Param0 = 0,
        Param1 = 1,
        Param2 = 2,
        Param3 = 3,
        Param4 = 4,
        Param5 = 5,
        Param6 = 6,
        Param7 = 7,
        Param8 = 8,
        Param9 = 9,

        None,
    }

    public class ParamNode : TimeTriggerNode<XParamData>
    {
        private const int StartIndex = 1;

        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, false);
            HeaderImage = "BluePrint/Header8";

            BluePrintPin pinIn = new BluePrintPin(this, -1, "In", PinType.Main, PinStream.In);
            AddPin(pinIn);
            BluePrintPin pinOut = new BluePrintPin(this, 0, "Out", PinType.Main, PinStream.Out);
            AddPin(pinOut);
        }

        public override void DrawDataInspector()
        {
            if (GUILayout.Button("+"))
            {
                bool addflag = false;
                for (int i = 0; i < GetRoot.GetConfigData<XConfigData>().ParamNames.Count; ++i)
                {
                    if (GetRoot.GetConfigData<XConfigData>().ParamNames[i] == "")
                    {
                        GetRoot.GetConfigData<XConfigData>().ParamNames[i] = "Param" + i;
                        addflag = true;
                        break;
                    }

                }
                if (!addflag && GetRoot.GetConfigData<XConfigData>().ParamNames.Count < 10)
                    GetRoot.GetConfigData<XConfigData>().ParamNames.Add("Param" + GetRoot.GetConfigData<XConfigData>().ParamNames.Count);
            }

            for (int i = 0; i < GetRoot.GetConfigData<XConfigData>().ParamNames.Count; ++i)
            {
                if (GetRoot.GetConfigData<XConfigData>().ParamNames[i] == "") continue;

                string used = "NodeIndex: ";
                for (int id = 0; id < GetRoot.widgetList.Count; ++id)
                {
                    if ((GetRoot.widgetList[id] as BaseSkillNode).useParam(i))
                    {
                        used += (GetRoot.widgetList[id] as BaseSkillNode).GetHosterData<XBaseData>().Index + ",";
                    }
                }
                EditorGUITool.LabelField(used);

                EditorGUILayout.BeginHorizontal();
                GetRoot.GetConfigData<XConfigData>().ParamNames[i] = EditorGUITool.TextField("Param" + i, GetRoot.GetConfigData<XConfigData>().ParamNames[i]);
                if (string.IsNullOrEmpty(GetRoot.GetConfigData<XConfigData>().ParamNames[i]))
                    GetRoot.GetConfigData<XConfigData>().ParamNames[i] = "Param" + i.ToString();
                if (i >= HosterData.Params.Count) HosterData.Params.Add(int.MinValue);
                if (used == "NodeIndex: ")
                {
                    if (GUILayout.Button("-"))
                    {
                        GetRoot.GetConfigData<XConfigData>().ParamNames[i] = "";
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            DrawLine();

            base.DrawDataInspector();

            int index = 10;
            index = EditorGUITool.Popup(index, GetRoot.GetConfigData<XConfigData>().ParamNames.ToArray());
            if (index != 10) HosterData.Params[index] = index;

            DrawLine();
            for (int i = 0; i < HosterData.Params.Count; ++i)
            {
                if (HosterData.Params[i] == int.MinValue) continue;
                EditorGUILayout.BeginHorizontal();
                {
                    HosterData.Params[i] = (int)(100 * EditorGUITool.FloatField(
                        (GetRoot.GetConfigData<XConfigData>().ParamNames.Count > i && GetRoot.GetConfigData<XConfigData>().ParamNames[i] != "") ? GetRoot.GetConfigData<XConfigData>().ParamNames[i] : "Param_" + i,
                        HosterData.Params[i] / 100f) + 0.5f);
                }

                if (GUILayout.Button("-"))
                {
                    HosterData.Params[i] = int.MinValue;
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        public override bool useParam(int index)
        {
            if (HosterData.Params.Count > index && HosterData.Params[index] != int.MinValue) return true;

            return false;
        }

        public override bool CompileCheck()
        {
            if (!base.CompileCheck()) return false;

            if (HosterData.Params.Count < 1)
            {
                LogError("ParamNode 参数不能为空！！！");
                return false;
            }

            return true;
        }

        public override T CopyData<T>(T data)
        {
            XParamData copy = base.CopyData(data) as XParamData;
            List<int> param = new List<int>();
            for (int i = 0; i < copy.Params.Count; ++i)
                param.Add(copy.Params[i]);
            copy.Params = param;

            return copy as T;
        }
    }
}
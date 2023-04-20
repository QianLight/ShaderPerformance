using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace EditorNode
{
    public class SwitchNode : TimeTriggerNode<XSwitchData>
    {
        public override bool BranchNode => (HosterData.SwitchType == 0);
        public override bool OneConnectPinOut { get { return true; } }

        private const int StartIndex = 1;

        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, false);
            HeaderImage = "BluePrint/Header8";

            BluePrintPin pinIn = new BluePrintPin(this, -1, "In", PinType.Main, PinStream.In);
            AddPin(pinIn);
            BluePrintPin defaultOut = new BluePrintPin(this, 0, "Default", PinType.Main, PinStream.Out);
            AddPin(defaultOut);

            if (HosterData != null)
            {
                if (HosterData.Rhs.Count == 0) HosterData.Rhs.Add(0);
                for (int i = 1; i < HosterData.Rhs.Count; ++i)
                {
                    BluePrintPin pinOut = new BluePrintPin(this, HosterData.Rhs[i], HosterData.Rhs[i].ToString(), PinType.Main, PinStream.Out, pinList[pinList.Count - 1].connectDeltaX + 10);
                    AddPin(pinOut);
                }
            }
        }

        public override T CopyData<T>(T data)
        {
            XSwitchData copy = base.CopyData(data) as XSwitchData;
            List<int> rhs = new List<int>();
            for (int i = 0; i < copy.Rhs.Count; ++i)
                rhs.Add(copy.Rhs[i]);
            copy.Rhs = rhs;

            return copy as T;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HosterData.SwitchType = EditorGUITool.Popup("SwitchType", HosterData.SwitchType, EditorGUITool.Translate<EditorEcs.XSwitchType>());

            int index = 0;
            string key;
            GetRoot.FunctionHash2Name.TryGetValue(HosterData.FunctionHash, out key);
            index = GetRoot.GetFunctionIndex(key, "s_");
            string[] funcName = GetRoot.GetFunctionTranslate("s_");
            string[] tips = GetRoot.GetFunctionTips("s_");
            GetRoot.FunctionName2Hash.TryGetValue(funcName[EditorGUITool.Popup("Function: ", index, EditorGUITool.Translate(funcName))], out HosterData.FunctionHash);

            if (funcName[index].EndsWith("_specifier"))
            {
                if (HosterData.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("s_get_skill_level_specifier"))
                {
                    HosterData.StringParameter = EditorGUITool.TextField("SkillName", HosterData.StringParameter);
                    HosterData.Specifier = (int)CFUtilPoolLib.XCommon.singleton.XHash(HosterData.StringParameter);
                }
                else if (HosterData.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("s_get_buff_level_specifier"))
                {
                    HosterData.Specifier = EditorGUITool.IntField("BuffID", HosterData.Specifier);
                }
                else
                    HosterData.Specifier = EditorGUITool.IntField("Specifier", HosterData.Specifier);
            }

            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.Toggle(HosterData.EditorTriggerCase == 0, GUILayout.Width(15))) HosterData.EditorTriggerCase = 0;
            else if (HosterData.EditorTriggerCase == 0) HosterData.EditorTriggerCase = -1;
            EditorGUITool.LabelField("Default");
            if (HosterData.Rhs.Count < Xuthus.SWITCH_BRANCH_MAX)
            {
                if (GUILayout.Button("+"))
                {
                    HosterData.Rhs.Add(pinList.Count + 1);
                    BluePrintPin pinOut = new BluePrintPin(this, pinList.Count, pinList.Count.ToString(), PinType.Main, PinStream.Out, pinList[pinList.Count - 1].connectDeltaX + 10);
                    AddPin(pinOut);
                }
            }
            EditorGUILayout.EndHorizontal();

            string[] strList = string.IsNullOrEmpty(HosterData.StringParameter) ? new string[0] : HosterData.StringParameter.Split('|');
            if (HosterData.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("s_get_last_skill"))
            {
                HosterData.StringParameter = "";
            }

            string caseTip = "Case" + tips[index];

            for (int i = 1; i < HosterData.Rhs.Count; ++i)
            {
                string strTmp = i < strList.Length ? strList[i] : "";

                EditorGUILayout.BeginHorizontal();

                if (EditorGUILayout.Toggle(HosterData.EditorTriggerCase == i, GUILayout.Width(15))) HosterData.EditorTriggerCase = i;
                else if (HosterData.EditorTriggerCase == i) HosterData.EditorTriggerCase = -1;

                if (HosterData.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("s_get_last_hit") ||
                    HosterData.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("s_get_curr_hit"))
                {
                    HosterData.Rhs[i] = EditorGUITool.Popup(caseTip, HosterData.Rhs[i], EditorGUITool.Translate<XHitType>());
                    pinList[i + StartIndex].Desc = ((XHitType)HosterData.Rhs[i]).ToString();
                }
                else if (HosterData.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("s_get_last_state") ||
                    HosterData.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("s_get_curr_state"))
                {
                    HosterData.Rhs[i] = EditorGUITool.Popup(caseTip, HosterData.Rhs[i], EditorGUITool.Translate<XStateType>());
                    pinList[i + StartIndex].Desc = ((XStateType)HosterData.Rhs[i]).ToString();
                }
                else if (HosterData.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("s_hit_direction"))
                {
                    HosterData.Rhs[i] = EditorGUITool.Popup(caseTip, HosterData.Rhs[i], EditorGUITool.Translate<XHitDirection>());
                    pinList[i + StartIndex].Desc = ((XHitDirection)HosterData.Rhs[i]).ToString();
                }
                else if (HosterData.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("s_get_death_from_slot") ||
                    HosterData.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("s_get_hit_effect_slot"))
                {
                    HosterData.Rhs[i] = EditorGUITool.Popup(caseTip, HosterData.Rhs[i], EditorGUITool.Translate<XHitSlot>());
                    pinList[i + StartIndex].Desc = EditorGUITool.Translate(((XHitSlot)HosterData.Rhs[i]).ToString());
                }
                else if (HosterData.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("s_get_last_skill"))
                {
                    strTmp = EditorGUITool.TextField("SkillName", strTmp);
                    HosterData.Rhs[i] = (int)CFUtilPoolLib.XCommon.singleton.XHash(strTmp);
                    pinList[i + StartIndex].Desc = strTmp;
                    HosterData.StringParameter += "|" + strTmp;
                }
                else
                {
                    HosterData.Rhs[i] = EditorGUITool.IntField(caseTip, HosterData.Rhs[i]);
                    pinList[i + StartIndex].Desc = HosterData.Rhs[i].ToString();
                }

                pinList[i + StartIndex].Desc = tips[index] + pinList[i + StartIndex].Desc;

                if (GUILayout.Button("-"))
                {
                    RemovePin(pinList[i + StartIndex]);
                    HosterData.Rhs.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (HosterData.EditorTriggerCase >= HosterData.Rhs.Count) HosterData.EditorTriggerCase = -1;
        }

        public override void BuildDataByPin()
        {
            GetHosterData<XBaseData>().TransferData.Clear();

            for (int i = 1; i < pinList.Count; ++i)
            {
                if (pinList[i].connections.Count != 0)
                {
                    AddPinData<XBaseData>(pinList[i].connections[0].connectEnd.GetNode<BaseSkillNode>().GetHosterData<XBaseData>());
                }
                else
                {
                    AddPinData<XBaseData>(new XBaseData() { Index = -1 });
                }
            }
        }

        public override void BuildPinByData(Dictionary<int, BaseSkillNode> IndexToNodeDic)
        {
            for (int i = 2; i < pinList.Count; ++i)
            {
                pinList[i].OnDeleted();
            }
            if (pinList.Count > 2)
                pinList.RemoveRange(2, pinList.Count - 2);

            for (int i = 0; i < GetHosterData<XBaseData>().TransferData.Count; ++i)
            {
                if (i != 0)
                {
                    BluePrintPin pinOut = new BluePrintPin(this, HosterData.Rhs[i], HosterData.Rhs[i].ToString(), PinType.Main, PinStream.Out, pinList[pinList.Count - 1].connectDeltaX + 10);
                    AddPin(pinOut);
                }
                if (GetHosterData<XBaseData>().TransferData[i].Index != -1)
                {
                    BluePrintPin startPin = pinList[i + StartIndex];
                    BluePrintPin endPin = IndexToNodeDic[GetHosterData<XBaseData>().TransferData[i].Index].pinList[0];
                    ConnectPin(startPin, endPin);
                }
            }
        }

        public override bool CompileCheck()
        {
            if (!base.CompileCheck()) return false;

            HashSet<int> hash = new HashSet<int>();
            for (int i = 1; i < HosterData.Rhs.Count; ++i)
            {
                if (hash.Contains(HosterData.Rhs[i]))
                {
                    LogError("SwitchNode出口值不能相同！！！");
                    return false;
                }
                hash.Add(HosterData.Rhs[i]);
            }

            int count = GetRoot.GetConfigData<EcsData.XSkillData>() != null ? GetRoot.GetConfigData<EcsData.XSkillData>().SwitchData.Count : GetRoot.GetConfigData<EcsData.XHitData>().SwitchData.Count;

            if (count > EditorEcs.Xuthus_VirtualServer.SWITCH_MAX)
            {
                LogError("Switch  个数最大上限(" + EditorEcs.Xuthus_VirtualServer.SWITCH_MAX + ")！！！" + GetRoot.DataPath);
                return false;
            }

            return true;
        }

        public override bool IgnoreMultiIn => false;
    }
}
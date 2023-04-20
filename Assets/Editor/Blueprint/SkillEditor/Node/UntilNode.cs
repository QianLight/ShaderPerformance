using EcsData;
using UnityEngine;
using BluePrint;
using UnityEditor;
using System.Collections.Generic;

namespace EditorNode
{
    public class UntilNode : TimeTriggerNode<XUntilData>
    {
        private BaseSkillNode multiConditionNode = null;

        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, false);

            HeaderImage = "BluePrint/Header8";

            if (AutoDefaultMainPin)
            {
                BluePrintPin pinIn = new BluePrintPin(this, -1, "In", PinType.Main, PinStream.In);
                BluePrintPin pinOut = new BluePrintPin(this, -2, "End", PinType.Main, PinStream.Out);
                AddPin(pinIn);
                AddPin(pinOut);
            }
        }

        public override T CopyData<T>(T data)
        {
            XUntilData copy = base.CopyData(data) as XUntilData;

            List<XFunctionData> func = new List<XFunctionData>();
            for (int i = 0; i < copy.Func.Count; ++i)
                func.Add(copy.Func[i].Clone() as XFunctionData);
            copy.Func = func;

            return copy as T;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HosterData.Duration = TimeFrameField("Duration", HosterData.Duration);

            HosterData.Intervals = TimeFrameField("Intervals", HosterData.Intervals);
            EditorGUITool.LabelField("Intervals 不随加减速变化，为固定时间间隔");

            HosterData.EndTrigger = EditorGUITool.Toggle("EndTrigger", HosterData.EndTrigger);
            HosterData.AllowBranch = EditorGUITool.Toggle("AllowBranch", HosterData.AllowBranch);
            HosterData.DoWhile = EditorGUITool.Toggle("DoWhile", HosterData.DoWhile);

            GetNodeByIndex<MultiConditionNode>(ref multiConditionNode, ref HosterData.MultiConditionIndex, true, "MultiConditionIndex");
            DrawLine();
            DrawFunctions(HosterData.Func, "UntilFunction");
        }

        protected void DrawFunctions(List<XFunctionData> func, string label, string without = "@")
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUITool.LabelField(label);
            if (func.Count < 4)
            {
                if (GUILayout.Button("+"))
                {
                    func.Add(new XFunctionData());
                }
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < func.Count; ++i)
            {
                DrawLine();
                EditorGUILayout.BeginHorizontal();
                EditorGUITool.LabelField("Function_" + i.ToString());
                if (GUILayout.Button("Delete"))
                {
                    func.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                DrawFunctionParam(func[i], without);
            }
        }

        protected void DrawFunctionParam(XFunctionData data, string without)
        {
            int index = 0;
            string key;
            GetRoot.FunctionHash2Name.TryGetValue(data.FunctionHash, out key);
            index = GetRoot.GetFunctionIndex(key, "u_", without);
            string[] funcName = GetRoot.GetFunctionTranslate("u_", without);
            GetRoot.FunctionName2Hash.TryGetValue(funcName[EditorGUITool.Popup("Function: ", index, EditorGUITool.Translate(funcName))], out data.FunctionHash);

            if (data.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("u_paracurve_down"))
            {
                ParamTemplate("Parameter1", data,
                    ()=> { data.Parameter1 = EditorGUITool.FloatField("G", data.Parameter1); });
                ParamTemplate("Parameter2", data,
                    () => { data.Parameter2 = EditorGUITool.FloatField("Attenuation", data.Parameter2); });
            }
            else if(data.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("u_paracurve_up"))
            {
                ParamTemplate("Parameter1", data,
                    () => { data.Parameter1 = EditorGUITool.FloatField("G", data.Parameter1); });
                ParamTemplate("Parameter2", data,
                    () => { data.Parameter2 = EditorGUITool.FloatField("Attenuation", data.Parameter2); });
            }
            else if(data.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("u_horizontal_xz"))
            {
                ParamTemplate("Parameter1", data,
                    () => { data.Parameter1 = EditorGUITool.FloatField("Acceleration", data.Parameter1); });
                ParamTemplate("Parameter2", data,
                    () => { data.Parameter2 = EditorGUITool.FloatField("Attenuation", data.Parameter2); });
            }
        }

        public override void BuildDataFinish()
        {
            base.BuildDataFinish();

            GetNodeByIndex<MultiConditionNode>(ref multiConditionNode, ref HosterData.MultiConditionIndex);
        }

        public override bool CompileCheck()
        {
            if (!base.CompileCheck()) return false;

            int count = GetRoot.GetConfigData<EcsData.XSkillData>() != null ? GetRoot.GetConfigData<EcsData.XSkillData>().UntilData.Count : GetRoot.GetConfigData<EcsData.XHitData>().UntilData.Count;

            if (count > EditorEcs.Xuthus_VirtualServer.UNTIL_MAX)
            {
                LogError("Until  个数最大上限(" + EditorEcs.Xuthus_VirtualServer.UNTIL_MAX + ")！！！" + GetRoot.DataPath);
                return false;
            }

            foreach (BlueprintConnection connection in pinList[1].connections)
            {
                if (!UntilChildCheck(connection.connectEnd.GetNode<BaseSkillNode>())) return false;
            }

            return true;
        }

        private bool UntilChildCheck(BaseSkillNode node)
        {
            if (!HosterData.AllowBranch)
            {
                if (node is ConditionNode || node is MultiConditionNode || node is SwitchNode || node is UntilNode)
                {
                    LogError("UntilNode_" + HosterData.Index + " 不能接Condition，Switch，Until节点！！！");
                    return false;
                }

                if (node is TargetSelectNode && (node as TargetSelectNode).HosterData.Sync)
                {
                    LogError("UntilNode_" + HosterData.Index + " 后续TargetSelect不能Sync！！！");
                    return false;
                }

                for (int i = 1; i < node.pinList.Count; ++i)
                {
                    for (int j = 0; j < node.pinList[i].connections.Count; ++j)
                        if (!UntilChildCheck(node.pinList[i].connections[j].connectEnd.GetNode<BaseSkillNode>()))
                            return false;
                }
            }

            return true;
        }

        public override void CalcTriggerTime()
        {
            base.CalcTriggerTime();

            DFSTriggerTime(this, HosterData.MultiConditionIndex);
        }

        public override bool useParam(int index)
        {
            for (int i = 0; i < HosterData.Func.Count; ++i)
            {
                if (HosterData.Func[i].Parameter1ParamIndex == index) return true;
                if (HosterData.Func[i].Parameter2ParamIndex == index) return true;
            }

            return false;
        }

        public override bool IgnoreMultiIn => false;
    }
}

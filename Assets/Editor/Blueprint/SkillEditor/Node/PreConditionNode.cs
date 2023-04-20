using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace EditorNode
{
    public class PreConditionNode : EventTriggerNode<XPreConditionData>
    {
        public override bool OneConnectPinOut { get { return true; } }

        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, false);
            HeaderImage = "BluePrint/Header8";
        }

        public override T CopyData<T>(T data)
        {
            XPreConditionData copy = base.CopyData(data) as XPreConditionData;
            List<XConditionData> tmp = new List<XConditionData>();
            for (int i = 0; i < copy.Cond.Count; ++i)
                tmp.Add(copy.Cond[i].Clone() as XConditionData);
            copy.Cond = tmp;

            List<int> tmpList = new List<int>();
            for (int i = 0; i < copy.ErrorType.Count; ++i)
                tmpList.Add(copy.ErrorType[i]);
            copy.ErrorType = tmpList;

            return copy as T;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HosterData.Or = EditorGUITool.Toggle("Or", HosterData.Or);
            HosterData.Not = EditorGUITool.Toggle("Not", HosterData.Not);

            DrawConditions(HosterData.Cond, "PreCondition", HosterData.ErrorType, "not pre");
        }

        public override bool CompileCheck()
        {
            if (!base.CompileCheck()) return false;

            if (HosterData.Cond.Count == 0)
            {
                LogError("PreConditionNode条件不能为空！！！");
                return false;
            }

            if (GetRoot.GetConfigData<XSkillData>().PreConditionData.Count > 1)
            {
                LogError("PreConditionNode最多一个！！！");
                return false;
            }

            return true;
        }
    }
}
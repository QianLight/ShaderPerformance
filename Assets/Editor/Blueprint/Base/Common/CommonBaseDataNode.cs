using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    public class BluePrintBaseDataNode<T> : BluePrintNode 
        where T : BluePrintNodeBaseData, new()
    {
        public T HostData = new T();
        public BlueprintRuntimeBaseNode RuntimeData;

        public virtual void InitData(T data, NodeConfigData configData)
        {
            HostData = data;
            nodeEditorData = configData;
        }

        public T GetHostData()
        {
            return HostData;
        }

        public virtual List<T> GetCommonDataList(BluePrintData data) { return null; }

        public virtual void OnCopy(T sData) { }

        public virtual void BeforeSave()
        {
            HostData.NodeID = nodeEditorData.NodeID;
            Root.graphConfigData.NodeConfigList.Add(nodeEditorData);
        }

        public virtual void ConvenientSave()
        {
            BeforeSave();
        }

        public virtual void BeforeSaveTpl(GraphConfigData d)
        {
            HostData.NodeID = nodeEditorData.NodeID;
            d.NodeConfigList.Add(nodeEditorData);
        }

        public virtual void AfterLoad()
        {
            Enabled = HostData.enabled;
        }

        public override void DrawHeader(Rect headRect)
        {
            base.DrawHeader(headRect);

            Rect rect = new Rect(headRect.x + 10, headRect.y + 8, 30, 20);

            DrawTool.DrawLabel(rect.Scale(Scale), HeaderText(), BlueprintStyles.HeaderStyle, TextAnchor.UpperLeft);

        }

        public virtual void OnEnterSimulation()
        {
            if (Root.simulatorEngine != null)
            {
                RuntimeData = Root.simulatorEngine.GetGraph(Root.GraphID).FindNode(HostData.NodeID);

                foreach (BluePrintPin pin in pinList)
                {
                    pin.OnEnterSimulation(RuntimeData);
                }
            }
        }

        public virtual void OnEndSimulation()
        {
            RuntimeData = null;
        }

        public override bool IsExecuted()
        {
            if(RuntimeData != null) return RuntimeData.Executed;
            return false;
        }

        public virtual string HeaderText()
        {
            return nodeEditorData.NodeID.ToString();
        }

    }

}

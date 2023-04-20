using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace LevelEditor
{
    public abstract class LevelBaseNode<T> : BluePrintBaseDataNode<T> 
        where T : BluePrintNodeBaseData, new()
    {
        protected List<BlueprinButton> buttonList = new List<BlueprinButton>();
        protected List<BlueprintSlider> sliderList = new List<BlueprintSlider>();

        public virtual List<T> GetDataList(LevelGraphData data) { return null; }

        public  override void UnInit()
        {
            buttonList.Clear();
            sliderList.Clear();
            base.UnInit();
        }

        protected void AddButton(BlueprinButton button)
        {
            buttonList.Add(button);
            AddChild(button);
        }

        protected void AddSlider(BlueprintSlider slider)
        {
            sliderList.Add(slider);
            AddChild(slider);
        }

        public override void CheckError()
        {
            base.CheckError();
            foreach(var pin in pinList)
            {
                if(pin.pinStream==PinStream.In&&pin.pinType==PinType.Main)
                {
                    if(pin.connections.Count>1)
                    {
                        nodeErrorInfo.ErrorDataList.Add(
                            new BlueprintErrorData(BlueprintErrorCode.Invalid, "同一入口不允许多条连线", pin.connections[0]));
                    }
                }
            }
        }

        public override void OnAdded()
        {
            base.OnAdded();
            if (LevelEditor.Instance.CurrentGraph != null)
                LevelOperationStack.Instance.PushOperation(LevelOperationType.AddNode, this, LevelEditor.Instance.CurrentGraph.GraphID);
        }

        //在删除时先于基类函数存储操作 不然pin的连接信息会丢失
        public override void OnDeleted()
        {
            if (LevelEditor.Instance.CurrentGraph != null)
                LevelOperationStack.Instance.PushOperation(LevelOperationType.DeleteNode, this, LevelEditor.Instance.CurrentGraph.GraphID);
            base.OnDeleted();
        }

        public override void OnConnectionBreak(BluePrintPin start, BluePrintPin end)
        {
            base.OnConnectionBreak(start, end);
            if (LevelEditor.Instance.CurrentGraph != null)
                LevelOperationStack.Instance.PushOperation(LevelOperationType.PinBreak, 
                new List<BluePrintPin>() { start,end}
                , LevelEditor.Instance.CurrentGraph.GraphID);
        }

        public override void OnConnectionSucc(BluePrintPin start, BluePrintPin end, BlueprintConnection connection)
        {
            base.OnConnectionSucc(start, end, connection);
            if (LevelEditor.Instance.CurrentGraph != null)
                LevelOperationStack.Instance.PushOperation(LevelOperationType.PinConnect,
               new List<BluePrintPin>() { start, end }
               , LevelEditor.Instance.CurrentGraph.GraphID);
        }

    }
}

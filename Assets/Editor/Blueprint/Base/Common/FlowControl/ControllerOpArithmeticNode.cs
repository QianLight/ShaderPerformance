using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    
    class ControllerOpArithmeticNode : ControllerBaseNode
    {
        //ArithmeticType arithType = ArithmeticType.Add;
        //int selectType = 0;

        GUIContent[] contents = new GUIContent[] { new GUIContent("+"), new GUIContent("-"), new GUIContent("*"), new GUIContent("div") };
        int[] values = new int[] { 0, 1, 2, 3 };

        public override int GetContorllerType()
        {
            return (int)ControllerNodeType.ControllerNode_Arithmetic;
        }

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);

            nodeEditorData.TitleName = "";
            nodeEditorData.BackgroundText = "+-*";

            BluePrintValuedPin pinIn = new BluePrintValuedPin(this, 1, "", PinType.Data, PinStream.In, VariantType.Var_Custom);
            pinIn.SetDefaultValue(0.0f);
            AddPin(pinIn);

            BluePrintValuedPin pinIn2 = new BluePrintValuedPin(this, 2, "", PinType.Data, PinStream.In, VariantType.Var_Custom);
            pinIn2.SetDefaultValue(0.0f);
            AddPin(pinIn2);

            BluePrintValuedPin pinIn3 = new BluePrintValuedPin(this, 4, "", PinType.Data, PinStream.In, VariantType.Var_Custom);
            pinIn3.SetDefaultValue(0.0f);
            AddPin(pinIn3);

            BluePrintValuedPin pinIn4 = new BluePrintValuedPin(this, 5, "", PinType.Data, PinStream.In, VariantType.Var_Custom);
            pinIn4.SetDefaultValue(0.0f);
            AddPin(pinIn4);

            BluePrintPin pinOut = new BluePrintValuedPin(this, 3, "", PinType.Data, PinStream.Out, VariantType.Var_Custom);
            AddPin(pinOut);
        }

        public override void DrawTitle(Rect titleBounds, string title)
        {
            Rect rect = new Rect(titleBounds.x, titleBounds.y + 20 * Scale, titleBounds.width, 40 * Scale);
            HostData.ArithmeticID = EditorGUI.IntPopup(rect, HostData.ArithmeticID, contents, values);
        }

        /// <summary>
        /// 输出数据类型检查所有输入数据类型是否相同 不相同的话返回unknown
        /// </summary>
        /// <param name="pin"></param>
        /// <returns></returns>

        public override VariantType GetOutputDataType(BluePrintValuedPin pin)
        {
            var inPins = pinList.FindAll((p) => p.pinStream == PinStream.In);
            VariantType dummy = VariantType.Var_Custom;
            foreach(var inPin in inPins)
            {
                if (inPin.reverseConnections.Count < 1)
                    continue;
                var p = inPin.reverseConnections[0].reverseConnectEnd as BluePrintValuedPin;
                var t = p.GetRealType();
                if (dummy != t && dummy != VariantType.Var_Custom && t != VariantType.Var_Custom) 
                    return VariantType.Var_Unknow;
                dummy = t;
            }
            if (dummy == VariantType.Var_Vector3 && HostData.ArithmeticID == 2)//向量点积特殊处理一下
                dummy = VariantType.Var_Float;
            return dummy;
        }

        public override void CheckError()
        {
            base.CheckError();
            var outPin = pinList.Find(p => p.pinStream == PinStream.Out) as BluePrintValuedPin;
            if(GetOutputDataType(outPin)==VariantType.Var_Vector3)
            {
                switch(HostData.ArithmeticID)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        var inPins = pinList.FindAll((p) => p.pinStream == PinStream.In);
                        int count = 0;
                        foreach (var inPin in inPins)
                        {
                            if (inPin.reverseConnections.Count >= 1)
                                count += 1;
                        }
                        if (count != 2)
                            nodeErrorInfo.ErrorDataList.Add(new BlueprintErrorData(BlueprintErrorCode.NodeDataError, "向量点积输入数量有且只能有两个", null));
                        break;
                    case 3:
                        nodeErrorInfo.ErrorDataList.Add(new BlueprintErrorData(BlueprintErrorCode.NodeDataError, "向量不支持除法", null));
                        break;
                }
            }
        }

    }
}

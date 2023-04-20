using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    class CommonVariableGetNode : BluePrintBaseDataNode<BluePrintVariantData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, false);

            DrawHead = false;

            nodeEditorData.BackgroundText = "";

            BluePrintPin pinValue = new BluePrintValuedPin(this, 1, "", PinType.Data, PinStream.Out, VariantType.Var_Custom);
            AddPin(pinValue);
        }

        public override List<BluePrintVariantData> GetCommonDataList(BluePrintData data)
        {
            return data.VarGetData;
        }

        public override void OnAdded()
        {
            nodeEditorData.Tag = "Get " + HostData.VariableName;
        }

        public VariantType GetRealVariableType()
        {
            VariantType t = VariantType.Var_Custom;
            if (Root.VarManager.GetVariantType(HostData.VariableName, ref t))
                return t;

            return VariantType.Var_Unknow;
        }
    }
}

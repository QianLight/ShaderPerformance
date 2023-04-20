using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    class CommonVariableSetNode : BluePrintBaseDataNode<BluePrintVariantData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);

            HeaderImage = "BluePrint/Header2";
            nodeEditorData.BackgroundText = "";

            BluePrintPin pinValue = new BluePrintValuedPin(this, 1, "", PinType.Data, PinStream.In, VariantType.Var_Custom);
            AddPin(pinValue);

            BluePrintPin pinValueOut = new BluePrintValuedPin(this, 2, "", PinType.Data, PinStream.Out, VariantType.Var_Custom);
            AddPin(pinValueOut);
        }

        public override List<BluePrintVariantData> GetCommonDataList(BluePrintData data)
        {
            return data.VarSetData;
        }

        public override void OnAdded()
        {
            nodeEditorData.Tag = "Set " + HostData.VariableName;

            VariantType t = VariantType.Var_Bool;
            if (Root.VarManager.GetVariantType(HostData.VariableName, ref t))
            {
                // set default value
                if (t == VariantType.Var_Float)
                    (GetPin(1) as BluePrintValuedPin).SetDefaultValue(0.0f);
            }

        }

        public VariantType GetRealVariableType()
        {
            VariantType t = VariantType.Var_Custom;
            if (Root.VarManager.GetVariantType(HostData.VariableName, ref t))
                return t;

            return VariantType.Var_Unknow;
        }

        #region simulation
        CommonRTVariableSetNode setVarData;

        public override void OnEnterSimulation()
        {
            base.OnEnterSimulation();
            setVarData = RuntimeData as CommonRTVariableSetNode;
        }
        #endregion
    }
}

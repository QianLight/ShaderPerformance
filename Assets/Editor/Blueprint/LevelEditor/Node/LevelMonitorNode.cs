using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace LevelEditor
{
    class LevelMonitorNode : LevelBaseNode<LevelMonitorData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);

            HeaderImage = "BluePrint/Header8";
            nodeEditorData.Tag = "Monitor";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;

            BluePrintPin pinBool = new BluePrintValuedPin(this, 3, "Condition", PinType.Data, PinStream.In, VariantType.Var_Bool);
            AddPin(pinBool);

            BluePrintPin pinValue = new BluePrintValuedPin(this, 2, "Value", PinType.Data, PinStream.In, VariantType.Var_Float);
            AddPin(pinValue);

            BluePrintValuedPin pinTarget = new BluePrintValuedPin(this, 1, "Target", PinType.Data, PinStream.In, VariantType.Var_Float);
            pinTarget.SetDefaultValue(0);
            AddPin(pinTarget);
        }

        public override List<LevelMonitorData> GetDataList(LevelGraphData data)
        {
            return data.MonitorData;
        }

    }
}

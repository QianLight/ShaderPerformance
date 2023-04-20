using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BluePrint;

namespace LevelEditor
{
    class LevelRTMonitorNode : BlueprintRuntimeDataNode<LevelMonitorData>
    {
        BlueprintRuntimeValuedPin pinTarget;
        BlueprintRuntimeValuedPin pinCurrent;
        BlueprintRuntimeValuedPin pinBool;

        private bool startMonitor = false;

        //public LevelRTMonitorNode(BlueprintRuntimeGraph e) : base(e)
        //{ }

        public override void Init(LevelMonitorData data, bool AutoStreamPin = true)
        {
            base.Init(data, AutoStreamPin);

            pinCurrent = new BlueprintRuntimeValuedPin(this, 2, PinType.Data, PinStream.In, VariantType.Var_Float);
            AddPin(pinCurrent);

            pinTarget = new BlueprintRuntimeValuedPin(this, 1, PinType.Data, PinStream.In, VariantType.Var_Float, true, true);
            AddPin(pinTarget);

            pinBool = new BlueprintRuntimeValuedPin(this, 3, PinType.Data, PinStream.In, VariantType.Var_Bool);
            AddPin(pinBool);

            startMonitor = false;
        }

        public override void Execute(BlueprintRuntimePin activePin)
        {
            base.Execute(activePin);

            startMonitor = true;
        }

        public override void Update(float deltaT)
        {
            if (!startMonitor) return;

            BPVariant target = new BPVariant();
            BPVariant current = new BPVariant();
            BPVariant condition = new BPVariant();

            if(pinBool.GetPinValue(ref condition))
            {
                if(condition.IsTrue())
                {
                    startMonitor = false;
                    if (pinOut != null)
                        pinOut.Active();
                    return;
                }
            }

            bool bTarget = pinTarget.GetPinValue(ref target);
            bool bCurrent = pinCurrent.GetPinValue(ref current);

            if (bTarget && bCurrent)
            {
                if(current.type == VariantType.Var_Float && current.type == VariantType.Var_Float && current.val._float <= target.val._float)
                {
                    startMonitor = false;

                    if (pinOut != null)
                        pinOut.Active();
                }
            }
        }
    }
}

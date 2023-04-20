using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrint
{
    class CommonRTVariableSetNode : BlueprintRuntimeDataNode<BluePrintVariantData>
    {
        BlueprintRuntimeValuedPin pinDataOut;
        BlueprintRuntimeValuedPin pinDataIn;

        //public CommonRTVariableSetNode(BlueprintRuntimeGraph e) : base(e)
        //{ }

        public override void Init(BluePrintVariantData data, bool AutoStreamPin = true)
        {
            base.Init(data, AutoStreamPin);

            VariantType t = parentGraph.VarManager.GetVariantType(data.VariableName);

            pinDataIn = new BlueprintRuntimeValuedPin(this, 1, PinType.Data, PinStream.In, t, true, true);
            AddPin(pinDataIn);

            pinDataOut = new BlueprintRuntimeValuedPin(this, 2, PinType.Data, PinStream.Out, t, false);
            pinDataOut.SetValueSource(GetValue);
            AddPin(pinDataOut);
        }

        protected BPVariant GetValue()
        {
            return parentGraph.VarManager.GetValue(HostData.VariableName);
        }

        public override void Execute(BlueprintRuntimePin activePin)
        {
            base.Execute(activePin);
            BPVariant bpv = default;
            if(pinDataIn.GetPinValue(ref bpv))
            {
                parentGraph.VarManager.SetValue(HostData.VariableName, bpv);
            }

            pinDataOut.dataOpen = true;

            if (pinOut != null)
                pinOut.Active();
        }

        
    }
}

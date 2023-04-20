using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrint
{
    class CommonRTVariableGetNode : BlueprintRuntimeDataNode<BluePrintVariantData>
    {
        BlueprintRuntimeValuedPin pinDataOut;

        //public CommonRTVariableGetNode(BlueprintRuntimeGraph e) : base(e)
        //{ }

        public override void Init(BluePrintVariantData data, bool AutoStreamPin = true)
        {
            base.Init(data, AutoStreamPin);

            VariantType t = parentGraph.VarManager.GetVariantType(data.VariableName);

            pinDataOut = new BlueprintRuntimeValuedPin(this, 1, PinType.Data, PinStream.Out, t);
            pinDataOut.SetValueSource(GetValue);
            AddPin(pinDataOut);
        }

        protected BPVariant GetValue()
        {
            return parentGraph.VarManager.GetValue(HostData.VariableName);
        }




    }
}

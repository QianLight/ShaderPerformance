using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BluePrint;

namespace LevelEditor
{
    public class LevelRTGetExternalVarNode : BlueprintRuntimeDataNode<LevelVarData>
    {
        BlueprintRuntimeValuedPin pinDataOut;

        public override void Init(LevelVarData data, bool AutoStreamPin = true)
        {
            base.Init(data, AutoStreamPin);

            VariantType t = VariantType.Var_Float;

            pinDataOut = new BlueprintRuntimeValuedPin(this, 1, PinType.Data, PinStream.Out, t);
            pinDataOut.SetValueSource(GetValue);
            AddPin(pinDataOut);
        }
    
        protected BPVariant GetValue()
        {
            // global value
            return default;
        }

    }

    public class LevelRTSetExternalVarNode : BlueprintRuntimeDataNode<LevelVarData>
    {
        BlueprintRuntimeValuedPin pinDataOut;
        BlueprintRuntimeValuedPin pinDataIn;

        public override void Init(LevelVarData data, bool AutoStreamPin = true)
        {
            base.Init(data, AutoStreamPin);

            VariantType t = VariantType.Var_Float;

            pinDataIn = new BlueprintRuntimeValuedPin(this, 1, PinType.Data, PinStream.In, t, true, true);
            AddPin(pinDataIn);

            pinDataOut = new BlueprintRuntimeValuedPin(this, 2, PinType.Data, PinStream.Out, t, false);
            pinDataOut.SetValueSource(GetValue);
            AddPin(pinDataOut);
        }

        protected BPVariant GetValue()
        {
            // get default value
             return default;
        }

        public override void Execute(BlueprintRuntimePin activePin)
        {
            base.Execute(activePin);
            pinDataOut.dataOpen = true;

            // set default value
            if (pinOut != null)
                pinOut.Active();
        }


    }
}


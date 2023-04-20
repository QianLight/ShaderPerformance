using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BluePrint;

namespace LevelEditor
{
    class LevelRTSwitchPartnerNode : BlueprintRuntimeDataNode<LevelSwitchPartnerData>
    {
        public override void Init(LevelSwitchPartnerData data, bool AutoStreamPin = true)
        {
            base.Init(data, false);

            BlueprintRuntimePin pinOut = new BlueprintRuntimePin(this, 1, PinType.Main, PinStream.Out);
            AddPin(pinOut);

            BlueprintRuntimePin pinIn = new BlueprintRuntimePin(this, 2, PinType.Main, PinStream.In);
            AddPin(pinIn);
        }
        public override void Execute(BlueprintRuntimePin activePin)
        {
            base.Execute(activePin);

            Debug.Log("Switch Partner");
        }
    }
}

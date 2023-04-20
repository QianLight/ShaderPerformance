using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BluePrint;

namespace LevelEditor
{
    class LevelRTExstringNode : BlueprintRuntimeDataNode<LevelExstringData>
    {
        //public LevelRTExstringNode(BlueprintRuntimeGraph e) : base(e)
        //{ }
        private bool startMonitor = false;

        public override void Init(LevelExstringData data, bool AutoStreamPin = true)
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

            startMonitor = true;
         }

        public void OnStringActive()
        {
            if (!startMonitor) return;

            BlueprintRuntimePin pinOut = GetPin(1);
            pinOut.Active();

        }




    }
}

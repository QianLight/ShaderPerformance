using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BluePrint;

namespace LevelEditor
{
    public class LevelRTRandomNode : BlueprintRuntimeDataNode<LevelRandomNodeData>
    {
        public override void Init(LevelRandomNodeData data, bool AutoStreamPin = true)
        {
            base.Init(data, true);
        }
        public override void Execute(BlueprintRuntimePin activePin)
        {
            base.Execute(activePin);

            if (pinOut != null)
                pinOut.Active();

        }
    }
}

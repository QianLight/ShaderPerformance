using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BluePrint;

namespace LevelEditor
{
    public class LevelRTRecordNode : BlueprintRuntimeDataNode<LevelRecordData>
    {
        public override void Execute(BlueprintRuntimePin activePin)
        {
            base.Execute(activePin);

            // record
            if (pinOut != null)
                pinOut.Active();

        }
    }
}
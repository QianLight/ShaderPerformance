using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BluePrint
{
    public class BlueprintRuntimeConnection
    {
        public BlueprintRuntimePin startPin;
        public BlueprintRuntimePin endPin;

        public BlueprintRuntimeConnection(BlueprintRuntimePin start, BlueprintRuntimePin end)
        {
            startPin = start;
            endPin = end;
        }
    }
}

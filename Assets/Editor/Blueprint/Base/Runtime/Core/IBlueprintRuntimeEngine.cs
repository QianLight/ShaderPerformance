using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BluePrint
{
    public interface IBlueprintRuntimeEngine
    {
        void Update();

        //BlueprintRuntimeGraph GetCurrentRunningGraph();

        bool IsRunning();

        bool IsPausing();

        bool IsStopping();

        BlueprintRuntimeGraph GetGraph(int graphID);

        BlueprintRuntimeGraph GetMainGraph();

    }
}

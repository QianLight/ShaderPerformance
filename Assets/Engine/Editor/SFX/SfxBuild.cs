using System;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

namespace CFEngine.Editor
{
    public partial class BuildFx : PreBuildPreProcess
    {
        public override string Name { get { return "Fx"; } }
        public override int Priority
        {
            get
            {
                return 1;
            }
        }

        public override void PreProcess()
        {
            base.PreProcess();
            //ProcessFolder("runtime/sfx", "fxloadlist");            
        }
    }
}
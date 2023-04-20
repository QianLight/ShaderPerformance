using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using CFEngine;
using CFEngine.Editor;
public partial class BuildTable : PreBuildPreProcess
{
    public override string Name { get { return "Table"; } }
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
        ProcessFolder("table", "tablelist");
    }
}
/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;


namespace Zeus.Framework.Hotfix
{
    public class HotFixReport : BaseHotFixStep
    {
        public HotFixReport(HotfixService executer) : base(executer, HotfixStep.Report)
        { }

        public override void Run()
        {
            OnProcess(0, 1);



            OnProcess(1, 1);
            NextStep();
        }
    }
}
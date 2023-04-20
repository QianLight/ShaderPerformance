using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GMSDK
{
    public class GPMGraphicLevel
    {
        public static int RequestGraphicLevel()
        {
            return GPMCXXBridge.RequestGraphicLevel();
        }
    }
}
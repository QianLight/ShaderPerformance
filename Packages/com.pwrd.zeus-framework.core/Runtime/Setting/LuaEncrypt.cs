/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using System;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using Zeus.Core.FileSystem;

namespace Zeus.Core
{
    public enum LuaEncrypt
    {
        None = 1,         //原生lua
        Bytecode = 2,    //Bytecode
    }
}

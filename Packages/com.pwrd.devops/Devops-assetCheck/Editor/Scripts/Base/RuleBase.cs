using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetCheck
{
    public abstract class RuleBase
    {
    }

    // 整个规则最后是以二维表的形式输出的
    // 使用这个接口多数情况是统计信息，所以不清理变量，所以注意需要在Check函数中清理需要清理的数据
    public interface CSVOutput
    {
        List<List<string>> ResultOutput(out string fileName);
    }
}

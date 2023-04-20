/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEditor;
using UnityEditor.Build;

namespace Zeus.Build
{
    public interface IInternalBeforeBuild
    {
        void OnInternalBeforeBuild(BuildTarget target, string outputPath);
    }
}

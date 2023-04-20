/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;

namespace Zeus.Build
{
    public interface IBeforeBuild
    {
        /// <summary>
        /// 在BuildPlayer前执行，优先于PreProcessBuildAttribute>=IPreprocesssBuild。
        /// </summary>
        /// <param name="target">目标平台</param>
        /// <param name="outputPath">完整输出路径</param>
        void OnBeforeBuild(BuildTarget target, string outputPath);
    }
}
#endif
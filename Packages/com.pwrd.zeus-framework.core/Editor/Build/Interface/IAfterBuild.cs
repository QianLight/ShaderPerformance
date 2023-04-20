/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;

namespace Zeus.Build
{
    public interface IAfterBuild
    {
        /// <summary>
        /// 在Build结束后执行，在PostprocessBuildAttribute和IPostProcessBuild之后，抛出异常时不执行。
        /// </summary>
        /// <param name="target">目标平台</param>
        /// <param name="locationPathName">完整输出路径</param>
        void OnAfterBuild(BuildTarget target, string locationPathName);
    }
}
#endif
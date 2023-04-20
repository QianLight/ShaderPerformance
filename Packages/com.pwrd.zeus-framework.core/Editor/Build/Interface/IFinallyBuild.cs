/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;

namespace Zeus.Build
{
    public interface IFinallyBuild
    {
        /// <summary>
        /// 在Build结束最后执行，不管过程中是否抛出异常都执行。
        /// </summary>
        /// <param name="target">目标平台</param>
        /// <param name="locationPathName">完整输出路径</param>
        void OnFinallyBuild(BuildTarget target, string locationPathName);
    }
}
#endif
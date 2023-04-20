/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;

namespace Zeus.Build
{
    public interface IModifyPlayerSettings
    {
        /// <summary>
        /// 在Build最初执行，将影响输出路径，如:EditorUserBuildSettings.exportAsGoogleAndroidProject = true将会影响Android输出为.apk还是project。
        /// </summary>
        /// <param name="target"></param>
        void OnModifyPlayerSettings(BuildTarget target);
    }
}
#endif
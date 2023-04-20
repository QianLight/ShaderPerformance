using UnityEditor.Callbacks;
using UnityEngine;

namespace UnityEditor.XBuild
{

    public sealed class PostProcessBuildEditor
    {
        public static void OnBuildingEnd(BuildTarget target, string path)
        {
            ProjectSettingIOS ps = null;
#if UNITY_IOS //internal
            ps = new ProjectSettingIOS_Internal();
#endif
            if (ps == null)
            {
                Debug.LogError("No platform matched, please check it!");
                return;
            }
            ps.PostProcessBuild(target, path);
            Debug.Log("Build Task over !");
        }
    }
}
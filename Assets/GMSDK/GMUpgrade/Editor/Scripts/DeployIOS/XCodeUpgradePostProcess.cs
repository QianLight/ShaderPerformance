#if UNITY_EDITOR && UNITY_IOS
using GMSDKUnityEditor.XCodeEditor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
#endif

public class XCodeUpgradePostProcess
{
#if UNITY_EDITOR && UNITY_IOS
    //[PostProcessBuild(GMSDKEnv.XCodePostProcessPriority+3)]
    public static void OnPostProcessBuild_CF(BuildTarget target, string pathToBuildProject)
    {
        if (target == BuildTarget.iOS)
        {
            Debug.Log("Run XCodeRecommendContactPostProcess to Config Xcode project.");
        }
        else
        {
            return;
        }
        
        XCProject project = new XCProject(pathToBuildProject);
        
        DeployIOSUpgrade.Deploy(pathToBuildProject);
        
        project.Save();
    }
#endif
}
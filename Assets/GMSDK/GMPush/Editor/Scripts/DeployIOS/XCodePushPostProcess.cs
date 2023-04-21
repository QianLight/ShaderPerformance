#if UNITY_EDITOR && UNITY_IOS
using GMSDKUnityEditor.XCodeEditor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
#endif

public class XCodePushPostProcess
{
#if UNITY_EDITOR && UNITY_IOS
    //[PostProcessBuild(GMSDKEnv.XCodePostProcessPriority+2)]
    public static void OnPostProcessBuild_CF(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.iOS)
        {
            Debug.Log("Run XCodeRecommendContactPostProcess to Config Xcode project.");
        }
        else
        {
            return;
        }
        
        XCProject project = new XCProject(pathToBuiltProject);
        
        DeployIOSPush.Deploy(pathToBuiltProject);
        
        project.Save();
    }
#endif     
}
using GMSDKUnityEditor.XCodeEditor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public static class XCodeRTCPostProcess
{
#if UNITY_EDITOR && UNITY_IOS
    [PostProcessBuild(GMSDKEnv.XCodePostProcessPriority+2)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuildProject)
    {
        if (target == BuildTarget.iOS)
        {
            Debug.Log("Run XCodeVoicePostProcess to Config Xcode project.");
        }
        else
        {
            return;
        }
        
        // Create a new project object from build target
        XCProject project = new XCProject(pathToBuildProject);
        
        DeployIOSRTC.Deploy(pathToBuildProject);

        string projmodFilePath = GMRTCEnv.Instance.PATH_RTC_EDITOR + "/Resources/GMRTCXcodeConfig.gsdkprojmods";
        Debug.Log("ProjMod File: " + projmodFilePath);
        project.ApplyMod(projmodFilePath);
        project.Save();
    }
#endif
}
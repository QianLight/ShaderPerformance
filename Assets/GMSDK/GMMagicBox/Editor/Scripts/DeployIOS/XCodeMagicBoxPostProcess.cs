using UnityEngine;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using GMSDKUnityEditor.XCodeEditor;
#endif
using System.IO;

public static class XCodeMagicBoxPostProcess
{

#if UNITY_EDITOR && UNITY_IOS
	[PostProcessBuild(1011)]
	public static void OnPostProcessBuild_CF( BuildTarget target, string pathToBuiltProject )
	{
#if UNITY_5 || UNITY_2017 || UNITY_2017_1_OR_NEWER
        if (target == BuildTarget.iOS)
        {
#else
        if (target == BuildTarget.iPhone) {
#endif
            Debug.Log("Run XCodeReplayPostProcess to Config Xcode project.");
        }
        else
        {
            return;
        }

        ConfigModel configModel = ConfigSettings.Instance.configModel;
        if (configModel.importMagicBox)
        {
            DeployIOSMagicBox.Deploy(pathToBuiltProject);
        }
    }

#endif

    public static void Log(string message)
    {
        UnityEngine.Debug.Log("PostProcess: " + message);
    }
}

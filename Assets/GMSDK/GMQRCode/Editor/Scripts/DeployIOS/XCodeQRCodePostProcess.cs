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

public static class XCodeQRCodePostProcess {

	#if UNITY_EDITOR && UNITY_IOS
	[PostProcessBuild(GMSDKEnv.XCodePostProcessPriority+13)]
	public static void OnPostProcessBuild( BuildTarget target, string pathToBuiltProject )
	{
	#if UNITY_5||UNITY_2017||UNITY_2017_1_OR_NEWER
		if (target == BuildTarget.iOS)
		{
	#else
	if (target == BuildTarget.iPhone) {
	#endif
			Debug.Log("Run XCodeQRCodePostProcess to Config Xcode project.");
		}
		else
		{
			return;
		}

		

		// Create a new project object from build target
		XCProject project = new XCProject( pathToBuiltProject );

		DeployIOSQRCode.Deploy(pathToBuiltProject);

		// Finally save the xcode project
		project.Save();
	}

	#endif

	public static void Log(string message)
	{
		UnityEngine.Debug.Log("PostProcess: "+message);
	}
}

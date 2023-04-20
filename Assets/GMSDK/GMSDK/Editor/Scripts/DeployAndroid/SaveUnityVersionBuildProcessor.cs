using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

#pragma warning disable 0618
class SaveUnityVersionBuildProcessor : IPreprocessBuild
{
#pragma warning restore 0618
    public int callbackOrder
    {
        get { return 1; }
    }

    public void OnPreprocessBuild(BuildTarget target, string path)
    {
        Debug.Log("start OnPreprocessBuild");
        ScriptableObject.CreateInstance<DomesticConfigSetting>().initPanelData();
        Debug.Log("SaveUnityVersionBuildProcessor.OnPreprocessBuild for target " + target + " at path " + path);
        List<string> lines = new List<string>();
        lines.Add("unity_version=" + Application.unityVersion);
        string content = string.Join("\n", lines.ToArray());
        string userPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        Debug.Log("OnPreprocessBuild for mac. user path:" + userPath);
        string gsdkDir = Path.Combine(userPath, ".gsdk");
        if (!Directory.Exists(gsdkDir))
        {
            Directory.CreateDirectory(gsdkDir);
        }
#if UNITY_EDITOR_WIN // window系统
        Debug.Log("OnPreprocessBuild for window. unity version:" + Application.unityVersion);
        string baseInfoPath = Path.Combine(gsdkDir, "gsdk_base_info.txt");
        Debug.Log("OnPreprocessBuild for mac. baseInfoPath:" + baseInfoPath);
        File.WriteAllText(baseInfoPath, content);
#else // mac系统和linux系统
        Debug.Log("OnPreprocessBuild for mac. unity version:" + Application.unityVersion);
        string baseInfoPath = Path.Combine(gsdkDir, "gsdk_base_info.txt");
        Debug.Log("OnPreprocessBuild for mac. baseInfoPath:" + baseInfoPath);
        File.WriteAllText(baseInfoPath, content);
#endif
    }
}
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
/// <summary>
/// 命令行编译
/// </summary>
public class GMEnderCommandBuild
{
    public const string version = "2.0";
    public const string build = "1";
    const string EnderVersion = "Ender平台/Ender Platform Version: " + version + " (build:" + build + ")";
    [MenuItem(EnderVersion, true, -30)]
    static bool ValidateSelection()
    {
        return false;
    }
    [MenuItem(EnderVersion, false, -30)]
    public static void ShowEnderVersion()
    {

    }

    public static string[] compatibleVersions = { "1.1" };
    public static bool isWarshipAppVersionCompatible(string appVersion)
    {
        if (appVersion == null)
        {
            return false;
        }
        if (appVersion.StartsWith(GMEnderCommandBuild.version))
        {
            return true;
        }
        foreach (string cVersion in compatibleVersions)
        {
            if (appVersion.StartsWith(cVersion))
            {
                return true;
            }
        }
        return false;
    }

//#if GMEnderOn
//    const string enderOnStatus = "Ender平台/已开启";
//#else
//    const string enderOnStatus = "Ender平台/已关闭";
//#endif
//    [MenuItem(enderOnStatus, true, -10)]
//    static bool EnderOnValidateSelection()
//    {
//        return false;
//    }
//    [MenuItem(enderOnStatus, false, -10)]
//    public static void e()
//    {

//    }

    [MenuItem("Ender平台/点击配置", false, 1)]
    public static void ConfigEnder()
    {
        Selection.activeObject = EnderEditorSettings.Instance;
    }

}
#endif

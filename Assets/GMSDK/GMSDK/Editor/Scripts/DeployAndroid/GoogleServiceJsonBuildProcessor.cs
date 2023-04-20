#if UNITY_2018_4_OR_NEWER
using System.IO;
using UnityEditor.Android;
using UnityEngine;

class GoogleServiceJsonBuildProcessor : IPostGenerateGradleAndroidProject
{
    public int callbackOrder
    {
        get { return 1; }
    }

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        Debug.Log("GoogleServiceJsonBuildProcessor.OnPreprocessBuild, path:" + path);
    }
}
#endif
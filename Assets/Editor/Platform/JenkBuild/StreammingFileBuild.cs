using System;
using System.Collections.Generic;
using System.IO;
using CFEngine;
using UnityEditor;
using UnityEngine;
using CFEngine.Editor;


public class StreammingFileBuild
{
    // 打包之前 打ab之后执行
    public static void ProcessStreamingFiles(BuildTarget target)
    {
        BuildLua.Build();
        //OniOSBuild(target);
        ProcessNative();
        Flush();
    }


    private static bool OniOSBuild(BuildTarget target)
    {
        if (IsIOSPatch(target))
        {
            System.Diagnostics.Process exep = new System.Diagnostics.Process();
            exep.StartInfo.FileName = "sh";
            exep.StartInfo.Arguments = HelperEditor.basepath + "/Shell/bundle_post.sh";
            exep.StartInfo.CreateNoWindow = true;
            exep.StartInfo.UseShellExecute = false;
            exep.StartInfo.RedirectStandardOutput = true;
            exep.StartInfo.StandardOutputEncoding = System.Text.Encoding.Default;
            exep.Start();
            string output = exep.StandardOutput.ReadToEnd();
            exep.WaitForExit();
            if (output != "")
            {
                Debug.Log(output);
            }
        }
        return true;
    }

    private static bool IsIOSPatch(BuildTarget target)
    {
        if (target == BuildTarget.iOS)
        {
            var ta = Resources.Load<TextAsset>("version");
            return ta != null;
        }
        return false;
    }


    private static void ProcessNative()
    {
        try
        {
            File.Copy(@"Assets/Plugins/Ecs/Xuthus.dll.Native",
                @"Assets/Plugins/Ecs/Xuthus.dll", true);
            AssetDatabase.ImportAsset(@"Assets/Plugins/Ecs/Xuthus.dll");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }


    private static void Flush()
    {
        Debug.Log("build flush");
        AssetDatabase.WriteImportSettingsIfDirty(Application.streamingAssetsPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

}
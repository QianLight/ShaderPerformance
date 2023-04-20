using UnityEditor;
using UnityEngine;
using System.IO;

public class ZipHandler
{
    public static void Build()
    {
#if UNITY_EDITOR_WIN
        WinParseZip();
#elif UNITY_EDITOR_OSX
        ZipAndroidRef();
#endif
    }

#if UNITY_EDITOR_WIN
    [MenuItem("Tools/Zip/ParseZip")]
    public static void WinParseZip()
    {
        string conf = HelperEditor.basepath + "/Shell/zip.txt";
        string[] lines = File.ReadAllLines(conf);
        for (int i = 0; i < lines.Length; i++)
        {
            if (!string.IsNullOrEmpty(lines[i]))
            {
                WinZip(lines[i]);
            }
        }

        string dest = Path.Combine(Application.streamingAssetsPath, "zipinfo.txt");
        AssetDatabase.CopyAsset(conf, dest);
    }

    [MenuItem("Tools/Zip/ParseUnZip")]
    public static void WinParseUnZip()
    {
        string p = Application.streamingAssetsPath + "/update/config.zip";
        Debug.Log(p);
        WinUnzip(p);
    }


    /**
     * 7z.exe 命令行工具
     * https://blog.csdn.net/embedded_sky/article/details/45201181
     **/
    public static void WinZip(string folder)
    {
        var path = Path.Combine(Application.streamingAssetsPath, folder);
        if (Directory.Exists(path))
        {
            string arg = @"a " + folder + ".zip " + path;
            if (Do7z(arg))
                MoveTarget(folder);
        }
        else
        {
            Debug.LogError("not exist: " + path);
        }
    }

    public static void WinUnzip(string zipFile)
    {
        string arg = @"e " + zipFile;
        if (Do7z(arg))
            Debug.Log("unzip success");
    }


    private static bool Do7z(string arg)
    {
        System.Diagnostics.Process exep = new System.Diagnostics.Process();
        exep.StartInfo.FileName = HelperEditor.basepath + "/Shell/7za.exe";
        exep.StartInfo.Arguments = arg;
        exep.StartInfo.CreateNoWindow = true;
        exep.StartInfo.UseShellExecute = false;
        exep.StartInfo.RedirectStandardOutput = true;
        exep.StartInfo.StandardOutputEncoding = System.Text.Encoding.Default;
        exep.Start();
        string output = exep.StandardOutput.ReadToEnd();
        exep.WaitForExit();
        if (output != "")
        {
            int errorIndex = output.IndexOf("error:");
            if (errorIndex >= 0)
            {
                string errorStr = output.Substring(errorIndex);
                Debug.LogError(errorStr);
                Debug.Log(output.Substring(0, errorIndex));
            }
            else
            {
                Debug.Log(output);
                return true;
            }
        }
        return false;
    }
#endif

    private static void MoveTarget(string folder)
    {
        var from = Path.Combine(HelperEditor.basepath, folder + ".zip");
        var dest = Path.Combine(Application.streamingAssetsPath, folder + ".zip");
        try
        {
            File.Move(from, dest);
            AssetDatabase.ImportAsset("Assets/StreamingAssets/" + folder + ".zip");
            AssetDatabase.Refresh();
        }
        catch (IOException e)
        {
            Debug.LogError(e.Message);
        }
    }

#if UNITY_EDITOR_OSX
     [MenuItem("Tools/Zip/ZipRef")]
    public static void ZipAndroidRef()
    {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            // string shell = HelperEditor.basepath + "/Shell/zip_filter.sh";
            // System.Diagnostics.Process.Start("/bin/bash", shell);
            
            System.Diagnostics.Process exep = new System.Diagnostics.Process();
            exep.StartInfo.FileName = "sh";
            exep.StartInfo.Arguments = HelperEditor.basepath + "/Shell/zip_filter.sh";
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
            Debug.Log("ZipAndroidRef");
        }
    }
    
    [MenuItem("Tools/Zip/UnZipRef")]
    public static void UnZipAndroidRef()
    {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            // string shell = HelperEditor.basepath + "/Shell/unzip_filter.sh";
            // System.Diagnostics.Process.Start("/bin/bash", shell);
            
            System.Diagnostics.Process exep = new System.Diagnostics.Process();
            exep.StartInfo.FileName = "sh";
            exep.StartInfo.Arguments = HelperEditor.basepath + "/Shell/unzip_filter.sh";
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
            Debug.Log("ZipAndroidRef");
        }
    }
#endif
}

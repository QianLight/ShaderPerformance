using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GenByteCode
{

    /*
     * 将lua 转成byte code
     */

#if UNITY_EDITOR_WIN

    [MenuItem("XLua/ByteCode")]
    public static void ByteCode()
    {
        System.Diagnostics.Process exep = new System.Diagnostics.Process();
        exep.StartInfo.FileName = "sh";
        exep.StartInfo.Arguments = HelperEditor.basepath + "/Shell/lua_bytecode_win.sh";
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
                ImportBytecodes();
            }
        }
    }


    static void ImportBytecodes()
    {
        string path = "Assets/StreamingAssets/lua2";
        AssetDatabase.ImportAsset(path);
        AssetDatabase.Refresh();
    }

#endif



    /*
     * osx 不再生成lua2, 而是直接覆盖
     */
#if UNITY_EDITOR_OSX

    [MenuItem("XLua/ByteCode(OSX)")]
    public static void ByetecodeOSX()
    {
        string shell = HelperEditor.basepath + "/Shell/lua_bytecode_osx.sh";
        System.Diagnostics.Process.Start("/bin/bash", shell);
        Debug.Log("ByetecodeOSX lua_bytecode_osx.sh");
       // ImportOSXBytecodes();
    }
    

    static void ImportOSXBytecodes()
    {
        string path = "Assets/StreamingAssets/lua";
        AssetDatabase.ImportAsset(path);
        AssetDatabase.Refresh();
        Debug.Log("byte code generated");
    }

#endif


    [MenuItem("XLua/Document")]
    public static void OpenLuaDOC()
    {
        Application.OpenURL("https://doc.weixin.qq.com/txdoc/word?scode=AHYA5gfqAAYQq4OPtWAPIAawYmAHI&docid=w2_APIAawYmAHImZuE8cXZQFmLPl8oY1&type=0");
    }
}

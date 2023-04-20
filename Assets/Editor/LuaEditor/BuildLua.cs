using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public partial class BuildLua
{
    const string tableDir = "Assets/StreamingAssets/table/";
    const string luaDir = "Assets/StreamingAssets/lua/";

    public static string[] SearchTable()
    {
        string dir = "Assets/StreamingAssets/lua/table";
        DirectoryInfo dirinfo = new DirectoryInfo(dir);
        FileInfo[] files = dirinfo.GetFiles("*.lua.txt");
        List<string> names = new List<string>();
        foreach (var file in files)
        {
            Debug.Log(file.Name);
            string name = file.Name.Replace(".lua.txt", "");
            if (name != "table")
            {
                names.Add(name);
            }
        }

        return names.ToArray();
    }


    private static void GenCrc32MainLua()
    {
        string path = Path.Combine(luaDir, "main.lua.txt");
        string target = Path.Combine(luaDir, "valid");
        string crc = HashHelper.ComputeCRC32(path);
        File.WriteAllText(target, crc);
        AssetDatabase.ImportAsset(target);
    }

    public static void Build()
    {
        GenCrc32MainLua();
#if UNITY_EDITOR_OSX
        GenByteCode.ByetecodeOSX();
#endif
        //        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        //        {
        //            CopyTableUsed();
        //        }
        //        else
        //        {
        //#if UNITY_EDITOR_WIN
        //            ZipHandler.WinParseZip();
        //            HandlerLuaTable();
        //#elif UNITY_EDITOR_OSX
        //            GenByteCode.ByetecodeOSX();
        //            ZipHandler.ZipAndroidRef();
        //#endif
        //        }
    }

    private static void CopyTableUsed()
    {
        string originDir = "Assets/BundleRes/Table/";
        if (Directory.Exists(tableDir))
        {
            Directory.Delete(tableDir, true);
        }

        Directory.CreateDirectory(tableDir);
        string[] patten = SearchTable();
        for (int i = 0; i < patten.Length; i++)
        {
            string from = originDir + patten[i] + ".bytes";
            string target = tableDir + patten[i] + ".bytes";
            File.Copy(from, target);
        }
    }
#if UNITY_EDITOR_WIN
    [MenuItem("XLua/LuaTable")]
    private static void HandlerLuaTable()
    {
        CopyTableUsed();
        ZipHandler.WinZip("table");
        Directory.Delete(tableDir, true);
        AssetDatabase.Refresh();
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
}
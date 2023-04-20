using UnityEditor;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.IO;
using CFEngine;
using CFEngine.Editor;

static class XNatives
{
    public static IntPtr? pecs_dll = null;

    [DllImport("kernel32.dll")]
    public static extern IntPtr LoadLibrary(string dllToLoad);

    [DllImport("kernel32.dll")]
    public static extern bool FreeLibrary(IntPtr hModule);
}

public class XEcsImporter : AssetPostprocessor
{

    [MenuItem("Tools/Ecs/Reimport")]
    static void Reimport()
    {
        ImportXuthus();
        ImportXuthus_VirtualServer();
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
        string[] movedFromAssetPaths)
    {

        if (EngineUtility.IsBuildingGame|| AssetsImporter.IsForbidAssetPostprocessor) return;

        //if (!EngineUtility.AutoAssetPostprocessor) return;

        foreach (string s in importedAssets)
        {
            if (s == @"Assets/Plugins/Ecs/Xuthus.dll.Native")
            {
                ImportXuthus();
            }

            if (s == @"Assets/Plugins/Ecs/Xuthus_VirtualServer.dll.Native")
            {
                ImportXuthus_VirtualServer();
            }
        }
    }

    private static void ImportXuthus()
    {
        XNatives.pecs_dll = XNatives.LoadLibrary(@"./Assets/Plugins/Ecs/Xuthus.dll");

        while (XNatives.FreeLibrary(XNatives.pecs_dll.Value)) ;
        XNatives.pecs_dll = null;

        File.Copy(@"Assets/Plugins/Ecs/Xuthus.dll.Native",
            @"Assets/Plugins/Ecs/Xuthus.dll", true);

        EditorPrefs.SetInt("ScriptCompilationDuringPlay", 1);
        AssetDatabase.Refresh();
                

        XNatives.pecs_dll = XNatives.LoadLibrary(@"./Assets/Plugins/Ecs/Xuthus.dll");
        if (XNatives.pecs_dll == null) throw new Exception("Load native dll failed.");
    }
    
    private static void ImportXuthus_VirtualServer()
    {
        XNatives.pecs_dll = XNatives.LoadLibrary(@"./Assets/Plugins/Ecs/Xuthus_VirtualServer.dll");

        while (XNatives.FreeLibrary(XNatives.pecs_dll.Value)) ;
        XNatives.pecs_dll = null;

        File.Copy(@"Assets/Plugins/Ecs/Xuthus_VirtualServer.dll.Native",
            @"Assets/Plugins/Ecs/Xuthus_VirtualServer.dll", true);

        AssetDatabase.Refresh();

        XNatives.pecs_dll = XNatives.LoadLibrary(@"./Assets/Plugins/Ecs/Xuthus_VirtualServer.dll");
        if (XNatives.pecs_dll == null) throw new Exception("Load native dll failed.");
    }
}
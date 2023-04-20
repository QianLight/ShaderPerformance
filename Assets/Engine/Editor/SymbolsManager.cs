using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Compilation;

public static class SymbolsManager 
{

    public static bool Contains(string symbol, BuildTargetGroup target = BuildTargetGroup.Unknown)
    {
        if (target == BuildTargetGroup.Unknown)
        {
            target = EditorUserBuildSettings.selectedBuildTargetGroup;
        }
        PlayerSettings.GetScriptingDefineSymbolsForGroup(target, out string[] defines);
        return Array.IndexOf(defines, symbol) >= 0;
    }

    public static void Set(string symbol, bool enable, BuildTargetGroup target = BuildTargetGroup.Unknown)
    {
        if (target == BuildTargetGroup.Unknown)
        {
            target = EditorUserBuildSettings.selectedBuildTargetGroup;
        }
        if (Contains(symbol, target) != enable)
        {
            PlayerSettings.GetScriptingDefineSymbolsForGroup(target, out string[] defines);
            if (enable)
                ArrayUtility.Add(ref defines, symbol);
            else
                ArrayUtility.Remove(ref defines, symbol);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(target, defines);
            CompilationPipeline.RequestScriptCompilation();
        }
    }
}

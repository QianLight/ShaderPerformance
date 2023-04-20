using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SFXAssetChecker : Editor
{
    private readonly static string path = "Assets/Effects/Mesh";

    [MenuItem("Tools/特效/模型检查")]
    private static void MeshReadwriteChecker()
    {
        string fPath = Application.dataPath;
        string tPath = fPath + path;
        var objects = new List<Mesh>();
        var AssetPaths = AssetDatabase.FindAssets($"t:Mesh",new[]{path});
        string result = "";
        for (int i = 0; i < AssetPaths.Length; i++)
        {
            Mesh asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(AssetPaths[i]), typeof(Mesh)) as Mesh;
            if (!asset.isReadable)
            {
                result += asset.name + '\n';
            }
        }
        Debug.LogError($"未开启ReadWrite列表:\n"+result);
    }
}

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GradientPalette : ScriptableObject
{
    public List<string> guids = new List<string>();
    public List<Gradient> gradients = new List<Gradient>();

    private static GradientPalette Instance
    {
        get
        {
            string[] guids = AssetDatabase.FindAssets("t:GradientPalette");
            string assetPath;
            if (guids.Length == 0)
            {
                assetPath = "Assets/Engine/Editor/EditorResources/GradientPalette.asset";
                AssetDatabase.CreateAsset(CreateInstance<GradientPalette>(), assetPath);
            }
            else
            {
                assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            }
            return AssetDatabase.LoadAssetAtPath<GradientPalette>(assetPath);
        }
    }

    public static bool Get(string assetPath, out Gradient gradient)
    {
        var instance = Instance;
        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        int index = instance.guids.IndexOf(guid);
        if (index >= 0)
        {
            gradient = instance.gradients[index];
            return true;
        }
        else
        {
            gradient = new Gradient();
            instance.gradients.Add(gradient);
            instance.guids.Add(guid);
            return false;
        }
    }

    public static void Set(string assetPath, Gradient gradient)
    {
        var instance = Instance;
        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        int index = instance.guids.IndexOf(guid);
        if (index >= 0)
        {
            instance.gradients[index] = gradient;
        }
        else
        {
            instance.guids.Add(guid);
            instance.gradients.Add(gradient);
        }
        EditorUtility.SetDirty(instance);
        AssetDatabase.SaveAssets();
    }
}
#endif

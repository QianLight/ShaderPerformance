using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GUIResourceHelper
{
    static Dictionary<string, Object> LoadedAssets = new Dictionary<string, Object>();

    public static Object GetAsset(string path)
    {
        if (LoadedAssets.ContainsKey(path))
            return LoadedAssets[path];
        Object asset = Resources.Load(path);
        if(asset != null)
        {
            LoadedAssets.Add(path, asset);
        }
        return asset;
    }
}

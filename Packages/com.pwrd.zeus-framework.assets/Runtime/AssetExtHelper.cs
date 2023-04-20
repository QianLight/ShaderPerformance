/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.IO;
using UnityEngine;
using Zeus.Framework.Asset;

public static class AssetExtHelper
{
    private static readonly string assetPathPrefix;
    private static readonly bool addAssetPathPrefix;
    static readonly string[] audioExt = {".wav", ".ogg", ".mp3"};
    static readonly string[] animExt = {".anim"};
    static readonly string[] prefabExt = {".prefab"};
    static readonly string[] textureExt = {".png", ".jpg", ".PNG", ".JPG", ".tga", ".TGA", ".exr", ".psd", ".hdr", ".cubemap", ".bmp"};
    static readonly string[] shaderExt = {".shader"};
    static readonly string[] animationControllerExt = {".controller"};
    static readonly string[] materialExt = {".mat"};
    static readonly string[] spriteAtlas = {".spriteatlas"};
    static readonly string[] overrideController = {".overrideController"};
    private static string[] shadervariants = {".shadervariants"};
    static readonly string[] scriptableObject = {".asset", ".playable"};

    private static readonly string[] assetExt = 
    {
        ".unity",
        ".wav", ".ogg", ".mp3",
        ".anim",
        ".prefab",
        ".png", ".jpg", ".PNG", ".JPG", ".tga", ".TGA", ".exr", ".psd", ".hdr", ".cubemap", ".bmp",
        ".shader",
        ".controller",
        ".mat",
        ".spriteatlas",
        ".overrideController",
        ".shadervariants",
        ".asset", ".playable"
    };

    static readonly string[] allExt =
    {
        ".unity",
        ".wav", ".ogg", ".mp3",
        ".anim",
        ".prefab",
        ".bytes", ".json", ".xml",
        ".png", ".jpg", ".PNG", ".JPG", ".tga", ".TGA", ".exr", ".psd", ".hdr", ".cubemap", ".bmp",
        ".shader",
        ".controller",
        ".mat",
        ".spriteatlas",
        ".overrideController",
        ".shadervariants",
        ".asset", ".playable",
        ".txt"
    };

    static AssetExtHelper()
    {
        var setting = AssetManagerSetting.LoadSetting();
        assetPathPrefix = setting.assetDatabaseSetting.AssetPathPrefix;
        addAssetPathPrefix = setting.assetDatabaseSetting.AddAssetPathPrefix;
    }
    
    private static bool CheckExt(string path, string[] extList)
    {
        foreach (var ext in extList)
        {
            if (path.EndsWith(ext))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 根据资源路径(需要后缀)获取资源的类型
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Type GetAssetType(string path)
    {
        if (path == null)
        {
            return null;
        }
        
        if (CheckExt(path, prefabExt))
        {
            return typeof(GameObject);
        }

        if (CheckExt(path, audioExt))
        {
            return typeof(AudioClip);
        }

        if (CheckExt(path, animExt))
        {
            return typeof(AnimationClip);
        }

        if (CheckExt(path, materialExt))
        {
            return typeof(Material);
        }

        if (CheckExt(path, shaderExt))
        {
            return typeof(Shader);
        }

        if (CheckExt(path, textureExt))
        {
            return typeof(Texture);
        }
        
        if (CheckExt(path, scriptableObject))
        {
            return typeof(ScriptableObject);
        }

        if(CheckExt(path, spriteAtlas))
        {
            return typeof(UnityEngine.U2D.SpriteAtlas);
        }

        return typeof(UnityEngine.Object);
    }

    /// <summary>
    /// 根据资源的加载路径获取其完整路径(带前缀和后缀名) 不传type时效率较低
    /// </summary>
    /// <param name="path"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetFullPathByType(string path, Type type = null)
    {
#if UNITY_EDITOR
        var extension = Path.GetExtension(path);
        if (!string.IsNullOrEmpty(extension))
        {
            return path;
        }

        string[] exts = (type == null) ? allExt : GetFileExtArray(type);
        foreach (var ext in exts)
        {
            string fullPath = TryAddPrefixPath(path) + ext;
            var obj = UnityEditor.AssetDatabase.LoadAssetAtPath(fullPath, type ?? GetAssetType(fullPath));
            if (obj != null)
            {
                return fullPath;
            }
        }
#endif
        return null;
    }
    
    private static string TryAddPrefixPath(string path)
    {
        if (addAssetPathPrefix && !path.StartsWith("Packages") && !path.StartsWith("Assets"))
        {
            path = Path.Combine(assetPathPrefix, path);
        }
        return path;
    }

    /// <summary>
    /// 返回资源的后缀名 (若传入的路径已有后缀则返回空)
    /// </summary>
    /// <param name="path"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetFileExt(string path, Type type = null, bool IsLog = false)
    {
#if UNITY_EDITOR
        var extension = Path.GetExtension(path);
        if(!string.IsNullOrEmpty(extension))
        {
            return "";
        }
        var fullPath = GetFullPathByType(path, type);
        if (fullPath != null)
        {
            return Path.GetExtension(fullPath);
        }
        if(IsLog)
            Debug.LogError("Not found ext " + path + " " + type);
#endif
        return "";
    }

    private static string[] GetFileExtArray(Type type)
    {
        if (type == typeof(GameObject))
        {
            return prefabExt;
        }
        if (type == typeof(AudioClip))
        {
            return audioExt;
        }
        if (type == typeof(Shader))
        {
            return shaderExt;
        }
        if (type == typeof(Texture) || type == typeof(Texture2D) || type == typeof(Cubemap))
        {
            return textureExt;
        }
        if (type == typeof(Material))
        {
            return materialExt;
        }
        if (type == typeof(UnityEngine.U2D.SpriteAtlas))
        {
            return spriteAtlas;
        }
        if (type == typeof(AnimatorOverrideController))
        {
            return overrideController;
        }
        if (type == typeof(AnimationClip))
        {
            return animExt;
        }
        if (type == typeof(RuntimeAnimatorController))
        {
            return animationControllerExt;
        }
        if (type == typeof(ScriptableObject))
        {
            return scriptableObject;
        }
        return allExt;
    }

    public static bool IsUnityAsset(string path)
    {
        return CheckExt(path, assetExt);
    }

    public static string RemoveExt(string path)
    {
        if (path == null)
            return null;

        int extLength = Path.GetExtension(path).Length;
        return path.Substring(0, path.Length - extLength);
    }
}
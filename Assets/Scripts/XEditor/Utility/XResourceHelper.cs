﻿#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using CFUtilPoolLib;

using UnityEditor;
using System.IO;

class TypeSpecializer<T>
{
    static string suffix = string.Empty;
    public static string Suffix { get { return suffix; } }

    static TypeSpecializer()
    {
        TypeSpecializer<AnimationClip>.suffix = ".anim";
        TypeSpecializer<GameObject>.suffix = ".prefab";
        TypeSpecializer<XCurve>.suffix = ".prefab";
        TypeSpecializer<AvatarMask>.suffix = ".mask";
    }
}

public class XResourceHelper : XSingleton<XResourceHelper>
{
    public static T LoadEditorResourceAtBundleRes<T>(string path, bool bInstantiate = false) where T : UnityEngine.Object
    {
        T o = AssetDatabase.LoadAssetAtPath<T>("Assets/BundleRes/" + path + GetSuffix<T>());
        if (bInstantiate)
            return UnityEngine.GameObject.Instantiate<T>(o, null);
        return o;

    }

    static string GetSuffix<T>() where T : UnityEngine.Object
    {
        return TypeSpecializer<T>.Suffix;
    }

    public static void LoadAllAssets(string folder, List<UnityEngine.Object> outputFiles)
    {
        DirectoryInfo direction = new DirectoryInfo(folder);
        FileSystemInfo[] fs = direction.GetFileSystemInfos();

        for (int i = 0; i < fs.Length; i++)
        {
            if (fs[i] is DirectoryInfo)
            {
                LoadAllAssets(fs[i].FullName, outputFiles);
            }
            else if (fs[i] is FileInfo)
            {
                if (fs[i].FullName.EndsWith(".meta")) continue;
                int index = fs[i].FullName.IndexOf("Assets\\");
                string path = fs[i].FullName.Substring(index).Replace('\\', '/');

                var obj = AssetDatabase.LoadMainAssetAtPath(path);
                if (obj != null)
                    outputFiles.Add(obj);
            }
        }
    }

}
#endif
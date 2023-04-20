using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum EditorPrefabFlagEnum
{
    ForceCopyGameObject = 1 << 0,
}

public class EditorPrefabFlager : MonoBehaviour
{
    public EditorPrefabFlagEnum flags;

    private static readonly List<EditorPrefabFlager> temp = new List<EditorPrefabFlager>();

    public static List<EditorPrefabFlager> Get(GameObject root, EditorPrefabFlagEnum flag)
    {
        if (!root)
        {
            return new List<EditorPrefabFlager>();
        }
        List<EditorPrefabFlager> result = new List<EditorPrefabFlager>();
        root.GetComponentsInChildren(true, temp);
        foreach (EditorPrefabFlager flager in temp)
        {
            if ((flager.flags & flag) > 0)
            {
                result.Add(flager);
            }
        }
        return result;
    }
}

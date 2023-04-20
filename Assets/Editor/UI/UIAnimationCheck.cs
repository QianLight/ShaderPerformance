using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.CFUI;

public class UIAnimationCheck 
{
    [UnityEditor.MenuItem("Tools/UI/UIAnimationCheck &9")]
    static void UIAnimationCheckMethod()
    {
        var obj = Selection.activeGameObject;
        var anims = obj.GetComponentsInChildren<CFAnimation>();
        foreach (var it in anims)
        {
            if (it.AnimList != null)
            {
                foreach (var i in it.AnimList)
                {
                    string path = GetPath(it.transform);
                    if (i.Target == null)
                    {
                        Debug.LogError(path + "  Target is null");
                    }

                    if (i.Target != null && i.m_Type == CFAnimType.Alpha)
                    {
                        if (i.Target.GetComponent<CanvasGroup>() == null)
                        {
                            path = GetPath(i.Target.transform);
                            Debug.LogError(path + "  CanvasGroup is null");
                        }
                    }
                    if (i.m_Curve == null)
                    {
                        Debug.LogError(path + "  Curve is null");
                    }
                }
            }
        }
    }

    private static string GetPath(Transform transform)
    {
        string str = string.Empty;
        Transform t = transform;
        while(t != null)
        {
            str = t.name + "/" + str; 
            t = t.parent;
        }
        str = str.TrimEnd('/');
        return str;
    }
}


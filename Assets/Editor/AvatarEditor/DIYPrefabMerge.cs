using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class DIYPrefabMerge
{

    const string prefix = "Assets/BundleRes/Runtime/Prefab";

    const string diy_name = "DIY_TPL";


    [MenuItem("Tools/常用命令/Clean Player Prefs")]
    static void CleanPrefers()
    {
        PlayerPrefs.DeleteAll();
        //EditorPrefs.DeleteAll();
        Debug.Log("Clean pref Done");
    }


    [MenuItem("Tools/DIY/DIYPart")]
    static void DIYPartMerge()
    {
        CleanEnv();

        var files = Match();
        string pat = prefix + "/" + files[0];
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(pat);
        GameObject go = GameObject.Instantiate(prefab);
        go.name = diy_name;
        var parts = Parse(go);
        TrimGo(go);

        Transform t = null;
        for (int i = 1; i < files.Count; i++)
        {
            string pat2 = prefix + "/" + files[i];
            var go2 = AssetDatabase.LoadAssetAtPath<GameObject>(pat2);
            var parts2 = Parse(go2);
            for (int j = 0; j < parts2.Count; j++)
            {
                if (!parts.Contains(parts2[j]))
                {
                    t = go2.transform.GetChild(j);
                    var g = GameObject.Instantiate(t.gameObject);
                    g.name = TrimPart(t.name);
                    g.transform.SetParent(go.transform);
                    g.transform.localPosition = Vector3.zero;
                    g.transform.localScale = Vector3.one;
                    g.transform.localEulerAngles = new Vector3(-90, 0, 0);

                    parts.Add(parts2[j]);
                }
            }
        }

        Debug.Log("child cnt: " + go.transform.childCount);

        EditorUtility.DisplayDialog("tip", "生成完毕，需要在Global表配置Ship_Parts_Num: " + go.transform.childCount, "ok");
    }


    static void CleanEnv()
    {
        GameObject g = GameObject.Find(diy_name);
        if (g != null)
        {
            GameObject.DestroyImmediate(g);
        }
    }

    static void TrimGo(GameObject go)
    {
        var parts = go.GetComponentsInChildren<Transform>();
        foreach(var p in parts)
        {
            p.name = TrimPart(p.name);
        }
    }

    static string TrimPart(string part)
    {
        int t1 = part.IndexOf('_');
        int t2 = part.IndexOf('_', 4);
        if (t2 > t1 && t2 < part.Length)
            return part.Substring(0, t1) + part.Substring(t2);
        return part;
    }


    static List<string> Parse(GameObject go)
    {
        var filters = go.GetComponentsInChildren<MeshFilter>();
        List<string> parts = new List<string>();
        for (int i = 0; i < filters.Length; i++)
        {
            var t = TrimPart(filters[i].gameObject.name);
            parts.Add(t);
        }
        return parts;
    }


    static List<string> Match()
    {
        List<string> rst = new List<string>();
        DirectoryInfo dir = new DirectoryInfo(prefix);
        var files = dir.GetFiles("OP_*_DIY.prefab");
        for (int i = 0; i < files.Length; i++)
        {
            Debug.Log(files[i].Name);
            rst.Add(files[i].Name);
        }
        return rst;
    }

}

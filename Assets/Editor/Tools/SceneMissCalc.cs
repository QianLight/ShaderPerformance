using System;
using System.Collections.Generic;
using A;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SceneMissCalc
{
    public static void Search(String path, List<string> results)

    {
        if (path != "")
        {
            EditorSceneManager.OpenScene(path);
        }

        Scene scene = SceneManager.GetActiveScene();
        GameObject[] gos = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject go in gos) Traverse(scene, go.transform,results);
        if (results.Count > 0)
        {
            Debug.LogError(" > Total Results: " + results.Count);
        }
    }

    [MenuItem("Tools/场景依赖丢失/当前场景")]
    public static void SearchMenu()
    {
        List<string> results = new List<string>();
        Search("", results);
        
        foreach (string result in results) Debug.LogError("> " + result);
    }

    private static string[] FindEnabledEditorScenes()
    {
        List<string> editorScenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            {
                editorScenes.Add(scene.path);
            }
        }

        return editorScenes.ToArray();
    }

    [MenuItem("Tools/场景依赖丢失/整个场景")]
    public static List<String> SearchAll()
    {
        var paths = FindEnabledEditorScenes();
        Dictionary<String, List<String>> results = new Dictionary<String, List<String>>();


        foreach (var path in paths)
        {
            var res = new List<String>();
            results.Add(path, res);
            Search(path, res);
        }

        var missGb = new List<String>();
        foreach (var result in results.Values)
            missGb.AddRange(result);
        foreach (string result in missGb) Debug.LogError("> " + result);
        return missGb;
    }


    private static void AppendComponentResult(String sceneName,string childPath, int index, List<String> res)
    {
        res.Add( "Scene:"+sceneName + "   Missing Component " + index + " of " + childPath);
    }

    private static void AppendTransformResult(String sceneName,string childPath, string name, List<String> res)
    {
        res.Add("Scene:"+sceneName + "   Missing Prefab for \"" + name + "\" of " + childPath);
    }

    private static void Traverse(Scene scene, Transform transform, List<String> res, string path = "")
    {
        string thisPath = path + "/" + transform.name;
        Component[] components = transform.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == null) AppendComponentResult(scene.name,thisPath, i, res);
        }

        for (int c = 0; c < transform.childCount; c++)
        {
            Transform t = transform.GetChild(c);
            PrefabAssetType pt = PrefabUtility.GetPrefabAssetType(t.gameObject);
            if (pt == PrefabAssetType.MissingAsset)
            {
                AppendTransformResult(scene.name,path + "/" + transform.name, t.name,res);
            }
            else
            {
                Traverse(scene, t, res,thisPath);
            }
        }
    }
}
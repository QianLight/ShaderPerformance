#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
 
public class NullComponentAndMissingPrefabSearchTool
{
  [MenuItem("Tools/引擎/资源检查/Log Missing Prefabs And Components")]
  public static void Search()
  {
    results.Clear();
    GameObject[] gos = SceneManager.GetActiveScene().GetRootGameObjects();
    foreach (GameObject go in gos) Traverse(go.transform);
    Debug.Log("> Total Results: " + results.Count);
    foreach (string result in results) Debug.Log("> " + result);
  }

  private static List<string> results = new List<string>();
  private static void AppendComponentResult(string childPath, int index)
  {
    results.Add("Missing Component " + index + " of " + childPath);
  }
  private static void AppendTransformResult(string childPath, string name)
  {
    results.Add("Missing Prefab for \"" + name + "\" of " + childPath);
  }
  private static void Traverse(Transform transform, string path = "")
  {
    string thisPath = path + "/" + transform.name;
    Component[] components = transform.GetComponents<Component>();
    for (int i = 0; i < components.Length; i++)
    {
      if (components[i] == null) AppendComponentResult(thisPath, i);
    }
    for (int c = 0; c < transform.childCount; c++)
    {
      Transform t = transform.GetChild(c);
      PrefabAssetType pt = PrefabUtility.GetPrefabAssetType(t.gameObject);
      if (pt == PrefabAssetType.MissingAsset)
      {
        AppendTransformResult(path + "/" + transform.name, t.name);
      } else
      {
        Traverse(t, thisPath);
      }
    }
  }

  // [MenuItem("Tools/2020-丢失清理 #C", false, 1)]
  public static void Clear()
  {
    GameObject[] gos = SceneManager.GetActiveScene().GetRootGameObjects();
    for (var index = gos.Length - 1; index >= 0; index--)
    {
      GameObject go = gos[index];
      if (go.name.Contains("Recovery GameObject") || go.name.Contains("Missing Prefab"))
      {
        results.Add("Missing GameObject " + go.name + " is removed");
        GameObject.DestroyImmediate(go);
      }
    }
    foreach (string result in results) Debug.Log("> " + result);
  }


    public static void ClearMissing()
    {
        GameObject[] gos = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject go in gos) TraverseMissing(go.transform);
    }


    private static void TraverseMissing(Transform transform, string path = "")
    {
        string thisPath = path + "/" + transform.name;


        if (IsExistMissingScripts(transform.gameObject))
        {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject);
        }         

        for (int c = 0; c < transform.childCount; c++)
        {
            Transform t = transform.GetChild(c);
            if (t.gameObject.name.Contains("Missing Prefab"))
            {
                GameObject.DestroyImmediate(t.gameObject);
            }
            else
            {
                TraverseMissing(t, thisPath);
            }
        }
    }

    private static bool IsExistMissingScripts(GameObject go)
    {
        int missingNum = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
        if (missingNum > 0)
        {
            return true;
        }
        else
        {
            foreach (Transform child in go.transform)
            {
                bool isHasMissing = IsExistMissingScripts(child.gameObject);
                if (isHasMissing) return isHasMissing;
            }
        }

        return false;
    }
}
#endif
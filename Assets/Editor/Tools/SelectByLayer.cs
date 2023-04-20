using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectByLayer : ScriptableWizard
{
    public string layerName;
    public List<GameObject> singleLayerList;
    private static readonly int PAGENUMBER = 20;
    private bool foldout = false;
    private bool custom = false;
    private int index = 0;

    [MenuItem("Tools/场景/层级选择")]
    static void SelectByLayerWizard()
    {
        ScriptableWizard.DisplayWizard<SelectByLayer>("层级搜索", "取消", "搜索");
    }

    private void SearchByLayer()
    {
        if (layerName == "")
        {
            EditorGUILayout.HelpBox("未输入layer名",MessageType.Warning);
            return;
        }
        singleLayerList = new List<GameObject>();
        Scene activeScene = SceneManager.GetActiveScene();
        var roots = activeScene.GetRootGameObjects();
        foreach (var root in roots)
        {
            SearchChild(root);
        }
    }

    private void SearchChild(GameObject go)
    {
        if (go.layer.Equals(LayerMask.NameToLayer(layerName)))
        {
            singleLayerList.Add(go);
        }
        if (go.transform.childCount != 0)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                GameObject child = go.transform.GetChild(i).gameObject;
                SearchChild(child);
            }
        }
    }

    protected override bool DrawWizardGUI()
    {
        // return base.DrawWizardGUI();
        custom = GUILayout.Toggle(custom, "自定义");
        if (custom)
        {
            layerName = EditorGUILayout.TextField("LayerName", layerName);
            if (GUILayout.Button("获取"))
            {
                SearchByLayer();
            }
        }
        else
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                for (int i = 0; i < 3; i++)
                {
                    if (GUILayout.Button($"CULL_LOD{i}"))
                    {
                        layerName = $"CULL_LOD{i}";
                        SearchByLayer();
                    }
                }
            }
        }
        

        if (GUILayout.Button("选取列表项"))
        {
            if (singleLayerList != null) Selection.objects = singleLayerList.ToArray();
        }
        if (singleLayerList != null)
        {
            DrawObjectList();
        }
        return true;
    }
    
    private void DrawObjectList()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            int count = singleLayerList != null ? singleLayerList.Count : 0;
            foldout = EditorGUILayout.Foldout(foldout, "Object");
            EditorGUILayout.LabelField($"{index * PAGENUMBER}-{index * PAGENUMBER + PAGENUMBER}/{count}");
            if (GUILayout.Button("<-"))
            {
                index--;
                if (index < 0) index = 0;
            }

            if (GUILayout.Button("->"))
            {
                index++;
                if (index > count / PAGENUMBER) index = count / PAGENUMBER;
            }
        }

        if (foldout && singleLayerList != null)
        {
            for (int i = index * PAGENUMBER; i < Mathf.Min(index * PAGENUMBER + PAGENUMBER, singleLayerList.Count); i++)
            {
                singleLayerList[i] = EditorGUILayout.ObjectField($"Target{i}", singleLayerList[i], typeof(GameObject), true) as GameObject;
            }
        }
    }
}

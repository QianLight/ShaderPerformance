using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Object = UnityEngine.Object;

public class ResourceRefrenceEditorWindow : EditorWindow
{
    [MenuItem("Tools/引擎/资源引用")]
    static void SearchRefrence()
    {
        ResourceRefrenceEditorWindow window = (ResourceRefrenceEditorWindow)EditorWindow.GetWindow(typeof(ResourceRefrenceEditorWindow), false, "资源索引预制体", true);
        window.maxSize = new Vector2(800, 1000);
        window.minSize = new Vector2(700, 600);
        window.Show();
    }

    enum RefMode
    {
        常规索引,
        模糊快速索引
    }
    private static Object searchObject;
    private List<Object> result = new List<Object>();
    private bool isTexMode;//,isMatMode
    private string matAssetPath = "Assets";
    private string prtAssetPath = "Assets";
    private RefMode _refMode = RefMode.常规索引;

    //private static string prefabTxt;
    private readonly  List<string> prefabTxt = new List<string>();
    private readonly  List<string> prefabFilePath = new List<string>();
    private readonly  List<Object> matObjs = new List<Object>();

    private bool refBool;

    private float scrollValue2;
    
    Vector2 scrollPos,scrollPos2;
    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal("box");
        {
            _refMode = (RefMode)EditorGUILayout.EnumPopup(_refMode);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal("box");
        {
            if (GUILayout.Button("预先收集预制体",GUILayout.Width(100)))
            {
                prefabTxt.Clear();
                prefabFilePath.Clear();
                result.Clear();
                
                string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { prtAssetPath });
                int prefabLength = prefabGuids.Length;
                for (int i = 0; i < prefabLength; i++)
                {
                    prefabFilePath.Add(AssetDatabase.GUIDToAssetPath(prefabGuids[i]));
                    EditorUtility.DisplayCancelableProgressBar("收集预制体备用", prefabFilePath[i], (float)i / prefabLength * 1.0f);
                    prefabTxt.Add(File.ReadAllText(prefabFilePath[i]));
                }
                EditorUtility.ClearProgressBar();
            }
            
            GUILayout.Space(20);
            
            if (_refMode == RefMode.常规索引)
            {
                EditorGUILayout.LabelField("预制体数量：",GUILayout.Width(50));
                EditorGUILayout.LabelField(prefabTxt.Count.ToString(),GUILayout.Width(50));
            }
            else
            {
                EditorGUILayout.LabelField("预制体数量：",GUILayout.Width(50));
                EditorGUILayout.LabelField(prefabTxt.Count.ToString(),GUILayout.Width(50));
                if (GUILayout.Button("预先收集材质球", GUILayout.Width(100)))
                {
                    matObjs.Clear();
                    result.Clear();
                    if(searchObject==null) return;

                    RefMaterial();
                }
                EditorGUILayout.LabelField("关联的材质球数量：",GUILayout.Width(100));
                EditorGUILayout.LabelField(matObjs.Count.ToString(),GUILayout.Width(50));
            }
        }
        EditorGUILayout.EndHorizontal();

        if (_refMode == RefMode.常规索引)
        {
            PrefabPathUI();

            EditorGUILayout.BeginHorizontal("box");
            {
                if (GUILayout.Button("搜索引用", GUILayout.Width(200)))
                {
                    result.Clear();

                    if (searchObject == null)
                        return;
                    SearchRefUber();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(1000), GUILayout.Height(500));
            
            EditorGUILayout.BeginVertical("box",GUILayout.Width(600));
            {
                //Debug.Log(result.Count);
                for (int i = 0; i < result.Count; i++)
                {
                    //Debug.Log(result[i].name);
                    EditorGUILayout.ObjectField(result[i], typeof(Object), true, GUILayout.Width(300));
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndScrollView();
        }
        else if(_refMode == RefMode.模糊快速索引)
        {
            PrefabPathUI();
            
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("索引当前文件夹材质球(输入具体的路径 索引会更快):");
            matAssetPath = EditorGUILayout.TextField(matAssetPath);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal("box");
            {
                
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(400), GUILayout.Height(500));
                {
                    EditorGUILayout.BeginVertical("box",GUILayout.Width(380));
                    {
                        EditorGUILayout.LabelField("---材质球---");
                        SearchRefTex();
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndScrollView();
                scrollPos2 = EditorGUILayout.BeginScrollView(scrollPos2, GUILayout.Width(400), GUILayout.Height(500));
                {
                    EditorGUILayout.BeginVertical("box",GUILayout.Width(380));
                    {
                        EditorGUILayout.LabelField("---预制体---");
                        for (int i = 0; i < result.Count; i++)
                        {
                            //Debug.Log(result[i].name);
                            EditorGUILayout.ObjectField(result[i], typeof(Object), true, GUILayout.Width(260));
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndScrollView();
                
                
            }
            EditorGUILayout.EndHorizontal();
            
        }
    }
    void SearchRefTex()
    {
        //Debug.Log(result.Count);
        for (int i = 0; i < matObjs.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            //Debug.Log(result[i].name);
            EditorGUILayout.ObjectField(matObjs[i], typeof(Object), true, GUILayout.Width(260));
            if (GUILayout.Button("引用", GUILayout.Width(100)))
            {
                result.Clear();
                            
                string assetPath = AssetDatabase.GetAssetPath(matObjs[i]);
                string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);   // 图片的GUID

                for (int j = 0; j < prefabTxt.Count; j++)
                {
                    if (prefabTxt[j].Contains(assetGuid))
                    {
                        Object fileObject = AssetDatabase.LoadAssetAtPath(prefabFilePath[j], typeof(Object));
                        result.Add(fileObject);
                    }
                    EditorUtility.DisplayCancelableProgressBar("材质关联预制体", "遍历prefabTxt", (float)j / prefabTxt.Count * 1.0f);
                }
                EditorUtility.ClearProgressBar();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
    void SearchRefUber()
    {
        string assetPath = AssetDatabase.GetAssetPath(searchObject);
        string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);   // 图片的GUID

        for (int i = 0; i < prefabTxt.Count; i++)
        {
            if(prefabTxt[i].Contains(assetGuid))
            {
                Object fileObject = AssetDatabase.LoadAssetAtPath(prefabFilePath[i], typeof(Object));
                
                result.Add(fileObject);
            }
            EditorUtility.DisplayCancelableProgressBar("关联预制体", "遍历prefabTxt", (float)i / prefabTxt.Count * 1.0f);
        }
        EditorUtility.ClearProgressBar();
    }

    void RefMaterial()
    {
        string assetPath = AssetDatabase.GetAssetPath(searchObject);
        string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);   // 图片的GUID
        string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { matAssetPath });
        int length = materialGuids.Length;
        for (int i = 0; i < length; i++)
        {
            string matFilePath = AssetDatabase.GUIDToAssetPath(materialGuids[i]);
            EditorUtility.DisplayCancelableProgressBar("收集关联材质球", matFilePath, (float)i / length * 1.0f);
            string content = File.ReadAllText(matFilePath);
            if(content.Contains(assetGuid))
            {
                Object fileObject = AssetDatabase.LoadAssetAtPath(matFilePath, typeof(Object));
                matObjs.Add(fileObject);
            }
        }
        EditorUtility.ClearProgressBar();
    }

    void PrefabPathUI()
    {
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField("需要索引的资源:");
        searchObject = EditorGUILayout.ObjectField(searchObject, typeof(Object), true, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();
            
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField("索引当前文件夹下预制体(输入具体的路径 索引会更快):");
        prtAssetPath = EditorGUILayout.TextField(prtAssetPath);
        EditorGUILayout.EndHorizontal();
    }
}


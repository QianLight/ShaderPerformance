#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public class PTestWindow : EditorWindow
{
    private static int layer = 4000;
    private static string NetUrl = "http://10.5.32.89:9000/";
    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
        GUILayout.Label("Canves Layer", EditorStyles.boldLabel);
        layer = EditorGUILayout.IntField("layer", layer);
        GUILayout.Label("Net Url", EditorStyles.boldLabel);
        NetUrl = EditorGUILayout.TextField("NetUrl", NetUrl);

        if (GUILayout.Button("Save"))  //在窗口上创建一个按钮
        {
            Save();
        }

        if (GUILayout.Button("Export Case List"))  //在窗口上创建一个按钮
        {
            ExportCaseList();
        }
    }

    [MenuItem("Devops/ExportCaseList")]  //添加菜单选项
    public static void ExportCaseList()
    {
        string date = System.DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss");
        var vFileName = Application.dataPath + "/" + date + ".csv";
        FileStream vFileStream = new FileStream(vFileName, FileMode.OpenOrCreate, FileAccess.Write);
        using (StreamWriter vStreamWriter = new StreamWriter(vFileStream, Encoding.UTF8))
        {
            List<CaseItemData> caseNames = GetCaseList();
            StringBuilder vStringBuilder = new StringBuilder();
            vStringBuilder.Append("Plan,");
            vStringBuilder.Append("Class,");
            vStringBuilder.Append("TestCase,");
            vStringBuilder.Append("Timer,");
            vStringBuilder.Append("CaseParmas0,");
            vStringBuilder.Append("CaseParmas1,");
            vStringBuilder.Append("CaseParmas2,");
            vStringBuilder.Append("CaseParmas3,");
            vStringBuilder.Append("CaseParmas4,");
            vStringBuilder.AppendLine();
            for (int i = 0; i < caseNames.Count; i++) {
                CaseItemData caseItem = caseNames[i];
                vStringBuilder.Append($",");
                vStringBuilder.Append($"{caseItem.type.Name},");
                vStringBuilder.Append($"{caseItem.type.Name},");
                vStringBuilder.Append($"600");
                vStringBuilder.Append($",");
                vStringBuilder.Append($",");
                vStringBuilder.Append($",");
                vStringBuilder.Append($",");
                vStringBuilder.Append($",");

                vStringBuilder.AppendLine();
            }


            vStreamWriter.Write(vStringBuilder);
            vStreamWriter.Flush();
            vStreamWriter.Close();
        }

        // 打开文件夹
        UnityEditor.EditorUtility.RevealInFinder(vFileName);
    }

    public static List<CaseItemData> GetCaseList()
    {
        List<CaseItemData> caseNames = new List<CaseItemData>();
        List<Type> types = GetSubClassType(PTestManager.typen("BP_Base"));
        foreach (Type type in types)
        {
            caseNames.Add(new CaseItemData { type = type });
        }

        return caseNames;
    }

    public string GetPath(string _scriptName)
    {
        string[] path = UnityEditor.AssetDatabase.FindAssets(_scriptName);
        if (path.Length > 1)
        {
            Debug.LogError("有同名文件" + _scriptName + "获取路径失败");
            return null;
        }
        //将字符串中得脚本名字和后缀统统去除掉
        string _path = AssetDatabase.GUIDToAssetPath(path[0]).Replace((@"/" + _scriptName + ".cs"), "");
        return _path;
    }

    public static List<Type>  GetSubClassType(Type parentType)
    {
         List<Type> types = new List<Type>();

        var CSharp = Assembly.Load("Assembly-CSharp");
        var assemblyAllTypes = CSharp.GetTypes();//获取该程序集中的所有类型
        foreach (var itemType in assemblyAllTypes)//遍历所有类型进行查找
        {
            var baseType = itemType.BaseType;//获取元素类型的基类
            if (baseType != null)//如果有基类
            {
                if (baseType.Name == parentType.Name)//如果基类就是给定的父类
                {
                    types.Add(itemType);
                }
            }
        }
        return types;
    }


    //public static List<CaseItemData> TypeMethods(Type type)
    //{
    //    var subTypeList = new List<CaseItemData>();
    //    MethodInfo[] methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);//
    //    foreach (MethodInfo method in methods)
    //    {
    //        if (method.ReturnType.Name.Contains("IEnumerator"))
    //            subTypeList.Add(new CaseItemData { type = type, method = method});
    //    }
    //    return subTypeList;
    //}

    private void Save()
    {
        PlayerPrefs.SetInt(PTestDataConfig.PTestLayer, layer);
        PlayerPrefs.SetString(PTestDataConfig.PTestNetUrl, NetUrl);
    }



    //public static void ShowWindow()
    //{

    //    layer = PlayerPrefs.GetInt(PTestDataConfig.PTestLayer, PTestConfig.TestLayer);
    //    NetUrl = PlayerPrefs.GetString(PTestDataConfig.PTestNetUrl, PTestNetConfig.url);

    //    EditorWindow window = EditorWindow.GetWindow(typeof(PTestWindow));
    //    window.minSize = new Vector2(800, 400);
    //    window.Show();
    //}

}
#endif
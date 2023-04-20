using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class PropertyReplacer : ScriptableWizard
{
    public MaterialDeserizer.ClearFlag flag;
    
    public string Oldname;

    public string Newname;

    private MaterialDeserizer materialDeserizer;
    private List<Material> _matList;
    
    [MenuItem("Tools/引擎/材质属性名一键替换")]
    static void ProjectReferenceSearcher()
    {
        ScriptableWizard.DisplayWizard<ProjectReferenceSearcherWizard>("材质属性名一键替换", "应用", "搜索");
    }
    private void OnWizardOtherButton()
    {
        _matList = new List<Material>();
        EditorSettings.serializationMode = SerializationMode.ForceText;
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);

        List<string> withoutExtensions = new List<string>() { ".prefab", ".unity", ".mat", ".asset" };
        string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
            . Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
        int startIndex = 0;
        if (files.Length > 0)
        {
            EditorApplication.update = delegate ()
            {
                string file = files[startIndex];
        
                bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file, (float)startIndex / (float)files.Length);
        
                if (System.Text.RegularExpressions.Regex.IsMatch(File.ReadAllText(file), Oldname))// 前缀(\w+)
                {
                    _matList.Add(AssetDatabase.LoadAssetAtPath<Material>(GetRelativeAssetsPath(file)));
                    // Debug.Log(file, AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(file)));
                }
        
                startIndex++;
                if (isCancel || startIndex >= files.Length)
                {
                    EditorUtility.ClearProgressBar();
                    EditorApplication.update = null;
                    startIndex = 0;
                    
                    helpString = "**********\n"+ "含有Property名为"+Oldname + "的材质球共"+_matList.Count+"\n**********\n匹配结束";
                }
            };
        }
        else
        {
            helpString = "**********\n"+ "含有Property名为"+Oldname + "的材质不存在\n**********\n";
        }
            
        
    }

    private void OnWizardCreate()
    {
        if (_matList.Count < 0 || _matList is null)
        {
            Debug.LogError("材质球列表为空，无法替换");
            return;
        }
        int startIndex = 0;
        EditorApplication.update = delegate ()
        {
            materialDeserizer = new MaterialDeserizer(_matList[startIndex]);
        
            bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", _matList[startIndex].name, (float)startIndex / (float)_matList.Count);
        
            materialDeserizer.ReplaceName(Newname, Oldname);
        
            startIndex++;
            if (isCancel || startIndex >= _matList.Count)
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update = null;
                startIndex = 0;
                    
                Debug.Log("替换完成");
            }
        };
        
    }

    static private string GetRelativeAssetsPath(string path)
    {
        return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
    }
}

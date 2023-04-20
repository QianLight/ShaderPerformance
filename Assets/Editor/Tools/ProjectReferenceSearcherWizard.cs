using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using System.IO;
using System.Linq;
using Object = UnityEngine.Object;

public class ProjectReferenceSearcherWizard : ScriptableWizard
{
    public Object Target;
    public List<Object> List;
    [MenuItem("Tools/引擎/关联使用搜索")]
    static void ProjectReferenceSearcher()
    {
        ScriptableWizard.DisplayWizard<ProjectReferenceSearcherWizard>("关联使用搜索器", "取消", "搜索");
        
    }

    private void OnWizardOtherButton()
    {
        List = new List<Object>();
        EditorSettings.serializationMode = SerializationMode.ForceText;
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        string targetpath = AssetDatabase.GetAssetPath(Target);
        if (!string.IsNullOrEmpty(targetpath))
        {
            string guid = AssetDatabase.AssetPathToGUID(targetpath);
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
            
                    if (System.Text.RegularExpressions.Regex.IsMatch(File.ReadAllText(file), guid))// 前缀(\w+)
                    {
                        List.Add(AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(file)));
                        // Debug.Log(file, AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(file)));
                    }
            
                    startIndex++;
                    if (isCancel || startIndex >= files.Length)
                    {
                        EditorUtility.ClearProgressBar();
                        EditorApplication.update = null;
                        startIndex = 0;
                        
                        helpString = "**********\n"+ "对象: "+Target.name + "\n**********\n匹配结束";
                    }
                };
            }
            else
            {
                helpString = "**********\n"+ "对象: "+Target.name + "\n**********\n该目录下无可供搜索的对象类型";
            }
            
        }
    }
    static private string GetRelativeAssetsPath(string path)
    {
        return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
    }
}

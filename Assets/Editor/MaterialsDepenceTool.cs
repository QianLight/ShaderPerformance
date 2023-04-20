using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class MaterialsDepenceTool
{
    [MenuItem("Tools/引擎/查找材质引用shader")]
    static void MaterialsDepenceToolFind()
    {
        var matGuids = AssetDatabase.FindAssets("t:material");
        var shaderGuids = AssetDatabase.FindAssets("t:shader");
        var useShaders = new HashSet<string>();
        var AllShaders = new List<string>(1000);
        foreach (var guid in matGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            var shader = AssetDatabase.GetAssetPath(mat.shader);
            useShaders.Add(shader);
        }
        foreach (var guid in shaderGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            AllShaders.Add(path);
        }
        


        FileStream fs = new FileStream("notUseShader.txt", FileMode.Create);
        StringBuilder sb = new StringBuilder();

        var num = 0;

        foreach (var shader in AllShaders)
        {
            if (!useShaders.Contains(shader))
            {
                num++;
                sb.Append($"{shader}\n");
                byte[] bytes = new UTF8Encoding().GetBytes(sb.ToString());
                fs.Write(bytes, 0, bytes.Length);
                sb.Clear();
            }
        }
        fs.Close();
        Debug.Log("OK "+ num + " 未被使用");
    }
}
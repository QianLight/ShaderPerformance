using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class TexSizeEditor : Editor
{
    private static List<string> filesPaths;
    private static string SavePath = Path.Combine(Application.persistentDataPath, "TexSizeSaveFile");
    private static string saveTex;
    [MenuItem("Assets/Tool/TexSize")]
    private static void TexSize()
    {
        filesPaths = new List<string>();
        saveTex = "";
        string guid = Selection.assetGUIDs[0];
        var selectPath = AssetDatabase.GUIDToAssetPath(guid);
        
        string[] texGuids = AssetDatabase.FindAssets("t:Texture", new[] { selectPath });
        int prefabLength = texGuids.Length;
        for (int i = 0; i < prefabLength; i++)
        {
            filesPaths.Add(AssetDatabase.GUIDToAssetPath(texGuids[i]));
            EditorUtility.DisplayCancelableProgressBar("收集Texture", filesPaths[i], (float)i / prefabLength * 1.0f);
        }
        EditorUtility.ClearProgressBar();
        
        // DirectoryInfo direction = new DirectoryInfo(selectPath);
        // FileInfo[] files = direction.GetFiles("*");
        // foreach (var f in files)
        // {
        //     if (f.Name.EndsWith(".meta"))
        //         continue;
        //     if (f.Name.EndsWith(".png")||f.Name.EndsWith(".PNG")||f.Name.EndsWith(".tga"))
        //     {
        //         string texPath = DirectoryInfoPath(f.FullName);
        //         filesPaths.Add(texPath);
        //     }
        // }

        foreach (var texPath in filesPaths)
        {
            //Object tex = AssetDatabase.LoadAssetAtPath<Object>(texPath);
            //Texture2D tex = tex as Texture2D;
            //var cIma = System.Drawing.Image.FromFile(texPath);
            TextureImporter textureImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (textureImporter != null)
            {
                TextureImporterPlatformSettings _platformSta = textureImporter.GetPlatformTextureSettings("Android");
                float texMaxSize = _platformSta.maxTextureSize;
                int width = 0;
                int height = 0;
                GetTextureOriginalSize(textureImporter, out width, out height);
                //Debug.LogError(tex.name +":"+width + "：" + height);
                int maxSize = 2048;
                // 同时满足 原始尺寸大于maxSize和压缩大小大于maxSize
                if ((width > maxSize || height > maxSize) && texMaxSize > maxSize)
                {
                    //saveTex+= tex.name +"  :  "+width + "*" + height + "\n";
                    saveTex += $"{textureImporter.name} : {width} * {height}  maxSize:{texMaxSize} \n";
                    Debug.LogError(textureImporter.name +"  :  "+width + "*" + height + "   maxSize:"+texMaxSize);
                }
            }
        }
        Save();
        filesPaths.Clear();
    }
    public static string DirectoryInfoPath(string str)
    {
        int c = str.LastIndexOf(@"Assets\");
        str = str.Substring(c, str.Length - str.LastIndexOf(@"Assets\"));
        return str;
    }
    public static void GetTextureOriginalSize(TextureImporter ti, out int width, out int height)
    {
        if (ti == null)
        {
            width = 0;
            height = 0;
            return;
        }

        object[] args = new object[2] { 0, 0 };
        MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
        mi.Invoke(ti, args);

        width = (int)args[0];
        height = (int)args[1];
    }
    
    static void Save()//存储数据
    {
        using (FileStream file = new FileStream(SavePath, FileMode.Create))
        {
            byte[] bts = System.Text.Encoding.UTF8.GetBytes(saveTex);
            file.Write(bts, 0, bts.Length);
            Debug.Log(SavePath);
        }
    }
}

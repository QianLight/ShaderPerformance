using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

public class CreateLuaScript 
{

    [MenuItem("Assets/Create/Lua Script/UI", false, 80)]
    public static void MakeHotfixLua()
    {
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
       ScriptableObject.CreateInstance<MyDoCreateScriptAsset>(),
       GetSelectedPathOrFallback() + "/XNewUI.lua.txt",
       null,
      "Shell/lua_temp/Temp_Lua.lua.txt");
    }


    // [MenuItem("Assets/Create/Lua Script/LuaView", false, 80)]
    // public static void MakeLuaViewLua()
    // {
    //     ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
    //    ScriptableObject.CreateInstance<MyDoCreateScriptAsset>(),
    //    GetSelectedPathOrFallback() + "/LuaView.lua.txt",
    //    null,
    //   "Shell/lua_temp/Temp-LuaView.lua.txt");
    // }
    
    [MenuItem("Assets/Create/Lua Script/Document", false, 80)]
    public static void MakeLuaDoc()
    {
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
            ScriptableObject.CreateInstance<MyDoCreateScriptAsset>(),
            GetSelectedPathOrFallback() + "/LuaDocument.lua.txt",
            null,
            "Shell/lua_temp/Temp-Document.lua.txt");
    }
    

    public static string GetSelectedPathOrFallback()
    {
        string path = "Assets";
        foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
                break;
            }
        }
        return path;
    }

}


class MyDoCreateScriptAsset : EndNameEditAction
{

    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        UnityEngine.Object o = CreateScriptAssetFromTemplate(pathName, resourceFile);
        ProjectWindowUtil.ShowCreatedAsset(o);
    }

    internal static UnityEngine.Object CreateScriptAssetFromTemplate(string pathName, string resourceFile)
    {
        string fullPath = Path.GetFullPath(pathName);
        StreamReader streamReader = new StreamReader(resourceFile);
        string text = streamReader.ReadToEnd();
        streamReader.Close();
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathName);
        string fileNameWithoutHead = fileNameWithoutExtension;
        Debug.Log("file name: " + fileNameWithoutHead);
        if (fileNameWithoutHead.EndsWith(".lua"))
        {
            fileNameWithoutHead = fileNameWithoutHead.Substring(0, fileNameWithoutHead.Length - 4);
        }

        string date = System.DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss");
        string user = System.Environment.UserName;
        text = text.Replace("#AuthorName#", user).Replace("#CreateTime#", date
           ).Replace("#SCRIPTNAME#", fileNameWithoutExtension).Replace("#SCRIPTTITLE#", fileNameWithoutHead);
        bool encoderShouldEmitUTF8Identifier = false;
        bool throwOnInvalidBytes = false;
        UTF8Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes);
        bool append = false;
        StreamWriter streamWriter = new StreamWriter(fullPath, append, encoding);
        streamWriter.Write(text);
        streamWriter.Close();
        AssetDatabase.ImportAsset(pathName);
        return AssetDatabase.LoadAssetAtPath(pathName, typeof(UnityEngine.Object));
    }

}
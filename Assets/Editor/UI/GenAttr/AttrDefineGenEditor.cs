using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;


public class AttrDefineGenEditor : Editor
{
    private readonly static string script_temp = "/Table/AttrDefine.txt";
    private readonly static string script_target = "src/client/CFClient/CFClient/XComponents/Attributes/XAttributeDefine.cs";
    private readonly static string script_editor = "res/OPProject/Assets";


    [MenuItem("Tools/Generator/AttrDefine")]
    public static void GeneratorGlobalConfigKey()
    {
        SaveToScript();
    }

    private static bool SaveToScript()
    {

        string scriptPath = Application.dataPath + script_temp;

        string template = File.ReadAllText(scriptPath, Encoding.Default);
        if (string.IsNullOrEmpty(template))
        {
            Debug.Log("没有找到属性配置文件!");
            return false;
        }
        StringBuilder sb = new StringBuilder();

        string[] lines = template.Split(new char[] { '\n' });
        char[] split = new char[] { '\t' };

        sb.AppendLine("// Auto gen from AttrDefine table. "/* + DateTime.Now.ToShortDateString()*/);
        sb.AppendLine("// ToolPath: Tools/Generator/AttrDefine ");
        sb.AppendLine("");
        sb.AppendLine("namespace CFClient");
        sb.AppendLine("{");
        sb.AppendLine("    public enum XAttributeDefine");
        sb.AppendLine("\t{");
        sb.AppendLine("\t\tXAttr_Invalid = -1,");

        for (int i = 3; i < lines.Length; ++i)
        {
            //Debug.Log(lines[i]);
            string[] fields = lines[i].Split(split);

            if (fields.Length >= 3)
            {
                sb.Append("\t\t");
                sb.AppendFormat("{0} = {1}, // {2}", fields[2], fields[1], fields[3]);
                sb.AppendLine("");
                //Debug.Log(sb.ToString());
            }
        }
        sb.AppendLine("\t}");
        sb.AppendLine("}");


        string targetPath = Application.dataPath.Replace(script_editor, script_target);

        if(File.Exists(targetPath))
            File.WriteAllText(targetPath , sb.ToString());

        Debug.Log("AttrDefine!");
        return true;
    }
}

using CFEngine.Editor;
using CFUtilPoolLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using CustomPropertyDrawer = UnityEditor.CustomPropertyDrawer;

public abstract class XScriptParse
{
    public uint type;
    public string file = string.Empty;
    public string script;
    public void Parse()
    {
        string file = Application.dataPath + script;
        if (!File.Exists(file))
        {
            Debug.LogError("没有找到脚本文件:" + file);
            return;
        }
        IEnumerable<string> table = File.ReadAllLines(file, Encoding.Default);
        Parse(table.GetEnumerator());
    }


    public abstract void Save();

    protected abstract void Parse(IEnumerator<string> table);

    protected void ReadUint(string[] strs, int index, ref uint value)
    {
        if (InRange(strs, index))
        {
            value = uint.Parse(strs[index]);
        }
    }

    protected void ReadFloat(string[] strs, int index , ref float value)
    {
        if (InRange(strs, index)) value = float.Parse(strs[index]);
    }

    protected void ReadInt(string[] strs, int index, ref int value)
    {
        if (InRange(strs, index))
        {
            value = int.Parse(strs[index]);
        }

    }
    
    protected void ReadString(string[] strs, int index, ref string value)
    {
        if (InRange(strs, index)) value = strs[index];
    }


    protected void ReadParams(string[] str, List<string> keys, ref List<Tri> values)
    {
        if(!InRange(str,1)) return;
        uint key;
        if (XGuideUtility.Str2GuidID(keys, str[1], out key)) {
            if (values == null) values = new List<Tri>();
            Tri tri = new Tri();
            tri.id = (int)key;
            tri.param = str.Length > 2 ? str[2] : "";
            values.Add(tri);
        }
        else
        {
            XDebug.singleton.AddErrorLog("not param str :" + str[0]);
        }
    }

    private bool InRange(string[] strs, int index)
    {
        return strs != null && index >= 0 && index < strs.Length;
    }

}

public class XGuideUtility
{
    public static readonly char[] TabSeparator = new char[] { ' ', '\t' };
    public const string script_path = "/Table/Guide/{0}.txt";
    public const string binary_path = "Assets/BundleRes/Table/Guide/{0}.bytes";
    public const string script_fix = "GuildScript";
    public const string script_root = "GuideTable";

    public const string assets_path = "Assets/Table/Guide/{0}.asset";

    public const uint Root = 1;
    public const uint Script = 2;

    public static List<string> KEY_Begin;
    public static List<string> KEY_End;
    public static List<string> KEY_Cmd;
    public static List<string> KEY_Method;
    public static List<string> KEY_Skin;

    public static bool Str2GuidID(List<string> keys,string str , out uint value)
    {
        value = 0;
        if (keys == null) return false;
        int intValue = keys.IndexOf(str);
        if (intValue > 0)
        {
            value = (uint)intValue;
            return true;
        }
        else {
            return false ;
        }
    }

    public const string Key_Tutorial = "tutorial"; //引导开头
    public const string Key_Pass = "pass"; //跳过标记 
    public const string Key_StartCond = "scond"; //触发条件
    public const string Key_Skip = "skip";
    public const string Key_Script = "script";



}
public class XGuideEditor : Editor, ITableCb
{



    // public static void Init()
    // {
    //     AssetsImporter.tableCb = Process;
    // }

    [MenuItem("Tools/Generator/GuideToBanary")]
    public static void GeneratorGuide()
    {
        Process("GuideTable");
    }

    public static void Process(string tableNames)
    {
        string[] names = tableNames.Split(' ');
        Queue<XScriptParse> scripts = new Queue<XScriptParse>();
        XScriptParse ng = null; ;
        for (int i = 0; i < names.Length; ++i)
        {
            if (TryGetScriptName(names[i], ref ng))
            {
                scripts.Enqueue(ng);
                XDebug.singleton.AddBlueLog("Parse:"+ng.file);
            }
        }
        while (scripts.Count > 0)
        {
            ng = scripts.Dequeue();
            ng.Parse();
            ng.Save();
        }
        scripts = null;
    }
    private static bool TryGetScriptName(string filename, ref XScriptParse ng)
    {
        if (filename.StartsWith(XGuideUtility.script_root))
        {
            ng = new XGuideListParse();
            ng.script = string.Format(XGuideUtility.script_path, filename);
            ng.type = XGuideUtility.Root;
            ng.file = filename;
            return true;
        }
        else if (filename.StartsWith(XGuideUtility.script_fix))
        {
            ng = new XGuideScriptParse();
            ng.script = string.Format(XGuideUtility.script_path, filename);
            ng.type = XGuideUtility.Script;
            ng.file = filename;
            return true;
        }
        return false;
    }
}
public class XGuideScriptParse : XScriptParse
{
    private XGuideScript scripts = new XGuideScript();
    private XGuideProcess process = null;
    public override void Save()
    {
        DataIO.SerializeData<XGuideScript>(string.Format(XGuideUtility.binary_path, file), scripts);
    }

    protected override void Parse(IEnumerator<string> table)
    {
        scripts.table.Clear();
        uint lineno = 0;
        try
        {
            string line = string.Empty;
            int step = 0;
            while (table.MoveNext())
            {
                line = table.Current;
                lineno++;
                if (line.StartsWith("end tutorial"))
                {
                    if (process == null)
                    {
                        XDebug.singleton.AddErrorLog("has not step  in tutorial : {0} ", file);
                    }
                    else
                    {
                       // process.lastCmdInQueue = true;
                    }
                    break;
                }

                if ( string.IsNullOrEmpty(line) || line.Length == 0 || line.StartsWith("--")) continue;
                string[] str = line.Split(XGuideUtility.TabSeparator, StringSplitOptions.RemoveEmptyEntries);
                switch (str[0])
                {
                    case "step":
                        step++;
                        process = new XGuideProcess();
                        scripts.table.Add(process);
                        //process.id = step;
                        break;
                    //case "scond":
                    //    ReadParams(str, XGuideUtility.KEY_Begin, ref process.triBegin);
                    //    break;
                    //case "skip":
                    //    ReadParams(str, XGuideUtility.KEY_Begin, ref process.triSkips);
                    //    break;
                    //case "econd":
                    //    ReadParams(str, XGuideUtility.KEY_End, ref process.triEnd);
                    //    break;
                    //case "fcond":
                    //    ReadParams(str, XGuideUtility.KEY_End, ref process.triEnd);
                    //    break;
                    //case "method":
                    //    ReadParams(str, XGuideUtility.KEY_Method, ref process.triMethod)
                    //    break;
                    //case "audio":
                    //    ReadString(str, 1, ref process.sound);
                    //    break;
                    case "canpass":
                        break;
                    case "dofinish":
                        break;
                    //case "succeed":
                    //    ReadUint(str, 1, ref process.success);
                    //    break;
                    //case "defeated":
                    //    ReadUint(str, 1, ref process.fail);
                    //    break;
                    //case "internaldelay":
                    //    break;
                    case "block":
                        //process.block = true;
                        break;
                    case "demonstration":
                        break;
                    //case "motion":
                    //    ReadFloat(str, 1, ref process.motion);
                    //    break;
                    //case "skin":
                    //    ReadParams(str,XGuideUtility.KEY_Skin,ref process.triSkins);
                    //    break;
                    default:
                        //ReadParams(str, XGuideUtility.KEY_Cmd, ref process.triHandle);
                        break;
                }
            }
        }
        catch (Exception)
        {
            XDebug.singleton.AddErrorLog(string.Format("Tutorial Parse Error! File = {0} ,lineno = {1}", file, lineno));
        }
    }
}
public class XGuideListParse : XScriptParse
{
    private XGuideList list = new XGuideList();

    private XGuideData current = null;

    public override void Save()
    {
        DataIO.SerializeData<XGuideList>(string.Format(XGuideUtility.binary_path, file), list);
    }

    protected override void Parse(IEnumerator<string> table)
    {
        list.table.Clear();
        uint lineno = 0;
        try
        {
            while (table.MoveNext())
            {
                string line = table.Current;
                lineno++;
                if (line.StartsWith("KEY_Begin")) ReadKeyValues(line, ref XGuideUtility.KEY_Begin);
                if (line.StartsWith("KEY_End")) ReadKeyValues(line, ref XGuideUtility.KEY_End);
                if (line.StartsWith("KEY_Cmd")) ReadKeyValues(line, ref XGuideUtility.KEY_Cmd);
                if (line.StartsWith("end tutorial")) break;

                if (string.IsNullOrEmpty(line) || line.Length == 0 || line.StartsWith("--")) continue;

                string[] strs = line.Split(XGuideUtility.TabSeparator, StringSplitOptions.RemoveEmptyEntries);
                switch (strs[0])
                {
                    case XGuideUtility.Key_Tutorial:
                        current = new XGuideData();
                    //    ReadInt(strs, 1, ref current.id);
                    //    break;
                    //case XGuideUtility.Key_Pass:
                    //    ReadUint(strs, 1, ref current.pass);
                    //    break;
                    //case XGuideUtility.Key_StartCond:
                    //    ReadParams(strs, XGuideUtility.KEY_Begin, ref current.triBegin);
                    //    break;
                    //case XGuideUtility.Key_Skip:
                    //    ReadParams(strs, XGuideUtility.KEY_Begin, ref current.triSkin);
                        break;
                    case XGuideUtility.Key_Script:
                        ReadString(strs, 1, ref current.script);
                        list.table.Add(current);
                        break;
                }
            }
        }
        catch (Exception)
        {
            XDebug.singleton.AddErrorLog(string.Format("Guide Parse Error! File = {0} ,lineno = {1}", file, lineno));
        }     
    }

    private void ReadKeyValues( string line , ref List<string> values )
    {
        string[] strs = line.Split(XGuideUtility.TabSeparator, StringSplitOptions.RemoveEmptyEntries);
        string[] value = strs[1].Split(',');
        values = new List<string>(value);
        XDebug.singleton.AddBlueLog("line:" + line);
    }
}


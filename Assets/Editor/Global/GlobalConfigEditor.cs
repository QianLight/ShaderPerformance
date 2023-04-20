using System.Reflection.Emit;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using CFUtilPoolLib;
using CFEngine.Editor;
/// <summary>
/// 自动生成GlobalConfig中的KEY值
/// </summary>
public class GlobalConfigEditor : Editor, ITableCb
{


    private readonly static string global_path = "/Table/{0}.txt";
    private readonly static string script_line = "\t\tpublic readonly static string {0} = \"{1}\";";
    private readonly static string script_temp = "/Editor/Global/GlobalNamesTemplate.txt";
    private readonly static string script_target = "src/client/CFClient/CFClient/";
    private readonly static string script_editor = "res/OPProject/Assets";

    public static void Init()
    {
        AssetsImporter.tableCb += Process;
        AssetsImporter.allTableCb += ProcessAll;
    }
    public static void Process(string tableNames)
    {
        int tableType = 0;
        string[] names = tableNames.Split(' ');
        for (int i = 0; i < names.Length; ++i)
        {
            tableType |= GetTableType(names[i]);
        }
        if ((tableType & 1) == 1) GeneratorGlobalConfigKey();
        else if ((tableType & 2) == 2) GeneratorSystemDefine();
        else if ((tableType & 4) == 4) GeneratorAttrDefine();

        //RunTableChecker.DoCheckIfPrimary(tableNames);
    }
    public static void ProcessAll()
    {
        GeneratorGlobalConfigKey();
        GeneratorSystemDefine();
        GeneratorAttrDefine();
    }
    public static int GetTableType(string filename)
    {
        if (filename.StartsWith("Global")) return 1;
        else if (filename.StartsWith("SystemDefine")) return 2;
        else if (filename.StartsWith("AttrDefine")) return 4;
        else return 0;
    }
    #region GlobalConfig
    private static uint _currentPart = 0;
    [MenuItem("Tools/Generator/GlobalToClass")]
    public static void GeneratorGlobalConfigKey()
    {
        _currentPart = 0;
        GeneratorGlobalConfig();
    }

    private static void GeneratorGlobalConfig()
    {
        string[] configs = { "GlobalCommon", "GlobalBattle", "GlobalLevel", "GlobalSystem" };
        string[] configFix = { "gc", "gb", "gl", "gs" };
        List<string> globals = new List<string>();
        Dictionary<string, string> names = new Dictionary<string, string>();
        for (int i = 0; i < configs.Length; i++)
        {
            if (!ReadLines(configs[i], ref names, configFix[i])) return;
        }
        if (SaveToScript("XGlobalNames", names))
        {
            Debug.Log("Generator Success!");
        }
    }

    private static void GeneratorAttrDefine()
    {
        AttrDefineGenEditor.GeneratorGlobalConfigKey();
    }

    private static bool SaveToScript(string csName, Dictionary<string, string> globals)
    {

        string scriptPath = Application.dataPath + script_temp;

        string template = File.ReadAllText(scriptPath, Encoding.Default);
        if (string.IsNullOrEmpty(template))
        {
            Debug.Log("没有找到模板文件!");
            return false;
        }
        StringBuilder builder = new StringBuilder();
        foreach (KeyValuePair<string, string> pair in globals)
        {
            builder.AppendLine(string.Format(script_line, pair.Key, pair.Value));
        }
        string buildStr = builder.ToString();
        template = template.Replace("{0}", csName);
        template = template.Replace("{1}", buildStr);
        string targetPath = Application.dataPath.Replace(script_editor, script_target);
        try
        {
            File.WriteAllText(targetPath + csName + ".cs", template);
        }
        catch (Exception)
        {
            Debug.Log("hasn't src!");
        }
        return true;
    }

    private static bool ReadLines(string fileName, ref Dictionary<string, string> builder, string fix)
    {
        string file = Application.dataPath + string.Format(global_path, fileName);
        if (!File.Exists(file))
        {
            Debug.LogError("没有找到配置文件！" + fileName);
            return false;
        }
        string[] lines = File.ReadAllLines(file, Encoding.Default);
        string[] lineValus = null;
        for (int i = 2; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i]))
            {
                Debug.LogError(string.Format("{0}文件第{1}行为空行", file, i + 1));
                return false;
            }
            lineValus = lines[i].Split('\t');
            if (!string.IsNullOrEmpty(lineValus[1]))
            {
                uint part = uint.Parse(lineValus[1]);
                if (part < 2 && part != _currentPart) continue;
            }
            string globalName = fix + "_" + lineValus[2];
            if (!builder.ContainsKey(globalName))
            {
                builder.Add(globalName, lineValus[2]);
            }
            else
            {
                Debug.LogError(string.Format("{0}文件有重复名{1}", fileName, lineValus[2]));
                return false;
            }
        }
        return true;
    }
    #endregion

    #region SystemDefine
    [MenuItem("Tools/Generator/SystemToClass")]
    public static void GeneratorSystemDefine()
    {
        GeneratorSystem();
    }

    struct Module
    {
        public uint hashCode;
        public string showName;
        public int systemID;
        public string className;
        public string prefab;
        public int luaFlag;
    }
    private static void GeneratorSystem()
    {
        string[] configs = { "SystemDefine" };
        Dictionary<uint, Module> names = new Dictionary<uint, Module>();

        for (int i = 0; i < configs.Length; i++)
        {
            if (!ReadLines(configs[i], 1, ref names)) return;
        }

        StringBuilder luaHost = new StringBuilder();
        StringBuilder luaSystemID = new StringBuilder();

        StringBuilder cshapeHost = new StringBuilder();
        StringBuilder classCreator = new StringBuilder();
        StringBuilder cshapeSystemID = new StringBuilder();
        Dictionary<uint, Module>.Enumerator enumerator = names.GetEnumerator();
    
        List<Module> modules = new List<Module>();

        while (enumerator.MoveNext())
        {
            modules.Add(enumerator.Current.Value);
        }
        modules.Sort(ModuleSorting);
        foreach (Module module in modules)
        {
            int systemID = module.systemID;
            luaHost.AppendLine(ConvertToHostLine(lua_id_host_line, module));
            if (systemID > 0 && systemID < 1024)
            {
                luaSystemID.AppendLine(ConvertToLuaSystemLine(lua_id_host_line_system, module));
            }
            if (module.luaFlag != 1)
            {
                cshapeHost.AppendLine(ConvertToHostLine(cshape_id_host_line, module));
                classCreator.AppendLine(ConvertToScriptLine(cshape_id_script_line, module));

                if (systemID > 0 && systemID < 1024) cshapeSystemID.AppendLine(ConvertToCshapeSystemLine(cshape_id_host_line_system, module));
            }
        }

        if (SaveToXUIHosts("XUIHosts", "/Editor/Global/SystemDefineTemplate.txt", cshapeHost, classCreator,cshapeSystemID))
        {
            Debug.Log("Generator C# Success!");
        }

        if (SaveToLua("LuaUI", "/Editor/Global/LuaUITemplate.txt", luaHost,luaSystemID))
        {
            Debug.Log("Generator Lua Success!");
        }
    }

    private static int ModuleSorting( Module f, Module l ){
        return f.systemID - l.systemID;
    }

    private static bool Filtration(ref string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        if (value.StartsWith("UE_")) return false;
        if (value.IndexOf("CF") == 0 || value.IndexOf("OP") == 0) value = value.Substring(2);
        else if (value.IndexOf("X") == 0) value = value.Substring(1);
        if (value.LastIndexOf("UI") > 0) value = value.Substring(0, value.Length - 2);
        if (value.LastIndexOf("View") > 0) value = value.Substring(0, value.Length - 4);
        return !string.IsNullOrEmpty(value);
    }
    private readonly static string lua_id_host_line_system = "\t\t{0} = {1},";
    private readonly static string lua_id_host_line = "\t\t{0} = {1},  --{2}";
    private readonly static string cshape_id_host_line_system = "\t\tpublic const uint {0} = {1};";
    private readonly static string cshape_id_host_line = "\t\tpublic const uint {0} = {1};    /* {2} */";
    private readonly static string cshape_id_script_line = "\t\t\t\tcase {0}: system = CFAllocator.Allocate<{1}>(); break;";
    private static string ConvertToHostLine(string temp, Module module)
    {
        return string.Format(temp, module.showName, module.hashCode,module.prefab);
    }

    private static string ConvertToScriptLine(string temp, Module module)
    {
        return string.Format(temp, module.showName, module.className);
    }

    private static string ConvertToCshapeSystemLine(string temp, Module module)
    {
        return string.Format(temp, module.showName, module.systemID);
    }

    private static string ConvertToLuaSystemLine(string temp, Module module)
    {
        return string.Format(temp, module.showName, module.systemID);
    }

    private static bool SaveToLua(string csName, string temp, StringBuilder builder,StringBuilder systemID)
    {
        string scriptPath = Application.dataPath + temp;

        string template = File.ReadAllText(scriptPath, Encoding.Default);
        if (string.IsNullOrEmpty(template))
        {
            Debug.Log("没有找到模板文件!");
            return false;
        }
        string buildStr = builder.ToString();
        template = template.Replace("{registerline}", buildStr);

        buildStr = systemID.ToString();
        template = template.Replace("{SystemIDLine}",buildStr);

        string targetPath = Application.dataPath + "/StreamingAssets/lua/module/core/";
        try
        {
            File.WriteAllText(targetPath + csName + ".lua.txt", template);
        }
        catch (Exception)
        {
            Debug.Log("...hasn't src!");
        }
        return true;
    }

    private static bool SaveToXUIHosts(string csName, string temp, StringBuilder builder, StringBuilder classCreator,StringBuilder systemID)
    {
        string scriptPath = Application.dataPath + temp;
        string template = File.ReadAllText(scriptPath, Encoding.Default);
        if (string.IsNullOrEmpty(template))
        {
            Debug.Log("没有找到模板文件!");
            return false;
        }
        string buildStr = builder.ToString();
        template = template.Replace("{classname}", csName);
        template = template.Replace("{fieldline}", buildStr);

        buildStr = classCreator.ToString();
        template = template.Replace("{registerline}", buildStr);

        buildStr = systemID.ToString();
        template = template.Replace("{systemIDField}",buildStr);

        string targetPath = Application.dataPath.Replace(script_editor, "src/client/CFClient/CFClient/UI/");
        try
        {
            File.WriteAllText(targetPath + csName + ".cs", template);
        }
        catch (Exception)
        {
            Debug.Log("...hasn't src!");
        }
        return true;
    }

    private static bool ReadLines(string fileName, int line, ref Dictionary<uint, Module> builder)
    {
        string file = Application.dataPath + string.Format(global_path, fileName);
        if (!File.Exists(file))
        {
            Debug.LogError("没有找到配置文件！" + fileName);
            return false;
        }
        string[] lines = File.ReadAllLines(file, Encoding.UTF8);
        string[] lineValus = null;
        for (int i = 2; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i]))
            {
                Debug.LogError(string.Format("{0}文件第{1}行为空行", file, i + 1));
                return false;
            }

            lineValus = lines[i].Split('\t');

            string showname = lineValus[line];
            if (!Filtration(ref showname))
            {
                continue;
            }

            uint hashCode = XCommon.singleton.XHash(lineValus[line]);
            if (builder.ContainsKey(hashCode))
            {
                Debug.Log(string.Format("存在相同的ID:{0}", lineValus[line]));
                continue;
            }



            string flag = lineValus[line + 2];
            Module module;
            module.showName = showname;
            if (!string.IsNullOrEmpty(lineValus[line + 2]))
            {
                module.luaFlag = int.Parse(lineValus[line + 2]);
            }
            else
            {
                module.luaFlag = 0;
            }
            module.hashCode = hashCode;
            if (string.IsNullOrEmpty(lineValus[line + 4]))
            {
                module.systemID = 0;
            }
            else
            {
                module.systemID = int.Parse(lineValus[line + 4]);
            }
            module.className = lineValus[line + 1];
            module.prefab = lineValus[line+3];
            builder.Add(hashCode, module);
        }
        return true;
    }


    #endregion


    public delegate bool FilterLine(string[] lineValues);
    private static bool FilterLines(string[] lineValues)
    {

        if (lineValues != null && lineValues.Length > 1)
        {
            uint part = uint.Parse(lineValues[1]);
            if (part < 2 && part != _currentPart) return false;
        }
        return true;
    }

}
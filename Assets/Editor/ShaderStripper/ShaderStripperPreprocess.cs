using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class ShaderStripperTool : EditorWindow
{
    [MenuItem("Tools/引擎/Shader半精度 变体组合分析")]
    public static void OpenShaderStripperToolWindow()
    {
        ShaderStripperTool window = EditorWindow.CreateInstance<ShaderStripperTool>();
        window.minSize = new Vector2(500, 250);
        window.Show();
    }


    [MenuItem("Tools/引擎/Shader半精度 开启")]
    public static void ShaderHalfOpen()
    {

        string srcFolder = Application.dataPath + "/Engine/Runtime/Shaders/TmpShader/";
        string srcFile = srcFolder + "CommonCF.hlsl";
        string targetFile = Application.dataPath.Replace("/Assets",
            "/Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonCF.hlsl");
        File.Copy(srcFile, targetFile,true);

        srcFile = srcFolder + "CommonAPI.hlsl";
        targetFile = Application.dataPath + "/Engine/Runtime/Shaders/Shading/NBR/API/CommonAPI.hlsl";
        File.Copy(srcFile, targetFile,true);
    }


    private string m_LastShaderFileOrFolder;

    private void OnGUI()
    {

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Shader文件：", m_LastShaderFileOrFolder);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("选择Shader文件", GUILayout.Width(200)))
        {
            string newFile = EditorUtility.OpenFilePanel("选择Shader文件", m_LastShaderFileOrFolder, "shader");
            if (!string.IsNullOrEmpty(newFile) && newFile != m_LastShaderFileOrFolder)
            {
                m_LastShaderFileOrFolder = newFile;
                ExportShader(ExportType.None);
            }
        }

        if (GUILayout.Button("选择Shader文件夹", GUILayout.Width(200)))
        {
            string newFile = EditorUtility.OpenFolderPanel("选择Shader文件夹", m_LastShaderFileOrFolder, "shader");
            m_LastShaderFileOrFolder = newFile;
        }


        if (GUILayout.Button("导出", GUILayout.Width(200)))
        {
            ExportShader(ExportType.None);
        }

        if (GUILayout.Button("按照高中低导出", GUILayout.Width(200)))
        {
            ExportShader(ExportType.SortByLevel);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("导出变量精度", GUILayout.Width(200)))
        {
            ExportShader(ExportType.TypeDetail);
        }

        if (GUILayout.Button("导出半精度预览", GUILayout.Width(200)))
        {
            ExportShader(ExportType.TypeDetailTotal);
        }
        
        EditorGUILayout.EndHorizontal();
    }

    public enum ExportType
    {
        None,
        SortByLevel,
        TypeDetail,
        TypeDetailTotal,
    }
    
    private string m_LastShaderFile;
    private bool m_IsFolder = false;
    private void ExportShader(ExportType exportType)
    {
        m_IsFolder = false;
        
        InitParseState();

        List<string> allShaders = new List<string>();
        if (m_LastShaderFileOrFolder.Contains("."))
        {
            allShaders.Add(m_LastShaderFileOrFolder);
        }
        else
        {
            string[] files = Directory.GetFiles(m_LastShaderFileOrFolder, "*.shader");
            allShaders.AddRange(files);
        }


        List<string> allList = new List<string>();
        for (int i = 0; i < allShaders.Count; i++)
        {
            string filePath = allShaders[i];
            m_LastShaderFile = filePath;
            EditorUtility.DisplayProgressBar("Analysis", filePath, i * 1.0f / allShaders.Count);
            Dictionary<string, ShaderParseData> total_variants = ParseConfig(m_LastShaderFile);
            List<string> tmp = PrintTotalVariants(total_variants, exportType);
            
            switch (exportType)
            {
                case ExportType.TypeDetailTotal:
                    allList.AddRange(tmp);
                    break;
            }

            Debug.Log("ShaderStripperTool:" + m_LastShaderFile);
        }

        EditorUtility.ClearProgressBar();
        
        switch (exportType)
        {
            case ExportType.TypeDetailTotal:
                string targetFile = m_LastShaderFileOrFolder+("/cfgameShaderTypes.txt");
                File.WriteAllLines(targetFile, allList);
                break;
        }
    }

    private List<string> PrintTotalVariants(Dictionary<string, ShaderParseData> total_variants, ExportType exportType)
    {
        List<string> allKeys = new List<string>();

        string fileName =m_LastShaderFile.Substring(m_LastShaderFile.LastIndexOf(@"\")+1);
        
        foreach (KeyValuePair<string, ShaderParseData> itm in total_variants)
        {
            switch (exportType)
            {
                case ExportType.TypeDetail:
                case ExportType.TypeDetailTotal:
                case ExportType.SortByLevel:
                    itm.Value.Sort();
                    break;
            }

            bool needBreak = false;
            foreach (var strItm in itm.Value.subPros)
            {
                
                if (needBreak)
                    break;
                

                switch (exportType)
                {
                    case ExportType.TypeDetail:
                        allKeys.Add(strItm.GetTypeDetail());
                        break;
                    case ExportType.TypeDetailTotal:
                        if (strItm.CheckHasMediump())
                        {
                            allKeys.Add(fileName + "   " + strItm.GetTypeDetail());
                            needBreak = true;
                        }
                        
                        break;
                    default:
                        allKeys.Add(strItm.GetNormal());
                        break;
                }

            }
        }

        switch (exportType)
        {
            case ExportType.TypeDetailTotal:
                break;
            default:
                string targetFile = m_LastShaderFile.Replace(".shader", ".txt");
                File.WriteAllLines(targetFile, allKeys);
                break;
        }

        return allKeys;
    }


    private Dictionary<string, int> ParseState = new Dictionary<string, int>();

    private List<string> shaderDataTypes = new List<string>();
    private string mediump = "mediump";
    private void InitParseState()
    {
        ParseState["None"] = 0;
        ParseState["SubProgram"] = 1;
        ParseState["Pass"] = 2;
        ParseState["PassFind"] = 3;
        ParseState["Keywords"] = 4;
        ParseState["Local Keywords"] = 5;

        shaderDataTypes = new List<string>();

        shaderDataTypes.Add(" float ");
        shaderDataTypes.Add(" vec2 ");
        shaderDataTypes.Add(" vec3 ");
        shaderDataTypes.Add(" vec4 ");
        
    }

    public class ShaderParseData
    {
        public List<SubProgramData> subPros = new List<SubProgramData>();

        public void Sort()
        {
            subPros.Sort(CompareItem);
        }

        protected int CompareItem(SubProgramData x, SubProgramData y)
        {
            x.SetKeyString();
            y.SetKeyString();
            return string.Compare(x.allKeywords, y.allKeywords, StringComparison.Ordinal);
        }
    }

    public class SubProgramData
    {
        public string passName;
        public List<string> highList = new List<string>();
        public List<string> mediumpList = new List<string>();
        public List<string> keywords = new List<string>();

        public bool CheckHasMediump()
        {
            return mediumpList.Count > 0 && firstIndex == 2;
        }

        public SubProgramData(string pa)
        {
            passName = pa;
        }

        public static List<string[]> firstKeyWordsList = new List<string[]>() { new String[3]
        {
            "_SHADER_LEVEL_HIGH","_SHADER_LEVEL_MEDIUM","_SHADER_LEVEL_LOW"
        },new String[3]
        {
            "_FX_LEVEL_HIGH","_FX_LEVEL_MEDIUM","_FX_LEVEL_LOW"
        }};


    public string GetTypeDetail()
        {
            return GetNormal() +
                   "\n high:" + string.Join(" ", highList) +
                   "\n med:" + string.Join(" ", mediumpList)+
                   "\n";
        }


        private string GetCommonStr()
        {
            SetKeyString();
            
            string per = " medPer: " +
                         (int) (mediumpList.Count * 1.0f /
                             (highList.Count + mediumpList.Count) * 100) + "%";

            return passName+"---->high:" + highList.Count + " med:" + mediumpList.Count + per;
        }

        public string GetNormal()
        {
            return GetCommonStr() +
                   " keywords:" + allKeywords;
        }

        public string allKeywords="";

        public void SetKeyString()
        {
            if (string.IsNullOrEmpty(allKeywords))
            {
                SetFirstKeyword();
                Sort();
                allKeywords = string.Join(" ", keywords);

            }
        }

        string firstStr = "";
        private int firstIndex = 0;

        public void SetFirstKeyword()
        {
            foreach (string[] firstKeyWords in firstKeyWordsList)
            {
                for (int i = 0; i < firstKeyWords.Length; i++)
                {
                    if (keywords.Contains(firstKeyWords[i]))
                    {
                        firstStr = firstKeyWords[i];
                        firstIndex = i;
                        break;
                    }
                }
            }
        }

        public void Sort()
        {
            keywords.Sort();

            if (!string.IsNullOrEmpty(firstStr))
            {
                if (keywords.Contains(firstStr)) keywords.Remove(firstStr);
                keywords.Insert(0, firstIndex + " " + firstStr);
            }
        }


    }


    private void CheckDataType(SubProgramData dataType, string line)
    {
        Debug.Log("CheckDataType***:"+line);
        if (dataType == null || string.IsNullOrEmpty(line)) return;

        for (int i = 0; i < shaderDataTypes.Count; i++)
        {
            string typeStr = shaderDataTypes[i];
            if (!line.Contains(typeStr)) continue;
            if (line.Contains(typeStr+"(")) continue;
            
            string[] arrs = line.Split(new char[1] {' '}, StringSplitOptions.None);
            string typeName = arrs[arrs.Length - 1].Replace(";", "");
            
            Debug.Log("CheckDataType:"+line);
            
            if (line.Contains(mediump))
            {
                dataType.mediumpList.Add(typeName);
            }
            else
            {
                dataType.highList.Add(typeName);
            }
            
            break;
        }
    }

    private Dictionary<string, ShaderParseData> ParseConfig(string filePath)
    {
        string[] allLines = File.ReadAllLines(filePath);

        int current_state = ParseState["None"];
        List<string> current_keywords = new List<string>();
        string current_pass = string.Empty;
        Dictionary<string, ShaderParseData> total_variants = new Dictionary<string, ShaderParseData>();

        SubProgramData _SubProgramData = null;
        
        for(int i=0;i<allLines.Length;i++)
        {
            string line = allLines[i];
            
            if (current_state == ParseState["None"] && line.Contains("Pass {"))
                current_state = ParseState["Pass"];
            else if (current_state == ParseState["Pass"])
            {
                string[] mc = Regex.Matches(line, "(?<=\")[\\w]+(?!=\")").Cast<Match>()
                    .Select(m => m.Value)
                    .ToArray();

                if (mc.Length == 0)
                {
                    Debug.LogWarning("line:" + line + "  index:" + i);
                    current_pass = i + "-" + line;
                }
                else
                {
                    current_pass = mc[0];
                }
                
                total_variants[current_pass] = new ShaderParseData();
                
                current_state = ParseState["PassFind"];
            }
            else if (current_state == ParseState["PassFind"])
            {
                if (line.Contains("SubProgram"))
                {
                    current_state = ParseState["SubProgram"];
                    current_keywords = new List<string>();
                }
                else if (line.Contains("Pass {"))
                {
                    current_state = ParseState["Pass"];
                }
            }
            else if (current_state == ParseState["SubProgram"])
            {
                if (line.StartsWith("Keywords"))
                {
                    current_state = ParseState["Keywords"];
                    
                    string[] mc = Regex.Matches(line, "(?<=\")[\\w]+(?!=\")").Cast<Match>()
                        .Select(m => m.Value)
                        .ToArray();
                    current_keywords.AddRange(mc);
                }
                else if (line.StartsWith("Local Keywords"))
                {
                    current_state = ParseState["Local Keywords"];
                    string[] mc = Regex.Matches(line, "(?<=\")[\\w]+(?!=\")").Cast<Match>()
                        .Select(m => m.Value)
                        .ToArray();
                    current_keywords.AddRange(mc);
                }
                else
                {
                    current_state = ParseState["PassFind"];

                    _SubProgramData = new SubProgramData(current_pass);
                    _SubProgramData.keywords = current_keywords;
                    total_variants[current_pass].subPros.Add(_SubProgramData);
                }
            }
            else if (current_state == ParseState["Keywords"])
            {
                if (line.StartsWith("Local Keywords"))
                {
                    current_state = ParseState["Local Keywords"];
                    string[] mc = Regex.Matches(line, "(?<=\")[\\w]+(?!=\")").Cast<Match>()
                        .Select(m => m.Value)
                        .ToArray();
                    current_keywords.AddRange(mc);
                }
                else
                {
                    current_state = ParseState["PassFind"];
                    
                    _SubProgramData = new SubProgramData(current_pass);
                    _SubProgramData.keywords = current_keywords;
                    total_variants[current_pass].subPros.Add(_SubProgramData);
                }
            }
            else if (current_state == ParseState["Local Keywords"])
            {
                current_state = ParseState["PassFind"];
                
                _SubProgramData = new SubProgramData(current_pass);
                _SubProgramData.keywords = current_keywords;
                total_variants[current_pass].subPros.Add(_SubProgramData);
            }

            CheckDataType(_SubProgramData, line);
        }

        return total_variants;
    }

}

public class ShaderStripperPreprocess : IPreprocessShaders
{
    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> shaderCompilerData)
    {
        // int count = shaderCompilerData.Count;
        //
        // Debug.Log(shader+"  passName:"+snippet.passName+"  passType:"+snippet.passType+"  shaderType:"+snippet.shaderType+" *** shaderCompilerData:"+count);
        //
        // for (int i = 0; i < count; ++i)
        // {
        //     ShaderKeywordSet ks = shaderCompilerData[i].shaderKeywordSet;
        //     ShaderKeyword[] shaderKeywords = ks.GetShaderKeywords();
        //
        //     string shaderStr = string.Empty;
        //
        //     foreach (var itm in shaderKeywords)
        //     {
        //         shaderStr += ShaderKeyword.GetGlobalKeywordName(itm) + ";";
        //     }
        //
        //     Debug.Log(shader + "  " + shaderStr, shader);
        //}
    }

    public int callbackOrder
    {
        get { return 0; }
    }
}

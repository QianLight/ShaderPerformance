using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using ShaderInstructAnalyze;
public class ShaderAnalyzeTool : EditorWindow
{
    [MenuItem("Tools/ÒýÇæ/GPU-Arm mali·ÖÎö")]
    public static void OpenShaderAnalyzeToolWindow()
    {
        ShaderAnalyzeTool window = EditorWindow.CreateInstance<ShaderAnalyzeTool>();
        window.Show();
    }

    ShaderAndAnalyeResult _shaderAndAnalyeResult;
    string _fragmentAnalyzeResultStr;
    string _vertexAnalyzeResultStr;

    string _cyclePath = "Total Cycle";
    CyclePathType _cyclePathType = CyclePathType.total;
    ShaderType _shaderType = ShaderType.fragment;

    string _topShaderRange = "0";
    int _topShaderRangeApply = 0;
    List<ShaderlabInfo> _topShadersInfos = new List<ShaderlabInfo>();
    List<VariantInfo> _currentPassVariants = new List<VariantInfo>();

    string _shaderlabPath = "none";
    string _workDirectory;
    string _editorToolWorkDirectory;
    string _compiledShaderPath;
    string _analyzeResultPath;
    string _analyzeResultBinName = "shaderAndAnalyeResult.bin";

    int _realWindowWidth;
    int _realWindowHeight;
    Vector2[] _scrollPositions = new Vector2[4];
    GUIStyle _topShaderInfoButtonStyle;

    int _currentTopShaderIndex = 0;

    private void OnEnable()
    {
        _workDirectory = Application.dataPath + "/../ShaderInstructAnalyzeTemp/";
        _editorToolWorkDirectory = Application.dataPath + "/Editor/ShaderInstructAnalyze/";
        _compiledShaderPath = _workDirectory + "CompiledShaderlab/ ";
        _analyzeResultPath = _workDirectory + "AnalyzeResult/";

        CheckWorkDirectory();

        if (File.Exists(_editorToolWorkDirectory + _analyzeResultBinName))
        {
            FileStream fs = new FileStream(_editorToolWorkDirectory + _analyzeResultBinName, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            _shaderAndAnalyeResult = bf.Deserialize(fs) as ShaderAndAnalyeResult;
            fs.Close();
            _topShadersInfos = _shaderAndAnalyeResult.GetTopShaderInfos(_topShaderRangeApply, _shaderType, _cyclePathType);
        }
        else
        {
            Debug.LogError("there is no shader analye result, please updata database");
        }
    }

    private void OnGUI()
    {
        _topShaderInfoButtonStyle = new GUIStyle("ButtonLeft");
        _topShaderInfoButtonStyle.alignment = TextAnchor.MiddleLeft;

        _realWindowWidth = (int)(Screen.width * 96 / Screen.dpi);
        _realWindowHeight = (int)(Screen.height * 96 / Screen.dpi);

        float leftPadding = _realWindowWidth * 0.005f;
        GUILayout.BeginArea(new Rect(leftPadding, _realWindowHeight * 0.01f, _realWindowWidth * 0.64f, _realWindowHeight * 0.20f));
        Settings();
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(leftPadding, _realWindowHeight * 0.21f, _realWindowWidth * 0.64f, _realWindowHeight * 0.38f));
        ShowTopShaderInfos(_topShaderRangeApply, _topShadersInfos, _shaderType, _cyclePathType, ref _scrollPositions[0]);
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(leftPadding, _realWindowHeight * 0.6f, _realWindowWidth * 0.64f, _realWindowHeight * 0.35f)); 
        if (_topShadersInfos.Count > _currentTopShaderIndex)
        {
            ShowShaderLabInfo(_topShadersInfos[_currentTopShaderIndex], _shaderType, _cyclePathType, ref _scrollPositions[1]);
        }
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(_realWindowWidth * 0.66f, _realWindowHeight * 0.02f, _realWindowWidth * 0.33f, _realWindowHeight * 0.46f));
        ShowShaderAnalyzeResult("Vertex Shader Analyze Result:", _vertexAnalyzeResultStr, ref _scrollPositions[2]);
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(_realWindowWidth * 0.66f, _realWindowHeight * 0.51f, _realWindowWidth * 0.33f, _realWindowHeight * 0.44f));
        ShowShaderAnalyzeResult("Fragment Shader Analyze Result:", _fragmentAnalyzeResultStr, ref _scrollPositions[3]);
        GUILayout.EndArea();
    }
    
    void Settings()
    {
        GUILayout.BeginVertical();
        GUILayout.Label("Compile ShaderLab : ");

        GUILayout.BeginHorizontal();
        _shaderlabPath = EditorGUILayout.TextField("ShaderLab path", _shaderlabPath);
        if (GUILayout.Button("Compile ShaderLab"))
        {
            CheckWorkDirectory();
            List<Shader> shaders = new List<Shader>();
            List<FileInfo> fileInfos = new List<FileInfo>();
            
            List<FileInfo> files = new List<FileInfo>();
            if (!_shaderlabPath.Contains(".shader"))
            {
                DirectoryInfo folder = new DirectoryInfo(_shaderlabPath);
                files = HelpFunction.GetAllFiles(ref fileInfos, folder, "*.shader");
            }
            else
            {
                FileInfo folder = new FileInfo(_shaderlabPath);
                files.Add(folder);
            }
            
            for (int i = 0; i < files.Count; i++)
            {
                string shaderlabCode = System.Text.Encoding.Default.GetString(HelpFunction.LoadFile(files[i].FullName));

                string pattern = "Shader.*\".*\"";
                Regex regex = new Regex(pattern);
                Match match = regex.Match(shaderlabCode);
                if (!match.Success)
                {
                    Debug.LogError("can't get shader name" + files[i].Name);
                    continue;
                }
                string shaderName = match.Value.Split(new char[] { '\"' })[1];
                shaders.Add(Shader.Find(shaderName));
            }
            GetCompiledShaderCode.Make(shaders.ToArray());
            DirectoryInfo compiledShaderFolder = new DirectoryInfo(Application.dataPath + "/../Temp/");
            var compiledShaderCodes = compiledShaderFolder.GetFiles("*.shader");
            foreach (var compiledShaderCode in compiledShaderCodes)
            {
                File.Copy(compiledShaderCode.FullName, _compiledShaderPath + compiledShaderCode.Name, true);
            }
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Update Database"))
        {
            CheckWorkDirectory();
            _shaderAndAnalyeResult = new ShaderAndAnalyeResult();
            _shaderAndAnalyeResult.SetShaderlabFolder(_workDirectory);
            DirectoryInfo folder = new DirectoryInfo(_compiledShaderPath);
            var files = folder.GetFiles("*.shader");
            for (int i = 0; i < files.Length; i++)
            {
                _shaderAndAnalyeResult.AddShaderlab(files[i].Name);
            }

            FileStream stream = new FileStream(_editorToolWorkDirectory + _analyzeResultBinName, FileMode.Create, FileAccess.Write, FileShare.None);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, _shaderAndAnalyeResult);
            stream.Close();
            Debug.Log("Update Success!!");
        }

        GUILayout.Space(20);
        GUILayout.Label("Top Shader Display : ");
        _topShaderRange = EditorGUILayout.TextField("top shader range", _topShaderRange);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Show Top " + _topShaderRange + " Fragment Shaders Sort By " + _cyclePath))
        {
            _shaderType = ShaderType.fragment;
            _topShaderRangeApply = int.Parse(_topShaderRange);
            _topShadersInfos = _shaderAndAnalyeResult.GetTopShaderInfos(int.Parse(_topShaderRange), _shaderType, _cyclePathType);
        }
        if (GUILayout.Button("Show Top " + _topShaderRange + " Vetrex Shaders Sort By " + _cyclePath))
        {
            _shaderType = ShaderType.vertex;
            _topShaderRangeApply = int.Parse(_topShaderRange);
            _topShadersInfos = _shaderAndAnalyeResult.GetTopShaderInfos(int.Parse(_topShaderRange), _shaderType, _cyclePathType);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Sort By Total Cycle"))
        {
            _cyclePathType = CyclePathType.total;
            _cyclePath = "Total Cycle";
        }
        if (GUILayout.Button("Sort By Longest Cycle"))
        {
            _cyclePathType = CyclePathType.longest;
            _cyclePath = "Longest Cycle";
        }
        if (GUILayout.Button("Sort By Shortest Cycle"))
        {
            _cyclePathType = CyclePathType.shortest;
            _cyclePath = "Shortest Cycle";
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    void ShowTopShaderInfos(int topShaderRange, List<ShaderlabInfo> topShaderInfos, ShaderType shaderType, CyclePathType cyclePathType, ref Vector2 scrollPositions)
    {
        scrollPositions = EditorGUILayout.BeginScrollView(scrollPositions);
        for (int i = 0; i < Math.Min(topShaderRange, topShaderInfos.Count); i++)
        {
            if (GUILayout.Button(i + ":" + topShaderInfos[i].m_topVariantInfos.GetInfo(shaderType, cyclePathType), _topShaderInfoButtonStyle))
            {
                _currentTopShaderIndex = i;
            };
        }
        EditorGUILayout.EndScrollView();
    }

    void ShowShaderLabInfo(ShaderlabInfo shaderlabInfo, ShaderType shaderType, CyclePathType cyclePathType, ref Vector2 scrollPositions)
    {
        scrollPositions = EditorGUILayout.BeginScrollView(scrollPositions);
        GUILayout.BeginHorizontal();
        
        GUILayout.BeginVertical();
        foreach (var pass in shaderlabInfo._passVariantsMap)
        {
            if(GUILayout.Button("pass : " + pass.Key, _topShaderInfoButtonStyle))
            {
                _currentPassVariants = pass.Value;
            }
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        foreach(var variant in _currentPassVariants)
        {
            if (GUILayout.Button(variant.GetInfo(shaderType, cyclePathType), _topShaderInfoButtonStyle))
            {
                string[] queryCondition = variant.GetQueryConditions();
                AnalyzeResult[] analyzeResult = _shaderAndAnalyeResult.GetAnalyzeResult(queryCondition[0],
                    queryCondition[1],
                    queryCondition[2].Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries));
                if (analyzeResult == null)
                {
                    _vertexAnalyzeResultStr = "Query condition illegal";
                }
                else
                {
                    _vertexAnalyzeResultStr = analyzeResult[0].Show();
                    _fragmentAnalyzeResultStr = analyzeResult[1].Show();
                    Debug.Log("ShowShaderLabInfoPath:" + analyzeResult[0].filePath);
                }
            }
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }

    void ShowShaderAnalyzeResult(string noteInfo, string shaderAnalyzeResult, ref Vector2 scrollPosition)
    {
        GUILayout.BeginVertical();
        GUILayout.Label(noteInfo);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        GUILayout.Label(shaderAnalyzeResult);
        EditorGUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    void CheckWorkDirectory()
    {
        if (!Directory.Exists(_compiledShaderPath))
        {
            Directory.CreateDirectory(_compiledShaderPath);
        }
        if (!Directory.Exists(_analyzeResultPath))
        {
            Directory.CreateDirectory(_analyzeResultPath);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System;

namespace ShaderInstructAnalyze
{
    [Serializable]
    public class ShaderAndAnalyeResult
    {
        // TODO: 按照total cycle的指令数，对shader变体进行排序。排序后，可以支持反向查询到对应的ShaderLab名称和keywords组合
        private Dictionary<string, Dictionary<string, Pass>> _shaderlabPassesMap = new Dictionary<string, Dictionary<string, Pass>>();
        private List<VariantInfo> _variantsInfo = new List<VariantInfo>();
        private string _shaderFolderPath;
        public void SetShaderlabFolder(string shaderFolderPath)
        {
            _shaderFolderPath = shaderFolderPath;
        }
        public void AddShaderlab(string shaderlabFileName)
        {
            string shaderlabCode = System.Text.Encoding.Default.GetString(HelpFunction.LoadFile(_shaderFolderPath + "CompiledShaderlab/" + shaderlabFileName));
            string shaderlabName = GetNativeShaderlabName(shaderlabFileName);
            string shaderlabWorkFlord = _shaderFolderPath + "AnalyzeResult/" + shaderlabName + '/';
            if(_shaderlabPassesMap.ContainsKey(shaderlabName))
            {
                shaderlabName = shaderlabFileName.Split('.')[0];
            }
            _shaderlabPassesMap.Add(shaderlabName, CreateNamePassMap(shaderlabCode, shaderlabWorkFlord, shaderlabName));
        }
        public AnalyzeResult[] GetAnalyzeResult(string shaderlabName, string passName, string[] keywords)
        {
            if(_shaderlabPassesMap.ContainsKey(shaderlabName))
            {
                var namePassMap = _shaderlabPassesMap[shaderlabName];
                if(namePassMap.ContainsKey(passName))
                {
                    var pass = namePassMap[passName];
                    return pass.GetAnalyzeResult(keywords);
                }
            }
            return null;
        }
        public string ShowTopShaders(int range, ShaderType shaderTpye, CyclePathType cyclePathType)
        {
            SortVariants(shaderTpye, cyclePathType);
            string topShaderInfo = "Top " + range + " variants is :\n";
            for (int i = 1; i <= range; i++)
            {
                topShaderInfo += i.ToString() + " : " + _variantsInfo[i-1].GetInfo(shaderTpye, cyclePathType) + "\n";
            }
            return topShaderInfo;
        }

        public List<ShaderlabInfo> GetTopShaderInfos(int range, ShaderType shaderTpye, CyclePathType cyclePathType)
        {
            SortVariants(shaderTpye, cyclePathType);
            range = Math.Min(range, _shaderlabPassesMap.Count);
            List<ShaderlabInfo> shaderLabInfos = new List<ShaderlabInfo>();

            for(int i = 0; i < _variantsInfo.Count; i++)
            {
                string[] queryConditions = _variantsInfo[i].GetQueryConditions();
                string shaderlabName = queryConditions[0];
                string passName = queryConditions[1];
                bool haveFindShaderlab = false;
                for(int j = 0; j < shaderLabInfos.Count; j++)
                {
                    if(shaderLabInfos[j].m_name == shaderlabName)
                    {
                        haveFindShaderlab = true;
                        if(shaderLabInfos[j]._passVariantsMap.ContainsKey(passName))
                        {
                            shaderLabInfos[j]._passVariantsMap[passName].Add(_variantsInfo[i]);
                        }
                        else
                        {
                            List<VariantInfo> variants = new List<VariantInfo>();
                            variants.Add(_variantsInfo[i]);
                            shaderLabInfos[j]._passVariantsMap.Add(passName, variants);
                        }
                    }
                }
                if(!haveFindShaderlab)
                {
                    ShaderlabInfo shaderlabInfo = new ShaderlabInfo(_variantsInfo[i]);
                    shaderLabInfos.Add(shaderlabInfo);
                }
            }
            return shaderLabInfos;
        }

        private void SortVariants(ShaderType shaderType, CyclePathType sortBy)
        {
            if(shaderType == ShaderType.fragment)
            {
                switch (sortBy)
                {
                    case CyclePathType.total:
                        _variantsInfo.Sort((a, b) => -a.m_totalFragmentCycleCount.CompareTo(b.m_totalFragmentCycleCount));
                        break;
                    case CyclePathType.longest:
                        _variantsInfo.Sort((a, b) => -a.m_longestFragmentCycleCount.CompareTo(b.m_longestFragmentCycleCount));
                        break;
                    case CyclePathType.shortest:
                        _variantsInfo.Sort((a, b) => -a.m_shortestFragmentCycleCount.CompareTo(b.m_shortestFragmentCycleCount));
                        break;
                }
            }
            else
            {
                switch (sortBy)
                {
                    case CyclePathType.total:
                        _variantsInfo.Sort((a, b) => -a.m_totalVertexCycleCount.CompareTo(b.m_totalVertexCycleCount));
                        break;
                    case CyclePathType.longest:
                        _variantsInfo.Sort((a, b) => -a.m_longestVertexCycleCount.CompareTo(b.m_longestVertexCycleCount));
                        break;
                    case CyclePathType.shortest:
                        _variantsInfo.Sort((a, b) => -a.m_shortestVertexCycleCount.CompareTo(b.m_longestVertexCycleCount));
                        break;
                }
            }
            
        }
        private string GetNativeShaderlabName(string shaderlabFileName)
        {
            string[] ss = shaderlabFileName.Split('.')[0].Split('-');
            return ss[ss.Length - 1].Replace(' ', '_');
        }
        private Dictionary<string, Pass> CreateNamePassMap(string shaderlabCode, string shaderlabWorkFlord, string shaderlabName)
        {
            Dictionary<string, Pass> namePassMap = new Dictionary<string, Pass>();
            int noNamePassIndex = 0;
            while (shaderlabCode.Contains("Pass"))
            {
                string passCode = GetOnepassOfShaderlab(ref shaderlabCode);
                if (passCode == null)
                {
                    Debug.LogError("no pass residue");
                    break;
                }

                Debug.Log("pass code is : " + passCode);
                string passName = GetPassName(passCode);
                if (passName == "")
                {
                    passName = "no_name_pass" + noNamePassIndex;
                    noNamePassIndex++;
                }
                string passWorkFlord = shaderlabWorkFlord + passName + '/';

                Debug.Log("passWorkFlord is : " + passWorkFlord);
                while(namePassMap.ContainsKey(passName))
                {
                    passName += '_';
                }
                Pass pass = CreatePass(passCode, passWorkFlord, shaderlabName, passName);
                namePassMap.Add(passName, pass);
            }
            return namePassMap;
        }

        private Pass CreatePass(string passCode, string passWorkFlord, string shaderlabName, string passName)
        {
            Pass pass = new Pass();
            int index = 0;
            while (passCode.Contains("Global Keywords:"))
            {
                string variantCode = GetOnevariantOfPass(ref passCode);
                string[] keywords = GetKeywords(variantCode);
                string fragmentShader = GetGlsl(ShaderType.fragment, variantCode);
                string vertexShader = GetGlsl(ShaderType.vertex, variantCode);
                if(fragmentShader == null || vertexShader == null)
                {
                    Debug.LogError("can't get glsl shader.shaderlab : " + shaderlabName + " pass : " + passName + " keywords : " + pass.stringArrayToString(keywords));
                    continue;
                }

                string fragmentName = index + ".frag";
                string vertexName = index + ".vert";
                HelpFunction.SaveToFile(fragmentShader, passWorkFlord, fragmentName);
                HelpFunction.SaveToFile(vertexShader, passWorkFlord, vertexName);

                AnalyzeResult fragmentAnalyzeResult = AnalyzeFragmentShader(passWorkFlord + fragmentName);
                fragmentAnalyzeResult.filePath = passWorkFlord + fragmentName;
                
                AnalyzeResult vertexAnalyzeResult = AnalyzeFragmentShader(passWorkFlord + vertexName);
                vertexAnalyzeResult.filePath = passWorkFlord + vertexName;
                
                if (fragmentAnalyzeResult == null || vertexAnalyzeResult == null)
                {
                    Debug.LogError("shader analyze result is null");
                    continue;
                }

                AnalyzeResult[] variantAnalyzeResult = new AnalyzeResult[] { vertexAnalyzeResult, fragmentAnalyzeResult };
                pass.AddVariant(keywords, variantAnalyzeResult);

                AddVariantsInfo(shaderlabName, passName, keywords, variantAnalyzeResult);
                index++;
            }
            return pass;
        }

        private void AddVariantsInfo(string shaderlabName, string passName, string[] keywords, AnalyzeResult[] variantsAnalyzeResult)
        {
            VariantInfo variantInfo = new VariantInfo(shaderlabName, passName, keywords);
            variantInfo.m_totalCycleCount = 0;

            variantInfo.m_totalVertexCycleCount = variantsAnalyzeResult[0].GetTotalCycle(CyclePathType.total);
            variantInfo.m_longestVertexCycleCount = variantsAnalyzeResult[0].GetTotalCycle(CyclePathType.longest);
            variantInfo.m_shortestVertexCycleCount = variantsAnalyzeResult[0].GetTotalCycle(CyclePathType.shortest);

            variantInfo.m_totalFragmentCycleCount = variantsAnalyzeResult[1].GetTotalCycle(CyclePathType.total); 
            variantInfo.m_longestFragmentCycleCount = variantsAnalyzeResult[1].GetTotalCycle(CyclePathType.longest);
            variantInfo.m_shortestFragmentCycleCount = variantsAnalyzeResult[1].GetTotalCycle(CyclePathType.shortest);

            _variantsInfo.Add(variantInfo);
        }     
        
        

        private AnalyzeResult AnalyzeFragmentShader(string fragmentShaderPath)
        {
            return GetAnalyzeResult(RunOfflineCompiler(fragmentShaderPath));
        }

        private AnalyzeResult GetAnalyzeResult(string offlineCompilerResult)
        {
            if(offlineCompilerResult.Contains("error"))
            {
                Debug.LogError("!!!offlineCompilerResult have error : " + offlineCompilerResult.Replace("null", "0"));
                return null;
            }
            AnalyzeResult analysisResult = JsonConvert.DeserializeObject<AnalyzeResult>(offlineCompilerResult.Replace("null", "0"));
            return analysisResult;
        }

        private AnalyzeResult GetAnalyzeResultAndSave(string offlineCompilerResult, string fileName)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            AnalyzeResult analysisResult = JsonConvert.DeserializeObject<AnalyzeResult>(offlineCompilerResult.Replace("null", "0"), settings);
            string result = JsonConvert.SerializeObject(analysisResult);
            Debug.LogError(JsonConvert.SerializeObject(analysisResult));
            HelpFunction.SaveToFile(result, _shaderFolderPath + "JSON/", fileName);
            return analysisResult;
        }
        private string RunOfflineCompiler(string shaderPath)
        {
            string args = " -c Mali-G78 --format json " + shaderPath;
            string offlineCompilerPath = "D:/GraphicTools/Arm Mobile Studio 2021.4/mali_offline_compiler/malioc.exe";
            Debug.Log("args is : " + args);
            var workdir = _shaderFolderPath;
            var results = HelpFunction.RunCmd(offlineCompilerPath, args, workdir);
            // Debug.Log('compiler error: ' + results[1]);
            return results[0];
        }
        private string GetOnepassOfShaderlab(ref string shaderlabCode)
        {
            //string pattern = "Pass(\n| )*\\{[^]*\\}";
            int begin = 0, end = 0;
            GetTagBlock(shaderlabCode, "Pass", ref begin, ref end);
            if (end <= begin) return null;

            string passCode = shaderlabCode.Substring(begin, end - begin + 1);
            shaderlabCode = shaderlabCode.Substring(end + 1);

            return passCode;
        }
        private string GetPassName(string passCode)
        {
            string name = "";
            string pattern = "Name.\"\\w*\"";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(passCode);
            if (match.Success)
            {
                int start = match.Value.IndexOf('\"');
                name = match.Value.Substring(start + 1, match.Value.Length - start - 2);
            }

            return name;
        }

        private string GetOnevariantOfPass(ref string passCode)
        {
            string pattern = "Global Keywords: ";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(passCode);
            if (match.Success)
            {
                int begin = match.Index;
                match = regex.Match(passCode.Substring(begin + pattern.Length));

                int end = passCode.Length - 1;
                if(match.Success)
                {
                    end = match.Index - 1 + (begin + pattern.Length);
                }

                if (end < begin)
                {
                    Debug.LogError("begin end is : " + begin + " & " + end);
                    Debug.LogError("Pass code is :\n " + passCode);
                }
                string variantCode = passCode.Substring(begin, end - begin);
                passCode = passCode.Substring(end) + 1;
                return variantCode;
            }
            else
            {
                return "";
            }
        }
        private string GetGlsl(ShaderType shaderType, string variantCode)
        {
            string pattern = "#ifdef VERTEX";
            if (shaderType == ShaderType.fragment)
            {
                pattern = "#ifdef FRAGMENT";
            }
            Regex regex = new Regex(pattern);
            Match match = regex.Match(variantCode);

            Stack ifMatch = new Stack();
            if (match.Success)
            {
                ifMatch.Push("#if");
                variantCode = variantCode.Substring(match.Index + pattern.Length + 1);
            }
            else return null;

            int glslEnd = 0;
            while (glslEnd < variantCode.Length - 6)
            {
                if (variantCode.Substring(glslEnd, 3) == "#if")
                {
                    ifMatch.Push("#if");
                    glslEnd += 3;
                }
                else if (variantCode.Substring(glslEnd, 6) == "#endif")
                {
                    ifMatch.Pop();
                    if (ifMatch.Count == 0)
                    {
                        break;
                    }
                    glslEnd += 6;
                }
                else
                {
                    glslEnd++;
                }
            }
            string glsl = variantCode.Substring(0, glslEnd);
            return glsl.Replace("#version 300 es", "#version 320 es");
        }

        private string[] GetKeywords(string variantCode)
        {
            List<string> keywords = new List<string>();
            string[] lines = variantCode.Split('\n');
            string[] globalKeywords = { "<none>" };
            string[] localKeywords = { "<none>" };

            for (int i = 0; i < lines.Length; i++)
            {
                string pattern = "Global Keywords: ";
                int begin = lines[i].IndexOf(pattern) + pattern.Length;
                if (begin > pattern.Length - 1)
                {
                    globalKeywords = lines[i].Substring(begin).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    pattern = "Local Keywords: ";
                    int localKeywordsBegin = lines[i + 1].IndexOf(pattern) + pattern.Length;
                    localKeywords = lines[i + 1].Substring(localKeywordsBegin).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    break;
                }
            }

            Action<string[]> SetKeywords = (string[] keywordsTemp) =>
            {
                if (keywordsTemp[0] != "<none>")
                {
                    for (int i = 0; i < keywordsTemp.Length; i++)
                    {
                        keywords.Add(keywordsTemp[i]);
                    }
                }
            };
            SetKeywords(globalKeywords);
            SetKeywords(localKeywords);
            if (keywords.Count == 0)
            {
                keywords.Add("none");
            }
            return keywords.ToArray();
        }

        private void GetTagBlock(in string code, string tag, ref int begin, ref int end)
        {
            string pattern = tag + "(\n| )*\\{";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(code);
            if(match.Success)
            {
                begin = match.Index + match.Value.Length - 1;
                Stack<char> bracketMatch = new Stack<char>();
                for (int i = begin; i < code.Length; i++)
                {
                    if (code[i] == '{')
                    {
                        bracketMatch.Push(code[i]);
                    }
                    else if (code[i] == '}')
                    {
                        bracketMatch.Pop();
                        if (bracketMatch.Count == 0)
                        {
                            end = i;
                            break;
                        }
                    }
                }
            }
        }
    }

    [Serializable]
    public class Pass
    { 
        private Dictionary<string, AnalyzeResult[]> _variantAnalyzeResult = new Dictionary<string, AnalyzeResult[]>();

        public void AddVariant(string[] keywords, AnalyzeResult[] variantAnalyzeResult)
        {
            _variantAnalyzeResult.Add(stringArrayToString(keywords), variantAnalyzeResult);
        }
        public AnalyzeResult[] GetAnalyzeResult(string[] keywords)
        {
            string key = stringArrayToString(keywords);
            if (_variantAnalyzeResult.ContainsKey(key))
            {
                return _variantAnalyzeResult[key];
            }
            else return null; 
        }

        public string stringArrayToString(string[] stringArray)
        {
            string result = "";
            Array.Sort(stringArray);

            for (int i = 0; i < stringArray.Length; i++)
            {
                result += stringArray[i]+'#';
            }
            return result;
        }
    }

    [Serializable]
    public class VariantInfo
    {
        public float m_totalCycleCount = 0;

        public float m_totalVertexCycleCount = 0;
        public float m_longestVertexCycleCount = 0;
        public float m_shortestVertexCycleCount = 0;

        public float m_totalFragmentCycleCount = 0;
        public float m_longestFragmentCycleCount = 0;
        public float m_shortestFragmentCycleCount = 0;

        private string _shaderlabName;
        private string _passName;
        private string[] _keywords;

        public VariantInfo(string shaderlabName, string passName, string[] keywords)
        {
            _shaderlabName = shaderlabName;
            _passName = passName;
            _keywords = keywords;
        }

        public string[] GetQueryConditions()
        {
            string keywords = "";
            for (int i = 0; i < _keywords.Length; i++)
            {
                keywords += _keywords[i] + " ";
            }
            return new string[] { _shaderlabName, _passName, keywords };
        }

        public string GetInfo(ShaderType shaderType, CyclePathType cyclePathType)
        {
            string result = "shaderlabName : " + _shaderlabName + "; passName : " + _passName + "; keywords is : ";
            for(int i = 0; i < _keywords.Length; i++)
            {
                result += _keywords[i] + " ";
            }
            if(shaderType == ShaderType.fragment)
            {
                switch(cyclePathType)
                {
                    case CyclePathType.total:
                        result += " totalFragmentCycleCount is : " + m_totalFragmentCycleCount;
                        break;
                    case CyclePathType.longest:
                        result += " longestFragmentCycleCount is : " + m_longestFragmentCycleCount;
                        break;
                    case CyclePathType.shortest:
                        result += " shortestFragmentCycleCount is : " + m_shortestFragmentCycleCount;
                        break;
                    default:
                        result += " totalFragmentCycleCount is : " + m_totalFragmentCycleCount;
                        break;
                }
            }
            else
            {
                switch (cyclePathType)
                {
                    case CyclePathType.total:
                        result += " totalVertexCycleCount is : " + m_totalVertexCycleCount;
                        break;
                    case CyclePathType.longest:
                        result += " longestVertexCycleCount is : " + m_longestVertexCycleCount;
                        break;
                    case CyclePathType.shortest:
                        result += " shortestVertexCycleCount is : " + m_shortestVertexCycleCount;
                        break;
                    default:
                        result += " totalVertexCycleCount is : " + m_totalVertexCycleCount;
                        break;
                }
            }
            return result;
        }
    }

    [Serializable]
    public class ShaderlabInfo
    {
        public string m_name;
        public VariantInfo m_topVariantInfos;
        public Dictionary<string, List<VariantInfo>> _passVariantsMap = new Dictionary<string, List<VariantInfo>>();
        
        public ShaderlabInfo(VariantInfo variantInfo)
        {
            string[] variantInfoQueryConditions = variantInfo.GetQueryConditions();
            m_name = variantInfoQueryConditions[0];
            List<VariantInfo> variantInfos = new List<VariantInfo>();
            variantInfos.Add(variantInfo);
            _passVariantsMap.Add(variantInfoQueryConditions[1], variantInfos);
            m_topVariantInfos = variantInfo;
        }
        public void AddVariantInfo(VariantInfo variantInfo)
        {
            string[] variantInfoQueryConditions = variantInfo.GetQueryConditions();
            string passName = variantInfoQueryConditions[1];
            if (_passVariantsMap.ContainsKey(passName))
            {
                _passVariantsMap[passName].Add(variantInfo);
            }
            else
            {
                List<VariantInfo> variantInfos = new List<VariantInfo>();
                variantInfos.Add(variantInfo);
                _passVariantsMap.Add(passName, variantInfos);
            }
        }
    }
}

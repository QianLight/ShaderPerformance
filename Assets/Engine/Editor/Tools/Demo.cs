using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

#region all data class
public class SCData
{
    public float[] data = new float[4];
    //Commond = 0,
    //Texture = 1,
    //Register = 2,
    //Branch = 3,
    public void SetAllValue(float value)
    {
        for (int i = 0; i < data.Length; i++)
            data[i] = value;
    }

    public void Add(SCData o)
    {
        for (int i = 0; i < data.Length; i++)
            data[i] += o.data[i];
    }
}

public class SCVariantData
{
    public string variant;
    public SCData VertData = null;
    public SCData FragData = null;
}

public class SCValue
{
    public bool IsEmpty = true;
    public SCData[] Value = new SCData[3];  //0:value or avg, 1:min, 2:max
    public SCValue()
    {
        for (int i = 0; i < 3; i++)
            Value[i] = new SCData();
    }

    public void Clear()
    {
        IsEmpty = true;
        Value[0].SetAllValue(0f);
        Value[1].SetAllValue(1000f);  //init min
        Value[2].SetAllValue(0f);  //init max
    }

    public void Total(SCData data)
    {
        IsEmpty = false;
        Value[0].Add(data);
        for(int i = 0; i < 4; i++)
        {
            if(data.data[i] > 0.1f)  //not null
            {
                if (data.data[i] < Value[1].data[i])  //find min
                    Value[1].data[i] = data.data[i];
                if (data.data[i] > Value[2].data[i])  //find max
                    Value[2].data[i] = data.data[i];
            }
        }
    }
}

public class SCPassData
{
    public List<SCVariantData> Data = new List<SCVariantData>();
    public SCValue VertValue = null;
    public SCValue FragValue = null;
}

public class SCSubShaderData
{
    public List<SCPassData> PassList = new List<SCPassData>();
    public SCValue VertValue = new SCValue();
    public SCValue FragValue = new SCValue();
}

public class ShaderCommandData
{
    public ShaderCommandData(Shader s)
    {
        shader = s;
    }
    public bool show = false;
    public Shader shader = null;
    public List<SCSubShaderData> SubList = new List<SCSubShaderData>();
    public SCValue VertValue = new SCValue();
    public SCValue FragValue = new SCValue();
}
#endregion

public class ShaderCommansWindows : EditorWindow
{
    public static readonly string EXEPATH = @"/Engine/Editor/Tools/ShaderCommands.exe";
    public static GUIStyle style0;
    public static GUIStyle style1;
    public static GUIStyle style2;
    public static GUIStyle style3;
    public static GUIStyle style4;
    public static GUIStyle style5;
    public Vector2 pos = new Vector2(0, 0);
    public int[] Vlimit = new int[4];
    public int[] Flimit = new int[4];
    public static Shader selectShader = null;

    public ShaderCommansWindows()
    {
        titleContent = new GUIContent("Shader指令数");
    }

    private void OnGUI()
    {
        style0 = new GUIStyle("label");
        style1 = new GUIStyle("label");
        style2 = new GUIStyle("label");
        style3 = new GUIStyle("label");
        style4 = new GUIStyle("label");
        style5 = new GUIStyle("label");
        style0.fontSize = 18;
        style0.normal.textColor = Color.red;
        style1.normal.textColor = Color.black;
        style2.normal.textColor = Color.blue;
        style3.normal.textColor = new Color32(153, 255, 51, 255);
        style4.normal.textColor = Color.red;
        GUILayout.BeginVertical();
        GUILayout.Label("开始前，请务必确保你的.shader文件默认打开方式为记事本，否则有可能导致机器卡死。", style0);
        GUILayout.Label("1.读取数据之后显示的每个shader的顶点和片元信息，显示的数据有两种情况，一种是【值】，另外一种是【平均值(最小值-最大值)】");
        GUILayout.Label("2.黑色的Empty代表该shader该模块没有指令，或该shader在对应变体该模块没有编译结果(原因未明)");
        GUILayout.Label("3.每个shader后面跟的三个按钮分别是：展开/隐藏详细信息、打开详细信息文件、打开编译后的源文件");
        GUILayout.Label("4.展开后的详细信息里面，绿色的Pass后面跟的代表当前Pass的综合信息（可能有平均值），往下每两行代表Pass里面的一个变体信息");
        GUILayout.Label("5.Variant信息如果显示为No代表没有变体（只有一种情况）或变体不包含任何编译指令");
        GUILayout.Label("6.前面8个框为限制数值，当填入非0数字时，后续超过该值的值都会标记为红色");
        GUILayout.BeginHorizontal();
        selectShader = EditorGUILayout.ObjectField(selectShader, typeof(Shader), false, GUILayout.Height(18), GUILayout.Width(250)) as Shader;
        if (GUILayout.Button("单个数据读取", GUILayout.Width(100), GUILayout.Height(30)))
        {
            if (selectShader == null)
                UnityEngine.Debug.LogError("请拖拽shader至左边引用处");
            else
                BuildData(false);
        }
        if (GUILayout.Button("全部数据读取", GUILayout.Width(100), GUILayout.Height(30)))
        {
            BuildData(true);
        }
        if (GUILayout.Button("Clear", GUILayout.Width(100), GUILayout.Height(30)))
        {
            DataList.Clear();
        }
        GUILayout.EndHorizontal();
        if(DataList.Count != 0)
        {
            //limit
            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(210));
            GUILayout.Label("Vert Limit:", GUILayout.Width(70));
            for (int i = 0; i < 4; i++)
                Vlimit[i] = EditorGUILayout.IntField(Vlimit[i], GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(210));
            GUILayout.Label("Frag Limit:", GUILayout.Width(70));
            for (int i = 0; i < 4; i++)
                Flimit[i] = EditorGUILayout.IntField(Flimit[i], GUILayout.Width(100));
            GUILayout.EndHorizontal();

            //title
            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(284));
            GUILayout.Label("Command", GUILayout.Width(100));
            GUILayout.Label("Texture", GUILayout.Width(100));
            GUILayout.Label("Temp Register", GUILayout.Width(100));
            GUILayout.Label("Branch", GUILayout.Width(100));
            GUILayout.Label("Variant", GUILayout.Width(100));
            GUILayout.EndHorizontal();
        }
        pos = GUILayout.BeginScrollView(pos);
        GUILayout.BeginVertical();
        for(int i = 0; i < DataList.Count; i++)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(DataList[i].shader, typeof(Shader), false,  GUILayout.Height(18), GUILayout.Width(250));
            //Vert
            GUILayout.Label("Vert:", GUILayout.Width(30));
            if (!DataList[i].VertValue.IsEmpty)
            {
                DrawValueLine(DataList[i].VertValue, Vlimit);
            }
            else
            {
                GUILayout.Label("Empty", style1, GUILayout.Width(100));
                GUILayout.Label("", GUILayout.Width(300));
            }
            //Show Btn
            if (GUILayout.Button(DataList[i].show ? "Hide" : "Show", GUILayout.Width(50), GUILayout.Height(20)))
            {
                DataList[i].show = !DataList[i].show;
            }
            //Open Command Msg File
            if (GUILayout.Button("ComMsg", GUILayout.Width(70), GUILayout.Height(20)))
            {
                string fileName = Application.dataPath.Replace("Assets", "Temp/") + "ShaderCommandsMsg-" + DataList[i].shader.name.Replace("/", "-") + ".txt";
                Process.Start("NotePad.exe", fileName);
            }
            //Open Compile Msg File
            if (GUILayout.Button("Source", GUILayout.Width(60), GUILayout.Height(20)))
            {
                string fileName = Application.dataPath.Replace("Assets", "Temp/") + "Compiled-" + DataList[i].shader.name.Replace("/", "-") + ".shader";
                Process.Start("NotePad.exe", fileName);
            }
            GUILayout.EndHorizontal();
            //Frag
            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(250));
            GUILayout.Label("Frag:", GUILayout.Width(30));
            if (!DataList[i].FragValue.IsEmpty)
            {
                DrawValueLine(DataList[i].FragValue, Flimit);
            }
            else
            {
                GUILayout.Label("Empty", style1, GUILayout.Width(100));
                GUILayout.Label("", GUILayout.Width(300));
            }

            GUILayout.EndHorizontal();
            if (DataList[i].show)
                DrawDetail(DataList[i]);
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    public void DrawDetail(ShaderCommandData data)
    {
        for (int l = 0; l < data.SubList.Count; l++)
        {
            SCSubShaderData subData = data.SubList[l];
            for (int j = 0; j < subData.PassList.Count; j++)
            {
                SCPassData passData = subData.PassList[j];
                for (int k = 0; k < passData.Data.Count; k++)
                {
                    //pass msg
                    if(k == 0)
                    {
                        //pass vert
                        GUILayout.BeginHorizontal();
                        if(j == 0)
                        {
                            GUILayout.Label("", GUILayout.Width(132));
                            GUILayout.Label("SubShader", style2, GUILayout.Width(70));
                            GUILayout.Label("Pass", style3, GUILayout.Width(40));
                            GUILayout.Label("Vert:", style3, GUILayout.Width(30));
                        }
                        else
                        {
                            GUILayout.Label("", GUILayout.Width(206));
                            GUILayout.Label("Pass", style3, GUILayout.Width(40));
                            GUILayout.Label("Vert:", style3, GUILayout.Width(30));
                        }
                        DrawValueLine(passData.VertValue, Vlimit);
                        GUILayout.EndHorizontal();
                        //pass frag
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("", GUILayout.Width(250));
                        GUILayout.Label("Frag:", style3, GUILayout.Width(30));
                        DrawValueLine(passData.FragValue, Flimit);
                        GUILayout.EndHorizontal();
                    }

                    //vert
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(250));
                    GUILayout.Label("Vert:", GUILayout.Width(30));
                    DrawDataLine(passData.Data[k].VertData, Vlimit);
                    GUILayout.Label(passData.Data[k].variant, GUILayout.Width(600));
                    GUILayout.EndHorizontal();

                    //frag
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(250));
                    GUILayout.Label("Frag:", GUILayout.Width(30));
                    DrawDataLine(passData.Data[k].FragData, Flimit);
                    GUILayout.EndHorizontal();
                }
            }
        }
    }

    public void DrawValueLine(SCValue value, int[] limit)
    {
        if(value == null)
        {
            GUILayout.Label("Empty", style1, GUILayout.Width(100));
            GUILayout.Label("", GUILayout.Width(300));
            return;
        }
        for (int j = 0; j < 4; j++)
        {
            if (value.Value[0].data[j] > 0.1f || j == 0)
            {
                if (value.Value[1].data[j] + 0.01f > value.Value[2].data[j])
                {
                    GUIStyle style = limit[j] != 0 && value.Value[0].data[j] >= limit[j] ? style4 : style5;
                    GUILayout.Label(value.Value[0].data[j].ToString("F0"), style, GUILayout.Width(100));
                }
                else
                {
                    GUIStyle style = limit[j] != 0 && value.Value[2].data[j] >= limit[j] ? style4 : style5;  //check max
                    GUILayout.Label(value.Value[0].data[j].ToString("F2") +
                        "(" + value.Value[1].data[j].ToString("F0") + "-" + value.Value[2].data[j].ToString("F0") + ")"
                        , style, GUILayout.Width(100));
                }
            }
            else
                GUILayout.Label("", GUILayout.Width(100));
        }
    }

    public void DrawDataLine(SCData data, int[] limit)
    {
        if (data == null)
        {
            GUILayout.Label("Empty", style1, GUILayout.Width(100));
            GUILayout.Label("", GUILayout.Width(300));
            return;
        }
        for (int j = 0; j < 4; j++)
        {
            if (data.data[j] > 0.1f || j == 0)
            {
                GUIStyle style = limit[j] != 0 && data.data[j] >= limit[j] ? style4 : style5;
                GUILayout.Label(data.data[j].ToString("F0"), style, GUILayout.Width(100));
            }
            else
                GUILayout.Label("", GUILayout.Width(100));
        }
    }

    public static List<string> shaderlist = new List<string>();
    public static List<ShaderCommandData> DataList = new List<ShaderCommandData>();
    public static Dictionary<string, Shader> dict = new Dictionary<string, Shader>();

    //[MenuItem("Test/Shader Command Check")]
    static void ShowWindows()
    {
        DataList.Clear();
        EditorWindow.GetWindow(typeof(ShaderCommansWindows));
    }
    public static void BuildData(bool all)
    {
        #region compile shader to commands
        shaderlist.Clear();
        string TempFilePath = Application.dataPath.Replace("Assets", "Temp/");
        string EndFilePath = TempFilePath + "End.txt";
        if (File.Exists(@EndFilePath))
        {
            File.Delete(EndFilePath);
        }
        UnityEngine.Debug.Log(Application.dataPath.Replace('/', '\\') + EXEPATH);
        Process.Start(Application.dataPath.Replace('/', '\\') + EXEPATH, Application.dataPath);

        var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.ShaderUtil");
        MethodInfo info = type.GetMethod("OpenCompiledShader", BindingFlags.NonPublic | BindingFlags.Static);
        List<object> param = new List<object>();
        param.Add(Shader.Find("CustomDebug/MipCheckerboard"));
        param.Add(0);
        param.Add(0);
        param.Add(false);
        string[] guids;
        if (all)
        {
            guids = AssetDatabase.FindAssets("t:Shader", null);
        }
        else
        {
            guids = new string[1] { AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(selectShader)) };
        }
        dict.Clear();
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.StartsWith("Packages/"))
                continue;
            Shader s = AssetDatabase.LoadAssetAtPath<Shader>(path);
            if(dict.ContainsKey(s.name))
            {
                UnityEngine.Debug.LogError("Find Same Shder:" + s.name);
                continue;
            }
            dict.Add(s.name, s);
            shaderlist.Add(s.name);
            param[0] = s;
            //UnityEngine.Debug.Log("Compile:" + s.name);
            info.Invoke(null, param.ToArray());
        }
        FileStream file = File.Create(@EndFilePath);
        file.Close();
        #endregion

        DataList.Clear();
        foreach (string sName in shaderlist)
        {
            string fileName = "Compiled-" + sName.Replace("/", "-") + ".shader";
            string line;
            StreamReader sr = new StreamReader(TempFilePath + fileName, System.Text.Encoding.Default);
            bool compileError = false;
            while ((line = sr.ReadLine()) != null)
            {
                if(line.Contains("Compile errors"))
                {
                    compileError = true;
                    break;
                }
            }
            sr.Close();
            if(compileError)
            {
                UnityEngine.Debug.LogError("Compile error: " + sName);
                continue;
            }

            ShaderCommandData commandData = new ShaderCommandData(dict[sName]);
            DataList.Add(commandData);
            SCSubShaderData subshaderData = null;
            SCPassData passData = null;
            SCVariantData variantData = null;
            string output = "";
            sr = new StreamReader(TempFilePath + fileName, System.Text.Encoding.Default);
            bool tier = false;
            while ((line = sr.ReadLine()) != null)
            {
                #region Pass Head, new SCPassData();
                if (line.Contains("Stats for Vertex shader:"))  //如果遇到了Pass的总和vert信息头
                {
                    output += "\t" + line + "\r\n";
                    line = sr.ReadLine();
                    output += "\t" + line + "\r\n";
                    passData = new SCPassData();
                    subshaderData.PassList.Add(passData);
                    passData.VertValue = new SCValue();
                    ReadMsg(line, ref passData.VertValue);
                    line = sr.ReadLine();
                    if (line.Contains("Stats for Fragment shader:"))  //下一行大概率是Frag信息
                    {
                        output += "\t" + line + "\r\n";
                        line = sr.ReadLine();
                        output += "\t" + line + "\r\n";
                        passData.FragValue = new SCValue();
                        ReadMsg(line, ref passData.FragValue);
                        line = sr.ReadLine();  //读取掉Pass
                        if (!line.Contains("Pass"))
                        {
                            UnityEngine.Debug.LogError("pass Fragment content next Error1.");
                        }
                        else
                        {
                            output += "\t" + "Pass" + "\r\n";
                        }
                    }
                    else if(!line.Contains("Pass"))  //如果不包含frag，那么应该直接就是一行Pass
                    {
                        UnityEngine.Debug.LogError("pass vertext content next Error.");
                    }
                    else
                    {
                        output += "\t" + "Pass" + "\r\n";
                    }
                    continue;
                }
                if (line.Contains("Stats for Fragment shader:"))  //如果没有vert直接就是frag信息头
                {
                    output += "\t" + line + "\r\n";
                    line = sr.ReadLine();
                    output += "\t" + line + "\r\n";
                    passData = new SCPassData();
                    subshaderData.PassList.Add(passData);
                    passData.FragValue = new SCValue();
                    ReadMsg(line, ref passData.FragValue);
                    line = sr.ReadLine();
                    if (!line.Contains("Pass"))  //下一行必定是Pass
                    {
                        UnityEngine.Debug.LogError("pass Fragment content next Error2.");
                    }
                    else
                    {
                        output += "\t" + "Pass" + "\r\n";
                    }
                    continue;
                }
                if (line.Contains("Pass {"))  //有可能pass里面不包含任何vert和frag函数
                {
                    //passData = new SCPassData();
                    if(subshaderData == null)
                    {
                        UnityEngine.Debug.LogError("Find Pass No SubShader:" + commandData.shader.name);
                    }
                    //subshaderData.PassList.Add(passData);
                    //UnityEngine.Debug.LogWarning("Error find Pass:" + commandData.shader.name);
                    //output += "\t" + "Pass" + "\r\n";
                    continue;
                }
                #endregion

                #region SubShader
                if (line.Contains("SubShader"))
                {
                    subshaderData = new SCSubShaderData();
                    commandData.SubList.Add(subshaderData);
                    output += "SubShader\r\n";
                    continue;
                }
                #endregion

                #region all variant
                if (line.Contains("eywords set in this variant"))
                {
                    variantData = new SCVariantData();
                    if(passData == null)
                    {
                        UnityEngine.Debug.LogError("PassData null:" + commandData.shader.name);
                        continue;
                    }
                    passData.Data.Add(variantData);
                    if (line.Contains("No"))
                    {
                        output += "\t\t" + line.Replace("keywords set in this ", "") + "\r\n";
                        variantData.variant = "No";
                    }
                    else
                    {
                        output += "\t\t" + line.Replace("Keywords set in this ", "") + "\r\n";
                        variantData.variant = line.Replace("Keywords set in this variant: ", "");
                    }
                    line = sr.ReadLine();
                    if (line.Contains("Hardware tier variant"))  //大概率接Tier信息
                    {
                        if (line != "-- Hardware tier variant: Tier 1")  //目前发现都是这句
                            UnityEngine.Debug.LogError("Tier Error!");
                        //output += "\t\t" + line + "\r\n";
                        tier = true;
                        line = sr.ReadLine();
                    }
                    else
                        tier = false;
                    if(line.Contains("Vertex shader for"))
                    {
                        output += "\t\t" + line;
                        if (passData.VertValue == null)  //空的情况
                        {
                            output += "\r\n";
                            tier = false;
                            continue;
                        }
                        line = sr.ReadLine();
                        if (line.Contains("No shader variant for this keyword set."))  //不存在该变体对应的接口情况
                        {
                            if(tier)
                            {
                                UnityEngine.Debug.LogError("Error find Tier msg.");
                            }
                            output += "No this variant\r\n";
                        }
                        else if(line.Contains("Stats"))//正常情况
                        {
                            output += line.Substring(line.IndexOf(':') + 1) + "\r\n";
                            variantData.VertData = new SCData();
                            ReadMsg2(line, ref variantData.VertData);
                        }
                        else //第四种情况
                        {
                            UnityEngine.Debug.LogError("Error find Other.");
                        }
                    }
                    tier = false;
                    continue;
                }
                if (line.Contains("Hardware tier variant"))  //Tier信息
                {
                    if (line != "-- Hardware tier variant: Tier 1")  //目前发现都是这句
                        UnityEngine.Debug.LogError("Tier Error!");
                    //output += "\t\t" + line + "\r\n";
                    tier = true;
                    continue;
                }
                if (line.Contains("Fragment shader for"))
                {
                    output += "\t\t" + line;
                    if (passData.FragValue == null)  //空的情况
                    {
                        output += "\r\n";
                        tier = false;
                        variantData = null;  //该变体已经结束，开始下一个
                        continue;
                    }
                    line = sr.ReadLine();
                    if (line.Contains("No shader variant for this keyword set."))  //不存在该变体对应的接口情况
                    {
                        if (tier)
                        {
                            UnityEngine.Debug.LogError("Error find Tier msg.");
                        }
                        output += "No this variant\r\n";
                    }
                    else if (line.Contains("Stats"))//正常情况
                    {
                        output += line.Substring(line.IndexOf(':') + 1) + "\r\n";
                        variantData.FragData = new SCData();
                        ReadMsg2(line, ref variantData.FragData);
                    }
                    else if(line.Contains("Shader Disassembly"))  //该Pass虽然有FragValue头，但是该变体的Frag依然为空的情况
                    {
                        output += "\r\n";
                        tier = false;
                        variantData = null;  //该变体已经结束，开始下一个
                        continue;
                    }
                    else  //第五种情况
                    {
                        UnityEngine.Debug.LogError("Error find Other:" + line + " by " + sName);
                    }
                    variantData = null;
                    continue;
                }
                #endregion
            }

            File.WriteAllText(@TempFilePath + "ShaderCommandsMsg-" + sName.Replace("/", "-") + ".txt", output);
            sr.Close();
        }

        #region dealwith data
        int shaderVCount;
        int shaderFCount;
        int subVCount;
        int subFCount;
        for(int i = 0; i < DataList.Count; i++)
        {
            ShaderCommandData shaderData = DataList[i];
            shaderData.VertValue.Clear();
            shaderData.FragValue.Clear();
            shaderVCount = 0;
            shaderFCount = 0;
            for (int j = 0; j < shaderData.SubList.Count; j++)
            {
                SCSubShaderData subData = shaderData.SubList[j];
                subData.VertValue.Clear();
                subData.FragValue.Clear();
                subVCount = 0;
                subFCount = 0;
                for(int k = 0; k < subData.PassList.Count; k++)
                {
                    SCPassData passData = subData.PassList[k];  //系统本身已统计
                    for(int l = 0; l < passData.Data.Count; l++)
                    {
                        SCVariantData vaData = passData.Data[l];
                        if (vaData != null && vaData.VertData != null)
                        {
                            shaderVCount++;
                            shaderData.VertValue.Total(vaData.VertData);
                            subVCount++;
                            subData.VertValue.Total(vaData.VertData);
                        }
                        if (vaData != null && vaData.FragData != null)
                        {
                            shaderFCount++;
                            shaderData.FragValue.Total(vaData.FragData);
                            subFCount++;
                            subData.FragValue.Total(vaData.FragData);
                        }
                    }
                }
                for(int k = 0; k < 4; k++)
                {
                    subData.VertValue.Value[0].data[k] /= subVCount;
                    subData.FragValue.Value[0].data[k] /= subFCount;
                }
            }
            for (int k = 0; k < 4; k++)
            {
                shaderData.VertValue.Value[0].data[k] /= shaderVCount;
                shaderData.FragValue.Value[0].data[k] /= shaderFCount;
            }
        }
        #endregion
        UnityEngine.Debug.Log("Finish.");
    }

    #region Regular Expression
    public static void ReadMsg(string content, ref SCValue value)
    {
        string content2 = content.Substring(content.IndexOf(':') + 1);
        if (content2.IndexOf(':') != -1)
            UnityEngine.Debug.LogError("Error find ':'");
        string[] strList = content2.Split(',');
        foreach (string s in strList)
        {
            if (s.Contains("avg"))
            {
                Match m = Regex.Match(s, @" (\d+) avg (\S+) (\S)(\d+)..(\d+)(\S)");
                if (m.Success)
                {
                    value.Value[0].data[GetType(m.Groups[2].Value)] = int.Parse(m.Groups[1].Value);
                    value.Value[1].data[GetType(m.Groups[2].Value)] = int.Parse(m.Groups[4].Value);
                    value.Value[2].data[GetType(m.Groups[2].Value)] = int.Parse(m.Groups[5].Value);
                }
                else
                {
                    UnityEngine.Debug.LogError("Error match avg");
                }
            }
            else
            {
                Match m = Regex.Match(s, @" (\d+) (\D+)");
                if (m.Success)
                {
                    value.Value[0].data[GetType(m.Groups[2].Value)] = int.Parse(m.Groups[1].Value);
                }
                else
                {
                    UnityEngine.Debug.LogError("Error match value");
                }
            }
        }
    }

    public static void ReadMsg2(string content, ref SCData data)
    {
        string content2 = content.Substring(content.IndexOf(':') + 1);
        if (content2.IndexOf(':') != -1)
            UnityEngine.Debug.LogError("Error find ':'2");
        string[] strList = content2.Split(',');
        foreach (string s in strList)
        {
            Match m = Regex.Match(s, @" (\d+) (\D+)");
            if (m.Success)
            {
                data.data[GetType(m.Groups[2].Value)] = int.Parse(m.Groups[1].Value);
            }
            else
            {
                UnityEngine.Debug.LogError("Error match data");
            }
        }
    }

    public static int GetType(string str)
    {
        switch(str)
        {
            case "math":
                return 0;
            case "texture":
            case "textures":
                return 1;
            case "temp registers":
                return 2;
            case "branch":
            case "branches":
                return 3;
        }
        UnityEngine.Debug.LogError("not find type:" + str);
        return 0;
    }
    #endregion
}
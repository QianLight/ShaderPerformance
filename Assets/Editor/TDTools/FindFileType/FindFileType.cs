using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text;

public class FIndFileType : EditorWindow
{
    public List<string> fileNameList = new List<string>();
    public List<string> filePathNameList = new List<string>(); 
    public List<string> outPutSkillList = new List<string>();
    public List<string> copySkillNameList = new List<string>();
    public string fileName = "";
    public string filePathName;
    public int fileType = 0;
    public string[] fileTypeStr;
    public string fileTypeName = "";
    public List<string> fileTypeList = new List<string>();
    public List<bool> fileTypeSelectList = new List<bool>();
    public List<int> fileSpecialList = new List<int>();
    public Vector2 listScrollViewPos;
    public int num1 = 0;
    public static string TargetFile;
    public static string TestFile;
    public List<bool> TargetFileList = new List<bool>();
    public string searchSignal = "*";

    // 将窗口添加到Tools菜单
    [MenuItem("Tools/TDTools/通用工具/FindFileType")]
    static void Init()
    {
        //显示窗口
        FIndFileType FindFileType = (FIndFileType)EditorWindow.GetWindow(typeof(FIndFileType),false,"FindFileType",true);
        FindFileType.Show();
    }

    private void OnEnable()
    {
        TargetFile = $"{Application.dataPath}/Editor/TDTools/FindFileType/TargetFile.txt";
        TestFile = $"{Application.dataPath}/Editor/TDTools/FindFileType/TestFile.txt";
        ReadTxt();
    }

    private void OnGUI()
    {

        //filenamePath输入
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("FileNamePath", filePathName);
        if (GUILayout.Button("...", GUILayout.Width(50)))
        {
            filePathName = EditorUtility.OpenFolderPanel("选择目录", filePathName, "");
        }
        EditorGUILayout.EndHorizontal();
        //filetype选择
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("全选"))
        {
            SetSelectAll(true);
        }
        if (GUILayout.Button("全不选"))
        {
            SetSelectAll(false);
        }
        EditorGUILayout.EndHorizontal();
        listScrollViewPos = EditorGUILayout.BeginScrollView(listScrollViewPos, false, true);
        for (int i = 0; i < fileTypeList.Count; ++i)
        {
            EditorGUILayout.BeginHorizontal();
            fileTypeSelectList[i] = EditorGUILayout.ToggleLeft(new GUIContent(fileTypeList[i]), fileTypeSelectList[i]);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        //搜索按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("搜索"))
        {
            FindFiles();
            WriteFiles();
            fileNameList.Clear();
            outPutSkillList.Clear();

            //EditorGUILayout.ToggleLeft(new GUIContent(sb));
        }
    }
    public void FindFiles()
    {
        //查找路径文件
        var list = Directory.GetFiles(filePathName, "*", SearchOption.AllDirectories);
        var result = list.Where(x => IsMatch(x)).ToArray();
        for (int i = 0; i < result.Length; i++)
        {
            filePathNameList.Add(result[i]);//路径名
            fileNameList.Add(Path.GetFileName(result[i]));
            string skillName = Path.GetFileName(result[i]);
            skillName = skillName.Replace(".bytes", "");
            skillName = skillName.Insert(0, "reloadskill ") + ";";
            outPutSkillList.Add(skillName);
            //fileNameList.Add(fileNameList[i].Substring(fileNameList[i].LastIndexOf(@"\")));//文件名
        }
    }
    public void SetSelectAll(bool value)
    {
        for (int i = 0; i < fileTypeSelectList.Count; ++i)
        {
            fileTypeSelectList[i] = value;
        }
    }
    public bool IsMatch(string str)
    {
        for(int i = 0; i < fileTypeList.Count; ++i)
        {
            if (fileTypeSelectList[i] && str.EndsWith(fileTypeList[i]))
                return true;
        }
        return false;
    }
    //读取后缀文件
    public void ReadTxt()
    {
        fileTypeList.Clear();
        fileTypeSelectList.Clear();
        string[] lines = File.ReadAllLines($"{Application.dataPath}/Editor/TDTools/FindFileType/fileExtension.txt");
        for(int i = 0; i < lines.Length; i++)
        {
            fileTypeList.Add(lines[i]);
            fileTypeSelectList.Add(true);
        }
    }
    public void WriteFiles()
    {
        StringBuilder sb = new StringBuilder();
        StringBuilder sb2 = new StringBuilder();
        sb.Clear();
        sb2.Clear();
        for (int i = 0; i< fileNameList.Count; i++)
        {
            sb.AppendLine(fileNameList[i]);
        }
        using (var file = File.Open(TargetFile, FileMode.Create))
        {
            var info = Encoding.UTF8.GetBytes(sb.ToString());
            file.Write(info, 0, info.Length);
        }
        //for (int i = 0; i < outPutSkillList.Count; i++)
        //{
        //    sb2.AppendLine(outPutSkillList[i]);
        //}
        //using (var file = File.Open(TestFile, FileMode.Create))
        //{
        //    var info = Encoding.UTF8.GetBytes(sb2.ToString());
        //    file.Write(info, 0, info.Length);
        //}
    }
    public void copySkillName(ref string s)
    {
        List<string> list = new List<string>();
        for (int i = 0; i < outPutSkillList.Count; i++)
        {
            list.Add(outPutSkillList[i]);
        }
        s = string.Join("", list.ToArray());
    }
}

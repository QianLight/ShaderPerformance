using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using CFClient;
using CFEngine;
using CFUtilPoolLib;
using CFEngine.Editor;
using XEditor;
using EcsData;
using AnimationUtility = UnityEditor.AnimationUtility;

public class AutoGenerateBehit : EditorWindow
{
    private Vector2 behitListScrollViewPos;
    internal static CommonAssets.AssetLoadCallback<GameObject, ModelImporter> srcFbx = new CommonAssets.AssetLoadCallback<GameObject, ModelImporter>("*.fbx");
    private List<string> fbxFileName = new List<string>();
    private List<string> fbxFolderName = new List<string>();
    private List<string> chargeFileName = new List<string>();
    private List<string> chargeFolderName = new List<string>();
    private List<string> realFolderNames = new List<string>();
    private List<string> animName = new List<string>();
    private List<string> desFilePath = new List<string>();
    private List<string> desFolderPath = new List<string>();
    private List<bool> fileSelectList = new List<bool>();
    private List<bool> folderSelectList = new List<bool>();
    private List<ObjectInfo> m_Objects = new List<ObjectInfo>();
    //
    private int presentID;
    private string audioTemplate = "";
    private string animTemplate = "";
    //
    private string svnPath = "";
    private string svnFilePath = "";
    private string projPath = "";
    private string copyPath = "";
    private string rootFolderName = "";
    private string inputFolderName = "";
    private string folderPathAnim = "";
    private string prefabName = "";
    private bool isFBX;
    private bool isCurve;
    private bool isBatchFBX;
    private bool isBatchCurve;
    private bool isBehitGraphExporter;
    private bool isShowBatchFolderName;
    private bool isShowFolderName;
    private bool isShowFileList;
    private bool _forward = true, _side = false, _up = false, _rotate = false;

    GUIStyle style = new GUIStyle();
    private enum XCurveType
    {
        Forward,
        Side,
        Up,
        Rotation
    }
    private string[] splittedFBXTypes =
    {
        "damage_fly_1",
        "damage_fly_flinch_1",
        "damage_flyaway_1",
        "damage_flyaway_2",
        "damage_subside",
        "resist_dizzy_loop",
        "resist_charm_loop",
        "resist_paresis_loop"
    };
    private string[] refSplittedFBX =
    {
        "Assets/Creatures/Role_usopp/Animation/Role_Usopp_damage_fly_1.FBX",
        "Assets/Creatures/Role_usopp/Animation/Role_Usopp_damage_fly_flinch_1.FBX",
        "Assets/Creatures/Role_usopp/Animation/Role_Usopp_damage_flyaway_1.FBX",
        "Assets/Creatures/Role_usopp/Animation/Role_Usopp_damage_flyaway_2.FBX",
        "Assets/Creatures/Role_usopp/Animation/Role_Usopp_damage_subside.FBX",
        "Assets/Creatures/Role_usopp/Animation/Role_Usopp_resist_dizzy_loop.FBX",
        "Assets/Creatures/Role_usopp/Animation/Role_Usopp_resist_charm_loop.FBX",
        "Assets/Creatures/Role_usopp/Animation/Role_Usopp_resist_paresis_loop.FBX"
    };
    private string[] templates = 
    {
        "Monster_MarineSwordman/Monster_MarineSwordman_die.bytes",
        "Monster_MarineSwordman/Monster_MarineSwordman_HitBack.bytes",
        "Monster_MarineSwordman/Monster_MarineSwordman_HitBack_heavy.bytes",
        "Monster_MarineSwordman/Monster_MarineSwordman_HitDraw.bytes",
        "Monster_MarineSwordman/Monster_MarineSwordman_HitFly.bytes",
        "Monster_MarineSwordman/Monster_MarineSwordman_HitFlyaway.bytes",
        "Monster_MarineSwordman/Monster_MarineSwordman_Knockdown.bytes",
        "Role_Usopp/Role_Usopp_die.bytes",
        "Role_Usopp/Role_Usopp_HitBack.bytes",
        "Role_Usopp/Role_Usopp_HitBack_heavy.bytes",
        "Role_Usopp/Role_Usopp_HitDraw.bytes",
        "Role_Usopp/Role_Usopp_HitFly.bytes",
        "Role_Usopp/Role_Usopp_HitFlyaway.bytes",
        "Role_Usopp/Role_Usopp_Knockdown.bytes"
    };

    [MenuItem("Tools/TDTools/关卡相关工具/自动生成受击脚本 %&b")]
    static void Init()
    {
        AutoGenerateBehit window = GetWindow<AutoGenerateBehit>();
        window.titleContent = new GUIContent("受击脚本自动生成");
        window.Show();
    }

    private void OnEnable()
    {
        style.fontStyle = FontStyle.Bold;
        style.normal = new GUIStyleState() { textColor = Color.white };
    }

    public void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("曲线文件"))
        {
            fileSelectList.Clear();
            chargeFileName.Clear();
            desFilePath.Clear();
            ReadPathConfig();
            OpenCurveFileWindow();
            copyPath = "/Assets/Editor/EditorResources/Curve/";
            isFBX = false;
            isBehitGraphExporter = false;
            isBatchCurve = false;
            isBatchFBX = false;
            isCurve = true;
        }
        if (GUILayout.Button("FBX文件"))
        {
            fileSelectList.Clear();
            fbxFileName.Clear();
            desFilePath.Clear();
            ReadPathConfig();
            OpenFBXFileWindow();
            copyPath = "/Assets/Creatures/";
            isCurve = false;
            isBehitGraphExporter = false;
            isBatchCurve = false;
            isBatchFBX = false;
            isFBX = true;
        }
        if (GUILayout.Button ("批量曲线文件"))
        {
            folderSelectList.Clear();
            realFolderNames.Clear();
            chargeFolderName.Clear();
            desFolderPath.Clear();
            ReadPathConfig();
            OpenBatchCurveFileWindow();
            copyPath = "/Assets/Editor/EditorResources/Curve/";
            isFBX = false;
            isBehitGraphExporter = false;
            isCurve = false;
            isBatchFBX = false;
            isBatchCurve = true;
        }
        if (GUILayout.Button ("批量FBX文件"))
        {
            folderSelectList.Clear();
            realFolderNames.Clear();
            fbxFolderName.Clear();
            desFolderPath.Clear();
            ReadPathConfig();
            OpenBatchFBXFileWindow();
            copyPath = "/Assets/Creatures/";
            isCurve = false;
            isBehitGraphExporter = false;
            isFBX = false;
            isBatchCurve = false;
            isBatchFBX = true;
        }
        if (GUILayout.Button("生成受击脚本"))
        {
            fileSelectList.Clear();
            animName.Clear();
            desFilePath.Clear();
            presentID = 0;
            audioTemplate = "";
            animTemplate = "";
            ReadPathConfig();
            OpenAnimFileWindow();
            isFBX = false;
            isCurve = false;
            isBatchCurve = false;
            isBatchFBX = false;
            isBehitGraphExporter = true;
        }
        EditorGUILayout.EndHorizontal();

        if (isShowBatchFolderName)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("批量处理目录：", style, GUILayout.Width(80));
            EditorGUILayout.LabelField(svnFilePath, style);
            EditorGUILayout.EndHorizontal();
        }

        if (isShowFolderName)
        {
            EditorGUILayout.BeginHorizontal();
            inputFolderName = EditorGUILayout.TextField("项目文件夹名(*)", inputFolderName);
            EditorGUILayout.EndHorizontal();
        }

        if (isBehitGraphExporter)
        {
            presentID = EditorGUILayout.IntField("PresentID(*)", presentID);
            audioTemplate = EditorGUILayout.TextField("AudioTemp", audioTemplate);
            animTemplate = EditorGUILayout.TextField("AnimTemp", animTemplate);
            GUILayout.Label("选择受击模板");
        }

        if (isFBX)
        {
            if (isShowFileList)
            {
                behitListScrollViewPos = EditorGUILayout.BeginScrollView(behitListScrollViewPos, false, true);
                for (int i = 0; i < fbxFileName.Count; ++i)
                {
                    EditorGUILayout.BeginHorizontal();
                    fileSelectList[i] = EditorGUILayout.ToggleLeft(fbxFileName[i], fileSelectList[i]);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("全选"))
                {
                    SetSelectAll(true);
                }
                if (GUILayout.Button("全不选"))
                {
                    SetSelectAll(false);
                }
                if (GUILayout.Button("复制文件"))
                {
                    CopyFile(copyPath);
                }
                if (GUILayout.Button("导出动画文件"))
                {
                    ExportAnim();
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        else if (isCurve)
        {
            if (isShowFileList)
            {
                behitListScrollViewPos = EditorGUILayout.BeginScrollView(behitListScrollViewPos, false, true);
                for (int i = 0; i < chargeFileName.Count; ++i)
                {
                    EditorGUILayout.BeginHorizontal();
                    fileSelectList[i] = EditorGUILayout.ToggleLeft(chargeFileName[i], fileSelectList[i]);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.BeginHorizontal();
                _forward = EditorGUILayout.ToggleLeft("Generate Forward", _forward);
                _side = EditorGUILayout.ToggleLeft("Generate Side", _side);
                _up = EditorGUILayout.ToggleLeft("Generate Up", _up);
                _rotate = EditorGUILayout.ToggleLeft("Generate Rotate", _rotate);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("全选"))
                {
                    SetSelectAll(true);
                }
                if (GUILayout.Button("全不选"))
                {
                    SetSelectAll(false);
                }
                if (GUILayout.Button("复制文件"))
                {
                    CopyFile(copyPath);
                }
                if (GUILayout.Button("导出曲线文件"))
                {
                    CurveGenerator(copyPath);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        else if (isBatchFBX)
        {
            if (isShowFileList)
            {
                behitListScrollViewPos = EditorGUILayout.BeginScrollView(behitListScrollViewPos, false, true);
                for (int i = 0; i < fbxFolderName.Count; ++i)
                {
                    EditorGUILayout.BeginHorizontal();
                    folderSelectList[i] = EditorGUILayout.ToggleLeft(fbxFolderName[i], folderSelectList[i], GUILayout.Width(500));
                    realFolderNames[i] = EditorGUILayout.TextField(realFolderNames[i], GUILayout.Width(200));
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("全选"))
                {
                    SetSelectAll(true);
                }
                if (GUILayout.Button("全不选"))
                {
                    SetSelectAll(false);
                }
                if (GUILayout.Button("复制文件夹"))
                {
                    CopyFile(copyPath);
                }
                if (GUILayout.Button("导出动画文件"))
                {
                    BatchExportAnim();
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        else if (isBatchCurve)
        {
            if (isShowFileList)
            {
                behitListScrollViewPos = EditorGUILayout.BeginScrollView(behitListScrollViewPos, false, true);
                for (int i = 0; i < chargeFolderName.Count; ++i)
                {
                    EditorGUILayout.BeginHorizontal();
                    folderSelectList[i] = EditorGUILayout.ToggleLeft(chargeFolderName[i], folderSelectList[i], GUILayout.Width(500));
                    realFolderNames[i] = EditorGUILayout.TextField(realFolderNames[i], GUILayout.Width(200));
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.BeginHorizontal();
                _forward = EditorGUILayout.ToggleLeft("Generate Forward", _forward);
                _side = EditorGUILayout.ToggleLeft("Generate Side", _side);
                _up = EditorGUILayout.ToggleLeft("Generate Up", _up);
                _rotate = EditorGUILayout.ToggleLeft("Generate Rotate", _rotate);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("全选"))
                {
                    SetSelectAll(true);
                }
                if (GUILayout.Button("全不选"))
                {
                    SetSelectAll(false);
                }
                if (GUILayout.Button("复制文件"))
                {
                    CopyFile(copyPath);
                }
                if (GUILayout.Button("导出曲线文件"))
                {
                    BatchCurveGenerator(copyPath);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        else if (isBehitGraphExporter)
        {
            if (isShowFileList)
            {
                behitListScrollViewPos = EditorGUILayout.BeginScrollView(behitListScrollViewPos, false, true);
                for (int i = 0; i < templates.Length; ++i)
                {
                    EditorGUILayout.BeginHorizontal();
                    fileSelectList[i] = EditorGUILayout.ToggleLeft(templates[i], fileSelectList[i]);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("全选"))
                {
                    SetSelectAll(true);
                }
                if (GUILayout.Button("全不选"))
                {
                    SetSelectAll(false);
                }
                if (GUILayout.Button("生成受击脚本"))
                {
                    BehitGraphExporter();
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    private void ReadPathConfig()
    {
        string pathConfig = Application.dataPath;
        pathConfig = pathConfig.Replace("/Assets", "/Assets/Editor/TDTools/AutoGenerateBehit/pathConfig.txt");
        string[] sr = File.ReadAllLines(pathConfig);
        svnPath = sr[0];
        projPath = sr[1];
    }

    private void OpenCurveFileWindow()
    {
        chargeFileName = new List<string>();
        svnPath = svnPath.Replace("svn=", "");
        svnFilePath = EditorUtility.OpenFolderPanel("Open File Dialog", svnPath, "");
        if (Directory.Exists(svnFilePath))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(svnFilePath);
            FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; ++i)
            {
                if (files[i].Name.EndsWith(".txt"))
                {
                    chargeFileName.Add(files[i].Name);
                    fileSelectList.Add(true);
                }
            }
            
        }
        if (chargeFileName[0].Contains("damage"))
            inputFolderName = chargeFileName[0].Substring(0, chargeFileName[0].IndexOf("damage") - 1);
        else
            inputFolderName = chargeFileName[0].Substring(0, chargeFileName[0].LastIndexOf("_"));
        isShowFolderName = true;
        isShowFileList = true;
    }

    private void OpenBatchCurveFileWindow ()
    {
        chargeFolderName = new List<string>();
        svnPath = svnPath.Replace("svn=", "");
        svnFilePath = EditorUtility.OpenFolderPanel("Open File Dialog", svnPath, "");
        if (Directory.Exists (svnFilePath))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(svnFilePath);
            DirectoryInfo[] directories = directoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < directories.Length; ++i)
            {
                chargeFolderName.Add(directories[i].Name);
                folderSelectList.Add(true);
                realFolderNames.Add(directories[i].Name);
            }
        }
        isShowFolderName = false;
        isShowBatchFolderName = true;
        isShowFileList = true;
    }

    private void OpenFBXFileWindow()
    {
        fbxFileName = new List<string>();
        svnPath = svnPath.Replace("svn=", "");
        svnFilePath = EditorUtility.OpenFolderPanel("Open File Dialog", svnPath, "");
        if (Directory.Exists(svnFilePath))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(svnFilePath);
            FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; ++i)
            {
                if (files[i].Name.EndsWith(".FBX"))
                {
                    fbxFileName.Add(files[i].Name);
                    fileSelectList.Add(true);
                }
            }
        }
        if (fbxFileName[0].Contains("damage"))
            inputFolderName = fbxFileName[0].Substring(0, fbxFileName[0].IndexOf("damage") - 1);
        else
            inputFolderName = fbxFileName[0].Substring(0, fbxFileName[0].LastIndexOf("_"));
        isShowFolderName = true;
        isShowFileList = true;
    }

    private void OpenBatchFBXFileWindow()
    {
        fbxFolderName = new List<string>();
        svnPath = svnPath.Replace("svn=", "");
        svnFilePath = EditorUtility.OpenFolderPanel("Open File Dialog", svnPath, "");

        if (Directory.Exists(svnFilePath))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(svnFilePath);
            DirectoryInfo[] directories = directoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < directories.Length; ++i)
            {
                fbxFolderName.Add(directories[i].Name);
                folderSelectList.Add(true);
                realFolderNames.Add(directories[i].Name);
            }
        }
        isShowFolderName = false;
        isShowBatchFolderName = true;
        isShowFileList = true;
    }
    private void OpenAnimFileWindow()
    {
        for (int i = 0; i < templates.Length; ++i)
            fileSelectList.Add(true);
        isShowFolderName = true;
        isShowFileList = true;
    }

    private void SetSelectAll(bool b)
    {
        if (isFBX)
        {
            for (int i = 0; i < fbxFileName.Count; ++i)
                fileSelectList[i] = b;
        }
        else if (isBatchFBX)
        {
            for (int i = 0; i < fbxFolderName.Count; ++i) 
                folderSelectList[i] = b;
        }
        else if (isBatchCurve)
        {
            for (int i = 0; i < chargeFolderName.Count; ++i)
                folderSelectList[i] = b;
        }
        else if (isCurve)
        {
            for (int i = 0; i < chargeFileName.Count; ++i)
                fileSelectList[i] = b;
        }
        else if (isBehitGraphExporter)
        {
            for (int i = 0; i < animName.Count; ++i)
                fileSelectList[i] = b;
        }
    }

    private void CopyFile(string path)
    {
        if (isFBX)
        {
            desFilePath.Clear();
            string folderPath = Application.dataPath.Replace("/Assets", path + inputFolderName);
            folderPathAnim = folderPath + "/Animation";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                Directory.CreateDirectory(folderPathAnim);
            }
            else if (!Directory.Exists(folderPathAnim))
            {
                Directory.CreateDirectory(folderPathAnim);
            }
            if (Directory.Exists(folderPathAnim))
            {
                for (int i = 0; i < fileSelectList.Count; ++i)
                {
                    if (fileSelectList[i])
                    {
                        desFilePath.Add("Assets/Creatures/" + inputFolderName + "/Animation/" + fbxFileName[i]);
                        File.Copy(svnFilePath + "/" + fbxFileName[i], folderPathAnim + "/" + fbxFileName[i], true);
                    }
                    else desFilePath.Add("");
                }
            }
            AssetDatabase.Refresh();
        }
        else if (isCurve)
        {
            desFilePath.Clear();
            string folderPath = Application.dataPath.Replace("/Assets", path + inputFolderName);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                for (int i = 0; i < fileSelectList.Count; ++i)
                {
                    if (fileSelectList[i])
                    {
                        desFilePath.Add(folderPath + "/" + chargeFileName[i]);
                        File.Copy(svnFilePath + "/" + chargeFileName[i], folderPath + "/" + chargeFileName[i], true);
                    }
                    else desFilePath.Add("");
                }
            }
            else
            {
                for (int i = 0; i < fileSelectList.Count; ++i)
                {
                    if (fileSelectList[i])
                    {
                        desFilePath.Add(folderPath + "/" + chargeFileName[i]);
                        File.Copy(svnFilePath + "/" + chargeFileName[i], folderPath + "/" + chargeFileName[i], true);
                    }
                    else desFilePath.Add("");
                }
            }
            AssetDatabase.Refresh();
        }
        else if (isBatchCurve)
        {
            desFolderPath.Clear(); 
            for (int i = 0; i < realFolderNames.Count; ++i)
            {
                if (!String.IsNullOrEmpty(realFolderNames[i]) && folderSelectList[i])
                {
                    string folderPath = Application.dataPath.Replace("/Assets", path + realFolderNames[i]);
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    desFolderPath.Add(folderPath);

                    DirectoryInfo directoryInfo = new DirectoryInfo(svnFilePath + "/" + chargeFolderName[i]);
                    FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly);
                    foreach (var file in files)
                    {
                        if (file.Name.EndsWith(".txt"))
                            File.Copy(file.FullName, folderPath + "/" + file.Name, true);
                    }
                    AssetDatabase.Refresh();
                }
            }
        }
        else if (isBatchFBX)
        {
            desFolderPath.Clear();
            for (int i = 0; i < realFolderNames.Count; ++i)
            {
                if (!String.IsNullOrEmpty(realFolderNames[i]) && folderSelectList[i])
                {
                    string folderPath = Application.dataPath.Replace("/Assets", path + realFolderNames[i]);
                    string folderAnimPath = folderPath + "/Animation";
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                        Directory.CreateDirectory(folderAnimPath); 
                    }
                    else if (!Directory.Exists(folderAnimPath))
                    {
                        Directory.CreateDirectory(folderAnimPath);
                    }
                    desFolderPath.Add(folderAnimPath);

                    DirectoryInfo directoryInfo = new DirectoryInfo(svnFilePath + "/" + fbxFolderName[i]);
                    FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly);
                    foreach (var file in files)
                    {
                        if (file.Name.EndsWith (".FBX"))
                            File.Copy(file.FullName, folderAnimPath + "/" + file.Name, true);
                    }
                    AssetDatabase.Refresh();
                }
            }
        }
    }

    private void ExportAnim()
    {
        m_Objects.Clear();
        for (int i = 0; i < fileSelectList.Count; ++i)
        {
            if (fileSelectList[i])
            {
                for (int j = 0; j < splittedFBXTypes.Length; ++j)
                {
                    if (fbxFileName[i].Contains(splittedFBXTypes[j]))
                    {
                        string monsterName = fbxFileName[i].Substring(0, fbxFileName[i].IndexOf(splittedFBXTypes[j]) - 1);
                        SplitFBX(j, "Assets/Creatures/" + inputFolderName + "/Animation/" + fbxFileName[i], monsterName);
                        break;
                    }
                }
                
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath("Assets/Creatures/" + inputFolderName + "/Animation/" + fbxFileName[i], typeof(UnityEngine.Object));
                string path = AssetDatabase.GetAssetPath(obj);
                ObjectInfo oi = new ObjectInfo();
                oi.obj = obj;
                oi.path = path;
                m_Objects.Add(oi);
            }
        }
        Fbx_ExportAnimFunc(m_Objects);
    }

    private void BatchExportAnim()
    {
        m_Objects.Clear();
        for (int i = 0; i < realFolderNames.Count; ++i)
        {
            if (!String.IsNullOrEmpty(realFolderNames[i]) && folderSelectList[i])
            {
                fbxFileName.Clear();
                fbxFileName = GetFilesInFolder(svnFilePath + "/" + fbxFolderName[i], ".FBX");

                DirectoryInfo directory = new DirectoryInfo(Application.dataPath.Replace("/Assets", copyPath + realFolderNames[i]) + "/Animation");
                foreach (var file in fbxFileName)
                {
                    if (!File.Exists(directory.FullName + "/" + file))
                        continue;
                    for (int j = 0; j < splittedFBXTypes.Length; ++j)
                        if (file.Contains(splittedFBXTypes[j]))
                        {
                            string monsterName = file.Substring(0, file.IndexOf(splittedFBXTypes[j]) - 1);
                            SplitFBX(j, "Assets/Creatures/" + realFolderNames[i] + "/Animation/" + file, monsterName);
                            break;
                        }

                    UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath("Assets/Creatures/" + realFolderNames[i] + "/Animation/" + file, typeof(UnityEngine.Object));
                    string path = AssetDatabase.GetAssetPath(obj);
                    ObjectInfo oi = new ObjectInfo();
                    oi.obj = obj;
                    oi.path = path;
                    m_Objects.Add(oi);
                }
            }
        }
        Fbx_ExportAnimFunc(m_Objects);
    }

    private List<string> GetFilesInFolder(string path, string postfix)
    {
        List<string> tmp = new List<string>();  
        DirectoryInfo directory = new DirectoryInfo(path);
        FileInfo[] files = directory.GetFiles();
        foreach (var file in files)
        {
            if (file.Name.EndsWith(postfix))
            {
                tmp.Add(file.Name);
            }
        }
        return tmp;
    }

    static void Fbx_ExportAnimFunc(List<ObjectInfo> objInfoList)
    {
        srcFbx.cb = (fbx, modelImporter, path, context) =>
        {
            string folderName;
            if (AssetsPath.GetCreatureFolderName(path, out folderName))
            {
                //Debug.Log(path + "   " + folderName);
                if (Directory.Exists(AssetsConfig.instance.ResourceAnimationPath) == false)
                {
                    AssetDatabase.CreateFolder(AssetsConfig.instance.ResourcePath, AssetsConfig.instance.ResourceAnimation);
                }
                if (modelImporter.animationCompression != ModelImporterAnimationCompression.Optimal)
                {
                    modelImporter.animationCompression = ModelImporterAnimationCompression.Optimal;
                    modelImporter.animationPositionError = 0.1f;
                    modelImporter.animationRotationError = 0.2f;
                    modelImporter.animationScaleError = 0.4f;
                    AssetDatabase.ImportAsset(path);
                }

                UnityEngine.Object[] allObjects = AssetDatabase.LoadAllAssetsAtPath(path);

                string targetPath = string.Format("{0}/{1}", AssetsConfig.instance.ResourceAnimationPath, folderName);

                int lastSep = path.LastIndexOf('/');
                int fileExtensionDot = path.LastIndexOf('.');
                string defaultName = path.Substring(lastSep + 1, fileExtensionDot - lastSep - 1);

                if (!Directory.Exists(targetPath))
                {
                    AssetDatabase.CreateFolder(AssetsConfig.instance.ResourceAnimationPath, folderName);
                }
                string editorWrapperPath = string.Format("{0}/{1}",
                    AssetsConfig.instance.ResourceEditorAnimationPath,
                    folderName);
                if (!Directory.Exists(editorWrapperPath))
                {
                    AssetDatabase.CreateFolder(AssetsConfig.instance.ResourceEditorAnimationPath, folderName);
                }

                foreach (UnityEngine.Object o in allObjects)
                {
                    AnimationClip oClip = o as AnimationClip;

                    if (oClip == null || oClip.name.StartsWith("__preview__Take 001")) continue;

                    string clipName = oClip.name;
                    if (clipName == "Take 001") clipName = defaultName;
                    AnimtionWrap animWrap = AnimtionWrap.CreateInstance<AnimtionWrap>();
                    animWrap.clip = oClip;
                    animWrap.name = clipName;
                    CommonAssets.CreateAsset<AnimtionWrap>(editorWrapperPath, clipName, ".asset", animWrap);
                    string copyPath = targetPath + "/" + clipName + ".anim";

                    AnimationClip newClip = new AnimationClip();
                    newClip.name = clipName;

                    EditorUtility.CopySerializedIfDifferent(oClip, newClip);
                    AssetDatabase.CreateAsset(newClip, copyPath);
                    if (newClip.name.Contains("_facial_"))
                    {
                        DeleteKeyFrameExceptFacial(newClip); //删除非表情的关键帧
                    }
                }
            }
            return false;
        };
        CommonAssets.EnumAssetInside<GameObject>(objInfoList, srcFbx, "ExportAnim");
    }

    static void DeleteKeyFrameExceptFacial(AnimationClip animClip)
    {
        if (animClip == null) return;
        EditorCurveBinding[] curveBinding = AnimationUtility.GetCurveBindings(animClip);
        for (int i = 0; i < curveBinding.Length; ++i)
        {
            var binding = curveBinding[i];
            string path = binding.path;
            string[] strs = path.Split('/');
            if (strs.Length > 0 && !strs[strs.Length - 1].StartsWith("Bine_"))
            {
                AnimationUtility.SetEditorCurve(animClip, binding, null);
            }
        }
    }

    private void CurveGenerator(string path)
    {
        string folderPath = Application.dataPath.Replace("/Assets", path + inputFolderName);

        if (desFilePath.Count == 0)
            GenerateDesFilePath(path);
            
        for (int i = 0; i < fileSelectList.Count; ++i)
        {
            if (fileSelectList[i])
            {
                string sr = Path.GetDirectoryName(folderPath + "/" + chargeFileName[i]);
                prefabName = Path.GetFileNameWithoutExtension(sr);
                GenerateCurve(desFilePath[i]);
            }
        }
    }

    private void BatchCurveGenerator (string path)
    {
        for (int i = 0; i < realFolderNames.Count; ++i)
        {
            if (!String.IsNullOrEmpty(realFolderNames[i]) && folderSelectList[i])
            {
                chargeFileName.Clear();
                chargeFileName = GetFilesInFolder(svnFilePath + "/" + chargeFolderName[i], ".txt");

                string folderPath = Application.dataPath.Replace("/Assets", path + realFolderNames[i]);

                DirectoryInfo directory = new DirectoryInfo(folderPath);

                foreach (var file in chargeFileName)
                {
                    if (!File.Exists(directory.FullName + "/" + file))
                        continue;
                    string sr = Path.GetDirectoryName(folderPath + "/" + file);
                    prefabName = Path.GetFileNameWithoutExtension(sr);
                    GenerateCurve (folderPath + "/" + file);
                }
            }
        }
    }

    private void GenerateDesFilePath (string path)
    {
        string folderPath = Application.dataPath.Replace("/Assets", path + inputFolderName);

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            for (int i = 0; i < fileSelectList.Count; ++i)
                if (fileSelectList[i])
                    desFilePath.Add(folderPath + "/" + chargeFileName[i]);
        }
        else
        {
            for (int i = 0; i < fileSelectList.Count; ++i)
            {
                if (fileSelectList[i])
                    desFilePath.Add(folderPath + "/" + chargeFileName[i]);
            }
                
        }
    }

    UnityEngine.Object GenerateCurve(string source)
    {
        UnityEngine.Object prefab = null;

        if (source == null || source.Length == 0) return prefab;

        if (Path.GetExtension(source).ToLower() == ".txt")
        {
            if (File.Exists(source))
            {
                string name = Path.GetFileNameWithoutExtension(source);

                using (StreamReader reader = new StreamReader(File.Open(source, FileMode.Open)))
                {
                    AnimationCurve curve_forward = new AnimationCurve();
                    AnimationCurve curve_side = new AnimationCurve();
                    AnimationCurve curve_up = new AnimationCurve();
                    AnimationCurve curve_rotation = new AnimationCurve();

                    List<Keyframe> frames_forward = new List<Keyframe>();
                    List<Keyframe> frames_side = new List<Keyframe>();
                    List<Keyframe> frames_up = new List<Keyframe>();
                    List<Keyframe> frames_rotation = new List<Keyframe>();

                    float base_rotation = 0;
                    int count = 0;

                    while (true)
                    {
                        string data = reader.ReadLine();
                        if (data == null) break;

                        string[] datas = data.Split('\t');

                        if (datas.Length > 3)
                        {
                            Keyframe frame = new Keyframe(XParse.Parse(datas[0]), XParse.Parse(datas[1]), 0, 0);
                            frames_forward.Add(frame);
                            frame = new Keyframe(XParse.Parse(datas[0]), XParse.Parse(datas[2]), 0, 0);
                            frames_side.Add(frame);
                            frame = new Keyframe(XParse.Parse(datas[0]), XParse.Parse(datas[3]), 0, 0);
                            frames_up.Add(frame);

                            if (_rotate)
                            {
                                if (datas.Length > 4)
                                {
                                    float parse = XParse.Parse(datas[4]);
                                    if (count == 0) base_rotation = parse;

                                    frame = new Keyframe(XParse.Parse(datas[0]), parse, 0, 0);
                                    frames_rotation.Add(frame);
                                }
                                else
                                {
                                    EditorUtility.DisplayDialog("Confirm your data",
                                        "Please select raw-data file with new format!",
                                        "Ok");
                                    return null;
                                }
                            }
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Confirm your data",
                                "Please select raw-data file with new format!",
                                "Ok");
                            return null;
                        }

                        count++;
                    }

                    if (_forward)
                    {
                        float max = 0;
                        float firstland = 0;

                        //RefineData(frames_forward);

                        curve_forward.keys = frames_forward.ToArray();
                        for (int i = 0; i < curve_forward.keys.Length; ++i)
                        {
                            curve_forward.SmoothTangents(i, 0); //zero weight means average
                        }

                        FindKeyPoint(curve_forward, out max, out firstland);
                        prefab = CreateCurvePrefab(name, prefabName, curve_forward, max, firstland, XCurveType.Forward);
                    }

                    if (_side)
                    {
                        float max = 0;
                        float firstland = 0;

                        //RefineData(frames_side);

                        curve_side.keys = frames_side.ToArray();
                        for (int i = 0; i < curve_side.keys.Length; ++i)
                        {
                            curve_side.SmoothTangents(i, 0); //zero weight means average
                        }
                        FindKeyPoint(curve_side, out max, out firstland);
                        prefab = CreateCurvePrefab(name, prefabName, curve_side, max, firstland, XCurveType.Side);
                    }

                    if (_up)
                    {
                        float max = 0;
                        float firstland = 0;

                        //RefineData(frames_up);

                        curve_up.keys = frames_up.ToArray();
                        for (int i = 0; i < curve_up.keys.Length; ++i)
                        {
                            curve_up.SmoothTangents(i, 0); //zero weight means average
                        }
                        FindKeyPoint(curve_up, out max, out firstland);
                        prefab = CreateCurvePrefab(name, prefabName, curve_up, max, firstland, XCurveType.Up);
                    }

                    if (_rotate)
                    {
                        float max = 0;
                        float firstland = 0;

                        RefineData(frames_rotation);

                        curve_rotation.keys = frames_rotation.ToArray();
                        for (int i = 0; i < curve_rotation.keys.Length; ++i)
                        {
                            curve_rotation.SmoothTangents(i, 0); //zero weight means average
                        }
                        FindKeyPoint(curve_rotation, out max, out firstland);
                        prefab = CreateCurvePrefab(name, prefabName, curve_rotation, max, firstland, XCurveType.Rotation);
                    }
                }
            }
        }

        return prefab;
    }

    private void BehitGraphExporter()
    {
        string behitPath = $"{Application.dataPath}/BundleRes/HitPackage/" + inputFolderName;
        uint pID = (uint)presentID;

        if (!Directory.Exists(behitPath))
        {
            Directory.CreateDirectory(behitPath);
        }

        for (int i = 0; i < fileSelectList.Count; ++i)
        {
            if (fileSelectList[i])
            {
                string template = templates[i];
                string name = template.Substring(template.IndexOf("/") + 1, template.Length - template.IndexOf("/") - 1);
                int length = name.IndexOf("_");
                length = name.IndexOf("_", length + 1);
                name = name.Replace(name.Substring(0, length), inputFolderName);

                var behitEditor = EditorWindow.GetWindow<BehitEditor>();
                behitEditor.ToolBarNewClicked();
                var behitGraph = behitEditor.CurrentGraph as BehitGraph;

                behitGraph.configData.PresentID = presentID;
                behitGraph.configData.Name = name.Replace(".bytes", "");
                behitGraph.configData.CopyFrom = templates[i];
                behitGraph.configData.AnimTemplate = animTemplate;
                behitGraph.configData.AudioTemplate = audioTemplate;
                //behitGraph.BuildFromTemplate(false);
                behitGraph.BuildFromTemplate();
                if (!behitGraph.BuildFromTemplate())
                {
                    Debug.Log("Refresh By Template Error!!");
                    ShowNotification(new GUIContent("Refresh By Template Error!!\n"), 5);
                }

                behitGraph.SaveData(behitPath + "/" + name, true);
                behitEditor.Close();
            }
        }
    }

    void FindKeyPoint(AnimationCurve curve, out float max, out float firstland)
    {
        max = curve[0].value;
        for (int k = 1; k < curve.length; ++k)
        {
            float t1 = curve[k - 1].time;
            float t2 = curve[k].time;
            float t3 = (t1 + t2) * 0.5f;

            max = Mathf.Max(max, curve.Evaluate(t1), curve.Evaluate(t2), curve.Evaluate(t3));
        }

        firstland = curve[curve.length - 1].time;
        for (int k = 1; k < curve.length; ++k)
        {
            if (curve[k].value < 0.001f && curve[k].value >= 0)
            {
                firstland = Mathf.Min(firstland, curve[k].time);
            }
        }
    }

    void RefineData(List<Keyframe> list)
    {
        int i = list.Count - 1;

        if (list[i].value == 0)
        {
            int j = i - 1;
            while (j >= 0)
            {
                if (list[j].value != 0)
                    break;
                j--;
            }
            if (j >= 0 && j + 2 < list.Count)
            {
                list.RemoveRange(j + 2, i - j - 1);
            }
        }
    }

    UnityEngine.Object CreateCurvePrefab(string name, string prefab_name, AnimationCurve curve, float maxvalue, float firstland, XCurveType type)
    {
        string path = XEditorPath.GetPath("Curve" + "/" + prefab_name);

        string server_curve_name = null;
        string fullname = null;



        switch (type)
        {
            case XCurveType.Forward:
                {
                    server_curve_name = name + "_forward";
                    fullname = path + server_curve_name + ".prefab";
                }
                break;
            case XCurveType.Side:
                {
                    server_curve_name = name + "_side";
                    fullname = path + server_curve_name + ".prefab";
                }
                break;
            case XCurveType.Up:
                {
                    server_curve_name = name + "_up";
                    fullname = path + server_curve_name + ".prefab";
                }
                break;
            case XCurveType.Rotation:
                {
                    server_curve_name = name + "_rotation";
                    fullname = path + server_curve_name + ".prefab";
                }
                break;
        }
        // GameObject go = new GameObject (server_curve_name);
        // prefab = PrefabUtility.SaveAsPrefabAsset (go, fullname);


        GameObject go = new GameObject(server_curve_name);
        XCurve xcurve = go.AddComponent<XCurve>();
        xcurve.Curve = curve;
        xcurve.Max_Value = maxvalue;
        xcurve.Land_Value = firstland;
        UnityEngine.Object prefab = PrefabUtility.SaveAsPrefabAsset(go, fullname);
        EditorGUIUtility.PingObject(prefab);
        XServerCurveGenerator.GenerateCurve(go, fullname);
        DestroyImmediate(go);

        return prefab;
    }
    
    private void SplitFBX (int fbxTypeIndex, string fbxPath, string monsterName)
    {
        ModelImporter fbxImporter = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        ModelImporter refImporter = AssetImporter.GetAtPath(refSplittedFBX[fbxTypeIndex]) as ModelImporter;

        var refAnimations = refImporter.clipAnimations;
        List<ModelImporterClipAnimation> fbxClipAnimations = new List<ModelImporterClipAnimation>();

        foreach (var refAnimation in refAnimations)
        {
            ModelImporterClipAnimation newClip = new ModelImporterClipAnimation();
            newClip = refAnimation;

            newClip.name = newClip.name.Replace("Role_Usopp", monsterName);
            fbxClipAnimations.Add(newClip);
        }
        fbxImporter.clipAnimations = fbxClipAnimations.ToArray();

        AssetDatabase.ImportAsset(fbxPath, ImportAssetOptions.ForceUpdate); // Remember to reimport the asset
    }
    
}

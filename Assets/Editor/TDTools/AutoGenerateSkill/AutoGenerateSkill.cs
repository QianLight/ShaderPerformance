using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using CFClient;
using Debug = UnityEngine.Debug;
using System.Runtime.InteropServices;
using CFEngine.Editor;
using CFEngine;
using AnimationUtility = UnityEditor.AnimationUtility;
using XEditor;
using EcsData;

namespace TDTools
{
    public class AutoGenerateSkill : EditorWindow
    {
        public string svnPath = "";
        public string oppPath = "";
        public List<string> fbxFileName = new List<string>();
        public List<string> chargeFileName = new List<string>();
        public List<string> skillName = new List<string>();
        public List<string> animName = new List<string>(); 
        public List<bool> fileSelectList = new List<bool>();
        private bool needSelect = false;
        private Vector2 skillListScrollViewPos;
        private bool showFileList = false;
        private string folderName = "";
        public string svnFilePath = "";
        public List<string> scrFileName = new List<string>();
        public List<string> distFilePath = new List<string>();
        private string copyPath = "";
        private bool isFBX = false;
        private bool isCurve = false;
        private List<ObjectInfo> m_Objects = new List<ObjectInfo>();
        internal static CommonAssets.AssetLoadCallback<GameObject, ModelImporter> srcFbx = new CommonAssets.AssetLoadCallback<GameObject, ModelImporter>("*.fbx");
        private string _prefab = "";
        private string folderPathAnim = "";
        private int presentID;
        private bool isSkillGraphExporter = false;
        private string inputFolderName = "";
        private bool showFolderName = false;
        private bool runTwice = false;

        private enum XCurveType
        {
            Forward,
            Side, 
            Up,
            Rotation
        }
        private bool _forward = true;
        private bool _side = false;
        private bool _up = false;
        private bool _rotate = false;

        [MenuItem("Tools/TDTools/关卡相关工具/自动生成技能脚本 %&s")]
        public static void ShowWindow()
        {
            //显示窗口
            AutoGenerateSkill AutoGenerateSkills = EditorWindow.GetWindow<AutoGenerateSkill>(typeof(AutoGenerateSkill));
            // abc
        }
        public void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("曲线文件"))
            {
                fileSelectList.Clear();
                chargeFileName.Clear();
                readPathConfig();
                opneCurveFileWindos();
                copyPath = "/Assets/Editor/EditorResources/Curve/";
                isFBX = false;
                isSkillGraphExporter = false;
                isCurve = true;
            }
            if (GUILayout.Button("FBX文件"))
            {
                fileSelectList.Clear();
                readPathConfig();
                opneFileWindos();
                copyPath = "/Assets/Creatures/";
                isCurve = false;
                isSkillGraphExporter = false;
                isFBX = true;
            }
            if (GUILayout.Button("生成技能脚本"))
            {
                animName.Clear();
                fileSelectList.Clear();
                opneAnimFileWindos();
                isFBX = false;
                isCurve = false;
                isSkillGraphExporter = true;
            }
            EditorGUILayout.EndHorizontal();
            if (isSkillGraphExporter)
            {
                EditorGUILayout.BeginHorizontal();
                presentID = EditorGUILayout.IntField("请输入PresentID", presentID);
                EditorGUILayout.EndHorizontal();
            }
            
            if (showFolderName)
            {
                EditorGUILayout.BeginHorizontal();
                inputFolderName = EditorGUILayout.TextField("项目文件夹名", inputFolderName);
                EditorGUILayout.EndHorizontal();
            }
            if (isFBX)
            {
                if (showFileList)
                {
                    skillListScrollViewPos = EditorGUILayout.BeginScrollView(skillListScrollViewPos, false, true);

                    for (int i = 0; i < fbxFileName.Count; ++i)
                    {
                        EditorGUILayout.BeginHorizontal();
                        fileSelectList[i] = EditorGUILayout.ToggleLeft(new GUIContent(fbxFileName[i]), fileSelectList[i]);
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
                        copyFile(copyPath);
                    }
                    if (GUILayout.Button("导出动画文件"))
                    {
                        exportAnim();
                    }
                    if (GUILayout.Button("刷新技能脚本"))
                    {
                        RefreshSkill();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            else if (isCurve)
            {
                if (showFileList)
                {
                    skillListScrollViewPos = EditorGUILayout.BeginScrollView(skillListScrollViewPos, false, true);

                    for (int i = 0; i < chargeFileName.Count; ++i)
                    {
                        EditorGUILayout.BeginHorizontal();
                        fileSelectList[i] = EditorGUILayout.ToggleLeft(new GUIContent(chargeFileName[i]), fileSelectList[i]);
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndScrollView();
                    EditorGUILayout.BeginHorizontal();
                    _forward = EditorGUILayout.ToggleLeft("Generate Forward", _forward);
                    _side = EditorGUILayout.ToggleLeft("Generate Side", _side);
                    _up = EditorGUILayout.ToggleLeft("Generate Up", _up);
                    _rotate = EditorGUILayout.ToggleLeft("Generate Rotation", _rotate);
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
                        copyFile(copyPath);
                    }
                    if (GUILayout.Button("导出曲线文件"))
                    {
                        CurveGenerator(copyPath);
                    }
                    if (GUILayout.Button("刷新技能脚本"))
                    {
                        RefreshSkill();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            else if (isSkillGraphExporter)
            {
                if (showFileList)
                {
                    skillListScrollViewPos = EditorGUILayout.BeginScrollView(skillListScrollViewPos, false, true);

                    for (int i = 0; i < animName.Count; ++i)
                    {
                        EditorGUILayout.BeginHorizontal();
                        fileSelectList[i] = EditorGUILayout.ToggleLeft(new GUIContent(animName[i]), fileSelectList[i]);
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
                    if (GUILayout.Button("生成!"))
                    {
                        SkillGraphExporter();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        public void readPathConfig()
        {
            string pathConfig = Application.dataPath;
            pathConfig = pathConfig.Replace("/Assets", "/Assets/Editor/TDTools/AutoGenerateSkill/pathConfig.txt");
            string[] sr = File.ReadAllLines(pathConfig);
            svnPath = sr[0];
            oppPath = sr[1];
        }
        public void opneFileWindos()
        {
            fbxFileName = new List<string>();
            skillName = new List<string>();
            svnPath = svnPath.Replace("svn=", "");
            svnFilePath = EditorUtility.OpenFolderPanel("Open File Dialog", svnPath, "");
            if (Directory.Exists(svnFilePath))
            {
                DirectoryInfo direction = new DirectoryInfo(svnFilePath);
                FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].Name.EndsWith(".FBX"))
                    {
                        fbxFileName.Add(files[i].Name);
                        skillName.Add(files[i].Name);
                        fileSelectList.Add(true);
                    }
                }
            }
            inputFolderName = fbxFileName[0].Substring(0, fbxFileName[0].LastIndexOf("_"));
            showFolderName = true;
            showFileList = true;
        }
        public void opneCurveFileWindos()
        {
            chargeFileName = new List<string>();
            svnPath = svnPath.Replace("svn=", "");
            svnFilePath = EditorUtility.OpenFolderPanel("Open File Dialog", svnPath, "");
            if (Directory.Exists(svnFilePath))
            {
                DirectoryInfo direction = new DirectoryInfo(svnFilePath);
                FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].Name.EndsWith(".txt"))
                    {
                        chargeFileName.Add(files[i].Name);
                        fileSelectList.Add(true);
                    }
                }
            }
            inputFolderName = chargeFileName[0].Substring(0, chargeFileName[0].LastIndexOf("_"));
            showFolderName = true;
            showFileList = true;
        }
        public void opneAnimFileWindos()
        {
            animName = new List<string>();
            string localAnimPath = $"{Application.dataPath}/BundleRes/Animation/" + inputFolderName;
            string animFilePath = EditorUtility.OpenFolderPanel("Open File Dialog", localAnimPath, "");
            if (Directory.Exists(animFilePath))
            {
                DirectoryInfo direction = new DirectoryInfo(animFilePath);
                FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].Name.EndsWith(".anim"))
                    {
                        animName.Add(files[i].Name);
                        fileSelectList.Add(true);
                    }
                }
            }
            showFolderName = true;
            showFileList = true;
        }
        public void SetSelectAll(bool value)
        {
            if (isFBX)
            {
                for (int i = 0; i < fbxFileName.Count; i++)
                {
                    fileSelectList[i] = value;
                }
            }
            else if (isCurve)
            {
                for (int i = 0; i < chargeFileName.Count; i++)
                {
                    fileSelectList[i] = value;
                }
            }
            else if (isSkillGraphExporter)
            {
                for (int i = 0; i < animName.Count; i++)
                {
                    fileSelectList[i] = value;
                }
            }
            
        }
        public void copyFile(string path)
        {
            
            if (isFBX)
            {
                distFilePath.Clear();
                folderName = inputFolderName;
                string folderPath = Application.dataPath.Replace("/Assets", path + folderName);
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
                    for (int i = 0; i < fileSelectList.Count; i++)
                    {

                        if (fileSelectList[i])
                        {
                            distFilePath.Add("Assets/Creatures/" + folderName + "/Animation/" + fbxFileName[i]);
                            File.Copy(svnFilePath + "/" + fbxFileName[i], folderPathAnim + "/" + fbxFileName[i], true);
                        }
                        else distFilePath.Add("");
                    }
                }
                AssetDatabase.Refresh();
            }
            else if (isCurve)
            {
                distFilePath.Clear();
                folderName = inputFolderName;
                string folderPath = Application.dataPath.Replace("/Assets", path + folderName);
                folderPathAnim = folderPath + "/Animation";
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    for (int i = 0; i < fileSelectList.Count; i++)
                    {
                        if (fileSelectList[i])
                        {
                            distFilePath.Add(folderPath + "/" + chargeFileName[i]);
                            File.Copy(svnFilePath + "/" + chargeFileName[i], folderPath + "/" + chargeFileName[i], true);
                        }
                        else distFilePath.Add("");
                    }
                }
                else
                {
                    for (int i = 0; i < fileSelectList.Count; i++)
                    {
                        if (fileSelectList[i])
                        {
                            distFilePath.Add(folderPath + "/" + chargeFileName[i]);
                            File.Copy(svnFilePath + "/" + chargeFileName[i], folderPath + "/" + chargeFileName[i], true);
                        }
                        else distFilePath.Add("");
                    }
                }
                AssetDatabase.Refresh();
            }
        }

        public void RefreshSkill()
        {
            string skillPath = $"{Application.dataPath}/BundleRes/SkillPackage/" + inputFolderName;
            
            List<string> AllSkillNames = new List<string>();
            DirectoryInfo directoryInfo = new DirectoryInfo(skillPath);
            foreach (FileInfo file in directoryInfo.GetFiles())                     //"C:/Users/user/Desktop/Project/OPProject/Assets/BundleRes/SkillPackage/Role_Ace/Role_Ace_attack1.bytes"
            {
                if (file.Extension == ".bytes")
                {
                    AllSkillNames.Add(file.Name);
                }
            }

            var skillEditor = EditorWindow.GetWindow<SkillEditor>();
            foreach (string skillname in AllSkillNames)
            {
                skillEditor.SaveSkill(skillPath + "/" + skillname);
            }
           skillEditor.Close();
        }

        public void exportAnim()
        {
            m_Objects.Clear();
            folderName = inputFolderName;
            for (int i = 0; i < fileSelectList.Count; i++)
            {

                if (fileSelectList[i])
                {
                    UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath("Assets/Creatures/" + folderName + "/Animation/" + fbxFileName[i], typeof(UnityEngine.Object));
                    string path = AssetDatabase.GetAssetPath(obj);
                    ObjectInfo oi = new ObjectInfo();
                    oi.obj = obj;
                    oi.path = path;
                    m_Objects.Add(oi);
                }
            }
            //for (int i = 0; i < distFilePath.Count; i++)
            //{
            //    UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(distFilePath[i], typeof(UnityEngine.Object));
            //    string path = AssetDatabase.GetAssetPath(obj);
            //    ObjectInfo oi = new ObjectInfo();
            //    oi.obj = obj;
            //    oi.path = path;
            //    m_Objects.Add(oi);
            //}
            Fbx_ExportAnimFuc(m_Objects);
        }
        //导出曲线文件
        public void CurveGenerator(string path)
        {

            string folderPath = Application.dataPath.Replace("/Assets", path + folderName);
            for (int i = 0; i < fileSelectList.Count; i++)
            {
                if (fileSelectList[i])
                {
                    string sr = Path.GetDirectoryName(folderPath + "/" + chargeFileName[i]);
                    _prefab = Path.GetFileNameWithoutExtension(sr);
                    GenerateCurve(distFilePath[i]);
                }
            }
        //    for (int i = 0; i < distFilePath.Count; i++)
        //    {
        //        string sr = Path.GetDirectoryName(distFilePath[i]);
        //        _prefab = Path.GetFileNameWithoutExtension(sr);
        //        GenerateCurve(distFilePath[i]);
        //    }
        }
        
        public void SkillGraphExporter()
        {
            string skillPath = $"{Application.dataPath}/BundleRes/SkillPackage/" + inputFolderName;
            uint ID = (uint)presentID;
            Vector2 posAnim = new Vector2(0, 0);
            Vector2 posCharge = new Vector2(0, 2);
            if (!Directory.Exists(skillPath))
            {
                Directory.CreateDirectory(skillPath);
            }
            for (int i = 0; i < fileSelectList.Count; i++)
            {

                if (fileSelectList[i])
                {
                    var skillEditor = EditorWindow.GetWindow<SkillEditor>();
                    skillEditor.ToolBarNewClicked();
                    var skillGraph = skillEditor.CurrentGraph as SkillGraph;
                    string name = animName[i].Replace(".anim","");
                    AnimationClip clip = AssetDatabase.LoadAssetAtPath("Assets/BundleRes/Animation/" + inputFolderName + "/" + name + ".anim" ,typeof(AnimationClip)) as AnimationClip;
                    var dataAnim = skillGraph.GetConfigData<XSkillData>().AnimationData;
                    var nodeAnim = skillGraph.AddNodeInGraphByScript<XAnimationData, EditorNode.AnimationNode>(posAnim, ref dataAnim, true);
                    var dataCharge = skillGraph.GetConfigData<XSkillData>().ChargeData;
                    var nodeCharge = skillGraph.AddNodeInGraphByScript<XChargeData, EditorNode.ChargeNode>(posCharge, ref dataCharge, true);
                    skillGraph.configData.Name = name;
                    skillGraph.configData.PresentID = presentID;
                    skillGraph.configData.AnimationData[0].TimeBased = true;
                    (skillGraph.widgetList[0] as EditorNode.AnimationNode).HosterData.ClipPath = "Animation/" + inputFolderName + "/" + name;
                    skillGraph.configData.Length = clip.length; 
                    skillGraph.configData.ChargeData[0].TimeBased = true;
                    skillGraph.configData.ChargeData[0].UsingCurve = true;
                    if (File.Exists(Application.dataPath + "/Editor/EditorResources/Curve/" + XEntityPresentationReader.GetData(ID).CurveLocation + skillGraph.configData.Name + ".txt"))
                    {
                        skillGraph.configData.ChargeData[0].CurveForward = "Curve/" + XEntityPresentationReader.GetData(ID).CurveLocation + skillGraph.configData.Name + "_forward.txt";
                        skillGraph.configData.ChargeData[0].CurveSide = "Curve/" + XEntityPresentationReader.GetData(ID).CurveLocation + skillGraph.configData.Name + "_side.txt";
                    }
                    else
                    {
                        skillGraph.DeleteNode(nodeCharge);
                    }
                    //if (!skillGraph.configData.ChargeData[0].CurveForward.Contains(name))
                    //{
                        
                    //}
                    //upCurve = AssetDatabase.LoadAssetAtPath(CurvePath + "Curve/" + XEntityPresentationReader.GetData((uint)GetRoot.GetConfigData<XConfigData>().PresentID).CurveLocation + GetRoot.GetConfigData<XConfigData>().Name + "_up.txt", typeof(TextAsset)) as TextAsset;
                    skillGraph.SaveData(skillPath + "/" + name + ".bytes", true);
                    skillEditor.Close();
                }
            }
            
        }
        
        static void Fbx_ExportAnimFuc(List<ObjectInfo> objInfoList)
        {
            srcFbx.cb = (fbx, modelImporter, path, context) =>
            {
                //Debug.Log(path);
                string folderName;
                if (AssetsPath.GetCreatureFolderName(path, out folderName))
                {
                    //Debug.Log(folderName);
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
                if (strs.Length > 0 && !strs[strs.Length - 1].StartsWith("Bone_"))
                {
                    AnimationUtility.SetEditorCurve(animClip, binding, null);
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
                            prefab = CreateCurvePrefab(name, _prefab, curve_forward, max, firstland, XCurveType.Forward);
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
                            prefab = CreateCurvePrefab(name, _prefab, curve_side, max, firstland, XCurveType.Side);
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
                            prefab = CreateCurvePrefab(name, _prefab, curve_up, max, firstland, XCurveType.Up);
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
                            prefab = CreateCurvePrefab(name, _prefab, curve_rotation, max, firstland, XCurveType.Rotation);
                        }
                    }
                }
            }

            return prefab;
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

        void FindKeyPoint(AnimationCurve curve, out float max, out float firstland)
        {
            max = curve[0].value;
            for (int k = 1; k < curve.length; k++)
            {
                float t1 = curve[k - 1].time;
                float t2 = curve[k].time;
                float t3 = (t1 + t2) * 0.5f;

                max = Mathf.Max(max, curve.Evaluate(t1), curve.Evaluate(t2), curve.Evaluate(t3));
            }

            firstland = curve[curve.length - 1].time;
            for (int k = 1; k < curve.length; k++)
            {
                if (curve[k].value < 0.001f && curve[k].value >= 0)
                {
                    firstland = Mathf.Min(firstland, curve[k].time);
                }
            }
        }
    }
}

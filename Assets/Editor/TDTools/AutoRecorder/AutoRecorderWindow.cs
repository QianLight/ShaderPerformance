using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Recorder;
using Cinemachine;
using VirtualSkill;
using CFEngine;
using CFUtilPoolLib;
using UnityEngine.UIElements;

namespace TDTools
{
    public class AutoRecorderWindow : EditorWindow
    {
        #region 技能脚本录制参数
        [Flags]
        enum ESpecialMask
        {
            NeedHit = 1,
            AIMode = 2,
        }

        private static readonly string[] CameraTypeStr = new string[]
        {
            "Small",
            "Middle",
            "Middle+",
            "Middle++",
            "Large",
            "Large+",
            "Titanic",
            "Titanic-Kaxi",
            "Custom",
        };

        private static readonly string[] CameraDirectionStr = new string[]
        {
            "All",
            "Front",
            "Right",
            "Left",
            "Back",
        };

        private static readonly string[] SpecialMask = new string[]
        {
            "NeedHit",
            "AIMode",
        };

        private static CinemachineFreeLook.Orbit[][] CameraTypeOrbits = new CinemachineFreeLook.Orbit[][]
        {
            // Small
            new CinemachineFreeLook.Orbit[]
            {
                new CinemachineFreeLook.Orbit(3f, 3f),
                new CinemachineFreeLook.Orbit(1f, 3f),
                new CinemachineFreeLook.Orbit(0.5f, 3f),
            },
            // Middle
            new CinemachineFreeLook.Orbit[]
            {
                new CinemachineFreeLook.Orbit(4f, 5f),
                new CinemachineFreeLook.Orbit(2f, 5f),
                new CinemachineFreeLook.Orbit(0.5f, 4f),
            },
            // Middle+
            new CinemachineFreeLook.Orbit[]
            {
                new CinemachineFreeLook.Orbit(4f, 7f),
                new CinemachineFreeLook.Orbit(4f, 7f),
                new CinemachineFreeLook.Orbit(0.5f, 4f),
            },
            // Middle++
            new CinemachineFreeLook.Orbit[]
            {
                new CinemachineFreeLook.Orbit(4f, 9f),
                new CinemachineFreeLook.Orbit(4f, 9f),
                new CinemachineFreeLook.Orbit(0.5f, 9f),
            },
            // Large
            new CinemachineFreeLook.Orbit[]
            {
                new CinemachineFreeLook.Orbit(5f, 12f),
                new CinemachineFreeLook.Orbit(5f, 12f),
                new CinemachineFreeLook.Orbit(5f, 12f),
            },
            // Large+
            new CinemachineFreeLook.Orbit[]
            {
                new CinemachineFreeLook.Orbit(6f, 17f),
                new CinemachineFreeLook.Orbit(6f, 17f),
                new CinemachineFreeLook.Orbit(6f, 17f),
            },
            // Titanic
            new CinemachineFreeLook.Orbit[]
            {
                new CinemachineFreeLook.Orbit(10f, 31f),
                new CinemachineFreeLook.Orbit(10f, 31f),
                new CinemachineFreeLook.Orbit(10f, 31f),
            },
            // Tatanic-Kaxi
            new CinemachineFreeLook.Orbit[]
            {
                new CinemachineFreeLook.Orbit(8f, 25f),
                new CinemachineFreeLook.Orbit(8f, 25f),
                new CinemachineFreeLook.Orbit(8f, 25f),
            },
            // Custom
            new CinemachineFreeLook.Orbit[]
            {
                new CinemachineFreeLook.Orbit(4f, 7f),
                new CinemachineFreeLook.Orbit(4f, 7f),
                new CinemachineFreeLook.Orbit(0.5f, 4f),
            },
        };

        private static readonly float[] CameraDirectionValue = new float[]
        {
            0f,
            0f,
            -90f,
            90f,
            180f,
        };

        private static Vector3[] CameraTypeTrackedOffsets = new Vector3[]
        {
            // Small
            new Vector3(0, 0, 0),
            // Middle
            new Vector3(0, 0, 0),
            // Middle+
            new Vector3(0, 0.5f, 0),
            // Middle++
            new Vector3(0, 0.5f, 0),
            // Large
            new Vector3(0, 0.5f, 0),
            // Large+
            new Vector3(0, 1.0f, 0),
            // Titanic
            new Vector3(0, 1.5f, 0),
            // Titanic-Kaxi
            new Vector3(0, -5f, 0),
            // Custom
            new Vector3(0, 0, 0),
        };

        private static string ConfigFile;
        private string configInfo;
        //private AutoRecorderConfig config;
        private NewConfig newConfig;

        public static AutoRecorderWindow Instance;
        private static SkillEditor skillEditor;
        private SkillGraph curGraph;
        private GUIStyle style;
        public RecorderController recorderController;
        public RecorderControllerSettings recorderControllerSettings;
        private MovieRecorderSettings recorderSettings;

        private string fullRecorderPath = "";
        private string recorderName = "";
        private string outPath = "";
        private string skillLocation = "";
        private string IdleRunState = "未开始";

        private bool needSelect = false;
        private List<string> fileNameList = new List<string>();    //所有文件名
        private List<bool> fileSelectList = new List<bool>();      //是否勾选
        private List<int> fileSpecialList = new List<int>();       //特殊标签
        private List<string> fileList = new List<string>();        //这里存的是实际用上的文件名
        private Dictionary<string, ESpecialMask> fileMaskDic = new Dictionary<string, ESpecialMask>();

        private GameObject playerObj;
        private Vector3 startPos;
        private Quaternion startRotate;
        private Vector3 pupetPos = new Vector3(0, 0, 7);
        private int pupetPresentID;
        public List<string> pupetStringFieldList = new List<string>();
        public List<string> pupetSkillNameList = new List<string>();

        private bool needResetCamera = true;
        private bool isAllDirection = false;
        private bool isRun = false;//外部控制跑动
        private int outsideCtrlDir = 0;
        private int[][] runDir = {
            new int[]{ 1,0 },
            new int[]{ 0,1 },
            new int[]{ 0,-1 },
            new int[]{ -1,0 },
            new int[]{ 0,0 }
        };
        private bool recAction = false;//录制动作，而非技能时需要用到
        string recActionList = "";
        private float recExtraDelay = 0.0f;
        private static readonly string[] defaultSmokeStrList = {
            "smoke",
            "baozhayan",
            "smioke"
        };
        
        Vector3 forward;
        Vector3 right;
        public static bool targetControl = false;
        //public ulong Target = SkillHoster.GetHoster.Target;
        public ulong Target = 0;
        private static ulong PlayerIndex = SkillHoster.PlayerIndex;
        //public SkillCamera cameraComponent=SkillHoster.GetHoster.cameraComponent;
        public SkillCamera cameraComponent;
        private Dictionary<ulong, Entity> EntityDic;
        


        private CinemachineFreeLook freeLook;
        private int cameraType = 0;
        private int cameraDirection = 0;
        private int cameraChangeDirectionDurationTime = 4;
        private int runStopChangeDirectionDurationTime = 1;
        private int runState = 0;

        private float t_height = 4f, t_radius = 7f, t_offset = 0f;


        private bool autoCast = false;
        private int castNum0 = 0; //切换技能
        private float timeInterval = 0f;
        private bool skillPlaying = false;
        private Vector2 skillListScrollViewPos;

        /// <summary>改变背景板位置的参数</summary>
        private Vector3[] VSPlaneInitPos = new Vector3[] {
            new Vector3(0,0,0),
            new Vector3(0,0,300),
            new Vector3(-300,0,0),
            new Vector3(0,0,-300),
            new Vector3(300,0,0)
        };
        #endregion

        private bool isHitRecorder = false;
        private bool isChangeCharacter = false;
        private int presentID;
        private int ChangedPresentID;  //有的时候要使用一个角色的技能，却要用另一个角色的模型

        private string beHitConfigFile;
        private string beHitConfigFilePath;
        private string beHitConfigInfo;
        private string hitConfigFile;
        private string hitConfigInfo;

        private string hitRecorderPath = "";
        private int hitDrectionCount = 0;

        private List<int> beHitPresentIDList = new List<int>();
        private List<float> beHitRadiusList = new List<float>();
        private List<string> beHitPresentNameList = new List<string>();

        private List<string> hitConfigList = new List<string>();  //_hit_backage_1........
        private List<int> hitPresentIDList = new List<int>();     //1,2,500,400.......
        private List<Vector3> hitPupetPosList = new List<Vector3>();     //受击者相对位置
        private List<int> hitPupetAngleList = new List<int>();     //受击目标相对角度
        private List<bool> hitConfigSelectList = new List<bool>(); //为true的存入hitConfigIndexList
        private List<int> hitConfigIndexList = new List<int>();  //通过hitConfigIndexList去取hitConfigList和hitPresentIDList

        private string hitSkill;
        private string hitSkillName;

        private int hitNum0; //一重循环 换人
        private int hitNum1; //二重循环 换hit脚本

        private GameObject hitGroupCameraGO;
        private int hitPupetAngle = 0;
        private float hitWaitTime = 0.2f;
        

        /// <summary>背景板</summary>
        private GameObject[] VCTset_Plane = new GameObject[5];
        /// <summary>去烟</summary>
        private bool needDeSmoke = false;
        private string PresentationFile;


        [MenuItem("Tools/TDTools/监修相关工具/AutoRecorder &%R")]
        public static void ShowWindow()
        {
            /*
            if (!Application.isPlaying)
            {
                /*
                SkillEditor.Instance.SetcurrentLod(SkillEditor.EnumLodDes.lod0);
                UnityEditor.EditorSettings.asyncShaderCompilation = false;
                Debug.Log("已切换lod0，关闭异步shader编译");
                *
                Debug.Log("需要在技能编辑器开启且当前有脚本运行时才能打开");
                return;
            }
            */
            skillEditor = GetWindow<SkillEditor>();
            if (skillEditor == null)
            {
                Debug.Log("需要在技能编辑器开启且当前有脚本运行时才能打开");
                return;
            }
            if (!Instance) Instance = GetWindow<AutoRecorderWindow>("技能半自动录制工具");

            if (!OverdrawMonitor.isOn)
            {
                SFXMgr.singleton.createMonitor = OverdrawMonitor.Instance.StartObserveProfile;
                SFXMgr.singleton.destoryMonitor = OverdrawMonitor.Instance.EndObserveProfile;
            }
            Instance.Focus();
        }

        private void OnEnable()
        {
            ConfigFile = $"{Application.dataPath}/Editor/TDTools/AutoRecorder/Config.xml";
            beHitConfigFile = $"{Application.dataPath}/Editor/TDTools/AutoRecorder/BehitList.txt";
            beHitConfigFilePath = $"{Application.dataPath}/Editor/TDTools/AutoRecorder";
            hitConfigFile = $"{Application.dataPath}/Editor/TDTools/AutoRecorder/hitList.txt";
            PresentationFile = $"{Application.dataPath}/Table/XEntityPresentation.txt";
            style = new GUIStyle() { fontSize = 12, normal = new GUIStyleState() { textColor = Color.white } };
            if (recorderController == null)
            {
                recorderControllerSettings = RecorderControllerSettings.LoadOrCreate($"{Application.dataPath}/Editor/TDTools/AutoRecorder/recorder.pref");
                foreach (var setting in recorderControllerSettings.RecorderSettings)
                {
                    (setting as MovieRecorderSettings).videoBitRateMode = VideoBitrateMode.High;
                }
                //recorderSettings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
                //recorderSettings.name = "AutoRecorder";
                //recorderSettings.videoBitRateMode = VideoBitrateMode.High;
                //recorderControllerSettings.AddRecorderSettings(recorderSettings);
                recorderController = new RecorderController(recorderControllerSettings);
            }
            curGraph = skillEditor.CurrentGraph as SkillGraph;
            var presentData = XEntityPresentationReader.GetData((uint)curGraph.configData.PresentID);
            fullRecorderPath = $"{Application.dataPath}/BundleRes/SkillPackage/{presentData.SkillLocation}";
            hitRecorderPath = $"{Application.dataPath}/BundleRes/SkillPackage/Role_hit";
            skillLocation = presentData.SkillLocation;
            recorderName = presentData.Prefab;
            hitDrectionCount = 0;
            presentID = curGraph.configData.PresentID;
            ChangedPresentID = 0;
            isAllDirection = false;

            InitConfig();
            InitFileList();
            BehitInitData();
            GameObject camera = GameObject.Find("FreeLook_skillEditor(Clone)");
            freeLook = camera?.GetComponent<CinemachineFreeLook>();
            playerObj = GameObject.Find("Player");
            startPos = playerObj.transform.position;
            startRotate = playerObj.transform.rotation;

            Target = SkillHoster.GetHoster.Target;
            cameraComponent = SkillHoster.GetHoster.cameraComponent;
            EntityDic = SkillHoster.GetHoster.EntityDic;

            VCTset_Plane[0] = GameObject.Find("VCtest_plane_fbx_0");
            VCTset_Plane[1] = GameObject.Find("VCtest_plane_fbx_1");
            VCTset_Plane[2] = GameObject.Find("VCtest_plane_fbx_2");
            VCTset_Plane[3] = GameObject.Find("VCtest_plane_fbx_3");
            VCTset_Plane[4] = GameObject.Find("VCtest_plane_fbx_4"); 
            //var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/TDTools/AutoRecorder/UI/AutoRecorder_UXML.uxml");
            //asset.CloneTree(rootVisualElement);
        }

        private void OnDisable()
        {
            if (recorderController == null)
            {
                recorderController.Settings.RemoveRecorder(recorderSettings);
            }
        }

        private void OnGUI()
        {
            if (!Application.isPlaying) {
                if (GUILayout.Button("切换lod0，关闭异步shader编译（去除未加载时的蓝色）"))
                {
                    SkillEditor.Instance.SetcurrentLod(SkillEditor.EnumLodDes.lod0);
                    UnityEditor.EditorSettings.asyncShaderCompilation = false;
                    Debug.Log("切换lod0，关闭异步shader编译");
                }
                if (GUILayout.Button("Tools/替换监修文件夹"))
                {
                    EditorApplication.ExecuteMenuItem("Tools/替换监修文件夹");
                }
                if (GUILayout.Button("修改Presentation体型比例"))
                {
                    PresentationScaleTo1();
                }
                
            }
            if (Application.isPlaying && !skillPlaying && !autoCast)
            {
                isHitRecorder = EditorGUILayout.Toggle("受击脚本录制", isHitRecorder);
                if (!isHitRecorder)
                {

                    EditorGUILayout.Space(20);
                    
                    EditorGUILayout.BeginHorizontal(style);
                    EditorGUILayout.LabelField("录制目录：", fullRecorderPath, style);
                    //if (GUILayout.Button("...", GUILayout.Width(50)))
                    //{
                    //    fullRecorderPath = EditorUtility.OpenFolderPanel("选择要录制的目录", fullRecorderPath, "");
                    //    InitFileList();
                    //}
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.LabelField("录制角色名：", recorderName, style);

                    //EditorGUILayout.Space(20);
                    isChangeCharacter= EditorGUILayout.Toggle("是否更换录制角色", isChangeCharacter);
                    if (isChangeCharacter)
                    {
                        ChangedPresentID = EditorGUILayout.IntField("输入录制角色PresentID", ChangedPresentID);
                    }
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("输出目录：", outPath, style);
                    if (GUILayout.Button("...", new GUILayoutOption[] { GUILayout.Width(50) }))
                    {
                        outPath = EditorUtility.OpenFolderPanel("选择要输出的目录", outPath, "");
                        //EditorPrefs.SetString(TimelineSettings.PREF_RECD, outPath);
                        EditorPrefs.SetString("PREF_RECD", outPath);
                    }
                    EditorGUILayout.EndHorizontal();

                    //if (GUILayout.Button("Build"))
                    //{
                    //    Build();
                    //    //AutoSkillCasterForRole.Instance.Init(fileList);
                    //}
                    if (GUILayout.Button("检查脚本"))
                    {
                        try
                        {
                            bool result = false;
                            StringBuilder sb = new StringBuilder();
                            for (int i = 0; i < fileNameList.Count; ++i)
                            {
                                if (fileSelectList[i])
                                {
                                    string fileName = $"{Application.dataPath}/BundleRes/SkillPackage/{skillLocation}/{fileNameList[i]}";
                                    bool res = AutoRecorderMgr.GetMgr.CheckSelectFile(fileName, out string itemInfo);
                                    if (res)
                                    {
                                        result = true;
                                        sb.AppendLine(itemInfo);
                                    }
                                }
                            }
                            if(result)
                            {
                                EditorUtility.DisplayDialog("check result", sb.ToString(), "确定");
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e.ToString());
                        }
                        curGraph.OpenData(curGraph.DataPath);
                    }
                    if (GUILayout.Button("开始录制"))
                    {
                        if (cameraDirection == 0) isAllDirection = true;
                        castNum0 = 0;
                        StartRecord();
                    }
                    EditorGUILayout.Space(20);
                    cameraType = EditorGUITool.Popup("相机规格", cameraType, CameraTypeStr);

                    if (cameraType == CameraTypeStr.Length - 1)
                    {
                        EditorGUILayout.LabelField("相机距离");
                        t_height = EditorGUILayout.Slider("高度", t_height, 1f, 12f);
                        t_radius = EditorGUILayout.Slider("半径", t_radius, 2f, 40f);
                        t_offset = EditorGUILayout.Slider("视角", t_offset, -5f, 5f);
                        CameraTypeOrbits[cameraType] = new CinemachineFreeLook.Orbit[] {
                            new CinemachineFreeLook.Orbit(t_height, t_radius),
                            new CinemachineFreeLook.Orbit(t_height, t_radius),
                            new CinemachineFreeLook.Orbit(t_height, t_radius),
                        };
                        CameraTypeTrackedOffsets[cameraType] = new Vector3(0, t_offset, 0);
                        EditorGUILayout.Space(10);
                    }
                    cameraDirection = EditorGUITool.Popup("相机位置", cameraDirection, CameraDirectionStr);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("预览相机配置（第一次需要点两下）"))
                    {
                        SetCameraParam();
                        if (cameraDirection != 0) isAllDirection = false;
                        else isAllDirection = true;
                    }
                    if (GUILayout.Button("重置相机配置（如果预览相机配置没反应，可以先点这个）"))
                    {
                        cameraType = 0;
                        hitDrectionCount = 0;
                        cameraDirection = 0;
                        SetCameraParam();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    needResetCamera = EditorGUILayout.ToggleLeft("需要在每个脚本录制前，重置相机参数为上方配置", needResetCamera);
                    //needDeSmoke = EditorGUILayout.ToggleLeft("去除烟尘", needDeSmoke);
                    EditorGUILayout.EndHorizontal();

                    pupetPos = EditorGUILayout.Vector3Field("受击目标相对位置", pupetPos);
                    EditorGUILayout.BeginHorizontal();
                    //pupetPresentID = EditorGUILayout.IntField("召唤目标presentID", pupetPresentID);
                   
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    needSelect = EditorGUILayout.ToggleLeft("选择文件模式", needSelect);
                    if (GUILayout.Button("仅选择当前文件"))
                    {
                        SelectCurrentGraphName();
                    }
                    EditorGUILayout.EndHorizontal();


                    if (needSelect)
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("全选"))
                        {
                            SetSelectAll(true);
                        }
                        if (GUILayout.Button("全不选"))
                        {
                            SetSelectAll(false);
                        }
                        if (GUILayout.Button("粘贴"))
                        {
                            PasteData();
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("全部Everything"))
                        {
                            for (int i = 0; i < fileNameList.Count; ++i)
                            {
                                fileSpecialList[i] = -1;
                            }
                        }
                        if (GUILayout.Button("全部Nothing"))
                        {
                            for (int i = 0; i < fileNameList.Count; ++i)
                            {
                                fileSpecialList[i] = 0;
                            }
                        }
                        recExtraDelay = EditorGUILayout.FloatField("额外延迟时间", recExtraDelay);
                        EditorGUILayout.EndHorizontal();
                        skillListScrollViewPos = EditorGUILayout.BeginScrollView(skillListScrollViewPos, false, true);

                        for (int i = 0; i < fileNameList.Count; ++i)
                        {
                            EditorGUILayout.BeginHorizontal();
                            fileSelectList[i] = EditorGUILayout.ToggleLeft(new GUIContent(fileNameList[i]), fileSelectList[i], new GUILayoutOption[] { GUILayout.Width(400f) });
                            fileSpecialList[i] = EditorGUILayout.MaskField(fileSpecialList[i], SpecialMask, new GUILayoutOption[] { GUILayout.Width(150f) });
                            pupetStringFieldList[i] = EditorGUILayout.TextField("召唤ID|技能名(多个用,隔开)", pupetStringFieldList[i]);
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndScrollView();
                    }


                    EditorGUILayout.BeginHorizontal();
                    cameraChangeDirectionDurationTime = EditorGUILayout.IntField("站立跑动录制持续时间", cameraChangeDirectionDurationTime);
                    runStopChangeDirectionDurationTime = EditorGUILayout.IntField("跑动间隔时间", runStopChangeDirectionDurationTime);
                    EditorGUILayout.EndHorizontal();
                    if (GUILayout.Button("开始站立跑动录制"))
                    {
                        if (cameraDirection == 0)
                        {
                            isAllDirection = true;
                            hitDrectionCount = 0;
                        }
                        RecordIdle();
                    }
                    EditorGUILayout.LabelField("站立跑动录制状态：", IdleRunState, style);
                    /*
                    if (GUILayout.Button("跑动测试"))
                    {
                        isRun = !isRun;
                        outsideCtrlDir = runState;
                        PreOMC();
                        if (isRun) SkillHoster.GetHoster.AutoFire += OusideMoveControl;
                        if(!isRun) SkillHoster.GetHoster.AutoFire -= OusideMoveControl;
                    }
                    runState=EditorGUILayout.IntSlider("运动方向，0~3=前右左后",runState,0,3);
                    */
                }
                else
                {
                    EditorGUILayout.BeginHorizontal(style);
                    EditorGUILayout.LabelField("录制受击列表：", beHitConfigFile, style);
                    if (GUILayout.Button("...", GUILayout.Width(50)))
                    {
                        beHitConfigFile = EditorUtility.OpenFilePanel("选择受击者配置文件", beHitConfigFilePath, "txt");
                        BehitUpdateBehitList();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal(style);
                    EditorGUILayout.LabelField("输出目录：", outPath, style);
                    if (GUILayout.Button("...", new GUILayoutOption[] { GUILayout.Width(50) }))
                    {
                        outPath = EditorUtility.OpenFolderPanel("选择要输出的目录", outPath, "");
                        EditorPrefs.SetString("PREF_RECD", outPath);
                    }
                    EditorGUILayout.EndHorizontal();
                    //pupetPos = EditorGUILayout.Vector3Field("受击目标相对位置", pupetPos);
                    //hitPupetAngle = EditorGUILayout.IntField("受击目标相对角度", hitPupetAngle);
                    hitWaitTime = EditorGUILayout.FloatField("受击等待时间", hitWaitTime);
                    if (GUILayout.Button("开始录制"))
                    {
                        hitNum0 = 0;
                        hitNum1 = 0;
                        BehitUpdateData();
                        BehitStartRecord();
                    }
                    needSelect = EditorGUILayout.ToggleLeft("选择文件模式", needSelect);
                    if (needSelect)
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("全选"))
                        {
                            SetSelectAll(true);
                        }
                        if (GUILayout.Button("全不选"))
                        {
                            SetSelectAll(false);
                        }
                        if (GUILayout.Button("粘贴"))
                        {
                            PasteData();
                        }
                        EditorGUILayout.EndHorizontal();
                        skillListScrollViewPos = EditorGUILayout.BeginScrollView(skillListScrollViewPos, false, true);

                        for (int i = 0; i < hitConfigList.Count; ++i)
                        {
                            EditorGUILayout.BeginHorizontal();
                            hitConfigSelectList[i] = EditorGUILayout.ToggleLeft(new GUIContent(hitConfigList[i]), hitConfigSelectList[i]);
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndScrollView();
                    }
                }
            }
            else
            {
                if (!isHitRecorder)
                {
                    EditorGUILayout.LabelField($"正在录制中：{castNum0}/{fileList.Count}", style);
                    EditorGUILayout.LabelField($"录制角色名：{recorderName}", style);
                    if (castNum0 < fileList.Count)
                    {
                        EditorGUILayout.LabelField($"录制脚本名：{Path.GetFileName(fileList[castNum0])}", style);
                    }
                }
                else
                {

                }
            }
        }
        private void PasteData()
        {
            List<string> copyBuffer = GUIUtility.systemCopyBuffer.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            for (int i=0;i<copyBuffer.Count;i++)
            {  
                   copyBuffer[i] = copyBuffer[i]+ ".bytes";
            }
            List<string> error = new List<string>();
            if (!isHitRecorder)
            {
                for (int i = 0; i < fileNameList.Count; ++i)
                {
                    if (copyBuffer.Contains(fileNameList[i],StringComparer.OrdinalIgnoreCase))
                        fileSelectList[i] = true;
                    else fileSelectList[i] = false;
                   
                }
                for(int i=0;i<copyBuffer.Count;++i)
                {
                    if (!fileNameList.Contains(copyBuffer[i], StringComparer.OrdinalIgnoreCase))
                        error.Add(copyBuffer[i]);
                }
            }
            else
            {
                for (int i = 0; i < hitConfigList.Count; ++i)
                {
                    if (copyBuffer.Contains(hitConfigList[i], StringComparer.OrdinalIgnoreCase))
                        hitConfigSelectList[i] = true;
                    else hitConfigSelectList[i] = false;
                }
                for (int i = 0; i < copyBuffer.Count; ++i)
                {
                    if (!hitConfigList.Contains(copyBuffer[i], StringComparer.OrdinalIgnoreCase))
                        error.Add(copyBuffer[i]);
                }
            }
            if(error.Count>0)
            {
                string temp= String.Join("\n", error);
                ShowNotification(new GUIContent( temp + "不存在!"), 5);
                Debug.Log(temp + "不存在!");
            }
        }

        private void InitConfig()
        {
            newConfig = new NewConfig();
            newConfig.FileSelect = new List<bool>();
            newConfig.fileSpecialList = new List<int>();
            newConfig.pupetStringFieldList = new List<string>();
            newConfig = newConfig.ReadConfig(ConfigFile);

            if (recorderName == newConfig.RecorderName)
            {
                cameraType = newConfig.CameraType;
                cameraDirection = newConfig.CameraDirection;
                outPath = newConfig.OutPath;
                t_height = newConfig.t_height;
                t_radius = newConfig.t_radius;
                t_offset = newConfig.t_offset;
            }
        }

        private void WriteConfig()
        {
            newConfig.OutPath = outPath;
            newConfig.RecorderName = recorderName;
            newConfig.CameraType = cameraType;
            newConfig.CameraDirection = cameraDirection;
            newConfig.t_height = t_height;
            newConfig.t_radius = t_radius;
            newConfig.t_offset = t_offset;
            newConfig.FileSelect.Clear();
            newConfig.fileSpecialList.Clear();
            newConfig.pupetStringFieldList.Clear();
            for (int i=0;i<fileNameList.Count;++i)
            {
                newConfig.FileSelect.Add(fileSelectList[i]);
                newConfig.fileSpecialList.Add(fileSpecialList[i]);
                newConfig.pupetStringFieldList.Add(pupetStringFieldList[i]);
            }

            newConfig.WriteConfig(ConfigFile, newConfig);
        }

        private void InitFileList()
        {
            if (fullRecorderPath != null)
            {
                fileList.Clear();
                fileNameList.Clear();
                fileSelectList.Clear();
                fileSpecialList.Clear();
                pupetStringFieldList.Clear();
                pupetSkillNameList.Clear();
                var list = Directory.GetFiles(fullRecorderPath, "*.bytes", SearchOption.TopDirectoryOnly);
                if (recorderName != newConfig.RecorderName)
                {
                    foreach (var file in list)
                    {
                        fileSelectList.Add(true);
                        fileSpecialList.Add(0);
                        pupetStringFieldList.Add("");
                    }
                }
                else
                {
                    for (int i = 0; i < list.Length; ++i)
                    {
                        if (i < newConfig.FileSelect.Count)
                        {
                            fileSelectList.Add(newConfig.FileSelect[i]);
                            fileSpecialList.Add(newConfig.fileSpecialList[i]);
                            pupetStringFieldList.Add(newConfig.pupetStringFieldList[i]);
                        }
                        else
                        {
                            fileSelectList.Add(true);
                            fileSpecialList.Add(0);
                            pupetStringFieldList.Add("");
                        }
                    }
                }
                foreach (var file in list)
                {
                    var path = Path.GetFileName(file);
                    fileNameList.Add(path);
                }
            }
        }

        private void SetCameraParam()
        {
            GameObject camera = GameObject.Find("FreeLook_skillEditor(Clone)");
            freeLook = camera?.GetComponent<CinemachineFreeLook>();
            if (freeLook != null)
            {
                freeLook.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;
                if(isAllDirection)
                    freeLook.m_XAxis.Value = CameraDirectionValue[hitDrectionCount+1];
                else
                    freeLook.m_XAxis.Value = CameraDirectionValue[cameraDirection];

                freeLook.m_Orbits = CameraTypeOrbits[cameraType];
                var transposer = freeLook.GetRig(1).GetCinemachineComponent<CinemachineComposer>();
                transposer.m_TrackedObjectOffset = CameraTypeTrackedOffsets[cameraType];
            }
            else
            {
                Debug.Log("不要在技能脚本Build时勾选SoloCamera！！！");
                //EditorUtility.DisplayDialog("设置错误", "不要在技能脚本Build时勾选SoloCamera！！！", "确定");
            }

            Vector3 cameraPos = new Vector3(camera.transform.position.x,0, camera.transform.position.z);
            for (int i = 0; i < VCTset_Plane.Length; i++) {
                VCTset_Plane[i].transform.position = cameraPos + VSPlaneInitPos[i];
            }

        }

        public void SetSelectAll(bool value)
        {
            if (!isHitRecorder)
            {
                for (int i = 0; i < fileNameList.Count; ++i)
                {
                    fileSelectList[i] = value;
                }
            }
            else
            {
                for (int i = 0; i < hitConfigList.Count; ++i)
                {
                    hitConfigSelectList[i] = value;
                }
            }
        }

        public void SetSpecialAll(int value)
        {
            for (int i = 0; i < fileNameList.Count; ++i)
            {
                fileSpecialList[i] = value;
            }
        }

        public void InitRecorderFile()
        {
            for (int i = 0; i < fileNameList.Count; ++i)
            {
                if (fileSelectList[i])
                {
                    string fileName = $"/BundleRes/SkillPackage/{skillLocation}/{fileNameList[i]}";
                    fileList.Add(fileName);
                    fileMaskDic[fileName] = (ESpecialMask)fileSpecialList[i];
                    pupetSkillNameList.Add(pupetStringFieldList[i]);
                }
            }
        }

        public void StartRecord()
        {
            if (cameraDirection == 0)
            {
                needResetCamera = true;
                hitDrectionCount = 0;
            }
           
            else hitDrectionCount = -1;
            autoCast = true;
            InitRecorderFile();
            SetMaximized(true);
            if (pupetSkillNameList[castNum0] != null)
                SkillHoster.MobSkillName = pupetSkillNameList[castNum0];
            RecorderSettings.OutputDir = outPath;
            SkillHoster.GetHoster.showGUI = false;
            SkillHoster.GetHoster.AutoFire += AutoSkillCaster;
            OverdrawMonitor.Instance.OnSkillEnd += MonitorParticleCount;
            if (isChangeCharacter && ChangedPresentID != 0)
            {
                SkillHoster.GetHoster.EntityDic[1].TransformSkin(ChangedPresentID);
            }
            WriteConfig();
        }

        private void AutoSkillCaster()
        {
            MonitorAnimatorState();
            if (OverdrawMonitor.Instance.GetSfxCount() < 0)
                MonitorParticleCount();
            if (needDeSmoke) {
                FindSmoke();
            }
            if (autoCast && Application.isPlaying)
            {
                XTimerMgr.singleton.Update(Time.deltaTime);
                if (!skillPlaying)
                {
                    OverdrawMonitor.Instance.OnSkillEnd -= MonitorParticleCount;
                    XEcsScriptEntity.EndSkill(SkillHoster.PlayerIndex);
                    timeInterval += Time.deltaTime;
                    if (timeInterval > hitWaitTime)
                    {
                        timeInterval = 0f;
                        if (needResetCamera)
                        {
                            SetCameraParam();
                        }
                              
                        
                        if(isAllDirection)
                            RecorderSettings.OutputName = $"{Path.GetFileNameWithoutExtension(fileList[castNum0])}";
                        else
                            RecorderSettings.OutputName = $"{Path.GetFileNameWithoutExtension(fileList[castNum0])}_{CameraDirectionStr[cameraDirection]}";

                        if (hitDrectionCount == 0 || !isAllDirection)
                        {
                            recorderController.PrepareRecording();
                            recorderController.StartRecording();
                        }

                        EndReset();

                        if (OverdrawMonitor.isOn)
                        {
                            //MonitorAnimatorState();
                            OverdrawMonitor.Instance.StartObserveProfile(1, fileList[castNum0]);
                        }
                        skillPlaying = true;
                        XTimerMgr.singleton.SetTimer(0.5f, FireSkill, null);
                        if (isAllDirection)
                            hitDrectionCount++;
                    }
                }
            }
        }

        private void FireSkill(object o)
        {
            (skillEditor.CurrentGraph as SkillGraph).OpenData($"{Application.dataPath}{fileList[castNum0]}");
           
            var specialMask = fileMaskDic[fileList[castNum0]];
            if (specialMask.HasFlag(ESpecialMask.NeedHit))
            {
                if (!String.IsNullOrEmpty(SkillHoster.MobSkillName))
                {
                    const ulong index = 10101;
                    string[] mobName = SkillHoster.MobSkillName.Split(',');
                    if (mobName.Length > 0)
                    {
                        string[] temp = mobName[0].Split('|');
                        if (temp.Length >= 2)
                        {
                            int presentID = int.Parse(temp[0]);
                            SkillHoster.GetHoster.CreatePuppet(presentID, 0, playerObj.transform.position.x + pupetPos.x, playerObj.transform.position.y + pupetPos.y, playerObj.transform.position.z + pupetPos.z, index); // 10101是专用于这个功能的特定的数字，临时做法，避免冲突
                            if (presentID == EntityDic[index].presentData.PresentID)
                            {
                                SkillHoster.GetHoster.FireSkill("/BundleRes/SkillPackage/" + EntityDic[index].presentData.SkillLocation + temp[1] + ".bytes", index);
                            }
                        }
                    }
                }
                else
                {
                    SkillHoster.GetHoster.CreatePuppet(10017, 180, playerObj.transform.position.x + pupetPos.x, playerObj.transform.position.y + pupetPos.y, playerObj.transform.position.z + pupetPos.z);
                }
            }
            SkillHoster.aiMode = specialMask.HasFlag(ESpecialMask.AIMode);
            SkillHoster.GetHoster.FireSkillForRecord(fileList[castNum0], SkillHoster.PlayerIndex);
            if (!isAllDirection )
            {
                castNum0++;
            }

            OverdrawMonitor.Instance.OnSkillEnd += MonitorParticleCount;
        }

        public void SkillEnd()
        {
            XTimerMgr.singleton.SetTimer(0.2f, SkillEnd, null);
        }

        private void SkillEnd(object o)
        {
            var frameIndex = recorderController.GetRecordingSessions().FirstOrDefault()?.frameIndex;
            if (frameIndex.HasValue && frameIndex / 30.0f - 0.3f < (skillEditor.CurrentGraph as SkillGraph).Length)
            {
                XTimerMgr.singleton.SetTimer(0.2f, SkillEnd, null);
                return;
            }
            skillPlaying = false;
            if(hitDrectionCount==4)
            {
                recorderController.StopRecording();
                hitDrectionCount = 0;
                castNum0++;
            }
            if(!isAllDirection)
            {
                recorderController.StopRecording();
            }
            
            if (castNum0 > fileList.Count - 1)
            {
                autoCast = false;
                SkillHoster.GetHoster.AutoFire -= AutoSkillCaster;
            }
            else if (pupetSkillNameList[castNum0] != null)
                SkillHoster.MobSkillName = pupetSkillNameList[castNum0];
            if (!autoCast)
            {
                RecorderSettings.OutputDir = "";
                RecorderSettings.OutputName = "";
                OverdrawMonitor.Instance.OnSkillEnd -= MonitorParticleCount;
                Close();
                SetMaximized(false);
                Debug.Log("录制完成");
            }
        }

        public void EndReset()
        {
            SkillHoster.GetHoster.CheckRemoveEntity();
        }

        private void BehitInitConfig()
        {
            if (File.Exists(ConfigFile))
                configInfo = File.ReadAllText(ConfigFile);
            newConfig = new NewConfig();
            outPath = newConfig.OutPath;
        }

        public void BehitStartRecord()
        {
            BehitCreateCamera();
            XTimerMgr.singleton.SetTimer(1f, BehitStartRecord, null);
        }

        public void BehitStartRecord(object o)
        {
            autoCast = true;
            SkillHoster.GetHoster.showGUI = false;
            SkillHoster.GetHoster.AutoFire += BehitAutoSkillCaster;
            OverdrawMonitor.Instance.OnSkillEnd += BehitSkillEnd;
        }

        //录制前操作
        public void BehitAutoSkillCaster()
        {
            if (autoCast && Application.isPlaying)
            {
                XTimerMgr.singleton.Update(Time.deltaTime);
                if (!OverdrawMonitor.Instance.isObserving)
                {
                    OverdrawMonitor.Instance.OnSkillEnd -= BehitSkillEnd;
                    XEcsScriptEntity.EndSkill(SkillHoster.PlayerIndex);
                    timeInterval += Time.deltaTime;
                    if (timeInterval > 0.2f && !skillPlaying)
                    {
                        timeInterval = 0f;
                        if (needResetCamera)
                        {
                            SetCameraParam();
                        }
                        if (hitNum0 == 0)  //换受击脚本
                        {
                            if (SkillHoster.GetHoster != null)
                            {
                                //CFUtilPoolLib.XTimerMgr.singleton.KillTimer();
                                //hitPresentIDList[hitConfigIndexList[hitNum0]]
                                SkillHoster.GetHoster.GetEntity();
                                SkillHoster.GetHoster.EntityDic[1].TransformSkin(hitPresentIDList[hitConfigIndexList[hitNum1]]);
                                Debug.Log(hitPresentIDList[hitConfigIndexList[hitNum1]]+"/"+hitConfigIndexList[hitNum1]);
                                //TransformSkinToken = CFUtilPoolLib.XTimerMgr.singleton.SetTimer(HosterData.TransformSkinLifeTime,
                                //    (object o) => { VirtualSkill.SkillHoster.GetHoster.EntityDic[1].TransformSkin(GetRoot.GetConfigData<XConfigData>().PresentID); },
                                //    null);
                            }
                        }
                        if (hitDrectionCount == 0)
                        {
                            recorderController.PrepareRecording();
                            string beHitName = beHitPresentNameList[hitNum0];
                            RecorderSettings.OutputDir = $"{outPath}/{beHitName}";
                            RecorderSettings.OutputName = $"{beHitName}{hitSkillName}";
                            EditorPrefs.SetString("PREF_RECD", $"{outPath}/{beHitName}");
                            recorderController.StartRecording();
                        }
                        EndReset();
                        playerObj = GameObject.Find("Player");
                        startPos = playerObj.transform.position;
                        startRotate = playerObj.transform.rotation;
                        SkillHoster.GetHoster.CreatePuppet(beHitPresentIDList[hitNum0], hitPupetAngle, playerObj.transform.position.x + pupetPos.x *
                            beHitRadiusList[hitNum0], playerObj.transform.position.y + pupetPos.y * beHitRadiusList[hitNum0], playerObj.transform.position.z
                            + pupetPos.z * beHitRadiusList[hitNum0]);
                        BehitChangeCamera();
                        hitDrectionCount++;
                        if (OverdrawMonitor.isOn)
                        {
                            OverdrawMonitor.Instance.StartObserveProfile(1, hitSkill);
                        }
                        skillPlaying = true;
                        XTimerMgr.singleton.SetTimer(0.5f, BehitFireSkill, null);
                    }
                }
            }
        }

        private void BehitFireSkill(object o)
        {
            SkillHoster.GetHoster.FireSkillForRecord(hitSkill, SkillHoster.PlayerIndex);
            OverdrawMonitor.Instance.OnSkillEnd += BehitSkillEnd;
        }

        private void BehitChangeCamera()
        {
            var cntg = hitGroupCameraGO.GetComponentInChildren<CinemachineTargetGroup>();
            if (cntg != null)
            {
                cntg.m_Targets[0].target = SkillHoster.GetHoster.EntityDic[SkillHoster.PlayerIndex].posXZ.transform;
                cntg.m_Targets[1].target = SkillHoster.GetHoster.EntityDic[SkillHoster.GetHoster.Target].posXZ.transform;
                cntg.m_Targets[0].weight = 1f;
                cntg.m_Targets[1].weight = 5f;
            }
            var cf = hitGroupCameraGO.GetComponent<CinemachineFreeLook>();
            cf.m_XAxis.Value = CameraDirectionValue[hitDrectionCount+1];
        }

        //录制后操作
        private void BehitSkillEnd()
        {
            XTimerMgr.singleton.SetTimer(0.2f, BehitSkillEnd, null);
        }

        private void BehitSkillEnd(object o)
        {
            skillPlaying = false;
            if (hitDrectionCount == 4)
            {
                recorderController.StopRecording();
                hitDrectionCount = 0;
                hitNum0++;
            }
            if (hitNum0 > beHitPresentIDList.Count - 1)
            {
                hitNum1++;
                hitNum0 = 0;
            }
            if (hitNum1 > hitConfigIndexList.Count - 1)
            {
                autoCast = false;
                SkillHoster.GetHoster.AutoFire -= BehitAutoSkillCaster;
            }
            else
            {
                hitSkill = $"/BundleRes/SkillPackage/Role_hit/{hitConfigList[hitConfigIndexList[hitNum1]]}.bytes";
                hitSkillName = hitConfigList[hitConfigIndexList[hitNum1]];
                pupetPos = hitPupetPosList[hitConfigIndexList[hitNum1]];
                hitPupetAngle = hitPupetAngleList[hitConfigIndexList[hitNum1]];
            }
            
            if (!autoCast)
            {
                RecorderSettings.OutputDir = "";
                RecorderSettings.OutputName = "";
                OverdrawMonitor.Instance.OnSkillEnd -= BehitSkillEnd;
                Close();
                Debug.Log("录制完成");
            }
        }

        //初始化各种参数，hitconfig,hitPresentIDList,hitPupetAngleList,hitPupetPosList在读取后就不再更改
        private void BehitInitData()
        {
            hitConfigList.Clear();
            hitPresentIDList.Clear();
            hitPupetPosList.Clear();
            hitPupetAngleList.Clear();
            hitConfigIndexList.Clear();
            hitConfigSelectList.Clear();

            if (File.Exists(hitConfigFile))
                hitConfigInfo = File.ReadAllText(hitConfigFile);


            //hit脚本配置txt
            var infoLines = hitConfigInfo.Trim().Split(new string[] { "\r\n" }, StringSplitOptions.None);
            foreach (var line in infoLines)
            {
                var items = line.Trim().Split('\t');
                hitConfigList.Add(items[0]);
                hitPresentIDList.Add(int.Parse(items[1]));
                string[] strs = items[3].Split(',');
                hitPupetPosList.Add(new Vector3(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2])));
                hitPupetAngleList.Add(int.Parse(items[4]));
                hitConfigSelectList.Add(true);
                //Debug.Log(items[0]+"/"+ items[1]);
            }
        }

        //更新受击人物脚本
        private void BehitUpdateBehitList()
        {
            beHitPresentIDList.Clear();
            beHitRadiusList.Clear();
            beHitPresentNameList.Clear();
            if (File.Exists(beHitConfigFile))
                beHitConfigInfo = File.ReadAllText(beHitConfigFile);

            //受击人物配置txt
            var infoLines = beHitConfigInfo.Trim().Split(new string[] { "\r\n" }, StringSplitOptions.None);
            foreach (var line in infoLines)
            {
                var items = line.Trim().Split('\t');
                beHitPresentIDList.Add(int.Parse(items[0]));
                beHitRadiusList.Add(float.Parse(items[1]));
                beHitPresentNameList.Add(items[2]);
            }
        }

        //点击开始录制的时候的初始化操作（更新受击脚本和受击人物）
        private void BehitUpdateData()
        {
            BehitUpdateBehitList();

            hitConfigIndexList.Clear();

            for (int i = 0; i < hitConfigList.Count; ++i)
            {
                if (hitConfigSelectList[i])
                {
                    string fileName = $"{hitConfigList[i]}";
                    hitConfigIndexList.Add(i);
                    //Debug.Log(fileName);
                }
            }

            hitSkill = $"/BundleRes/SkillPackage/Role_hit/{hitConfigList[hitConfigIndexList[hitNum1]]}.bytes";
            hitSkillName = hitConfigList[hitConfigIndexList[hitNum1]];
            pupetPos = hitPupetPosList[hitConfigIndexList[hitNum1]];
            hitPupetAngle=hitPupetAngleList[hitConfigIndexList[hitNum1]];
        }
        private void BehitCreateCamera()
        {
            var freeLook = AssetDatabase.LoadAssetAtPath("Assets/BundleRes/Prefabs/Cinemachine/Jianxiu/Jianxiu2.prefab", typeof(GameObject)) as GameObject;
            hitGroupCameraGO = GameObject.Instantiate<GameObject>(freeLook, null);
            SkillHoster.GetHoster.CreatePuppet(beHitPresentIDList[hitNum0], hitPupetAngle, playerObj.transform.position.x + pupetPos.x *
                            beHitRadiusList[hitNum0], playerObj.transform.position.y + pupetPos.y * beHitRadiusList[hitNum0], playerObj.transform.position.z
                            + pupetPos.z * beHitRadiusList[hitNum0]);
            BehitChangeCamera();
        }

        /// <summary>
        /// 在SkillHoster.GetHoster.AutoFire += OusideMoveControl;前先赋值
        /// </summary>
        private void PreOMC()
        {
            Target = SkillHoster.GetHoster.Target;//下面这三个，控制运动的，需要在开始运动前重新赋值
            cameraComponent = SkillHoster.GetHoster.cameraComponent;//只在onEnable里赋值会导致开启窗口而停止程序时
            EntityDic = SkillHoster.GetHoster.EntityDic;//重新启动游戏后bug
            outsideCtrlDir = 0;
        }

        private void OusideMoveControl()
        {
            //Debug.Log("OusideMoveControl");

            cameraComponent = SkillHoster.GetHoster.cameraComponent;
            forward = cameraComponent.CameraObject.transform.forward;
            right = cameraComponent.CameraObject.transform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();
            int nv = runDir[outsideCtrlDir][0], nh = runDir[outsideCtrlDir][1];

            if (isRun)
                XEcsScriptEntity.OnMove(
                targetControl ? Target : PlayerIndex,
                XCommon.singleton.AngleToFloat(nv * forward + nh * right),
                EntityDic[targetControl ? Target : PlayerIndex].obj.transform.position);

            //else if (isRun ^ preOutsideCtrlRun)
            else if (!isRun)//不做本帧与上一帧亦或了，直接用本帧isRun做判断
                XEcsScriptEntity.OnStop(
                targetControl ? Target : PlayerIndex,
                XCommon.singleton.AngleToFloat(nv * forward + nh * right),
                EntityDic[targetControl ? Target : PlayerIndex].obj.transform.position);
        }



        private void RecordIdle()//开始录制静止状态
        {
            Debug.Log("开始录制idle");
            RecorderSettings.OutputDir = outPath;
            RecorderSettings.OutputName = recorderName + "_idle";
            recorderController.PrepareRecording();
            recorderController.StartRecording();
            IdleRunState = "正在录制idle。请不要移动角色或修改录制工具，否则会发生意外问题。等待至“结束录制”出现";
            IdleDurationChangeDirectionFunc(new object());
        }
        private void IdleDurationChangeDirectionFunc(object o)
        {
            if (hitDrectionCount < 4)
            {
                SetCameraParam();
                XTimerMgr.singleton.SetTimer(cameraChangeDirectionDurationTime, IdleDurationChangeDirectionFunc, null);
            }
            if (hitDrectionCount >= 4 || !isAllDirection)//结束录制后
            {
                recorderController.StopRecording();
                Debug.Log("StopRecording  ");
                IdleRunState = "结束站立录制";
                PreOMC();
                isRun = true;//先往前跑，让跑动方向向前
                SkillHoster.GetHoster.AutoFire += OusideMoveControl;
                hitDrectionCount = 0;
                XTimerMgr.singleton.SetTimer(1, PreRecordRun, null);
                return;
            }
            hitDrectionCount++;
        }
        private void PreRecordRun(object o)//在开始录制跑动前，先让跑动停下来，避免录入停止的动作
        {
            isRun = false;
            XTimerMgr.singleton.SetTimer(1, RecordRun, null);
        }
        /// <summary>
        /// 开始跑动
        /// </summary>
        /// <param name="o"></param>
        private void RecordRun(object o)//开始录制跑步状态
        {
            //isRun = false;
            Debug.Log("开始录制run");
            hitDrectionCount = 0;
            RecorderSettings.OutputDir = outPath;
            RecorderSettings.OutputName = recorderName + "_run";
            recorderController.PrepareRecording();
            recorderController.StartRecording();
            IdleRunState = "正在录制run。请不要移动角色或修改录制工具，否则会发生意外问题。等待至“结束录制”出现";
            outsideCtrlDir = -1;
            RunStopDurationFunc(new object());
        }
        private void RunDurationChangeDirectionFunc(object o)
        {
            if (outsideCtrlDir >= 4 || !isAllDirection)
            {
                SkillHoster.GetHoster.AutoFire -= OusideMoveControl;
                //SkillHoster.outsideCtrlRun = false;
                recorderController.StopRecording();
                Debug.Log("结束录制");
                IdleRunState = "结束录制";
                return;
            }
            isRun = true;
            //outsideCtrlDir = hitDrectionCount;
            XTimerMgr.singleton.SetTimer(cameraChangeDirectionDurationTime, RunStopDurationFunc, null);//进入停止跑动的计时器
        }
        private void RunStopDurationFunc(object o)//停止跑动阶段
        {
            isRun = false;
            
            outsideCtrlDir++;//跑动方向计数器++
            XTimerMgr.singleton.SetTimer(runStopChangeDirectionDurationTime, RunDurationChangeDirectionFunc, null);
        }

        private void SetMaximized(bool max)
        {
            var windows = (UnityEditor.EditorWindow[])Resources.FindObjectsOfTypeAll(typeof(UnityEditor.EditorWindow));
            foreach (var window in windows)
            {
                if (window != null && window.GetType().FullName == "UnityEditor.GameView")
                {
                    window.maximized = max;
                    break;
                }
            }

            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
        private void FindSmoke(string[] SmokeStrList= null)//
        //private List<GameObject> FindSmoke(string SmokeStr="smoke")//
        {
            if (SmokeStrList == null) SmokeStrList = defaultSmokeStrList;
            GameObject[] AllGO = GameObject.FindObjectsOfType<GameObject>();
            for (int i = 0; i<AllGO.Length; i++) {
                for (int j = 0; j < SmokeStrList.Count(); j++)
                {
                    if (AllGO[i].name.ToLower().Contains(SmokeStrList[j]))
                    {
                        //ret.Add(AllGO[i]);
                        Debug.Log("AllGO[i].name  " + AllGO[i].name);
                        AllGO[i].SetActive(false);
                    }
                }
            }
            //return ret;
        }

        string LastFrameState = "EMPTY";
        private void MonitorAnimatorState()
        {
            playerObj = GameObject.Find("Player");
            Animator a = playerObj.GetComponent<Animator>();
            AnimatorClipInfo[] AnimString = a.GetCurrentAnimatorClipInfo(1);         //xanimator的第二级是attacklayer
            string State;      
            if (a.GetCurrentAnimatorClipInfoCount(1) == 0)
                State = "EMPTY";           
            else    
                State = AnimString[0].clip.name;
            if (LastFrameState != "EMPTY" && State == "EMPTY")
            {
                //DoubleCondition(2);
                if(DoubleCondition(2))
                    XTimerMgr.singleton.SetTimer(recExtraDelay, SkillEnd, null);
                //OverdrawMonitor.Instance.OnSkillEnd?.Invoke();
                //SkillHoster.GetHoster.OnSkillEnd(PlayerIndex);                 
            }
            LastFrameState = State;
        }

        private void MonitorParticleCount()
        {
            //DoubleCondition(1);
            if(DoubleCondition(1))
                XTimerMgr.singleton.SetTimer(recExtraDelay, SkillEnd, null);
        }

        private int LastSatisefiedCondition = 0;
        public bool DoubleCondition(int ConditionIndex)         //1是特效判断，2是动作判断
        {
            bool Result = false;
            if(LastSatisefiedCondition == 1 && ConditionIndex == 2 || LastSatisefiedCondition == 2 && ConditionIndex == 1)
            {
                Result = true;
                LastSatisefiedCondition = 0;
            }
            else
                LastSatisefiedCondition = ConditionIndex;
            //XTimerMgr.singleton.SetTimer(recExtraDelay, SkillEnd, null);
            return Result;
        }

        private void PresentationScaleTo1() {
            //PresentationFile
            string[] PresentationLine=new string[1];
            try
            {
                PresentationLine = File.ReadAllLines(PresentationFile);
            }
            catch {
                Debug.Log("XEntityPresentation打开失败，检查是否已打开该文件");
            }
            for (int i = 2; i < PresentationLine.Length; i++) {
            //for (int i = 2; i < 3; i++) {
                //string SinglePresentationStr = PresentationStr[i];
                string[] PresentationElement= PresentationLine[i].Split('\t');
                /*
                Debug.Log(PresentationLine[i]);
                for (int j = 0; j < PresentationLine[i].Length; j++) {
                    Debug.Log(PresentationElement[j]);
                }
                */
                PresentationElement[17] = "1";

                /* 找不到受击脚本脚本就报错的问题修复了，这段应该暂时不需要了
                string BehitStr = PresentationElement[69];
                if (BehitStr.Length > 10) { 
                    //为了不用regex所以只能遍历了
                    int BehitStrPos = 0, BehitStrEndPos = BehitStr.Length-1;
                    for (BehitStrPos = 0; BehitStrPos < BehitStr.Length-10; BehitStrPos++)
                    {
                        if (BehitStr.Substring(BehitStrPos, 2) == "6=")
                        {
                            for (BehitStrEndPos = BehitStrPos; BehitStrEndPos < BehitStr.Length; BehitStrEndPos++)
                            {
                                if (BehitStr.Substring(BehitStrEndPos, 1) == "|")break;
                            }
                            break;
                        }
                    }
                    if (BehitStr.Substring(BehitStrPos, 2) == "6=")
                    {
                        Debug.Log(BehitStr.Substring(BehitStrPos, BehitStrEndPos-BehitStrPos));
                        string BehitStrNew = BehitStr.Substring(0, BehitStrPos - 1) + BehitStr.Substring(BehitStrEndPos, BehitStr.Length - BehitStrEndPos);
                        PresentationElement[69] = BehitStrNew;
                        //Debug.Log(BehitStrNew);
                    }
                }
                */

                //Debug.Log(PresentationLine[i]);
                PresentationLine[i] = string.Join("\t", PresentationElement);
                //Debug.Log(PresentationLine[i]);
            }
            
            try
            {
                File.WriteAllLines(PresentationFile, PresentationLine, Encoding.GetEncoding("Unicode"));
                Debug.Log("XEntityPresentation所有角色体型变为1");
            }
            catch
            {
                Debug.Log("XEntityPresentation写入失败，检查是否已打开该文件");
            }
            
        }

        /// <summary>仅选择当前脚本名，并返回脚本名</summary>
        private string SelectCurrentGraphName() {
            SetSelectAll(false);
            SkillGraph skillGraph = GetWindow<SkillEditor>().CurrentGraph as SkillGraph;
            string CurrentGraphName = skillGraph.configData.Name + ".bytes";
            Debug.Log("当前脚本名"+ CurrentGraphName);

            for (int i = 0; i < fileSelectList.Count; i++)
            {
                if (CurrentGraphName.Equals(fileNameList[i]))
                {
                    fileSelectList[i] = true;
                    break;
                }
            }
            return CurrentGraphName;
        }
    }

}
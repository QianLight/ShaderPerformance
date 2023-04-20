using Devops.Core;
using Devops.Performance;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class PTestManager : Singleton<PTestManager>
{
    [HideInInspector]
    public long outTimer = 1000;
    private int inTimer = 0;

    public static GameObject PTestobj;

    public GameObject caseObj;
    //public GameObject caseObj;

    public PTestScriptableObject ptestObject;

    [HideInInspector]
    public static List<Type> types = new List<Type>();
    [HideInInspector]
    public List<CaseItemData> caseNames = new List<CaseItemData>();
    [HideInInspector]
    public List<CaseItemData> caseSelect = new List<CaseItemData>();


    private Coroutine timerCoroutine;
    private Coroutine caseCoroutine;

    public Action<CaseItemData, bool> caseStart;
    public Action<CaseItemData, bool> caseEnd;
    public Action<string> caseFailed;

    [HideInInspector]
    public bool Testing = false;
    [HideInInspector]
    public bool TestFinished = false;

    [HideInInspector]
    public bool startprofiler = false;

    [HideInInspector]
    public string buildRunId = "";
    [HideInInspector]
    public string buildTaskId = "";
    [HideInInspector]
    public string jobId = "";
    [HideInInspector]
    public string versionId = "";
    [HideInInspector]
    public string planName = "";
    private string deviceId = "";

    [HideInInspector]
    public long GameStartTime = 0;
    [HideInInspector]
    public ICaseState InTestCaseItem = null;
    [HideInInspector]
    public DevopsTestProfilerData profilerData = null;
    // Start is called before the first frame update
    IEnumerator Start()
    {


        //Encoding utf = Encoding.GetEncoding("UTF-8");
        //string filepath = PTestNode.FilePath("jsontest.json");
        //string InfoConfig = File.ReadAllText(filepath, utf);
        //PTestNetManager.Instance.SetCSVDatas(InfoConfig);
        //string json = PTestNode.ReadCSVStringToJson(PTestNodeManager.Instance.extend_list["AutoTest_GuideProcess.csv"]);
        //string file = PTestNode.FilePath("AutoTest_GuideProcess.csv");
        //string json = PTestNode.ReadCSVToJson(file);
        //Debug.Log(json);

        yield return Init();
    }



    [RuntimeInitializeOnLoadMethod]
    public static void RegisterToCore()
    {
        Devops.Core.DevopsUI.RegisterFunction(StaticDisplaySwitch, 0, "功能测试");
        if (Devops.Core.DevopsUI.Instance() != null)
            InitGameObj();
    }

    public static void StaticDisplaySwitch()
    {
        InitGameObj();
        PTestUIManager.Instance.testUI.OnBtnUIShowClick();
        //PerformancePanel.Instance().SetEnable(true);
    }

    public static void InitGameObj()
    {
        if (PTestobj == null)
        {
            var obj = Resources.Load("Prefab/PTestManager");
            PTestobj = GameObject.Instantiate(obj) as GameObject;
        }

    }

    public IEnumerator Init()
    {

        Debug.Log("PTestManager  Init");
        GameStartTime = PTestUtils.GetTimeStamp();
        InitConfig();
        GetSubClassType(typen("BP_Base"));
        AddTestComponent();
        yield return new WaitForSeconds(2);
        //GetConfig();
        PTestUIManager.Instance.Init();
        PTestNetManager.Instance.Init();
        yield return new WaitForEndOfFrame();
    }


    public void InitConfig()
    {
#if UNITY_EDITOR

#else
#if UNITY_STANDALONE_WIN
        List<string> commandLineArgs = new List<string>(Environment.GetCommandLineArgs());
        //取索引为>=1的部分 index=0的值为文件路径信息 
        if (commandLineArgs.Count > 1)
        {
            string port = commandLineArgs[1];
            PTestConfig.Port = int.Parse(port);
        }

#endif

#endif
        PTestConfig.TestLayer = PlayerPrefs.GetInt(PTestDataConfig.PTestLayer, PTestConfig.TestLayer);
        //PTestNetConfig.url = PlayerPrefs.GetString(PTestDataConfig.PTestNetUrl, PTestNetConfig.url);
    }


    public void StartCases(bool startprofiler)
    {
        Testing = true;
        TestFinished = false;

        PTestDataManager.Instance.ClearCaseResult();
        PTestUIManager.Instance.HidePlane();
        PTestUIManager.Instance.StartTest();
        PTestManager.Instance.caseFailed = null;
        PTestManager.Instance.caseStart = null;
        PTestManager.Instance.caseEnd = null;
        PTestUIManager.Instance.testUI.GetPlaneTesting().InitCase();
        PTestUIManager.Instance.testUI.planeTestingMini.InitCase();

        SocketHelper.GetInstance().SendMessage(SocketHelper.SocketStatus.TestStart.ToString());
        caseCoroutine = StartCoroutine(new CatchableEnumerator(CasesDriver(), (e) =>
        {
            Debug.LogError(e);
            PTestDataManager.Instance.SetErrorSteps(e.TargetSite.DeclaringType.ToString(), e.ToString(), false);
            caseFailed?.Invoke(e.ToString());
        }));
        Coroutine coroutine = StartCoroutine(OutTimerChecker());
    }

    public IEnumerator OutTimerChecker()
    {
        while (inTimer < outTimer)
        {
            inTimer++;
            yield return new WaitForSecondsRealtime(1);
        }
        StopTesting();
        PTestDataManager.Instance.SetTimeOutSteps("Time Out", PTestData.key + "Case Run Time Out", false);
        CaseFinish();
    }

    public void StopTesting()
    {
        if (caseCoroutine != null)
            StopCoroutine(caseCoroutine);
        if (InTestCaseItem!=null&&InTestCaseItem.state_Enum == State_Enum.Running)
        {
            InTestCaseItem.state_Enum = State_Enum.Finished;
            InTestCaseItem.OnExit();

        }
        Testing = false;
        //SocketHelper.GetInstance().SendMessage(SocketHelper.SocketStatus.TestEnd.ToString());
    }

    public void CaseFinish()
    {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        SendData();

        SocketHelper.GetInstance().SendMessage(SocketHelper.SocketStatus.TestFinish.ToString());

        PTestDataManager.Instance.ClearCaseResult();
        Testing = false;
        PTestUIManager.Instance.EndTest();

    }
    public void SendData()
    {
        TestData json = PTestDataManager.Instance.CaseResultToJson();
        SocketData testData = new SocketData();
        testData.result = json;
        string result = "&result&&" + JsonUtility.ToJson(testData);
        Debug.Log(result);

        SocketHelper.GetInstance().SendMessage(result);
    }

    public void AddTestComponent()
    {
        foreach (Type type in types)
        {
            //object obj = caseObj.AddComponent(type);

            caseNames.Add(GetSubClassNames(type));

        }
    }

    public void LoadCaseList(CaseNetList NetList)
    {
        this.buildRunId = NetList.buildRunId;
        this.buildTaskId = NetList.buildTaskId;
        this.jobId = NetList.jobId;
        this.versionId = NetList.versionId;
        this.deviceId = NetList.deviceId;
        foreach (CaseNetItemData caseNetItem in NetList.caseList)
        {

            bool haveCase = false;

            foreach (CaseItemData caseItem in caseNames)
            {
                if (caseItem.type.Name == caseNetItem.Type)
                {
                    caseItem.CaseParam0 = caseNetItem.CaseParam0;
                    caseItem.CaseParam1 = caseNetItem.CaseParam1;
                    caseItem.CaseParam2 = caseNetItem.CaseParam2;
                    caseItem.CaseParam3 = caseNetItem.CaseParam3;
                    caseItem.CaseParam4 = caseNetItem.CaseParam4;

                    caseSelect.Add(caseItem);
                    haveCase = true;
                }

            }
            if (!haveCase)
            {
                CaseItemData missItem = new CaseItemData();
                missItem.missType = caseNetItem.Type;
                missItem.isMissed = true;
            }

        }

    }


    public void SetUpCase(CaseItemData caseItem)
    {
        //BPTestNode caseClass = caseObj.GetComponent(caseItem.type) as BPTestNode;
        PTestData.key = caseItem.type.Name;
        long start = PTestUtils.GetTimeStamp();
        PTestData.start = start;
        PTestDataManager.Instance.CreateCaseData(PTestData.key);
        //caseItem.method.Invoke(caseItem.obj, new object[] { });
        //return caseClass.StartCase(type);
    }

    public IEnumerator CasesDriver()
    {

        List<ICaseState> caseStates = new List<ICaseState>();
        for (int i = 0; i < caseSelect.Count; i++)
        {
            ICaseState caseState = new ICaseState();
            caseState.caseItemData = caseSelect[i];
            caseStates.Add(caseState);
        }

        Debug.Log("PTestManager StartTest :" + caseStates.Count);

        for (int i = 0; i < caseStates.Count; i++)
        {
            ICaseState caseState = caseStates[i];
            CaseItemData caseItem = caseSelect[i];

            yield return ItemCaseDriver(caseState, caseItem);
            StopProfiler();
        }
        TestFinished = true;
        yield return new WaitForSecondsRealtime(2);
        CaseFinish();
    }

    public IEnumerator ItemCaseDriver(ICaseState caseState, CaseItemData caseItem)
    {
        Debug.Log("PTestManager StartTest Method :" + caseItem.type);

        if (caseItem.isMissed)
        {
            PTestData.key = caseItem.type.Name;
            PTestDataManager.Instance.CreateCaseData(caseItem.missType);
            PTestDataManager.Instance.SetErrorSteps(caseItem.missType, "Missed Case", false);
        }
        else {
            InTestCaseItem = caseState;
            ProfilerDataAddCase();

            inTimer = 0;

            caseStart?.Invoke(caseItem, true);

            caseState.OnEnter(() =>
            {
                SetUpCase(caseItem);
            });


            while (caseState.state_Enum == State_Enum.Running)
            {

                caseState.OnUpdate();

                yield return new WaitForSecondsRealtime(1);
            }
            //Destroy(caseState.caseClass);
            if (PTestDataManager.Instance.CaseResults.ContainsKey(PTestData.key) && PTestData.key != null)
            {
                CaseData lastData = PTestDataManager.Instance.CaseResults[PTestData.key];
                lastData.endTime = PTestUtils.GetTimeStamp();
            }

            caseState.OnExit();


            caseEnd?.Invoke(caseItem, true);
            InTestCaseItem = null;
        }

    }


    public void GetSubClassType(Type parentType)
    {
        if (parentType == null)
            return;
        var assemblyAllTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(baseType => baseType.IsSubclassOf(parentType));
        foreach (var itemType in assemblyAllTypes)//遍历所有类型进行查找
        {
            if (itemType.Name.Contains(ptestObject.caseFilter))
            {

                var baseType = itemType.BaseType;//获取元素类型的基类
                if (baseType != null)//如果有基类
                {
                    if (baseType.Name == parentType.Name)//如果基类就是给定的父类
                    {
                        types.Add(itemType);
                    }
                }
            }
                //types.Add(itemType);
        }
    }


    public static Type typen(string typeName)
    {
        Type type = null;
        Assembly[] assemblyArray = AppDomain.CurrentDomain.GetAssemblies();
        int assemblyArrayLength = assemblyArray.Length;
        for (int i = 0; i < assemblyArrayLength; ++i)
        {
            type = assemblyArray[i].GetType(typeName);
            if (type != null)
            {
                return type;
            }
        }

        for (int i = 0; (i < assemblyArrayLength); ++i)
        {
            Type[] typeArray = assemblyArray[i].GetTypes();
            int typeArrayLength = typeArray.Length;
            for (int j = 0; j < typeArrayLength; ++j)
            {
                if (typeArray[j].Name.Equals(typeName))
                {
                    return typeArray[j];
                }
            }
        }
        return type;
    }

    public CaseItemData GetSubClassNames(Type parentType)
    {

        return new CaseItemData { type = parentType };//获取所有子类类型的名称
    }


    public void SendTag(string tag)
    {
        SocketHelper.GetInstance().SendMessage("$TestTag$:" + tag);
        if (startprofiler)
            DevopsProfilerClient.Instance().SetTagData(tag);

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            //PTestUtils.ClickOverGUI();
        }
    }

    public void ProfilerDataAddCase()
    {
        if (profilerData != null && InTestCaseItem != null)
        {
            profilerData.caseNames.Add(InTestCaseItem.caseItemData.type.Name);
            profilerData.caseCount = profilerData.caseNames.Count;
        }
    }

    public bool isProfilerStart()
    {
        return DevopsProfilerClient.Instance().IsOpen();
    }

    public void StartProfiler(string[] profilerTag, int time = 1000, bool snap = false)
    {
        if (startprofiler)
        {
            //DevopsProfilerClient.Instance().SetObjectSnap(snap);
            DevopsProfilerClient.Instance().SetInterval(time);
            DevopsProfilerClient.Instance().ConnectServer(true, buildRunId, deviceId, planName, profilerTag);
            DevopsProfilerClient.Instance().SetTagData(tag);
            Debug.Log("PTestManager StartProfiler");
            profilerData = new DevopsTestProfilerData();
            profilerData.time[0] = Convert.ToInt64(Time.realtimeSinceStartup);
            ProfilerDataAddCase();
        }
    }


    public void StopProfiler()
    {
        if (startprofiler)
        {
            if (profilerData != null)
            {
                profilerData.time[1] = Convert.ToInt64(Time.realtimeSinceStartup);
                profilerData.profilerId = DevopsProfilerClient.Instance().GetReportId();

                SocketHelper.GetInstance().SendMessage("$ProfilerData$:" + JsonUtility.ToJson(profilerData));
            }


            DevopsProfilerClient.Instance().StopConnectServer();

            profilerData = null;
        }
    }

        public void ProfilerSnap()
    {
        if (startprofiler)
        {
            DevopsProfilerClient.Instance().ProfilerScreenShot(false);
        }
    }
    public void ObjectReferences()
    {
        if (startprofiler)
        {
            DevopsProfilerClient.Instance().ObjectReferences();

        }
    }
    public void ProfilerTag(string tag)
    {
        if (startprofiler)
            DevopsProfilerClient.Instance().SetRemark(tag);
    }

    public void ProfilerSnapPremises()
    {
        if (startprofiler)
            DevopsProfilerClient.Instance().ScreenShotNative();
    }
    public void CaseFinished()
    {
        InTestCaseItem.CaseFinish = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class CaseData
{
    //public int caseSeq = 1;
    public string moduleName = "";
    public string caseName = "";
    public long startTime = 0;
    public long endTime = 0;
    public long caseCost = 0;
    public string caseResult = "passed";
    public List<StepData> steps = new List<StepData>();


}
[System.Serializable]
public class Snapshot
{
    public string tag = "";
    public string name = "";
    public string content = "";
    public int width = 0 ;
    public int height = 0;
    public long frameCount = 0;
    public bool profiler = false;

}


[System.Serializable]
public class ImageData
{
    public string name = "";
    public string content = "";
    public int width = 0;
    public int height = 0;
}

[System.Serializable]
public class StepData {
    public string name = "";
    public string result = "passed";
    public string log = "";
    public long endTime = 0;
    public Snapshot snapshot = null;

}
[System.Serializable]
public class LabelData
{
    public string name = "";
    public string value = "";

}
[System.Serializable]
public class Attachments {
    public string name = "";
    public string source = "";
    public string type = "";
}

[System.Serializable]
public class TestData {

    public int buildRundId = 0;
    public string testPlanName = "test";
    public string engineType = "unity";
    public string startType = "自动";
    public string cpuModel = SystemInfo.processorType;
    public string gpuModel = SystemInfo.graphicsDeviceName;
    public string[] tags = new string[] { };

    public string engineVersion = Application.unityVersion;
    public string autoPluginVersion = "1.0";
    public string testPluginVersion = "1.0";
    public string deviceId = SystemInfo.deviceModel;
    //public string deviceUuid = SystemInfo.deviceUniqueIdentifier;
    public string systemType = SystemInfo.deviceType.ToString();
    public string systemVersion = SystemInfo.operatingSystem;
    public string appVersion = Application.version;
    public string packageId = Application.identifier;
    public int totalCaseCount = 0;
    public int passedCaseCount = 0;
    public int failedCaseCount = 0;
    public int skippedCaseCount = 0;
    public int errorCaseCount = 0;

    [SerializeField]
    public List<CaseData> datas;

}

[System.Serializable]
public class SocketData
{
    public TestData result;
}

public class FPSData
{
    public float FPS = 0;
    public float jank = 0;
    public float bigJank = 0;
    public float frameCount = Time.frameCount;
    public long timer = PTestUtils.GetTimeStamp();

    public long gameTimer = 0;
}

[System.Serializable]
public class DevopsTestProfilerData
{
    public string profilerId ="";
    public float[] time = new float[2];
    public int caseCount = 0;
    [SerializeField]
    public List<string> caseNames = new List<string>();
}
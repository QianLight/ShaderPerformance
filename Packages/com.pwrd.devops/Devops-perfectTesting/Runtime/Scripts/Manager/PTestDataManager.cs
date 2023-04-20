using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTestDataManager : Singleton<PTestDataManager>
{
    public Dictionary<string, CaseData> CaseResults = new Dictionary<string, CaseData>();
    [HideInInspector]
    public string planName = "";
    //public Dictionary<string,ImageData> screenshots = new Dictionary<string, ImageData>();
    public void CreateCaseData(string key)
    {
        CaseData caseData = new CaseData
        {
            caseName = key,
            moduleName = key,
            startTime = PTestUtils.GetTimeStamp(),
        };
        CaseResults.Add(key, caseData);
    }


    // Start is called before the first frame update
    public void AddCaseSteps(string key, string des, string tag, bool successed, ScreenType screenType)
    {
        CaseData caseData = CaseResults[PTestData.key];

        StepData stepData = new StepData();
        CaseResults[PTestData.key].steps.Add(stepData);
        stepData.name = key;
        stepData.result = successed ? "passed" : "failed";
        stepData.endTime = PTestUtils.GetTimeStamp();
        stepData.log = des;

        if (caseData.caseResult == "passed")
            caseData.caseResult = successed ? "passed" : "failed";

        PTestManager.Instance.ProfilerTag(key);
        if (screenType != ScreenType.None) {
            if (PTestManager.Instance.startprofiler)
            {
                if (screenType != ScreenType.SceenShot)
                    PTestManager.Instance.ProfilerSnap();
                else if(screenType != ScreenType.ObjectShot)
                    PTestManager.Instance.ObjectReferences();
            }
            Snapshot imageData = PTestUtils.ScreenShot();

            stepData.snapshot = imageData;
            stepData.snapshot.tag = tag;
            stepData.snapshot.profiler = PTestManager.Instance.isProfilerStart();

        }
        PTestManager.Instance.SendData();

    }

    public void SetErrorSteps(string key, string des, bool successed)
    {

        Snapshot imageData = PTestUtils.ScreenShot();
        CaseData caseData = CaseResults[PTestData.key];

        StepData stepData = new StepData();
        CaseResults[PTestData.key].steps.Add(stepData);
        stepData.name = key;
        stepData.result = "failed";
        stepData.log = des;
        stepData.endTime = PTestUtils.GetTimeStamp();

        caseData.steps.Add(stepData);

        stepData.snapshot = imageData;
        stepData.snapshot.profiler = PTestManager.Instance.isProfilerStart();

        caseData.caseResult = "error";
        caseData.endTime = PTestUtils.GetTimeStamp();
        caseData.caseCost = caseData.endTime - caseData.startTime;
        PTestManager.Instance.SendData();

    }

    public void SetTimeOutSteps(string key, string des, bool successed)
    {
        CaseData caseData = null;
        if (CaseResults.ContainsKey(PTestData.key))
            caseData = CaseResults[PTestData.key];
        else
            foreach (CaseData cases in CaseResults.Values) {
                caseData = cases;
            }
        caseData.steps.Add(new StepData
        {
            name = key,
            result = successed ? "passed" : "failed",
            log = des
        });
        caseData.caseResult = successed ? "passed" : "failed";
        caseData.endTime = PTestUtils.GetTimeStamp();
        caseData.caseCost = caseData.endTime - caseData.startTime;
        PTestManager.Instance.SendData();
    }


    public void ClearCaseResult()
    {
        CaseResults.Clear();
        //screenshots.Clear();
    }

    public TestData CaseResultToJson()
    {
        TestData testData = new TestData();
        testData.testPlanName = planName;
        List<CaseData> datas = new List<CaseData>();
        int totalCaseCount = PTestManager.Instance.caseSelect.Count;
        int passedCaseCount = 0;
        int failedCaseCount = 0;
        int errorCaseCount = 0;


        foreach (CaseData caseData in CaseResults.Values)
        {
            caseData.caseCost = caseData.endTime - caseData.startTime;
            datas.Add(caseData);
            if (caseData.caseResult == "passed")
                passedCaseCount++;
            if (caseData.caseResult == "failed")
                failedCaseCount++;
            if (caseData.caseResult == "error")
                errorCaseCount++;
        }
        testData.totalCaseCount = totalCaseCount;
        testData.passedCaseCount = passedCaseCount;
        testData.failedCaseCount = failedCaseCount;
        testData.errorCaseCount = errorCaseCount;

        testData.datas = datas;
        return testData;
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
[System.Serializable]
public class CaseItemData
{
    [SerializeField]
    public Type type;

    [SerializeField]
    public string CaseParam0;
    [SerializeField]
    public string CaseParam1;
    [SerializeField]
    public string CaseParam2;
    [SerializeField]
    public string CaseParam3;
    [SerializeField]
    public string CaseParam4;

    public bool isMissed = false;

    public string missType;
}
[System.Serializable]
public class CaseNetList
{
    public string planName="";
    public string buildRunId = "";
    public string buildTaskId = "";
    public string jobId = "";
    public string versionId = "";
    public string deviceId = "";
    public string extend_config = "";

    public bool startprofiler = false;
    public long caseTime = 10000;
    [SerializeField]
    public List<CaseNetItemData> caseList = new List<CaseNetItemData>();

}


[System.Serializable]
public class CaseNetItemData
{
    [SerializeField]
    public string Type;
    [SerializeField]
    public string Method;
    [SerializeField]
    public string CaseParam0;
    [SerializeField]
    public string CaseParam1;
    [SerializeField]
    public string CaseParam2;
    [SerializeField]
    public string CaseParam3;
    [SerializeField]
    public string CaseParam4;
}

[System.Serializable]
class CSVData
{
    public string name;
    public string content;
}

[System.Serializable]
class CSVDatas
{
    [SerializeField]
    public List<CSVData> content = new List<CSVData>();
}


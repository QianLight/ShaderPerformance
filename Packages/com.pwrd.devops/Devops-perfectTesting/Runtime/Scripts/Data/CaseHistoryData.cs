using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class CaseHistoryData
{
    [SerializeField]
    public List<CaseHistoryItemData> caseHistories = new List<CaseHistoryItemData>();
}

[System.Serializable]
public class CaseHistoryItemData
{
    //public CaseHistoryListData listData = new CaseHistoryListData();
    [SerializeField]
    public List<CaseHItemData> caseSuccessed = new List<CaseHItemData>();
    [SerializeField]
    public List<CaseHItemData> caseWait = new List<CaseHItemData>();
    [SerializeField]
    public List<CaseHItemData> caseFailed = new List<CaseHItemData>();
    public string time;
}
[System.Serializable]

public class CaseHistoryListData {
       
    
}


[System.Serializable]
public class CaseHItemData
{
    [SerializeField]
    public string type;
    public int status;
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PTestPlaneMini : MonoBehaviour
{
    public Button BtnBack;
    public Button BtnShow;

    public Button BtnStop;

    public CircleProgress progress;
    public Text txtFinish;
    public Text txtTesting;

    public PTestPlaneTesting planeFinished;
    public List<CaseItemData> itemCases = new List<CaseItemData>();
    public List<CaseItemData> itemCasesSuccess = new List<CaseItemData>();
    public List<CaseItemData> itemCasesWait = new List<CaseItemData>();
    [HideInInspector]
    public List<CaseItemData> itemCaseFailed = new List<CaseItemData>();

    public CaseItemData itemWait = null;
    // Start is called before the first frame update
    void Start()
    {
        planeFinished = PTestUIManager.Instance.testUI.GetPlaneTesting();
        BtnBack.onClick.AddListener(OnBtnBackClick);
        BtnShow.onClick.AddListener(OnBtnTestingClick);
        BtnStop.onClick.AddListener(OnBtnStopClick);
    }

    public void InitCase()
    {

        PTestManager.Instance.caseFailed += CaseFailed;
        PTestManager.Instance.caseStart += CaseStart;
        PTestManager.Instance.caseEnd += CaseEnd;
        itemCases.Clear();
        itemCasesSuccess.Clear();
        itemCasesWait.Clear();
        itemCaseFailed.Clear();

        itemCases.AddRange(PTestManager.Instance.caseSelect);
        itemCasesWait.AddRange(PTestManager.Instance.caseSelect);

    }

    public void OnBtnBackClick()
    {
        gameObject.SetActive(false);
        transform.parent.gameObject.SetActive(false);
        Devops.Core.EntrancePanel.Instance().SetEnable(true);

    }

    public void OnBtnStopClick()
    {
        gameObject.SetActive(false);

        planeFinished.OnBtnStopClick();

    }

    public void CaseStart(CaseItemData caseItem, bool isSuccess)
    {
        itemCasesWait.Remove(caseItem);
        itemWait = caseItem;

        TestFinished();
    }

    public void CaseEnd(CaseItemData caseItem, bool isSuccess)
    {
        itemCasesSuccess.Add(caseItem);
        itemWait = null;
        TestFinished();
    }

    public void CaseFailed(string e)
    {
        itemCaseFailed.Add(itemWait);
        itemWait = null;
        TestFinished();
    }


    public void TestFinished()
    {
        txtFinish.text = "已完成" + itemCasesSuccess.Count + "项";
        txtTesting.text = "测试中 共计" + itemCases.Count + "项";
        progress.Percentage = (float)(itemCasesSuccess.Count + itemCaseFailed.Count) / (float)itemCases.Count;
    }

    public void OnBtnTestingClick()
    {
        PTestUIManager.Instance.testUI.GetPlaneTesting().gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}

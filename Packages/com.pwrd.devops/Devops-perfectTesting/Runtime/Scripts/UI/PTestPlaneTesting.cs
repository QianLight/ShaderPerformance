using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PTestPlaneTesting : MonoBehaviour
{
    public PTestUI pTestUI;

    public CircleProgress progress;
    public Text txtFinish;
    public Text txtTesting;
    public Transform layoutWait;
    public Transform layoutSuccessed;
    public Transform layoutTesting;
    public Transform layoutFailed;

    public Button BtnBack;
    public Button BtnTesting;
    public Button BtnFinished;
    public Button BtnStop;

    //public PageView pageView;
    public PTestPlaneFinished planeFinished;
    public PTestPlaneMini planeMini;
    public List<ItemCase> itemCases = new List<ItemCase>();
    public List<ItemCase> itemCasesSuccess = new List<ItemCase>();
    public List<ItemCase> itemCasesWait = new List<ItemCase>();
    [HideInInspector]
    public List<ItemCase> itemCaseFailed = new List<ItemCase>();
    public ItemCase testingCase = null;
    public List<CaseItemData> caseSelect
    {
        get
        {
            return PTestManager.Instance.caseSelect;
        }

    }

    private bool caseFinished = false;
    // Start is called before the first frame update
    IEnumerator Start()
    {
       
        BtnBack.onClick.AddListener(OnBtnBackClick);
        BtnTesting.onClick.AddListener(OnBtnTestingClick);
        BtnFinished.onClick.AddListener(OnBtnFinishedClick);
        BtnStop.onClick.AddListener(OnBtnStopClick);
        //pageView.OnPageChanged += OnPageChange;
        yield return new WaitForEndOfFrame();
        yield return  UpdateLayout(layoutWait as RectTransform);
        yield return UpdateLayout(layoutWait.parent as RectTransform);

        //RectTransform rect = pageView.rect.content.transform as RectTransform;
        //RectTransform pageRect = pageView.transform as RectTransform;
        //pageRect.sizeDelta = new Vector2(0, itemCases.Count*155);
        //Debug.Log(rect.sizeDelta);
        //Debug.Log(rect.rect);
    }

    public void InitCase()
    {

        PTestManager.Instance.caseFailed += CaseFailed;
        PTestManager.Instance.caseStart += CaseStart;
        PTestManager.Instance.caseEnd += CaseEnd;
        caseFinished = true;
        itemCases.Clear();
        itemCaseFailed.Clear();
        itemCasesSuccess.Clear();
        itemCasesWait.Clear();
        DestroyChildren(layoutSuccessed);
        DestroyChildren(layoutWait);
        DestroyChildren(layoutFailed);
        DestroyChildren(layoutTesting);
        for (int i = 0; i < caseSelect.Count; i++)
        {
            CaseItemData classname = caseSelect[i];
            ItemCase item = pTestUI.CloneItem(layoutWait);
            item.init(classname);
            item.btnSelect.gameObject.SetActive(false);
            item.btnDelete.gameObject.SetActive(false);
            //item.imgStatus.gameObject.SetActive(true);
            itemCases.Add(item);
            item.SetCaseStatus(CaseStatus.wait);
        }
        //RectTransform pageRect = pageView.transform as RectTransform;
        //pageRect.sizeDelta = new Vector2(0, itemCases.Count * 155);
        TestFinished();
    }

    public void CaseStart(CaseItemData caseItem,bool isSuccess)
    {

        //if (testingCase != null) {
        //    testingCase.transform.SetParent(layoutSuccessed);
        //    testingCase.SetCaseStatus(CaseStatus.successed);
        //}

        ItemCase itemCase = GetItemCase(caseItem);
        itemCase.SetCaseStatus(CaseStatus.testing);
        itemCase.transform.SetParent(layoutTesting);
        testingCase = itemCase;
        if (gameObject.activeSelf) {
            StartCoroutine(UpdateLayout(layoutSuccessed as RectTransform));
            StartCoroutine(UpdateLayout(layoutWait as RectTransform));
            StartCoroutine(UpdateLayout(layoutWait.parent as RectTransform));
            StartCoroutine(UpdateLayout(layoutSuccessed.parent as RectTransform));
            StartCoroutine(UpdateLayout(layoutWait.parent.parent as RectTransform));
            StartCoroutine(UpdateLayout(layoutSuccessed.parent.parent as RectTransform));
        }
        TestFinished();

    }

    public void CaseEnd(CaseItemData caseItem, bool isSuccess)
    {

        if (testingCase != null)
        {
            testingCase.transform.SetParent(layoutSuccessed);
            testingCase.SetCaseStatus(CaseStatus.successed);

        }
        if (gameObject.activeSelf)
        {
            StartCoroutine(UpdateLayout(layoutSuccessed as RectTransform));
            StartCoroutine(UpdateLayout(layoutWait as RectTransform));
            StartCoroutine(UpdateLayout(layoutWait.parent as RectTransform));
            StartCoroutine(UpdateLayout(layoutSuccessed.parent as RectTransform));
            StartCoroutine(UpdateLayout(layoutWait.parent.parent as RectTransform));
            StartCoroutine(UpdateLayout(layoutSuccessed.parent.parent as RectTransform));
        }
        TestFinished();
        if (layoutSuccessed.childCount + layoutFailed.childCount == itemCases.Count && caseFinished)
        {
            caseFinished = false;
            //BtnStop.GetComponentInChildren<Text>().text = "测试报告";
            CaseFinished();
        }
    }

    public void CaseFailed(string e)
    {
        if (testingCase != null)
        {
            testingCase.transform.SetParent(layoutFailed);
            testingCase.SetCaseStatus(CaseStatus.faild);
            testingCase.txtError.text = e;
            //itemCaseFailed = testingCase;
            testingCase = null;
        }

        if (gameObject.activeSelf)
        {
            StartCoroutine(UpdateLayout(layoutSuccessed as RectTransform));
            StartCoroutine(UpdateLayout(layoutWait as RectTransform));
            StartCoroutine(UpdateLayout(layoutWait.parent as RectTransform));
            StartCoroutine(UpdateLayout(layoutSuccessed.parent as RectTransform));
            StartCoroutine(UpdateLayout(layoutWait.parent.parent as RectTransform));
            StartCoroutine(UpdateLayout(layoutSuccessed.parent.parent as RectTransform));
        }
        TestFinished();
        if (layoutSuccessed.childCount + layoutFailed.childCount == itemCases.Count && caseFinished)
        {
            caseFinished = false;
            //BtnStop.GetComponentInChildren<Text>().text = "测试报告";
            CaseFinished();
        }
    }


    public ItemCase GetItemCase(CaseItemData caseItem)
    {
        foreach (ItemCase itemCase in itemCases)
        {
            if (itemCase.classType == caseItem)
                return itemCase;
        }
        return null;
    }

    public void OnBtnBackClick()
    {
        gameObject.SetActive(false);
        transform.parent.gameObject.SetActive(false);
        Devops.Core.EntrancePanel.Instance().SetEnable(true);

    }

    public void OnBtnTestingClick()
    {
        layoutTesting.parent.gameObject.SetActive(true);
        layoutSuccessed.parent.gameObject.SetActive(false);

        BtnTesting.transform.GetChild(0).gameObject.SetActive(true);
        BtnFinished.transform.GetChild(0).gameObject.SetActive(false);

        StartCoroutine(UpdateLayout(layoutWait.parent.parent as RectTransform));
        StartCoroutine(UpdateLayout(layoutSuccessed.parent.parent as RectTransform));
    }

    public void OnBtnFinishedClick()
    {
        layoutTesting.parent.gameObject.SetActive(false);
        layoutSuccessed.parent.gameObject.SetActive(true);
        BtnTesting.transform.GetChild(0).gameObject.SetActive(false);
        BtnFinished.transform.GetChild(0).gameObject.SetActive(true);
        StartCoroutine(UpdateLayout(layoutWait.parent.parent as RectTransform));
        StartCoroutine(UpdateLayout(layoutSuccessed.parent.parent as RectTransform));
    }

    public void OnBtnStopClick()
    {
        PTestManager.Instance.StopTesting();
        CaseFinished();

    }

    public void CaseFinished()
    {

        SaveHistory();
        for (int i = 0; i < layoutSuccessed.childCount; i++)
        {
            ItemCase itemCase = layoutSuccessed.GetChild(i).GetComponent<ItemCase>();
            if (itemCase != null)
                itemCasesSuccess.Add(itemCase);
        }
        for (int i = 0; i < layoutTesting.childCount; i++)
        {
            ItemCase itemCase = layoutTesting.GetChild(i).GetComponent<ItemCase>();
            if (itemCase != null)
                itemCasesWait.Add(itemCase);
        }

        for (int i = 0; i < layoutWait.childCount; i++)
        {
            ItemCase itemCase = layoutWait.GetChild(i).GetComponent<ItemCase>();
            if (itemCase != null)
                itemCasesWait.Add(itemCase);
        }

        for (int i = 0; i < layoutFailed.childCount; i++)
        {
            ItemCase itemCase = layoutFailed.GetChild(i).GetComponent<ItemCase>();
            if (itemCase!= null)
                itemCaseFailed.Add(itemCase);
        }

        for (int i = 0; i < itemCaseFailed.Count; i++)
        {
            //ItemCase itemCase = layoutFailed.GetChild(i).GetComponent<ItemCase>();
            if (itemCaseFailed[i] == null)
                itemCaseFailed.Remove(itemCaseFailed[i]);
        }
        gameObject.SetActive(false);
        planeFinished.gameObject.SetActive(true);

        planeFinished.DestoryAll();


        planeFinished.itemCases .AddRange(itemCases);
        planeFinished.itemCasesSuccess.AddRange(itemCasesSuccess);
        planeFinished.itemCasesWait.AddRange(itemCasesWait);
        planeFinished.itemCaseFailed.AddRange( itemCaseFailed);
        planeFinished.InitCases("");
        planeMini.gameObject.SetActive(false);
    }


    public void SaveHistory()
    {
        CaseHistoryData caseHistory = new CaseHistoryData();
        string history = PlayerPrefs.GetString(PTestConfig.TestHistory, "");
        if (history != "") {
            caseHistory = JsonUtility.FromJson<CaseHistoryData>(history);
        }

        CaseHistoryItemData itemData = new CaseHistoryItemData();
        for (int i = 0; i < layoutSuccessed.childCount; i++)
        {
            CaseItemData caseItemData = layoutSuccessed.GetChild(i).GetComponent<ItemCase>().classType;
            itemData.caseSuccessed.Add(new CaseHItemData
            {
                type = caseItemData.type.Name,
                status = (int)CaseStatus.successed
            }) ;
        }
        for (int i = 0; i < layoutTesting.childCount; i++)
        {
            CaseItemData caseItemData = layoutTesting.GetChild(i).GetComponent<ItemCase>().classType;
            itemData.caseWait.Add(new CaseHItemData
            {
                type = caseItemData.type.Name,
                status = (int)CaseStatus.successed
            });
        }

        for (int i = 0; i < layoutWait.childCount; i++)
        {

            CaseItemData caseItemData = layoutWait.GetChild(i).GetComponent<ItemCase>().classType;
            itemData.caseWait.Add(new CaseHItemData
            {
                type = caseItemData.type.Name,
                //methodName = caseItemData.method.Name,
                status = (int)CaseStatus.successed
            });
        }


        for (int i = 0; i < layoutFailed.childCount; i++)
        {

            CaseItemData caseItemData = layoutFailed.GetChild(i).GetComponent<ItemCase>().classType;
            itemData.caseFailed.Add(new CaseHItemData
            {
                type = caseItemData.type.Name,
                //methodName = caseItemData.method.Name,
                status = (int)CaseStatus.successed
            });
        }


        itemData.time = DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss");

        caseHistory.caseHistories.Insert(0, itemData);
        string json = JsonUtility.ToJson(caseHistory);
        //Debug.Log(json);
        PlayerPrefs.SetString(PTestConfig.TestHistory, json);
        
    }

    public void OnPageChange(int i)
    {
        if (i == 0) {
            BtnTesting.transform.GetChild(0).gameObject.SetActive(true);
            BtnFinished.transform.GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            BtnTesting.transform.GetChild(0).gameObject.SetActive(false);
            BtnFinished.transform.GetChild(0).gameObject.SetActive(true);
        }

    }
    IEnumerator UpdateLayout(RectTransform rect)
    {
        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        yield return new WaitForEndOfFrame();
        Vector3 vecScale = rect.localScale;
        float width = rect.rect.width;
        float height = rect.rect.height;
        while (rect.rect.width == 0)
        {
            Debug.Log(rect.rect.width);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            yield return new WaitForEndOfFrame();
        }
    }


    public void TestFinished()
    {
        txtFinish.text = "已完成" + layoutSuccessed.childCount + "项";
        txtTesting.text = "测试中 共计" + itemCases.Count + "项";
        progress.Percentage = (float)(layoutSuccessed.childCount+ layoutFailed.childCount) / (float)itemCases.Count;
    }

    public void DestroyChildren(Transform transform)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}

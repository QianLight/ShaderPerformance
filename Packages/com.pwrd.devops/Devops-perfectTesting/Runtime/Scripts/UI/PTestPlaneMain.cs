using Devops.Test;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PTestPlaneMain : MonoBehaviour
{
    public PTestUI pTestUI;


    public Devops.Test.ItemHistory objHistory;

    public Transform Content;
    public Transform historyContent;

    public Button BtnCases;
    public Button BtnHistory;

    public GameObject pageCase;
    public GameObject pageHistory;

    public Button btnSearch;
    public Button BtnContinue;
    public Button BtnBack;

    public Toggle btnSelectAll;

    public Text txtSelectNum;


    public List<CaseHistoryItemData> caseHistories = new List<CaseHistoryItemData>();
    public List<ItemCase> itemCases = new List<ItemCase>();
    public List<ItemClassCase> itemClassCases = new List<ItemClassCase>();

    public List<CaseItemData> caseSelect
    {
        get
        {
            return PTestManager.Instance.caseSelect;
        }

    }
    // Start is called before the first frame update
    void Start()
    {
        BindEvent();

    }

    public void Init()
    {
        InitTestCase();
    }

    public void InitTestCase()
    {

        //PTestManager.Instance.LoadCaseList();
        Dictionary<Type, List<CaseItemData>> classes = new Dictionary<Type, List<CaseItemData>>();
        for (int i = 0; i < pTestUI.caseNames.Count; i++)
        {
            Type type = pTestUI.caseNames[i].type;
            if (!classes.ContainsKey(pTestUI.caseNames[i].type))
                classes[type] = new List<CaseItemData>();
            classes[type].Add(pTestUI.caseNames[i]);
        }

        foreach (KeyValuePair<Type, List<CaseItemData>> keyValue in classes)
        {
            ItemClassCase item = pTestUI.CloneItemClass(Content);
            item.SetClassName(keyValue.Key);
            itemCases.AddRange(item.ItemCases(keyValue.Value, TextSelectChange));
            itemClassCases.Add(item);

        }

        if (transform.parent.gameObject.activeSelf)
            StartCoroutine(UpdateLayout(Content as RectTransform));

    }


    public void SetSelecct()
    {
        foreach (ItemClassCase keyValue in itemClassCases)
        {

            keyValue.SetSelect();
        }
        CheckSelectAll();
    }

    public void BindEvent()
    {
        btnSearch.onClick.AddListener(OnBtnSearch);
        BtnContinue.onClick.AddListener(OnBtnContinue);
        btnSelectAll.onValueChanged.AddListener(OnBtnSelect);
        BtnCases.onClick.AddListener(OnBtnCaseClick);
        BtnHistory.onClick.AddListener(OnBtnHistoryClick);
        BtnBack.onClick.AddListener(OnBtnCaseBack);

    }

    public Devops.Test.ItemHistory CloneItem(Transform parent)
    {
        Devops.Test.ItemHistory item = Instantiate<Devops.Test.ItemHistory>(objHistory, parent);
        item.planeFinished = pTestUI.planeFinished;
        return item;
    }

    public void OnBtnCaseBack()
    {
        PTestUIManager.Instance.testUI.OnBtnUIShowClick();
        Devops.Core.EntrancePanel.Instance().SetEnable(true);

    }

    public void OnBtnCaseClick()
    {
        pageCase.SetActive(true);
        pageHistory.SetActive(false);
        BtnCases.GetComponentInChildren<Text>().color = new Color(1, 1, 1, 1);
        BtnHistory.GetComponentInChildren<Text>().color = new Color(1, 1, 1, 0.5f);

    }

    public void OnBtnHistoryClick()
    {
        pageCase.SetActive(false);
        pageHistory.SetActive(true);
        BtnCases.GetComponentInChildren<Text>().color = new Color(1, 1, 1, 0.5f);
        BtnHistory.GetComponentInChildren<Text>().color = new Color(1, 1, 1, 1);
        CaseHistoryData caseHistory = new CaseHistoryData();
        string history = PlayerPrefs.GetString(PTestConfig.TestHistory, "");
        if (history != "")
        {
            caseHistory = JsonUtility.FromJson<CaseHistoryData>(history);
        }


        foreach (CaseHistoryItemData historyItemData in caseHistory.caseHistories)
        {

            if (!caseHistories.Exists(a => a.time == historyItemData.time))
            {
                Devops.Test.ItemHistory itemHistory = CloneItem(historyContent);
                //itemHistory.itemData = historyItemData;
                itemHistory.InitCase(historyItemData);
                caseHistories.Add(historyItemData);
            }
            //foreach (Transform item in historyContent)
            //{

            //}
        }
    }

    public void OnBtnSearch()
    {
        pTestUI.planeSearch.gameObject.SetActive(true);
        pTestUI.planeSearch.Init(itemCases);
    }

    public void OnBtnContinue()
    {
        //caseSelect.Clear();
        foreach (ItemCase item in itemCases)
        {
            if (item.btnSelect.isOn && !caseSelect.Contains(item.classType))
            {
                caseSelect.Add(item.classType);
            }
            else if (!item.btnSelect.isOn && caseSelect.Contains(item.classType))
            {
                caseSelect.Remove(item.classType);
            }
        }
        pTestUI.planeAffirm.gameObject.SetActive(true);
        pTestUI.planeAffirm.Init(caseSelect);
    }

    public void OnBtnSelect(bool select)
    {
        SetSelectStatus(select);
    }

    public void SetSelectStatus(bool select)
    {
        foreach (ItemCase item in itemCases)
        {
            item.btnSelect.isOn = select;
        }

        foreach (ItemClassCase item in itemClassCases)
        {
            item.btnSelectAll.isOn = select;
        }
        TextSelectChange();
    }

    public void TextSelectChange()
    {
        int i = 0;
        foreach (ItemCase item in itemCases)
        {
            if (item.btnSelect.isOn)
                i++;
        }
        txtSelectNum.text = "已选" + i + "条";
    }

    public void CheckSelectAll()
    {
        bool selectAll = true;

        foreach (ItemClassCase item in itemClassCases)
        {
            item.CheckSelectAll();
        }

        foreach (ItemCase item in itemCases)
        {
            if (!item.btnSelect.isOn)
            {
                selectAll = false;
                btnSelectAll.onValueChanged.RemoveAllListeners();
                btnSelectAll.isOn = false;
                btnSelectAll.onValueChanged.AddListener(OnBtnSelect);

                return;
            }
        }
        if (itemCases.Count > 0)
            btnSelectAll.isOn = true;
    }

    // Update is called once per frame
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
}

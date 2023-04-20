using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Devops.Test{ 
public class ItemHistory : MonoBehaviour
{
    public Text txtTime;
    public Text txtDes;

    public Button btnClick;

    public CaseHistoryItemData itemData;

    public PTestPlaneFinished planeFinished;

    public List<ItemCase> itemCases = new List<ItemCase>();
    public List<ItemCase> itemCasesSuccess = new List<ItemCase>();
    public List<ItemCase> itemCasesWait = new List<ItemCase>();
    public List<ItemCase> itemCaseFailed = new List<ItemCase>();


    public void InitCase(CaseHistoryItemData itemData)
    {
        this.itemData = itemData;
        txtTime.text = itemData.time;
        int success = itemData.caseSuccessed.Count;
        int wait = itemData.caseWait.Count;

        int failed = itemData.caseFailed.Count;

        txtDes.text = "共计测试脚本" + (success + failed + wait) + " 成功" + success + " 未执行" + wait + " 失败"+ failed ;
    }
    // Start is called before the first frame update
    void Start()
    {
        btnClick.onClick.AddListener(OnBtnClickClick);
    }

    public void OnBtnClickClick()
    {
        planeFinished.DestoryAll();

        LoadCaseList(itemData.caseSuccessed, itemCasesSuccess, transform);
        LoadCaseList(itemData.caseWait, itemCasesWait, transform);
        LoadCaseList(itemData.caseFailed, itemCaseFailed, transform);
        //if (itemData.caseFailed.Count > 0)
        //{
        //    itemCaseFailed = PTestUIManager.Instance.testUI.CloneItem(transform);
        //    itemCaseFailed.SetCaseStatus(CaseStatus.faild);

        //}
        //LoadCaseList(itemData.caseSuccessed, itemCasesSuccess, planeFinished.layoutSuccess);

        planeFinished.gameObject.SetActive(true);
        planeFinished.itemCases = itemCases;
        planeFinished.itemCasesSuccess = itemCasesSuccess;
        planeFinished.itemCasesWait = itemCasesWait;
        planeFinished.itemCaseFailed = itemCaseFailed;

        planeFinished.InitCases("");
    }

    public void LoadCaseList(List<CaseHItemData> NetList, List<ItemCase> partCases , Transform content)
    {
        foreach (CaseHItemData caseNetItem in NetList)
        {
            foreach (CaseItemData caseItem in PTestManager.Instance.caseNames)
            {
                if (caseItem.type.Name == caseNetItem.type)
                {
                    ItemCase item =PTestUIManager.Instance.testUI.CloneItem(content);
                    item.init(caseItem);
                    item.btnSelect.gameObject.SetActive(false);
                    item.btnDelete.gameObject.SetActive(false);
                    //item.imgStatus.gameObject.SetActive(true);
                    partCases.Add(item);
                    item.SetCaseStatus((CaseStatus)Enum.ToObject(typeof(CaseStatus), caseNetItem.status));
                    itemCases.Add(item);
                }

            }

        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
}
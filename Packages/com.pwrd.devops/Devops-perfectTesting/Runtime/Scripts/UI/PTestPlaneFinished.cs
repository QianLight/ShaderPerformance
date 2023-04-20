using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PTestPlaneFinished : MonoBehaviour
{

    public Button btnBack;

    public Button btnFailed;

    public Button btnSuccess;

    public Button btnWait;

    public Toggle btnSelectAll;

    public Button btnAffirm;

    public Button btnCancel;

    public Button btnContinue;

    public Transform layoutAffrim;

    public Text txtSelect;


    public Transform layoutFailed;

    public Transform layoutSuccess;

    public Transform layoutWait;

    public Transform pageFailed;

    public Transform pageSuccess;

    public Transform pageWait;

    //public PageView pageView;
    [HideInInspector]
    public List<ItemCase> itemCases = new List<ItemCase>();

    [HideInInspector]
    public List<ItemCase> itemCaseFailed = new List<ItemCase>();
    [HideInInspector]
    public List<ItemCase> itemCasesSuccess = new List<ItemCase>();
    [HideInInspector]
    public List<ItemCase> itemCasesWait = new List<ItemCase>();

    public PTestPlaneMain planeMain;

    public List<CaseItemData> caseSelect
    {
        get
        {
            return PTestManager.Instance.caseSelect;
        }

    }
    public void InitCases(string e,bool isClear = true)
    {
        //if(isClear)
        //    caseSelect.Clear();


        for (int i = 0; i < itemCasesSuccess.Count; i++)
        {
            itemCasesSuccess[i].transform.SetParent(layoutSuccess);
        }

        for (int i = 0; i < itemCasesWait.Count; i++)
        {
            itemCasesWait[i].transform.SetParent(layoutWait);
            itemCasesWait[i].SetCaseStatus(CaseStatus.wait,false);
        }

        for (int i = 0; i < itemCaseFailed.Count; i++)
        {
            itemCaseFailed[i].transform.SetParent(layoutFailed);
            itemCaseFailed[i].SetCaseStatus(CaseStatus.faild, false);
        }

        for (int i = 0; i < itemCases.Count; i++)
        {
            itemCases[i].btnSelect.gameObject.SetActive(true);
            itemCases[i].btnSelect.onValueChanged.AddListener(OnBtnItemSelect);
        }

        OnBtnSuccessClick();
    }

    public void DestoryAll()
    {
        DestroyChildren(layoutSuccess);
        DestroyChildren(layoutWait);
        DestroyChildren(layoutFailed);

        itemCases.Clear();
        itemCasesSuccess.Clear();
        itemCasesWait.Clear();
        itemCaseFailed.Clear();

    }

    public void OnBtnItemSelect(bool b)
    {
        if (pageSuccess.gameObject.activeSelf) {
            ItemSelectAllCheck(itemCasesSuccess);
        }

        if (pageWait.gameObject.activeSelf)
        {
            ItemSelectAllCheck(itemCasesWait);

        }
        if (pageFailed.gameObject.activeSelf)
        {
            ItemSelectAllCheck(itemCaseFailed);

        }
    }

    public void ItemSelectAllCheck(List<ItemCase> cases)
    {
        bool isSelect = true;
        for (int i = 0; i < cases.Count; i++)
        {
            if (!cases[i].btnSelect.isOn)
            {
                isSelect = false;
                break;
            }
        }
        if (cases.Count == 0)
            isSelect = false;
        if (isSelect)
        {
            btnSelectAll.isOn = true;
        }
        else
        {
            btnSelectAll.onValueChanged.RemoveAllListeners();
            btnSelectAll.isOn = false;
            btnSelectAll.onValueChanged.AddListener(OnBtnSelectAll);

        }
    }
    
    public void OnBtnSelectAll(bool b)
    {

        if (pageSuccess.gameObject.activeSelf)
        {
            ItemSelectAll(itemCasesSuccess,b);
        }

        if (pageWait.gameObject.activeSelf)
        {
            ItemSelectAll(itemCasesWait, b);

        }
        if (pageFailed.gameObject.activeSelf)
        {
            ItemSelectAll(itemCaseFailed, b);

        }
    }

    public void ItemSelectAll(List<ItemCase> cases,bool b)
    {
        for (int i = 0; i < cases.Count; i++)
        {
            cases[i].btnSelect.isOn = b;
        }
    }

    public void OnBtnAffrimClick()
    {
        layoutAffrim.gameObject.SetActive(true);
    }

    public void OnBtnCancelClick()
    {
        layoutAffrim.gameObject.SetActive(false);

    }

    public void OnBtnContinueClick()
    {
        OnBtnCancelClick();
        gameObject.SetActive(false);
        planeMain.gameObject.SetActive(true);
        foreach (ItemCase item in itemCases)
        {
            if (item.btnSelect.isOn && !caseSelect.Contains(item.classType))
            {
                caseSelect.Add(item.classType);
            }else if (!item.btnSelect.isOn && caseSelect.Contains(item.classType))
            {
                caseSelect.Remove(item.classType);

            }
        }
        planeMain.SetSelecct();
        planeMain.OnBtnCaseClick();
        DestroyChildren(layoutSuccess);
        DestroyChildren(layoutWait);
        DestroyChildren(layoutFailed);
    }

    public void OnBtnBackClick()
    {
        //PTestManager.Instance.caseSelect.Clear();
        DestroyChildren(layoutSuccess);
        DestroyChildren(layoutWait);
        DestroyChildren(layoutFailed);
        gameObject.SetActive(false);
        PTestUIManager.Instance.testUI.planeMain.gameObject.SetActive(true);
        PTestUIManager.Instance.testUI.planeMain.SetSelecct();
        //PTestUIManager.Instance.testUI.planeAffirm.gameObject.SetActive(true);
        //PTestUIManager.Instance.testUI.planeAffirm.InitTestCase();
        //PTestUIManager.Instance.HidePlane();
    }

    public void DestroyChildren(Transform transform)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    public void OnBtnFailedClick()
    {
        //pageView.pageTo(0);
        btnFailed.transform.GetChild(0).gameObject.SetActive(true);
        btnSuccess.transform.GetChild(0).gameObject.SetActive(false);
        btnWait.transform.GetChild(0).gameObject.SetActive(false);
        pageFailed.gameObject.SetActive(true);
        pageSuccess.gameObject.SetActive(false);
        pageWait.gameObject.SetActive(false);
        ItemSelectAllCheck(itemCaseFailed);

    }

    public void OnBtnSuccessClick()
    {
        //pageView.pageTo(1);
        btnFailed.transform.GetChild(0).gameObject.SetActive(false);
        btnSuccess.transform.GetChild(0).gameObject.SetActive(true);
        btnWait.transform.GetChild(0).gameObject.SetActive(false);
        pageFailed.gameObject.SetActive(false);
        pageSuccess.gameObject.SetActive(true);
        pageWait.gameObject.SetActive(false);
        ItemSelectAllCheck(itemCasesSuccess);

    }

    public void OnBtnWaitClick()
    {
        //pageView.pageTo(2);
        btnFailed.transform.GetChild(0).gameObject.SetActive(false);
        btnSuccess.transform.GetChild(0).gameObject.SetActive(false);
        btnWait.transform.GetChild(0).gameObject.SetActive(true);
        pageFailed.gameObject.SetActive(false);
        pageSuccess.gameObject.SetActive(false);
        pageWait.gameObject.SetActive(true);
        ItemSelectAllCheck(itemCasesWait);

    }

    public void OnPageChange(int i)
    {
        switch (i) {
            case 0:
                btnFailed.transform.GetChild(0).gameObject.SetActive(true);
                btnSuccess.transform.GetChild(0).gameObject.SetActive(false);
                btnWait.transform.GetChild(0).gameObject.SetActive(false);
                break;
            case 1:
                btnFailed.transform.GetChild(0).gameObject.SetActive(false);
                btnSuccess.transform.GetChild(0).gameObject.SetActive(true);
                btnWait.transform.GetChild(0).gameObject.SetActive(false);
                break;
            case 2:
                btnFailed.transform.GetChild(0).gameObject.SetActive(false);
                btnSuccess.transform.GetChild(0).gameObject.SetActive(false);
                btnWait.transform.GetChild(0).gameObject.SetActive(true);
                break;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        btnBack.onClick.AddListener(OnBtnBackClick);
        btnFailed.onClick.AddListener(OnBtnFailedClick);
        btnSuccess.onClick.AddListener(OnBtnSuccessClick);
        btnWait.onClick.AddListener(OnBtnWaitClick);
        btnAffirm.onClick.AddListener(OnBtnAffrimClick);
        btnCancel.onClick.AddListener(OnBtnCancelClick);
        btnContinue.onClick.AddListener(OnBtnContinueClick);
        btnSelectAll.onValueChanged.AddListener(OnBtnSelectAll);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

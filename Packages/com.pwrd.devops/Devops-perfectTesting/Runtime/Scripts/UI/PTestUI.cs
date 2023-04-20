using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PTestUI : MonoBehaviour
{
    public GameObject uiMain;

    public Transform contentList;

    //public Transform contentSelect;

    public Button btnUIShow;

    public ItemCase itemCase;
    public ItemClassCase itemClassObj;
    public PTestPlaneMain planeMain;
    public PTestPlaneAffirm planeAffirm;
    public PTestPlaneSearch planeSearch;

    public PTestPlaneTesting planeTesting;
    public PTestPlaneTesting planeTestingHorizontal;
    public PTestPlaneMini planeTestingMini;

    public PTestPlaneFinished planeFinished;

    public CanvasScaler canvasScaler;

    public bool isHorizontal = false;
    public List<CaseItemData> caseNames {
        get {

            return PTestManager.Instance.caseNames;
        }

    }


    // Start is called before the first frame update
    void Start()
    {

        if (UnityEngine.Screen.width > UnityEngine.Screen.height)
        {
            canvasScaler.matchWidthOrHeight = 1;
            isHorizontal = true;
        }
        else
        {
            canvasScaler.matchWidthOrHeight = 0;
            isHorizontal = false;
        }
    }


    public void Init() {
        planeMain.Init();
        BindEvent();
    }

    public ItemClassCase CloneItemClass(Transform parent)
    {
        ItemClassCase item = Instantiate<ItemClassCase>(itemClassObj, parent);
        return item;
    }

    public ItemCase CloneItem(Transform parent)
    {
        ItemCase item = Instantiate<ItemCase>(itemCase, parent);
        item.imgStatus.gameObject.SetActive(false);
        return item;
    }

    public void AddItem(CaseItemData caseItem)
    {
        planeMain.CheckSelectAll();
        //if (!PTestManager.Instance.caseSelect.Contains(caseItem))
        //    PTestManager.Instance.caseSelect.Add(caseItem);
    }

    public void RemoveItem(CaseItemData caseItem)
    {
        planeMain.CheckSelectAll();

        //if (PTestManager.Instance.caseSelect.Contains(caseItem))
        //    PTestManager.Instance.caseSelect.Remove(caseItem);
    }

    public void BindEvent()
    {
        btnUIShow.onClick.AddListener(OnBtnUIShowClick);
    }

    public void OnBtnUIShowClick()
    {
        if (uiMain.activeSelf)
        {
            GetPlaneTesting().gameObject.SetActive(false);

            uiMain.SetActive(false);
        }
        else
        {
            if (PTestManager.Instance.Testing) 
            
            {
                planeMain.gameObject.SetActive(false);

                if (!isHorizontal)
                    planeTestingMini.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                else
                    planeTestingMini.transform.localScale = new Vector3(1, 1, 1);

                planeTestingMini.gameObject.SetActive(true);

            }
            else { 
                if (PTestManager.Instance.caseSelect.Count > 0)
                {
                    planeAffirm.gameObject.SetActive(true);
                    planeAffirm.InitTestCase();
                }
            }
            planeMain.SetSelecct();

            uiMain.SetActive(true);
        }

     }
       
     public PTestPlaneTesting GetPlaneTesting()
    {
        if (!isHorizontal)
            return  planeTesting;
        else
            return planeTestingHorizontal;
    }

        // Update is called once per frame
        void Update()
    {
        
    }
}

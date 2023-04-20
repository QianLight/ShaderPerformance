using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemClassCase : MonoBehaviour
{
    public Toggle btnSelectAll;
    public Image imgExpend;
    public Sprite Open;
    public Sprite Close;
    public Button btnClass;
    public GameObject layout;
    public ItemCase caseObj;
    public List<ItemCase> itemCases = new List<ItemCase>();

    // Start is called before the first frame update
    void Start()
    {
        //btnClass.onClick.AddListener(()=> {
        //    if (layout.activeSelf)
        //    {
        //        layout.SetActive(false);
        //        imgExpend.sprite = Close;
        //    }
        //    else
        //    {
        //        layout.SetActive(true);
        //        imgExpend.sprite = Open;
        //    }
        //    //StartCoroutine(UpdateLayout(layout.transform as RectTransform));
        //    StartCoroutine(UpdateLayout(transform as RectTransform));
        //    StartCoroutine(UpdateLayout(transform.parent as RectTransform));
        //});

        btnSelectAll.onValueChanged.AddListener(SelectAll);
    }

    public void SetClassName(Type type)
    {
        btnClass.GetComponentInChildren<Text>().text = type.Name;
    }

    public List<ItemCase> ItemCases(List<CaseItemData> caseItems,Action TextSelectChange)
    {
        for (int i = 0; i < caseItems.Count; i++)
        {
            CaseItemData classname = caseItems[i];
            ItemCase item = CloneItem(layout.transform);
            item.init(classname);
            item.btnDelete.gameObject.SetActive(false);
            item.btnSelect.onValueChanged.AddListener((b) => {
                TextSelectChange?.Invoke();
            });
            itemCases.Add(item);
            item.SetCaseStatus(CaseStatus.None);

            foreach (CaseItemData caseItem in PTestManager.Instance.caseSelect)
            {
                if (caseItem == classname)
                {
                    item.btnSelect.isOn = true;
                }

            }
        }
        return itemCases;
    }

    public void SetSelect()
    {

        for (int i = 0; i < itemCases.Count; i++)
        {
            ItemCase item = itemCases[i];
            item.SetCaseStatus(CaseStatus.None);
            for (int j = 0; j <  PTestManager.Instance.caseSelect.Count; j++)
            {
                    CaseItemData caseItem = PTestManager.Instance.caseSelect[j];
                if (caseItem == item.classType)
                {
                    item.btnSelect.isOn = true;
                }
                

            }
        }
    }

    public ItemCase CloneItem(Transform parent)
    {
        ItemCase item = Instantiate<ItemCase>(caseObj, parent);
        return item;
    }

    public void SelectAll(bool b)
    {
        for (int i = 0; i < itemCases.Count; i++)
        {
            itemCases[i].btnSelect.isOn = b;
        }
            
    }

    public void CheckSelectAll()
    {
        bool selectAll = true;


        foreach (ItemCase item in itemCases)
        {
            if (!item.btnSelect.isOn)
            {
                selectAll = false;
                btnSelectAll.onValueChanged.RemoveAllListeners();
                btnSelectAll.isOn = false;
                btnSelectAll.onValueChanged.AddListener(SelectAll);

                return;
            }
        }
        btnSelectAll.isOn = true;
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
}

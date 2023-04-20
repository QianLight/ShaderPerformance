using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PTestPlaneAffirm : MonoBehaviour
{

    public PTestUI pTestUI;

    public Transform Content;

    public Button BtnContinue;
    public Button BtnBack;
    public Button BtnClear;

    public Text txtSelectNum;

    public List<ItemCase> itemCases = new List<ItemCase>();
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

    public void Init(List<CaseItemData> caseSelect)
    {
        this.itemCases.Clear();
        InitTestCase();
    }

    public void InitTestCase()
    {
        ResetView();

        for (int i = 0; i < caseSelect.Count; i++)
        {
            CaseItemData classname = caseSelect[i];
            ItemCase item = pTestUI.CloneItem(Content);
            item.init(classname);
            item.CanDrag = true;
            item.btnSelect.gameObject.SetActive(false);
            item.SetCaseStatus(CaseStatus.None);
            item.imgStatus.gameObject.SetActive(false);

            ItemDrag itemDrag = item.gameObject.AddComponent<ItemDrag>();
            //item.imgSort.gameObject.SetActive(true);
            itemDrag.MouseUp += MouseUp;
            item.btnDelete.onClick.AddListener(()=> {
                ItemDelete(classname);
            });
            itemCases.Add(item);
        }
        TextSelectChange();

    }

    public void MouseUp(ItemCase dragCase)
    {
        float dis = 100000000000;
        int index = 0;
        for (int i = 0; i < Content.childCount; i++) {
            ItemCase itemCase = Content.GetChild(i).GetComponent<ItemCase>();
            if (itemCase == dragCase)
                continue;
            float disitem = Vector3.Distance(itemCase.rectTransform.position ,dragCase.rectTransform.position);
            if (disitem < dis) {
                dis = disitem;
                index = i;
                //if (disitem > 50)
                //{
                //    index = -1;
                //    break;
                //}
            }

        }
        if(index>=0)
            dragCase.transform.SetSiblingIndex(index);
        StartCoroutine(UpdateLayout(Content as RectTransform));
    }


    public bool isInRect(ItemCase itemCase,Vector3 pos)
    {
        Vector3[] fourCornersArray = new Vector3[4];
        itemCase.rectTransform.GetWorldCorners(fourCornersArray);

        bool isInRect =
            pos.x >= fourCornersArray[0].x &&
            pos.x <= fourCornersArray[2].x &&
            pos.y >= fourCornersArray[0].y &&
            pos.y <= fourCornersArray[2].y;
        return isInRect;

    }


    public void BindEvent()
    {
        BtnBack.onClick.AddListener(OnBtnBack);
        BtnContinue.onClick.AddListener(OnBtnContinue);
        BtnClear.onClick.AddListener(OnBtnClear);
    }

    public void ItemDelete(CaseItemData type)
    {
        for (int i = 0; i < Content.childCount; i++)
        {
            ItemCase item = Content.GetChild(i).gameObject.GetComponent<ItemCase>();

            if (item.classType.type == type.type)
            {
                caseSelect.Remove(type);
                Destroy(Content.GetChild(i).gameObject);
            }

        }
        TextSelectChange();
    }

    public void OnBtnClear()
    {
        ResetView();
    }

    public void ResetView()
    {
        for (int i = 0; i < Content.childCount; i++)
        {
            Destroy(Content.GetChild(i).gameObject);
        }
        TextSelectChange();
    }

    public void OnBtnBack()
    {
        OnBtnClear();
        gameObject.SetActive(false);
    }

    public void OnBtnContinue()
    {
        caseSelect.Clear();
        for (int i = 0; i < Content.childCount; i++)
        {
            ItemCase itemCase = Content.GetChild(i).GetComponent<ItemCase>();
            caseSelect.Add(itemCase.classType);
        }
        //PTestManager.Instance.caseSelect.Clear();
        //PTestManager.Instance.caseSelect.AddRange(caseSelect);
        PTestManager.Instance.StartCases(false);
        OnBtnClear();
        Devops.Core.EntrancePanel.Instance().SetEnable(true);
    }
    public void TextSelectChange()
    {
        txtSelectNum.text = "共计" + caseSelect.Count + "条";
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
    // Update is called once per frame
    void Update()
    {
        
    }
}

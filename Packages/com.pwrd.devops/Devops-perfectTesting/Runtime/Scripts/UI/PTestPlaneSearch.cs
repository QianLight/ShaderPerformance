using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class PTestPlaneSearch : MonoBehaviour
{
    public PTestPlaneMain planeMain;
    public PTestUI pTestUI;

    public Transform Content;
    public List<ItemCase> itemCases = new List<ItemCase>();
    public List<ItemCase> searchCases = new List<ItemCase>();
    public InputField txtInput;
    public Button btnSearch;
    public Button btnBack;
    // Start is called before the first frame update
    void Start()
    {
        BindEvent();

    }

    public void Init(List<ItemCase> itemCases)
    {
        this.itemCases.Clear();
        Clear();
        this.itemCases.AddRange(itemCases);
    }

    public void OnBtnSearch()
    {
        string text = txtInput.text;
        Search(text);
    }

    public void Search(string text)
    {
        Clear();
        if (text == "")
            return;
        foreach (ItemCase item in itemCases)
        {
            if(Regex.IsMatch(item.txtCaseName.text, text)){
                ItemCase searchitem = Instantiate<ItemCase>(item, Content);
                searchCases.Add(searchitem);
            }
        }
        StartCoroutine(UpdateLayout(Content as RectTransform));
    }

    public void BindEvent()
    {
        btnSearch.onClick.AddListener(OnBtnSearch);
        btnBack.onClick.AddListener(OnBtnBack);
        txtInput.onEndEdit.AddListener(Search);
        txtInput.onValueChanged.AddListener(Search);

    }

    public void OnBtnBack()
    {
        SelectItem();
        gameObject.SetActive(false);
        //Clear();
    }

    public void SelectItem()
    {
        foreach(ItemCase isearch in searchCases)
        {
            foreach (ItemCase item in planeMain.itemCases)
            {
                if (item.txtCaseName.text == isearch.txtCaseName.text) {
                    item.btnSelect.isOn = isearch.btnSelect.isOn;
                }
            }
        }
        planeMain.TextSelectChange();
    }

    public void Clear()
    {
        searchCases.Clear();
        for (int i = 0; i < Content.childCount; i++)
        {
            Destroy(Content.GetChild(i).gameObject);
        }
    }

    IEnumerator UpdateLayout(RectTransform rect)
    {
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemCase : MonoBehaviour
{
    public RectTransform rectTransform;
    public CaseItemData classType;
    public Text txtCaseName;
    public Text txtError;

    public Button btnDelete;
    public Toggle btnSelect;

    public Image imgStatus;
    public Image imgTesting;
    public Action MouseDown;
    public Action<ItemCase> MouseMove;

    public Sprite sprWait;
    public Sprite sprSuccessed;
    public Sprite sprFaild;
    public Sprite sprTesting;
    [HideInInspector]
    public CaseStatus caseStatus = CaseStatus.None;
    private bool objStart = false;
    private bool _CanDrag = false;

    private Coroutine coroutineWaiting;
    private Coroutine coroutineTesting;

    public bool CanDrag {
        set {
            _CanDrag = value;
        }

        get {

            return _CanDrag;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        BindEvent();
        rectTransform = GetComponent<RectTransform>();
        objStart = true;
        //SetCaseStatus(caseStatus);

    }


    public void init(CaseItemData classtype) {
        this.classType = classtype;
        //txtCaseName.text = classtype.type.Name+"."+ classtype.method.Name;
        txtCaseName.text = classtype.type.Name;

    }


    public void BindEvent()
    {
        btnDelete.onClick.AddListener(OnBtnDeleteClick);
        btnSelect.onValueChanged.AddListener(OnBtnSelectClick);
    }

    public void OnBtnDeleteClick()
    {
        //PTestManager.Instance.StartCase(classType);

    }

    public void OnBtnSelectClick(bool select)
    {
        if (select)
            PTestUIManager.Instance.testUI.AddItem(classType);
        else
            PTestUIManager.Instance.testUI.RemoveItem(classType);
    }

    public void SetCaseStatus(CaseStatus status,bool animation = true)
    {
        caseStatus = status;
        //if (!objStart)
        //    return;
        if (coroutineWaiting != null) {
            StopCoroutine(coroutineWaiting);
            coroutineWaiting = null;
        }

        if (coroutineTesting != null)
        {
            StopCoroutine(coroutineTesting);
            coroutineTesting = null;
        }
        //StopCoroutine("ImageAnimation");
        //StopCoroutine("ImageTestingAnimation");
        imgStatus.gameObject.SetActive(true);
        imgTesting.gameObject.SetActive(false);
        txtError.gameObject.SetActive(false);
        switch (status)
        {
            case CaseStatus.None:
                imgStatus.gameObject.SetActive(false);
                imgTesting.gameObject.SetActive(false);
                break;
            case CaseStatus.wait:
                imgStatus.sprite = sprWait;
                if (animation)
                    coroutineWaiting = PTestAnimatorManager.Instance.StartImageAnimation(imgStatus.transform);
                break;
            case CaseStatus.successed:
                imgStatus.sprite = sprSuccessed;
                imgStatus.transform.Rotate(0, 0, 0, Space.World);

                break;
            case CaseStatus.faild:
                imgStatus.sprite = sprFaild;
                imgStatus.transform.Rotate(0, 0, 0, Space.World);
                txtError.gameObject.SetActive(true);
                break;
            case CaseStatus.testing:
                //imgStatus.sprite = sprTesting;
                imgStatus.gameObject.SetActive(false);
                imgTesting.gameObject.SetActive(true);
                if (animation)
                    coroutineTesting =   PTestAnimatorManager.Instance.StarImageTestingAnimation(imgTesting);
                break;

        }
    }

    private void OnEnable()
    {
        
    }

    public IEnumerator ImageAnimation()
    {
        while (true) {
            //imgStatus.transform.Rotate(new Vector3(90, 0, 0) * Time.deltaTime);
            imgStatus.transform.Rotate(0, 0, 25 * Time.deltaTime, Space.World);
            yield return new WaitForFixedUpdate();
        }

    }


    public IEnumerator ImageTestingAnimation()
    {
        float time = 0;
        while (true)
        {
            imgTesting.fillAmount = time;
            yield return new WaitForSecondsRealtime(0.1f);
            time += 0.1f;
            if (time >= 1)
            {
                time = 0;
            }
        }

    }

    // Update is called once per frame
    void Update()
    {


    }

  
}


public enum CaseStatus
{
    None,
    wait,
    successed,
    faild,
    testing
}
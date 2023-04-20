using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTestUIManager : Singleton<PTestUIManager>
{
    public Canvas uiCanvas;

    public PTestUI testUI;

    public GameObject animator;
    // Start is called before the first frame update
    void Start()
    {

    }


    public void Init()
    {
        InitCanvas();
        testUI.Init();
    }

    public void InitCanvas()
    {
        //uiCanvas.worldCamera = Camera.main;
        uiCanvas.sortingOrder = PTestConfig.TestLayer;
    }

    public void HidePlane()
    {
        testUI.planeMain.gameObject.SetActive(true);
        testUI.planeSearch.gameObject.SetActive(false);
        testUI.planeAffirm.gameObject.SetActive(false);
        testUI.uiMain.SetActive(false);
    }

    public void StartTest()
    {
        animator.SetActive(true);
    }

    public void CaseStart (){
    
    }
    public void EndTest()
    {
        animator.SetActive(false);
    }
}


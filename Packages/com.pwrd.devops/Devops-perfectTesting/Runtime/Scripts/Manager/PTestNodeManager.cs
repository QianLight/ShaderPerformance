using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTestNodeManager : Singleton<PTestNodeManager>
{

    public BPTestNode testNode;
    public string extend_config = "";
    public Dictionary<string,string> extend_list = new Dictionary<string, string>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    { 
        //Debug.Log(Input.mousePosition.x + " " + Input.mousePosition.y);

    }

    public void FindGameObjectLooper()
    {


    }

    public  Vector2 WorldToScreenPoint(Camera ScenceCamera, Vector3 worldPoint)
    {
        return RectTransformUtility.WorldToScreenPoint(ScenceCamera, worldPoint);
    }


    public void Looper(Func<bool> checkFunc, Action successed, Action failed, int outTime)
    {
        StartCoroutine(_Looper(checkFunc, successed, failed, outTime));
    }


    public IEnumerator _Looper(Func<bool> checkFunc ,Action successed,Action failed,int outTime)
    {
        int timer = 0;
        bool check = !checkFunc.Invoke();
        while (check && timer < outTime)
        {
            timer++;

            yield return new WaitForSecondsRealtime(1);
            check = !checkFunc.Invoke();

        }

        if (check)
        {
            successed?.Invoke();

        }
        else
        {
            failed?.Invoke();
        }

    }
}

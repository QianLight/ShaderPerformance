using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


public class IBaseTestNode
{
    /// <summary>
    /// 
    /// </summary>
    public  void SetUpClass()
    {

    }

    /// <summary>
    /// 
    /// </summary>
    public  void TearDownClass()
    {

    }

    /// <summary>
    /// 
    /// </summary>
    public  void SetUpMethod()
    {

    }

    /// <summary>
    /// 
    /// </summary>
    public  void TearDownMethod()
    {

    }

    public static GameObject GetGameObject(string objname)
    {
        GameObject Target = GameObject.Find(objname).transform.gameObject;

        return Target;
    }

    public static C GetGameObject<C>(string objname) where C : Component
    {
        GameObject Target = GameObject.Find(objname).transform.gameObject;

        return Target.GetComponent<C>();
    }


    public static void GetMethodDetail()
    {
        var stacktrace = new StackTrace(true);
        var methodname = stacktrace.GetFrame(1).GetMethod().Name;
        var classname = stacktrace.GetFrame(1).GetMethod().DeclaringType.Name;
        var fullname = stacktrace.GetFrame(1).GetFileName();
        //UnityEngine.Debug.Log(fullname);
        PTestUtils.AddCaseSteps(methodname , fullname,"" , true);
    }




}

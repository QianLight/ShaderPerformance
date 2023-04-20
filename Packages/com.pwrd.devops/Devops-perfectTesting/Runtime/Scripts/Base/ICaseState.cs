using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ICaseState : IState
{
    public CaseItemData caseItemData;
    public State_Enum state_Enum = State_Enum.Wait;
    private object CaseObj = null;
    public bool CaseFinish = false;

    public void OnEnter(Action action)
    {
        action?.Invoke();

        //PTestManager.Instance.caseObj.AddComponent(caseItemData.type);
        Type type = caseItemData.type;
        MethodInfo method = type.GetMethod("Start") ;
        CaseObj = Activator.CreateInstance(type);
        object[] parameters = new object[] { };
        method.Invoke(CaseObj, null);

        state_Enum = State_Enum.Running;
    }

    public void OnExit()
    {
        //caseClass = PTestManager.Instance.caseObj.AddComponent(caseItemData.type) as BPTestNode;
        Type type = caseItemData.type;
        MethodInfo method = type.GetMethod("OnDestroy");
        object[] parameters = new object[] { };
        method.Invoke(CaseObj, null);
        state_Enum = State_Enum.Finished;
        CaseObj = null;
        PTestNetManager.Instance.JankEvent = null;

    }

    public void OnUpdate()
    {
       if (CaseFinish)
        {
            state_Enum = State_Enum.Finished;
        }
    }

}

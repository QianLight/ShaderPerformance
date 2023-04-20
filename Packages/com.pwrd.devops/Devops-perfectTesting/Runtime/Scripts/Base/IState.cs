using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    //进入状态时调用一次
    void OnEnter(Action action);

    //处于状态时，连续调用
    void OnUpdate();

    //退出状态时调用一次
    void OnExit();

}


/// <summary>
/// 状态枚举类型
/// </summary>
public enum State_Enum
{
    Wait,
    Running,
    Finished
}
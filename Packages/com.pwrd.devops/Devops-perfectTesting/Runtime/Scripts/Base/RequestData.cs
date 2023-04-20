using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class RequestData<T>
{
    public bool result;
    public string msg;
    [SerializeField]
    public T data;
}

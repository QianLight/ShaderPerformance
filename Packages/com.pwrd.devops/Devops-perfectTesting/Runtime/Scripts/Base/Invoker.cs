using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devops.Test { 

public class Invoker : MonoBehaviour
{
    static Invoker _instance;
    public static void InvokeInMainThread(System.Action _delegate)
    { _instance.delegates.Add(_delegate); }

    public List<System.Action> delegates = new List<System.Action>();

    private void Awake()
    { _instance = this; }

    void Update()
    { Execute(); }

    void Execute()
    {
        if (delegates.Count == 0)
            return;
        for (int i = 0; i < delegates.Count; i++)
            delegates[i]();
        delegates.Clear();
    }

}
}

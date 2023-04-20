using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class EditorTaskProxy : MonoBehaviour
{
    private static EditorTaskProxy m_Instance;

    public static EditorTaskProxy Instance
    {
        get
        {
            if (m_Instance == null)
            {
                var go = new GameObject();
                go.name = "__TASK_PROXY__";
                go.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInBuild;
                m_Instance = go.AddComponent<EditorTaskProxy>();
            }

            return m_Instance;
        }
    }

    private Dictionary<int, Stack<IEnumerator>> m_map = new Dictionary<int, Stack<IEnumerator>>();
    private int m_idCounter = -1;

    private List<Stack<IEnumerator>> m_taskList = new List<Stack<IEnumerator>>();
    private List<Stack<IEnumerator>> m_endedList = new List<Stack<IEnumerator>>();
    private Dictionary<object, long> m_timerMap = new Dictionary<object, long>();

    public void Awake()
    {
        EditorApplication.update += EditorUpdate;
    }

    private void OnDestroy()
    {
        EditorApplication.update -= EditorUpdate;
    }

    private void EditorUpdate()
    {
        for (int i = 0; i < m_taskList.Count; i++)
        {
            var stack = m_taskList[i];
            try
            {
                var task = stack.Peek();
                if (m_timerMap.ContainsKey(task))
                {
                    var timer = m_timerMap[task];
                    var delta = DateTime.Now.ToFileTime() - timer;
                    var wait = task.Current as WaitForSeconds;
                    var waitTime = GetField<float>(wait, "m_Seconds");
                    if ((float)delta / 10000000f > waitTime)
                    {
                        m_timerMap.Remove(task);
                    }
                    else
                    {
                        continue;
                    }
                }

                var r = task.MoveNext();
                if (!r)
                {
                    stack.Pop();
                    if (stack.Count == 0)
                        m_endedList.Add(stack);
                }
                else
                {
                    var cur = task.Current;
                    if (cur is IEnumerator)
                    {
                        stack.Push(cur as IEnumerator); 
                    }

                    if (cur is WaitForSeconds)
                    {
                        m_timerMap[task] = DateTime.Now.ToFileTime();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                continue;
            }
        }

        foreach (var stack in m_endedList)
        {
            m_taskList.Remove(stack);
        }

        m_endedList.Clear();
    }

    private int AllocateID()
    {
        m_idCounter++;
        return m_idCounter;
    }

    private void Update()
    {
        //Debug.Log("Task Proxy Update");
    }

    private void FixedUpdate()
    {
        //Debug.Log("Task Proxy FixedUpdate");
    }


    public int StartTask(IEnumerator itor)
    {
        var stack = new Stack<IEnumerator>();
        stack.Push(itor);
        m_taskList.Add(stack);
        var id = AllocateID();
        m_map[id] = stack;
        return id;
    }

    public void StopTask(int id)
    {
        Stack<IEnumerator> stack = null;
        if (m_map.TryGetValue(id, out stack))
        {
            m_endedList.Add(stack);
        }
    }

    private static T GetField<T>(object instance, string fieldname)
    {
        BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        return (T) instance.GetType().GetField(fieldname, bindingAttr).GetValue(instance);
    }


    public static void Test()
    {
        var id = Instance.StartTask(TestLoop());
        Instance.StartTask(TestStopLoop(id));
    }

    private static IEnumerator TestLoop()
    {
        while (true)
        {
            Debug.Log("TestLoop");
            yield return null;
        }
    }

    private static IEnumerator TestStopLoop(int id)
    {
        yield return new WaitForSeconds(10);

        EditorTaskProxy.Instance.StopTask(id);
    }
}
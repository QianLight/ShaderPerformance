using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class GlobalFunc
{
#if PROFILER
    
    //消耗统计
    private static Dictionary<string, float> timeStack = new Dictionary<string, float>();
#endif

    public static void BeginSample(string name)
    {
#if PROFILER

        float startSampleTime = Time.realtimeSinceStartup * 1000;
        if (timeStack.ContainsKey(name))
        {
            timeStack[name] = startSampleTime;
        }
        else
        {
            timeStack.Add(name, startSampleTime);
        }
#endif
    }

    public static void EndSample(string name)
    {
#if PROFILER

        if(timeStack.Count == 0)
            return;
        
        if (!timeStack.ContainsKey(name))
        {
            return;
        }

        float startSampleTime = timeStack[name];
        Debug.Log( name + "耗时 ：" + (Time.realtimeSinceStartup * 1000 - startSampleTime) + "毫秒");
        timeStack.Remove(name);
#endif
    }
}
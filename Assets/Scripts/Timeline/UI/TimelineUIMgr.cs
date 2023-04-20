using CFUtilPoolLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.CFUI;
using UnityEngine.U2D;

public interface ITimelineUI
{
    bool isShow { get; }

    void Clean();

    void Return();

    GameObject GetUIRoot();

}

public class TimelineUIMgr
{

    static HashSet<ITimelineUI> stacks = new HashSet<ITimelineUI>();

    private static Canvas _canvas;

    private static bool _initial= false;

    public static Canvas canvas
    {
        get
        {
            if (_canvas == null)
            {
                GameObject cans = GameObject.Find("Canvas");
                if (cans)
                {
                    _canvas = cans.GetComponent<Canvas>();
                }
            }
            return _canvas;
        }
    }

    private static void Initial()
    {
        _initial = true;
    }


    public static void Regist(ITimelineUI ui)
    {
        if (!_initial)
        {
            Initial();
        }
        stacks.Add(ui);
    }

    public static void ReturnAll()
    {
        var e = stacks.GetEnumerator();
        while (e.MoveNext())
        {
            e.Current?.Return();
        }
        e.Dispose();
        stacks.Clear();
    }
}
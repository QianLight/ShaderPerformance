/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;
using Zeus;

class AppRoot : MonoBehaviour
{
    static private AppRoot _Instance = null;

    const float ASYNC_INTERVAL = 0.5f;


    public static AppRoot Instance
    {
        get { return _Instance; }
    }

    private long m_FrameNumber;

    public long FrameNumber
    {
        get { return m_FrameNumber; }
    }

    void Awake()
    {
        _Instance = this;
        DontDestroyOnLoad(this);

        m_FrameNumber = 0;

        //zLog.Instance.Init(true, true);
    }

    void Start()
    {

    }

    void Update()
    {
        m_FrameNumber++;

        // Manage asynchronous tasks.
        AsyncManager.Instance.Update();
        // Timer is ticking.
        Timers.Instance.Update();
    }

    void OnDestory()
    {
        Destroy();
        _Instance = null;
    }

    private void Destroy()
    {

    }

    private void _InitMemoryAllocation(int size)
    {
        byte[] buff = new byte[size];
        buff = null;
    }
}

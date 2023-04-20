using GMSDK;
using UnityEngine;
using System.IO;
using System;

public class GMPushMgr : ServiceSingleton<GMPushMgr>
{
    private BasePushSDK m_sdk;
    public BasePushSDK SDK { get { return this.m_sdk; } }

    private GMPushMgr()
    {
        if (m_sdk == null)
        {
            m_sdk = new BasePushSDK();
        }
    }
}
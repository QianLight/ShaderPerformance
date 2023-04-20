using GMSDK;
using UnityEngine;
using System.IO;
using System;


public class GMMagicBoxMgr : ServiceSingleton<GMMagicBoxMgr>
{
    private BaseMagicBoxSDK m_sdk;
    public BaseMagicBoxSDK SDK { get { return this.m_sdk; } }

    private GMMagicBoxMgr()
    {
        if (m_sdk == null)
        {
            m_sdk = new BaseMagicBoxSDK();
        }
    }
}

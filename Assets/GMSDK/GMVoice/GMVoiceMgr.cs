using GMSDK;
using UnityEngine;
using System.IO;
using System;


public class GMVoiceMgr : ServiceSingleton<GMVoiceMgr>
{
    private BaseVoiceSDK m_sdk;
    public BaseVoiceSDK SDK { get { return this.m_sdk; } }

    private GMVoiceMgr()
    {
        if (m_sdk == null)
        {
            m_sdk = new BaseVoiceSDK();
        }
    }
}

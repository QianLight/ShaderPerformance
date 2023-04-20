using GMSDK;
using UnityEngine;
using System.IO;
using System;


public class GMRTCMgr : GMSDK.ServiceSingleton<GMRTCMgr>
{
    private BaseRTCSDK _sdk;
    public BaseRTCSDK SDK { get { return this._sdk; } }

    private GMRTCMgr()
    {
        if (_sdk == null)
        {
            _sdk = new BaseRTCSDK();
        }
    }
}
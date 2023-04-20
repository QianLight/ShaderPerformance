using GMSDK;
using UnityEngine;
using System.IO;
using System;


public class GMShareMgr : ServiceSingleton<GMShareMgr>
{
	private ShareSDK shareSdk;
	public ShareSDK sSDK { get { return this.shareSdk; } }
	
    private GMShareMgr()
    {
	    if (shareSdk == null) {
		    shareSdk = new ShareSDK ();
	    }
    }
}

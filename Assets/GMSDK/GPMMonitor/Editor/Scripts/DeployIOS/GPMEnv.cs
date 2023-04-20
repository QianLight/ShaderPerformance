using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;


public class GPMEnv
{
	public readonly string PATH_GPM_EDITOR;
	public readonly string PATH_GPM_LIBRARYS_IOS;
	public readonly string PATH_GPM_LIBRARYS_ANDROID;

	static GPMEnv instance;

	public static GPMEnv Instance
    {
        get
        {
            if (instance == null) {
				instance = new GPMEnv();
            }
            return instance;
        }
    }

	public GPMEnv()
    {
		PATH_GPM_EDITOR = GMSDKEnv.Instance.SubEditorPath(@"GPMMonitor");
		PATH_GPM_LIBRARYS_IOS = Path.Combine(PATH_GPM_EDITOR, @"Librarys/iOS");
		PATH_GPM_LIBRARYS_ANDROID = Path.Combine(PATH_GPM_EDITOR, @"Librarys/Android");
    }
}

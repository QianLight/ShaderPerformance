using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;


public class GMLocationEnv
{
	public readonly string PATH_Location_EDITOR;
	public readonly string PATH_Location_LIBRARYS_IOS;
	public readonly string PATH_Location_LIBRARYS_ANDROID;

	static GMLocationEnv instance;

	public static GMLocationEnv Instance
    {
        get
        {
            if (instance == null) {
				instance = new GMLocationEnv();
            }
            return instance;
        }
    }

	public GMLocationEnv()
    {
		PATH_Location_EDITOR = GMSDKEnv.Instance.SubEditorPath(@"GMLocation");
        PATH_Location_LIBRARYS_IOS = Path.Combine(PATH_Location_EDITOR, @"Librarys/iOS");
        PATH_Location_LIBRARYS_ANDROID = Path.Combine(PATH_Location_EDITOR, @"Librarys/Android");
    }
}

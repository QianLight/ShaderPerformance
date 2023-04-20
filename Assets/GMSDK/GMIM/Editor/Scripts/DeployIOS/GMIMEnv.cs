using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;


public class GMIMEnv
{
	public readonly string PATH_IM_EDITOR;
	public readonly string PATH_IM_LIBRARYS_IOS;
	public readonly string PATH_IM_LIBRARYS_ANDROID;

	static GMIMEnv instance;

	public static GMIMEnv Instance
    {
        get
        {
            if (instance == null) {
				instance = new GMIMEnv();
            }
            return instance;
        }
    }

	public GMIMEnv()
    {
		PATH_IM_EDITOR = GMSDKEnv.Instance.SubEditorPath(@"GMIM");
        PATH_IM_LIBRARYS_IOS = Path.Combine(PATH_IM_EDITOR, @"Librarys/iOS");
        PATH_IM_LIBRARYS_ANDROID = Path.Combine(PATH_IM_EDITOR, @"Librarys/Android");
    }
}

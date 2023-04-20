using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;


public class GMShareEnv
{
	public readonly string PATH_SHARE_EDITOR;
	public readonly string PATH_SHARE_LIBRARYS_IOS;
	public readonly string PATH_SHARE_LIBRARYS_ANDROID;
	public readonly string PATH_SHARE_IOS_PLIST;

	static GMShareEnv instance;

	public static GMShareEnv Instance
    {
        get
        {
            if (instance == null) {
				instance = new GMShareEnv();
            }
            return instance;
        }
    }

	public GMShareEnv()
    {
		PATH_SHARE_EDITOR = GMSDKEnv.Instance.SubEditorPath(@"GMShare");
		PATH_SHARE_LIBRARYS_IOS = Path.Combine(PATH_SHARE_EDITOR, @"Librarys/iOS");
		PATH_SHARE_LIBRARYS_ANDROID = Path.Combine(PATH_SHARE_EDITOR, @"Librarys/Android");
		PATH_SHARE_IOS_PLIST = Path.Combine(PATH_SHARE_EDITOR, @"Resources/GMSDKInfo.plist");
    }
}

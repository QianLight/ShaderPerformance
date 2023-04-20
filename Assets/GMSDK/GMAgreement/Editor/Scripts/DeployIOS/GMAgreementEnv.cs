using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class GMAgreementEnv {
	public readonly string PATH_Agreement_EDITOR;
	public readonly string PATH_Agreement_LIBRARYS_IOS;
	public readonly string PATH_Agreement_LIBRARYS_ANDROID;

	static GMAgreementEnv instance;

	public static GMAgreementEnv Instance
	{
		get
		{
			if (instance == null) {
				instance = new GMAgreementEnv();
			}
			return instance;
		}
	}

	public GMAgreementEnv()
	{
		PATH_Agreement_EDITOR = GMSDKEnv.Instance.SubEditorPath(@"GMAgreement");
		PATH_Agreement_LIBRARYS_IOS = Path.Combine(PATH_Agreement_EDITOR, @"Librarys/iOS");
		PATH_Agreement_LIBRARYS_ANDROID = Path.Combine(PATH_Agreement_EDITOR, @"Librarys/Android");
	}
}

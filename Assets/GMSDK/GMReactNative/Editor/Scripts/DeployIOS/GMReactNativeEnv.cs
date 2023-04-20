using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class GMReactNativeEnv {
	public readonly string PATH_ReactNative_EDITOR;
	public readonly string PATH_ReactNative_LIBRARYS_IOS;
	public readonly string PATH_ReactNative_LIBRARYS_ANDROID;

	static GMReactNativeEnv instance;

	public static GMReactNativeEnv Instance
	{
		get
		{
			if (instance == null) {
				instance = new GMReactNativeEnv();
			}
			return instance;
		}
	}

	public GMReactNativeEnv()
	{
		PATH_ReactNative_EDITOR = GMSDKEnv.Instance.SubEditorPath(@"GMReactNative");
		PATH_ReactNative_LIBRARYS_IOS = Path.Combine(PATH_ReactNative_EDITOR, @"Librarys/iOS");
		PATH_ReactNative_LIBRARYS_ANDROID = Path.Combine(PATH_ReactNative_EDITOR, @"Librarys/Android");
	}
}

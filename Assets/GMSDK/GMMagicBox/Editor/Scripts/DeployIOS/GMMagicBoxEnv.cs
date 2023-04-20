using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;


public class GMMagicBoxEnv
{
    public readonly string PATH_MAGICBOX_EDITOR;
    public readonly string PATH_MAGICBOX_LIBRARYS_IOS;
    public readonly string PATH_MAGICBOX_LIBRARYS_ANDROID;

    static GMMagicBoxEnv instance;

    public static GMMagicBoxEnv Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GMMagicBoxEnv();
            }
            return instance;
        }
    }

    public GMMagicBoxEnv()
    {
        PATH_MAGICBOX_EDITOR = Path.Combine(GMSDKEnv.Instance.MAIN_GSDK_FOLDER, @"GMMagicBox/Editor");
        PATH_MAGICBOX_LIBRARYS_IOS = Path.Combine(GMSDKEnv.Instance.MAIN_GSDK_FOLDER, @"GMMagicBox/Editor/Librarys/iOS");
        PATH_MAGICBOX_LIBRARYS_ANDROID = Path.Combine(GMSDKEnv.Instance.MAIN_GSDK_FOLDER, @"GMMagicBox/Editor/Librarys/Android");
    }
}

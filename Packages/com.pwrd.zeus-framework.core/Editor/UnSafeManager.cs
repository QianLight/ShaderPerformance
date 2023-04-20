/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
[InitializeOnLoad]
public static class UnSafeManager
{
    static UnSafeManager()
    {
        IsUnSafe = File.Exists(UNSAFE_FILE_PATH);
        IsUnSafeEditor = File.Exists(UNSAFE_EDITOR_FILE_PATH);
    }
    #region unsafe
    const string UNSAFE_FILE_PATH = "Assets/smcs.rsp";
    const string UNSAFE_MENU_PATH = "Zeus/Unsafe/Unsafe";

    public static bool IsUnSafe { get; private set; }

    //[MenuItem(UNSAFE_MENU_PATH)]
    public static void ChangeUnSafeMode()
    {
        const string content = "-unsafe";
        if (!File.Exists(UNSAFE_FILE_PATH))
        {
            //  开启unsafe
            File.WriteAllText(UNSAFE_FILE_PATH, content);
            AssetDatabase.Refresh();
            IsUnSafe = true;
        }
        else
        {
            //  关闭unsafe
            File.Delete(UNSAFE_FILE_PATH);
            AssetDatabase.Refresh();
            IsUnSafe = false;
        }
    }

    //[MenuItem(UNSAFE_MENU_PATH, true)]
    private static bool UnSafeMenuCheck()
    {
        Menu.SetChecked(UNSAFE_MENU_PATH, IsUnSafe);
        return true;
    }
    #endregion

    #region unsafe editor
    const string UNSAFE_EDITOR_FILE_PATH = "Assets/gmcs.rsp";
    const string UNSAFE_EDITOR_MENU_PATH = "Zeus/Unsafe/Unsafe (Editor)";

    public static bool IsUnSafeEditor { get; private set; }

    //[MenuItem(UNSAFE_EDITOR_MENU_PATH)]
    public static void ChangeUnSafeEditorMode()
    {
        const string content = "-unsafe";
        if (!File.Exists(UNSAFE_EDITOR_FILE_PATH))
        {
            //  开启unsafe
            File.WriteAllText(UNSAFE_EDITOR_FILE_PATH, content);
            AssetDatabase.Refresh();
            IsUnSafeEditor = true;
        }
        else
        {
            //  关闭unsafe
            File.Delete(UNSAFE_EDITOR_FILE_PATH);
            AssetDatabase.Refresh();
            IsUnSafeEditor = false;
        }
    }

    //[MenuItem(UNSAFE_EDITOR_MENU_PATH, true)]
    private static bool UnSafeEditorMenuCheck()
    {
        Menu.SetChecked(UNSAFE_EDITOR_MENU_PATH, IsUnSafeEditor);
        return true;
    }
    #endregion
}
#endif
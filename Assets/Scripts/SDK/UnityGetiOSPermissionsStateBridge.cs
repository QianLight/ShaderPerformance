using System.Runtime.InteropServices;
using UnityEngine;

//参考: https://www.jianshu.com/p/b74b8e388265

/// <summary>
/// iOS权限静态类
/// </summary>
public class UnityGetiOSPermissionsStateBridge
{
    private static UnityGetiOSPermissionsStateBridge instance;
#if UNITY_IOS
    /// <summary>
    /// 首次调用获取麦克风权限
    /// </summary>
    [DllImport("__Internal")]
    public static extern void GetMicrophonePermission();

    /// <summary>
    /// 获取麦克风权限准许状态
    /// </summary>
    [DllImport("__Internal")]
    public static extern int GetMicrophonePermissionState();

    /// <summary>
    /// 获取麦克风权限准许状态
    /// </summary>
    [DllImport("__Internal")]
    public static extern int GetLocationPermissionState();

    /*
    /// <summary>
    /// 首次调用获取相机权限
    /// </summary>
    [DllImport("__Internal")]
    public static extern void GetCameraPermission();

    /// <summary>
    /// 获取相机权限准许状态
    /// </summary>
    [DllImport("__Internal")]
    public static extern bool GetCameraPermissionState();

    /// <summary>
    /// 首次调用获取相册权限
    /// </summary>
    [DllImport("__Internal")]
    public static extern void GetPhotoPermission();

    /// <summary>
    /// 获取相册权限准许状态
    /// </summary>
    [DllImport("__Internal")]
    public static extern bool GetPhotoPermissionState();
    */
    /// <summary>
    /// 打开本程序的设置
    /// </summary>
    [DllImport("__Internal")]
    public static extern void OpenAppSettings();
#endif

    private UnityGetiOSPermissionsStateBridge()
    {
#if UNITY_IOS
        Debug.Log("UnityGetiOSPermissionsStateBridge init");
        UnityGetiOSPermissionsState.GetMicrophonePermission = GetMicrophonePermission;
        UnityGetiOSPermissionsState.GetMicrophonePermissionState = GetMicrophonePermissionState;
        UnityGetiOSPermissionsState.OpenAppSettings = OpenAppSettings;
        UnityGetiOSPermissionsState.GetLocationPermissionState = GetLocationPermissionState;
#endif
    }

    public static void Init()
    {
        if(Application.platform == RuntimePlatform.IPhonePlayer && instance == null)
        {
            instance = new UnityGetiOSPermissionsStateBridge();
        }
    }
}
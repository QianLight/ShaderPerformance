using System.Runtime.InteropServices;
using UnityEngine;

//�ο�: https://www.jianshu.com/p/b74b8e388265

/// <summary>
/// iOSȨ�޾�̬��
/// </summary>
public class UnityGetiOSPermissionsStateBridge
{
    private static UnityGetiOSPermissionsStateBridge instance;
#if UNITY_IOS
    /// <summary>
    /// �״ε��û�ȡ��˷�Ȩ��
    /// </summary>
    [DllImport("__Internal")]
    public static extern void GetMicrophonePermission();

    /// <summary>
    /// ��ȡ��˷�Ȩ��׼��״̬
    /// </summary>
    [DllImport("__Internal")]
    public static extern int GetMicrophonePermissionState();

    /// <summary>
    /// ��ȡ��˷�Ȩ��׼��״̬
    /// </summary>
    [DllImport("__Internal")]
    public static extern int GetLocationPermissionState();

    /*
    /// <summary>
    /// �״ε��û�ȡ���Ȩ��
    /// </summary>
    [DllImport("__Internal")]
    public static extern void GetCameraPermission();

    /// <summary>
    /// ��ȡ���Ȩ��׼��״̬
    /// </summary>
    [DllImport("__Internal")]
    public static extern bool GetCameraPermissionState();

    /// <summary>
    /// �״ε��û�ȡ���Ȩ��
    /// </summary>
    [DllImport("__Internal")]
    public static extern void GetPhotoPermission();

    /// <summary>
    /// ��ȡ���Ȩ��׼��״̬
    /// </summary>
    [DllImport("__Internal")]
    public static extern bool GetPhotoPermissionState();
    */
    /// <summary>
    /// �򿪱����������
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
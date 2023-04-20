using System;
using System.Runtime.InteropServices;

/// <summary>
/// Agreement 一级错误码使用：GSDK.ErrorCode.AgreementXXX
/// 具体内容请参考 Assets/GSDK/Service/Agreement/IAgreementService.cs 中的 ErrorCode 部分
/// </summary>
public static class Client
{
    public enum CRYPTOTYPE
    {
        RC4 = 1,
        AES_CBC_128 = 2,
        AES_CBC_192 = 3,
        AES_CBC_256 = 4,
    }

#if UNITY_IOS && !UNITY_EDITOR
        const string dll = "__Internal";
#elif UNITY_EDITOR || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    const string dll = "libtps_client";
#elif UNITY_ANDROID
    const string dll = "tps_client";
#endif

    [DllImport(dll)]
    public static extern int TpsClientSdkInit(byte[] cfgBuf, UInt32 cfgLen, CRYPTOTYPE cryptoMethod);

    [DllImport(dll)]
    public static extern void TpsClientSdkClose();

    [DllImport(dll)]
    public static extern int TpsClientSdkEncrypt(byte[] inData, UInt32 inLen, byte[] outBuf,
        ref UInt32 outBufMaxSizes);

    [DllImport(dll)]
    public static extern int TpsClientSdkDecrypt(byte[] inData, UInt32 inLen, byte[] outBuf,
        ref UInt32 outBufMaxSizes);

    [DllImport(dll)]
    public static extern int TpsClientSdkVersion(byte[] buf, UInt32 bufSizes);
}
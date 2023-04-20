using System;
using System.Runtime.InteropServices;

public class NativeRead
{
#if UNITY_ANDROID
    public const string libName = "Utility";

    [DllImport(libName)]
    public static extern int ReadAssetsBytes(string name, ref IntPtr ptr);

    [DllImport(libName)]
    public static extern int ReadAssetsBytesWithOffset(string name, ref IntPtr ptr, int offset, int length);
   

    [DllImport(libName)]
    public static extern int ReadRawBytes(string name, ref IntPtr ptr);

    [DllImport(libName)]
    public static extern void ReleaseBytes(IntPtr ptr);

#endif

}
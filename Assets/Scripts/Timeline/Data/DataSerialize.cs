using System.Collections;
using System.Collections.Generic;
using System.IO;
using CFEngine;
using CFUtilPoolLib;
using UnityEngine;
public partial class DataSerialize
{
#if UNITY_EDITOR
    public static void Save (BinaryWriter bw, TimelineData data)
    {

    }
#endif
    private static string GetStr (string[] strPool, int index)
    {
        if (strPool != null && index >= 0 && index < strPool.Length)
        {
            return strPool[index];
        }
        return "";
    }
    
    public static void Load (CFBinaryReader reader, TimelineData data)
    {

    }

}
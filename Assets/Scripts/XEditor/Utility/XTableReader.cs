#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using CFUtilPoolLib;
using CFEngine;

public class XTableReader
{
    public static bool ReadFile(string location, CVSReader reader)
    {
        CVSReader.Init();
        CFBinaryReader.Init();
        return reader.ReadTable(location);
    }
}
#endif
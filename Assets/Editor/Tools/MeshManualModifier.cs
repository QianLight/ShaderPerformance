
using System;
using System.Collections.Generic;
using System.IO;
using CFEngine;
using UnityEditor;
using UnityEngine;
public class MeshManualModifier:Editor
{
    [MenuItem("ArtTools/SFX/SetMeshReadableModifier")]
    public static void SetMeshReadable()
    {
        // DebugLog.AddLog2(Selection.activeTransform.ToString());
        var rootPath = "Assets/Effects/Mesh";
        List<Mesh> fxMeshes = new List<Mesh>();
        DirectoryInfo dir = new DirectoryInfo(Application.dataPath + "/" + rootPath.Replace("Assets/", ""));
        DirectoryInfo[] infos = dir.GetDirectories();
        FileInfo[] rootfiles = dir.GetFiles();
        foreach (var info in infos)
        {
            FileInfo[] fis = info.GetFiles("*.asset", SearchOption.AllDirectories);
            for (int i = 0; i < fis.Length; i++)
            {
                FileInfo fi = fis[i];
                if (fi.Extension.Equals(".asset"))
                {
                    string strings = File.ReadAllText(fi.FullName);
                    strings = strings.Replace("m_IsReadable: 0", "m_IsReadable: 1");
                    File.WriteAllText(fi.FullName, strings);
                    Debug.Log($"Write{fi.FullName}");
                }
            }
        }

        for (int i = 0; i < rootfiles.Length; i++)
        {
            if (rootfiles[i].Extension.Equals(".asset"))
            {
                string strings = File.ReadAllText(rootfiles[i].FullName);
                strings = strings.Replace("m_IsReadable: 0", "m_IsReadable: 1");
                File.WriteAllText(rootfiles[i].FullName, strings);
                Debug.Log($"Write{rootfiles[i].FullName}");
            }
        }
    }
}

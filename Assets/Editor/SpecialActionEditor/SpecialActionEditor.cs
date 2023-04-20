using CFUtilPoolLib;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using XEditor;

public class SpecialActionEditor : EditorWindow
{

    [MenuItem("XEditor/SpecialAction/Build")]
    static void Build()
    {
        GameObject[] goList = GameObject.FindGameObjectsWithTag("SpecialAction");

        string data = "";
        for(int i=0;i<goList.Length;++i)
        {
            if (i != 0) data += "\n";

            uint type = uint.Parse(goList[i].name);
            switch((SpecialActionType)type)
            {
                case SpecialActionType.RopeLeap:
                case SpecialActionType.LongRopeLeap:
                    {
                        data += BuildRopeLeap(goList[i]);
                    }break;
                default:
                    Debug.LogError("SpecialAction Type Error!!!" + "   " + goList[i].name);
                    return;
            }
        }

        string path = EditorUtility.SaveFilePanel("Select a file to save", XEditorPath.Cli, "SpecialAction_.txt", "txt");

        if (!string.IsNullOrEmpty(path))
        {
            StreamWriter sw = File.CreateText(path);
            sw.WriteLine("{0}", data);
            sw.Flush();
            sw.Close();

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("SpecialAction", "Success", "OK");
        }
    }

    static string BuildRopeLeap(GameObject go)
    {
        string data = "";

        data += uint.Parse(go.name);
        data += " " + (int)(go.transform.position.x * 100 + 0.5);
        data += " " + (int)(go.transform.position.y * 100 + 0.5);
        data += " " + (int)(go.transform.position.z * 100 + 0.5);
        data += " " + (int)(XCommon.singleton.AngleToFloat(go.transform.forward) * 100 + 0.5);

        Vector3 hook = go.transform.Find("Hook").position;
        data += " " + (int)(hook.x * 100 + 0.5);
        data += " " + (int)(hook.y * 100 + 0.5);
        data += " " + (int)(hook.z * 100 + 0.5);

        return data;
    }

}

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using CFUtilPoolLib;
using System.Collections.Generic;
using System;
using System.Text;

namespace XEditor
{
    public class OneKeyRepairTool : MonoBehaviour
    {
        [MenuItem(@"Help/OneKeyRepair")]
        static void OneKeyRepair()
        {
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/Lib");
            EditorApplication.ExecuteMenuItem("Assets/Reimport");
            EditorApplication.ExecuteMenuItem("Tools/Ecs/Reimport");
            CFEngine.Editor.TableAssets.ExeTable2Bytes(string.Empty);
            EditorApplication.ExecuteMenuItem("Help/RestartUnity");
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("UGUI", "Canvas过多", "t:Prefab", "")]

    public class UICanvasCheck : RuleBase
    {
        [PublicParam("Canvas数量", eGUIType.Input)]
        public int canvasCount = 10;

        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            GameObject gObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (gObject == null)
                return true;
            Canvas[] canvases = gObject.GetComponentsInChildren<Canvas>();
            output = canvases.Length.ToString();
            return canvases.Length < canvasCount;
        }
    }
}
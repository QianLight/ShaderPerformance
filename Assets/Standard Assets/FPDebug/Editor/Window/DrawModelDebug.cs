using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;

public partial class FPDebugWindow
{
    GameObject drawModelObj = null;
    private void drawModelDebug()
    {
        if (drawModelObj == null)
        {
            GUILayout.Label("拖放GameObject到这里:");
        }

        GUILayout.Space(20);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Object:");
        drawModelObj = EditorGUILayout.ObjectField(drawModelObj, typeof(GameObject), true) as GameObject;
        if (GUILayout.Button("X"))
        {
            drawModelObj = null;
        }
        EditorGUILayout.EndHorizontal();

        onMouseDrag(getModelObjsct);
    }


    void getModelObjsct(UnityEngine.Object[] handleObjs)
    {
        for (int i = 0; i < handleObjs.Length; i++)
        {
            UnityEngine.Object handleObj = handleObjs[i];
            GameObject obj = handleObj as GameObject;
            
            if (obj != null)
            {
                drawModelObj = obj;
                break;
            }
        }
    }

}

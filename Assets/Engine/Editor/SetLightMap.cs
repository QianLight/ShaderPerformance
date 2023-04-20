using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CopyLightMapScale : MonoBehaviour
{
    
    [MenuItem("Tools/引擎/CopyLightMapScale #&L")] // & alt  #shift 
    static void Method()
    {
        //var logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
        //var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        //clearMethod.Invoke(null, null);
        doCount = 0;
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] objList = scene.GetRootGameObjects();
        foreach(GameObject o in objList)
        {
            FindGameobject(o);
        }
        Debug.Log("Copy Light MapScale count:" + doCount);
    }

    static int doCount = 0;
    static void GameobjectAction(GameObject g)
    {
        var obj = g.GetComponent<CFEngine.MeshRenderObject>();
        var mr = g.GetComponent<MeshRenderer>();
        if(obj && mr)
        {
            if(obj.LightmapScale > 0)
            {
                mr.scaleInLightmap = obj.LightmapScale;
                doCount++;
            }
        }
    }

    static void FindGameobject(GameObject g)
    {
        GameobjectAction(g);
        foreach (Transform o in g.transform)
        {
            GameObject gameObj = o.gameObject;
            FindGameobject(gameObj);
        }
    }

}

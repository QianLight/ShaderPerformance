
using UnityEngine;
using UnityEditor;
using System.Collections;
 
public class PrintNode :  Editor{
    [MenuItem("ArtTools/Print")]
    public static void Print() {
        GameObject obj = Selection.activeGameObject;
        string str = "";
        Check(obj.transform, "/", ref str);
        Debug.Log(str);
    }
 
    static void Check(Transform tf, string gap, ref string str) {
        str = tf.name+ gap+str;
        Transform item=tf.parent;
        //foreach (Transform item in tf) {
            if(item)
            {
                Check(item, gap + "", ref str);
                }
            
        //}
    }
}

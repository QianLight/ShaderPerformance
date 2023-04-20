using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DiyShipHelp 
{


    [MenuItem("Tools/DIY/CopyMaterial")]
    static void CopyMaterial()
    {
        if (Selection.gameObjects.Length != 2) return;




        List<string> allTags = new List<string>();

        allTags.Add("OP_CabinTop");
        allTags.Add("OP_Gun");
        allTags.Add("OP_Head");
        allTags.Add("OP_Head_02");
        allTags.Add("OP_LookOut");
        allTags.Add("OP_LookOut_02");
        allTags.Add("OP_LookOut_03");
        allTags.Add("OP_LookOut_04");
        allTags.Add("OP_Other");
        allTags.Add("OP_Other_02");
        allTags.Add("OP_Other_03");
        allTags.Add("OP_Other_04");
        allTags.Add("OP_Other_05");
        allTags.Add("OP_Other_06");
        allTags.Add("OP_Other_07");
        allTags.Add("OP_Railing");
        allTags.Add("OP_Side");
        allTags.Add("OP_Stern");
        allTags.Add("OP_Wing");
        allTags.Add("OP_Other_08");
        allTags.Add("OP_Other_09");
        allTags.Add("OP_Other_10");
        allTags.Add("OP_Other_11");
        allTags.Add("OP_Other_12");
        allTags.Add("OP_Other_13");
        allTags.Add("OP_Other_14");

        Transform srcTf = Selection.gameObjects[0].transform;
        Transform targetTf = Selection.gameObjects[1].transform;

        if (Selection.gameObjects[0].name.Contains("DIY"))
        {
            srcTf = Selection.gameObjects[1].transform;
            targetTf = Selection.gameObjects[0].transform;
        }


        
        Debug.Log(srcTf + "   " + targetTf);

        for (int i = 0; i < allTags.Count; i++)
        {
            string name = allTags[i].Replace("OP", string.Empty);

            MeshRenderer src = GetRenderByName(srcTf, name);
            MeshRenderer target = GetRenderByName(targetTf, name);

            if (src == null || target == null) continue;
            
            target.sharedMaterial = src.sharedMaterial;
        }
    }

    private static MeshRenderer GetRenderByName(Transform tf, string name)
    {
        for (int i = 0; i < tf.childCount; i++)
        {
            Transform childSrc = tf.GetChild(i);
            if (childSrc.name.Contains(name)) return childSrc.GetComponent<MeshRenderer>();
        }
        
        return null;
    }

}

using CFUtilPoolLib;
using UnityEditor;
using UnityEngine;
using XEditor;

[CustomEditor(typeof(XRoleParts))]
public class XPartsEditor : Editor
{

    private XRoleParts part;

    private void OnEnable()
    {
        part = target as XRoleParts;
    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Debug"))
        {
            SkinnedMeshRenderer[] sks = part.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int i = 0; i < sks.Length; i++)
            {
                var item = sks[i];
                var bs = item.bones;
                Debug.Log(string.Format("************** {0} ***************", item.name));
                for (int j = 0; j < bs.Length; j++)
                {
                    Debug.Log(j + ": " + bs[j].name);
                }
                if (item.name.Contains("hair"))
                {
                    var weights = sks[i].sharedMesh.boneWeights;
                    foreach (var it in weights)
                    {
                        Debug.Log(it);
                    }
                }
            }
        }
        if (GUILayout.Button("Bind Face"))
        {
            SkinnedMeshRenderer[] sks = part.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int i = 0; i < sks.Length; i++)
            {
                var item = sks[i];
                if (item.name.Contains("face"))
                {
                    Debug.Log(item.bones.Length);
                    part.face = item.bones;
                }
            }
        }
        GUILayout.EndHorizontal();
    }
}

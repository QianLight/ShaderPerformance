using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ControlPartAsset))]
public class ControlPartAssetEditor : Editor
{
    private SerializedObject m_so;
    private SerializedProperty m_time;
    private ControlPartAsset m_asset;


    private SerializedProperty m_boneProp;


    private void OnEnable()
    {
        m_asset = target as ControlPartAsset;
        m_so = new SerializedObject(m_asset);
    }

    public override void OnInspectorGUI()
    {
        GUILayout.BeginVertical();
        if (GUILayout.Button("add", GUILayout.Width(100)))
        {
            ControlPartAsset.PartInfo partInfo = new ControlPartAsset.PartInfo();
            partInfo.m_path = string.Empty;
            partInfo.m_enable = false;
            m_asset.m_partInfos.Add(partInfo);
        }

        for (int i = 0; i < m_asset.m_partInfos.Count; ++i)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(i.ToString() + ".", GUILayout.Width(50));
            m_asset.m_partInfos[i].m_path = GUILayout.TextField(m_asset.m_partInfos[i].m_path, GUILayout.Width(300));
            m_asset.m_partInfos[i].m_enable = GUILayout.Toggle(m_asset.m_partInfos[i].m_enable, "enable", GUILayout.Width(100));
            if (GUILayout.Button("x", GUILayout.Width(50)))
            {
                m_asset.m_partInfos.RemoveAt(i);
                break;
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
        m_so.ApplyModifiedProperties();
    }
}

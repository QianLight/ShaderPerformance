using UnityEngine;
using UnityEditor;

public class UIAssistantWindow : EditorWindow
{
    private int defaultDepth = UIAssistantTools.DefaultDepth;
    private int depthIdx = 1;
    private GameObject m_selectObj = null;
    private TreeNode m_treeRootNode = null;
    private Vector2 m_ScrollPos = Vector2.zero;


    public void ShowSelection()
    {
        if(Selection.activeGameObject == null) 
        {
            EditorUtility.DisplayDialog("Tips", "Select Object is null!", "close");
            return;
        }

        if (Selection.activeGameObject.layer == LayerMask.NameToLayer("UI")) 
        {
            m_selectObj = Selection.activeGameObject;
            Refresh();
        }
    }
    void OnGUI()
    {
        GUILabelType();
        GUI.color = Color.red;
        GUILayout.Label("HO-Hierarychy序列 | N-节点名 |  D-节点深度  |  M-材质ID  | T-TextureID  | IM-是否有Mask  | I2-是否有RectMask2D ");
        GUI.color = Color.white;
        UIAssistantTools.ShowSceneBox  = GUILayout.Toggle(UIAssistantTools.ShowSceneBox , " Show SceneBox: ");
        GUILabelType(TextAnchor.UpperLeft);
        GUILayout.Space(2);
        CreateSplit();
        
        GUILayout.BeginHorizontal();
        for(int i = 0; i < UIAssistantTools.m_colors.Length; ++i) 
        {
            GUI.color = UIAssistantTools.m_colors[i];
            GUILayout.Label(string.Format("{0} : ", i));
            GUILayout.Box("", GUILayout.Width(15), GUILayout.Height(15));
            GUILayout.Space(140);
        }

        GUILayout.EndHorizontal();
        GUI.color = Color.white;
        GUILabelType(TextAnchor.UpperLeft);
        GUILayout.Space(2);
        CreateSplit();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Catch"))
        {
            Catch();
        }
        if (GUILayout.Button("Refresh"))
        {
            Refresh();
        }
        if (GUILayout.Button("Clear"))
        {
            Reset();
        }
        GUILayout.EndHorizontal();

        GUILabelType(TextAnchor.UpperLeft);
        GUILayout.Space(2);
        CreateSplit();

        ShowCatchUI();
    }

    private void ShowCatchUI() 
    {
        if (m_selectObj != null) 
        {
            GUILabelType(TextAnchor.UpperLeft);
            GUILayout.Space(2);
            GUILayout.Label(m_treeRootNode == null ? "Result: " : string.Format("Result: {0}(Mask {1})", m_treeRootNode.batchCount, m_treeRootNode.maskBatchCount));
            GUILayout.Space(2);

            m_ScrollPos = GUI.BeginScrollView(new Rect(10, 180, position.width -20, position.height - 200), m_ScrollPos, new Rect(0, 0, m_treeRootNode.RecursiveSize.x, m_treeRootNode.RecursiveSize.y), true, true);
            m_treeRootNode.OnGUI();
            GUI.EndScrollView();
        }
    }

    private void Catch()
    {
        if(Selection.activeGameObject == null) 
        {
            EditorUtility.DisplayDialog("Tips", "Select Object is null!", "close");
            return;
        }

        if (Selection.activeGameObject.layer == LayerMask.NameToLayer("UI")) 
        {
            m_selectObj = Selection.activeGameObject;
            Refresh();
        }
    }

    private void Refresh()
    {
        if (m_treeRootNode != null)
        {
            m_treeRootNode.Destroy();
        }

        if (m_selectObj != null)
        {
            depthIdx = 1;
            m_treeRootNode = new TreeNode(m_selectObj.name, m_selectObj, depthIdx * defaultDepth);
            m_treeRootNode.IsRoot = true;
            GenChildNodes(m_treeRootNode, m_selectObj.transform);
            UIAssistantTools.GenTreeInfo(m_treeRootNode);
        }
    }

    private void GenChildNodes(TreeNode node, Transform transform)
    {
        if(transform.childCount > 0) 
        {
            int depth = 0;
            for (int i = 0; i < transform.childCount; ++i)
            {
                Transform child = transform.GetChild(i);
                if(child.gameObject.activeSelf) 
                {
                    depth = node.Depth + 1;
                    if (child.GetComponent<Canvas>() != null) 
                    {
                        ++depthIdx;
                        depth = depthIdx * defaultDepth;
                    }
                    TreeNode childNode = new TreeNode(child.name, child.gameObject, depth);
                    GenChildNodes(childNode, child);
                    node.AddChild(childNode);
                }
            }
        }
    }

    private void Reset()
    {
        m_selectObj = null;
        m_treeRootNode = null;
        m_ScrollPos = Vector2.zero;
    }

    public GUIStyle GUILabelType(TextAnchor anchor = TextAnchor.UpperCenter)
    {
        GUIStyle labelstyle = GUI.skin.GetStyle("Label");
        labelstyle.alignment = anchor;
        return labelstyle;
    }

    public void CreateSplit()
    {
        GUILayout.Label("-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
    }
}
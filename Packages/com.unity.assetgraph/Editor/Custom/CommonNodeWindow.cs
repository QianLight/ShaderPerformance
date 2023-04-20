using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CommonNodeWindow : EditorWindow
{
    //[MenuItem("Window/CommonNodeWindow")]
    public static void OpenWindow()
    {
        var window = GetWindow<CommonNodeWindow>();
        //window.m_graph = LoadDepGraph();
        window.m_graph = MakeGraph();
        //window.m_graph = LoadSkillFlow();
        window.m_graph.Init(true);
    }

    private CommonNodeGraph<CommonNode> m_graph;

    public void OnGUI()
    {
        DrawMenu();
        if (null == m_graph)
        {
            return;
        }
        m_graph.Draw(this);
    }

    private string m_filterKey;
    private string m_lastFilterKey;
    private void DrawMenu()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            if(GUILayout.Button("Load"))
            {
                var jsonFile = EditorUtility.OpenFilePanel("SelectSerializedJson", Application.dataPath, "json");
                if(!string.IsNullOrEmpty(jsonFile))
                {
                    if(File.Exists(jsonFile))
                    {
                        var json = File.ReadAllText(jsonFile);
                        m_graph = JsonUtility.FromJson<CommonNodeGraph<CommonNode>>(json);
                        m_graph.Init(true);
                    }
                }
            }
            if (GUILayout.Button("Reset"))
            {
                if(null != m_graph)
                {
                    m_graph.ResetOffsetAndScale();
                }
            }
            if (GUILayout.Button("Undo"))
            {
                if(null != m_graph)
                {
                    m_graph.CancelCommand();
                }
            }
            if (GUILayout.Button("Redo"))
            {
                if(null != m_graph)
                {
                    m_graph.RedoCommand();
                }
            }
            if (GUILayout.Button("SaveAs"))
            {
                if(null != m_graph)
                {
                    var filePath = EditorUtility.SaveFilePanel("Save", Path.Combine(Application.dataPath, ".."), "graph", ".json");
                    File.WriteAllText(filePath, JsonUtility.ToJson(m_graph));
                }
            }
            if(null == m_graph)
            {
                EditorGUILayout.LabelField($"NodeCount: {0}");
            }
            else
            {
                EditorGUILayout.LabelField($"NodeCount: {m_graph.m_nodes.Count}");
            }
            m_filterKey = EditorGUILayout.TextField("Filter:", m_filterKey);
            if (GUILayout.Button("Filter"))
            {
                if (m_lastFilterKey != m_filterKey)
                {
                    if (!string.IsNullOrEmpty(m_filterKey))
                    {
                        if(null != m_graph)
                        {
                            m_graph.Filter(m_filterKey);
                        }
                    }
                    m_lastFilterKey = m_filterKey;
                }
            }
        }
    }


    private static CommonNodeGraph<CommonNode> MakeGraph()
    {
        var graph = new CommonNodeGraph<CommonNode>();
        CommonNodeManager<CommonNode>.Instance.Reset();
        CommonConnectionManager<CommonNode>.Instance.Reset();
        var start = CommonNodeManager<CommonNode>.Instance.CreateNode("Start");
        var tmpList = new List<CommonNode>();
        for (var i = 0; i < 10; i++)
        {
            var next = CommonNodeManager<CommonNode>.Instance.CreateNode("A" + i);
            tmpList.Add(next);
            CommonConnectionManager<CommonNode>.Instance.Connect(start, next);
        }
        var anotherTmpList = new List<CommonNode>();
        foreach(var node in tmpList)
        {
            for(var i = 0; i < 5; i++)
            {
                var next = CommonNodeManager<CommonNode>.Instance.CreateNode(node.Name + "-B" + i);
                anotherTmpList.Add(next);
                CommonConnectionManager<CommonNode>.Instance.Connect(node, next);
            }
        }
        tmpList.Clear();
        anotherTmpList.Clear();
        for(var i = 0; i < 10; i++)
        {
            var prev = CommonNodeManager<CommonNode>.Instance.CreateNode("C" + i);
            CommonConnectionManager<CommonNode>.Instance.Connect(prev, start);
        }
        CommonNodeManager<CommonNode>.Instance.GetAllNodes(graph.m_nodes);
        CommonConnectionManager<CommonNode>.Instance.GetAllConnection(graph.m_connections);
        return graph;
    }

    private static Dictionary<string, List<string>> ParseManifest()
    {
        var dic = new Dictionary<string, List<string>>();
        string realPath = @"D:\Project\Client\Bundles\Android\Android";
        ulong offset = 0;
        AssetBundle manifestBundle = AssetBundle.LoadFromFile(realPath, 0, offset);
        Object asset = manifestBundle.LoadAsset("assetbundlemanifest");
        manifestBundle.Unload(false);
        var manifest = (AssetBundleManifest)asset;
        var bundles = manifest.GetAllAssetBundles();
        foreach(var bundle in bundles)
        {
            var list = dic[bundle] = new List<string>();
            var deps = manifest.GetDirectDependencies(bundle);
            foreach(var dep in deps)
            {
                list.Add(dep);
            }
        }
        return dic;
    }

    private static CommonNodeGraph<CommonNode> LoadDepGraph()
    {
        var json = File.ReadAllText("d:/bundleGraph.json");
        var graph = JsonUtility.FromJson<CommonNodeGraph<CommonNode>>(json);
        graph.SetColorFunc((name) =>
        {
            if(name.StartsWith("eff"))
            {
                return Color.blue;
            }
            else if(name.StartsWith("shared"))
            {
                return Color.green;
            }
            return Color.gray;
        });
        return graph;
    }

    [MenuItem("Test/MakeDepGraph")]
    private static void SerializeDepGraph()
    {
        AssetBundle.UnloadAllAssetBundles(true);
        string folder = @"D:\Project\Client\Bundles\Android";
        var dic = ParseManifest();
        var graph = new CommonNodeGraph<CommonNode>();
        Dictionary<string, CommonNode> nodeDic = new Dictionary<string, CommonNode>();
        foreach(var pair in dic)
        {
            var node = CommonNodeManager<CommonNode>.Instance.CreateNode(pair.Key);
            var ab = AssetBundle.LoadFromFile(Path.Combine(folder, pair.Key));
            var names = ab.GetAllAssetNames();
            var desc = "";
            foreach(var name in names)
            {
                desc += name + ",";
            }
            node.Desc = desc;
            nodeDic[pair.Key] = node;
            ab.Unload(true);
        }
        foreach(var pair in dic)
        {
            var deps = pair.Value;
            foreach(var dep in deps)
            {
                CommonConnectionManager<CommonNode>.Instance.Connect(nodeDic[pair.Key], nodeDic[dep]);
            }
        }
        CommonNodeManager<CommonNode>.Instance.GetAllNodes(graph.m_nodes);
        CommonConnectionManager<CommonNode>.Instance.GetAllConnection(graph.m_connections);
        Dictionary<string, HashSet<string>> connectionDic = new Dictionary<string, HashSet<string>>();
        foreach(var connection in graph.m_connections)
        {
            if(!connectionDic.TryGetValue(connection.m_from, out var val))
            {
                val = connectionDic[connection.m_from] = new HashSet<string>();
            }
            if(!val.Contains(connection.m_to))
            {
                val.Add(connection.m_to);
            }
            else
            {
                Debug.LogError($"connection repeated {connection}");
            }
        }
        var json = JsonUtility.ToJson(graph);
        File.WriteAllText("d:/bundleGraph.json", json);
        Debug.Log("Serialize dep graph finish");
    }

    private static CommonNodeGraph<CommonNode> LoadSkillFlow()
    {
        var json = File.ReadAllText("d:/flow.json");
        return JsonUtility.FromJson<CommonNodeGraph<CommonNode>>(json);
    }
}

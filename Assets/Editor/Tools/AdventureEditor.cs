using UnityEditor;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using CFUtilPoolLib;
using System;
using UnityEngine.CFUI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using CFEngine;

namespace AdventureEditor
{
    class AdventureEditor : EditorWindow
    {
        private GameObject PathPointPrefab;
        private GameObject PathLinePrefab;

        private AdventureNodes _NodesTable;
        private AdventureLines _LinesTable;

        int layer_mask;

        GameObject sceneRoot;
        GameObject nodeRoot;
        GameObject lineRoot;

        AdventureEditorConfig config;

        AdventureLines.RowData _GetLineData(uint configID, int linekey)
        {
            if (_LinesTable.Table == null)
                return null;
            for (int i = 0; i < _LinesTable.Table.Length; ++i)
            {
                AdventureLines.RowData rowData = _LinesTable.Table[i];
                if (rowData.ConfigID != configID)
                    continue;

                int rowkey = _MakeLineKey((short)rowData.NodeID[0], (short)rowData.NodeID[1]);
                if (rowkey != linekey)
                    continue;

                return rowData;
            }
            return null;
        }

        void _InitRoot()
        {
            if (sceneRoot == null)
            {
                sceneRoot = GameObject.Find("AdventureRoot");
                if (sceneRoot == null)
                {
                    sceneRoot = new GameObject("AdventureRoot");
                    nodeRoot = new GameObject("NodeRoot");
                    nodeRoot.transform.parent = sceneRoot.transform;
                    lineRoot = new GameObject("LineRoot");
                    lineRoot.transform.parent = sceneRoot.transform;
                }
                else
                {
                    nodeRoot = GameObject.Find("AdventureRoot/NodeRoot");
                    lineRoot = GameObject.Find("AdventureRoot/LineRoot");
                }
            };
        }

        [MenuItem(@"XEditor/AdventureEditor")]
        static void ShowWindow()
        {
            //EditorWindow.GetWindowWithRect<XReactLineEditor>(new Rect(800f, 100f, 800f, 800f), false, @"React Line Editor", true);
            var window = EditorWindow.GetWindow<AdventureEditor>(@"AdventureEditor", true);
            //window.position = new Rect(500, 400, 1000, 500);
            window.wantsMouseMove = true;
            window.Show();
            window.Repaint();
        }

        public void OnEnable()
        {
            //PathPointPrefab = AssetDatabase.LoadAssetAtPath(LevelGridTool.ResPath + "NaviPoint.prefab", typeof(GameObject)) as GameObject;
            //PathLinePrefab = AssetDatabase.LoadAssetAtPath(LevelGridTool.ResPath + "NaviLine.prefab", typeof(GameObject)) as GameObject;
            //NormalLineMaterial = AssetDatabase.LoadAssetAtPath(LevelGridTool.ResPath + "NaviLine.mat", typeof(Material)) as Material;
            //SelectLineMaterial = AssetDatabase.LoadAssetAtPath(LevelGridTool.ResPath + "NaviLineOneway.mat", typeof(Material)) as Material;

        }

        private void Awake()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;

            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;

            PathPointPrefab = AssetDatabase.LoadAssetAtPath("Assets/Tools/Common/Editor/Level/Res/AdventureNode.prefab", typeof(GameObject)) as GameObject;
            PathLinePrefab = AssetDatabase.LoadAssetAtPath("Assets/Tools/Common/Editor/Level/Res/NaviLine.prefab", typeof(GameObject)) as GameObject;
            config = AssetDatabase.LoadAssetAtPath("Assets/Tools/Common/Editor/Level/Res/AdventureEditorConfig.asset", typeof(AdventureEditorConfig)) as AdventureEditorConfig;
            layer_mask = (1 << LayerMask.NameToLayer("Dummy") | 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Terrain"));
        }
        private void OnDestroy()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            Selection.selectionChanged -= OnSelectionChanged;
            _NodesTable = null;
            if (sceneRoot != null)
                GameObject.DestroyImmediate(sceneRoot);
        }

        void OnGUI()
        {
            if (_NodesTable == null)
                _LoadTable();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("地图ID", new GUILayoutOption[] { GUILayout.Width(150f) });
            EditorGUILayout.EndHorizontal();

            uint.TryParse(EditorGUILayout.TextField(m_CurrentConfigID.ToString(), GUILayout.Width(150f)), out m_CurrentConfigID);

            if (m_ConfigList.Contains(m_CurrentConfigID))
            {
                if (GUILayout.Button("Load", new GUILayoutOption[] { GUILayout.Width(150f) }))
                {
                    _LoadMap();
                }
            }
            else
            {
                EditorGUILayout.LabelField("New ID");
                if (GUILayout.Button("Init", new GUILayoutOption[] { GUILayout.Width(150f) }))
                {
                    _InitMap();
                }
            }

            {
                if (GUILayout.Button("Save", new GUILayoutOption[] { GUILayout.Width(150f) }))
                {
                    _SaveToTable();
                }
                if (GUILayout.Button("Refresh All", new GUILayoutOption[] { GUILayout.Width(150f) }))
                {
                    _Refresh();
                }
                //if (GUILayout.Button("Save Line Decals", new GUILayoutOption[] { GUILayout.Width(150f) }))
                //{
                //    _SaveCurrentLineDecals();
                //}
                //if (GUILayout.Button("Save Selected Line Decal", new GUILayoutOption[] { GUILayout.Width(150f) }))
                //{
                //    _SaveSelectedLineDecals();
                //}
                GUILayout.Space(5);
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("选中点之后，按住C选中下一个点可连接", new GUILayoutOption[] { GUILayout.Width(350f) });
                EditorGUILayout.LabelField("选中线之后，按住C点击拐角处创建该连线的拐角标记：", new GUILayoutOption[] { GUILayout.Width(350f) });
                EditorGUILayout.LabelField("    拐角标记的ScaleX表明拐角半径", new GUILayoutOption[] { GUILayout.Width(350f) });
                EditorGUILayout.LabelField("    要注意拐角标记的Parent，别放错", new GUILayoutOption[] { GUILayout.Width(350f) });
                EditorGUILayout.BeginVertical();
            }
            //template = (ShipTemplate)EditorGUILayout.IntPopup(m_MapList, new GUILayoutOption[] { GUILayout.Width(150f) });
            //if (GUILayout.Button("load", new GUILayoutOption[] { GUILayout.Width(150f) }))

            GUILayout.Space(10);
            if (GUILayout.Button("Reload Table", new GUILayoutOption[] { GUILayout.Width(150f) }))
            {
                _LoadTable();
            }

        }

        void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            //UnityEditor.HandleUtility.AddDefaultControl(controlID);

            switch (e.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (Event.current.button == 0)
                    {
                        if (e.clickCount == 1)
                            OnMouseSingleClick(sceneView);
                        if (e.clickCount == 2)
                            OnMouseDoubleClick(sceneView);
                        //else if (e.clickCount == 2)
                        //    OnDoubleClick(sceneView);
                    }
                    break;
                case EventType.KeyDown:
                    if (Event.current.keyCode == KeyCode.C)
                    {
                        bTryConnectNode = true;
                    }
                    break;
                case EventType.KeyUp:
                    if (Event.current.keyCode == KeyCode.C)
                    {
                        bTryConnectNode = false;
                    }
                    break;
            }

        }
        private void OnMouseSingleClick(SceneView sceneView)
        {
            Ray r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            RaycastHit hitInfo;

            bool bHit = Physics.Raycast(r, out hitInfo, 10000.0f, layer_mask);
            if (bHit)
            {
                if (this.bTryConnectNode)
                {
                    if (Selection.activeGameObject != null)
                    {
                        AdventureLineBehaviour line = Selection.activeGameObject.GetComponent<AdventureLineBehaviour>();
                        if (line != null)
                        {
                            Vector3 clickPoint = hitInfo.point + new Vector3(0, config.ClickOffsetY, 0);
                            _AddCorner(clickPoint, line);
                            return;
                        }
                    }
                }
            }
        }
        private void OnMouseDoubleClick(SceneView sceneView)
        {
            Ray r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            RaycastHit hitInfo;

            bool bHit = Physics.Raycast(r, out hitInfo, 10000.0f, layer_mask);

            if (bHit)
            {
                Vector3 clickPoint = hitInfo.point + new Vector3(0, config.ClickOffsetY, 0);
                _TryAddNode(clickPoint);
            }
        }

        void _TryAddNode(Vector3 pos)
        {
            var bhv = _CreateNode(pos, Quaternion.Euler(0.0f, config.CreateRotateY, 0.0f));
            if (bhv != null)
            {
                //bhv.id = (short)(nodeRoot.transform.childCount - 1);
                bhv.id = ++newNodesID;
                if (Time.time - lastSelectedTime <= 0.15f)
                {
                    _CreateLine(lastSelectedNode, bhv);
                }
            }
        }

        void _AddCorner(Vector3 pos, AdventureLineBehaviour parent)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.parent = parent.transform;
            go.transform.position = pos;
            go.transform.localScale = Vector3.one;
        }

        AdventureNodeBehaviour lastSelectedNode;
        bool isSelectingNode;
        float lastSelectedTime;
        bool bTryConnectNode = false;

        GameObject lastSelectGo;
        GameObject currentSelectGo;
        private void OnSelectionChanged()
        {
            GameObject o = Selection.activeGameObject;
            lastSelectGo = currentSelectGo;
            currentSelectGo = o;
            if (o != null)
            {
                ////Transform t = o.transform;
                ////AdventureNodeBehaviour bhv = null;
                ////while (bhv == null && t != null)
                ////{
                ////    bhv = t.gameObject.GetComponent<AdventureNodeBehaviour>();
                ////    t = t.parent;
                ////}
                AdventureNodeBehaviour bhv = o.GetComponent<AdventureNodeBehaviour>();

                if (bhv != null)
                {
                    ///> 如果上一个选中和当前选中都是Node，并且按住了C，则连接
                    if (isSelectingNode && bTryConnectNode)
                    {
                        bool bConnected = false;
                        for (int i = 0; i < lineRoot.transform.childCount; ++i)
                        {
                            GameObject go = lineRoot.transform.GetChild(i).gameObject;
                            var lb = go.GetComponent<AdventureLineBehaviour>();
                            if (lb.startNode == lastSelectedNode && lb.endNode == bhv
                                || lb.startNode == bhv && lb.endNode == lastSelectedNode)
                            {
                                bConnected = true;
                                break;
                            }
                        }
                        if (!bConnected)
                            _CreateLine(lastSelectedNode, bhv);
                    }

                    lastSelectedNode = bhv;
                    lastSelectedTime = Time.time;
                    isSelectingNode = true;
                    return;
                }
            }

            if (isSelectingNode)
            {
                lastSelectedTime = Time.time;
                isSelectingNode = false;
            }
        }

        HashSet<uint> m_ConfigList = new HashSet<uint>();
        uint m_CurrentConfigID = 0;

        void _LoadTable()
        {
            _NodesTable = new AdventureNodes();
            _LinesTable = new AdventureLines();
            m_ConfigList.Clear();
            if (!XTableReader.ReadFile(s_BytesDir2 + "AdventureNodes", _NodesTable))
            {
                Debug.Log("Error occurred when loading AdventureNodes Table.");
                return;
            }
            if (!XTableReader.ReadFile(s_BytesDir2 + "AdventureLines", _LinesTable))
            {
                Debug.Log("Error occurred when loading AdventureLines Table.");
                return;
            }

            for (int i = 0; i < _NodesTable.Table.Length; ++i)
            {
                m_ConfigList.Add(_NodesTable.Table[i].ConfigID);
            }
        }

        static int _MakeLineKey(short node0, short node1)
        {
            if (node0 == node1)
                return 0;

            if (node0 < node1)
                return (node0 << 16) | (ushort)node1;

            return (node1 << 16) | (ushort)node0;
        }

        void _ClearNodes()
        {
            if (nodeRoot != null)
            {
                for (int i = nodeRoot.transform.childCount - 1; i >= 0; --i)
                {
                    GameObject.DestroyImmediate(nodeRoot.transform.GetChild(i).gameObject);
                }
            }
        }
        void _ClearLines()
        {
            if (lineRoot != null)
            {
                for (int i = lineRoot.transform.childCount - 1; i >= 0; --i)
                {
                    GameObject.DestroyImmediate(lineRoot.transform.GetChild(i).gameObject);
                }
            }
        }

        AdventureNodeBehaviour _CreateNode(Vector3 pos, Quaternion rot)
        {
            if (nodeRoot == null)
                return null;
            GameObject go = GameObject.Instantiate(PathPointPrefab) as GameObject;
            go.transform.parent = nodeRoot.transform;
            go.transform.position = pos;
            go.transform.rotation = rot;

            var bhv = go.AddComponent<AdventureNodeBehaviour>();
            bhv.destroyCallback = _OnNodeDelete;
            return bhv;
        }

        void _OnNodeDelete(AdventureNodeBehaviour node)
        {
            for (int i = lineRoot.transform.childCount - 1; i >= 0; --i)
            {
                GameObject go = lineRoot.transform.GetChild(i).gameObject;
                var bhv = go.GetComponent<AdventureLineBehaviour>();
                if (bhv.startNode == node || bhv.endNode == node)
                    GameObject.DestroyImmediate(go);
            }
        }

        AdventureLineBehaviour _CreateLine(AdventureNodeBehaviour startNode, AdventureNodeBehaviour endNode)
        {
            if (lineRoot == null)
                return null;
            if (startNode == null || endNode == null)
                return null;


            for (int i = 0; i < lineRoot.transform.childCount; ++i)
            {
                GameObject go = lineRoot.transform.GetChild(i).gameObject;
                var bhv = go.GetComponent<AdventureLineBehaviour>();
                if (bhv.startNode == startNode && bhv.endNode == endNode
                    || bhv.endNode == startNode && bhv.startNode == endNode)
                    return null;
            }


            {
                GameObject go = GameObject.Instantiate(PathLinePrefab) as GameObject;
                go.transform.parent = lineRoot.transform;
                var bhv = go.AddComponent<AdventureLineBehaviour>();
                bhv.Set(startNode, endNode);
                bhv.destroyCallback = _OnLineDelete;
                return bhv;
            }
        }
        void _OnLineDelete(AdventureLineBehaviour node)
        {

        }
        short newNodesID = -1;
        short oldMaxNodesID = -1;
        void _InitMap()
        {
            _InitRoot();
            _ClearLines();
            _ClearNodes();
            oldMaxNodesID = newNodesID = -1;
        }
        void _LoadMap()
        {
            _InitRoot();
            _ClearLines();
            _ClearNodes();
            oldMaxNodesID = newNodesID = -1;

            bool bFound = false;

            Dictionary<short, AdventureNodeBehaviour> id2comp = new Dictionary<short, AdventureNodeBehaviour>();
            for (int i = 0; i < _NodesTable.Table.Length; ++i)
            {
                var rowData = _NodesTable.Table[i];
                if (rowData.ConfigID != m_CurrentConfigID)
                {
                    if (bFound)
                        break;
                    else
                        continue;
                }
                bFound = true;

                var bhv = _CreateNode(
                    new Vector3(rowData.Pos[0], rowData.Pos[1], rowData.Pos[2]),
                    Quaternion.Euler(rowData.Rot[0], rowData.Rot[1], rowData.Rot[2]));

                bhv.Load(rowData);
                //bhv.focusCamera = _ToString(rowData.FocusCamera);
                oldMaxNodesID = newNodesID = Math.Max(newNodesID, bhv.id);

                id2comp.Add(bhv.id, bhv);
            }

            for (int i = 0; i < nodeRoot.transform.childCount; ++i)
            {
                GameObject go = nodeRoot.transform.GetChild(i).gameObject;
                var bhv = go.GetComponent<AdventureNodeBehaviour>();
                bhv.PostLoad(nodeRoot);
            }

            bFound = false;
            for (int i = 0; i < _NodesTable.Table.Length; ++i)
            {
                if (_NodesTable.Table[i].ConfigID != m_CurrentConfigID)
                {
                    if (bFound)
                        break;
                    else
                        continue;
                }
                bFound = true;

                if (_NodesTable.Table[i].NextNodes != null)
                {
                    short startNodeId = _NodesTable.Table[i].NodeID;
                    //GameObject startNode = nodeRoot.transform.GetChild(startNodeId).gameObject;
                    var startNode = id2comp[startNodeId];

                    for (int j = 0; j < _NodesTable.Table[i].NextNodes.Length; ++j)
                    {
                        short endNodeId = _NodesTable.Table[i].NextNodes[j];
                        if (startNodeId == endNodeId)
                            continue;
                        //GameObject endNode = nodeRoot.transform.GetChild(endNodeId).gameObject;
                        var endNode = id2comp[endNodeId];

                        var bhv = _CreateLine(startNode.GetComponent<AdventureNodeBehaviour>(), endNode.GetComponent<AdventureNodeBehaviour>());
                        if (bhv != null)
                        {
                            var lineData = _GetLineData(m_CurrentConfigID, _MakeLineKey(startNodeId, endNodeId));
                            if (lineData != null)
                            {
                                bhv.Load(lineData);
                            }
                        }
                    }
                }
            }
        }

        static string s_AdvDir = "Assets\\Table\\Adventure\\";
        static string s_BytesDir = "Assets\\BundleRes\\Table\\";
        static string s_BytesDir2 = "Table\\";
        void _SaveToTable()
        {
            _SaveNodes();
            _SaveLines();
            AssetDatabase.ImportAsset(s_BytesDir + "AdventureNodes.bytes");
            AssetDatabase.ImportAsset(s_BytesDir + "AdventureLines.bytes");
            _LoadTable();
        }

        void _SaveNodes()
        {
            string path = s_AdvDir + "AdventureNodes.txt";
            StreamReader sr = new StreamReader(path, System.Text.Encoding.Unicode);
            string colLine = sr.ReadLine();
            string commentLine = sr.ReadLine();
            sr.Close();

            StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Unicode);
            sw.WriteLine(colLine);
            sw.WriteLine(commentLine);

            string[] content = new string[AdventureNodeBehaviour.NODES_CONTENT_LENGTH];

            bool bFound = false;
            for (int i = 0; i < _NodesTable.Table.Length; ++i)
            {
                if (_NodesTable.Table[i].ConfigID != m_CurrentConfigID)
                {
                    AdventureNodeBehaviour.Save(_NodesTable.Table[i], content);
                    XTableWriter.WriteContent(sw, content);
                }
                else if (!bFound)
                {
                    bFound = true;
                    _SaveCurrentNodes(sw);
                }
            }

            if (!bFound)
            {
                bFound = true;
                _SaveCurrentNodes(sw);
            }
            XTableWriter.EndWriteFile(sw);
        }

        void _SaveCurrentNodes(StreamWriter sw)
        {
            string[] content = new string[AdventureNodeBehaviour.NODES_CONTENT_LENGTH];
            if (nodeRoot != null && lineRoot != null)
            {
                ///> 按顺序刷新ID
                List<AdventureNodeBehaviour> nodes = new List<AdventureNodeBehaviour>();
                for (int i = 0; i < nodeRoot.transform.childCount; ++i)
                {
                    GameObject go = nodeRoot.transform.GetChild(i).gameObject;
                    var bhv = go.GetComponent<AdventureNodeBehaviour>();
                    nodes.Add(bhv);
                }
                ///> 0, 1, 2, 5, 6 | 7, 8, 9   竖线前为旧节点，可有空挡。新增节点填补空挡
                nodes.Sort((a, b) => a.id.CompareTo(b.id));
                short id = 0;
                ///> 找到竖线
                int startIndex = 0;
                for (startIndex = 0; startIndex < nodes.Count; ++startIndex)
                {
                    if (nodes[startIndex].id <= oldMaxNodesID)
                        continue;
                    break;
                }
                int searchEmptyIndex = 0;
                short emptyID = 0;
                ///> 设置竖线后的节点id，有空挡填空挡，没有往后加
                for (int i = startIndex; i < nodes.Count; ++i)
                {
                    if (searchEmptyIndex < startIndex)
                    {
                        for (;searchEmptyIndex < startIndex; ++searchEmptyIndex)
                        {
                            if (nodes[searchEmptyIndex].id > emptyID)
                                break;
                            ++emptyID;
                        }
                    }
                    nodes[i].id = emptyID++;
                }

                ///> 统计nextNodes,prevNodes
                Dictionary<short, HashSet<short>> nextNodes = new Dictionary<short, HashSet<short>>();
                for (int i = 0; i < lineRoot.transform.childCount; ++i)
                {
                    GameObject go = lineRoot.transform.GetChild(i).gameObject;
                    var bhv = go.GetComponent<AdventureLineBehaviour>();
                    bhv.RefreshName();

                    if (!nextNodes.ContainsKey(bhv.startNode.id))
                        nextNodes[bhv.startNode.id] = new HashSet<short>();
                    nextNodes[bhv.startNode.id].Add(bhv.endNode.id);

                    if (!nextNodes.ContainsKey(bhv.endNode.id))
                        nextNodes[bhv.endNode.id] = new HashSet<short>();
                    nextNodes[bhv.endNode.id].Add(bhv.startNode.id);
                }

                ///>写入
                HashSet<short> watchNodes = new HashSet<short>();
                HashSet<short> newWatchNodes = new HashSet<short>();
                HashSet<short> newWatchList = new HashSet<short>();

                for (int i = 0; i < nodes.Count; ++i)
                {
                    AdventureNodeBehaviour node = nodes[i];
                    ///> 统计watchNodes
                    watchNodes.Clear();
                    newWatchNodes.Clear();
                    newWatchList.Clear();

                    newWatchNodes.Add(node.id);
                    int watchDistance = config.WatchDistance;
                    while (watchDistance > 0)
                    {
                        foreach (var n in newWatchNodes)
                        {
                            if (nextNodes.ContainsKey(n))
                            {
                                foreach (var ne in nextNodes[n])
                                    newWatchList.Add(ne);
                            }
                        }

                        newWatchNodes.Clear();
                        foreach (var n in newWatchList)
                        {
                            if (watchNodes.Contains(n))
                                continue;
                            watchNodes.Add(n);
                            newWatchNodes.Add(n);
                        }
                        newWatchList.Clear();
                        --watchDistance;
                    }

                    watchNodes.Remove(node.id);

                    node.Save(content, m_CurrentConfigID, node.id, nextNodes.ContainsKey(node.id) ? nextNodes[node.id] : null, watchNodes);
                    XTableWriter.WriteContent(sw, content);
                }
            }
        }

        void _SaveLines()
        {
            string path = s_AdvDir + "AdventureLines.txt";
            StreamReader sr = new StreamReader(path, System.Text.Encoding.Unicode);
            string colLine = sr.ReadLine();
            string commentLine = sr.ReadLine();
            sr.Close();

            StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Unicode);
            sw.WriteLine(colLine);
            sw.WriteLine(commentLine);

            string[] content = new string[AdventureLineBehaviour.LINES_CONTENT_LENGTH];

            bool bFound = false;
            if (_LinesTable.Table != null)
            {
                for (int i = 0; i < _LinesTable.Table.Length; ++i)
                {
                    if (_LinesTable.Table[i].ConfigID != m_CurrentConfigID)
                    {
                        AdventureLineBehaviour.Save(_LinesTable.Table[i], content);
                        XTableWriter.WriteContent(sw, content);
                    }
                    else if (!bFound)
                    {
                        bFound = true;
                        _SaveCurrentLines(sw);
                    }
                }
            }
            if (!bFound)
            {
                bFound = true;
                _SaveCurrentLines(sw);
            }
            XTableWriter.EndWriteFile(sw);
        }
        void _SaveCurrentLines(StreamWriter sw)
        {
            string[] content = new string[AdventureLineBehaviour.LINES_CONTENT_LENGTH];
            for (int i = 0; i < lineRoot.transform.childCount; ++i)
            {
                AdventureLineBehaviour bhv = lineRoot.transform.GetChild(i).GetComponent<AdventureLineBehaviour>();
                if (bhv == null)
                    continue;
                if (bhv.hide || bhv.transform.childCount > 0)
                {
                    bhv.Save(content, m_CurrentConfigID);
                    XTableWriter.WriteContent(sw, content);
                }
            }
        }

        MeshDecal m_DecalTool = new MeshDecal();
        void _SaveCurrentLineDecals()
        {
            var scene = SceneManager.GetActiveScene();

            AssetDatabase.StartAssetEditing();
            
            SceneAssets.CreateFolder(AssetsConfig.instance.ResourcePath + "/SceneConfig", scene.name);

            string dir = string.Format("{0}/SceneConfig/{1}", AssetsConfig.instance.ResourcePath, scene.name);
            string folder = SceneAssets.CreateFolder(dir, string.Format("advlines{0}", m_CurrentConfigID));

            for (int i = 0; i < lineRoot.transform.childCount; ++i)
            {
                AdventureLineBehaviour bhv = lineRoot.transform.GetChild(i).GetComponent<AdventureLineBehaviour>();
                if (bhv == null)
                    continue;
                EditorUtility.DisplayProgressBar(
                    "Creating Line Decals", 
                    string.Format("Creating Line {0}-{1}", bhv.startNode.id, bhv.endNode.id), 
                    (i + 1.0f) / lineRoot.transform.childCount);

                Mesh mesh = m_DecalTool.Split(bhv.gameObject.transform, 2f, 2.0f, bhv.Length);
                EditorCommon.CreateAsset<Mesh>(
                    string.Format("{0}/advline{1}-{2}.asset", folder, bhv.startNode.id, bhv.endNode.id), 
                    ".asset", 
                    mesh);
            }
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        void _SaveSelectedLineDecals()
        {
            GameObject o = Selection.activeGameObject;
            if (o == null)
                return;

            AdventureLineBehaviour bhv = o.GetComponent<AdventureLineBehaviour>();
            if (bhv == null)
                return;

            var scene = SceneManager.GetActiveScene();

            AssetDatabase.StartAssetEditing();
            string dir = string.Format("{0}/SceneConfig/{1}", AssetsConfig.instance.ResourcePath, scene.name);
            string folder = SceneAssets.CreateFolder(dir, string.Format("advlines{0}", m_CurrentConfigID));

            {

                Mesh mesh = m_DecalTool.Split(bhv.gameObject.transform, 2f, 2.0f, bhv.Length);
                EditorCommon.CreateAsset<Mesh>(
                    string.Format("{0}/advline{1}-{2}.asset", folder, bhv.startNode.id, bhv.endNode.id),
                    ".asset",
                    mesh);
            }
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        void _Refresh()
        {
            ///> 按顺序刷新ID
            for (int i = 0; i < nodeRoot.transform.childCount; ++i)
            {
                GameObject go = nodeRoot.transform.GetChild(i).gameObject;
                var bhv = go.GetComponent<AdventureNodeBehaviour>();
                bhv.id = (short)i;
            }
            for (int i = 0; i < lineRoot.transform.childCount; ++i)
            {
                GameObject go = lineRoot.transform.GetChild(i).gameObject;
                var bhv = go.GetComponent<AdventureLineBehaviour>();
                bhv.RefreshName();
                bhv.RefreshPosition();
            }
        }
    }
}

using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using CFUtilPoolLib;
using CFEngine;
using XLevel;

namespace XEditor.Level
{
    class LevelNavigation
    {
        private LevelDisplayWalkableMode m_Mode; 

        private List<NaviPoint> AllNaviPoint = new List<NaviPoint>();
        private List<NaviLine> AllNaviLine = new List<NaviLine>();
        private List<LinkLine> AllLinkLine = new List<LinkLine>();

        private NaviPoint currentSelect = null;

        private LevelNaviMapGenerator NaviGenerator = null;

        private List<GameObject> TestNaviMarkPoint = new List<GameObject>();
        private List<GameObject> TestNaviLine = new List<GameObject>();

        private bool bDirty = true;

        public GameObject NaviPointPrafab;
        public GameObject NaviLinePrefab;
        public GameObject LinkLinePrefab;
        public GameObject TestNaviMarkPrefab;
        public GameObject TestNaviLinePrefab;
        
        static public Material NaviPointMaterial;
        static public Material NaviPointSelectMaterial;
        static public Material NaviLineMaterial;
        static public Material NaviLineOnewayMaterial;

        private GameObject NaviPointRoot;

        public LevelNavigation(LevelDisplayWalkableMode _mode)
        {
            NaviPointPrafab = AssetDatabase.LoadAssetAtPath(LevelGridTool.ResPath + "NaviPoint.prefab", typeof(GameObject)) as GameObject;
            NaviLinePrefab = AssetDatabase.LoadAssetAtPath(LevelGridTool.ResPath + "NaviLine.prefab", typeof(GameObject)) as GameObject;
            LinkLinePrefab = AssetDatabase.LoadAssetAtPath(LevelGridTool.ResPath + "LinkLine.prefab", typeof(GameObject)) as GameObject;
            TestNaviMarkPrefab = AssetDatabase.LoadAssetAtPath(LevelGridTool.ResPath + "TestNaviMark.prefab", typeof(GameObject)) as GameObject;
            TestNaviLinePrefab = AssetDatabase.LoadAssetAtPath(LevelGridTool.ResPath + "TestNaviLine.prefab", typeof(GameObject)) as GameObject;

            NaviPointMaterial = AssetDatabase.LoadAssetAtPath(LevelGridTool.ResPath + "NaviPointMat.mat", typeof(Material)) as Material;
            NaviPointSelectMaterial = AssetDatabase.LoadAssetAtPath(LevelGridTool.ResPath + "NaviPointSelectMat.mat", typeof(Material)) as Material;
            NaviLineMaterial = AssetDatabase.LoadAssetAtPath(LevelGridTool.ResPath + "NaviLine.mat", typeof(Material)) as Material;
            NaviLineOnewayMaterial = AssetDatabase.LoadAssetAtPath(LevelGridTool.ResPath + "NaviLineOneway.mat", typeof(Material)) as Material;

            LevelGridTool.ClearUnusedRoot("NaviRoot");
            NaviPointRoot = new GameObject();
            NaviPointRoot.name = "NaviRoot";
            NaviPointRoot.transform.position = Vector3.zero;

            m_Mode = _mode;
        }

        public bool IsNavigationPathMatrixReady()
        {
            return !bDirty;
        }

        public void Clear()
        {
            UnSelected();

            ClearNaviPoint();

            ClearNaviLine();

            ClearLinkLine();

            ClearTestNavi();

            ClearTestNaviLine();

            //if (NaviPointRoot != null)
            //{
            //    GameObject.DestroyImmediate(NaviPointRoot);
            //    NaviPointRoot = null;
            //}
        }

        public void OnUpdate()
        {
            if(currentSelect != null)
            {
                if (currentSelect.go.transform.position != currentSelect.Pos)
                {
                    Vector3 queryPos = currentSelect.go.transform.position;

                    QuadTreeElement selectGrid = m_Mode.m_tool.m_Data.GetPosHeight(queryPos);
                    if (selectGrid != null)
                    {
                        currentSelect.go.transform.position = selectGrid.pos;
                        currentSelect.Pos = selectGrid.pos;

                        if (currentSelect.linkedIndex > -1)
                        {
                            LinkLine ll = FindLinkLine((int)currentSelect.Index, currentSelect.linkedIndex);
                            if (ll != null)
                                ll.SetDirection(currentSelect.Pos, AllNaviPoint[currentSelect.linkedIndex].Pos);
                        }
                    }
                    else
                    {
                        currentSelect.go.transform.position = currentSelect.Pos;
                    }

                    //LevelBlock selectBlock = m_Mode.m_tool.m_Data.GetBlockByCoord(queryPos);
                    //if (selectBlock != null)
                    //{
                    //    QuadTreeElement selectGrid = selectBlock.GetGridByCoord(queryPos);
                    //    if (selectGrid != null)
                    //    {
                    //        currentSelect.go.transform.position = selectGrid.pos;
                    //        currentSelect.Pos = selectGrid.pos;

                    //        if(currentSelect.linkedIndex > -1)
                    //        {
                    //            LinkLine ll = FindLinkLine((int)currentSelect.Index, currentSelect.linkedIndex);
                    //            if (ll != null)
                    //                ll.SetDirection(currentSelect.Pos, AllNaviPoint[currentSelect.linkedIndex].Pos);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        currentSelect.go.transform.position = currentSelect.Pos;
                    //    }
                    //}
                    //else
                    //{
                    //    currentSelect.go.transform.position = currentSelect.Pos;
                    //}

                    bDirty = true;
                    LevelTool.wantsRepaint = true;              
                }
            }
        }

        public void AddNaviPoint(Vector3 newPoint, int linkIndex, bool repaint = true)
        {
            NaviPoint np = new NaviPoint();

            QuadTreeElement q = m_Mode.m_tool.m_Data.QueryGrid(newPoint);
            if (q == null) return;

            int index = -1;
            for(int i = 0; i < AllNaviPoint.Count; ++i)
            {
                if(!AllNaviPoint[i].IsValid())
                {
                    index = i;
                    break;
                }
            }

            if(index == -1)
            {
                np.Index = (uint)AllNaviPoint.Count;
                np.Pos = newPoint;
                np.linkedIndex = linkIndex;
                np.go = GameObject.Instantiate(NaviPointPrafab) as GameObject;

                np.go.name = "np_" + np.Index.ToString();
                np.go.transform.parent = NaviPointRoot.transform;
                np.go.transform.position = newPoint;
                np.go.GetComponent<NaviPointBehaviour>().destroyCallback = OnGameObjectDestroyed;

                np.selectMat = NaviPointSelectMaterial;
                np.mat = NaviPointMaterial;

                AllNaviPoint.Add(np);
            }
            else if(index >= 0)
            {
                AllNaviPoint[index].Pos = newPoint;
                AllNaviPoint[index].linkedIndex = linkIndex;

                AllNaviPoint[index].go = GameObject.Instantiate(NaviPointPrafab) as GameObject;

                AllNaviPoint[index].go.name = "np_" + index.ToString();
                AllNaviPoint[index].go.transform.parent = NaviPointRoot.transform;
                AllNaviPoint[index].go.transform.position = newPoint;

                AllNaviPoint[index].go.GetComponent<NaviPointBehaviour>().destroyCallback = OnGameObjectDestroyed;
            }

            if(repaint)
            {
                bDirty = true;
                LevelTool.wantsRepaint = true;
            }
        }


        public void AddTestPoint(Vector3 Point)
        {
            GameObject go = GameObject.Instantiate(TestNaviMarkPrefab) as GameObject;
            go.transform.parent = NaviPointRoot.transform;
            go.transform.position = Point + new Vector3(0, 1, 0);

            TestNaviMarkPoint.Add(go);

            if(TestNaviMarkPoint.Count > 2)
            {
                GameObject.DestroyImmediate(TestNaviMarkPoint[TestNaviMarkPoint.Count - 3]);
                TestNaviMarkPoint[TestNaviMarkPoint.Count - 3] = null;
            }

            if(TestNaviMarkPoint.Count >= 2)
            {
                if(NaviGenerator != null)
                {
                    Vector3 start = TestNaviMarkPoint[TestNaviMarkPoint.Count - 2].transform.position;
                    Vector3 end = TestNaviMarkPoint[TestNaviMarkPoint.Count - 1].transform.position;

                    List<int> path = NaviGenerator.GetNearestPath(start, end);

                    // if(!SceneAssets.scene_subname_directory.Contains("kuhan"))
                    // {
                    //     if (path.Count >= 2)
                    //     {
                    //         Vector3 v1 = Vector3.Normalize(AllNaviPoint[path[0]].Pos - start);
                    //         Vector3 v2 = Vector3.Normalize(AllNaviPoint[path[1]].Pos - AllNaviPoint[path[0]].Pos);

                    //         float f = Vector3.Dot(v1, v2);
                    //         if (f < 0)
                    //         {
                    //             path.RemoveAt(0);
                    //         }
                    //     }
                    // }
                    

                    ClearTestNaviLine();

                    if (path.Count > 0)
                        AddTestNaviLine(start, AllNaviPoint[path[0]].Pos);

                    for (int i = 0; i < path.Count - 1; ++i)
                    {
                        AddTestNaviLine(AllNaviPoint[path[i]].Pos, AllNaviPoint[path[i + 1]].Pos);
                    }

                    if (path.Count > 0)
                        AddTestNaviLine(AllNaviPoint[path[path.Count - 1]].Pos, end);
                }
            }
        }

        public void AddTestNaviLine(Vector3 s1, Vector3 s2)
        {
            GameObject line = GameObject.Instantiate(TestNaviLinePrefab) as GameObject;
            line.transform.parent = NaviPointRoot.transform;

            LineRenderer lr = line.GetComponent<LineRenderer>();

            lr.positionCount = 2;
            lr.SetPosition(0, s1 + new Vector3(0, 1, 0));
            lr.SetPosition(1, s2 + new Vector3(0, 1, 0));

            TestNaviLine.Add(line);
        }

        public void OnSelect(GameObject go)
        {
            string name = go.transform.parent.gameObject.name;
            int index = int.Parse(name.Substring(3));

            if(index >=0 && index < AllNaviPoint.Count)
            {
                if (currentSelect != null)
                    currentSelect.SetSelected(false);

                currentSelect = AllNaviPoint[index];
                currentSelect.SetSelected(true);

                Selection.activeGameObject = go.transform.parent.gameObject;
            }
        }

        public void UnSelected()
        {
            if (currentSelect == null) return;

            currentSelect.SetSelected(false);

            currentSelect = null;
            Selection.activeGameObject = null;

        }

        public void AddLink(GameObject go)
        {
            if (currentSelect == null) return;

            string name = go.transform.parent.gameObject.name;
            int index = int.Parse(name.Substring(3));

            if (index >= 0 && index < AllNaviPoint.Count)
            {
                NaviPoint linkedPoint = AllNaviPoint[index];

                currentSelect.linkedIndex = index;
                linkedPoint.linkedIndex = (int)currentSelect.Index;

                LinkLine ll = new LinkLine();

                ll.startIndex = (int)currentSelect.Index;
                ll.endIndex = index;
                ll.go = GameObject.Instantiate(LinkLinePrefab) as GameObject;
                ll.go.name = "link_" + currentSelect.Index + "_" + index;
                ll.go.transform.parent = NaviPointRoot.transform;
                ll.SetDirection(AllNaviPoint[index].Pos, currentSelect.Pos);

                AllLinkLine.Add(ll);

                bDirty = true;
                LevelTool.wantsRepaint = true;
            }
        }

        //public void RemoveLastPoint()
        //{
        //    if(AllNaviPoint.Count > 0)
        //    {
        //        NaviPoint np = AllNaviPoint[AllNaviPoint.Count - 1];
        //        np.Destroy();               
        //    }
        //}

        public void DeleteCurrentSelected()
        {
            if(currentSelect != null)
            {
                if(currentSelect.linkedIndex > -1)
                {
                    NaviPoint linkedPoint = AllNaviPoint[currentSelect.linkedIndex];

                    DeleteLinkLine((int)currentSelect.Index, currentSelect.linkedIndex);
                    linkedPoint.linkedIndex = -1;
                }

                currentSelect.Destroy();
                currentSelect = null;

                bDirty = true;
                LevelTool.wantsRepaint = true;
            }
        }

        private void DeleteLinkLine(int startIndex, int endIndex)
        {
            for(int i = 0; i < AllLinkLine.Count; ++i)
            {
                if((AllLinkLine[i].startIndex == startIndex && AllLinkLine[i].endIndex == endIndex) ||
                    (AllLinkLine[i].endIndex == startIndex && AllLinkLine[i].startIndex == endIndex))
                {
                    AllLinkLine[i].Destroy();
                    AllLinkLine.RemoveAt(i);
                    break;
                }
            }
        }


        private LinkLine FindLinkLine(int startIndex, int endIndex)
        {
            for (int i = 0; i < AllLinkLine.Count; ++i)
            {
                if ((AllLinkLine[i].startIndex == startIndex && AllLinkLine[i].endIndex == endIndex) ||
                    (AllLinkLine[i].endIndex == startIndex && AllLinkLine[i].startIndex == endIndex))
                {
                    return AllLinkLine[i];
                }
            }

            return null;
        }

        private void OnGameObjectDestroyed(GameObject go)
        {
            int index = int.Parse(go.name.Substring(3));

            if(index >= 0 && index < AllNaviPoint.Count)
            {
                NaviPoint tobeDel = AllNaviPoint[index];

                if (currentSelect != null && index == currentSelect.Index)
                {
                    currentSelect.go = null;
                    currentSelect.Pos = Vector3.zero; 
                    currentSelect = null;
                }

                if (AllNaviPoint[index].linkedIndex >= AllNaviPoint.Count)
                {
                    Debug.Log("OnGameObjectDestroyed:" + AllNaviPoint[index].linkedIndex + " " + index);
                    return;
                }
                
                if (AllNaviPoint[index].linkedIndex > -1)
                {
                    DeleteLinkLine(index, AllNaviPoint[index].linkedIndex);
                    AllNaviPoint[AllNaviPoint[index].linkedIndex].linkedIndex = -1;
                }

                AllNaviPoint[index].Pos = Vector3.zero;
                AllNaviPoint[index].linkedIndex = -1;
                AllNaviPoint[index].go = null;

                bDirty = true;
                LevelTool.wantsRepaint = true;
            }
        }

        public void SaveToFile(ref SceneContext context)
        {
           
            string clientDataPath = string.Format("{0}/{1}_NaviMap.navi", context.terrainDir, context.name);

            try
            {
                using (FileStream fs = new FileStream(clientDataPath, FileMode.Create))
                {
                    BinaryWriter bw = new BinaryWriter(fs);
                    bw.Write(AllNaviPoint.Count);
                    
                    for(int i = 0; i < AllNaviPoint.Count; ++i)
                    {
                        bw.Write(AllNaviPoint[i].Pos.x);
                        bw.Write(AllNaviPoint[i].Pos.y);
                        bw.Write(AllNaviPoint[i].Pos.z);
                        bw.Write(AllNaviPoint[i].linkedIndex);
                    }

                    for(int i = 0; i < AllNaviPoint.Count; ++i)
                        for(int j = 0; j < AllNaviPoint.Count; ++j)
                        {
                            bw.Write(NaviGenerator.ConnectedGraph[i, j]);
                        }

                    for (int i = 0; i < AllNaviPoint.Count; ++i)
                        for (int j = 0; j < AllNaviPoint.Count; ++j)
                        {
                            bw.Write(NaviGenerator.PathMatrix[i, j]);
                        }

                    EditorUtility.DisplayDialog("导航图保存成功", "导航图保存成功", "OK");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
        
        public void SaveToServerXML(ref SceneContext context)
        {
            string serverWayPointDir = "Assets/BundleRes/Table/WayPoint/";

            string filename = context.name;
            string xmlpath = serverWayPointDir + filename + ".xml";

            XmlDocument xmlDoc = new XmlDocument();
            XmlNode xmlDec = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes");
            xmlDoc.AppendChild(xmlDec);
            XmlNode rootNode = xmlDoc.CreateElement("WayPoint");
            //ArrayList nodeList = new ArrayList();

            for (int j = 0; j < AllNaviPoint.Count; j++)
            {
                XmlNode subNode = xmlDoc.CreateElement("point");

                XmlAttribute attr = xmlDoc.CreateAttribute("index");
                attr.Value = j.ToString();
                ((XmlElement)subNode).SetAttributeNode(attr);

                attr = xmlDoc.CreateAttribute("pos");
                attr.Value = string.Format("{0}:{1}:{2}", AllNaviPoint[j].Pos.x, AllNaviPoint[j].Pos.y, AllNaviPoint[j].Pos.z);
                ((XmlElement)subNode).SetAttributeNode(attr);

                rootNode.AppendChild(subNode);
            }

            xmlDoc.AppendChild(rootNode);
            xmlDoc.Save(xmlpath);

            EditorUtility.DisplayDialog("导航图保存成功", "导航图保存成功", "OK");
        }

        public void LoadFromFile(ref SceneContext context)
        {
            string clientDataPath = string.Format("{0}/{1}_NaviMap.navi", context.terrainDir, context.name);

            using (FileStream fs = new FileStream(clientDataPath, FileMode.Open))
            {
                BinaryReader br = new BinaryReader(fs);

                ClearNaviPoint();

                int count = br.ReadInt32();

                for(int i = 0; i < count; ++i)
                {
                    float x = br.ReadSingle();
                    float y = br.ReadSingle();
                    float z = br.ReadSingle();
                    int linkIndex = br.ReadInt32();

                    AddNaviPoint(new Vector3(x, y, z), linkIndex, false);
                }

                if (NaviGenerator == null)
                    NaviGenerator = new LevelNaviMapGenerator(m_Mode.m_tool.m_Data);
                
                NaviGenerator.SetNaviPoints(AllNaviPoint);

                //for (int i = 0; i < AllNaviPoint.Count; ++i)
                //    for (int j = 0; j < AllNaviPoint.Count; ++j)
                //    {
                //        NaviGenerator.ConnectedGraph[i, j] = br.ReadSingle();
                //    }

                //for (int i = 0; i < AllNaviPoint.Count; ++i)
                //    for (int j = 0; j < AllNaviPoint.Count; ++j)
                //    {
                //        NaviGenerator.PathMatrix[i, j] = br.ReadInt32();
                //    }

                //DrawConnectionGraph();
                //DrawLinkGraph();
                bDirty = false;
            }
        }

        private void ReorganizeNaviPoint()
        {
            List<NaviPoint> newList = new List<NaviPoint>();
            Dictionary<int, int> indexMap = new Dictionary<int, int>();

            int index = 0;
            for(int i = 0; i < AllNaviPoint.Count; ++i)
            {
                if(AllNaviPoint[i].IsValid())
                {
                    newList.Add(AllNaviPoint[i]);

                    newList[index].Index = (uint)index;
                    newList[index].go.name = "np_" + index.ToString();
                    indexMap.Add(i, index);

                    index++;
                }
            }

            for(int i = 0; i < newList.Count; ++i)
            {
                if(newList[i].linkedIndex > -1)
                {
                    if(!indexMap.ContainsKey(newList[i].linkedIndex))
                    {
                        Debug.Log(newList[i].linkedIndex);
                    }
                    else
                    {
                        int newlink = indexMap[newList[i].linkedIndex];
                        newList[i].linkedIndex = newlink;
                    }
                    
                }
            }

            AllNaviPoint = newList;
        }

        public void GenerateNaviPointMap()
        {
            ReorganizeNaviPoint();

            if (NaviGenerator == null)
                NaviGenerator = new LevelNaviMapGenerator(m_Mode.m_tool.m_Data);

            NaviGenerator.Reset();
            NaviGenerator.SetNaviPoints(AllNaviPoint);
            LevelDisplayWalkableMode.m_debugGrid.Clear();
            NaviGenerator.GenerateNaviMap();

            ClearNaviLine();
            DrawConnectionGraph();
            NaviGenerator.GeneratePathMatrix();

            bDirty = false;
        }


        private void DrawConnectionGraph()
        {
            ClearNaviLine();
            for (int i = 0; i < AllNaviPoint.Count; ++i)
                for (int j = 0; j < AllNaviPoint.Count; ++j)
                {
                    if (i != j && (NaviGenerator.ConnectedGraph[i, j] < float.MaxValue || NaviGenerator.ConnectedGraph[j, i] < float.MaxValue))
                    {
                        NaviLine nl = new NaviLine();

                        nl.startIndex = i;
                        nl.endIndex = j;
                        nl.go = GameObject.Instantiate(NaviLinePrefab) as GameObject;
                        nl.go.name = i + "_" + j;
                        nl.go.transform.parent = NaviPointRoot.transform;

                        bool bTwoWay = (NaviGenerator.ConnectedGraph[i, j] < float.MaxValue && NaviGenerator.ConnectedGraph[j, i] < float.MaxValue);
                        nl.SetDirection(AllNaviPoint[i].Pos, AllNaviPoint[j].Pos, bTwoWay, NaviLineOnewayMaterial, NaviLineMaterial);

                        AllNaviLine.Add(nl);
                    }
                }
        }

        private void DrawLinkGraph()
        {
            ClearLinkLine();
            for (int i = 0; i < AllNaviPoint.Count; ++i)
            {
                if(AllNaviPoint[i].linkedIndex > -1 && i < AllNaviPoint[i].linkedIndex)
                {
                    LinkLine ll = new LinkLine();

                    ll.startIndex = i;
                    ll.endIndex = AllNaviPoint[i].linkedIndex;
                    ll.go = GameObject.Instantiate(LinkLinePrefab) as GameObject;
                    ll.go.name = "link_" + i + "_" + AllNaviPoint[i].linkedIndex;
                    ll.go.transform.parent = NaviPointRoot.transform;
                    ll.SetDirection(AllNaviPoint[i].Pos, AllNaviPoint[AllNaviPoint[i].linkedIndex].Pos);

                    AllLinkLine.Add(ll);
                }
            }
        }

        public void ClearNaviPoint()
        {
            for (int i = 0; i < AllNaviPoint.Count; ++i)
            {
                AllNaviPoint[i].Destroy();
            }

            AllNaviPoint.Clear();

            if(NaviGenerator != null)
                NaviGenerator.Reset();

        }

        public void ClearNaviLine()
        {
            for (int i = 0; i < AllNaviLine.Count; ++i)
            {
                AllNaviLine[i].Destroy();
            }

            AllNaviLine.Clear();
        }

        public void ClearTestNavi()
        {
            for (int i = 0; i < TestNaviMarkPoint.Count; ++i)
            {
                if (TestNaviMarkPoint[i] != null)
                    GameObject.DestroyImmediate(TestNaviMarkPoint[i]);
            }

            TestNaviMarkPoint.Clear();           
        }

        public void ClearTestNaviLine()
        {
            for (int i = 0; i < TestNaviLine.Count; ++i)
            {
                if (TestNaviLine[i] != null)
                    GameObject.DestroyImmediate(TestNaviLine[i]);
            }

            TestNaviLine.Clear();
        }

        public void ClearLinkLine()
        {
            for (int i = 0; i < AllLinkLine.Count; ++i)
            {
                AllLinkLine[i].Destroy();
            }

            AllLinkLine.Clear();
        }
    }
}

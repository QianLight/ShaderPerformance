using System.Collections.Generic;
using System.IO;
using CFEngine;
using CFEngine.Editor;
using CFUtilPoolLib;
using UnityEditor;
using UnityEngine;
using XLevel;

namespace XEditor.Level
{
    class PatrolPoint
    {
        public Vector3 pos;
        public float Time;
        public float Time2;
        public bool HoldHere;

        public GameObject go;
    }

    class PatrolPath
    {
        //public int ID;
        public int PatrolID;
        public List<PatrolPoint> path;

        public GameObject PathRoot;

        public int CacheMax = 0;
        public bool LinkLineDirty = true;

        public List<LinkLine> LinkLines = new List<LinkLine> ();

        public void Clear ()
        {
            ClearPatrolPoint ();

            ClearLinkLine ();

            if (PathRoot != null)
            {
                GameObject.DestroyImmediate (PathRoot);
                PathRoot = null;
            }
        }

        public void ClearPatrolPoint ()
        {
            for (int i = 0; i < path.Count; ++i)
            {
                if (path[i].go != null)
                    GameObject.DestroyImmediate (path[i].go);
            }

            path.Clear ();
        }

        public void ClearLinkLine ()
        {
            for (int i = 0; i < LinkLines.Count; ++i)
            {
                LinkLines[i].Destroy ();
            }

            LinkLines.Clear ();
        }
    }

    class LevelPatrol
    {
        private LevelDisplayPatrolMode m_Mode;

        private PatrolTable _data_info;
        // scene name -> patrol data
        private Dictionary<string, List<PatrolPath>> AllPatrolRecord = new Dictionary<string, List<PatrolPath>> ();

        //private List<LinkLine> AllLinkLine = new List<LinkLine>();
        private List<PatrolPath> CurrentScenePathList = null;
        private PatrolPath CurrentOperatePath = null;

        private GameObject PatrolNode;
        public GameObject PatrolPointPrafab;
        public GameObject PatrolLinePrefab;
        public GameObject LinkLinePrefab;

        static public Material PatrolLineMaterial;
        static public Material PatrolSelectLineMaterial;

        public LevelPatrol (LevelDisplayPatrolMode _mode)
        {
            PatrolPointPrafab = AssetDatabase.LoadAssetAtPath (LevelGridTool.ResPath + "NaviPoint.prefab", typeof (GameObject)) as GameObject;
            PatrolLinePrefab = AssetDatabase.LoadAssetAtPath (LevelGridTool.ResPath + "NaviLine.prefab", typeof (GameObject)) as GameObject;
            LinkLinePrefab = AssetDatabase.LoadAssetAtPath (LevelGridTool.ResPath + "LinkLine.prefab", typeof (GameObject)) as GameObject;

            PatrolLineMaterial = AssetDatabase.LoadAssetAtPath (LevelGridTool.ResPath + "NaviLine.mat", typeof (Material)) as Material;
            PatrolSelectLineMaterial = AssetDatabase.LoadAssetAtPath (LevelGridTool.ResPath + "NaviLineOneway.mat", typeof (Material)) as Material;

            LevelGridTool.ClearUnusedRoot ("PatrolRoot");
            PatrolNode = new GameObject ();
            PatrolNode.name = "PatrolRoot";
            PatrolNode.transform.position = Vector3.zero;

            m_Mode = _mode;
        }

        public void Clear ()
        {
            if (CurrentScenePathList != null)
            {
                for (int i = 0; i < CurrentScenePathList.Count; ++i)
                {
                    CurrentScenePathList[i].Clear ();
                }

                CurrentScenePathList.Clear ();
            }

            AllPatrolRecord.Clear ();
            CurrentScenePathList = null;
            CurrentOperatePath = null;
        }

        public bool IsPatrolInfoDirty ()
        {
            if (CurrentOperatePath != null)
                return CurrentOperatePath.LinkLineDirty;

            return false;
        }

        public void OnUpdate ()
        {
            for (int i = CurrentScenePathList.Count - 1; i >= 0; i--)
            {
                Transform t = PatrolNode.transform.Find("PatrolPath_" + CurrentScenePathList[i].PatrolID);
                if (t == null)
                {
                    CurrentScenePathList.RemoveAt(i);
                }
            }      
        }

        public void UpdateCurrentPath()
        {
            if (CurrentOperatePath != null)
            {
                for (int i = 0; i < CurrentOperatePath.path.Count; ++i)
                {
                    if (CurrentOperatePath.path[i].go.transform.position != CurrentOperatePath.path[i].pos)
                    {
                        Vector3 queryPos = CurrentOperatePath.path[i].go.transform.position;
                        QuadTreeElement selectGrid = m_Mode.m_tool.m_Data.GetPosHeight(queryPos);

                        if (selectGrid != null)
                        {
                            CurrentOperatePath.path[i].go.transform.position = selectGrid.pos;
                            CurrentOperatePath.path[i].pos = selectGrid.pos;
                        }
                        else
                        {
                            CurrentOperatePath.path[i].go.transform.position = CurrentOperatePath.path[i].pos;
                        }

                        CurrentOperatePath.LinkLineDirty = true;
                        LevelTool.wantsRepaint = true;
                    }
                }
            }
        }

        public void SetOperationPath (int patrolID)
        {
            CurrentOperatePath = null;
            if (patrolID == 0) return;
            for (int k = 0; k < CurrentScenePathList.Count; ++k)
            {
                if (patrolID == CurrentScenePathList[k].PatrolID)
                    CurrentOperatePath = CurrentScenePathList[k];
            }
        }

        private void CreatePatrolRecord ()
        {
            // find id slot
            int emptyID = 0;
            for (int i = 1; i < 10000; ++i)
            {
                bool bExist = false;
                for (int k = 0; k < CurrentScenePathList.Count; ++k)
                {
                    if (i == CurrentScenePathList[k].PatrolID)
                    {
                        bExist = true;
                        break;
                    }
                }

                if (!bExist)
                {
                    emptyID = i;
                    break;
                }
            }

            PatrolPath newPath = new PatrolPath ();
            newPath.PatrolID = emptyID;
            newPath.path = new List<PatrolPoint> ();
            newPath.PathRoot = new GameObject ();
            newPath.PathRoot.name = "PatrolPath_" + emptyID;
            newPath.PathRoot.transform.parent = PatrolNode.transform;

            CurrentScenePathList.Add (newPath);

            Selection.activeGameObject = newPath.PathRoot;
            CurrentOperatePath = newPath;
        }

        public void AddPatrolPoint (Vector3 newPoint)
        {
            QuadTreeElement selectGrid = m_Mode.m_tool.m_Data.GetPosHeight (newPoint);
            if (selectGrid == null) return;

            if (CurrentOperatePath == null)
            {
                CreatePatrolRecord ();
            }

            GameObject go = GameObject.Instantiate (PatrolPointPrafab) as GameObject;
            go.transform.parent = CurrentOperatePath.PathRoot.transform;
            go.transform.position = selectGrid.pos;
            go.name = "pp_" + (CurrentOperatePath.CacheMax);
            CurrentOperatePath.CacheMax++;
            CurrentOperatePath.LinkLineDirty = true;
            LevelTool.wantsRepaint = true;

        }

        public void LinkPatrolPoint ()
        {
            if (CurrentScenePathList == null) return;

            
  
            for (int i = 0; i < CurrentScenePathList.Count; ++i)
            {
                Transform pathRoot = CurrentScenePathList[i].PathRoot.transform;
                CurrentScenePathList[i].path.Clear ();

                for (int j = 0; j < pathRoot.childCount; ++j)
                {
                    Transform patrolPoint = pathRoot.GetChild (j);
                    if (patrolPoint.gameObject.name.StartsWith ("pp_"))
                    {
                        PatrolPoint p = new PatrolPoint ();
                        p.pos = patrolPoint.position;
                        p.HoldHere = patrolPoint.GetChild(0).GetComponent<ShipPointBehaviour>().HoldHere;
                        p.Time = patrolPoint.GetChild(0).GetComponent<ShipPointBehaviour>().patroIdleTime;
                        //p.Time2 = patrolPoint.GetChild(0).GetComponent<ShipPointBehaviour>().patroIdleTime2;
                        p.go = patrolPoint.gameObject;
                        CurrentScenePathList[i].path.Add (p);
                    }
                }

                CurrentScenePathList[i].LinkLineDirty = true;
            }

            GenerateLinkLine ();
        }

        private void GenerateLinkLine ()
        {
            if (CurrentScenePathList == null) return;

            for (int i = 0; i < CurrentScenePathList.Count; ++i)
            {
                if (!CurrentScenePathList[i].LinkLineDirty) continue;

                Transform pathRoot = CurrentScenePathList[i].PathRoot.transform;
                CurrentScenePathList[i].ClearLinkLine ();

                for (int j = 0; j < CurrentScenePathList[i].path.Count; ++j)
                {
                    // link line
                    if (j < CurrentScenePathList[i].path.Count - 1)
                    {
                        LinkLine ll = new LinkLine ();

                        ll.startIndex = j;
                        ll.endIndex = j + 1;
                        ll.go = GameObject.Instantiate (PatrolLinePrefab) as GameObject;
                        ll.go.name = "link_" + j + "_" + (j + 1);
                        ll.go.transform.parent = pathRoot;
                        ll.SetDirection (CurrentScenePathList[i].path[j].pos, CurrentScenePathList[i].path[j + 1].pos);

                        CurrentScenePathList[i].LinkLines.Add (ll);
                    }
                }

                CurrentScenePathList[i].LinkLineDirty = false;
                LevelTool.wantsRepaint = true;
            }
        }

        private void GetCurrentScenePathList ()
        {
            SceneContext sceneContext = new SceneContext ();
            if (EngineContext.IsRunning)
            {
                SceneAssets.GetSceneContext (ref sceneContext,
                    EngineContext.sceneName,
                    EngineContext.instance.sceneAsset.path);
            }
            else
                SceneAssets.GetCurrentSceneContext (ref sceneContext);

            string sceneName = sceneContext.dir.Substring(7);
            Debug.Log("PPPP" +sceneName);
            if (AllPatrolRecord.ContainsKey (sceneName))
            {
                CurrentScenePathList = AllPatrolRecord[sceneName];
            }
            else
            {
                CurrentScenePathList = new List<PatrolPath> ();
                //Debug.Log("PPPPPP" + sceneContext.dir);
                AllPatrolRecord.Add (sceneName, CurrentScenePathList);
            }

            for (int i = 0; i < CurrentScenePathList.Count; ++i)
            {
                CurrentScenePathList[i].PathRoot = new GameObject ();
                CurrentScenePathList[i].PathRoot.name = "PatrolPath_" + CurrentScenePathList[i].PatrolID;
                CurrentScenePathList[i].PathRoot.transform.parent = PatrolNode.transform;

                for (int j = 0; j < CurrentScenePathList[i].path.Count; ++j)
                {
                    GameObject go = GameObject.Instantiate (PatrolPointPrafab) as GameObject;
                    go.transform.parent = CurrentScenePathList[i].PathRoot.transform;
                    go.transform.position = CurrentScenePathList[i].path[j].pos;
                    go.name = "pp_" + j;
                    CurrentScenePathList[i].path[j].go = go;
                    var shippoint = go.transform.GetChild(0).GetComponent<ShipPointBehaviour>();
                    shippoint.HoldHere= CurrentScenePathList[i].path[j].HoldHere;
                    shippoint.patroIdleTime= CurrentScenePathList[i].path[j].Time;
                    //shippoint.patroIdleTime2= CurrentScenePathList[i].path[j].Time2;
                }

            }
        }

        public void LoadFromTable ()
        {
            _data_info = new PatrolTable ();

            if (!XTableReader.ReadFile ("Table/PatrolTable", _data_info))
            {
                Debug.Log ("<color=red>Error occurred when loading associate data file.</color>");
            }

            AllPatrolRecord.Clear ();

            if (_data_info.Table != null)
            {
                for (int i = 0; i < _data_info.Table.Length; ++i)
                {
                    PatrolTable.RowData row = _data_info.Table[i];

                    List<PatrolPath> RecordForScene = null;
                    if (AllPatrolRecord.ContainsKey (row.Scene))
                    {
                        RecordForScene = AllPatrolRecord[row.Scene];
                    }
                    else
                    {
                        RecordForScene = new List<PatrolPath> ();
                        AllPatrolRecord.Add (row.Scene, RecordForScene);
                    }

                    PatrolPath record = new PatrolPath ();
                    record.PatrolID = row.PatrolID;
                    //record.PathRoot = new GameObject();
                    //record.PathRoot.name = "PatrolPath_" + row.PatrolID;
                    //record.PathRoot.transform.parent = PatrolNode.transform;
                    if (record.path == null) record.path = new List<PatrolPoint> ();
                    record.path.Clear ();
                    for (int j = 0; j < row.Path.Count; ++j)
                    {
                        PatrolPoint p = new PatrolPoint ();
                        p.pos = new Vector3 (row.Path[j, 0], row.Path[j, 1], row.Path[j, 2]);
                        p.Time = row.Path[j,3];
                        p.HoldHere = row.Path[j, 4] == 0;
                        //p.Time2 = row.Path[j, 4];

                        //GameObject go = GameObject.Instantiate(PatrolPointPrafab) as GameObject;
                        //go.transform.parent = record.PathRoot.transform;
                        //go.transform.position = p.pos;
                        //go.name = "pp_" + j;
                        //p.go = go;

                        record.path.Add (p);
                    }

                    record.CacheMax = record.path.Count;
                    RecordForScene.Add (record);
                }
            }

            GetCurrentScenePathList ();
            GenerateLinkLine ();
        }

        public void SaveToTable ()
        {
            string[] col = new string[] { "ID", "Scene", "PatrolID", "Path" };
            string[] comment = new string[] { "ID", "Scene", "PatrolID", "Path" };

            string path = "Assets\\Table\\PatrolTable.txt";
            StreamWriter sw = XTableWriter.StartWriteFile (path);
            XTableWriter.WriteCol (sw, col);
            XTableWriter.WriteComment (sw, comment);

            foreach (KeyValuePair<string, List<PatrolPath>> pair in AllPatrolRecord)
            {
                List<PatrolPath> RecordForScene = pair.Value;

                for (int k = 0; k < RecordForScene.Count; ++k)
                {
                    string[] content = new string[4];
                    content[0] = "1";
                    content[1] = pair.Key;
                    content[2] = RecordForScene[k].PatrolID.ToString ();

                    string s = "";
                    for (int i = 0; i < RecordForScene[k].path.Count; ++i)
                    {
                        PatrolPoint p = RecordForScene[k].path[i];
                        float time,time2;
                        bool nextpoint;
                        if (p.go == null)
                        {
                            nextpoint = p.HoldHere;
                            time = p.Time;
                            //time2 = p.Time2;
                        }
                        else
                        {
                            var point = p.go.transform.GetChild(0).GetComponent<ShipPointBehaviour>();
                            nextpoint = point.HoldHere;
                            time = point.patroIdleTime;
                            //time2 = point.patroIdleTime2;
                        }
                        string ss = p.pos.x.ToString () + "=" + p.pos.y.ToString () + "=" + p.pos.z.ToString () + "=" + time+ "=" + (nextpoint?0:1) ;

                        s += ss;
                        if (i < RecordForScene[k].path.Count - 1) s += '|';
                    }

                    content[3] = s;

                    XTableWriter.WriteContent (sw, content);
                }

            }

            XTableWriter.EndWriteFile (sw);
        }

    }
}
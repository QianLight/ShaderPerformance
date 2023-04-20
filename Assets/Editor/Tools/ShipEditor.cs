using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.IO;
using System.Xml.Serialization;
using CFUtilPoolLib;
using System;
using System.Collections.Generic;
using XEditor.Level;
using CFEngine.Editor;

namespace XEditor
{

    public class BiDictionary<T1, T2>
        where T1: class
        where T2: class
    {
        public List<T1> keyArray = new List<T1>();
        public List<T2> valueArray = new List<T2>();

        public void Clear()
        {
            keyArray.Clear();
            valueArray.Clear();
        }

        public int Count()
        {
            return keyArray.Count;
        }

        public void Add(T1 key, T2 value)
        {
            keyArray.Add(key);
            valueArray.Add(value);
        }

        public void Remove(T1 key)
        {
            int i = -1;
            for ( i = 0; i < keyArray.Count; ++i)
            {
                if (keyArray[i] == key)
                {
                    keyArray.RemoveAt(i);
                    break;
                }
            }

            if (i >= 0 && i < valueArray.Count)
                valueArray.RemoveAt(i);
        }

        public T2 GetValue(T1 key)
        {
            for(int i = 0; i < keyArray.Count; ++i)
            {
                if (keyArray[i] == key) return valueArray[i];
            }

            return default(T2);
        }

        public T1 GetKey(T2 value)
        {
            for (int i = 0; i < valueArray.Count; ++i)
            {
                if (valueArray[i] == value) return keyArray[i];
            }

            return default(T1);
        }
    }
    public class PathEditorData
    {
        public GameObject pathRoot;

        public Dictionary<ShipPointData, GameObject> PointObject = new Dictionary<ShipPointData, GameObject>();

        public LineRenderer LineRender;

        public bool Contains(GameObject o)
        {
            foreach(KeyValuePair<ShipPointData, GameObject> pair in PointObject)
            {
                if (o == pair.Value) return true;
            }

            return false;
        }
    }

    public class EventEditorData
    {
        public GameObject eventRoot;

        public GameObject eventCamera;

        public Dictionary<ShipEventPointData, GameObject> PointObject = new Dictionary<ShipEventPointData, GameObject>();

    }

    class ShipEditor : EditorWindow
    {
        public int layer_mask;

        public static ShipEditor Instance = null;

        private GameObject ShipModel = null;
        ShipTemplate template = ShipTemplate.Meili;

        ShipData fullData = new ShipData();
        ShipPathData CurrentOpPath = null;
        ShipEventData CurrentOpEvent = null;

        BiDictionary<ShipPathData, PathEditorData> editorMapData = new BiDictionary<ShipPathData, PathEditorData>();
        BiDictionary<ShipEventData, EventEditorData> editorEventMapData = new BiDictionary<ShipEventData, EventEditorData>();

        private static GUIStyle normalStyle;
        private static GUIStyle boldStyle;

        private GameObject PathPointPrefab;
        private GameObject PathLinePrefab;
        private Material NormalLineMaterial;
        private Material SelectLineMaterial;

        public static GUIStyle NormalStyle
        {
            get
            {
                if (normalStyle == null)
                    normalStyle = new GUIStyle()
                    {
                        normal = { textColor = Color.black },//new Color(0.35f, 0.35f, 0.35f) },
                                                             //padding = new RectOffset(4, 4, 4, 4),
                        fontSize = Mathf.RoundToInt(12f),
                        fontStyle = FontStyle.Normal,

                        alignment = TextAnchor.MiddleLeft,
                    };
                return normalStyle;
            }
        }

        public static GUIStyle BoldStyle
        {
            get
            {
                if (boldStyle == null)
                    boldStyle = new GUIStyle()
                    {
                        normal = { textColor = Color.black },//new Color(0.35f, 0.35f, 0.35f) },
                                                             //padding = new RectOffset(4, 4, 4, 4),
                        fontSize = Mathf.RoundToInt(12f),
                        fontStyle = FontStyle.Bold,

                        alignment = TextAnchor.MiddleLeft,
                    };
                return boldStyle;
            }
        }

        [MenuItem(@"XEditor/ShipEditor")]
        static void ShowWindow()
        {
            //EditorWindow.GetWindowWithRect<XReactLineEditor>(new Rect(800f, 100f, 800f, 800f), false, @"React Line Editor", true);
            var window = EditorWindow.GetWindow<ShipEditor>(@"ShipEditor", true);
            window.position = new Rect(500, 400, 1000, 500);
            window.wantsMouseMove = true;
            window.Show();
            window.Repaint();
        }

        public void OnEnable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;

            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;

            layer_mask = (1 << LayerMask.NameToLayer("Dummy") | 1 << LayerMask.NameToLayer("Default"));

            PathPointPrefab = AssetDatabase.LoadAssetAtPath(LevelGridTool.ResPath + "NaviPoint.prefab", typeof(GameObject)) as GameObject;
            PathLinePrefab = AssetDatabase.LoadAssetAtPath(LevelGridTool.ResPath + "NaviLine.prefab", typeof(GameObject)) as GameObject;
            NormalLineMaterial = AssetDatabase.LoadAssetAtPath(LevelGridTool.ResPath + "NaviLine.mat", typeof(Material)) as Material;
            SelectLineMaterial = AssetDatabase.LoadAssetAtPath(LevelGridTool.ResPath + "NaviLineOneway.mat", typeof(Material)) as Material;
        }

        public void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            Selection.selectionChanged -= OnSelectionChanged;

            for(int i = 0; i < editorMapData.Count(); ++i)
            {
                GameObject.DestroyImmediate(editorMapData.valueArray[i].pathRoot);
                GameObject.DestroyImmediate(editorMapData.valueArray[i].LineRender);
            }

            for(int i = 0; i < editorEventMapData.Count(); ++i)
            {
                GameObject.DestroyImmediate(editorEventMapData.valueArray[i].eventRoot);
            }

            editorMapData.Clear();
            editorEventMapData.Clear();

            ShipModel = null;
            //if (ShipModel != null)
            //{
            //    GameObject.DestroyImmediate(ShipModel);
            //    ShipModel = null; 
            //}
        }

        void OnGUI()
        {
            if(ShipModel == null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("先选择一个船体", new GUILayoutOption[] { GUILayout.Width(150f) });
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                template = (ShipTemplate)EditorGUILayout.EnumPopup(template, new GUILayoutOption[] { GUILayout.Width(150f) });
                if(GUILayout.Button("load", new GUILayoutOption[] { GUILayout.Width(150f) }))
                {
                    EditorSceneManager.OpenScene("Assets/Scenes/Scenelib/OP_zhucheng/Op_zhucheng.unity");
                    ShipModel = GameObject.Find("船参考/OP_MLH_fbx_0");
                    ShipModel.SetActive(true);
                    Renderer[] r = ShipModel.GetComponentsInChildren<Renderer>();
                    for (int i = 0; i < r.Length; ++i)
                    {
                        r[i].gameObject.layer = LayerMask.NameToLayer("Dummy");
                        r[i].gameObject.AddComponent<MeshCollider>();
                    }

                    SceneView.lastActiveSceneView.LookAtDirect(ShipModel.transform.position, Quaternion.Euler(45, 0, 0), 20);

                    string configFile = "Assets/BundleRes/ReactPackage/Ship/" + template.ToString() + ".bytes";
                    LoadFromData(configFile);
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                if(GUILayout.Button("Save", new GUILayoutOption[] { GUILayout.Width(150f) }))
                {
                    DataIO.SerializeData<ShipData>("Assets/BundleRes/ReactPackage/Ship/" + template.ToString() + ".bytes", fullData);
                }

                if (GUILayout.Button("新增路径", new GUILayoutOption[] { GUILayout.Width(150f) }))
                {
                    NewPath();
                }
 
                DrawPathList();

                if (GUILayout.Button("新增事件", new GUILayoutOption[] { GUILayout.Width(150f) }))
                {
                    NewEvent();
                }

                DrawEventList();

            }
        }

        private void LoadFromData(string configFile)
        {
            fullData = DataIO.DeserializeData<ShipData>(configFile);

            _LoadPathData();
            _LoadEventData();
        }

        private void _LoadPathData()
        {
            for (int i = 0; i < fullData.allPathData.Count; ++i)
            {
                PathEditorData pathEditorData = new PathEditorData();
                GameObject pathRoot = new GameObject();
                pathRoot.name = "ShipEditorPath" + XCommon.singleton.XHash(Time.time.ToString() + i);
                pathRoot.transform.parent = ShipModel.transform;
                pathRoot.transform.localPosition = Vector3.zero;
                pathRoot.transform.localScale = Vector3.one;
                pathRoot.transform.rotation = Quaternion.identity;
                pathEditorData.pathRoot = pathRoot;

                editorMapData.Add(fullData.allPathData[i], pathEditorData);

                for (int j = 0; j < fullData.allPathData[i].pointData.Count; ++j)
                {
                    GameObject go = GameObject.Instantiate(PathPointPrefab) as GameObject;
                    go.transform.parent = pathRoot.transform;
                    go.transform.localPosition = fullData.allPathData[i].pointData[j].position;
                    go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    go.transform.localRotation = Quaternion.identity;

                    pathEditorData.PointObject.Add(fullData.allPathData[i].pointData[j], go);

                    ShipPointBehaviour behaviour = go.transform.GetChild(0).GetComponent<ShipPointBehaviour>();

                    behaviour.roomPos = fullData.allPathData[i].pointData[j].roomPos;
                    behaviour.displayProbability = fullData.allPathData[i].pointData[j].displayProbability;
                }
            }
        }

        private void _LoadEventData()
        {
            for (int i = 0; i < fullData.allEventData.Count; ++i)
            {
                EventEditorData eventEditorData = new EventEditorData();
                GameObject eventRoot = new GameObject();
                eventRoot.name = "ShipEvent" + XCommon.singleton.XHash(Time.time.ToString() + i);
                eventRoot.transform.parent = ShipModel.transform;
                eventRoot.transform.localPosition = Vector3.zero;
                eventRoot.transform.localScale = Vector3.one;
                eventRoot.transform.rotation = Quaternion.identity;
                eventEditorData.eventRoot = eventRoot;

                GameObject eventCamera = new GameObject();
                eventCamera.name = "EventCamera";
                eventCamera.AddComponent<Camera>();
                eventCamera.transform.parent = eventRoot.transform;
                eventCamera.transform.localPosition = fullData.allEventData[i].cameraPosition;
                eventCamera.transform.localScale = Vector3.one;
                eventCamera.transform.rotation = Quaternion.Euler(fullData.allEventData[i].cameraRotation);
                eventEditorData.eventCamera = eventCamera;

                editorEventMapData.Add(fullData.allEventData[i], eventEditorData);

                for (int j = 0; j < fullData.allEventData[i].shipEvent.Count; ++j)
                {
                    GameObject go = GameObject.Instantiate(PathPointPrefab) as GameObject;
                    go.transform.parent = eventRoot.transform;
                    go.transform.localPosition = fullData.allEventData[i].shipEvent[j].position;
                    go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    go.transform.localRotation = Quaternion.identity;

                    eventEditorData.PointObject.Add(fullData.allEventData[i].shipEvent[j], go);


                    ShipPointBehaviour behaviour = go.transform.GetChild(0).GetComponent<ShipPointBehaviour>();
                    behaviour.anim = fullData.allEventData[i].shipEvent[j].animation;
                    behaviour.displayProbability = 1;
                }
            }
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (ShipModel == null) return;

            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            //UnityEditor.HandleUtility.AddDefaultControl(controlID);

            switch (e.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (Event.current.button == 0)
                    {
                        //if(e.clickCount == 1)
                        //    Event.current.Use();
                        if (e.clickCount == 2)
                            OnMouseDoubleClick(sceneView);
                        //else if (e.clickCount == 2)
                        //    OnDoubleClick(sceneView);
                    }

                    break;
                //case EventType.ScrollWheel:
                //    break;
                //case EventType.MouseMove:
                //    OnMouseMove(sceneView);
                //    break;
                //case EventType.MouseDrag:
                //    if (e.button == 0)
                //    {
                //        if (OnMouseDrag(sceneView))
                //            Event.current.Use();
                //    }
                //    break;
                //case EventType.KeyDown:
                //    {
                //        if (OnKeyDown(sceneView))
                //            Event.current.Use();
                //    }
                //    break;

            }
            
        }

        private void OnMouseDoubleClick(SceneView sceneView)
        {
            if (CurrentOpPath == null && CurrentOpEvent == null) return;

            Ray r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            RaycastHit hitInfo;

            bool bHit = Physics.Raycast(r, out hitInfo, 10000.0f, layer_mask);

            if (bHit)
            {
                Vector3 clickPoint = hitInfo.point;
                AddPoint(clickPoint);
            }
        }

        private int UpdateFrequency = 10;
        private int _updateFrequency = 0;
        private void Update()   
        {
            if (_updateFrequency++ < UpdateFrequency) return;

            _updateFrequency = 0;

            for (int i = 0; i < editorMapData.keyArray.Count; ++i)
            {
                PathEditorData eData = editorMapData.valueArray[i];

                GameObject root = eData.pathRoot;

                int pointCount = root.transform.childCount;

                if(editorMapData.keyArray[i].pointData.Count != pointCount)
                {
                    Debug.LogError("Path: Logic Data != Editor Data");
                    continue;
                }

                for (int j = 0; j < pointCount; ++j)
                {
                    AssignPointData(editorMapData.keyArray[i].pointData[j], root.transform.GetChild(j).gameObject);
                }

                SetupLinkLine(eData, editorMapData.keyArray[i] == CurrentOpPath);
            }

            for (int i = 0; i < editorEventMapData.keyArray.Count; ++i)
            {
                EventEditorData eData = editorEventMapData.valueArray[i];

                GameObject root = eData.eventRoot;
                GameObject camera = eData.eventCamera;

                editorEventMapData.keyArray[i].cameraPosition = camera.transform.position - ShipModel.transform.position;
                editorEventMapData.keyArray[i].cameraRotation = camera.transform.rotation.eulerAngles;

                int pointCount = root.transform.childCount - 1;

                if (editorEventMapData.keyArray[i].shipEvent.Count != pointCount)
                {
                    Debug.LogError("Event: Logic Data != Editor Data");
                    continue;
                }

                for (int j = 0; j < pointCount; ++j)
                {
                    AssignEventData(editorEventMapData.keyArray[i].shipEvent[j], root.transform.GetChild(j + 1).gameObject);
                }
            }
        }

        private void AssignPointData(ShipPointData p, GameObject o)
        {
            p.position = o.transform.GetChild(0).position - ShipModel.transform.position - new Vector3(0, 0.65f, 0);

            ShipPointBehaviour behaviour = o.transform.GetChild(0).GetComponent<ShipPointBehaviour>();

            if (behaviour != null)
            {
                p.roomPos = behaviour.roomPos;
                p.displayProbability = behaviour.displayProbability;
            }
        }

        private void AssignEventData(ShipEventPointData p, GameObject o)
        {
            Transform t = o.transform.GetChild(0);

            p.position = t.position - ShipModel.transform.position - new Vector3(0, 0.65f, 0);

            ShipPointBehaviour behaviour = t.GetComponent<ShipPointBehaviour>();

            p.animation = "";
            if (behaviour != null)
            {
                p.rotation = t.rotation.eulerAngles;
                p.animation = behaviour.anim;
                //
            }
        }

        private void SetupLinkLine(PathEditorData eData, bool selected)
        {
            if(eData.LineRender == null)
            {
                GameObject go = GameObject.Instantiate(PathLinePrefab) as GameObject;
                go.transform.parent = ShipModel.transform;
                eData.LineRender = go.GetComponent<LineRenderer>();
            }

            eData.LineRender.positionCount = eData.pathRoot.transform.childCount;

            for(int i = 0; i < eData.pathRoot.transform.childCount; ++i)
            {
                Vector3 pos = eData.pathRoot.transform.GetChild(i).GetChild(0).position;
                eData.LineRender.SetPosition(i, pos);
            }

            eData.LineRender.material = selected ? SelectLineMaterial : NormalLineMaterial;

        }

        private void OnSelectionChanged()
        {
            GameObject o = Selection.activeGameObject;
            if (o == null)
            {
                UpdateCurrentPathDataByEditorData();
                UpdateCurrentEventDataByEditorData();
                return;
            }

            Transform t = o.transform;
            while (t != null)
            {
                if (t.gameObject.name.StartsWith("ShipEditorPath")) break;
                if (t.gameObject.name.StartsWith("ShipEventPath")) break;
                t = t.parent;
            }

            if (t != null && t.gameObject.name.StartsWith("ShipEditorPath"))
            {
                for (int i = 0; i < editorMapData.valueArray.Count; ++i)
                {
                    if (t.gameObject == editorMapData.valueArray[i].pathRoot)
                    {
                        CurrentOpPath = editorMapData.keyArray[i];
                        CurrentOpEvent = null;
                        break;
                    }
                }
            }

            if (t != null && t.gameObject.name.StartsWith("ShipEventPath"))
            {
                for(int i = 0; i < editorEventMapData.valueArray.Count; ++i)
                {
                    if (t.gameObject == editorEventMapData.valueArray[i].eventRoot)
                    {
                        CurrentOpEvent = editorEventMapData.keyArray[i];
                        CurrentOpPath = null;
                        break;
                    }
                }
            }

            Repaint();
        }

        private void UpdateCurrentPathDataByEditorData()
        {
            
            if (CurrentOpPath != null)
            {
                PathEditorData eData = editorMapData.GetValue(CurrentOpPath);

                if (eData == null) return;

                GameObject rootObject = eData.pathRoot;

                ShipPointData deleteKey = null;

                for (int i = 0; i < rootObject.transform.childCount; ++i)
                {
                    if(rootObject.transform.GetChild(i).childCount == 0)
                    {
                        GameObject.DestroyImmediate(rootObject.transform.GetChild(i).gameObject);
                        break;
                    }
                }

                foreach (KeyValuePair<ShipPointData, GameObject> pair in eData.PointObject)
                {
                    bool bFind = false;
                    for (int i = 0; i < rootObject.transform.childCount; ++i)
                    {
                        if(rootObject.transform.GetChild(i).gameObject == pair.Value)
                        {
                            bFind = true;
                            break;
                        }
                    }

                    if(!bFind)
                    {
                        deleteKey = pair.Key;
                        break;
                    }

                }

                if (deleteKey != null)
                {
                    CurrentOpPath.pointData.Remove(deleteKey);
                    eData.PointObject.Remove(deleteKey);
                }
                
            }
        }

        private void UpdateCurrentEventDataByEditorData()
        {
            if (CurrentOpEvent != null)
            {
                EventEditorData eData = editorEventMapData.GetValue(CurrentOpEvent);

                if (eData == null) return;

                GameObject rootObject = eData.eventRoot;

                ShipEventPointData deleteKey = null;

                for (int i = 1; i < rootObject.transform.childCount; ++i)
                {
                    if (rootObject.transform.GetChild(i).childCount == 0)
                    {
                        GameObject.DestroyImmediate(rootObject.transform.GetChild(i).gameObject);
                        break;
                    }
                }

                foreach (KeyValuePair<ShipEventPointData, GameObject> pair in eData.PointObject)
                {
                    bool bFind = false;
                    for (int i = 1; i < rootObject.transform.childCount; ++i)
                    {
                        if (rootObject.transform.GetChild(i).gameObject == pair.Value)
                        {
                            bFind = true;
                            break;
                        }
                    }

                    if (!bFind)
                    {
                        deleteKey = pair.Key;
                        break;
                    }

                }

                if (deleteKey != null)
                {
                    CurrentOpEvent.shipEvent.Remove(deleteKey);
                    eData.PointObject.Remove(deleteKey);
                }

            }
        }

        private void NewPath()
        {
            ShipPathData newPath = new ShipPathData();
            fullData.allPathData.Add(newPath);

            PathEditorData pathEditorData = new PathEditorData();
            GameObject pathRoot = new GameObject();
            pathRoot.name = "ShipEditorPath" + XCommon.singleton.XHash(Time.time.ToString());
            pathRoot.transform.parent = ShipModel.transform;
            pathRoot.transform.localPosition = Vector3.zero;
            pathRoot.transform.localScale = Vector3.one;
            pathRoot.transform.rotation = Quaternion.identity;
            pathEditorData.pathRoot = pathRoot;

            editorMapData.Add(newPath, pathEditorData);

            CurrentOpEvent = null;
            CurrentOpPath = newPath;
        }

        private void NewEvent()
        {
            ShipEventData newEvent = new ShipEventData();
            fullData.allEventData.Add(newEvent);

            EventEditorData eventEditorData = new EventEditorData();
            GameObject eventRoot = new GameObject();
            eventRoot.name = "ShipEvent" + XCommon.singleton.XHash(Time.time.ToString());
            eventRoot.transform.parent = ShipModel.transform;
            eventRoot.transform.localPosition = Vector3.zero;
            eventRoot.transform.localScale = Vector3.one;
            eventRoot.transform.rotation = Quaternion.identity;
            GameObject eventCamera = new GameObject();
            eventCamera.name = "EventCamera";
            eventCamera.AddComponent<Camera>();
            eventCamera.transform.parent = eventRoot.transform;
            eventCamera.transform.localPosition = Vector3.zero;
            eventCamera.transform.localScale = Vector3.one;
            eventCamera.transform.rotation = Quaternion.identity;

            eventEditorData.eventRoot = eventRoot;
            eventEditorData.eventCamera = eventCamera;
            editorEventMapData.Add(newEvent, eventEditorData);

            CurrentOpPath = null;
            CurrentOpEvent = newEvent;
        }


        private void DrawPathList()
        {
            int toberemove = -1;
            for(int i = 0; i < fullData.allPathData.Count; ++i)
            {
                PathEditorData eData = editorMapData.GetValue(fullData.allPathData[i]);
                EditorGUILayout.BeginHorizontal();

                GUIStyle style = CurrentOpPath == fullData.allPathData[i] ? BoldStyle : NormalStyle;
                EditorGUILayout.LabelField("path" + eData.pathRoot.name, style);
                if(GUILayout.Button("-", new GUILayoutOption[] { GUILayout.Width(20f) }))
                {
                    toberemove = i;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (toberemove >= 0)
            {
                if (fullData.allPathData[toberemove] == CurrentOpPath)
                    CurrentOpPath = null;

                PathEditorData eData = editorMapData.GetValue(fullData.allPathData[toberemove]);
                if(eData != null)
                {
                    GameObject root = eData.pathRoot;
                    GameObject.DestroyImmediate(root);

                    if(eData.LineRender != null) GameObject.DestroyImmediate(eData.LineRender);

                    editorMapData.Remove(fullData.allPathData[toberemove]);
                }

                fullData.allPathData.RemoveAt(toberemove);
            }
        }

        private void DrawEventList()
        {
            int toberemove = -1;
            for (int i = 0; i < fullData.allEventData.Count; ++i)
            {
                EventEditorData eData = editorEventMapData.GetValue(fullData.allEventData[i]);
                EditorGUILayout.BeginHorizontal();

                GUIStyle style = CurrentOpEvent == fullData.allEventData[i] ? BoldStyle : NormalStyle;
                EditorGUILayout.LabelField("path" + eData.eventRoot.name, style);
                if (GUILayout.Button("-", new GUILayoutOption[] { GUILayout.Width(20f) }))
                {
                    toberemove = i;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (toberemove >= 0)
            {
                if (fullData.allEventData[toberemove] == CurrentOpEvent)
                    CurrentOpEvent = null;

                EventEditorData eData = editorEventMapData.GetValue(fullData.allEventData[toberemove]);
                if (eData != null)
                {
                    GameObject root = eData.eventRoot;
                    GameObject.DestroyImmediate(root);

                    editorEventMapData.Remove(fullData.allEventData[toberemove]);
                }

                fullData.allEventData.RemoveAt(toberemove);
            }
        }

        private void AddPoint(Vector3 point)
        {
            if (CurrentOpPath != null)
            {
                AddPathPoint(point);
            }

            if(CurrentOpEvent != null)
            {
                AddEventPoint(point);
            }

            
        }

        private void AddPathPoint(Vector3 point)
        {
            ShipPointData data = new ShipPointData() { position = point - ShipModel.transform.position };
            CurrentOpPath.pointData.Add(data);

            PathEditorData eData = editorMapData.GetValue(CurrentOpPath);
            if (eData != null)
            {
                GameObject go = GameObject.Instantiate(PathPointPrefab) as GameObject;
                go.transform.parent = eData.pathRoot.transform;
                go.transform.position = point;
                go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                go.transform.localRotation = Quaternion.identity;

                eData.PointObject.Add(data, go);
            }
        }

        private void AddEventPoint(Vector3 point)
        {
            ShipEventPointData data = new ShipEventPointData() { position = point - ShipModel.transform.position, rotation = Vector3.zero };
            CurrentOpEvent.shipEvent.Add(data);

            EventEditorData eData = editorEventMapData.GetValue(CurrentOpEvent);
            if (eData != null)
            {
                GameObject go = GameObject.Instantiate(PathPointPrefab) as GameObject;
                go.transform.parent = eData.eventRoot.transform;
                go.transform.position = point;
                go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                go.transform.localRotation = Quaternion.identity;

                eData.PointObject.Add(data, go);
            }
        }

        
    }
}

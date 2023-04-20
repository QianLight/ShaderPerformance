using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;

public partial class FPDebugWindow : EditorWindow
{
    private static FPDebugWindow window;
    private static int width = 780, height = 850;
    static string[] tabStr = new string[] { "基础性能调试", "Shader性能调试", "模型调试" };
    static int tabIndex = 0;
    static bool run = false;
    [MenuItem("Tools/引擎/FPDebugWindow")]
    static void Init()
    {
        window = (FPDebugWindow)EditorWindow.GetWindow(typeof(FPDebugWindow));
        window.titleContent = new GUIContent("FPDebugWindow V" + Version.V);
        window.position = new Rect((Screen.currentResolution.width - width) / 2, (Screen.currentResolution.height - height) / 2, width, height);
        window.Show();
        tabIndex = 0;
    }
    void OnEnable()
    {
        run = true;
        connectIP = PlayerPrefs.GetString("FPDebugWindowConnectIP");
        SelectIPS = connectIP.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (SelectIPS.Length == 0)
        {
            connectIP = "10.1.65.115";
        }
        else
        {
            connectIP = SelectIPS[0];
        }
        tabIndex = 0;
    }

    void OnDisable()
    {
        run = false;
        clearScene();
        FPDebugObjectItem.ObjectHandle = null;
        FPDebugObjectItem.PostHandle = null;
        FPDebugObjectItem.ParameterHandle = null;
        FPMaterialView.ClearList();
    }

    private void deleteAllObjects()
    {
        GameObject[] all = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
        for (int i = 0; i < all.Length; i++)
        {
            var item = all[i];
            if (item.scene.isLoaded)
            {
                GameObject.DestroyImmediate(item);
            }
        }
    }
    private void createList(SCList sc)
    {
        for (int i = 0; i < sc.List.Count; i++)
        {
            SC scene = sc.List[i];
            Transform parent = createScene(scene);
            for (int j = 0; j < scene.OList.Count; j++)
            {
                OI oi = scene.OList[j];
                createObject(parent, oi);
            }
            for (int j = 0; j < scene.PList.Count; j++)
            {
                PP pp = scene.PList[j];
                if (objList.ContainsKey(pp.ID))
                {
                    GameObject obj = objList[pp.ID];
                    createPost(obj.GetComponent<FPDebugObjectItem>(), pp);
                }
            }
        }
    }
    private void clearScene()
    {
        for (int i = 0; i < sceneList.Count; i++)
        {
            GameObject scene = sceneList[i];
            GameObject.DestroyImmediate(scene);
        }
        objList.Clear();
    }
    private Transform createScene(SC sc)
    {
        //oi.ID
        GameObject ori = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("FPDebugClient"));
        ori.name = "Scene_" + sc.N;
        ori.transform.position = Vector3.zero;
        sceneList.Add(ori);
        return ori.transform;
    }
    private void createObject(Transform scene, OI oi)
    {
        //oi.ID
        GameObject ori = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("FPDebugClient"));
        FPDebugObjectItem item = ori.GetComponent<FPDebugObjectItem>();
        ori.name = oi.N;
        objList[oi.ID] = ori;
        if (oi.PID == 0)
        {
            ori.transform.parent = scene;
        }
        else
        {
            ori.transform.parent = objList[oi.PID].transform;
        }
        if (oi.E == 0)
        {
            ori.SetActive(false);
        }
        item.RemoteID = oi.ID;
        ori.transform.localPosition = oi.Po;
        ori.transform.localRotation = Quaternion.Euler(oi.Ro);
        ori.transform.localScale = oi.Sc;
        if (oi.R == 1)
        {
            //Renderer render = ori.GetComponent<Renderer>();
            //render.enabled = true;
            ori.transform.localScale = oi.Sc;
            //ori.transform.localScale = new Vector3(oi.Sc.x * oi.Si.x, oi.Sc.y * oi.Si.y, oi.Sc.z * oi.Si.z);
        }
        else
        {
            ori.transform.localScale = oi.Sc;
        }
    }
    private void createPost(FPDebugObjectItem item, PP pp)
    {
        if (item == null || pp.VL == null || pp.VL.Count == 0)
            return;

        item.VL = pp.VL;
    }
    private List<GameObject> sceneList = new List<GameObject>();
    private Dictionary<int, GameObject> objList = new Dictionary<int, GameObject>();
    private Dictionary<int, SelectObject> enableList = new Dictionary<int, SelectObject>();
    private Dictionary<int, SelectObject> disableList = new Dictionary<int, SelectObject>();
    private List<int> tmpList = new List<int>();
    public struct SelectObject
    {
        public int Id;
        public int Parent;
        public SelectObject(int id, int parent)
        {
            Id = id;
            Parent = parent;
        }
    }
    private void getFail(object ok)
    {
        if (ok == null)
        {
            getFps = 0;
            connect = false;
        }
    }
    public void ObjectEnable(int parentId, int id, bool enable)
    {
        if (!run)
            return;
        wait = true;
        if (enable)
        {
            enableList.Add(id, new SelectObject(id, parentId));
        }
        else
        {
            disableList.Add(id, new SelectObject(id, parentId));
        }
    }
    public void PostEnable(int id, string post, bool enable)
    {
        wait = true;
        ClientMessage.SetPost(id, post, enable, delegate (object ok)
        {
            wait = false;
            getFail(ok);
        });
    }
    public void PostParameter(int id, string post, string para, bool enable, string value)
    {
        wait = true;
        ClientMessage.SetPostPara(id, post, para, enable, value, delegate (object ok)
        {
            wait = false;
            getFail(ok);
        });
    }
    private static string connectIP = "";
    public static string[] SelectIPS = null;
    private bool connect = false;
    private bool wait = false;
    [NonSerialized]
    private RenderInfo renderInfo = null;
    private string modelInfo = null;
    private string[] fps = null;
    private int getFps = 0, maxFpsDuration = 10;
    Rect buttonRect;
    public static void SaveIPS(string ip)
    {
        List<string> strList = new List<string>();
        if (SelectIPS != null)
        {
            strList.AddRange(SelectIPS);
            for (int i = 0; i < strList.Count; i++)
            {
                if (strList[i] == ip)
                {
                    strList.RemoveAt(i);
                }
            }
        }

        if (!string.IsNullOrEmpty(ip))
        {
            strList.Insert(0, ip);
        }
        if (strList.Count > 7)
        {
            strList.RemoveAt(strList.Count - 1);
        }
        SelectIPS = strList.ToArray();
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < strList.Count; i++)
        {
            if (i != 0)
                sb.Append(",");
            sb.Append(strList[i]);
        }
        PlayerPrefs.SetString("FPDebugWindowConnectIP", sb.ToString());
    }
    public static void SelectIP(int index, bool delect)
    {
        if (SelectIPS != null)
        {
            if (delect)
            {
                List<string> strList = new List<string>();
                strList.AddRange(SelectIPS);
                strList.RemoveAt(index);
                SelectIPS = strList.ToArray();
            }
            else
            {
                connectIP = SelectIPS[index];
            }
        }
        window.Repaint();
    }
    private void updateSelect(Dictionary<int, SelectObject> list, bool enable)
    {
        tmpList.Clear();
        foreach (KeyValuePair<int, SelectObject> k in list)
        {
            if (!list.ContainsKey(k.Value.Parent))
            {
                tmpList.Add(k.Key);
            }
        }
        for (int i = 0; i < tmpList.Count; i++)
        {
            int id = tmpList[i];
            ClientMessage.SetObject(id, enable, delegate (object ok)
            {
                wait = false;
                getFail(ok);
            });
        }

        list.Clear();
    }
    private void Update()
    {
        if (enableList.Count > 0)
        {
            updateSelect(enableList, true);
        }
        if (disableList.Count > 0)
        {
            updateSelect(disableList, false);
        }
        if (getFps > 0)
        {
            getFps++;
            if (getFps > maxFpsDuration)
            {
                ClientMessage.GetFPS(false, delegate (string[] mFps)
                {
                    if (mFps == null)
                    {
                        getFail(mFps);
                        fps = null;
                    }
                    else
                    {
                        fps = mFps;
                    }
                });
                getFps = 1;
            }
        }
    }
    private void getObjects(SCList obj)
    {
        if (obj != null)
        {
            FPDebugObjectItem.ObjectHandle = null;
            FPDebugObjectItem.PostHandle = null;
            FPDebugObjectItem.ParameterHandle = null;
            clearScene();
            FPDebugObjectItem.ObjectHandle = ObjectEnable;
            FPDebugObjectItem.PostHandle = PostEnable;
            FPDebugObjectItem.ParameterHandle = PostParameter;
            createList(obj);
        }
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        connectIP = EditorGUILayout.TextField("IP:", connectIP);
        if (GUILayout.Button("连接历史", GUILayout.Width(80)))
        {
            PopupWindow.Show(buttonRect, new PopupExample());
        }
        if (Event.current.type == EventType.Repaint) buttonRect = GUILayoutUtility.GetLastRect();
        EditorGUILayout.EndHorizontal();

        if (wait)
        {
            GUILayout.Label("Wait...");
        }
        else
        {
            if (connect)
            {
                tabIndex = GUILayout.Toolbar(tabIndex, tabStr);
                switch (tabIndex)
                {
                    case 0:
                        {
                            drawRenderSetting();
                            break;
                        }
                    case 1:
                        {
                            drawShaderDebug();
                            break;
                        }
                    case 2:
                        {
                            drawModelDebug();
                            break;
                        }

                }

            }
            else
            {
                if (GUILayout.Button("链接手机"))
                {
                    ClientMessage.Init(connectIP);
                    ClientMessage.Start(delegate (object ok)
                    {
                        if (ok != null)
                        {
                            connect = true;
                            SaveIPS(connectIP);
                            ClientMessage.GetObjectList(delegate (SCList obj)
                            {
                                getFail(obj);
                                getObjects(obj);
                            });
                            getModelInfo();
                            ClientMessage.GetRenderInfo(delegate (RenderInfo obj)
                            {
                                getFail(obj);
                                renderInfo = obj;
                                if(renderInfo != null)
                                {
                                    FPShaderFix.ShaderPlatform = renderInfo.Platform;
                                }
                            });
                        }
                        else
                        {
                            getFail(ok);
                        }
                    });
                }
            }
            if (GUILayout.Button("Close"))
            {
                Close();
            }
        }
    }
    delegate void doDragObjs(UnityEngine.Object[] handleObjs);

    void onMouseDrag(doDragObjs handle)
    {
        var e = Event.current;
        if (e.type == EventType.DragUpdated || e.type == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (e.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                handle(DragAndDrop.objectReferences);
            }
        }
    }
    private void getModelInfo()
    {
        ClientMessage.GetModelInfo(delegate (string res)
        {
            getFail(res);
            modelInfo = res;
        });
    }

    public class PopupExample : PopupWindowContent
    {
        public override Vector2 GetWindowSize()
        {
            return new Vector2(200, 150);
        }

        public override void OnGUI(Rect rect)
        {
            if (SelectIPS != null)
            {
                for (int i = 0; i < SelectIPS.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(SelectIPS[i]))
                    {
                        SelectIP(i, false);
                    }
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        SelectIP(i, true);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        public override void OnOpen()
        {

        }

        public override void OnClose()
        {

        }
    }
}

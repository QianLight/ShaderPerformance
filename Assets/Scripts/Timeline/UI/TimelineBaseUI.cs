using CFEngine;
using CFUtilPoolLib;
using UnityEngine;

public class XAdaptive
{
    public Vector2 left;
    public Vector2 right;
}

public class TimelineBaseUI<T, AST> : XSingleton<T>, ITimelineUI where T : new()
    where AST : new()
{
    private static XAdaptive m_xAdaptive = new XAdaptive();
    public ScreenOrientation m_currentOrientation = Screen.orientation;

    public GameObject go;
    private AssetHandler asset;

    protected virtual string prefab { get; set; }

    const string suffix = "UI/OPsystemprefab/cutscene/";
    protected virtual string Dir
    {
        get
        {
            return suffix;
        }
    }

    protected AST arg;

    public GameObject GetUIRoot() { return go; }


    protected bool isload
    {
        get
        {
            if (go == null && !EngineContext.IsRunning)
            {
                // editor mode
                var canvas = TimelineUIMgr.canvas;
                if (canvas)
                {
                    string path = string.Format("{0}(Clone)", prefab);
                    var tf = canvas.transform.Find(path);
                    if (tf) go = tf.gameObject;
                }
            }
            return go;
        }
    }

    public void CheckAdaptive()
    {
        if (Screen.orientation == m_currentOrientation) return;
        m_currentOrientation = Screen.orientation;
        CalAdaptive();
        AdjustAdaptive(this.go);
    }

    public void CalAdaptive()
    {
        XDebug.singleton.AddGreenLog("TimelineBaseUI Screen full:" + Screen.width + ":" + Screen.height + ":" + Screen.orientation);
        XDebug.singleton.AddGreenLog("TimelineBaseUI Screen Safearea:" + Screen.safeArea.width + ":" + Screen.safeArea.height + ":" + Screen.safeArea.min + ":" + Screen.safeArea.max);
        m_xAdaptive.left = new Vector2(Screen.safeArea.min.x / Screen.width * 1624, 0);
        m_xAdaptive.right = new Vector2((Screen.safeArea.max.x - Screen.width) / Screen.width * 1624, 0);
        XDebug.singleton.AddGreenLog("TimelineBaseUI adaptive:" + m_xAdaptive.left + ":" + m_xAdaptive.right);
    }

    public void AdjustAdaptive(GameObject go)
    {
        if (go == null) return;
        RectTransform rt = go.transform as RectTransform;
        if (rt == null) return;
        rt.offsetMin = m_xAdaptive.left;
        rt.offsetMax = m_xAdaptive.right;
        XDebug.singleton.AddGreenLog("AdjustAdaptive:" + rt.offsetMin + ":" + rt.offsetMax);
    }

    public bool isShow
    {
        get { return go != null && go.activeSelf; }
    }

    public void Return()
    {
        OnDestroy();
        arg = default(AST);
        LoadMgr.singleton.Destroy(ref asset);
        if (go != null)
        {
            //XResourceLoaderMgr.singleton.UnSafeDestroy(go);
            go = null;
        }
    }

    public TComponent GetComponent<TComponent>(string name)
    {
        TComponent res = default;
        Transform tf = go.transform.Find(name);
        if (tf != null) res = tf.GetComponent<TComponent>();
        return res;
    }

    private GameObject Initial()
    {
        if (!isload)
        {
            go = EngineUtility.LoadPrefab(Dir + prefab, ref asset, 0, true, null, true);
            //go = XResourceLoaderMgr.singleton.CreateFromPrefab(suffix + prefab) as GameObject;
            var can = TimelineUIMgr.canvas;
            if (can == null) return go;
            var rect = go.transform.GetComponent<RectTransform>();
            go.transform.SetParent(can.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMax = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.pivot = 0.5f * Vector2.one;
        }
        if (!go.activeSelf) go.SetActive(true);
        OnCreated();
        ControlLayer();
        TimelineUIMgr.Regist(this);
        return go;
    }

    public virtual void Show(AST arg)
    {
        this.arg = arg;
        Initial();
    }

    public virtual void Clean()
    {
        if (go) go.SetActive(false);
    }

    protected virtual void OnCreated() { }

    protected virtual void ControlLayer()
    {

    }
    protected virtual void OnDestroy()
    {
    }

    public virtual void Update(float time)
    {
    }
}
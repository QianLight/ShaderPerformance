using CFUtilPoolLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.CFUI;

public class TimelinePvpUI : TimelineBaseUI<TimelinePvpUI, UISign>
{
    protected override string prefab { get { return "PVPStart1"; } }

    private List<PVPDataView> m_pvpDataViews = new List<PVPDataView>();
    private CFText m_roleName;
    private CFText m_gradeName;
    private CFRawImage m_icon;
    private List<System.Object> m_infos;
    private const int NUM = 6;
    private bool inited = false;
    private XRuntimeFmod fmod;


    private Transform self;
    private Transform opponent;

    private PVPInfo selfInfo;
    private PVPInfo opponentInfo;

    private class PVPInfo
    {
        public CFText name;
        public CFText gradeName;
        public CFRawImage icon;
    }

    private class PVPDataView
    {
        public Transform go;
        public CFText name;
        public CFText level;
        public CFImage quality;
        public Transform startPrefab;
        public List<Transform> stars;
        public List<Transform> cachedStars;
        // public CFImage color;
        public CFGridLayoutGroup group;
        public void Recycle()
        {
            if (stars != null)
            {
                cachedStars?.AddRange(stars);
            }
            stars.Clear();
        }
    }

    public override void Update(float time)
    {
        if (this.go == null) return;
        base.Update(time);
        CheckAdaptive();
    }

    public override void Show(UISign sign)
    {
        base.Show(sign);
        InitUI();
        Recycle();
        CalAdaptive();
        AdjustAdaptive(this.go);

        m_infos = (List<System.Object>)RTimeline.singleton.Arg; //0-枚举类型 1~6-伙伴双方 7-对方 8-自己
        if (m_infos == null) return;

        PVPData opponentData = m_infos[7] as PVPData;
        PVPData selfData = m_infos[8] as PVPData;
        SetRoleInfo(selfInfo, selfData);
        SetRoleInfo(opponentInfo, opponentData);

        if (fmod == null)
        {
            fmod = XRuntimeFmod.GetFMOD();
        }
        if (sign == UISign.TEAM_OPEN1)
        {
            self.gameObject.SetActive(false);
            opponent.gameObject.SetActive(true);
            SetHandleInfoByOffset(1);
        }
        else if (sign == UISign.TEAM_OPEN2)
        {
            self.gameObject.SetActive(true);
            opponent.gameObject.SetActive(false);
            SetHandleInfoByOffset(4);
        }
    }

    private void SetRoleInfo(PVPInfo view, PVPData data)
    {
        if (data != null)
        {
            view.name.text = data.name;
            view.gradeName.text = data.gradeName;
            view.icon.SetTexturePath(data.iconPath);
        }
    }

    private void InitUI()
    {
        if (inited) return;
        inited = true;

        self = go.transform.Find("Self");
        opponent = go.transform.Find("Opponent");

        selfInfo = new PVPInfo();
        opponentInfo = new PVPInfo();
        InitInfo(selfInfo, self.gameObject);
        InitInfo(opponentInfo, opponent.gameObject);
        m_pvpDataViews = new List<PVPDataView>();
        for (int i = 0; i < NUM; ++i)
        {
            PVPDataView view = new PVPDataView();
            if (view.stars == null)
            {
                view.stars = new List<Transform>();
            }
            if (view.cachedStars == null)
            {
                view.cachedStars = new List<Transform>();
            }
            Transform handle = null;
            if (i < 3)
            {
                handle = go.transform.Find("Opponent/RoleInfoHandle" + (i + 1));
            }
            else
            {
                handle = go.transform.Find("Self/RoleInfoHandle" + (i - 2));
            }
            InitRoleInfoHandle(view, handle);
            m_pvpDataViews.Add(view);
        }
    }

    private void InitInfo(PVPInfo info, GameObject root)
    {
        info.name = GetComponent<CFText>(root.name + "/PlayerInfo/Name");
        info.gradeName = GetComponent<CFText>(root.name + "/PlayerInfo/Gradename");
        info.icon = GetComponent<CFRawImage>(root.name + "/PlayerInfo/Gradename/Icon");
    }

    private void InitRoleInfoHandle(PVPDataView view, Transform handle)
    {
        Transform go = handle;
        CFText name = handle.Find("Level/Name").GetComponent<CFText>();
        CFText level = handle.Find("Level").GetComponent<CFText>();
        CFImage quality = handle.Find("Quality").GetComponent<CFImage>();
        CFGridLayoutGroup group = handle.Find("Star").GetComponent<CFGridLayoutGroup>();
        Transform starPrefab = handle.Find("Star/temp");
        starPrefab.gameObject.SetActive(false);
        view.go = go;
        view.name = name;
        view.level = level;
        view.quality = quality;
        view.startPrefab = starPrefab;
        view.group = group;
    }

    private void Recycle()
    {
        for (int i = 0; i < m_pvpDataViews.Count; ++i)
        {
            m_pvpDataViews[i].Recycle();
        }
    }

    private void SetHandleInfoByOffset(int offset)
    {
        for (int i = 0; i < 3; ++i)
        {
            int index = i + offset; //1~3 and 4~6
            if (m_infos != null && index < m_infos.Count)
            {
                PVPData data = m_infos[index] as PVPData; //m_infos 0-枚举类型 1~6-伙伴双方 7-对方 8-自己
                SetHandleInfo(m_pvpDataViews[index - 1], data); //m_pvpDataViews有6个，所以从0~5
                if (i == 0)
                {
                    fmod.StartEvent("event:/" + data.eventName, AudioChannel.Action, true);
                }
            }
        }
    }

    private void SetHandleInfo(PVPDataView view, PVPData data)
    {
        if (data != null)
        {
            view.go.gameObject.SetActive(true);
            view.name.text = data.name;
            view.level.text = "Lv." + data.level.ToString();
            string atlas = "ui_comm2";
            string spriteName = XCommon.Format("ui_character_quality_{0}", 5 - data.quality);
            view.quality.Bind(spriteName, atlas); //稀有度
            Debug.Log("SetHandleInfo atlas=" + atlas + " spriteName=" + spriteName);
            for (int i = 0; i < data.starNum; ++i)
            {
                if (view.cachedStars.Count > 0)
                {
                    Transform t = view.cachedStars[view.cachedStars.Count - 1];
                    t.gameObject.SetActive(true);
                    view.cachedStars.Remove(t);
                    view.stars.Add(t);
                }
                else
                {
                    Transform newStar = GameObject.Instantiate(view.startPrefab);
                    newStar.SetParent(view.startPrefab.parent);
                    newStar.gameObject.SetActive(true);
                    newStar.localScale = Vector3.one;
                    view.stars.Add(newStar);
                }
            }
            for (int i = 0; i < view.cachedStars.Count; ++i)
            {
                view.cachedStars[i].gameObject.SetActive(false);
            }
        }
        else
        {
            view.go.gameObject.SetActive(false);
        }
    }

    //private T GetComponent<T>(string name)
    //{
    //    T res = default(T);
    //    Transform tf = go.transform.Find(name);
    //    if (tf != null) res = tf.GetComponent<T>();
    //    return res;
    //}

    protected override void OnDestroy()
    {
        m_pvpDataViews = null;
        inited = false;
        base.OnDestroy();
        if (fmod != null)
        {
            XRuntimeFmod.ReturnFMOD(fmod);
            fmod = null;
        }
    }
}

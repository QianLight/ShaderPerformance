using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CFClient;
using CFUtilPoolLib;
using RuntimeProfile;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Networking;
using Object = System.Object;

public class RuntimeProfilerCtrl : MonoBehaviour
{
    private static RuntimeProfilerCtrl instance;

    public bool SetActive(params string[] values)
    {
        if (values.Length > 1)
        {
            if (values[1] == "0")
            {
                IsProfile = false;
            }

            if (values[1] == "1")
            {
                IsProfile = true;
            }
        }

        return true;
    }
    
    public static RuntimeProfilerCtrl Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("RuntimeProfiler");
                GameObject.DontDestroyOnLoad(go);
                instance = go.AddComponent<RuntimeProfilerCtrl>();
            }

            return instance;
        }
        
    }

    [SerializeField] private  String ServiceUrl = "http://10.253.48.71:8080";
    private const int MaxRecordNum = 3;
    private ProfilerDataEndRequest _profilerDataEndRequest;


    private bool isProfile;


    private static readonly int TickClacNum = 3;

    private Dictionary<String, StatInfo> _profilterDict;
    private static readonly int CacheProfilerDataNum = 10;
    private NeedSendToServerData<ProfilerData> _profilerDataList;
    private static Vector2Int _savePicSize = new Vector2Int(960, 720);
    private Texture2D UtilPngTex2d;
    private uint _nowTick;

    private IXScene iXScene;

    private uint LastSceneId;

    // 由服务器发回 作为此关卡记录的凭证 每针数据都需要带着此id发送
    private uint nowSceneDataId;

    public delegate void ProfileDataFullEvent();

    private ProfileDataFullEvent _profileDataFullEvent;

    public void AddProfileDataFullEvent(ProfileDataFullEvent e)
    {
        _profileDataFullEvent = e;
    }

    public bool IsProfile
    {
        get { return isProfile; }
        set
        {
            if (value)
            {
                UtilPngTex2d = new Texture2D(_savePicSize.x, _savePicSize.y, TextureFormat.ARGB32, false);
                _profilerDataEndRequest = new ProfilerDataEndRequest(MaxRecordNum, _savePicSize);
                _profilerDataList = new NeedSendToServerData<ProfilerData>(CacheProfilerDataNum);
            }
            else
            {
                Destroy(UtilPngTex2d);
                _profileDataFullEvent = null;
                _profilerDataEndRequest.Release();
            }

            _nowTick = 0;
            LastSceneId = 0;
            EndProflieSceneData();
            isProfile = value;
        }
    }


    private void AddProfiler(String statName, ProfilerCategory pc, ProfilerMarkerDataUnit unit)
    {
        if (_profilterDict == null)
        {
            _profilterDict = new Dictionary<String, StatInfo>();
        }

        if (!_profilterDict.ContainsKey(statName))
        {
            StatInfo info = new StatInfo();
            info.Cat = pc;
            info.Name = statName;
            info.Unit = unit;
            info.Recorder = default;
            _profilterDict.Add(statName, info);
        }
    }

    private void Awake()
    {
        LastSceneId = 0;
        isProfile = false;
        _profilterDict = new Dictionary<String, StatInfo>();

        AddProfiler(ProfilerProperties.SystemUsedMemory, ProfilerCategory.Memory, ProfilerMarkerDataUnit.Bytes);
        AddProfiler(ProfilerProperties.TotalReservedMemory, ProfilerCategory.Memory, ProfilerMarkerDataUnit.Bytes);
        AddProfiler(ProfilerProperties.TotalUsedMemory, ProfilerCategory.Memory, ProfilerMarkerDataUnit.Bytes);
        AddProfiler(ProfilerProperties.GCReservedMemory, ProfilerCategory.Memory, ProfilerMarkerDataUnit.Bytes);
        AddProfiler(ProfilerProperties.GCUsedMemory, ProfilerCategory.Memory, ProfilerMarkerDataUnit.Bytes);

        AddProfiler(ProfilerProperties.DrawCallsCount, ProfilerCategory.Render, ProfilerMarkerDataUnit.Count);
        AddProfiler(ProfilerProperties.SetPassCount, ProfilerCategory.Render, ProfilerMarkerDataUnit.Count);
        AddProfiler(ProfilerProperties.TotalBatchCount, ProfilerCategory.Render, ProfilerMarkerDataUnit.Count);
        AddProfiler(ProfilerProperties.TriangleCount, ProfilerCategory.Render, ProfilerMarkerDataUnit.Count);
        AddProfiler(ProfilerProperties.VertexCount, ProfilerCategory.Render, ProfilerMarkerDataUnit.Count);
        AddProfiler(ProfilerProperties.MainThread, ProfilerCategory.Internal, ProfilerMarkerDataUnit.TimeNanoseconds);

        DontDestroyOnLoad(gameObject);
    }


    public ProfilerData GetNowProfilerData(uint nowTick)
    {
        ProfilerData data = new ProfilerData();
        data.TickId = nowTick;
        data.SceneDataId = nowSceneDataId;
        data.Time = DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");
        data.TriangleCount = _profilterDict[ProfilerProperties.TriangleCount].Recorder.LastValue;
        data.VertexCount = _profilterDict[ProfilerProperties.VertexCount].Recorder.LastValue;
        data.DrawCallCount = _profilterDict[ProfilerProperties.DrawCallsCount].Recorder.LastValue;
        data.SetPassCount = _profilterDict[ProfilerProperties.SetPassCount].Recorder.LastValue;
        data.TotalBatchCount = _profilterDict[ProfilerProperties.TotalBatchCount].Recorder.LastValue;
        data.FrameTime = _profilterDict[ProfilerProperties.MainThread].Recorder.LastValue * 1e-6f;
        data.GCUsedMemory = _profilterDict[ProfilerProperties.GCUsedMemory].Recorder.LastValue;
        data.TotalUseMemory = _profilterDict[ProfilerProperties.TotalUsedMemory].Recorder.LastValue;
        data.TotalReservedMemory = _profilterDict[ProfilerProperties.TotalReservedMemory].Recorder.LastValue;
        data.SystemUsedMemory = _profilterDict[ProfilerProperties.SystemUsedMemory].Recorder.LastValue;
        data.GCReservedMemory = _profilterDict[ProfilerProperties.GCReservedMemory].Recorder.LastValue;

        return data;
    }


    private IEnumerator SetMessageToServiceWithTex2D(ProfilerDataEndRequest request, String path)
    {
        var url = ServiceUrl + path;
        var json = JsonUtility.ToJson(request);
        WWWForm form = new WWWForm();
        form.AddField("data", json);
        for (int i = 0; i < request.SlowestProfiles.Count; i++)
        {
            if (request.SlowestProfiles[i] == null || request.SlowestProfiles[i].SlowestTex == null)
            {
                continue;
            }

            string id = "slow_" + DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");
            RenderTexture.active = request.SlowestProfiles[i].SlowestTex;
            UtilPngTex2d.ReadPixels(new Rect(0, 0, _savePicSize.x, _savePicSize.y), 0, 0);
            UtilPngTex2d.Apply();
            var bytes = UtilPngTex2d.EncodeToPNG();
            form.AddBinaryData($"slow_{i}", bytes, $"{id}.jpg");
        }

        ;
        for (int i = 0; i < request.MaxMemoryProfiles.Count; i++)
        {
            if (request.MaxMemoryProfiles[i] == null || request.MaxMemoryProfiles[i].SlowestTex == null)
            {
                continue;
            }

            string id = "max_memory_" + DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");
            RenderTexture.active = request.MaxMemoryProfiles[i].SlowestTex;
            UtilPngTex2d.ReadPixels(new Rect(0, 0, _savePicSize.x, _savePicSize.y), 0, 0);
            UtilPngTex2d.Apply();
            var bytes = UtilPngTex2d.EncodeToPNG();
            form.AddBinaryData($"max_memory_{i}", bytes, $"{id}.jpg");
        }

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log("上传失败:" + www.error);
            }
        }
    }

    private IEnumerator SetMessageToService(Object obj, String path, Action<UnityWebRequest> success = null,
        Action<UnityWebRequest> fail = null)
    {
        var url = ServiceUrl + path;
        var json = JsonUtility.ToJson(obj);
        byte[] postData = Encoding.UTF8.GetBytes(json);
        using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm(url, UnityWebRequest.kHttpVerbPOST))
        {
            webRequest.chunkedTransfer = false;
            webRequest.uploadHandler = new UploadHandlerRaw(postData);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Accept", "application/json");
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                if (success != null)
                {
                    success(webRequest);
                }
            }
            else
            {
                if (fail != null)
                {
                    fail(webRequest);
                }
            }
        }
    }

    private void SendEndSceneDataToService(uint id)
    {
        _profilerDataEndRequest.SceneDataId = id;
        StartCoroutine(SetMessageToServiceWithTex2D(_profilerDataEndRequest, "/unity_data/end"));
    }

    private void SendStartSceneDataToService()
    {
        var id = CFGame.singleton == null ? 0 : CFGame.singleton.PlayerID;

        UserData userData = new UserData()
        {
            UserId = id.ToString(),
            MobilePhone = SystemInfo.deviceName,
            AllMemory = SystemInfo.systemMemorySize,
        };
        var Scene = iXScene == null ? 0 : iXScene.GetSceneID();
        var scene = XSceneMgr.singleton?.GetMapDataBySceneID(Scene);
        var sceneName = scene == null ? "unknown" : scene.Comment;

        SceneData sceneData = new SceneData()
        {
            SceneId = Scene,
            SceneName = sceneName,
            SceneEntryTime = DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss")
        };
        SceneRecordDataRequest request = new SceneRecordDataRequest()
        {
            UserData = userData,
            SceneData = sceneData
        };
        StartCoroutine(SetMessageToService(request, "/unity_data/start",
            (req) =>
            {
                var str = req.downloadHandler.text;
                var data = JsonUtility.FromJson<StartProfilerResp>(str);
                if (data == null)
                {
                    Debug.LogWarning("解析json失败");
                    return;
                }

                if (data.Code != 0)
                {
                    Debug.LogWarning("获取开始数据失败:" + data.Msg);
                    return;
                }

                nowSceneDataId = data.SceneDataId;
            },
            (req) => { Debug.LogWarning("获取开始数据失败:" + req.error); }));
    }

    private void CompareDataWithNowSlowestAndMaxMemory(ref ProfilerData data)
    {
        var minframeTime = _profilerDataEndRequest.SlowestProfiles[0].data.FrameTime;
        var minFrameTimeIndex = 0;
        for (int i = 0; i < _profilerDataEndRequest.SlowestProfiles.Count; i++)
        {
            if (_profilerDataEndRequest.SlowestProfiles[i].data.TickId == 0)
            {
                minframeTime = _profilerDataEndRequest.SlowestProfiles[i].data.FrameTime;
                minFrameTimeIndex = i;
                break;
            }

            if (minframeTime > _profilerDataEndRequest.SlowestProfiles[i].data.FrameTime)
            {
                minframeTime = _profilerDataEndRequest.SlowestProfiles[i].data.FrameTime;
                minFrameTimeIndex = i;
            }
        }

        if (minframeTime < data.FrameTime ||
            _profilerDataEndRequest.SlowestProfiles[minFrameTimeIndex].data.TickId == 0)
        {
            ScreenCapture.CaptureScreenshotIntoRenderTexture(_profilerDataEndRequest.SlowestProfiles[minFrameTimeIndex]
                .SlowestTex);
            _profilerDataEndRequest.SlowestProfiles[minFrameTimeIndex].data = data;
        }

        long minMemoryNum = _profilerDataEndRequest.MaxMemoryProfiles[0].data.TotalUseMemory;
        var minMemoryIndex = 0;
        for (int i = 0; i < _profilerDataEndRequest.MaxMemoryProfiles.Count; i++)
        {
            if (_profilerDataEndRequest.MaxMemoryProfiles[i].data.TickId == 0)
            {
                minMemoryNum = _profilerDataEndRequest.MaxMemoryProfiles[i].data.TotalUseMemory;
                minMemoryIndex = i;
                break;
            }

            if (minMemoryNum > _profilerDataEndRequest.MaxMemoryProfiles[i].data.TotalUseMemory)
            {
                minMemoryNum = _profilerDataEndRequest.MaxMemoryProfiles[i].data.TotalUseMemory;
                minMemoryIndex = i;
            }
        }

        if (minMemoryNum < data.TotalUseMemory ||
            _profilerDataEndRequest.MaxMemoryProfiles[minMemoryIndex].data.TickId == 0)
        {
            ScreenCapture.CaptureScreenshotIntoRenderTexture(_profilerDataEndRequest.MaxMemoryProfiles[minMemoryIndex]
                .SlowestTex);
            _profilerDataEndRequest.MaxMemoryProfiles[minMemoryIndex].data = data;
        }
    }

    private void StartProflieSceneData()
    {
        _nowTick = 0;

        foreach (var profile in _profilterDict)
        {
            var value = profile.Value;
            value.Recorder = ProfilerRecorder.StartNew(value.Cat, value.Name);
        }
    }

    private void EndProflieSceneData()
    {
        foreach (var profile in _profilterDict)
        {
            var value = profile.Value;
            value.Recorder.Dispose();
        }
    }


    public ProfilerData[] GetProfilerDataList()
    {
        return _profilerDataList.GetDatas();
    }

    private void Update()
    {
        if (Input.touchCount == 7)
        {
            IsProfile = !IsProfile;
            Debug.Log($"Profile Open :{IsProfile}");
        }

        if (!isProfile)
            return;

        if (iXScene == null && XInterfaceMgr.singleton != null)
        {
            iXScene = XInterfaceMgr.singleton.GetInterface<IXScene>(XInterfaceMgr.XSceneID);
        }

        if (iXScene == null)
        {
            return;
        }

        // 切换了场景
        if (LastSceneId != iXScene.GetSceneID() && LastSceneId != 0)
        {
            EndProflieSceneData();
            var cacheId = nowSceneDataId;
            nowSceneDataId = 0;
            StartCoroutine(SetMessageToService(_profilerDataList, "/unity_data/tick",
                (_) =>
                {
                    SendEndSceneDataToService(cacheId);
                    _profilerDataEndRequest.Clear();
                }));
        }

        // 设置新场景
        if (LastSceneId != iXScene.GetSceneID())
        {
            StartProflieSceneData();
            //在这个函数里面进行 nowSceneDataId 赋值
            SendStartSceneDataToService();
            LastSceneId = iXScene.GetSceneID();
        }

        if (!iXScene.GetSceneReady())
        {
            return;
        }

        _nowTick++;
        if (_nowTick % TickClacNum != 0)
            return;

        var data = GetNowProfilerData(_nowTick);
        // 保存信息
        CompareDataWithNowSlowestAndMaxMemory(ref data);
        if (_profilerDataList.IsFull())
        {
            if (nowSceneDataId != 0)
            {
                StartCoroutine(SetMessageToService(_profilerDataList, "/unity_data/tick"));
            }
            _profileDataFullEvent?.Invoke();
            _profilerDataList.Clear();
        }

        _profilerDataList.AddToList(data);
    }


    private static GUIStyle _style = new GUIStyle();


    void OnGUI()
    {
        _style.fontSize = 15;
        _style.normal.textColor = Color.green;
        if (IsProfile)
        {
            GUI.Label(new Rect(30, 30, 100, 100), "ProfilerToServer:on", _style);
        }
    }
}

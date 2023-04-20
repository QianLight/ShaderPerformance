using Devops.Core;
using PENet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Devops.Performance
{
    public class DevopsProfilerClient : MonoBehaviour
    {
        static DevopsProfilerClient instance;

        public static bool HasInstance()
        {
            return instance != null;
        }
        public static DevopsProfilerClient Instance()
        {
            if (instance == null)
            {
                GameObject devopsProfiler = new GameObject();
                devopsProfiler.name = "DevopsProfilerClient";
                DontDestroyOnLoad(devopsProfiler);
                devopsProfiler.AddComponent<DevopsProfilerClient>();
            }
            return instance;
        }
        PESocket<ClientSession> mPEClient;
        ClientSystemInfo mClientSystemInfo;
        ScreenShotTextureInfo mScreenShotTextureInfo = new ScreenShotTextureInfo();
        TagFrameInfo mTagFrameInfo = new TagFrameInfo();
        private bool NeedCheckProfilerState = false;
        float mHeartBeatTimer = 0;
        HeartBeatInfo mHeartBeatInfo = new HeartBeatInfo();
        string mRemarkInfo = string.Empty;
        MemoryInfo memoryinfo = new MemoryInfo();
        public Action<bool> EventServerConnected;
        public Action<bool> EventSnapScreening;
        bool globleDisable = false;
        string mReportId = null;
        string mScreenshotPath = string.Empty;

        NetMsgSender MemorySnapshotSender;
        NetMsgSender ObjectMemorySnapshotSender;
#if UNITY_ANDROID
        AndroidJavaClass capUtils;
        AndroidJavaClass systemInfo;
#endif
        private int CaptureInterval = 300;
        private int FrameIndex = 0;
        private int ProfilerStartFrame = 0;
        private void Awake()
        {
            instance = this;
            MemorySnapshotSender = new NetMsgSender((int)EMessageType.MemorySnapshot);
            MemorySnapshotSender.WriteEnd();
            ObjectMemorySnapshotSender = new NetMsgSender((int)EMessageType.ObjectMemorySnapshot);
            ObjectMemorySnapshotSender.WriteEnd();
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                DevopsScreenshot.Instance();
                capUtils = new AndroidJavaClass("com.pwrd.upsdk.CaptureUtils");
                systemInfo = new AndroidJavaClass("com.pwrd.upsdk.SystemInfo");
            }
#endif
        }

        public string GetReportId()
        {
            return mReportId;
        }

        private void Update()
        {
            if (NeedCheckProfilerState)
            {
                if (UnityEngine.Profiling.Profiler.enabled)
                {
                    ProfilerStartFrame = Time.frameCount;
                    NeedCheckProfilerState = false;
                }
            }
            if (mPEClient != null && mPEClient.session != null)
            {
                mPEClient.session.OnUpdate();
                mHeartBeatTimer += Time.unscaledDeltaTime;
                if (mHeartBeatTimer >= 5.0f)
                {
                    mHeartBeatTimer = 0.0f;
                    mHeartBeatInfo.remark = mRemarkInfo;
                    NetMsgSender sender = mHeartBeatInfo.GetMsgSender();
                    mPEClient.session.SendMsg(sender.buffer, sender.GetBufferSize());
                }
            }
            UpdateScreenShot();
        }

        private int GetProfilerFrame()
        {
            if (ProfilerStartFrame == 0)
                return 0;
            return Time.frameCount - ProfilerStartFrame;
        }


        public string GetUnityVersion()
        {
#if UNITY_2018_1
            return "2018.1.x";
#elif UNITY_2018_2
            return "2018.2.x";
#elif UNITY_2018_3
            return "2018.3.x";
#elif UNITY_2018_4
            return "2018.4.x";
#elif UNITY_2019_1
            return "2019.1.x";
#elif UNITY_2019_2
            return "2019.2.x";
#elif UNITY_2019_3
            return "2019.3.x";
#elif UNITY_2019_4
            return "2019.4.x";
#elif UNITY_2020_1
            return "2020.1.x";
#elif UNITY_2020_2
            return "2020.2.x";
#elif UNITY_2020_3
            return "2020.3.x";
#elif UNITY_2021_1
            return "2021.1.x";
#elif UNITY_2021_2
            return "2021.2.x";
#else
            return "2021.3.x";   
#endif
        }

        public void GlobleDisable()
        {
            globleDisable = true;
            if (IsOpen())
            {
                StopConnectServer();
            }
        }

        // web上的手动自动标签
        // web上的tags
        public async Task<bool> ConnectServer(bool bAuto, string runid = "", string deviceId = "", string gameTestPlan = "",  string[] profilerTags = null)
        {
            if(!Debug.isDebugBuild) 
            {
                Debug.LogError("ConnectServer return not Development Build");
                return false;
            }
            if (globleDisable)
            {
                Debug.LogError("ConnectServer return globleDisable");
                return false;
            }
            if (UnityEngine.Profiling.Profiler.enabled)
            {
                Debug.LogError("ConnectServer return profiler has enabled");
                return false;
            }
            ClientSession.OnServerConnected += OnServerConnected;
            ClientSession.OnServerDisconnected += OnServerDisconnect;
            mPEClient = new PESocket<ClientSession>();
            PerformanceInfo performanceInfo = await DevopsInfoSettings.Instance().GetPerformanceInfo();
            if (performanceInfo == null)
            {
                Debug.LogError("Can not get performanceInfo from core");
                return false;
            }
            mReportId = await DevopsInfoSettings.Instance().GetReportId();
            if(string.IsNullOrEmpty(mReportId))
            {
                Debug.LogError("Can not get reportId");
                return false;
            }
            string serverIp = performanceInfo.ServerIp;
            mClientSystemInfo = new ClientSystemInfo()
            {
                //msgType = EMessageType.Connect,
                deviceName = SystemInfo.deviceName,
                deviceModel = SystemInfo.deviceModel,
                graphicsDeviceID = SystemInfo.graphicsDeviceID.ToString(),
                graphicsDeviceName = SystemInfo.graphicsDeviceName,
                graphicsDeviceType = SystemInfo.graphicsDeviceType.ToString(),
                graphicsDeviceVendor = SystemInfo.graphicsDeviceVendor,
                graphicsDeviceVendorID = SystemInfo.graphicsDeviceVendorID.ToString(),
                operatingSystem = GetOperatingSystem(),
                unityVersion = GetUnityVersion(),
                remarkInfo = mRemarkInfo,
                reportWebAddress = performanceInfo.WebIpPort,
                buildTimestamp = performanceInfo.BuildTimestamp,
                versionId = performanceInfo.VersionId,
                platform = Application.platform.ToString(),
#if UNITY_ANDROID
                processorType = (Application.platform == RuntimePlatform.Android) ? systemInfo.CallStatic<string>("GetCpuName") : SystemInfo.processorType,
#else
                processorType = SystemInfo.processorType,
#endif
                screenResolution = Screen.width.ToString() + " x " + Screen.height.ToString(),
                tags = profilerTags,
                auto = bAuto,
                packageVersion = DevopsCoreDefine.PackageVersion,
                buildRunId = runid,
                deviceUuid = deviceId,
                reportId = mReportId,
                testPlan = gameTestPlan,
            };
            mScreenshotPath = $"{performanceInfo.Id}/reportId-{mReportId}/";
            EventServerConnected?.Invoke(true);
            StartCoroutine("TryConnectServer", serverIp);
            return true;
        }

        private string GetOperatingSystem()
        {
            string[] operatingSystemInfos = SystemInfo.operatingSystem.Split('/');
            return operatingSystemInfos[0];
        }

        IEnumerator TryConnectServer(string serverIp)
        {
            yield return null;
            NeedCheckProfilerState = true;
            Debug.Log($"Devops TryConnectServer:{serverIp}");
            mPEClient.StartAsClient(serverIp, 17666, OnTryConnectFailed);
        }

        void OnTryConnectFailed()
        {
            StopConnectServer();
        }

        void OnServerConnected()
        {
            EventServerConnected?.Invoke(true);
            StartCoroutine("_SendSystemInfo");
            StartCoroutine("_SendTag");
        }

        void OnServerDisconnect()
        {
            StopConnectServer();
        }

        IEnumerator _SendSystemInfo()
        {
            yield return null;
            NetMsgSender sender = mClientSystemInfo.GetMsgSender();
            mPEClient.session.SendMsg(sender.buffer, sender.GetBufferSize());
        }

        IEnumerator _SendTag()
        {
            yield return null;
            if (!string.IsNullOrEmpty(mTagFrameInfo.tag))
            {
                NetMsgSender sender = mTagFrameInfo.GetMsgSender();
                mPEClient.session.SendMsg(sender.buffer, sender.GetBufferSize());
            }
        }

        public void StopConnectServer()
        {
            if (mPEClient == null)
                return;
            ClientSession.OnServerConnected -= OnServerConnected;
            ClientSession.OnServerDisconnected -= OnServerDisconnect;
            mPEClient.Close();
            mPEClient = null;
            NeedCheckProfilerState = true;
            ProfilerStartFrame = 0;
            EventServerConnected?.Invoke(false);
        }

        private void OnDestroy()
        {
            StopConnectServer();
        }

        public bool IsOpen()
        {
            return mPEClient != null && mPEClient.session != null && UnityEngine.Profiling.Profiler.enabled;
        }

        public bool IsConnect()
        {
            return mPEClient != null;
        }

        public void ScreenShotNative()
        {

#if UNITY_ANDROID
            DevopsScreenshot.Instance().GetScreenShot(false, mScreenshotPath, null);
#endif

        }

        public void ObjectReferences()
        {
            if (mPEClient == null || mPEClient.session == null)
                return;
            string textureName = DevopsScreenshot.Instance().GetScreenShot(true, mScreenshotPath, OnScreenShotEnd);
            mScreenShotTextureInfo.frame = GetProfilerFrame();
            mScreenShotTextureInfo.isAuto = false;
            mScreenShotTextureInfo.reason = eScreenShotReason.ObjectSnapShot;
            mScreenShotTextureInfo.textureName = mScreenshotPath + textureName;
            NetMsgSender sender = mScreenShotTextureInfo.GetMsgSender();
            mPEClient.session.SendMsg(sender.buffer, sender.GetBufferSize());
            mPEClient.session.SendMsg(ObjectMemorySnapshotSender.buffer, ObjectMemorySnapshotSender.GetBufferSize());
        }

        void OnScreenShotEnd(ScreenshotInfo info)
        {
            EventSnapScreening?.Invoke(false);
        }

        public void ProfilerScreenShot(bool bAuto)
        {
            if (mPEClient == null || mPEClient.session == null)
                return;
            string textureName = DevopsScreenshot.Instance().GetScreenShot(true, mScreenshotPath, OnScreenShotEnd);
            mScreenShotTextureInfo.frame = GetProfilerFrame();
            mScreenShotTextureInfo.isAuto = bAuto;
            mScreenShotTextureInfo.reason = eScreenShotReason.None;
            mScreenShotTextureInfo.textureName = mScreenshotPath + textureName;
            NetMsgSender sender = mScreenShotTextureInfo.GetMsgSender();
            mPEClient.session.SendMsg(sender.buffer, sender.GetBufferSize());
        }

        void ReceiveMemoryMethod(string memoryData)
        {
            string[] memoryDatas = memoryData.Split(',');
            memoryinfo.vss = int.Parse(memoryDatas[0]);
            memoryinfo.uss = int.Parse(memoryDatas[2]);
            memoryinfo.rss = int.Parse(memoryDatas[3]);
            memoryinfo.pss = int.Parse(memoryDatas[1]);
            NetMsgSender memorysender = memoryinfo.GetMsgSender();
            mPEClient.session.SendMsg(memorysender.buffer, memorysender.GetBufferSize());
        }

        public void SetTagData(string tag)
        {
            mTagFrameInfo.tag = tag;
            mTagFrameInfo.frame = GetProfilerFrame();
            if(mPEClient != null && mPEClient.session != null)
            {
                NetMsgSender sender = mTagFrameInfo.GetMsgSender();
                mPEClient.session.SendMsg(sender.buffer, sender.GetBufferSize());
            }
        }
        void UpdateScreenShot()
        {
            if (!IsOpen())
                return;
            FrameIndex++;

            if (FrameIndex >= CaptureInterval)
            {
                FrameIndex = 0;
                ProfilerScreenShot(true);
            }
        }

        public void SetRemark(string info)
        {
            mRemarkInfo = info;
        }

        public int SetInterval(int interval)
        {
            CaptureInterval = Math.Max(interval, 30);
            return CaptureInterval;
        }

        public int GetInterval()
        {
            return CaptureInterval;
        }
    }
}
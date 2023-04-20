using Devops.Performance;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Devops.Performance
{
    public class PerformancePanel : MonoBehaviour
    {
        static PerformancePanel instance;
        public Button CloseBtn;
        public InputField FrameInterval;
        public Toggle ObjectSnapScreen;
        public InputField Remark;
        public Toggle AddUI;
        public Button BtnProfiler;
        public Text TextProfiler;
        public Button SnapScreen;
        public GameObject ProfilerBeginImage;
        public GameObject ProfilerEndImage;
        // Start is called before the first frame update
        bool bNeedRefreshUI = false;

        [RuntimeInitializeOnLoadMethod]
        public static void RegisterToCore()
        {
            Devops.Core.DevopsUI.RegisterFunction(StaticDisplaySwitch, 1, "性能测试");
        }

        public static void StaticDisplaySwitch()
        {
            PerformancePanel.Instance().SetEnable(true);
        }

        public static PerformancePanel Instance()
        {
            if(instance == null)
            {
                Object obj = Resources.Load("PerformancePanel");
                GameObject gObject = GameObject.Instantiate(obj, Devops.Core.DevopsUI.Instance().PanelParent) as GameObject;
            }
            return instance;
        }

        private void Awake()
        {
            instance = this;
        }
        void Start()
        {
            CloseBtn.onClick.AddListener(() =>
            {
                SetEnable(false);
            });
            FrameInterval.text = DevopsProfilerClient.Instance().GetInterval().ToString();
            FrameInterval.onEndEdit.AddListener((string str) =>
            {
                if(int.TryParse(str, out int result))
                {
                    int interval = DevopsProfilerClient.Instance().SetInterval(result);
                    if(interval != result)
                    {
                        FrameInterval.text = interval.ToString();
                    }
                }
                else
                {
                    FrameInterval.text = DevopsProfilerClient.Instance().GetInterval().ToString();
                }
            });
            //ObjectSnapScreen.isOn = DevopsProfilerClient.Instance().IsObjectSnap();
            //ObjectSnapScreen.onValueChanged.AddListener((bool b) => {
            //    DevopsProfilerClient.Instance().SetObjectSnap(b);
            //});
            Remark.onEndEdit.AddListener((string str) => {
                DevopsProfilerClient.Instance().SetRemark(str);
            });
            //AddUI.onValueChanged.AddListener((bool b) => {
            //    if(SimplePerformancePanel.Instance() == null)
            //    {
            //        SimplePerformancePanel.Instantiation();
            //    }
            //    SimplePerformancePanel.Instance().SetEnable(b);
            //});
            BtnProfiler.onClick.AddListener(()=>{
                if(DevopsProfilerClient.Instance().IsConnect())
                {
                    DevopsProfilerClient.Instance().StopConnectServer();
                }
                else
                {
                    DevopsProfilerClient.Instance().ConnectServer(false);
                }
                RefreshState();
            });
            SnapScreen.onClick.AddListener(()=> {
                DevopsProfilerClient.Instance().ProfilerScreenShot(false);
            });
        }

        private void OnEnable()
        {
            RefreshState();

            if (SimplePerformancePanel.Instance() != null)
            {
                AddUI.isOn = SimplePerformancePanel.Instance().IsEnable();
            }
            else
            {
                AddUI.isOn = false;
            }
            //DevopsProfilerClient.Instance().EventTryConnect += OnProfilerTryConnectState;
            DevopsProfilerClient.Instance().EventServerConnected += OnProfilerConnectState;
            DevopsProfilerClient.Instance().EventSnapScreening += OnProfilerSnapScreen;
        }
        void OnDisable()
        {
            if (!DevopsProfilerClient.HasInstance())
                return;
            //DevopsProfilerClient.Instance().EventTryConnect -= OnProfilerTryConnectState;
            DevopsProfilerClient.Instance().EventServerConnected -= OnProfilerConnectState;
            DevopsProfilerClient.Instance().EventSnapScreening -= OnProfilerSnapScreen;
        }

        private void Update()
        {
            if (bNeedRefreshUI)
            {
                bNeedRefreshUI = false;
                RefreshState();
            }
        }

        //void OnProfilerTryConnectState(bool bTry)
        //{
        //    RefreshState();
        //}
        void OnProfilerConnectState(bool connect)
        {
            bNeedRefreshUI = true;
        }

        void OnProfilerSnapScreen(bool snapScreen)
        {
            SnapScreen.interactable = !snapScreen;
        }

        void RefreshState()
        {
            bool bProfilerOpen = DevopsProfilerClient.Instance().IsConnect();
            TextProfiler.text = bProfilerOpen ? "结束" : "开始";
            if(bProfilerOpen)
            {
                ProfilerBeginImage.SetActive(false);
                ProfilerEndImage.SetActive(true);
            }
            else
            {
                ProfilerBeginImage.SetActive(true);
                ProfilerEndImage.SetActive(false);
            }
            SnapScreen.interactable = true;
        }

        public void SetEnable(bool enable)
        {
            gameObject.SetActive(enable);
            if(!enable)
            {
                Core.EntrancePanel.Instance().SetEnable(true);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Devops.Performance
{
    public class SimplePerformancePanel : MonoBehaviour
    {
        static SimplePerformancePanel instance;

        public static SimplePerformancePanel Instance()
        {
            if (instance == null)
            {
                Object obj = Resources.Load("SimplePerformancePanel");
                GameObject gObject = GameObject.Instantiate(obj, Devops.Core.DevopsUI.Instance().PanelParent) as GameObject;
            }
            return instance;
        }

        public Button ProfilerBegin;
        public Button ProfilerEnd;
        public Button SnapScreen;
        public Button ObjectReferences;

        private bool dirty = false;

        [RuntimeInitializeOnLoadMethod]
        public static void RegisterToCore()
        {
            Devops.Core.DevopsUI.RegisterFunction(StaticDisplaySwitch, 1, "性能测试");
        }

        public static void StaticDisplaySwitch()
        {
            SimplePerformancePanel.Instance().SetEnable(true);
        }

        private void Awake()
        {
            instance = this;
        }
        // Start is called before the first frame update
        void Start()
        {
            transform.SetAsFirstSibling();
            RefreshState();
            ProfilerBegin.onClick.AddListener(() =>
            {
                DevopsProfilerClient.Instance().ConnectServer(false);
                RefreshState();
            });
            ProfilerEnd.onClick.AddListener(()=> 
            {
                DevopsProfilerClient.Instance().StopConnectServer();
                RefreshState();
            });
            SnapScreen.onClick.AddListener(()=> 
            {
                DevopsProfilerClient.Instance().ProfilerScreenShot(false);
            });
            ObjectReferences.onClick.AddListener(() =>
            {
                DevopsProfilerClient.Instance().ObjectReferences();
            });
            SnapScreen.interactable = true;
        }

        private void Update()
        {
            if(dirty)
            {
                RefreshState();
            }
        }

        void RefreshState()
        {
            bool bProfilerOpen = DevopsProfilerClient.Instance().IsConnect();
            ProfilerBegin.gameObject.SetActive(!bProfilerOpen);
            ProfilerEnd.gameObject.SetActive(bProfilerOpen);
            SnapScreen.gameObject.SetActive(bProfilerOpen);
            ObjectReferences.gameObject.SetActive(bProfilerOpen);
            SnapScreen.interactable = true;

            dirty = false;
        }

        public void SetEnable(bool enable)
        {
            gameObject.SetActive(enable);
        }

        public bool IsEnable()
        {
            return gameObject.activeSelf;
        }

        void OnEnable()
        {
            DevopsProfilerClient.Instance().EventServerConnected += OnProfilerConnectState;
            DevopsProfilerClient.Instance().EventSnapScreening += OnProfilerSnapScreen;
            RefreshState();
        }

        void OnDisable()
        {
            if (!DevopsProfilerClient.HasInstance())
                return;
            DevopsProfilerClient.Instance().EventServerConnected -= OnProfilerConnectState;
            DevopsProfilerClient.Instance().EventSnapScreening -= OnProfilerSnapScreen;
        }

        void OnProfilerConnectState(bool connect)
        {
            dirty = true;
        }

        void OnProfilerSnapScreen(bool snapScreen)
        {
            SnapScreen.interactable = !snapScreen;
        }
    }
}
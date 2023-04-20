#if ENABLE_UPO && ENABLE_UPO_OVERDRAW
using System;
using System.Linq;
using UnityEngine;

public class MyOverdraw : MonoBehaviour
{
    private static MyOverdraw instance;
    
    public static bool Enabled = true;//在UPO设置中打开为true
    public static bool Reset = true;
    
    public static bool NotSupportedPlatform = false;
    
    public static MyOverdraw Instance
    {
        get {
            if (instance == null) {
                instance = FindObjectOfType<MyOverdraw>();
                if (instance == null) {
                    if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat) || !SystemInfo.SupportsTextureFormat(TextureFormat.RGBAFloat)
                    || !SystemInfo.supportsComputeShaders) {
                        NotSupportedPlatform = true;
                        Debug.Log("SystemInfoNotSupported");
                    }
                    
                    var monitorManager = new GameObject("OverdrawMonitor");
                    monitorManager.hideFlags = HideFlags.DontSave | HideFlags.HideInInspector;
                    DontDestroyOnLoad(monitorManager);
                    instance = monitorManager.AddComponent<MyOverdraw>();
                    instance._monitorManager = monitorManager;
                    
                    
                }
            }
            return instance;
        }
    }
    
    private GameObject _monitorManager;
    // Update is called once per frame
    void Update()
    {
        if (!Enabled) {
            if (!Reset) {
                ResetMonitors();
                Reset = true;
            }
            return;
        }
        if (NotSupportedPlatform) {
            return;
        }
        
        Camera[] allCam = Camera.allCameras;
        
        var monitors = GetAllMonitors();
        OverdrawCameraMonitorWithoutCS[] monitorToBeDestroyed = {};
        foreach (var monitor in monitors) {
            if (!Array.Exists(allCam, c => monitor._targetCam == c))
            {
                monitorToBeDestroyed.Append(monitor);
                //Destroy(monitor);
            }
        }

        if (monitorToBeDestroyed.Length != 0)
        {
            foreach (var monitor in monitorToBeDestroyed)
            {
                Destroy(monitor);
            }
        }

        monitors = GetAllMonitors();
        foreach (Camera cam in allCam) {
            if (!Array.Exists(monitors, m => m._targetCam == cam)) {
                var monitor = _monitorManager.AddComponent<OverdrawCameraMonitorWithoutCS>();
                monitor.SetTargetCamera(cam);
            }
        }
    }
    
    
    OverdrawCameraMonitorWithoutCS[] GetAllMonitors() {
        return
            _monitorManager.GetComponentsInChildren<OverdrawCameraMonitorWithoutCS>(true);
    }
    
    void ResetMonitors() {
        var monitors = GetAllMonitors();
        foreach (var monitor in monitors) {
            DestroyImmediate(monitor);
        }
    }
}
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMgr : MonoBehaviour
{
    public Canvas Root;
    private static Canvas rootCanvas;
    private void Start()
    {
        rootCanvas = Root;
    }
    public void SetCancasCamera()
    {
        setCancasCamera(transform);
    }
    void setCancasCamera(Transform root)
    {
        foreach(Transform t in root)
        {
            setCancasCamera(t);
        }
        Canvas can = root.gameObject.GetComponent<Canvas>();
        if (can != null && currentCamera != null)
        {
            can.worldCamera = currentCamera;
            if(System.Object.ReferenceEquals(root, transform))
            {
                Root.planeDistance = currentCamera.nearClipPlane + 0.001f;
            }
            else
            {
                root.localRotation = Quaternion.identity;
                Debug.LogError(root.gameObject.name);
            }
        }
    }
    private static Camera currentCamera;
    public static void AddCamera(Camera cam)
    {
        //TO DO暂时需要处理Canvas的子Canvas的位置问题需要解决。
        return;
        if(rootCanvas != null)
        {
            rootCanvas.worldCamera = cam;
            cam.cullingMask |= 1 << 5;
            currentCamera = cam;
        }
    }
}

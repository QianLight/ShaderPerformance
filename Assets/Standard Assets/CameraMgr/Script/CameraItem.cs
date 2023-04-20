using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraItem : MonoBehaviour
{
    private void Start()
    {
        Camera cam = GetComponent<Camera>();
        if(cam != null)
        {
            CameraMgr.AddCamera(cam);
        }
    }
}

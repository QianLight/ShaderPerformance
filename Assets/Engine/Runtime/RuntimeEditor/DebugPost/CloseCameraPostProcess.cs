using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CloseCameraPostProcess : MonoBehaviour
{
    public Camera m_mainCamera;

    private UniversalAdditionalCameraData data;
    // Update is called once per frame
    public bool m_openPost = true;
    void Update()
    {
        if (m_mainCamera == null)
        {
            m_mainCamera = Camera.main;
        }
        else
        {
            if (data == null)
            {
                data = m_mainCamera.GetComponent<UniversalAdditionalCameraData>();
            }

            data.renderPostProcessing = m_openPost;
        }
    }
}

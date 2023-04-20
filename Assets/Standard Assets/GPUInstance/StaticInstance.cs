using System;
using System.Collections;
using System.Collections.Generic;
using CFEngine;
using UnityEngine;
using UnityEngine.Rendering;

public class StaticInstance : MonoBehaviour
{
    public float CullingDistance = 35;
    public int DelayFrame = 2;
    private bool DisableBack = true;
    public const InstanceType Type = InstanceType.Mid;
    private int frame = 0;
    public List<string> CullingShaderName = new List<string>();
    InstanceHandld instance = null;

    private static StaticInstance _instance;

    public static StaticInstance Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        _instance = this;
    }

    private Dictionary<Material, Material> m_RenderMats = new Dictionary<Material, Material>();

    public Material GetRenderMaterial(Material mat)
    {
        if (mat == null) return null;
        
        if (m_RenderMats.ContainsKey(mat)) return m_RenderMats[mat];
        Material newMat = new Material(mat);
        m_RenderMats.Add(mat, newMat);
        return newMat;
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    private void OnEnable()
    {
        instance = new InstanceHandld();
        frame = 0;
        drawState = true;
    }

    private Camera m_MainCamera;
    private void Update()
    {
        frame++;
        if (frame > DelayFrame)
        {
            frame = 5721;
            m_MainCamera = EngineUtility.GetMainCamera();
            
            if (instance.Update(m_MainCamera))
            {
                instance.SetInstance(transform, m_MainCamera, CullingShaderName, CullingDistance, Type);
            }
        }
    }
    
    private bool drawState = true;

    private void OnDisable()
    {
        instance.ClearInstance(DisableBack);
    }
}

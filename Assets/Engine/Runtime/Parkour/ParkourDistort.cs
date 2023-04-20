using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ParkourDistort : MonoBehaviour
{
    
    public enum BendAxis
    {
        XAxis,ZAxis
    }
    public bool m_isParkourEnabled = true;
    [Header("弯曲轴向")]
    public BendAxis m_bendAxis;
    [Header("偏移值")]
    public Vector3 m_offset = new Vector3(100,100,0);
    [Header("弯曲程度"), Range(-2, 2f)]
    public float m_strength = 0.0f;
    [Header("平坦距离")]
    public float m_flatDistance=15;
    int _strengthID, _flatDistanceID,_bendPivotID,_bendAmountID;
    private int _parkourID;
    [Header("排除扭曲")]
    public List<Transform> m_ignoreDistortObjects = new List<Transform>();

    void Awake()
    {
        _parkourID = Shader.PropertyToID("_IsParkour");
        Shader.SetGlobalFloat(_parkourID, 1);
        _strengthID =  Shader.PropertyToID("_ParkourStrength");
        _flatDistanceID = Shader.PropertyToID("_FlatDistance");
        _bendPivotID = Shader.PropertyToID("_BendPivot");
        _bendAmountID = Shader.PropertyToID("_BendAmount");
        //Shader.EnableKeyword("_PARKOUR");
        
        if (Application.isPlaying)
        {
            for (int i = 0; i < m_ignoreDistortObjects.Count; i++)
            {
                MeshRenderer[] mrs = m_ignoreDistortObjects[i].GetComponentsInChildren<MeshRenderer>();
                for (int j = 0; j < mrs.Length; j++)
                {
                    if(mrs[j].sharedMaterial.HasProperty("_Parkout"))
                        mrs[j].material.SetFloat("_Parkout", 0);
                }
            }
        }
    }

    [ContextMenu("EnableKey")]
    public void EnableKey()
    {
        _parkourID = Shader.PropertyToID("_IsParkour");
        Shader.SetGlobalFloat(_parkourID, 1);
        
        _strengthID = Shader.PropertyToID("_ParkourStrength");
        _flatDistanceID = Shader.PropertyToID("_FlatDistance");
        _bendPivotID = Shader.PropertyToID("_BendPivot");
        _bendAmountID = Shader.PropertyToID("_BendAmount");
        //Shader.EnableKeyword("_PARKOUR");
    }
    [ContextMenu("DisableKey")]
    public void DisableKey()
    {
        _parkourID = Shader.PropertyToID("_IsParkour");
        Shader.SetGlobalFloat(_parkourID, 0);
        
        _strengthID = Shader.PropertyToID("_ParkourStrength");
        _flatDistanceID = Shader.PropertyToID("_FlatDistance");
        
        _bendPivotID = Shader.PropertyToID("_BendPivot");
        _bendAmountID = Shader.PropertyToID("_BendAmount");
        //Shader.DisableKeyword("_PARKOUR");
    }

    // Update is called once per frame
    #if UNITY_EDITOR
    void Update()
    {
        Shader.SetGlobalFloat(_strengthID, m_strength/90.0f);
        Shader.SetGlobalFloat(_flatDistanceID, m_flatDistance);
        Shader.SetGlobalVector(_bendPivotID,new Vector4(m_offset.x,m_offset.y,m_offset.z,0));
        Shader.SetGlobalFloat(_parkourID, m_isParkourEnabled ? 1 : 0);
        //根据轴向往shader里传0或1，用来lerp两个不同的轴向弯曲
        switch (m_bendAxis)
        {
            case BendAxis.XAxis:
                Shader.SetGlobalFloat(_bendAmountID,0);
                break;
            case BendAxis.ZAxis:
                Shader.SetGlobalFloat(_bendAmountID,1);
                break;
        }
    }
    #else
    void OnEnable(){
        Shader.SetGlobalFloat(_strengthID, m_strength/90.0f);
        Shader.SetGlobalFloat(_flatDistanceID, m_flatDistance);
        Shader.SetGlobalVector(_bendPivotID,new Vector4(m_offset.x,m_offset.y,m_offset.z,0));
        Shader.SetGlobalFloat(_parkourID, 1);
        //根据轴向往shader里传0或1，用来lerp两个不同的轴向弯曲
        switch (m_bendAxis)
        {
            case BendAxis.XAxis:
                Shader.SetGlobalFloat(_bendAmountID,0);
                break;
            case BendAxis.ZAxis:
                Shader.SetGlobalFloat(_bendAmountID,1);
                break;
        }
    }
    #endif
    

    private void OnDisable()
    {
        Shader.SetGlobalFloat(_strengthID, 0);
        Shader.SetGlobalFloat(_flatDistanceID, 0);
        Shader.SetGlobalFloat(_parkourID, 0);

    }
}

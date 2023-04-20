using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshPosDeliver : MonoBehaviour
{
    public ParticleSystemRenderer psr;
    private MaterialPropertyBlock _mpb;
    private static readonly int ROOT_POS_WS = Shader.PropertyToID("_RootPosWS");

    void Reset()
    {
        psr = GetComponent<ParticleSystemRenderer>();
    }
    void Start()
    {
        if (psr != null)
        {
            _mpb = new MaterialPropertyBlock();
        }
        else
        {
            this.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        _mpb.SetVector(ROOT_POS_WS, transform.position);
        psr.SetPropertyBlock(_mpb);
    }
}

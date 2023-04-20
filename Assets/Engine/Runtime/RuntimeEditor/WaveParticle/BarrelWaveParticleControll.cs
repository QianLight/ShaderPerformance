using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelWaveParticleControll : WaveParticleControll
{
    public float m_height = 0.3f;
    private void Start()
    {
        base.Init();
    }
    private void Update()
    {
         this.transform.rotation = Quaternion.Euler(m_rotate.x, m_rotate.y, m_rotate.z);
        if(transform.position.y > m_height)
        {
            _particle.Stop();
        }
        else
        {
            _particle.Play(true);
        }
    }
}

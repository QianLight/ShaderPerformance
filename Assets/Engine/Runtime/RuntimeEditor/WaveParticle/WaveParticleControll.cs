using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveParticleControll : MonoBehaviour
{
    protected ParticleSystem _particle;
    public Vector3 m_rotate = new Vector3(-90, 0f, 0f);
    public virtual void Init()
    {
        _particle = GetComponent<ParticleSystem>();
        this.transform.rotation = Quaternion.Euler(m_rotate.x, m_rotate.y, m_rotate.z);
    }
}

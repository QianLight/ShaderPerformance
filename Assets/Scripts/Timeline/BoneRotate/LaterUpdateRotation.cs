using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[ExecuteInEditMode]
public class LaterUpdateRotation : MonoBehaviour
{
    private Quaternion m_startQuaternion = Quaternion.identity;
    private Quaternion m_laterQuaternion = Quaternion.identity;

    private void OnEnable()
    {
        this.m_startQuaternion = this.transform.localRotation;
    }

    void LateUpdate()
    {
        this.transform.localRotation = m_laterQuaternion;
    }

    public void SetLaterQuaternion(Quaternion laterQuaternion)
    {
        this.m_laterQuaternion = laterQuaternion;
    }

    public Quaternion GetCurQuaternion()
    {
        return m_startQuaternion;
    }


    //public void SetEnable(bool vis)
    //{
    //    //this.quaternion = this.transform.localRotation;
    //    this.enabled = vis;
    //    if (!vis)
    //    {
    //        m_laterQuaternion = this.transform.localRotation;
    //    }
    //}
}
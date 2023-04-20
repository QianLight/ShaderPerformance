using UnityEngine;

[ExecuteInEditMode]
public class LookAt : MonoBehaviour
{
    public Transform m_self;
    public Transform m_head;
    public Transform m_lookTarget;
    public bool m_isLocal = true;
    private Vector3 m_headToTarget;
    private Vector3 m_roleForward;
    private Vector3 m_projectDir;

    private Vector3 m_a;
    private Vector3 m_b;

    private Vector3 m_angles;
    private Vector3 m_flag1 = Vector3.one * -1; //不控制
    private Vector3 m_flag2 = Vector3.zero;     //看向正前方
    private Vector3 m_flag3 = Vector3.one;      //走k的值

    public void LateUpdate()
    {
        if (m_self == null || m_head == null || m_lookTarget == null) return;

        if (m_lookTarget.localScale == m_flag1) return;                     //不控制
        if (m_lookTarget.localScale == m_flag2) m_angles = Vector3.zero;    //看向正前方
        if (m_lookTarget.localScale == m_flag3)                             //走k的值      
        {
            m_roleForward = m_self.transform.forward;
            m_headToTarget = m_lookTarget.position - m_head.position;

            m_projectDir.x = m_headToTarget.x;
            m_projectDir.y = m_roleForward.y;
            m_projectDir.z = m_headToTarget.z;

            float angleX = Vector3.Angle(m_roleForward, m_projectDir);
            float angleY = Vector3.Angle(m_headToTarget, m_projectDir);

            //投射到xz平面
            m_a.x = m_roleForward.x;
            m_a.y = 0;
            m_a.z = m_roleForward.z;

            m_b.x = m_headToTarget.x;
            m_b.y = 0;
            m_b.z = m_headToTarget.z;

            bool flag = true;
            Vector3 isLeft = Vector3.Cross(m_a, m_b); //判断m_headToTarget是在m_roleForward的左侧还是右侧，叉乘判断
            if (isLeft.y > 0)
            {
                flag = true;
            }
            else
            {
                flag = false;
            }

            if (flag) angleX = -angleX;
            if (m_headToTarget.y < m_roleForward.y) angleY = -angleY;

            m_angles.x = angleX;
            m_angles.y = 0;
            m_angles.z = angleY;
        }

        if (m_isLocal)
        {
            m_head.localEulerAngles = m_angles; //local坐标
        }
        else
        {
            m_head.eulerAngles = m_angles; //world坐标，保证策划k多少就是多少，不受其他动画影响。
        }
    }
}





using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaliCrashFix : MonoBehaviour
{
    //��Mali-G76���Կ��ϣ�Ĭ�ϵ�ShadowMap��Ĭ�ϵ�HDR��OpaqueTexture�ᵼ�»��������
    //ԭ����Ĭ�ϵ�ShadowMap����Depth��ʽ��OpaqueTexture����HDR��
    //������������Ⱦ֮ǰ��������׼������ͼ�滻Ĭ����ͼ
    [System.Serializable]
    public struct DefaultTexture
    {
        public string m_name;
        public Texture2D m_tex;
    }
    public List<DefaultTexture> m_defaultTexDic = new List<DefaultTexture>();
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < m_defaultTexDic.Count; i++)
        {
            Shader.SetGlobalTexture(m_defaultTexDic[i].m_name, m_defaultTexDic[i].m_tex);
        }
    }
}

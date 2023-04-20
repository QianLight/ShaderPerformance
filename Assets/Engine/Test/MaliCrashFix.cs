using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaliCrashFix : MonoBehaviour
{
    //在Mali-G76的显卡上，默认的ShadowMap和默认的HDR的OpaqueTexture会导致花屏或崩溃
    //原因是默认的ShadowMap不是Depth格式，OpaqueTexture不是HDR。
    //处理方法是在渲染之前，用事先准备的贴图替换默认贴图
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

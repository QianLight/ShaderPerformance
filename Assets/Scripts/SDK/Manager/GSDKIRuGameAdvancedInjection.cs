using CFUtilPoolLib;
using GSDK.RNU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class GSDKIRuGameAdvancedInjection : GSDK.RNU.IRuGameAdvancedInjection
{
    private static GSDKIRuGameAdvancedInjection m_instance;
    private static CFUtilPoolLib.GSDK.IRuGameAdvancedInjection m_injectionManager;

    public static GSDKIRuGameAdvancedInjection Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new GSDKIRuGameAdvancedInjection();
                m_injectionManager = XInterfaceMgr.singleton.GetInterface<CFUtilPoolLib.GSDK.IRuGameAdvancedInjection>(XCommon.singleton.XHash("IRuGameAdvancedInjection"));
            }
            return m_instance;
        }
    }
    /// <summary>
    /// 获取游戏测的纹理，包括RenderTexure，Texure2D等等
    /// </summary>
    /// <param name="id">唯一id</param>
    /// <param name="action">方便处理异步情况下的Texture返回</param>
    public void GetTextureByIDAsync(string id, Action<Texture> action)
    {
        m_injectionManager.GetTextureByIDAsync(id, action);
    }

    /// <summary>
    /// 根据id释放纹理资源。每当九尾不再使用此纹理的时候，都会调用此方法
    /// </summary>
    /// <param name="id">唯一id</param>
    /// <param name="texture">即将释放的texture</param>
    public void ReleaseTexture(string id, Texture texture)
    {
        m_injectionManager.ReleaseTexture(id, texture);
    }

    /// <summary>
    ///  可用来获取游戏的gameobject，如加载好的prefab
    /// </summary>
    /// <param name="id">唯一id</param>
    /// <param name="action"></param>
    public void GetGameObjectByIDAsync(string id, Action<GameObject> action)
    {
        m_injectionManager.GetGameObjectByIDAsync(id, action);
    }


    /// <summary>
    ///  播放声音
    /// </summary>
    /// <param name="id">唯一id</param>
    public void PlayAudio(string id)
    {
        m_injectionManager.PlayAudio(id);
    }

    /// <summary>
    ///  暂停播放声音
    /// </summary>
    /// <param name="id">唯一id</param>
    public void PauseAudio(string id)
    {
        m_injectionManager.PauseAudio(id);
    }

    /// <summary>
    /// 打开游戏面板
    /// </summary>
    /// <param name="id"></param>
    /// <param name="p"></param>
    public void OpenGameUI(string id, Dictionary<string, object> p)
    {
        m_injectionManager.OpenGameUI(id, p);
    }

    /// <summary>
    ///  可用来获取游戏的特效，挂到 transform 下并返回 object
    /// </summary>
    /// <param name="id">唯一id</param>
    /// <param name="transform">特效父节点</param>
    public object PlayUIEffect(string id, Transform transform)
    {
        return m_injectionManager.PlayUIEffect(id, transform);
    }

    /// <summary>
    ///  可用来释放游戏的特效，指定释放 object
    /// </summary>
    /// <param name="ob"></param>
    public void DestroyUIEffect(object ob)
    {
        m_injectionManager.DestroyUIEffect(ob);
    }
}

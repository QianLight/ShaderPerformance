using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GSDK.RNU
{
    public interface IRuGameAdvancedInjection
    {
        /// <summary>
        /// 获取游戏测的纹理，包括RenderTexure，Texure2D等等
        /// </summary>
        /// <param name="id">唯一id</param>
        /// <param name="action">方便处理异步情况下的Texture返回</param>
        void GetTextureByIDAsync(string id, Action<Texture> action);
        
        /// <summary>
        /// 根据id释放纹理资源。每当九尾不再使用此纹理的时候，都会调用此方法
        /// </summary>
        /// <param name="id">唯一id</param>
        /// <param name="texture">即将释放的texture</param>
        void ReleaseTexture(string id, Texture texture);
        
        /// <summary>
        ///  可用来获取游戏的gameobject，如加载好的prefab
        /// </summary>
        /// <param name="id">唯一id</param>
        /// <param name="action"></param>
        void GetGameObjectByIDAsync(string id, Action<GameObject> action);
        
        
        /// <summary>
        ///  播放声音
        /// </summary>
        /// <param name="id">唯一id</param>
        void PlayAudio(string id);
        
        /// <summary>
        ///  暂停播放声音
        /// </summary>
        /// <param name="id">唯一id</param>
        void PauseAudio(string id);
        
        /// <summary>
        /// 打开游戏面板
        /// </summary>
        /// <param name="id"></param>
        /// <param name="p"></param>
        void OpenGameUI(string id, Dictionary<string, object> p);

    }
}
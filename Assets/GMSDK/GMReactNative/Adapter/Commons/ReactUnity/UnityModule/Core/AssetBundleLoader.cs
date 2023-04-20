/*
 * @Author: hexiaonuo
 * @Date: 2021-10-15
 * @Description: assetbundle file loader
 * @FilePath: ReactUnity/UnityModule/Core/AssetBundleLoader.cs
 */

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GSDK.RNU
{
    partial class RNUMainCore
    {

        private AssetBundle assetBundle;
        private static IRuGameAdvancedInjection advancedInjection;
        public static Texture2D LoadTexture2DAsset(string name)
        {
            return mainCoreInstance.assetBundle.LoadAsset<Texture2D>(name);
        }

        public static Font LoadFontAsset(string name)
        {
            // 调试模式无assetBundle
            if (mainCoreInstance.assetBundle == null) return null;
            return mainCoreInstance.assetBundle.LoadAsset<Font>(name);
        }

        public static GameObject LoadGameObject(string name)
        {
            return mainCoreInstance.assetBundle.LoadAsset<GameObject>(name);
        }

        public static Material LoadMaterial(string name)
        {
            return mainCoreInstance.assetBundle.LoadAsset<Material>(name);
        }
        
        
        // new api
        
        public static void SetGameAdvancedInjection(IRuGameAdvancedInjection injection)
        {
            advancedInjection = injection;
        }
        
        public static void LoadTextureByIDAsync(string id, Action<Texture> successAction, Action<string> errorAction = null,
            Action<float> progressAction = null)
        {
            if (id.StartsWith("game://"))
            {
                if (advancedInjection == null)
                {
                    Util.Log("advancedInjection is null, cannot use texure starts with game:// ");
                    successAction(null);
                }
                else
                {
                    advancedInjection.GetTextureByIDAsync(id.Substring(7), successAction);
                    return;
                }
            }

            //TODO  StaticCommonScript.LoadTexture区分http ，ab包文件
            StaticCommonScript.LoadTexture(id, successAction, errorAction, progressAction);
        }
        public static void ReleaseTexture(string id, Texture texture)
        {
            if (id.StartsWith("game://"))
            {
                advancedInjection.ReleaseTexture(id.Substring(7), texture);
                return; 
            }
            
            // no-op 1. ab包的图片 不需要Release。 由ab包的 assetBundle.Unload处理 2. http 图片在活动存在期间都会被缓存，最终销毁
        }

        public static void LoadGameObjectByIDAsync(string id, Action<GameObject> action)
        {
            if (id.StartsWith("game://"))
            {
                if (advancedInjection == null)
                {
                    Util.Log("advancedInjection is null, cannot use texure starts with game:// ");
                    action(null);
                }
                else
                {
                    advancedInjection.GetGameObjectByIDAsync(id.Substring(7), action);
                    return;
                }
            }
            
            action(mainCoreInstance.assetBundle.LoadAsset<GameObject>(id));
        }

        public static void PlayGameAudio(string id)
        {
            if (advancedInjection == null)
            {
                Util.Log("advancedInjection is null, cannot PlayGameAudio ");
                return;
            }
            
            advancedInjection.PlayAudio(id);
        }
        
        public static void PauseGameAudio(string id)
        {
            if (advancedInjection == null)
            {
                Util.Log("advancedInjection is null, cannot PauseGameAudio ");
                return;
            }
            
            advancedInjection.PauseAudio(id);
        }

        public static void OpenGameUI(string id, Dictionary<string, object> p)
        {
            if (advancedInjection == null)
            {
                Util.Log("advancedInjection is null, cannot OpenGameUI ");
                return;
            }
            
            advancedInjection.OpenGameUI(id, p);
        }
        
    }
}
/*
 * @author yankang.nj
 * 绑定在rootPanel上的通用MonoBehaviour， ReactUnity引擎的诸多操作依赖此文件
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine.UI;
using Object = UnityEngine.Object;


namespace GSDK.RNU
{
    public class StaticCommonScript: MonoBehaviour
    {
        private static List<IFrameListener> fls = new List<IFrameListener>();
        private static List<IFrameIdleListener> fils = new List<IFrameIdleListener>();

        private static StaticCommonScript scc;

        // TODO 放到单独的管理文件
        private static Dictionary<string, Texture> cacheTextures = new Dictionary<string, Texture>();
        
        public static void Init(StaticCommonScript scc)
        {
            StaticCommonScript.scc = scc;
        }

        public static Coroutine StaticStartCoroutine(IEnumerator routine)
        {
            if (scc != null) { return scc.StartCoroutine(routine);}
            Util.Log("StaticCommonCoroutine not init now ");
            return null;
        }

        public static void Destroy()
        {
            if (scc != null)
            {
                scc.StopAllCoroutines();
                scc = null;
            }
            fls.Clear();
            fils.Clear();

            foreach (var kv in cacheTextures)
            {
                Object.Destroy(kv.Value);
            }
            cacheTextures.Clear();
        }


        public static void AddFrameListener(IFrameListener fl)
        {
            fls.Add(fl);
        }

        public static void RemoveFrameListener(IFrameListener fl)
        {
            fls.Remove(fl);
        }


        public static void AddFrameIdleListener(IFrameIdleListener fil)
        {
            fils.Add(fil);
        }
        public static void RemoveFrameIdleListener(IFrameIdleListener fil)
        {
            fils.Remove(fil);
        }
        
        public static void LoadTexture(string uri, Action<Texture> successAction, Action<string> errorAction = null,
            Action<float> progressAction = null)
        {
            if (uri.StartsWith("http") || uri.StartsWith("file"))
            {
                if (cacheTextures.ContainsKey(uri))
                {
                    successAction(cacheTextures[uri]);
                }
                else
                {
                    scc.StartCoroutine(GetTextureUri(uri, successAction, errorAction, progressAction));
                }
                
                
                //scc.StartCoroutine(GetTextureHttp(uri, action));
            }
            else
            {
                Texture texture = RNUMainCore.LoadTexture2DAsset(uri);
                successAction(texture);
            }
        }

        private static IEnumerator GetTextureUri(string uri, Action<Texture> successAction,
            Action<string> errorAction = null, Action<float> progressAction = null)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(uri);

            if (progressAction != null)
            {
                www.SendWebRequest();
                while (!www.isDone)
                {
                    if (progressAction != null)
                    {
                        progressAction(www.downloadProgress);
                    }
                    yield return null;
                }
                progressAction(1);
            }
            else
            {
                yield return www.SendWebRequest();
            }
            
            if (www.error != null)
            {
                if (errorAction != null)
                {
                    errorAction(www.error);
                }
            }
            else
            {
                Texture texture = DownloadHandlerTexture.GetContent(www);
                cacheTextures[uri] = texture;
                successAction(texture);
            }
        }
        
        
        private void Update()
        {
            // 由于在遍历的过程中 IFrameListener 可能发生改变。所以需要调用ToList
            foreach (IFrameListener fl in fls.ToList())
            {
                try
                {
                    fl.Do();
                }
                catch (Exception e)
                {
                    // no-op
                }
            }
        }

        private void LateUpdate()
        {
            // 由于在遍历的过程中 IFrameIdleListener 可能发生改变。所以需要调用ToList
            foreach (IFrameIdleListener fil in fils.ToList())
            {
                try
                {
                    fil.Do();
                }
                catch (Exception e)
                {
                    // no-op
                }
            }
        }

        public static GameObject InstantiatePrefab(GameObject go)
        {
           return Instantiate(go);
        }


    }
}

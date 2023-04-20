using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;
using Object = System.Object;

namespace GSDK.RNU
{
    public class ReactImage : SimpleBaseView
    {
        public static string sOnLoadEventName = "onLoad"; // onLoad 回调事件，在图片加载成功时回调
        public static string sOnLoadEndEventName = "onLoadEnd"; // onLoadEnd 回调事件，无论加载是否成功，在图片加载结束时回调
        public static string sOnProgressEventName = "onProgress"; // onProgress 回调事件，每一帧回调下载进度
        public static string sOnLoadStartEventName = "onLoadStart"; // onLoadStart 回调事件，在图片刚开始加载时回调
        public static string sOnErrorEventName = "onError";  // onError 回调事件，在图片加载错误时回调
        private GameObject realGameObject;
        // 记录是否是默认的透明， 初始值是Transparent， 当设置完纹理之后修改为white。
        private bool isInitTransparent;
        
        private string preUrl = null;
        // false时不向js侧发送对应回调事件
        private bool isListenOnLoad = false;
        private bool isListenOnLoadEnd = false;
        private bool isListenOnProgress = false;
        private bool isListenOnLoadStart = false;
        private bool isListenOnError = false;

        public ReactImage(string name)
        {
            realGameObject = new GameObject(name);
            RawImage ri = realGameObject.AddComponent<RawImage>();
            ri.raycastTarget = false;
            ri.color = Color.clear;
            isInitTransparent = true;
            
            realGameObject.AddComponent<LoadScript>();
        }


        public override GameObject GetGameObject()
        {
            return realGameObject;
        }

        public void SetNetworkUri(string uri)
        {
            if (uri == "")
            {
                return;
            }
            
            RawImage rawImage = realGameObject.GetComponent<RawImage>();
            if (rawImage == null)
            {
                Util.Log("rawImage is null, return");
                return;
            }
            
            if (isListenOnLoadStart)
            {
                ImageLoadCall(sOnLoadStartEventName, new Hashtable());
            }

            var successAction = new Action<Texture>(texture =>
            {

                DestroyTexture();
                
                if (!rawImage.IsDestroyed())
                {
                    if (isInitTransparent)
                    {
                        rawImage.color = Color.white;
                        isInitTransparent = false;
                    }
                    
                    rawImage.texture = texture;
                    preUrl = uri;
                    if (isListenOnLoad)
                    {
                        ImageLoadCall(sOnLoadEventName, new Hashtable
                        {
                            {"width", texture.width},
                            {"height", texture.height},
                            {"uri", uri}
                        });
                    }
                    if (isListenOnLoadEnd)
                    {
                        ImageLoadCall(sOnLoadEndEventName, new Hashtable());
                    }
                }
            });

            var errorAction = new Action<string>(error =>
            {
                if (!rawImage.IsDestroyed())
                {
                    if (isListenOnError)
                    {
                        ImageLoadCall(sOnErrorEventName, new Hashtable
                        {
                            {"error", error}
                        });
                    }
                    if (isListenOnLoadEnd)
                    {
                        ImageLoadCall(sOnLoadEndEventName, new Hashtable());
                    }
                }

            });

            var progressAction = isListenOnProgress
                ? new Action<float>(progress =>
                {
                    if (!rawImage.IsDestroyed())
                    {
                        if (isListenOnProgress)
                        {
                            ImageLoadCall(sOnProgressEventName, new Hashtable
                            {
                                {"loaded", progress},
                                {"total", 1}
                            });
                        }
                    }
                })
                : null;

            RNUMainCore.LoadTextureByIDAsync(uri, successAction, errorAction, progressAction);
        }

        public void setIsListenOnload(bool listener)
        {
            isListenOnLoad = listener;
        }
        
        public void setIsListenOnloadEnd(bool listener)
        {
            isListenOnLoadEnd = listener;
        }
        
        public void setIsListenOnProgress(bool listener)
        {
            isListenOnProgress = listener;
        }
        
        public void setIsListenOnError(bool listener)
        {
            isListenOnError = listener;
        }
        
        public void setIsListenOnLoadStart(bool listener)
        {
            isListenOnLoadStart = listener;
        }
        
        private void ImageLoadCall(string name, Hashtable hashtable)
        {
            StaticCommonScript.StaticStartCoroutine(ImageLoadCallSend(name, hashtable));
        }

        private IEnumerator ImageLoadCallSend(string name, Hashtable hashtable)
        {
            yield return null;
            ArrayList args = new ArrayList();
            args.Add(GetId());
            args.Add(name);
            args.Add(hashtable);

            RNUMainCore.CallJSFunction("RCTEventEmitter", "receiveEvent", args);
        }
        
        public void setMaterial(string material)
        {
            RawImage rawImage = realGameObject.GetComponent<RawImage>();
            
            if (rawImage == null)
            {
                Util.Log("rawImage is null, setMaterial failed, return");
                return;
            }

            rawImage.material = RNUMainCore.LoadMaterial(material);
        }
        
        public void SetLongPressTime(float time)
        {
            IPointerEvent gLongPress = realGameObject.GetComponent<IPointerEvent>();
            if (gLongPress == null)
            {
                gLongPress = realGameObject.AddComponent<IPointerEvent>();
            }

            if (gLongPress == null)
            {
                Util.Log("AddComponent IPointerEvent failed, return");
                return;
            }
            gLongPress.SetLongPressTime(time);
        }

        public void SetDoubleClickTime(float time)
        {
            IPointerEvent gLongPress = realGameObject.GetComponent<IPointerEvent>();
            if (gLongPress == null)
            {
                gLongPress = realGameObject.AddComponent<IPointerEvent>();
            }

            if (gLongPress == null)
            {
                Util.Log("AddComponent IPointerEvent failed, return");
                return;
            }
            gLongPress.SetDoubleClickTime(time);
        }


        private void DestroyTexture()
        {
            var rawImage = realGameObject.GetComponent<RawImage>();
            var tex = rawImage.texture;
            if (tex == null) return;
            rawImage.texture = null;
            RNUMainCore.ReleaseTexture(preUrl, tex);
        }

        public override void Destroy()
        {
            DestroyTexture();
            base.Destroy();
        }
    }
}

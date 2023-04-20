#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine
{
    [Serializable]
    public class EnvBox
    {
        [System.NonSerialized]
        public BoxBoundsHandle boundsHandle = new BoxBoundsHandle ();
        public Vector3 center;
        public Vector3 size;
        public float rotY;

        public EnvBox Clone ()
        {
            return new EnvBox ()
            {
                center = center,
                    size = size,
                    rotY = rotY,
            };
        }
    }

    public interface IEnvContainer
    {
        Transform GetTransform ();
    }

    public class EnvAreaProfile : ScriptableObject, IEnvArea
    {
        public List<EnvBox> areaList = new List<EnvBox> ();
        public EnvBlock envBlock = new EnvBlock ();
        public List<LightingInfo> lights = new List<LightingInfo> ();

        //public List<ResRedirectInfo> resRedirects = new List<ResRedirectInfo> ();

        private  void FindLights(Transform tran)
        {
            for (int i = 0; i < tran.childCount;++i)
            {
                var child = tran.GetChild(i);
                if(child.TryGetComponent<SceneLightRender>(out var slr))
                {
                    lights.Add(slr.GetLigthInfo());
                }
                FindLights(child);
            }
        }
        public void OnUpdate(IEnvContainer envContainer)
        {
            int frameCount = Time.frameCount % 30;
            if (frameCount == 0)
            {
                lights.Clear();
                var tran = envContainer.GetTransform();
                if (tran != null)
                {
                    FindLights(tran);
                }
            }
        }
        public void OnSave (IEnvContainer envContainer)
        {
            //resRedirects.Clear();
            for (int i = 0; i < envBlock.envParams.Count; ++i)
            {
                var envParam = envBlock.envParams[i];
                if (envParam.param != null)
                {
                    if (envParam.param is ResParam)
                    {
                        var resParam = envParam.param as ResParam;
                        if (!string.IsNullOrEmpty (resParam.value) &&
                            resParam.asset != null)
                        {
                            envParam.resType = resParam.resType;
                            string assetName = resParam.value + ResObject.GetExt (resParam.resType);
                            string path = AssetDatabase.GetAssetPath (resParam.asset);
                            int index = path.LastIndexOf ("/");
                            if (index >= 0)
                            {
                                path = path.Substring (0, index + 1);
                            }
                            else
                            {
                                path = "";
                            }
                            
                            /*resRedirects.Add (new ResRedirectInfo ()
                            {
                                name = assetName,
                                    path = path,
                                    type = 0
                            });*/
                        }
                    }
                }
            }
            lights.Clear();
            var tran = envContainer.GetTransform();
            if (tran != null)
            {
                FindLights(tran);
            }
        }

        public List<ResRedirectInfo> GetResInfo()
        {
            return null;
        }

        public static EnvAreaProfile Clone(EnvBlock envBlock,string name)
        {
            var eap = CreateInstance<EnvAreaProfile>();
            eap.name = name;
            eap.envBlock = envBlock;
            return eap;
        }
    }
}
#endif
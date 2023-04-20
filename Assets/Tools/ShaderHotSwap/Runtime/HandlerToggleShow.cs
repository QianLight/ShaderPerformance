using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UsingTheirs.ShaderHotSwap
{
    public static class HandlerToggleShow
    {

        public const string particlesystem = "particlesystem";
        public const string meshrender = "meshrender";
        public const string skinrender = "skinrender";
        public const string skybox = "skybox";
        public const string ui = "ui";
        public const string log = "log";

        public static Dictionary<string, List<GameObject>> toggleDic = new Dictionary<string, List<GameObject>>();

        public static string HandlerMain(string jsonRequest)
        {
            try
            {
                var req = JsonUtility.FromJson<QueryCommonReq>(jsonRequest);

                var resString = "";
                List<GameObject> newListData = new List<GameObject>();
                if (req.hidden)
                {
                    switch (req.msg)
                    {
                        case particlesystem:
                            resString = ToggleParticleSystem(newListData);
                            break;
                        case meshrender:
                            resString = ToggleMeshRender(newListData);
                            break;
                        case skinrender:
                            resString = ToggleSkinMesh(newListData);
                            break;
                        case ui:
                            resString = ToggleUI(newListData);
                            break;
                        case log:
                            resString = ToggleLog(false);
                            break;
                        case skybox:
                            Camera.main.clearFlags = CameraClearFlags.SolidColor;
                            break;
                    }

                    toggleDic[req.msg] = newListData;
                }
                else
                {
                    switch (req.msg)
                    {
                        case skybox:
                            Camera.main.clearFlags = CameraClearFlags.Skybox;
                            break;
                        case log:
                            resString=ToggleLog(true);
                            break;
                        default:
                            if (toggleDic.ContainsKey(req.msg))
                            {
                                newListData = toggleDic[req.msg];

                                for (int i = 0; i < newListData.Count; i++)
                                {
                                    GameObject obj = newListData[i];
                                    obj.SetActive(true);
                                }

                                resString = req.msg + " Lenght:" + newListData.Count;
                            }

                            break;
                    }

                }


                MemoryLogger.Log("[HandlerParticleSystem] req: {0}  res:{1}", req.hidden, resString);

                return resString;
            }
            catch (System.Exception e)
            {
                return HandlerCommon.ErrorString(e.ToString());
            }
        }

        private static string ToggleParticleSystem(List<GameObject> listData)
        {
            ParticleSystem[] allRenders = GameObject.FindObjectsOfType<ParticleSystem>();
            for (int i = 0; i < allRenders.Length; i++)
            {
                ParticleSystem ps = allRenders[i];
                listData.Add(ps.gameObject);
            }

            return ToggleCommon(listData);
        }

        private static string ToggleSkinMesh(List<GameObject> listData)
        {
            SkinnedMeshRenderer[] allRenders = GameObject.FindObjectsOfType<SkinnedMeshRenderer>();
            for (int i = 0; i < allRenders.Length; i++)
            {
                SkinnedMeshRenderer ps = allRenders[i];
                listData.Add(ps.gameObject);
            }

            return ToggleCommon(listData);
        }

        private static string ToggleMeshRender(List<GameObject> listData)
        {
            MeshRenderer[] allRenders = GameObject.FindObjectsOfType<MeshRenderer>();
            for (int i = 0; i < allRenders.Length; i++)
            {
                MeshRenderer ps = allRenders[i];
                listData.Add(ps.gameObject);
            }

            return ToggleCommon(listData);
        }

        private static string ToggleUI(List<GameObject> listData)
        {
            Canvas[] allRenders = GameObject.FindObjectsOfType<Canvas>();
            for (int i = 0; i < allRenders.Length; i++)
            {
                Canvas ps = allRenders[i];
                listData.Add(ps.gameObject);
            }

            return ToggleCommon(listData);
        }
        
        private static string ToggleLog(bool b)
        {
            GameObject lastDebugObj = GameObject.Find("Canvas/root");
            if (lastDebugObj != null)
            {
                MonoBehaviour lastDebug = lastDebugObj.GetComponent("CFClient.DebugBehaivour") as MonoBehaviour;
                if (lastDebug != null)
                    lastDebug.enabled = b;
            }

            Debug.unityLogger.logEnabled = b;

            return "log:" + b;
        }

        private static string ToggleCommon(List<GameObject> listData)
        {
            var resString = "";

            RemoteRendererList newRemoteRendererList = new RemoteRendererList();

            newRemoteRendererList.renderers = new List<RemoteRenderer>();

            for (int i = 0; i < listData.Count; i++)
            {
                GameObject itm = listData[i];
                RemoteRenderer newItem = new RemoteRenderer();
                newItem.name = itm.name;
                newItem.instanceID = itm.GetInstanceID();
                newRemoteRendererList.renderers.Add(newItem);
                itm.gameObject.SetActive(false);
            }

            resString = JsonUtility.ToJson(newRemoteRendererList);
            return resString;
        }
    }
}

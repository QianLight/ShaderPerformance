/********************************************************************
	created:	2022年6月20日 19:23:48
	file base:	StreamerLoader.cs
	author:		c a o   f e n g
	
	purpose:	资源加载器 目前主要加载prefab 
*********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace CFEngine.WorldStreamer
{
    public class StreamerLoader
    {
        public StreamerData m_StreamerData;
        private LoadState m_LoadState = LoadState.None;

        public bool IsLoading()
        {
            return m_LoadState == LoadState.Loading;
        }

        public Bounds m_AreaBounds;

        public static bool IsEditorMonitor = false;
        public Transform m_Parent;

        public StreamerLoader(StreamerData data, Transform parent)
        {
            m_Parent = parent;
            m_StreamerData = data;

            Vector3 parPos = parent.position;


            m_AreaBounds = StreamerHelper.TransformBounds(m_StreamerData.m_ShowBounds.center,
                m_StreamerData.m_ShowBounds.size + m_StreamerData.m_LoadingRange, parent);

            //Debug.Log("StreamerLoader:" + parent.position + "  " + m_AreaBounds, parent);
        }


        public bool CaculateCurrentShowLoading(Vector3 targetPos)
        {
            return m_AreaBounds.Contains(targetPos);//如果还没有可显示的层级 在配置的可显示区域内启动显示
        }
        

        public void Show(EnumSteamerActionType type)
        {
            switch (type)
            {
                case EnumSteamerActionType.Loading:
                    Loading();
                    break;
                case EnumSteamerActionType.Unloading:
                    UnLoading();
                    break;
            }

            UpdateAssetState();
        }

        private void UpdateAssetState()
        {
            switch (m_LoadState)
            {
                case LoadState.Loading:
                    
                    break;
                case LoadState.Unloading:

                    break;
            }
        }
        
        
        public void SendStreamerAction(EnumSteamerActionType type, EnumStreamerLODType lodType,Transform parent=null)
        {
            if (m_StreamerData.m_EnumSteamerLODType != lodType && lodType != EnumStreamerLODType.All) return;

            Show(type);
        }


        private void Loading()
        {
            if (m_LoadState != LoadState.None) return;
            m_LoadState = LoadState.Loading;

            LoadingPrefab(null);
        }


        private void UnLoading()
        {
            if (m_LoadState != LoadState.Loaded) return;

            m_LoadState = LoadState.Unloading;
            
            UnLoadingPrefab(null);
        }




        #region prefab

        private GameObject m_GameObject;
        private AssetHandler m_AssetHandler;

        public void LoadingPrefab(Action callBack)
        {

#if UNITY_EDITOR
            if (!Application.isPlaying || IsEditorMonitor)
            {
                m_GameObject = GameObject.Instantiate(m_StreamerData.m_AssetObj, m_Parent) as GameObject;
                m_LoadState = LoadState.Loaded;
                LightmapManager.RefreshAllVolumnsInEditor();
                return;
            }
#endif

            m_AssetHandler = null;
            LoadPrefabAsync("prefabs/StreamerWorld/" + m_StreamerData.m_AssetName, ref m_AssetHandler,
                OceanWorldResLoadCb);

            void OceanWorldResLoadCb(AssetHandler ah, LoadInstance li)
            {

                m_LoadState = LoadState.Loaded;

                GameObject solo = ah.obj as GameObject;
                if (solo == null)
                {
                    LoadMgr.singleton.Destroy(ref m_AssetHandler);
                    return;
                }
                solo.transform.localPosition=Vector3.zero;
                solo.transform.localEulerAngles=Vector3.zero;
                
                solo.transform.SetParent(m_Parent,false);
                LightmapManager.RefreshAllVolumns();
            }
        }

        public void UnLoadingPrefab(Action callBack)
        {

#if UNITY_EDITOR
            if (!Application.isPlaying || IsEditorMonitor)
            {
                if (m_GameObject != null)
                    GameObject.DestroyImmediate(m_GameObject);

                m_LoadState = LoadState.None;
                LightmapManager.RefreshAllVolumnsInEditor();
                return;
            }
#endif

            //Debug.Log("UnLoadingPrefab:"+m_AssetHandler.path);
            
            LoadMgr.singleton.Destroy(ref m_AssetHandler);
            m_LoadState = LoadState.None;
            LightmapManager.RefreshAllVolumns();
        }


        private GameObject LoadPrefabAsync(string path, ref AssetHandler asset, ResLoadCb cb, uint flag = 0)
        {
           // Debug.Log("LoadPrefabAsync:"+path);
            return EngineUtility.LoadPrefab(path, ref asset, flag, true, cb, true);
        }

        #endregion
        
    }

}

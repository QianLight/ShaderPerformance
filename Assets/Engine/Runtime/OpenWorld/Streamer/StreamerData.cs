using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace CFEngine.WorldStreamer
{

    [Serializable]
    public class StreamerData
    {
        [Header("��Դ����")] public string m_AssetName = "";

#if UNITY_EDITOR
        [Header("������Դ")] public UnityEngine.Object m_AssetObj;
#endif
        [Header("���ط�Χ����")] public Vector3 m_LoadingRange = new Vector3(3, 3, 3);
        //[Header("ж�ط�Χ����")] public Vector3 m_UnLoadingRange = new Vector3(3, 3, 3);

        [Header("��ʾBounds")] public Bounds m_ShowBounds = new Bounds(Vector3.zero, Vector3.one);

        [Header("LOD����")] public EnumStreamerLODType m_EnumSteamerLODType = EnumStreamerLODType.None;


        public StreamerData(UnityEngine.Object obj)
        {
            if (obj == null) return;
#if UNITY_EDITOR
            m_AssetObj = obj;
#endif
            //CaculateBounds();
        }



        public void RefreshData()
        {
#if UNITY_EDITOR
            if (m_AssetObj == null) return;
#endif
            CaculateBounds();
        }
        


        public void CaculateBounds()
        {

#if UNITY_EDITOR
            m_AssetName = m_AssetObj.name;


            List<UnityEngine.Renderer> allListRenders = new List<Renderer>();

            allListRenders.AddRange((m_AssetObj as GameObject).transform.GetComponentsInChildren<MeshRenderer>(false));



            Bounds _ShowBounds = new Bounds();

            _ShowBounds = allListRenders[0].bounds;

            foreach (Renderer r in allListRenders)
            {
                if (r == null)
                    continue;

                if (_ShowBounds.extents == Vector3.zero)
                    _ShowBounds = r.bounds;
                else
                {
                    _ShowBounds.Encapsulate(r.bounds);
                }
            }

            m_ShowBounds = _ShowBounds;

#endif
        }


    }


    public enum LoadState
    {
        None,
        Loading,
        Loaded,
        Unloading,
    }
}

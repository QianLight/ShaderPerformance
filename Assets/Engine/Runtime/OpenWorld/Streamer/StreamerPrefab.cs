/********************************************************************
	created:	2022年6月20日 19:24:17
	file base:	StreamerPrefab.cs
	author:		c a o   f e n g
	
	purpose:	主要给策划 进行场景配置
*********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CFEngine.WorldStreamer
{
    public class StreamerPrefab : MonoBehaviour
    {
        public StreamerManager m_StreamerManager;

        public StreamerLoader[] m_StreamerLoader;

        public void InitData()
        {
            if (m_StreamerManager == null) return;

            if (m_StreamerLoader == null)
                m_StreamerLoader = m_StreamerManager.GetLoadersByData(transform);

            m_defaultShow = m_StreamerLoader[m_StreamerLoader.Length - 1];
        }


        private StreamerLoader m_StreamerDataTmp;
        public bool isVisible = false;

        private StreamerLoader m_defaultShow;
        private StreamerLoader m_CurrentShow;
        private StreamerLoader m_LastShow;
        
        public void UpdateState(Vector3 targetPos)
        {
            if (!isVisible) return;

            ShowModelSteps(targetPos);
        }

        private void ShowModelSteps(Vector3 targetPos) //显示模型三部曲 判断状态  激活当前显示并且加载完成   最后删除
        {
            m_CurrentShow = null;
            for (int i = 0; i < m_StreamerLoader.Length; i++)
            {
                m_StreamerDataTmp = m_StreamerLoader[i];

                if (m_StreamerDataTmp.CaculateCurrentShowLoading(targetPos))
                {
                    m_CurrentShow = m_StreamerDataTmp;
                    m_CurrentShow.Show(EnumSteamerActionType.Loading);
                    break;
                }
            }

            if (m_CurrentShow == null) //meiy显示最高等级的 逐级显示
            {
                m_CurrentShow = m_defaultShow; //显示最低等级
                m_CurrentShow.Show(EnumSteamerActionType.Loading);
            }
            if (m_CurrentShow.IsLoading()) return;//加载完成之后 再进行卸载

            if (m_CurrentShow != m_LastShow)//假如触发了 切换 尝试卸载
            {
                if (m_LastShow != null)
                {
                    m_LastShow.Show(EnumSteamerActionType.Unloading);
                    m_LastShow = null;
                }

                m_LastShow = m_CurrentShow;
            }
        }


        public void SendStreamerAction(EnumSteamerActionType type, EnumStreamerLODType lodType)
        {
            for (int i = 0; i < m_StreamerLoader.Length; i++)
            {
                m_StreamerLoader[i].SendStreamerAction(type,lodType);
            }
        }


        public void Show(bool b)
        {
            InitData();
            if (b)
            {
                SendStreamerAction(EnumSteamerActionType.Loading, EnumStreamerLODType.LOD0);
            }
            else
            {
                SendStreamerAction(EnumSteamerActionType.Unloading, EnumStreamerLODType.LOD0);
            }

            if (!b)
            {
                for (int i = transform.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }
        }



        public string ShowName = "";

        public BoundingSphere GetBoundingSphere()
        {
            for (int j = 0; j < m_StreamerLoader.Length; j++)
            {
                StreamerData sd = m_StreamerLoader[j].m_StreamerData;
                if (sd.m_EnumSteamerLODType != EnumStreamerLODType.LOD0) continue;

                ShowName = sd.m_AssetName;
                Vector3 size = sd.m_ShowBounds.extents;
                float radius = Mathf.Max(Mathf.Max(size.x, size.y), size.z);

                Vector3 newPos = transform.TransformPoint(sd.m_ShowBounds.center);
                return new BoundingSphere(newPos, radius);
            }

            return new BoundingSphere(Vector3.zero, 1000);
        }

        private void OnEnable()
        {
           
        }
    }
}

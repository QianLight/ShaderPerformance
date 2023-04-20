using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CFEngine.WorldStreamer
{
    public class CullingStreamerByCullingGroup
    {

        CullingGroup m_CullingGroup;
        StreamerPrefab[] m_StreamerPrefab;
        public void InitCullingData(StreamerPrefab[] streamers)
        {

            m_StreamerPrefab = streamers;
            m_CullingGroup = new CullingGroup();

            BoundingSphere[] spheres = new BoundingSphere[streamers.Length];

            for (int i = 0; i < streamers.Length; i++)
            {
                StreamerPrefab sm = streamers[i];
                spheres[i] = sm.GetBoundingSphere();
            }

            Camera cam =  Camera.main;
            m_CullingGroup.targetCamera = cam;
            m_CullingGroup.SetDistanceReferencePoint(cam.transform);
            m_CullingGroup.SetBoundingSpheres(spheres);
            m_CullingGroup.SetBoundingSphereCount(spheres.Length);
            m_CullingGroup.onStateChanged = OnChange;
            m_CullingGroup.enabled = true;
        }

        private StreamerPrefab m_SteamerManagerTmp;
        private void OnChange(CullingGroupEvent evt)
        {
            //Debug.Log("CullingGroupEvent: index:" + evt.index + "  isVisible:" + evt.isVisible + "  wasVisible:" + evt.wasVisible + "  hasBecomeInvisible:" + evt.hasBecomeInvisible + "  hasBecomeVisible:" + evt.hasBecomeVisible);
            m_SteamerManagerTmp = m_StreamerPrefab[evt.index];

            if (evt.hasBecomeVisible)
            {
                m_SteamerManagerTmp.isVisible = true;
            }


            if (evt.hasBecomeInvisible)
            {
                m_SteamerManagerTmp.isVisible = false;
                m_SteamerManagerTmp.SendStreamerAction(EnumSteamerActionType.Unloading, EnumStreamerLODType.All);
            }
        }

        public void OnDestory()
        {
            m_CullingGroup.Dispose();
            m_CullingGroup = null;
        }

        public void OnDrawGizmos()
        {
            
        }
    }
}
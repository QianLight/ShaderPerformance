using System;
using CFUtilPoolLib;
using UnityEngine;
using System.Collections.Generic;
using FMODUnity;

namespace Assets.Scripts.FMOD
{
    public class XStudioListener : IStudioListener
    {
        private static StudioListener m_studioListener;
        private static XStudioListener m_instance;
        public static XStudioListener Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new XStudioListener();
                }
                return m_instance;
            }
        }

        XStudioListener()
        {
        }
        public bool Deprecated
        {
            get;
            set;
        }

        public UnityEngine.Object AddListener(GameObject go)
        {
            StudioListener listener = go.GetComponent<StudioListener>();
            if (listener == null)
            {
                listener = go.AddComponent<StudioListener>();
                m_studioListener = listener;
            }
            return listener;
        }

        public void ReturnListener(UnityEngine.Object listener)
        {
            GameObject.Destroy(listener);
        }

        public void SetListenerIndex(UnityEngine.Object listener, int index)
        {
            if(listener != null)
            {
                StudioListener studioListener = listener as StudioListener;
                studioListener.ListenerNumber = index;
            }
        }

        public void SetListenerEar(UnityEngine.Object listener, GameObject go)
        {
            if(listener != null)
            {
                StudioListener studioListener = listener as StudioListener;
                studioListener.Camera = go;
            }
        }

        public void UpdateListenerAttribute(ref Vector3 forward, ref Vector3 up, ref Vector3 position)
        {
            if (m_studioListener == null) return;
            m_studioListener.UpdateListenerAttribute(ref forward, ref up, ref position);
        }

        public void UseCameraAttribute()
        {
            if (m_studioListener == null) return;
        }
    }
}
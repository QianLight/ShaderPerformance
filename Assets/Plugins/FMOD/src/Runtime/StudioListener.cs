using UnityEngine;

namespace FMODUnity
{
    [AddComponentMenu("FMOD Studio/FMOD Studio Listener")]
    public class StudioListener : MonoBehaviour
    {
        #if UNITY_PHYSICS_EXIST || !UNITY_2019_1_OR_NEWER
        Rigidbody rigidBody;
        #endif
        #if UNITY_PHYSICS2D_EXIST || !UNITY_2019_1_OR_NEWER
        Rigidbody2D rigidBody2D;
        #endif

        public GameObject attenuationObject;

        public int ListenerNumber = -1;

        [HideInInspector]
        public GameObject Camera;

        private GameObject m_listenerGo; //监听者和相机有一定偏移
        public float m_forwardScale = 4;
        public Vector3 m_offsetPos = Vector3.zero;
        private bool m_useCamera = true;

        void OnEnable()
        {
            if (FMODUnity.RuntimeManager.Instance != null)
            {
                InitListenerGo();
                RuntimeUtils.EnforceLibraryOrder();
#if UNITY_PHYSICS_EXIST || !UNITY_2019_1_OR_NEWER
                rigidBody = gameObject.GetComponent<Rigidbody>();
#endif
#if UNITY_PHYSICS2D_EXIST || !UNITY_2019_1_OR_NEWER
                rigidBody2D = gameObject.GetComponent<Rigidbody2D>();
#endif
                ListenerNumber = RuntimeManager.AddListener(this);
            }
        }
        private void InitListenerGo()
        {
            if (m_listenerGo == null)
            {
                m_listenerGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                m_listenerGo.name = "listener";
            }

            m_listenerGo.transform.SetParent(this.transform);
            m_listenerGo.transform.position = this.transform.position + this.transform.forward * m_forwardScale + m_offsetPos;
            m_listenerGo.transform.localEulerAngles = Vector3.zero;
            m_listenerGo.transform.localScale = Vector3.one;
            m_listenerGo.SetActive(false);
        }

        void OnDisable()
        {
            if (FMODUnity.RuntimeManager.Instance != null)
            {
                RuntimeManager.RemoveListener(this);
            }
        }

        void Update()
        {
            if (FMODUnity.RuntimeManager.Instance != null)
            {
                if (m_listenerGo == null)
                {
                    OnEnable();
                }

                if (m_useCamera)
                {
                    SetListenerAttributeByCamera();
                    m_listenerGo.transform.position = this.transform.position;
                }

                //if (ListenerNumber >= 0 && ListenerNumber < FMOD.CONSTANTS.MAX_LISTENERS)
                //{
                //    SetListenerLocation();
                //}
            }
        }

        void SetListenerAttributeByCamera()
        {
            if (FMODUnity.RuntimeManager.Instance != null)
            {
                RuntimeManager.SetListenerLocation(ListenerNumber, this.gameObject);
            }
        }

        void SetListenerLocation()
        {
            #if UNITY_PHYSICS_EXIST || !UNITY_2019_1_OR_NEWER
            if (rigidBody)
            {
                RuntimeManager.SetListenerLocation(ListenerNumber, gameObject, rigidBody, attenuationObject);
            }
            else
            #endif
            #if UNITY_PHYSICS2D_EXIST || !UNITY_2019_1_OR_NEWER
            if (rigidBody2D)
            {
                RuntimeManager.SetListenerLocation(ListenerNumber, gameObject, rigidBody2D, attenuationObject);
            }
            else
            #endif
            {
                RuntimeManager.SetListenerLocation(ListenerNumber, gameObject, attenuationObject);
            }
        }


        /// <summary>
        /// 这个有XEntity的Update调用，设置监听位置到人物头顶
        /// </summary>
        /// <param name="forward"></param>
        /// <param name="up"></param>
        /// <param name="position"></param>
        public void UpdateListenerAttribute(ref Vector3 forward, ref Vector3 up, ref Vector3 position)
        {
            if (FMODUnity.RuntimeManager.Instance != null)
            {
                m_useCamera = false;
                forward = forward; //耳朵的朝向改成相反的方向，保证声音左右耳正确。
                RuntimeManager.SetListenerLocation(ListenerNumber, ref forward, ref up, ref position);
                m_listenerGo.transform.position = position;
                m_listenerGo.transform.forward = forward;
            }
        }

        /// <summary>
        /// 这个有XEntity的Update调用，设置监听位置到摄像机位置
        /// </summary>
        /// <param name="forward"></param>
        /// <param name="up"></param>
        /// <param name="position"></param>

        public void UseCameraAttribute()
        {
            m_useCamera = true;
        }

        private void OnDrawGizmos()
        {
            if (m_listenerGo == null) return;
            Gizmos.color = Color.red;
            Vector3 pos = m_listenerGo.transform.position;
            Gizmos.DrawLine(pos, pos + m_listenerGo.transform.right * 3);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pos, pos + m_listenerGo.transform.up * 3);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(pos, pos + m_listenerGo.transform.forward * 3);
        }
    }
}
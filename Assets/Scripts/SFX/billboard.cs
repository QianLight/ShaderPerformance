using CFUtilPoolLib;
using UnityEngine;

public class billboard : MonoBehaviour
{
    public bool RandomZ_Rotation = false;
#if !UNITY_EDITOR
    private IAssociatedCamera _camera = null;
#endif
    private Transform cameraTrans
    {
        get
        {
#if UNITY_EDITOR
            return Camera.main == null ? null : Camera.main.transform;
#else
            if (_camera == null || _camera.Deprecated)
                _camera = XInterfaceMgr.singleton.GetInterface<IAssociatedCamera> (XCommon.singleton.XHash ("IAssociatedCamera"));
            if (_camera == null)
                return null;
            Camera c = _camera.Get ();
            return c == null ? null : c.transform;
#endif
        }
    }
    private Transform cacheTrans = null;
    private Transform cacheCameraTrans = null;
    //private static float updateTime = 0.1f;
    //private float time = 0.0f;
    // Use this for initialization
    //void Start () 
    //   {

    //}

    // Update is called once per frame
    void Update ()
    {
        if (cacheTrans == null)
            cacheTrans = transform;
        //if (_camera == null || _camera.Deprecated) _camera = XInterfaceMgr.singleton.GetInterface<IAssociatedCamera>(XCommon.singleton.XHash("IAssociatedCamera"));

        if (cacheCameraTrans == null)
        {
            //Camera c = _camera.Get();
            //if (c != null)
            //    cacheCameraTrans = c.transform;
            cacheCameraTrans = cameraTrans;
        }
        if (cacheCameraTrans != null && cacheTrans != null)
        {
            cacheTrans.LookAt (cacheCameraTrans.position);
            if (RandomZ_Rotation)
            {
                RandomZ_Rotation = false;
                cacheTrans.Rotate (Vector3.forward, Random.Range (0f, 360f));
            }
        }
    }
}
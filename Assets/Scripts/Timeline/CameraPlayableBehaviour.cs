using CFEngine;
using CFUtilPoolLib;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class CameraPlayableBehaviour : DirectBaseBehaviour
{
    private GameObject _dummyObject = null;
    private Transform _dummyCamera = null;
    private float _scale = 1f;
    private double _duration;
    private float _shakeTime = 0.0f;

    public static bool reShaking = false;
    public static bool isShaking = false;
#if UNITY_EDITOR
    const string CAMERA_NAME = "DummyCamera";
#endif

    private CameraPlayableAsset cameraAsset
    {
        get
        {
            return asset as CameraPlayableAsset;
        }
    }
    public void Set (double duration)
    {
        _duration = duration;
        _shakeTime = 0;

    }
    public override void Reset ()
    {
        base.Reset ();
    }
    public static void TrigerShaking ()
    {
        if (isShaking)
        {
            reShaking = true;
        }
        isShaking = true;
    }

    public override void OnBehaviourPlay (Playable playable, FrameData info)
    {
        var cpa = cameraAsset;
        if (cpa != null && cpa._clip != null)
        {
            _scale = cpa._clip.length / (float) playable.GetDuration ();
        }
        _dummyObject = RTimeline.singleton.dummyObject;
        _dummyCamera = RTimeline.singleton.dummyCamera;

#if UNITY_EDITOR
        if (!EngineContext.IsRunning)
        {
            if (_dummyObject == null)
            {
                _dummyObject = GameObject.Find (CAMERA_NAME);
            }
            if (_dummyObject == null)
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject> (
                    string.Format ("{0}/Prefabs/Cinemachine/DummyCamera.prefab", AssetsConfig.instance.ResourcePath));
                _dummyObject = (GameObject) PrefabUtility.InstantiatePrefab (go);
            }
            if (_dummyObject != null)
            {
                _dummyCamera = _dummyObject.transform.GetChild (0);
            }
        }
#endif
    }

    public override void PrepareFrame (Playable playable, FrameData info)
    {
        var cpa = cameraAsset;
        var context = EngineContext.instance;

        if (context != null &&
            _dummyObject != null && _dummyCamera != null &&
            context.CameraTransCache != null &&
            cpa != null && cpa._clip != null)
        {
            float time = (float) playable.GetTime ();
            if (time < _duration)
            {
                cpa._clip.SampleAnimation (_dummyObject, time * _scale);
                Vector3 forward = -_dummyCamera.right; // Vector3.Cross (_dummyCamera.forward, _dummyCamera.up);
                Quaternion q = Quaternion.LookRotation (forward, _dummyCamera.up);
                context.CameraTransCache.rotation = q;
                context.CameraTransCache.position = _dummyCamera.position + Shake () ;
            }
        }
    }


    public Vector3 Shake ()
    {
        var cpa = cameraAsset;
        if (cpa != null && cpa._shakeClip != null)
        {
            bool shaking = isShaking;
            bool reshaking = reShaking;

            if (reshaking || (shaking && _shakeTime < 1e-4))
            {
                _shakeTime = Time.time;
                reShaking = false;
            }
            if (shaking)
            {
                float len = cpa._shakeClip.length;
                float curr = Time.time - _shakeTime;
                if (curr >= len)
                {
                    isShaking = false;
                    _shakeTime = 0;
                }
                else
                {
                    cpa._shakeClip.SampleAnimation (_dummyObject, curr);
                    return _dummyCamera.localPosition;
                }
            }
        }

        return Vector3.zero;
    }

}
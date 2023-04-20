#if !UNITY_2019_1_OR_NEWER
#define CINEMACHINE_TIMELINE
#endif
#if CINEMACHINE_TIMELINE

using UnityEngine.Playables;
using Cinemachine;
using UnityEngine.Timeline;
using UnityEngine;

//namespace Cinemachine.Timeline
//{
public sealed class CinemachineShotPlayable : DirectBaseBehaviour//PlayableBehaviour
{
    public CinemachineVirtualCameraBase VirtualCamera;

    public bool IsValid { get { return VirtualCamera != null; } }

    private GameObject[] objs = null;

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        base.PrepareFrame(playable, info);
    //}

    //public override void OnBehaviourPlay(Playable playable, FrameData info)
    //{
    //    base.OnBehaviourPlay(playable, info);

    //    GameObject[] objs = null;
        

        if (!string.IsNullOrEmpty(VirtualCamera.FollowPath) || !string.IsNullOrEmpty(VirtualCamera.LookAtPath))
        {
            if (Application.isPlaying && objs == null)
                objs = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        }

        if (!string.IsNullOrEmpty(VirtualCamera.FollowPath) && VirtualCamera.Follow == null)
        {
            GameObject obj = GetLookatAndFollowObj(VirtualCamera.FollowPath, objs);
            if (obj != null)
                VirtualCamera.Follow = obj.transform;
        }
        if (!string.IsNullOrEmpty(VirtualCamera.LookAtPath) && VirtualCamera.LookAt == null)
        {
            GameObject obj = GetLookatAndFollowObj(VirtualCamera.LookAtPath, objs);
            if (obj != null)
                VirtualCamera.LookAt = obj.transform;
        }
    }

    public static GameObject GetLookatAndFollowObj(string path, GameObject[] objs)
    {
        GameObject obj = null;
        if (Application.isPlaying && objs != null)
        {
            obj = GameObject.Find(path);
            if (obj != null)
                return obj;

            for (int j = 0; j < objs.Length; j++)
            {
                Transform tmp = objs[j].transform.Find(path);
                if (tmp != null)
                    return tmp.gameObject;
            }
        }
        else if (!Application.isPlaying)
        {
            obj = GameObject.Find(path);
        }
        return obj;
    }
}
//}
#endif

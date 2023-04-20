using UnityEngine;
namespace CFEngine
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class DistortionControl : MonoBehaviour
    {
        public bool useForUI;
        private void Update()
        {
        #if UNITY_EDITOR
            if (Application.isPlaying && EngineContext.IsRunning || !EngineContext.IsRunning)
            {
                CheckCamera();
            }

            return;
        #endif
            if (EngineContext.instance != null)
            {
                CheckCamera();
            }
        }

        private void CheckCamera()
        {
            if (useForUI) EngineContext.none3DCamera = true;
            if(EngineContext.IsRunning)EngineContext.instance.stateflag.SetFlag(EngineContext.SFlag_Distortion, true);
        }
    }

// #if UNITY_EDITOR
//     [CustomEditor (typeof (DistortionControl))]
//     public class DistortionControlEditor : UnityEngineEditor
//     {
//     }
// #endif
}

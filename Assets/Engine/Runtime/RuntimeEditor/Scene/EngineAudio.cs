#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class EngineAudio : MonoBehaviour
    {
        public virtual void StartAudio (string name)
        {

        }
        public virtual void StopAudio ()
        {

        }

        public virtual void SetPos()
        {

        }

        public virtual bool IsValid()
        {
            return false;
        }
    }
}
#endif
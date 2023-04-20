#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class InteractiveExString : MonoBehaviour
    {
        public string ExString = "";
    }
}
#endif
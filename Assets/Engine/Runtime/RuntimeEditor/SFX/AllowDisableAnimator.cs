#if UNITY_EDITOR
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngineEditor = UnityEditor.Editor;
#endif
namespace CFEngine
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class AllowDisableAnimator : MonoBehaviour
    {
    }
}
#endif
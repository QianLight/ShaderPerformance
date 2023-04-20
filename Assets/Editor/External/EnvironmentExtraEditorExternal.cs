#if UNITY_EDITOR
using CFUtilPoolLib;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    public partial class EnvironmentExtraEditor
    {
        public void OnInspectorExternal ()
        {
            if (EngineContext.IsRunning)
            {
                var context = EngineContext.instance;
                EditorGUILayout.BeginHorizontal ();
                ee.hideLayer = EditorGUILayout.MaskField ("HideLayer", ee.hideLayer, GameObjectLayerHelper.hideLayerStr);
                EditorGUILayout.EndHorizontal ();
                EditorGUILayout.BeginHorizontal ();
                EditorGUILayout.LabelField (string.Format ("Stack:{0}", GameObjectLayerHelper.stackIndex.ToString ()));
                if (GUILayout.Button ("Push", GUILayout.MaxWidth (80)))
                {
                    GameObjectLayerHelper.HideLayerWithMask (ee.hideLayer, false);
                    GameObjectLayerHelper.Push ();
                    context.renderflag.SetFlag (EngineContext.RFlag_LayerMaskDirty, true);
                }
                if (GUILayout.Button ("Pop", GUILayout.MaxWidth (80)))
                {
                    GameObjectLayerHelper.Pop ();
                    context.renderflag.SetFlag (EngineContext.RFlag_LayerMaskDirty, true);
                }
                EditorGUILayout.EndHorizontal ();
            }
        }


    }
}
#endif
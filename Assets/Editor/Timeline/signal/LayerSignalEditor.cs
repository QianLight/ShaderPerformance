using CFUtilPoolLib;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;

[CustomEditor(typeof(LayerSignal))]
public class LayerSignalEditor : Editor
{
    LayerSignal signal;

    private void OnEnable()
    {
        signal = target as LayerSignal;
    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();


        PlayableDirector dir = TimelineEditor.inspectedDirector;
        if (dir)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("hide layer");
            signal.layerMask = EditorGUILayout.MaskField(signal.layerMask, GameObjectLayerHelper.hideLayerStr);
            GUILayout.EndHorizontal();
        }

    }
}

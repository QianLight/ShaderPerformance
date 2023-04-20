using System;
using System.Collections.Generic;
using System.Linq;
using CFEngine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{

    [CustomEditor (typeof (EngineTrack), true, isFallback = true)]
    [CanEditMultipleObjects]
    class EngineTrackInspector : UnityEditor.Editor
    {
        private GUIStyle pressedButton;
        private void OnEnable ()
        {
            var et = target as EngineTrack;
            et.getEditTime = GetTimelineEditTime;
            et.animEnv = et.timelineAsset.BindEngineObject ();
            if (et.animEnv != null)
            {
                et.animEnv.RefreshEditAnims ();
            }
        }
        private static float GetTimelineEditTime ()
        {
            return TimelineEditor.editorTime;
        }

        public override void OnInspectorGUI ()
        {
            var et = target as EngineTrack;
            serializedObject.Update ();
            if (et.animEnv != null)
            {
                et.infiniteTrackStyle = DirectorStyles.Instance.infiniteTrack;
                et.bgTex = DirectorStyles.Instance.bottomShadow.normal.background;
                et.keySize.x = DirectorStyles.Instance.keyframe.fixedWidth;
                et.keySize.y = DirectorStyles.Instance.keyframe.fixedHeight;
                et.bgColor = DirectorStyles.Instance.customSkin.colorInfiniteClipLine;
                et.keyframeStyle = DirectorStyles.Instance.keyframe;
                EditorGUI.BeginChangeCheck();
                et.animEnv.OnInspectorGUI ();
                if(EditorGUI.EndChangeCheck())
                {
                    et.animEnv.Update(TimelineEditor.editorTime);
                    TimelineWindow.instance.treeView.Refresh();
                }
            }
            serializedObject.ApplyModifiedProperties ();
        }
    }
}
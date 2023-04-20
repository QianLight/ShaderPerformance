using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
    public delegate void OnClose ();
    public class AnimClipEditor : EditorWindow
    {
        private AnimEditor m_AnimEditor;
        private OnClose closeEvent;
        public static void OpenAnimClipWindow (AnimationClip clip, OnClose closeEvent)
        {
            AnimClipEditor window = EditorWindow.GetWindow<AnimClipEditor> ("EnvCurve", true);
            window.name = "EnvCurve";
            window.position = new Rect (200, 700, 1000, 200);
            window.Init (clip,closeEvent);

            window.Show ();
        }
        static AnimationClipSelectionItem Create (AnimationClip animationClip)
        {
            AnimationClipSelectionItem selectionItem = CreateInstance (typeof (AnimationClipSelectionItem)) as AnimationClipSelectionItem;

            selectionItem.gameObject = null;
            selectionItem.scriptableObject = null;
            selectionItem.animationClip = animationClip;
            selectionItem.id = 0; // no need for id since there's only one item in selection.

            return selectionItem;
        }

        private void Init (AnimationClip clip, OnClose closeEvent)
        {
            if (m_AnimEditor == null)
                m_AnimEditor = CreateInstance (typeof (AnimEditor)) as AnimEditor;
            m_AnimEditor.state.frameRate = 30;
            m_AnimEditor.selection = Create (clip);
            this.closeEvent = closeEvent;
        }

        void OnDisable ()
        {
            if(closeEvent!=null)
            {
                closeEvent();
                closeEvent = null;
            }
        }

        void OnGUI ()
        {
            if (m_AnimEditor != null)
            {
                m_AnimEditor.OnAnimEditorGUI (this, position);
            }
        }
    }
}
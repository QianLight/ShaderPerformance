using UnityEngine.Playables;
using UnityEngine.Timeline;


namespace UnityEditor.Timeline.Signals
{
    [CustomEditor(typeof(JumpSignalEmmiter))]
    public class JumpSignalEditor : Editor
    {

        JumpSignalEmmiter signal;


        private void OnEnable()
        {
            signal = target as JumpSignalEmmiter;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            PlayableDirector dir = TimelineEditor.inspectedDirector;

            if (signal.jumpTime < 0)
            {
                EditorUtility.DisplayDialog("warn", "jump time can't set to less than zero", "OK");
                signal.jumpTime = 0;
            }

            var director = TimelineEditor.inspectedDirector;
            if (director != null)
            {
                double dur = director.duration;
                EditorGUILayout.LabelField("   jump time range: 0-" + dur.ToString("f2"));
                if (signal.jumpTime > dur)
                {
                    EditorUtility.DisplayDialog("warn", "jump time is more than max", "OK");
                    signal.jumpTime = (float)(dur - 1e-4);
                }
            }
        }

    }
}
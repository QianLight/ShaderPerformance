using UnityEngine.Timeline;


namespace UnityEditor.Timeline.Signals
{
    [CustomEditor(typeof(SlowSignalEmitter))]
    public class SlowSignalEditor : Editor
    {
        SlowSignalEmitter signal;


        private void OnEnable()
        {
            signal = target as SlowSignalEmitter;
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (signal.slowRate < 0)
            {
                EditorUtility.DisplayDialog("warn", "slow rate can't set to less than zero", "OK");
                signal.slowRate = 0;
            }

        }

    }

}
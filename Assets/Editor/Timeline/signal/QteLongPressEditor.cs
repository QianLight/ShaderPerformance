//using UnityEditor;
//using UnityEditor.Timeline;
//using UnityEngine.Playables;
//using UnityEngine;

////[CustomEditor(typeof(QteLongPressSignal))]
//public class QteLongPressEditor : Editor
//{
//    QteLongPressSignal signal;


//    private void OnEnable()
//    {
//        signal = target as QteLongPressSignal;
//    }


//    public override void OnInspectorGUI()
//    {
//        base.OnInspectorGUI();

//        EditorGUILayout.Space();
//        EditorGUILayout.LabelField("ERROR INFO:");

//        bool FIND = false;
//        if ( signal.failedTo < -1e-5)
//        {
//            FIND = true;
//            EditorGUILayout.HelpBox("The time(failed to) must be more than zero", MessageType.Warning);
//        }
//        if (signal.waitTime < 1e-5)
//        {
//            FIND = true;
//            EditorGUILayout.HelpBox("The time(wait time) must be less than zero", MessageType.Error);
//        }

//        if (signal.PressTime < 1e-5)
//        {
//            FIND = true;
//            EditorGUILayout.HelpBox("The time(press time) must be less than zero", MessageType.Error);
//        }

//        PlayableDirector dir = TimelineEditor.inspectedDirector;
//        if (dir)
//        {
//            if (signal.failedTo > dir.duration)
//            {
//                FIND = true;
//                EditorGUILayout.HelpBox("The time(failed to) must be more than zero, max value is "
//                    + dir.duration.ToString("f3"), MessageType.Error);
//            }
//        }

//        if (signal.backwardRate < 1e-5)
//        {
//            FIND = true;
//            EditorGUILayout.HelpBox("backward rate must be more than zero", MessageType.Error);
//        }

//        //if (signal.BtnPosition == Vector2.zero)
//        //{
//        //    FIND = true;
//        //    EditorGUILayout.HelpBox("Are you sure btn position is (0,0)", MessageType.Warning);
//        //}

//        if (!FIND)
//        {
//            EditorGUILayout.LabelField("  no error, no warn");
//        }
//    }


//}
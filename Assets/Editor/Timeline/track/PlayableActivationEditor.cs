using CFUtilPoolLib;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CFEngine;

namespace XEditor
{

    public class PlayableActivationEditor : PlayableBaseEditor
    {
        public PlayableActivationEditor (int indx, PlayableDirectorInspector parent) : base (indx,parent) { }

        public override void Reset () { }

        public override void OnInspectorGUI (PlayableBinding pb)
        {
            DirectorActiveBinding bind = editor.EdtData.GetNewActiveBinding (pb.streamName);
            EditorGUILayout.LabelField (pb.streamName, EditorStyles.boldLabel);
            bind.type = (PlayerableActiveType) EditorGUILayout.EnumPopup ("Type", bind.type);

            if (bind.type == PlayerableActiveType.Entity) // "Entity"
            {
                bind.val = EditorGUILayout.TextField ("entity id:", bind.val);
            }
            editor.EdtData.Set (pb.streamName, ref bind);
            EditorGUILayout.Space ();
        }

        // public override void UnloadRef (PlayableDirectorInspector editor, PlayableBinding pb)
        // {
        //     if (pb.sourceObject != null)
        //     {
        //         editor.Director.SetGenericBinding (pb.sourceObject, null);
        //     }
        // }
        public static void OnLoad (ref DirectorActiveBinding bind, DirectorTrackAsset track, ICallbackData cbData)
        { 

        }

    }
}
using A;
using CFEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine;

namespace UnityEditor.Timeline
{
    public class ExternalManipulator
    {

        private static void Delete()
        {
            var state =  TimelineWindow.instance.state;
            TimelineAction.Invoke<DeleteAction>(state);
        }

        public static void Delete(TimelineClip clip)
        {
            Selection.activeObject = clip.asset;
            Delete();
        }

        public static void Delete(TimelineClip[] clips)
        {
            Object[] objects =new Object[clips.Length];
            for(int i=0;i<clips.Length;i++)
            {
                objects[i] = EditorClipFactory.GetEditorClip(clips[i]);
            }
            Selection.objects = objects;
            Delete();
        }

        public static TimelineClip CreateClip(TrackAsset track, System.Type playableAssetType, double dur, double start)
        {
            var state = TimelineWindow.instance.state;
            var clip = TimelineHelpers.CreateClipOnTrack(playableAssetType, null, track, dur, state);
            clip.start = start;
            clip.duration = dur;
            return clip;
        }

    }
}

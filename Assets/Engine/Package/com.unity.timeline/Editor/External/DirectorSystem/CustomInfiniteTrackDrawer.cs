using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
    
    internal class CustomInfiniteTrackDrawer : InfiniteTrackDrawer
    {

        private WindowState state;
        private CustomInfiniteTrackDrawContext context;
        private ICustomInfiniteTrackDraw drawer;

        public CustomInfiniteTrackDrawer (TrackAsset track) : base (null)
        {
            drawer = track as ICustomInfiniteTrackDraw;
            context.getTimePixelCb = GetTimePixel;
            context.drawBackTextCb = DrawRecordBackground;
        }

        float GetTimePixel (double time)
        {
            if (state != null)
                return state.TimeToPixel (time);
            return -1;
        }
        public static void DrawRecordBackground (ref Rect trackRect, GUIContent recordingLabel)
        {
            var styles = DirectorStyles.Instance;

            Graphics.ShadowLabel (trackRect,
                DirectorStyles.Elipsify (recordingLabel.text, trackRect, styles.fontClip),
                styles.fontClip, Color.red, Color.black);
        }
        public override float GetHeightExt ()
        {
            return drawer != null?drawer.GetHeightExt () : 0;
        }
        public override bool DrawTrack (Rect trackRect, TrackAsset trackAsset, Vector2 visibleTime, WindowState state)
        {
            this.state = state;
            if (drawer != null)
            {
                drawer.DrawTrack (ref trackRect, ref context);
            }
            return true;
        }
        // public override void OnBuildTrackContextMenu (GenericMenu menu, TrackAsset track, WindowState state)
        // {
        //     base.OnBuildTrackContextMenu (menu, track, state);
        // }
    }
}
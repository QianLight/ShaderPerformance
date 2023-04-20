#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEditor;

namespace UnityEngine.Timeline
{

    [AttributeUsage (AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class CustomInfiniteTrackAttribute : Attribute { }
    public delegate float GetTimePixel (double time);
    public delegate void DrawBackText (ref Rect trackRect, GUIContent recordingLabel);

    public struct CustomInfiniteTrackDrawContext
    {
        public GetTimePixel getTimePixelCb;
        public DrawBackText drawBackTextCb;
    }
    public interface ICustomInfiniteTrackDraw
    {
        void DrawTrack (ref Rect trackRect, ref CustomInfiniteTrackDrawContext context);
        float GetHeightExt ();
    }
}
#endif
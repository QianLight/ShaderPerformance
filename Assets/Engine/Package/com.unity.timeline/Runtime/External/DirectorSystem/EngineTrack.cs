#if UNITY_EDITOR
using CFEngine;
using System;
using UnityEditor;

namespace UnityEngine.Timeline
{
    public delegate float GetTimelineEditTime ();
    [CustomInfiniteTrack, HideInMenu]
    public partial class EngineTrack : TrackAsset, ITimelineClipAsset, ICustomInfiniteTrackDraw
    {
        [NonSerialized]

        public IAnimEnv animEnv;
        [NonSerialized]

        public GetTimelineEditTime getEditTime;
        [NonSerialized]

        public GUIStyle infiniteTrackStyle;

        [NonSerialized]
        public GUIStyle keyframeStyle;
        [NonSerialized]
        public Texture2D bgTex;

        [NonSerialized]
        public Vector2 keySize;
        [NonSerialized]
        public Color bgColor;
        private GUIContent titleContent;
        public ClipCaps clipCaps { get { return ClipCaps.None; } }

        public override bool CanCompileClips ()
        {
            return true;
        }

        public float GetHeightExt ()
        {
            if (animEnv != null)
            {
                var editingAnims = animEnv.GetEditAnims ().Count - 1;
                if (editingAnims < 0)
                    editingAnims = 0;
                return editingAnims * 30.0f;
            }
            return 0;
        }

        public void DrawKeyFrame (ref Rect trackRect, AnimContextBase animContext, GetTimePixel getTimePixel,
            float fps, int frame)
        {
            for (int i = 0; i < animContext.Count; ++i)
            {
                float time = animContext.GetKeyTime (i);
                int f = Mathf.RoundToInt (time * fps);
                float x = getTimePixel (time);
                var bounds = new Rect (x, trackRect.yMin + 3.0f, 1.0f, trackRect.height - 6.0f);

                var iconWidth = keySize.x;
                var iconHeight = keySize.y;

                var keyframeRect = bounds;
                keyframeRect.width = iconWidth;
                keyframeRect.height = iconHeight;
                keyframeRect.xMin -= iconWidth / 2.0f;
                keyframeRect.yMin = trackRect.yMin + ((trackRect.height - iconHeight) / 2.0f);
                // Color c = GUI.color;
                // GUI.color = keyColor;
                GUI.Label (keyframeRect, GUIContent.none, keyframeStyle);
                // GUI.color = c;
                EditorGUI.DrawRect (bounds, bgColor);

            }
        }
        public void DrawTrack (ref Rect trackRect, ref CustomInfiniteTrackDrawContext context)
        {
            if (animEnv != null)
            {
                var editingAnims = animEnv.GetEditAnims ();
                if (editingAnims.Count > 0)
                {
                    var timelineAsset = this.timelineAsset;
                    if (timelineAsset != null)
                    {
                        if (titleContent == null)
                        {
                            titleContent = new GUIContent ("Param");
                        }
                        float fps = timelineAsset.editorSettings.fps;
                        float deltaTime = 1.0f / fps;
                        float editTime = getEditTime != null?getEditTime () : 0;
                        animEnv.SetEditTime (editTime);
                        int frame = Mathf.RoundToInt (editTime * fps);
                        Vector2 clickpos = Vector2.zero;
                        var e = Event.current;
                        bool delete = false;
                        if (e.type == EventType.MouseDown)
                        {
                            if (e.clickCount > 1 && e.button == 0)
                            {
                                //EditorUtility.DisplayDialog ("Delete", e.mousePosition.ToString (), "OK", "Cancel");
                                clickpos = e.mousePosition;
                            }
                        }
                        else if (e.type == EventType.KeyDown)
                        {
                            if (e.keyCode == KeyCode.D)
                            {
                                delete = true;
                                clickpos = e.mousePosition;
                            }
                        }
                        var shadowRect = trackRect;
                        shadowRect.yMin = shadowRect.yMax;
                        shadowRect.height = 15.0f;
                        GUI.DrawTexture (shadowRect, bgTex, ScaleMode.StretchToFill);
                        var extraRect = trackRect;
                        for (int i = 0; i < editingAnims.Count; ++i)
                        {
                            var item = editingAnims[i];
                            if (extraRect.Contains (clickpos))
                            {
                                if (delete)
                                {
                                    item.DeleteKey (frame * deltaTime);
                                }
                                else
                                {
                                    item.AddKey (frame * deltaTime);
                                }

                            }
                            GUI.Box (extraRect, GUIContent.none, infiniteTrackStyle != null?infiniteTrackStyle : GUIStyle.none);
                            titleContent.text = item.name;
                            context.drawBackTextCb (ref extraRect, titleContent);
                            DrawKeyFrame (ref extraRect, item, context.getTimePixelCb, fps, frame);
                            extraRect.y += 30;
                        }
                    }

                }

            }
        }

    }
}
#endif
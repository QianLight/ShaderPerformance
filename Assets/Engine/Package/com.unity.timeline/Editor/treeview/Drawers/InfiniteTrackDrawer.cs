using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
    internal class InfiniteTrackDrawer : TrackDrawer
    {
        readonly IPropertyKeyDataSource m_DataSource;
        Rect m_TrackRect, m_selectRect;
        bool m_select;

        public InfiniteTrackDrawer(IPropertyKeyDataSource dataSource)
        {
            m_selectRect = Rect.zero;
            m_DataSource = dataSource;

            var context = TimelineWindow.instance.keyContext;
            context.cbRect -= OnSelectRect;
            context.cbRect += OnSelectRect;
            context.cbReset -= OnReset;
            context.cbReset += OnReset;
            context.cbDrag -= DragSelect;
            context.cbDrag += DragSelect;
        }

        public bool CanDraw(TrackAsset track, WindowState state)
        {
            var keys = m_DataSource.GetKeys();
            var isTrackEmpty = track.clips.Length == 0;

            return keys != null || (state.IsArmedForRecord(track) && isTrackEmpty);
        }

        static void DrawRecordBackground(Rect trackRect)
        {
            var styles = DirectorStyles.Instance;

            EditorGUI.DrawRect(trackRect, styles.customSkin.colorInfiniteTrackBackgroundRecording);

            Graphics.ShadowLabel(trackRect,
                DirectorStyles.Elipsify(DirectorStyles.recordingLabel.text, trackRect, styles.fontClip),
                styles.fontClip, Color.white, Color.black);
        }

        public virtual float GetHeightExt() { return 0; }

        public override bool DrawTrack(Rect trackRect, TrackAsset trackAsset, Vector2 visibleTime, WindowState state)
        {
            m_TrackRect = trackRect;

            if (!CanDraw(trackAsset, state))
                return true;

            if (state.recording && state.IsArmedForRecord(trackAsset))
                DrawRecordBackground(trackRect);

            GUI.Box(trackRect, GUIContent.none, DirectorStyles.Instance.infiniteTrack);

            var shadowRect = trackRect;
            shadowRect.yMin = shadowRect.yMax;
            shadowRect.height = 15.0f;
            GUI.DrawTexture(shadowRect, DirectorStyles.Instance.bottomShadow.normal.background, ScaleMode.StretchToFill);

            if (!m_select)
            {
                var keys = m_DataSource.GetKeys();
                if (keys != null && keys.Length > 0)
                {
                    for (int i = 0; i < keys.Length; i++)
                        DrawKeyFrame(keys[i], state, false);
                }
            }
            else
            {
                for (int i = 0; i < keys_run.Length; i++)
                    DrawKeyFrame(keys_run[i], state, keys_select[i]);
            }
            return true;
        }

        void DrawKeyFrame(float key, WindowState state, bool select)
        {
            var x = state.TimeToPixel(key);
            var bounds = new Rect(x, m_TrackRect.yMin + 3.0f, 1.0f, m_TrackRect.height - 6.0f);

            if (!m_TrackRect.Overlaps(bounds))
                return;

            var iconWidth = DirectorStyles.Instance.keyframe.fixedWidth;
            var iconHeight = DirectorStyles.Instance.keyframe.fixedHeight;

            var keyframeRect = bounds;
            keyframeRect.width = iconWidth;
            keyframeRect.height = iconHeight;
            keyframeRect.xMin -= iconWidth / 2.0f;
            keyframeRect.yMin = m_TrackRect.yMin + ((m_TrackRect.height - iconHeight) / 2.0f);

            Vector2 c = new Vector2(x, m_TrackRect.yMax + m_TrackRect.height / 2);

            // case 890650 : Make sure to use GUI.Label and not GUI.Box since the number of key frames can vary while dragging keys in the inline curves causing hotControls to be desynchronized
            var context = TimelineWindow.instance.keyContext;

            GUI.Label(keyframeRect, GUIContent.none, DirectorStyles.Instance.keyframe);

            if (select)
            {
                EditorGUI.DrawRect(bounds, Color.gray);
            }
            else
            {
                EditorGUI.DrawRect(bounds, DirectorStyles.Instance.customSkin.colorInfiniteClipLine);
            }
        }

        float[] keys_bef, keys_run;
        bool[] keys_select;

        void OnSelectRect(Rect rect)
        {
            var state = TimelineWindow.instance.state;
            m_selectRect = rect;
            if (!m_select)
            {
                if (m_DataSource == null) return;
                
                var keys = m_DataSource.GetKeys();
                if (keys != null)
                {
                    int len = keys.Length;
                    keys_bef = new float[len];
                    keys_run = new float[len];
                    keys_select = new bool[len];
                    for (int i = 0; i < len; i++)
                    {
                        var x = state.TimeToPixel(keys[i]);
                        Vector2 c = new Vector2(x, m_TrackRect.yMax + m_TrackRect.height / 2);
                        keys_select[i] = m_selectRect.Contains(c);
                        keys_run[i] = keys[i];
                        keys_bef[i] = keys[i];
                    }
                    m_select = true;
                }
            }
            else
            {
                m_select = false;
                ShiftAnim();
                keys_bef = null;
                keys_run = null;
                keys_select = null;
            }
        }

        private void ShiftAnim()
        {
            var state = TimelineWindow.instance.state;
            var ctx = TimelineWindow.instance.keyContext;
            var aniTr = m_DataSource as AnimationTrackKeyDataSource;
            if (aniTr != null)
            {
                var clip = aniTr.exposedClip;
                if (clip)
                {
                    int len = keys_run.Length;
                    var binds = AnimationUtility.GetCurveBindings(clip);
                    foreach (var bind in binds)
                    {
                        AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, bind);
                        var keys = curve.keys;
                        for (int i = 0; i < keys.Length; i++)
                        {
                            var key = curve.keys[i];
                            float t = key.time;
                            for (int j = 0; j < len; j++)
                            {
                                if (Mathf.Abs(t - keys_bef[j]) < 1e-3)
                                {
                                    if (keys_select[j])
                                        key.time = keys_run[j];
                                    break;
                                }
                            }
                            keys[i] = key;
                        }
                        curve.keys = keys;
                        AnimationUtility.SetEditorCurve(clip, bind, curve);
                    }
                }
            }
        }

        void OnReset()
        {
            m_select = false;
            m_selectRect = Rect.zero;
        }

        void DragSelect(float x)
        {
            if (m_select)
            {
                if (keys_run != null)
                {
                    var state = TimelineWindow.instance.state;
                    int len = keys_run.Length;
                    for (int i = 0; i < len; i++)
                    {
                        if (keys_select[i])
                        {
                            var px = state.TimeToPixel(keys_run[i]);
                            px += x;
                            keys_run[i] = state.PixelToTime(px);
                        }
                    }
                }
            }
        }

    }

}
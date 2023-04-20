using UnityEngine;


namespace UnityEditor.Timeline
{
    public class TimelineExternGUI
    {

        private static Texture2D _envIco;

        public static Texture2D envIco
        {
            get
            {
                if (_envIco == null)
                {
                    string pat = "Assets/Editor/Timeline/res/RenderLayer.png";
                    _envIco = AssetDatabase.LoadAssetAtPath<Texture2D>(pat);
                }
                return _envIco;
            }
        }

        internal static bool showEnv = true;
        internal const int wt = 18;

        public static void ShowEnvGUI()
        {
            EditorGUI.BeginChangeCheck();
            {
                showEnv = GUILayout.Toggle(showEnv, envIco, GUI.skin.button, GUILayout.MaxWidth(26), GUILayout.MaxHeight(24));
            }
            if (EditorGUI.EndChangeCheck())
            {
                TimelineWindow.instance.treeView.Reload();
            }
        }


        public static bool PreviewCameraEndBlend { get; set; }

        internal static float EndDuration { get; set; }

        public static void SetBlendWrap(bool showBlend, float endDuration)
        {
            PreviewCameraEndBlend = showBlend;
            EndDuration = endDuration;
        }

        internal static int endFrame = 0;
        internal static float endTime = 0;
        internal static float deltaFrame = 0;
        internal static void EndPreviewGUI(WindowState state)
        {
            if (PreviewCameraEndBlend)
            {
                GUILayout.Toggle(false, "B", GUI.skin.button, GUILayout.MaxWidth(24), GUILayout.MaxHeight(24));
                var director = state.masterSequence.director;
                if (director == null) director = state.referenceSequence.director;
                if (director)
                {
                    if (director.time >= director.duration)
                    {
                        if (endFrame == 0)
                        {
                            endFrame = state.masterSequence.frame;
                            deltaFrame = 1 / state.masterSequence.frameRate;
                        }
                        if (endTime == 0 || Time.time - endTime >= deltaFrame)
                        {
                            director.time = endFrame * deltaFrame;
                            if (endTime > 0) director.Pause();
                            state.Evaluate();
                            state.InvokeTimeChangeCallback();
                            endFrame++;
                            endTime = Time.time;
                        }
                    }
                    if (director.time - director.duration > EndDuration)
                    {
                        endTime = 0;
                        endFrame = 0;
                        director.time = 0;
                        state.masterSequence.time = 0.0;
                        state.Pause();
                    }
                }
            }
        }

    }
}
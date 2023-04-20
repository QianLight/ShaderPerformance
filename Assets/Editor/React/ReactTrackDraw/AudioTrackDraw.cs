#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using CFUtilPoolLib;

namespace XEditor {


    public class AudioTrackDraw : BaseTrackDraw
    {
        public int selectTrack = -1;
        public XAudioData selectData = null;
        public XAudioDataExtra selectDataExtra = null;

        public override string Name { get { return "Audio"; } }

        public override void OnDrawLeft()
        {
            var ReactDataSet = Window.ReactDataSet;
            if (ReactDataSet.ReactData.Audio != null && ReactDataSet.ReactData.Audio.Count > 0)
            {
                for (int i = 0; i < ReactDataSet.ReactData.Audio.Count; ++i)
                {
                    var curData = ReactDataSet.ReactData.Audio[i];
                    curData.Index = i;

                    GUI.color = selectTrack == i ? Window.selectdColor : Window.trackColor;
                    GUILayout.BeginHorizontal("box");
                    {
                        GUILayout.Space(30);
                        if (GUILayout.Button(curData.Clip, Window.trackStyle))
                        {
                            selectTrack = i;

                            selectData = curData;
                            selectDataExtra = ReactDataSet.ReactDataExtra.Audio[i];
                        }

                        GUI.color = Window.trackDropColor;
                        if (GUILayout.Button(" Del ", Window.trackDropStyle))
                        {
                            selectTrack = -1;
                            selectData = null;
                            selectDataExtra = null;

                            //del
                            ReactDataSet.ReactData.Audio.RemoveAt(i);
                            ReactDataSet.ReactDataExtra.Audio.RemoveAt(i);
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                //auto select.
                if (selectTrack == -1 && ReactDataSet.ReactData.Audio.Count > 0)
                {
                    selectTrack = 0;
                    selectData = ReactDataSet.ReactData.Audio[0];
                    selectDataExtra = ReactDataSet.ReactDataExtra.Audio[0];
                }
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add(+)", GUILayout.Width(60), GUILayout.Height(30)))
                {
                    Add();
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            GUI.color = ReactCommon.InitColor;
        }

        public override void Add()
        {
            var ReactDataSet = Window.ReactDataSet;
            ReactDataSet.ReactData.Audio.Add(new XAudioData());
            ReactDataSet.ReactDataExtra.Audio.Add(new XAudioDataExtra());
        }

        public override string GetCount()
        {
            var ReactDataSet = Window.ReactDataSet;
            if (ReactDataSet == null) return "";
            if (ReactDataSet.ReactData.Audio != null && ReactDataSet.ReactData.Audio.Count > 0)
                return string.Format(" ({0})", ReactDataSet.ReactData.Audio.Count);
            return "";
        }

        public override void OnDrawRight()
        {
            if (null == selectData || null == selectDataExtra) return;

            EditorGUILayout.BeginHorizontal();
            {
                selectData.Clip = EditorGUILayout.TextField("Clip Name", selectData.Clip);
            }
            EditorGUILayout.EndHorizontal();

            float audio_at = Window.ReactDataSet.ReactDataExtra.ReactClip_Frame * selectDataExtra.Ratio;
            EditorGUILayout.BeginHorizontal();
            audio_at = EditorGUILayout.FloatField("Play At ", audio_at);
            GUILayout.Label("(frame)", ReactCommon.NormalLabelStyle);
            GUILayout.Label("", GUILayout.MaxWidth(30));
            EditorGUILayout.EndHorizontal();

            selectDataExtra.Ratio = Window.ReactDataSet.ReactDataExtra.ReactClip_Frame == 0 ? 0 : audio_at / Window.ReactDataSet.ReactDataExtra.ReactClip_Frame;
            if (selectDataExtra.Ratio > 1) selectDataExtra.Ratio = 1;

            EditorGUILayout.BeginHorizontal();
            selectDataExtra.Ratio = EditorGUILayout.Slider("Ratio", selectDataExtra.Ratio, 0, 1);
            GUILayout.Label("(0~1)", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            selectData.At = (selectDataExtra.Ratio * Window.ReactDataSet.ReactDataExtra.ReactClip_Frame) * ReactCommon.FRAME_RATIO;

            selectData.Channel = (AudioChannel)EditorGUILayout.EnumPopup("Channel", selectData.Channel);

            selectData.BlowEnergy = EditorGUILayout.Toggle("BlowEnergy", selectData.BlowEnergy);
        }
    }
}

#endif
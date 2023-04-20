using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace XEditor
{
    [CustomEditor(typeof(UIPlayableTrack))]
    public class UIPlayableTrackInspector : Editor
    {
        UIPlayableTrack track;
        PlayableDirector director;

        class Line
        {
            public string key;
            public string content;
        }

        List<Line> table = new List<Line>();


        private void OnEnable()
        {
            track = target as UIPlayableTrack;
            director = GameObject.FindObjectOfType<PlayableDirector>();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("对应表格fade.txt", MessageType.Info);


            GUILayout.BeginHorizontal();
            if (GUILayout.Button("import"))
            {
                Read();
                Import();
            }
            GUILayout.EndHorizontal();
        }

        private void Import()
        {
            var clips = track.GetClips();

            foreach (var clip in clips)
            {
                UIFadeAsset uIFade = clip.asset as UIFadeAsset;
                if (uIFade != null)
                {
                    var it = table.Find(x => x.key.Trim().Equals(uIFade.key));
                    if (it != null)
                    {
                        uIFade.content = it.content;
                    }
                }
            }
        }

        private void Read()
        {
            using (FileStream fs = new FileStream(OriginalSetting.fadePat, FileMode.Open))
            {
                table.Clear();
                StreamReader reader = new StreamReader(fs, Encoding.Unicode);
                string line = reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    Line row = new Line();
                    string[] a = line.Split('\t');
                    row.key = a[0];
                    row.content = a[1];
                    table.Add(row);
                }
            }
        }

    }
}
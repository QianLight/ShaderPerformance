using System;
using System.Collections.Generic;
using CFUtilPoolLib;
using System.IO;
using System.Text;
using System.Linq;

namespace XEditor
{
    struct ActorLine
    {
        public int idx;
        public string title;
        public double start, dur;
        public string timeline;
        public int type;
        public string speaker;
        public string m_head;
        public string m_face;
        public bool m_isPause;
        public bool m_blackVisible;
        public bool m_isEmpty;
        public string m_fadeInClip;
        public string m_fadeOutClip;
        public string m_bgPath;
        public bool m_autoEnable;
        public bool m_skipEnable;
    }

    class ActorLinesTable : XSingleton<ActorLinesTable>
    {
        public List<ActorLine> table = new List<ActorLine>();

        private int Sort(ActorLine x, ActorLine y)
        {
            if (x.timeline.Equals(y.timeline))
            {
                //if(x.type == y.type)
                //{
                //    return x.idx.CompareTo(y.idx);
                //}
                //else
                //{
                //    return x.type.CompareTo(y.type);
                //}
                return x.start.CompareTo(y.start); //按照时间顺序来排序，读起来连贯
            }
            return x.timeline.CompareTo(y.timeline);
        }

        public void Read(string path)
        {
            if (File.Exists(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    table.Clear();
                    StreamReader reader = new StreamReader(fs, Encoding.Unicode);
                    string line = reader.ReadLine();
                    while ((line = reader.ReadLine()) != null)
                    {
                        ActorLine row = new ActorLine();
                        string[] a = line.Split('\t');
                        row.timeline = a[0];
                        row.idx = int.Parse(a[1]);
                        row.start = float.Parse(a[2]);
                        row.dur = float.Parse(a[3]);
                        row.title = a[4];
                        row.type = int.Parse(a[5]);
                        row.speaker = a[6];
                        row.m_head = a[7];
                        row.m_face = a[8];
                        row.m_isPause = int.Parse(a[9]) > 0;
                        row.m_blackVisible = int.Parse(a[10]) > 0;
                        row.m_isEmpty = a.Length >= 12 ? int.Parse(a[11]) > 0 : false;
                        row.m_fadeInClip = a[12];
                        row.m_fadeOutClip = a[13];
                        row.m_bgPath = a[14];
                        row.m_autoEnable = string.IsNullOrEmpty(a[15]) ? false : int.Parse(a[15]) > 0;
                        row.m_skipEnable = string.IsNullOrEmpty(a[16]) ? false : int.Parse(a[16]) > 0;
                        table.Add(row);
                    }
                }
            }
        }

        public void Write(string path)
        {
            using (FileStream writer = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
            {
                StreamWriter sw = new StreamWriter(writer, Encoding.Unicode);

                sw.WriteLine("timeline\tindex\tstart\tduration\ttitle\ttype\tspeaker\thead\tface\tisPause\tblackVisible\tisEmpty\tinclip\toutclip\tbgpath\tauto\tskip");

                table.Sort(Sort);
                for (int i = 0; i < table.Count; i++)
                {
                    ActorLine line = table[i];
                    sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}\t{14}\t{15}\t{16}",
                        line.timeline,
                        line.idx,
                        (line.start).ToString("f2"),
                        (line.dur).ToString("f2"),
                        line.title,
                        line.type,
                        line.speaker,
                        line.m_head,
                        line.m_face,
                        line.m_isPause ? 1 : 0,
                        line.m_blackVisible ? 1 : 0,
                        line.m_isEmpty ? 1 : 0,
                        line.m_fadeInClip,
                        line.m_fadeOutClip,
                        line.m_bgPath,
                        line.m_autoEnable ? 1 : 0,
                        line.m_skipEnable ? 1 : 0);
                }
                sw.Close();
            }
        }

        public void AddLine(ActorLine line)
        {
            table.Add(line);
        }

        public void RemoveItem(string timelineName, int type)
        {
            table.RemoveAll(x => x.timeline == timelineName && x.type == type);
        }

        public ActorLine[] GetItem(string timeline, int type)
        {
            return table.Where(x => x.timeline == timeline && x.type == type).ToArray();
        }
    }
}

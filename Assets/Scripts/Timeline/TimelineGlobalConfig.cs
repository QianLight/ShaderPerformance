using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TimelineGlobalConfig
{
    public string m_path = "Assets/Editor/TimelineConfig.txt";
    private static TimelineGlobalConfig m_instance;
    public static TimelineGlobalConfig Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new TimelineGlobalConfig();
            }
            return m_instance;
        }
    }

    public Dictionary<string, string> m_keyValues;

    public void ReadConfig()
    {
        //if (m_keyValues != null) return;
        if (m_keyValues != null) m_keyValues.Clear();
        else
        {
            m_keyValues = new Dictionary<string, string>();
        }
        using (FileStream fs = new FileStream(m_path, FileMode.Open))
        {
            StreamReader reader = new StreamReader(fs, Encoding.UTF8);
            string line = reader.ReadLine();
            while ((line = reader.ReadLine()) != null)
            {
                string[] strs = line.Split('\t');
                m_keyValues.Add(strs[0], strs[1]);
            }
        }
    }
}
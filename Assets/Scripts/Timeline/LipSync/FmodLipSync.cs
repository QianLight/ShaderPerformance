//#if FMOD_LIVEUPDATE
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEditor;


namespace LipSync
{
    public struct Record
    {
        public char ch;
        public float[] blend;

        public override string ToString()
        {
            string b = "";
            int last = blend.Length - 1;
            for (int i = 0; i < last; i++)
            {
                b += blend[i].ToString("f3") + '\t';
            }
            b += blend[last].ToString("f3");
            return ch.ToString() + '\t' + b;
        }
    }

    public class FmodLipSync : LipSync
    {
        public const string recdPat = "Assets/Editor/Timeline/res/recd.txt";
        public StudioEventEmitter emiter;
        public bool save;
        private int rate;
        private float audioLen;
        FMOD.DSP m_FFTDsp;
        FMOD.ChannelGroup master;
        FMOD.DSP mixerHead;

        List<Record> records = new List<Record>();

        void Start()
        {
            Application.targetFrameRate = 60;
            InitializeRecognizer();
            InitDsp();
        }

        void InitDsp()
        {
            RuntimeManager.CoreSystem.createDSPByType(FMOD.DSP_TYPE.FFT, out m_FFTDsp);
            m_FFTDsp.setParameterInt((int)FMOD.DSP_FFT.WINDOWTYPE, (int)FMOD.DSP_FFT_WINDOW.HANNING);
            m_FFTDsp.setParameterInt((int)FMOD.DSP_FFT.WINDOWSIZE, windowSize);
            RuntimeManager.CoreSystem.getMasterChannelGroup(out master);
            var m_Result = master.addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, m_FFTDsp);
            m_Result = master.getDSP(0, out mixerHead);
            mixerHead.setMeteringEnabled(true, true);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                records.Clear();
                emiter.Play();
                int len;
                emiter.EventDescription.getLength(out len);
                audioLen = len / 1000.0f;
                FMOD.SPEAKERMODE mode;
                int raw;
                RuntimeManager.CoreSystem.getSoftwareFormat(out rate, out mode, out raw);
                records.Clear();
            }
            if (emiter.IsPlaying())
            {
                recognizeResult = runtimeRecognizer.RecognizeByAudioSource(m_FFTDsp, rate);
                RecordingFmod(recognizeResult);
                UpdateForward();
            }
            else if (records.Count > 0)
            {
                OutputRecd();
                records.Clear();
#if UNITY_EDITOR
                AssetDatabase.Refresh();
                Debug.Log("output recd finish");
#endif
            }
        }

        private void OnGUI()
        {
            if(!string.IsNullOrEmpty(recognizeResult))
            {
                GUI.Label(new Rect(20, 20, 100, 60), recognizeResult);
            }
        }


        void RecordingFmod(string word)
        {
            char ch = string.IsNullOrEmpty(word) ? '-' : word[0];
            var rcd = new Record() { ch = ch };
            int cnt = currentVowels.Length;
            rcd.blend = new float[cnt];
            for (int i = 0; i < cnt; i++)
            {
                rcd.blend[i] = targetBlendShapeObject.GetBlendShapeWeight(propertyIndexs[i]);
            }
            records.Add(rcd);
        }

        /// <summary>
        /// 运行时导出数据
        /// </summary>
        void OutputRecd()
        {
            if (save)
            {
                using (FileStream writer = new FileStream(recdPat, FileMode.Create, FileAccess.ReadWrite))
                {
                    StreamWriter sw = new StreamWriter(writer, Encoding.Unicode);
                    sw.WriteLine(audioLen);
                    sw.WriteLine(CombineVowels());
                    for (int i = 0; i < records.Count; i++)
                    {
                        sw.WriteLine(records[i]);
                    }
                    sw.Close();
                }
            }
        }

        string CombineVowels()
        {
            string ret = "";
            int i = 0;
            foreach(var it in propertyNames)
            {
                ret += it;
                if (++i != vowelToIndexDict.Count) ret += "\t";
            }
            return ret;
        }

    }
}

//#endif

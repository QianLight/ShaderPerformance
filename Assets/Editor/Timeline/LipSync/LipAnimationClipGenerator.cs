using FMODUnity;
using LipSync;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class LipAnimationClipGenerator : EditorWindow
{
    #region record

    public struct Record
    {
        public float frameTime;
        public char vowel;
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
            return frameTime.ToString("f6") + '\t' + vowel.ToString() + '\t' + b;
        }
    }

    public StudioEventEmitter emiter;
    public bool save;
    private int rate;
    private float m_audioLength;
    FMOD.DSP m_FFTDsp;
    FMOD.ChannelGroup master;
    FMOD.DSP mixerHead;
    List<Record> m_records = new List<Record>();

    public static string[] m_vowelsJP = { "a", "i", "u", "e", "o" };
    public static string[] vowelsCN = { "a", "e", "i", "o", "u", "v" };
    private static string[] m_variables = new string[] { "A", "E", "I", "O", "U" };
    protected const int MAX_BLEND_VALUE_COUNT = 6;
    public ERecognizerLanguage m_language;
    public SkinnedMeshRenderer targetBlendShapeObject;
    public string[] propertyNames;// = new string[MAX_BLEND_VALUE_COUNT];
    public float propertyMinValue = 0.0f;
    public float propertyMaxValue = 100.0f;
    public int windowSize = 1024;
    public float amplitudeThreshold = 0.01f;
    public float moveTowardsSpeed = 8;
    protected LipSyncRuntimeRecognizer m_runtimeRecognizer;
    protected string[] m_currentVowels;
    protected Dictionary<string, int> m_vowelToIndexDict = new Dictionary<string, int>();
    protected int[] propertyIndexs = new int[MAX_BLEND_VALUE_COUNT];
    protected float blendValuesSum;
    protected string recognizeResult;
    protected float[] targetBlendValues = new float[MAX_BLEND_VALUE_COUNT];
    protected float[] currentBlendValues = new float[MAX_BLEND_VALUE_COUNT];


    private void Init()
    {
        Application.targetFrameRate = 60;
        propertyNames = m_vowelsJP;
        m_language = ERecognizerLanguage.Japanese;
        InitializeRecognizer();
        InitDsp();
        //m_eventList.Clear();
    }

    public void InitializeRecognizer()
    {
        switch (m_language)
        {
            case ERecognizerLanguage.Japanese:
                m_currentVowels = m_vowelsJP;
                break;
            case ERecognizerLanguage.Chinese:
                m_currentVowels = vowelsCN;
                break;
        }
        for (int i = 0; i < m_currentVowels.Length; ++i)
        {
            m_vowelToIndexDict[m_currentVowels[i]] = i;
        }
        m_runtimeRecognizer = new LipSyncRuntimeRecognizer(m_language, windowSize, amplitudeThreshold);
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

    protected void UpdateForward()
    {
        for (int i = 0; i < targetBlendValues.Length; ++i)
        {
            targetBlendValues[i] = 0.0f;
        }
        if (recognizeResult != null)
        {
            targetBlendValues[m_vowelToIndexDict[recognizeResult]] = 1.0f;
        }

        for (int k = 0; k < currentBlendValues.Length; ++k)
        {
            if (propertyIndexs[k] != -1)
            {
                currentBlendValues[k] = Mathf.MoveTowards(currentBlendValues[k], targetBlendValues[k], moveTowardsSpeed * Time.deltaTime);
                //targetBlendShapeObject.SetBlendShapeWeight(propertyIndexs[k], Mathf.Lerp(propertyMinValue, propertyMaxValue, currentBlendValues[k]));
            }
        }
    }

    void RecordingFmod(string word)
    {
        char ch = string.IsNullOrEmpty(word) ? '-' : word[0];
        Record record = new Record() { vowel = ch };
        int count = m_currentVowels.Length;
        record.blend = new float[count];
        for (int i = 0; i < count; i++)
        {
            //rcd.blend[i] = targetBlendShapeObject.GetBlendShapeWeight(propertyIndexs[i]);
            record.blend[i] = currentBlendValues[i];
        }
        record.frameTime = Time.time - m_eventStartTime;
        m_records.Add(record);
    }

    /// <summary>
    /// 运行时导出数据
    /// </summary>
    void GenerateAnimationiClip()
    {
        //WriteRecordResult();
        GenerateAnimationClip();
    }

    private void GenerateAnimationClip()
    {
        AnimationClip clip = new AnimationClip();
        clip.legacy = false;
        int vowelCount = m_currentVowels.Length;
        int recordCount = m_records.Count;
        for (int i = 0; i < vowelCount; i++)
        {
            AnimationCurve curve = new AnimationCurve();
            for (int j = 0; j < recordCount; j++)
            {
                float t = m_records[j].frameTime;
                float v = m_records[j].blend[i];

                // optimus for removing repeated frame
                if (j > 0 && j < recordCount - 2) if (m_records[j - 1].blend[i] == v && v == m_records[j + 1].blend[i])
                        continue;

                Keyframe frame = new Keyframe(t, v, 0, 0);
                curve.AddKey(frame);
            }
            clip.SetCurve(string.Empty, typeof(FacialExpressionCurve), m_variables[i], curve);
        }

        string eventShortName = string.Empty;
        int index = m_currentEventName.LastIndexOf('/'); ////event:/Cutscene/Chapter_1/1000102
        eventShortName = m_currentEventName.Substring(index + 1);
        string path = "Assets/BundleRes/Animation/FacialMouth/" + eventShortName + ".anim";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        AssetDatabase.CreateAsset(clip, path);
    }

    private void WriteRecordResult()
    {
        string fileName = string.Format(recordPath, m_index);
        using (FileStream writer = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
        {
            StreamWriter sw = new StreamWriter(writer, Encoding.Unicode);
            sw.WriteLine(m_audioLength);
            sw.WriteLine(CombineVowels());
            for (int i = 0; i < m_records.Count; i++)
            {
                sw.WriteLine(m_records[i]);
            }
            sw.Close();
        }
    }

    string CombineVowels()
    {
        string ret = "";
        int i = 0;
        foreach (var it in propertyNames)
        {
            ret += it;
            if (++i != m_vowelToIndexDict.Count) ret += "\t";
        }
        return ret;
    }
    #endregion

    private FMOD.Studio.EventInstance m_eventInstance;
    private FMOD.Studio.EventDescription m_eventDescription;
    public const string recordPath = "Assets/Editor/Timeline/res/record{0}.txt";
    //private string[] m_events = new string[] { "event:/Cutscene/Chapter_1/1000101", "event:/Cutscene/Chapter_1/1000102" }; //test
    private List<string> m_eventList = new List<string>();
    private static int m_index;
    private float m_eventStartTime;
    private string m_currentEventName = string.Empty;
    private string m_eventPrefix = string.Empty;
    private static bool m_inited = false;

    //ui
    public Vector2 m_eventScroll = Vector2.zero;

    /// <summary>
    /// 导出音频的AnimationClip
    /// </summary>
    [MenuItem("Tools/Timeline/GenerateLipClips")]
    static void InitWindow()
    {
        EditorWindow.GetWindow(typeof(LipAnimationClipGenerator), true, "GenerateLipClips");
        m_inited = false;
        Application.targetFrameRate = 60;
    }

    public void OnGUI()
    {
        GUILayout.BeginVertical();
        EditorGUILayout.Space();
        if (GUILayout.Button("Cope", GUILayout.MaxWidth(160)))
        {
            if(!m_inited)
            {
                m_inited = true;
                Init();
                FMOD.SPEAKERMODE mode;
                int raw;
                RuntimeManager.CoreSystem.getSoftwareFormat(out rate, out mode, out raw);
            }

            m_index = 0;
            PlayEvent();
        }

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("event prefix", GUILayout.Width(100));
        m_eventPrefix = EditorGUILayout.TextField(m_eventPrefix, GUILayout.Width(200));
        if (GUILayout.Button("search", GUILayout.Width(100)))
        {
            SearchEvents();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("eventCache", GUILayout.Width(100));
        EditorGUILayout.ObjectField(string.Empty, EventManager.eventCache, typeof(EventCache), true, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("evenList", GUILayout.Width(502));
        if (GUILayout.Button("removeAll", GUILayout.Width(100)))
        {
            RemoveAll();
        }
        if (GUILayout.Button("add", GUILayout.Width(100)))
        {
            AddEvent();
        }
        EditorGUILayout.EndHorizontal();

        m_eventScroll = GUILayout.BeginScrollView(m_eventScroll, GUILayout.Width(800), GUILayout.Height(500));
        for (int i = 0; i < m_eventList.Count; ++i)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("event" + i, GUILayout.Width(100));
            m_eventList[i] = EditorGUILayout.TextField(m_eventList[i], GUILayout.Width(400));
            if (GUILayout.Button("remove", GUILayout.Width(100)))
            {
                RemoveEvent(i);
            }
            EditorGUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void AddEvent()
    {
        m_eventList.Insert(0, "event:/");
    }

    private void RemoveAll()
    {
        m_eventList.Clear();
    }

    private void RemoveEvent(int i)
    {
        if (i >= 0 && i < m_eventList.Count)
        {
            m_eventList.RemoveAt(i);
        }
    }

    private void SearchEvents()
    {
        m_eventList.Clear();
        List<EditorEventRef> editorEvents = EventManager.eventCache.EditorEvents;
        for (int i = 0; i < editorEvents.Count; ++i)
        {
            if (editorEvents[i].Path.StartsWith(m_eventPrefix))
            {
                m_eventList.Add(editorEvents[i].Path);
            }
        }
    }

    private void PlayEvent()
    {
        if (m_index >= m_eventList.Count)
        {
            AssetDatabase.Refresh();
            Debug.LogError("cope finish!");
            return;
        }
        m_currentEventName = m_eventList[m_index];
        Guid guid = PathToGUID(m_currentEventName);
        FMOD.RESULT result = RuntimeManager.StudioSystem.getEventByID(guid, out m_eventDescription);
        if (result != FMOD.RESULT.OK)
        {
            string.Format("FMOD Studio: Encounterd Error: {0} {1} {2}", result, FMOD.Error.String(result), m_currentEventName);
            return;
        }
        result = m_eventDescription.createInstance(out m_eventInstance);
        if (result != FMOD.RESULT.OK)
        {
            string.Format("FMOD Studio: Encounterd Error: {0} {1} {2}", result, FMOD.Error.String(result), m_currentEventName);
            return;
        }
        int len;
        m_eventDescription.getLength(out len);
        m_audioLength = len / 1000.0f;
        m_eventInstance.start();
        m_eventStartTime = Time.time;
    }

    private void Update()
    {
        if (IsPlaying())
        {
            if (m_runtimeRecognizer != null)
            {
                recognizeResult = m_runtimeRecognizer.RecognizeByAudioSource(m_FFTDsp, rate);
                RecordingFmod(recognizeResult);
                UpdateForward();
            }
        }
        else if (m_records.Count > 0)
        {
            GenerateAnimationiClip();
            m_records.Clear();
            Debug.LogError("output event succeed " + m_eventList[m_index]);

            // cope next event
            if(m_index < m_eventList.Count)
            {
                m_index++;
                PlayEvent();
            }
        }
    }

    public bool IsPlaying()
    {
        if (m_eventInstance.isValid() && m_eventInstance.isValid())
        {
            FMOD.Studio.PLAYBACK_STATE playbackState;
            m_eventInstance.getPlaybackState(out playbackState);
            return (playbackState != FMOD.Studio.PLAYBACK_STATE.STOPPED);
        }
        return false;
    }

    private static Guid PathToGUID(string path)
    {
        Guid guid = Guid.Empty;
        if (path.StartsWith("{"))
        {
            FMOD.Studio.Util.parseID(path, out guid);
        }
        else
        {
            var result = RuntimeManager.StudioSystem.lookupID(path, out guid);
            if (result == FMOD.RESULT.ERR_EVENT_NOTFOUND)
            {
                return new Guid();
            }
        }
        return guid;
    }
}

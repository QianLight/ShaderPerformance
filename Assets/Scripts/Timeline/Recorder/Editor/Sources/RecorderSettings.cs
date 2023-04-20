using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

namespace UnityEditor.Recorder
{

    [Flags]
    public enum ImageSource
    {
        /// <summary>
        /// Use the current active camera.
        /// </summary>
        ActiveCamera = 1,
        /// <summary>
        /// Use the main camera.
        /// </summary>
        MainCamera = 2,
        /// <summary>
        /// Use the first camera that matches a GameObject tag.
        /// </summary>
        TaggedCamera = 4
    }

    public enum FrameRatePlayback
    {
        /// <summary>
        /// The frame rate doesn't vary during recording, even if the actual frame rate is lower or higher.
        /// </summary>
        Constant,

        /// <summary>
        /// Use the application's frame rate, which might vary during recording. This option is not supported by all Recorders.
        /// </summary>
        Variable,
    }

    public enum RecordMode
    {
        /// <summary>
        /// Record every frame between when the recording is started and when it is stopped (either using the UI or through API methods).
        /// </summary>
        Manual,

        /// <summary>
        /// Record one single frame according to the specified frame number.
        /// </summary>
        SingleFrame,

        /// <summary>
        /// Record all frames within an interval of frames according to the specified Start and End frame numbers.
        /// </summary>
        FrameInterval,

        /// <summary>
        /// Record all frames within a time interval according to the specified Start time and End time.
        /// </summary>
        TimeInterval
    }

    public abstract class RecorderSettings : ScriptableObject
    {
        public static string OutputDir = "";
        public static string OutputName = "";

        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        [SerializeField] private bool enabled = true;

        protected internal abstract string Extension { get; }

        [SerializeField] internal int captureEveryNthFrame = 1;


        public RecordMode RecordMode { get; set; }

        public FrameRatePlayback FrameRatePlayback { get; set; }

        float frameRate = 30;
        public float FrameRate
        {
            get => frameRate;
            set => frameRate = value;
        }

        public int StartFrame { get; set; }

        public int EndFrame { get; set; }

        public float StartTime { get; set; }

        public float EndTime { get; set; }

        public bool CapFrameRate { get; set; }

        protected internal virtual bool ValidityCheck(List<string> errors)
        {
            var ok = true;

            if (InputsSettings != null)
            {
                var inputErrors = new List<string>();
                var valid = InputsSettings.All(x => x.ValidityCheck(inputErrors));
                if (!valid)
                {
                    errors.AddRange(inputErrors);
                    ok = false;
                }
            }

            if (Math.Abs(FrameRate) <= float.Epsilon)
            {
                ok = false;
                errors.Add("Invalid frame rate");
            }
            if (!IsPlatformSupported)
            {
                errors.Add("Current platform is not supported");
                ok = false;
            }

            return ok;
        }


        public virtual bool IsPlatformSupported
        {
            get { return true; }
        }

        public abstract IEnumerable<RecorderInputSettings> InputsSettings { get; }

        internal virtual void SelfAdjustSettings() { }

        public virtual void OnAfterDuplicate()
        {
        }

        protected internal virtual bool HasErrors()
        {
            return false;
        }

        internal virtual bool HasWarnings()
        {
            return !ValidityCheck(new List<string>());
        }

        internal string BuildAbsolutePath()
        {
            CreateDirectory();
            if (string.IsNullOrEmpty(OutputName))
            {
                string date = DateTime.Now.ToString("hh_mm");
                return Path.Combine(OutputDir, "rec_" + date + "." + Extension);
            }
            else
            {
                return Path.Combine(OutputDir, OutputName + "." + Extension);
            }
        }

        internal void CreateDirectory()
        {
            try
            {
                if (string.IsNullOrEmpty(OutputDir))
                {
                    OutputDir = EditorPrefs.GetString("PREF_RECD", Application.dataPath);
                }
                if (!Directory.Exists(OutputDir))
                {
                    Directory.CreateDirectory(OutputDir);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        internal void OnValidate()
        {
            captureEveryNthFrame = Mathf.Max(1, captureEveryNthFrame);
        }
    }
}

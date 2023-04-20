using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.Media;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace UnityEditor.Recorder
{

    [RecorderSettings(typeof(MovieRecorder), "Movie", "movie_16")]
    public class MovieRecorderSettings : RecorderSettings
    {
        internal VideoBitrateMode videoBitRateMode;
        
        internal MediaEncoderRegister encodersRegistered;


        internal MediaEncoderRegister GetCurrentEncoder()
        {
            return encodersRegistered;
        }
        
        internal void DestroyIfExists(MediaEncoderHandle handle)
        {
            if (m_EncoderManager.Exists(handle))
                m_EncoderManager.Destroy(handle);
        }

        internal MediaEncoderManager m_EncoderManager = new MediaEncoderManager();

        [SerializeField, UsedImplicitly] internal int containerFormatSelected = 0;
        [SerializeField, UsedImplicitly] internal int encoderPresetSelected = 0;
        [SerializeField, UsedImplicitly] internal string encoderPresetSelectedName = "";
        [SerializeField, UsedImplicitly] internal int encoderColorDefinitionSelected = 0;
        [SerializeField, UsedImplicitly] internal string encoderCustomOptions = "";

        [SerializeField] ImageInputSelector m_ImageInputSelector = new ImageInputSelector();
        [SerializeField] AudioInputSettings m_AudioInputSettings = new AudioInputSettings();


        internal enum MovieRecorderSettingsAttributes
        {
            CodecFormat,        // for encoders that support multiple formats (e.g. ProRes 4444XQ vs ProRes 422)
            CustomOptions,      // for encoders that can have additional options (e.g. command-line arguments)
            ColorDefinition,    // for encoders that support different color definitions
        }

        internal static readonly Dictionary<MovieRecorderSettingsAttributes, string> AttributeLabels = new Dictionary<MovieRecorderSettingsAttributes, string>()
        {
            { MovieRecorderSettingsAttributes.CodecFormat, "CodecFormat" },
            { MovieRecorderSettingsAttributes.CustomOptions, "CustomOptions" },
            { MovieRecorderSettingsAttributes.ColorDefinition, "ColorDefinition" }
        };

        public MovieRecorderSettings()
        {
            FrameRate = 30;
            videoBitRateMode = TimelineSettings.bitRate;
            var iis = m_ImageInputSelector.Selected as StandardImageInputSettings;
            if (iis != null)
                iis.maxSupportedSize = ImageHeight.x2160p_4K;

            m_ImageInputSelector.ForceEvenResolution(true);
            encodersRegistered = new CoreMediaEncoderRegister();
        }

        public void RefreshSetting()
        {
            TimelineSettings.ApplySetting();
            videoBitRateMode = TimelineSettings.bitRate;
        }


        public override IEnumerable<RecorderInputSettings> InputsSettings
        {
            get
            {
                yield return m_ImageInputSelector.Selected;
                yield return m_AudioInputSettings;
            }
        }

        protected internal override string Extension
        {
            get { return "mp4"; }
        }

        protected internal override bool ValidityCheck(List<string> errors)
        {
            var ok = base.ValidityCheck(errors);

            if (FrameRatePlayback == FrameRatePlayback.Variable)
            {
                errors.Add("Movie recorder does not properly support Variable frame rate playback. Please consider using Constant frame rate instead");
                ok = false;
            }

            var iis = m_ImageInputSelector.Selected as ImageInputSettings;
            if (iis != null)
            {
                string errorMsg;
                if (!encodersRegistered.SupportsResolution(this, iis.OutputWidth, iis.OutputHeight, out errorMsg))
                {
                    errors.Add(errorMsg);
                    ok = false;
                }
            }
            return ok;
        }

        internal override void SelfAdjustSettings()
        {
            var selectedInput = m_ImageInputSelector.Selected;
            if (selectedInput == null) return;

            var iis = selectedInput as StandardImageInputSettings;
            if (iis != null)
            {
                iis.maxSupportedSize = ImageHeight.x2160p_4K;

                if (iis.outputImageHeight != ImageHeight.Window && iis.outputImageHeight != ImageHeight.Custom)
                {
                    if (iis.outputImageHeight > iis.maxSupportedSize)
                        iis.outputImageHeight = iis.maxSupportedSize;
                }
            }

            var cbis = selectedInput as ImageInputSettings;
            if (cbis != null)
            {
                cbis.RecordTransparency = false;
            }
            var gis = selectedInput as GameViewInputSettings;
            if (gis != null)
                gis.FlipFinalOutput = SystemInfo.supportsAsyncGPUReadback;

            m_ImageInputSelector.ForceEvenResolution(true);
        }
    }
}

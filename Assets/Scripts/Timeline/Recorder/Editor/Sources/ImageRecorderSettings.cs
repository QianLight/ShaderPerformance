using System;
using System.Collections.Generic;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace UnityEditor.Recorder
{
    public class ImageRecorderSettings : RecorderSettings
    {

        public enum ImageRecorderOutputFormat
        {
            PNG,
            JPEG,
            EXR
        }
        internal enum ColorSpaceType
        {
            sRGB_sRGB,
            Unclamped_linear_sRGB
        }

        public ImageRecorderOutputFormat OutputFormat
        {
            get { return outputFormat; }
            set { outputFormat = value; }
        }

        [SerializeField] ImageRecorderOutputFormat outputFormat = ImageRecorderOutputFormat.JPEG;


        public bool CaptureAlpha
        {
            get { return captureAlpha; }
            set { captureAlpha = value; }
        }

        [SerializeField] private bool captureAlpha;


        public bool CaptureHDR
        {
            get { return CanCaptureHDRFrames() && m_ColorSpace == ColorSpaceType.Unclamped_linear_sRGB; ; }
        }


        [SerializeField] ImageInputSelector m_ImageInputSelector = new ImageInputSelector();
        [SerializeField] internal ColorSpaceType m_ColorSpace = ColorSpaceType.Unclamped_linear_sRGB;


        protected internal override string Extension
        {
            get
            {
                switch (OutputFormat)
                {
                    case ImageRecorderOutputFormat.PNG:
                        return "png";
                    case ImageRecorderOutputFormat.JPEG:
                        return "jpg";
                    case ImageRecorderOutputFormat.EXR:
                        return "exr";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public ImageInputSettings imageInputSettings
        {
            get { return m_ImageInputSelector.ImageInputSettings; }
            set { m_ImageInputSelector.ImageInputSettings = value; }
        }

        protected internal override bool ValidityCheck(List<string> errors)
        {
            var ok = base.ValidityCheck(errors);
            return ok;
        }

        public override IEnumerable<RecorderInputSettings> InputsSettings
        {
            get { yield return m_ImageInputSelector.Selected; }
        }

        internal bool CanCaptureHDRFrames()
        {
            bool isGameViewInput = imageInputSettings.InputType == typeof(GameViewInput);
            bool isFormatExr = OutputFormat == ImageRecorderOutputFormat.EXR;
            return !isGameViewInput && isFormatExr && CameraInputSettings.UsingHDRP();
        }

        internal bool CanCaptureAlpha()
        {
            bool formatSupportAlpha = OutputFormat == ImageRecorderOutputFormat.PNG ||
                OutputFormat == ImageRecorderOutputFormat.EXR;
            bool inputSupportAlpha = imageInputSettings.SupportsTransparent;
            return (formatSupportAlpha && inputSupportAlpha && !CameraInputSettings.UsingHDRP());
        }

        internal override void SelfAdjustSettings()
        {
            var input = m_ImageInputSelector.Selected;

            if (input == null)  return;

            var cbis = input as CameraInputSettings;
            if (cbis != null)
            {
                cbis.RecordTransparency = CanCaptureAlpha() && CaptureAlpha;
            }

            var gis = input as GameViewInputSettings;
            if (gis != null)
                gis.FlipFinalOutput = SystemInfo.supportsAsyncGPUReadback;
        }
    }
}

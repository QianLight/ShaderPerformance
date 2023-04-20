using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Recorder.Input
{

    public abstract class ImageInputSettings : RecorderInputSettings
    {
        public abstract int OutputWidth { get; set; }

        public abstract int OutputHeight { get; set; }

        public virtual bool SupportsTransparent
        {
            get { return true; }
        }

        public bool RecordTransparency { get; set; }
    }


    [Serializable]
    public abstract class StandardImageInputSettings : ImageInputSettings
    {
        [SerializeField]
        OutputResolution m_OutputResolution = new OutputResolution();

        internal bool forceEvenSize;

        public override int OutputWidth
        {
            get { return ForceEvenIfNecessary(m_OutputResolution.GetWidth()); }
            set { m_OutputResolution.SetWidth(ForceEvenIfNecessary(value)); }
        }

        public override int OutputHeight
        {
            get { return ForceEvenIfNecessary(m_OutputResolution.GetHeight()); }
            set { m_OutputResolution.SetHeight(ForceEvenIfNecessary(value)); }
        }

        internal ImageHeight outputImageHeight
        {
            get { return m_OutputResolution.imageHeight; }
            set { m_OutputResolution.imageHeight = value; }
        }

        internal ImageHeight maxSupportedSize
        {
            get { return m_OutputResolution.maxSupportedHeight; }
            set { m_OutputResolution.maxSupportedHeight = value; }
        }

        int ForceEvenIfNecessary(int v)
        {
            if (forceEvenSize && outputImageHeight != ImageHeight.Custom)
                return (v + 1) & ~1;

            return v;
        }

        protected internal override bool ValidityCheck(List<string> errors)
        {
            var ok = true;

            var h = OutputHeight;

            if (h > (int)maxSupportedSize)
            {
                ok = false;
                errors.Add("Output size exceeds maximum supported height: " + (int)maxSupportedSize + "px");
            }

            var w = OutputWidth;
            if (w <= 0 || h <= 0)
            {
                ok = false;
                errors.Add("Invalid output resolution: " + w + "x" + h);
            }

            return ok;
        }
    }
}

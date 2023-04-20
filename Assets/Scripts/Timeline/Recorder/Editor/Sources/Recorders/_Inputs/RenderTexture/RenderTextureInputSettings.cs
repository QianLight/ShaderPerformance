using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace UnityEditor.Recorder.Input
{
    [DisplayName("Render Texture Asset")]
    [Serializable]
    public class RenderTextureInputSettings : ImageInputSettings
    {
        [SerializeField] internal RenderTexture renderTexture;

        public RenderTexture RenderTexture
        {
            get { return renderTexture; }
            set { renderTexture = value; }
        }

        public bool FlipFinalOutput
        {
            get { return flipFinalOutput; }
            set { flipFinalOutput = value; }
        }

        [SerializeField] private bool flipFinalOutput = false;

        protected internal override Type InputType
        {
            get { return typeof(RenderTextureInput); }
        }

        public override int OutputWidth
        {
            get { return renderTexture == null ? 0 : renderTexture.width; }
            set
            {
                if (renderTexture != null)
                    renderTexture.width = value;
            }
        }

        public override int OutputHeight
        {
            get { return renderTexture == null ? 0 : renderTexture.height; }
            set
            {
                if (renderTexture != null)
                    renderTexture.height = value;
            }
        }

        protected internal override bool ValidityCheck(List<string> errors)
        {
            var ok = true;

            if (renderTexture == null)
            {
                ok = false;
                errors.Add("Missing source render texture object/asset.");
            }
            return ok;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace UnityEditor.Recorder.Input
{
    /// <summary>
    /// Use this class to manage all the information required to record audio from a Scene.
    /// </summary>
    [DisplayName("Audio")]
    [Serializable]
    public class AudioInputSettings : RecorderInputSettings
    {
        /// <summary>
        /// Use this property to record the audio from the Scene (True) or not (False).
        /// </summary>
        public bool PreserveAudio
        {
            get { return false; }
            set
            {
                preserveAudio = value;
            }
        }

        [SerializeField] private bool preserveAudio = false;


        public bool CapturaAudio
        {
            get { return useCapturaAudio; }
            set
            {
                useCapturaAudio = value;
            }
        }


        [SerializeField] private bool useCapturaAudio = false;
        /// <inheritdoc/>
        protected internal override Type InputType
        {
            get { return typeof(AudioInput); }
        }

        /// <inheritdoc/>
        protected internal override bool ValidityCheck(List<string> errors)
        {
            return true;
        }
    }
}

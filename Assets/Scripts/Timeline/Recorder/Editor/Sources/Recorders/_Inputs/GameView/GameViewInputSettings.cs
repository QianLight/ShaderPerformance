using System;
using System.ComponentModel;
using UnityEngine;

namespace UnityEditor.Recorder.Input
{

    [DisplayName("Game View")]
    [Serializable]
    public class GameViewInputSettings : StandardImageInputSettings
    {

        public bool FlipFinalOutput
        {
            get { return flipFinalOutput; }
            set { flipFinalOutput = value; }
        }
        [SerializeField] private bool flipFinalOutput;


        public GameViewInputSettings()
        {
            outputImageHeight = TimelineSettings.solution;
        }
        
        protected internal override Type InputType
        {
            get { return typeof(GameViewInput); }
        }
        
        public override bool SupportsTransparent
        {
            get { return false; }
        }
    }
}

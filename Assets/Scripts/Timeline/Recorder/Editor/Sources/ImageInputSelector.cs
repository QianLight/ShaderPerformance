using System;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace UnityEditor.Recorder
{
   
    [Serializable]
    public class ImageInputSelector : InputSettingsSelector
    {
        [SerializeField] public GameViewInputSettings gameViewInputSettings = new GameViewInputSettings();
        [SerializeField] public CameraInputSettings cameraInputSettings = new CameraInputSettings();
        [SerializeField] public RenderTextureInputSettings renderTextureInputSettings = new RenderTextureInputSettings();

        public ImageInputSettings ImageInputSettings
        {
            get { return (ImageInputSettings)Selected; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (value is CameraInputSettings ||
                    value is GameViewInputSettings ||
                    value is RenderTextureInputSettings)
                {
                    Selected = value;
                }
                else
                {
                    throw new ArgumentException("Video input type not supported: '" + value.GetType() + "'");
                }
            }
        }

        internal void ForceEvenResolution(bool value)
        {
            gameViewInputSettings.forceEvenSize = value;
            cameraInputSettings.forceEvenSize = value;
        }
    }

    [Serializable]
    class UTJImageInputSelector : InputSettingsSelector
    {
        [SerializeField] public CameraInputSettings cameraInputSettings = new CameraInputSettings();
        [SerializeField] public RenderTextureInputSettings renderTextureInputSettings = new RenderTextureInputSettings();

        public ImageInputSettings imageInputSettings
        {
            get { return (ImageInputSettings)Selected; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (value is CameraInputSettings ||
                    value is RenderTextureInputSettings)
                {
                    Selected = value;
                }
                else
                {
                    throw new ArgumentException("Video input type not supported: '" + value.GetType() + "'");
                }
            }
        }
    }
}

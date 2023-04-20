using System.Collections.Generic;
using System.Linq;
using Unity.Media;
using static UnityEditor.Recorder.MovieRecorderSettings;

namespace UnityEditor.Recorder
{
    [CustomEditor(typeof(MovieRecorderSettings))]
    class MovieRecorderEditor : RecorderEditor
    {
        SerializedProperty m_EncoderColorDefinitionSelected;
        MovieRecorderSettings set;

        private MediaEncoderRegister RegisteredEncoders
        {
            get
            {
                if (_mRegisteredEncoders != null)
                    return _mRegisteredEncoders;

                _mRegisteredEncoders = (target as MovieRecorderSettings).encodersRegistered;
                return _mRegisteredEncoders;
            }
        }

        private MediaEncoderRegister _mRegisteredEncoders = null;


        protected override void OnEnable()
        {
            base.OnEnable();

            if (target == null)
                return;
            set = target as MovieRecorderSettings;
            m_EncoderColorDefinitionSelected = serializedObject.FindProperty("encoderColorDefinitionSelected");
        }


        protected override void FileTypeAndFormatGUI()
        {
            var lsEncoderNamesSupportingSelectedFormat = new List<string>();
            lsEncoderNamesSupportingSelectedFormat.Add(RegisteredEncoders.GetName());
            List<IMediaEncoderAttribute> attr = new List<IMediaEncoderAttribute>();
            RegisteredEncoders.GetAttributes(out attr);

            var movieSettings = target as MovieRecorderSettings;
            var anAttr = attr.FirstOrDefault(a => a.GetName() == AttributeLabels[MovieRecorderSettingsAttributes.CodecFormat]);

            anAttr = attr.FirstOrDefault(a => a.GetName() == AttributeLabels[MovieRecorderSettingsAttributes.ColorDefinition]);
            if (anAttr != null)
            {
                MediaPresetAttribute pAttr = (MediaPresetAttribute)anAttr;
                var presetName = new List<string>();
                foreach (var p in pAttr.Value)
                {
                    presetName.Add(p.displayName);
                }

                if (presetName.Count > 0)
                {
                    ++EditorGUI.indentLevel;
                    m_EncoderColorDefinitionSelected.intValue =
                        EditorGUILayout.Popup(pAttr.GetLabel(), m_EncoderColorDefinitionSelected.intValue,
                            presetName.ToArray());
                    --EditorGUI.indentLevel;
                    movieSettings.encoderColorDefinitionSelected = m_EncoderColorDefinitionSelected.intValue;
                }
            }
            
            ++EditorGUI.indentLevel;
            set.videoBitRateMode = (VideoBitrateMode)EditorGUILayout.EnumPopup("Quality", set.videoBitRateMode);
            --EditorGUI.indentLevel;
            

        }
    }
}

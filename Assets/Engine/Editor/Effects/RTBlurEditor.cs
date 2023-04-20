using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    [EnvEditor (typeof (RTBlur))]
    public sealed class RTBlurEditor : EnvEffectEditor<RTBlur>
    {

        public override void OnEnable () { }

        public override void OnInspectorGUI ()
        {
            EditorUtilities.DrawHeaderLabel ("RTBlur");
            var context = EngineContext.instance;
            if (context != null)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button (EditorUtilities.GetContent ("PPBlur"), EditorStyles.miniButton, GUILayout.Width (80)))
                {
                    EnvHelp.RTBlur (context);
                }
                if (GUILayout.Button (EditorUtilities.GetContent ("Cancel"), EditorStyles.miniButton, GUILayout.Width (80)))
                {
                    context.logicflag.SetFlag (EngineContext.Flag_UIBGReady, false);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
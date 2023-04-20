using UnityEditor;
using UnityEngine;

namespace CFEngine
{
    [CustomEditor(typeof(PostProcessResources))]
    public class ShaderResourceEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawSaveScatterButton();
        }

        private void DrawSaveScatterButton()
        {
            GUILayout.Space(10);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(30);
                if (GUILayout.Button("Save Scatter Params"))
                {
                    ScatterEditor.SaveDefaultScatter();
                }
                GUILayout.Space(30);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace CFEngine.Editor
{
    public enum EPrefabType
    {
        None,
        PrefabConfig,
        PrefabLayer,
        UIFix,
        PrefabCameraBlockConfig,
        Num,
    }

    public class PrefabTool : ToolTemplate
    {
        private EPrefabType tool = EPrefabType.None;
        public PrefabTool (EditorWindow editorWindow) : base (editorWindow) { }
        public override void OnEnable ()
        {
            base.OnEnable ();
            if (toolIcons == null)
            {
                toolIcons = new GUIContent[]
                {
                new GUIContent ("PrefabConfig"),
                new GUIContent ("PrefabMisc"),
                new GUIContent ("UIFix"),
                new GUIContent("PrefabCameraBlockConfig")
                };
            }
            tools.Clear ();
            for (EPrefabType i = EPrefabType.None; i < EPrefabType.Num; ++i)
            {
                CommonToolTemplate stt = null;
                switch (i)
                {
                    case EPrefabType.UIFix:
                        stt = ScriptableObject.CreateInstance<PrefabUIFix> ();
                        break;
                    case EPrefabType.PrefabLayer:
                        stt = ScriptableObject.CreateInstance<PrefabMisc> ();
                        break;
                    case EPrefabType.PrefabConfig:
                        stt = ScriptableObject.CreateInstance<PrefabConfigTool> ();
                        break;
                    case EPrefabType.PrefabCameraBlockConfig:
                        stt = ScriptableObject.CreateInstance<PrefabCameraBlockConfigTool>();
                        break;

                }
                tools.Add (stt);
            }

            SetTool ((int) tool);

        }
    }
}
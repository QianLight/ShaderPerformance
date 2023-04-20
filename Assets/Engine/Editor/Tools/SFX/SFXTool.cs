using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace CFEngine.Editor
{
    public enum ESFXType
    {
        None,
        SFXConfig,
        Num,
    }

    public class SFXTool : ToolTemplate
    {
        private ESFXType tool = ESFXType.None;
        public SFXTool (EditorWindow editorWindow) : base (editorWindow) { }
        public override void OnEnable ()
        {
            base.OnEnable ();
            if (toolIcons == null)
            {
                toolIcons = new GUIContent[]
                {
                new GUIContent ("SFXConfig"),
                };
            }
            tools.Clear ();
            for (ESFXType i = ESFXType.None; i < ESFXType.Num; ++i)
            {
                CommonToolTemplate stt = null;
                switch (i)
                {
                    case ESFXType.SFXConfig:
                        stt = ScriptableObject.CreateInstance<SFXConfigTool> ();
                        break;

                }
                tools.Add (stt);
            }

            SetTool ((int) tool);

        }
    }
}
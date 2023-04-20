using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace CFEngine.Editor
{
    public enum ECharacterType
    {
        None,
        AvatarConfig,
        AvatarCompare,
        OutlineConfig,
        SelfShadowConfig,
        MiscConfig,
        EffectConfig,
        PartConfig,
        Num,
    }
    public class CharacterTool : ToolTemplate
    {
        private ECharacterType tool = ECharacterType.None;
        public CharacterTool (EditorWindow editorWindow) : base (editorWindow) { }

        public override void OnEnable ()
        {
            base.OnEnable ();
            if (toolIcons == null)
            {
                toolIcons = new GUIContent[]
                {
                new GUIContent ("AvatarConfig"),
                new GUIContent ("AvatarCompare"),
                new GUIContent ("OutlineConfig"),
                new GUIContent ("SelfShadowConfig"),
                new GUIContent ("MiscConfig"),
                new GUIContent ("EffectConfig"),
                new GUIContent ("PartConfig"),
                };
            }
            tools.Clear ();
            for (ECharacterType i = ECharacterType.None; i < ECharacterType.Num; ++i)
            {
                CommonToolTemplate stt = null;
                switch (i)
                {

                    case ECharacterType.AvatarConfig:
                        stt = ScriptableObject.CreateInstance<AvatarTool> ();
                        break;
                    case ECharacterType.AvatarCompare:
                        stt = ScriptableObject.CreateInstance<AvatarCompareTool> ();
                        break;
                    case ECharacterType.OutlineConfig:
                        stt = ScriptableObject.CreateInstance<OutlineConfigTool> ();
                        break;
                    case ECharacterType.MiscConfig:
                        stt = ScriptableObject.CreateInstance<MiscConfigTool> ();
                        break;
                    case ECharacterType.SelfShadowConfig:
                        stt = ScriptableObject.CreateInstance<SelfShadowConfigTool> ();
                        break;
                    case ECharacterType.EffectConfig:
                        stt = ScriptableObject.CreateInstance<EffectConfigTool> ();
                        break;
                    case ECharacterType.PartConfig:
                        stt = ScriptableObject.CreateInstance<PartConfigTool> ();
                        break;
                }
                tools.Add (stt);
            }

            SetTool ((int) tool);

        }
    }
}
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngineEditor = UnityEditor.Editor;
using System.Text;
using UnityEngine.CFUI;
using UnityObject = UnityEngine.Object;
using System.IO;
namespace CFEngine
{
    public class SFXPrefabAnalyze : SFXAnalyze
    {
        SFXPrefab sp;
        public override byte ComponentType
        {
            get { return SFXComponent.SFX_COMPONENT_PREFAB; }
        }
        public override void Analyze (ref SFXContext context,
               ref SFXTmpContext parentContext, ref SFXTmpContext tmpContext)
        {
            base.Analyze(ref context, ref parentContext, ref tmpContext);
            sp = Prepare<SFXPrefab>(context.trans);
            if (sp != null)
            {
                context.componentType = ComponentType;
            }
        }

        public override void CreateComponent (ref SFXContext context, ref SFXTmpContext tmpContext)
        {
            base.CreateComponent(ref context, ref tmpContext);
            var goComp = context.comp as SFXGameObject;
            goComp.prefabName = context.trans.name;
            goComp.animPath = sp.animPath;
            goComp.flag.SetFlag(SFXComp.Flag_Hide, sp.startHide);
            for (int i = context.trans.childCount - 1; i >= 0; --i)
            {
                UnityObject.DestroyImmediate(context.trans.GetChild(i).gameObject);
            }
        }

        public override void Reset(ref SFXContext context, bool save)
        {
            Remove(ref sp, save);
        }
    }

}
#endif
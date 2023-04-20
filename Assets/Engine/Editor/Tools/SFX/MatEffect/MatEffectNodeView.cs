using System;
using System.Collections.Generic;
using CFEngine.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CFEngine.Editor
{
    [CustomNodeView (typeof (EffectTemplate))]
    public class EffectTemplateView : NodeView, ICanDirty
    {
        protected override void OnInitialize ()
        {
            base.OnInitialize ();
            var et = target as EffectTemplate;
            this.title = string.IsNullOrEmpty (et.effectName) ? "EffectTemplate" : et.effectName;

        }

        public override void OnUpdate ()
        {
            base.OnUpdate ();
            var et = target as EffectTemplate;
            this.title = string.IsNullOrEmpty (et.effectName) ? "EffectTemplate" : et.effectName;
        }
    }

    [CustomNodeView (typeof (ShaderParam))]
    public class ShaderParamView : NodeView, ICanDirty { }

    [CustomNodeView (typeof (ShaderColor))]
    public class ShaderColorView : NodeView, ICanDirty { }

    [CustomNodeView (typeof (ShaderTexture))]
    public class ShaderTextureView : NodeView, ICanDirty { }

    [CustomNodeView (typeof (RenderActive))]
    public class RenderActiveView : NodeView, ICanDirty { }

    [CustomNodeView (typeof (MatSwitch))]
    public class MatSwitchView : NodeView, ICanDirty { }

    [CustomNodeView (typeof (MatLoad))]
    public class MatLoadView : NodeView, ICanDirty { }

    [CustomNodeView(typeof(MatAdd))]
    public class MatAddView : NodeView, ICanDirty { }

    [CustomNodeView (typeof (ScaleParam))]
    public class ScaleParamView : NodeView, ICanDirty { }

    [CustomNodeView(typeof(ShaderRT))]
    public class ShaderRTView : NodeView, ICanDirty { }

    [CustomNodeView(typeof(GetPos))]
    public class GetPosView : NodeView, ICanDirty { }

    [CustomNodeView(typeof(ShaderKeyWord))]
    public class ShaderKeyWordView : NodeView, ICanDirty { }

    
}
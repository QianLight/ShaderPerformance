#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CFEngine
{
    public abstract class AttributeDecorator
    {
        static Dictionary<Type, AttributeDecorator> s_AttributeDecorators;
        private static EnvParam dummyEP = new EnvParam ();
        public static uint Flag_EditSrcObj = 0x00000001;

        public static bool DebugCurrentParamValue = false;

        private static GUIStyle redText = null;
        public GUIStyle GetRedTextSyle ()
        {
            if (redText == null)
            {
                redText = new GUIStyle ("Label");
                redText.normal.textColor = Color.red;
            }
            return redText;
        }

        public virtual void ResetValue (SerializedParameter spo, Attribute attribute) 
        { 
            
        }

        public bool OnGUI (SerializedParameter profileSP,
            GUIContent title, Attribute attribute, RuntimeParamOverride runtimeParam)
        {
            dummyEP.runtimeParam = runtimeParam;
            dummyEP.param = null;
            dummyEP.valueMask = 0;
            return OnGUI (profileSP, title, attribute, dummyEP, Flag_EditSrcObj,out var overrideChange);
        }
        public virtual SerializedPropertyType GetSType ()
        {
            return SerializedPropertyType.Generic;
        }
        public virtual bool OnGUI (
            SerializedParameter spo,
            GUIContent title,
            Attribute attribute,
            EnvParam envParam,
            uint flag,
            out bool overrideChange)
        {
            overrideChange = false;
            return false;
        }
        public virtual void SetInfo (string settingname, string name, Attribute attribute,
            EnvParam envParam)
        {

        }

        public static void DebugValue<T, TT> (RuntimeParamOverride runtimeParam, ref T srcValue) where TT : ParamOverride, new()
        {
            var runtime = runtimeParam != null ? runtimeParam.runtime : null;
            if (DebugCurrentParamValue && runtime != null)
            {
                srcValue = (runtime as ParamOverride<T, TT>).value;
            }
        }

        public static RuntimeParamOverride DebugRuntimeParam (SerializedParameterOverride spo)
        {
            EnvParam envParam;
            var param = RuntimeUtilities.GetRuntimeParam (spo.baseProperty, out envParam);
            if (param != null)
            {
                if (envParam != null && envParam.valueMask != 0)
                {
                    GUI.color = Color.red;
                }
                else
                {
                    GUI.color = Color.white;
                }
            }
            return param;
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnEditorReload ()
        {

            if (s_AttributeDecorators == null)
            {
                s_AttributeDecorators = new Dictionary<Type, AttributeDecorator> ();
            }
            ReloadDecoratorTypes ();
        }

        static void ReloadDecoratorTypes ()
        {
            s_AttributeDecorators.Clear ();

            // Look for all the valid attribute decorators
            var types = EngineUtility.GetAllAssemblyTypes ()
                .Where (
                    t => t.IsSubclassOf (typeof (AttributeDecorator)) &&
                    t.IsDefined (typeof (CFDecoratorAttribute), false) &&
                    !t.IsAbstract
                );

            // Store them
            foreach (var type in types)
            {
                var attr = type.GetAttribute<CFDecoratorAttribute> ();
                var decorator = (AttributeDecorator) Activator.CreateInstance (type);
                s_AttributeDecorators.Add (attr.attributeType, decorator);
            }
        }
        public static AttributeDecorator GetDecorator (Type attributeType)
        {
            if (s_AttributeDecorators == null)
            {
                s_AttributeDecorators = new Dictionary<Type, AttributeDecorator> ();
                ReloadDecoratorTypes ();
            }

            AttributeDecorator decorator;
            return !s_AttributeDecorators.TryGetValue (attributeType, out decorator) ?
                null :
                decorator;
        }
    }
}
#endif
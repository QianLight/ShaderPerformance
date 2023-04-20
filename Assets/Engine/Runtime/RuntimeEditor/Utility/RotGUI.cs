#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CFEngine
{
    public class RotGUI
    {
        public RotEditor editor;
        public SerializedProperty RotationSp;
        public Transform trans;
        public RotContext context = new RotContext();

        public Transform tran
        {
            get
            {
                return trans;
            }
        }

        public void OnInit (Transform t)
        {
            editor = new RotEditor ();
            SerializedObject so = new SerializedObject (t);
            this.RotationSp = so.FindProperty ("m_LocalRotation");
            trans = t;
            editor.OnInit (t);

        }

        public bool BeginGUI ()
        {
            if (editor != null)
            {
                editor.BeginRotationField (context);
                return true;
            }
            return false;
        }
        
        public void EndGUI ()
        {
            if (editor != null)
            {
                editor.EndRotationField (context);                     
                RotationSp.serializedObject.SetIsDifferentCacheDirty ();
            }
        }
    }
}
#endif
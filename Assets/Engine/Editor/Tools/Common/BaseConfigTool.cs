using System;
using System.Collections.Generic;
using CFEngine;
using UnityEditor;
using UnityEngine;
using UnityEngine.CFUI;
using UnityEngine.SceneManagement;

namespace CFEngine.Editor
{
    public partial class BaseConfigTool<T> : CommonToolTemplate where T : AssetBaseConifg<T>
    {
        public enum OpType
        {
            None,
            OpSaveAll,
            OpSaveConfig
        }

        protected T config;
        private OpType opType = OpType.None;
        private Vector2 scroll;
        public override void OnInit ()
        {
            base.OnInit ();
        }

        public override void OnUninit ()
        {
            base.OnUninit ();
        }

        protected virtual void OnConfigGui (ref Rect rect)
        {

        }

        public override void DrawGUI (ref Rect rect)
        {
            if (config != null)
            {
                EditorGUILayout.BeginHorizontal ();
                if (GUILayout.Button ("Save", GUILayout.MaxWidth (100)))
                {
                    opType = OpType.OpSaveAll;
                }
                if (GUILayout.Button ("SaveConfig", GUILayout.MaxWidth (100)))
                {
                    opType = OpType.OpSaveConfig;
                }
                EditorGUILayout.ObjectField (config, typeof (ScriptableObject), false, GUILayout.MaxWidth (400));
                EditorGUILayout.EndHorizontal ();
                //EditorCommon.BeginScroll (ref scroll, 50, 50);
                OnConfigGui (ref rect);
                //EditorCommon.EndScroll ();
            }
        }
        protected virtual void OnConfigUpdate ()
        {

        }
        public override void Update ()
        {
            if (config != null)
            {
                OnConfigUpdate ();
                switch (opType)
                {
                    case OpType.OpSaveAll:
                        SaveAll ();
                        break;
                    case OpType.OpSaveConfig:
                        config.Save ();
                        break;
                }
                opType = OpType.None;
            }
        }
        protected virtual void OnSave ()
        {

        }
        private void SaveAll ()
        {
            if (config != null)
            {
                OnSave ();
                config.Save ();
            }
        }
    }
}
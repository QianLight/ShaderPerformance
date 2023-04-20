using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UsingTheirs.ShaderHotSwap
{

    public class EditorCoroutine
    {
        public static EditorCoroutine Start(IEnumerator coroutine)
        {
            var editorCoroutine = new EditorCoroutine(coroutine);
            editorCoroutine.EditorStart();
            return editorCoroutine;
        }

        IEnumerator coroutine;
        CustomYieldInstruction current;

        EditorCoroutine(IEnumerator coroutine)
        {
            this.coroutine = coroutine;
        }

        void EditorStart()
        {
            current = null;
            EditorApplication.update += EditorUpdate;
        }
        public void EditorStop()
        {
            EditorApplication.update -= EditorUpdate;
        }

        void EditorUpdate()
        {
            if (current != null && !current.isDone)
                return;

            if (!coroutine.MoveNext())
                EditorStop();

            current = coroutine.Current as CustomYieldInstruction;
        }

        public class CustomYieldInstruction
        {
            public virtual bool isDone { get { return _isDone(); } }
            System.Func<bool> _isDone;
            public CustomYieldInstruction(System.Func<bool> isDone = null)
            {
                this._isDone = isDone;
            }
        }

        public class CustomWaitForSeconds : CustomYieldInstruction
        {
            double begin;
            float seconds;
            public CustomWaitForSeconds(float seconds)
            {
                this.seconds = seconds;
                this.begin = EditorApplication.timeSinceStartup;
            }
            public override bool isDone { get { return EditorApplication.timeSinceStartup - begin >= seconds; } }
        }
    }

}

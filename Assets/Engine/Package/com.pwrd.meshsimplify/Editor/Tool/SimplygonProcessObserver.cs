using System.Collections;
using System.Collections.Generic;
using Simplygon;
using Simplygon.Unity.EditorPlugin;
using UnityEditor;
using UnityEngine;

namespace Athena.MeshSimplify
{
    public class SimplygonProcessObserver : Observer
    {
        public override bool OnProgress(spObject subject, float progressPercent)
        {
            ShowProcessBar(subject.GetName(), progressPercent);
            return true;
        }

        static void ShowProcessBar(string info, float progressPercent)
        {
            EditorUtility.DisplayProgressBar("Simplygon Reduction", "Reducte:" + info, progressPercent);
            if (Mathf.Approximately(progressPercent, 1))
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
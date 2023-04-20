using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LightProbeHelper
{
    [MenuItem("Assets/场景/LightProbe/Change")]
    static void LightProbeChange()
    {

        UnityEngine.Object[] allObjs = Selection.objects;

        for (int i = 0; i < allObjs.Length; i++)
        {
            EditorUtility.DisplayCancelableProgressBar("LightProbeChange", i.ToString(), i * 1.0f / allObjs.Length);

            UnityEngine.Object obj = allObjs[i];
            string objPath = AssetDatabase.GetAssetPath(obj);
            GameObject targetGO = AssetDatabase.LoadAssetAtPath(objPath, typeof(GameObject)) as GameObject;
            UnityEngine.GameObject inst = PrefabUtility.InstantiatePrefab(targetGO) as GameObject;
            if (inst == null) continue;

            Renderer[] renders = inst.GetComponentsInChildren<Renderer>();
            foreach (Renderer rd in renders)
            {
                rd.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                rd.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
                rd.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
            }
            PrefabUtility.SaveAsPrefabAssetAndConnect(inst, objPath, InteractionMode.AutomatedAction);
            GameObject.DestroyImmediate(inst);

        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

    }

    [MenuItem("Assets/场景/LightProbe/Off")]
    static void LightProbeOff()
    {
        Renderer[] renders = GameObject.FindObjectsOfType<Renderer>();

        for (int i = 0; i < renders.Length; i++)
        {
            renders[i].lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        }
    }
}

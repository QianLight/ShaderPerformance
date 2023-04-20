using FMODUnity;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LipSync
{
    public class GenerateLipEnv
    {

        [MenuItem("GameObject/Create Lip Sync", priority = 39)]
        static void CreateLipEnv()
        {
            if (Selection.activeObject != null && Selection.activeObject is GameObject)
            {
                GraphicsSettings.renderPipelineAsset = null;

                GameObject GO = Selection.activeObject as GameObject;
                GO.transform.localPosition = new Vector3(0, -0.6f, -9.1f);
                GO.transform.localEulerAngles = new Vector3(0, 180, 0);

                var emitter = GO.AddComponent<StudioEventEmitter>();
                var sync = GO.AddComponent<FmodLipSync>();
                sync.emiter = emitter;
                sync.save = true;

                if (Camera.main != null)
                {
                    int ui = LayerMask.NameToLayer("UI");
                    int mask = Camera.main.cullingMask;
                    Camera.main.cullingMask = ~(1 << ui) & mask;
                }
                CreateVisual();
            }
            else
            {
                EditorUtility.DisplayDialog("tip", "Select a Avatar at first", "ok");
            }
        }


        static void CreateVisual()
        {
            GameObject go = GameObject.Find("Camara");
            if (go == null)
            {
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Editor/Timeline/res/lip/Camera.prefab");
                go = GameObject.Instantiate<GameObject>(obj);
                go.name = "Camera";
            }
        }

    }
}
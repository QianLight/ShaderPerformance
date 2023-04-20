using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CFEngine.Editor
{
    [Serializable]
    public class SceneBatchInstancing : SceneBatchProcessor
    {
        public override string Name => "重置Instancing";

        public override void Process(SceneBatchContext context)
        {
            context.processRawScene.Add(ProcessRawScene);
        }

        public static void ProcessCurrentScene()
        {
            ProcessCurrentScene(out bool _);
        }
        
        private static void ProcessCurrentScene(out bool dirty)
        {
            GameObject rootGo = GameObject.Find("EditorScene/Instance");
            dirty = rootGo;
            if (!rootGo)
                return;

            Process(rootGo);
        }

        private static void Process(GameObject go)
        {
            GPUInstancingRoot root = go.GetComponent<GPUInstancingRoot>();
            if (!root)
                root = go.AddComponent<GPUInstancingRoot>();

            GPUInstancingRootEditor.CollectInstancingData(root);
            for (int i = 0; i < root.groups.Count; i++)
            {
                GPUInstancingGroup group = root.groups[i];
                if (@group.lods.Count < 2)
                {
                    GPUInstancingLod lod = new GPUInstancingLod();
                    lod.mesh = @group.lods[0].mesh;
                    lod.distance = root.defaultDistance[1];
                    @group.lods.Add(lod);
                }
            }

            EditorUtility.SetDirty(root);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static void ProcessRawScene(out bool dirty, out bool error)
        {
            error = false;
            ProcessCurrentScene(out dirty);
        }
    }
}
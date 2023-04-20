using System;
using UnityEngine;

namespace CFEngine.Editor
{
    [Serializable]
    public class SceneBatchBakeSmartShadow : SceneBatchProcessor
    {
        public override string Name => "重新保存SmartShadow";

        public override void Process(SceneBatchContext context)
        {
            context.processRawScene.Add(ProcessRawScene);
        }

        private static void ProcessRawScene(out bool dirty, out bool error)
        {
            GameObject rootGo = GameObject.Find("EditorScene");

            if (!rootGo)
                dirty = false;
            
            SmartShadow[] ssArray = rootGo.GetComponentsInChildren<SmartShadow>();
            if (ssArray.Length == 0)
                dirty = false;

            foreach (SmartShadow ss in ssArray)
                ss.Bake();

            dirty = true;
            error = false;
        }
    }
}
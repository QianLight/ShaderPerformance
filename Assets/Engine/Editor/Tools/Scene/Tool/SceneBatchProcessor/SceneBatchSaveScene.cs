using System;

namespace CFEngine.Editor
{
    [Serializable]
    public class SceneBatchSaveScene : SceneBatchProcessor
    {
        public override string Name => "重新保存场景";

        public override void Process(SceneBatchContext context)
        {
            context.processRawScene.Add(ProcessRawScene);
        }

        private static void ProcessRawScene(out bool dirty, out bool error)
        {
            dirty = true;
            error = false;
        }
    }
}
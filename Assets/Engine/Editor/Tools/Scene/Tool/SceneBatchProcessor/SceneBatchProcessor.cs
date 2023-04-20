using System;

namespace CFEngine.Editor
{
    [Serializable]
    public class SceneBatchProcessor : BatchToolProcessor<SceneBatchContext>
    {
        public override string Name => GetType().ToString();
    }
}
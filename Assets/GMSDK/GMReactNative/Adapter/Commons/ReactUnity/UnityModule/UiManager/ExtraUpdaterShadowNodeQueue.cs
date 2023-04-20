
using System.Collections.Generic;

namespace GSDK.RNU {
    public class ExtraUpdaterShadowNodeQueue {
        private static HashSet<ReactSimpleShadowNode> queue = new HashSet<ReactSimpleShadowNode>();

        public static void Add(ReactSimpleShadowNode shadowNodeToUpdate) {
            queue.Add(shadowNodeToUpdate);
        }

        public static void execute(UIViewOperationQueue operationQueue) {
            try
            {
                foreach (ReactSimpleShadowNode shadowNodeToUpdate in queue)
                {
                    shadowNodeToUpdate.DoExtraUpdate(operationQueue);
                }
            }
            finally
            {
                queue.Clear();
            }
        }

        public static void ClearAll()
        { 
            queue.Clear();
        }
    }
}
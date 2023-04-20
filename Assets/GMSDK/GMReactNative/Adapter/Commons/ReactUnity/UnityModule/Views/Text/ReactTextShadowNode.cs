using System.Collections.Generic;

namespace GSDK.RNU
{
    public class ReactTextShadowNode : ReactBaseTextShadowNode
    {
        public static string sExtraUpdateFlag = "__text__";

        public ReactTextShadowNode(int tag) : base(tag)
        {
        }

        protected override string GetChildText()
        {
            string text = "";
            for (int i = 0; i < children.Count; i++)
            {
                // for now we only support  <Text>aa</Text> . not support <Text>aa <Text>bb</Text></Text>
                ReactRawTextShadowNode child = (ReactRawTextShadowNode)children[i];
                text += child.text;
            }
            return text;
        }
        public override void Insert(int index, ReactSimpleShadowNode child)
        {
            children.Insert(index, child);
            child.parent = this;

            ExtraUpdaterShadowNodeQueue.Add(this);
        }
        
        public override void DoExtraUpdate(UIViewOperationQueue operationQueue)
        {
            string text = GetChildText();
            operationQueue.HandleUpdateView(new UpdateViewOperation(
                this.tag,
                ReactTextManager.ReactTextName,
                new Dictionary<string, object>
                {
                    {sExtraUpdateFlag, text}
                }
            ));

            this.yogaNode.MarkDirty();
        }
    }

}
